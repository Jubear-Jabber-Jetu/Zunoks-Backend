using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public class ScreeningAdminService : IScreeningAdminService
{
    private readonly ZunoksDbContext _context;
    private readonly ILogger<ScreeningAdminService> _logger;

    public ScreeningAdminService(ZunoksDbContext context, ILogger<ScreeningAdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ScreeningDashboardStatsDto> GetDashboardStatsAsync(int? surveyId = null)
    {
        var query = _context.ScreeningSubmissions
            .Where(s => s.IsCompleted)
            .AsQueryable();

        if (surveyId.HasValue)
            query = query.Where(s => s.SurveyId == surveyId.Value);

        var submissions = await query
            .Include(s => s.SectionScores)
                .ThenInclude(ss => ss.Section)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        var ranked = BuildRankedList(submissions);

        return new ScreeningDashboardStatsDto
        {
            TotalCandidates = submissions.Count,
            AverageScore = submissions.Count > 0
                ? Math.Round(submissions.Average(s => s.Percentage), 2)
                : 0,
            HighestScore = submissions.Count > 0 ? submissions.Max(s => s.TotalScore) : 0,
            LowestScore = submissions.Count > 0 ? submissions.Min(s => s.TotalScore) : 0,
            TopCandidates = ranked.OrderByDescending(r => r.TotalScore).Take(10).ToList(),
            RecentSubmissions = ranked.Take(10).ToList(),
        };
    }

    public async Task<PaginatedScreeningSubmissionsDto> GetCandidatesPagedAsync(
        int page,
        int pageSize,
        int? surveyId = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        decimal? minScore = null,
        decimal? maxScore = null,
        string? experience = null,
        string? expectedSalary = null,
        string? sortBy = null,
        string? sortDir = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var query = _context.ScreeningSubmissions
            .Where(s => s.IsCompleted)
            .Include(s => s.SectionScores)
                .ThenInclude(ss => ss.Section)
            .AsQueryable();

        if (surveyId.HasValue)
            query = query.Where(s => s.SurveyId == surveyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.FullName.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term) ||
                s.Phone.ToLower().Contains(term));
        }

        if (dateFrom.HasValue)
            query = query.Where(s => s.SubmittedAt >= dateFrom.Value.Date);

        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.Date.AddDays(1);
            query = query.Where(s => s.SubmittedAt < toExclusive);
        }

        if (minScore.HasValue)
            query = query.Where(s => s.Percentage >= minScore.Value);

        if (maxScore.HasValue)
            query = query.Where(s => s.Percentage <= maxScore.Value);

        if (!string.IsNullOrWhiteSpace(experience))
        {
            var exp = experience.Trim().ToLower();
            query = query.Where(s => s.YearsOfExperience != null && s.YearsOfExperience.ToLower().Contains(exp));
        }

        if (!string.IsNullOrWhiteSpace(expectedSalary))
        {
            var salary = expectedSalary.Trim().ToLower();
            query = query.Where(s => s.ExpectedSalary != null && s.ExpectedSalary.ToLower().Contains(salary));
        }

        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLower()) switch
        {
            "name" => descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
            "email" => descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
            "score" or "totalscore" => descending ? query.OrderByDescending(s => s.TotalScore) : query.OrderBy(s => s.TotalScore),
            "percentage" => descending ? query.OrderByDescending(s => s.Percentage) : query.OrderBy(s => s.Percentage),
            "experience" => descending
                ? query.OrderByDescending(s => s.YearsOfExperience)
                : query.OrderBy(s => s.YearsOfExperience),
            "salary" or "expectedsalary" => descending
                ? query.OrderByDescending(s => s.ExpectedSalary)
                : query.OrderBy(s => s.ExpectedSalary),
            "id" => descending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id),
            _ => descending ? query.OrderByDescending(s => s.SubmittedAt) : query.OrderBy(s => s.SubmittedAt),
        };

        var totalCount = await query.CountAsync();
        var pageItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var allForRank = await _context.ScreeningSubmissions
            .Where(s => s.IsCompleted && (!surveyId.HasValue || s.SurveyId == surveyId.Value))
            .OrderByDescending(s => s.TotalScore)
            .ThenBy(s => s.SubmittedAt)
            .Select(s => s.Id)
            .ToListAsync();

        var rankMap = allForRank
            .Select((id, index) => new { id, rank = index + 1 })
            .ToDictionary(x => x.id, x => x.rank);

        var items = pageItems.Select(s => MapListItem(s, rankMap.GetValueOrDefault(s.Id, 0))).ToList();

        return new PaginatedScreeningSubmissionsDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<ScreeningSubmissionDetailDto?> GetCandidateDetailsAsync(int id)
    {
        var submission = await _context.ScreeningSubmissions
            .Include(s => s.Survey)
            .Include(s => s.SectionScores)
                .ThenInclude(ss => ss.Section)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.Section)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Option)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsCompleted);

        if (submission == null) return null;

        var rank = await GetRankAsync(submission.Id, submission.SurveyId, submission.TotalScore);

        return new ScreeningSubmissionDetailDto
        {
            Id = submission.Id,
            SurveyId = submission.SurveyId,
            SurveyTitle = submission.Survey.Title,
            FullName = submission.FullName,
            Email = submission.Email,
            Phone = submission.Phone,
            YearsOfExperience = submission.YearsOfExperience,
            NoticePeriod = submission.NoticePeriod,
            PortfolioLinks = submission.PortfolioLinks,
            ExpectedSalary = submission.ExpectedSalary,
            SubmittedAt = submission.SubmittedAt,
            TotalScore = submission.TotalScore,
            MaxScore = submission.MaxScore,
            Percentage = submission.Percentage,
            Rank = rank,
            SectionScores = submission.SectionScores
                .OrderBy(ss => ss.Section.SortOrder)
                .Select(ss => new ScreeningSectionScoreDto
                {
                    SectionId = ss.SectionId,
                    SectionTitle = ss.Section.Title,
                    Category = ss.Section.Category,
                    Score = ss.Score,
                    MaxScore = ss.MaxScore,
                })
                .ToList(),
            Answers = submission.Answers
                .OrderBy(a => a.Question.Section.SortOrder)
                .ThenBy(a => a.Question.SortOrder)
                .Select(a => new ScreeningAnswerDetailDto
                {
                    QuestionId = a.QuestionId,
                    QuestionText = a.Question.Text,
                    SectionTitle = a.Question.Section.Title,
                    SelectedOptionLabel = a.Option?.Label,
                    TextAnswer = a.TextAnswer,
                    ScoreEarned = a.ScoreEarned,
                })
                .ToList(),
        };
    }

    public async Task<ScreeningRankingDto> GetRankingsAsync(string rankBy, int? surveyId = null, int limit = 50)
    {
        limit = Math.Clamp(limit, 1, 200);

        var query = _context.ScreeningSubmissions
            .Where(s => s.IsCompleted)
            .Include(s => s.SectionScores)
                .ThenInclude(ss => ss.Section)
            .AsQueryable();

        if (surveyId.HasValue)
            query = query.Where(s => s.SurveyId == surveyId.Value);

        var submissions = await query.ToListAsync();
        var ranked = BuildRankedList(submissions);

        var sorted = (rankBy?.ToLower()) switch
        {
            "technical" => ranked.OrderByDescending(r => r.TechnicalScore).ThenByDescending(r => r.TotalScore),
            "leadership" => ranked.OrderByDescending(r => r.LeadershipScore).ThenByDescending(r => r.TotalScore),
            "tool" or "tools" => ranked.OrderByDescending(r => r.ToolScore).ThenByDescending(r => r.TotalScore),
            _ => ranked.OrderByDescending(r => r.TotalScore).ThenBy(r => r.SubmittedAt),
        };

        return new ScreeningRankingDto
        {
            RankedBy = rankBy ?? "total",
            Items = sorted.Take(limit).Select((item, index) =>
            {
                item.Rank = index + 1;
                return item;
            }).ToList(),
        };
    }

    public async Task<List<ScreeningSubmission>> GetSubmissionsForExportAsync(int? surveyId = null, IEnumerable<int>? ids = null)
    {
        var query = _context.ScreeningSubmissions
            .Where(s => s.IsCompleted)
            .Include(s => s.Survey)
            .Include(s => s.SectionScores)
                .ThenInclude(ss => ss.Section)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.Section)
            .Include(s => s.Answers)
                .ThenInclude(a => a.Option)
            .AsQueryable();

        if (surveyId.HasValue)
            query = query.Where(s => s.SurveyId == surveyId.Value);

        if (ids != null)
        {
            var idList = ids.ToList();
            if (idList.Count > 0)
                query = query.Where(s => idList.Contains(s.Id));
        }

        return await query
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    private static List<ScreeningSubmissionListItemDto> BuildRankedList(List<ScreeningSubmission> submissions)
    {
        var ordered = submissions
            .OrderByDescending(s => s.TotalScore)
            .ThenBy(s => s.SubmittedAt)
            .ToList();

        var rankMap = ordered
            .Select((s, index) => new { s.Id, Rank = index + 1 })
            .ToDictionary(x => x.Id, x => x.Rank);

        return ordered.Select(s => MapListItem(s, rankMap[s.Id])).ToList();
    }

    private static ScreeningSubmissionListItemDto MapListItem(ScreeningSubmission s, int rank)
    {
        return new ScreeningSubmissionListItemDto
        {
            Id = s.Id,
            FullName = s.FullName,
            Email = s.Email,
            Phone = s.Phone,
            YearsOfExperience = s.YearsOfExperience,
            ExpectedSalary = s.ExpectedSalary,
            SubmittedAt = s.SubmittedAt,
            TotalScore = s.TotalScore,
            MaxScore = s.MaxScore,
            Percentage = s.Percentage,
            Rank = rank,
            TechnicalScore = SumCategoryScore(s, "Technical"),
            LeadershipScore = SumCategoryScore(s, "Leadership"),
            ToolScore = SumCategoryScore(s, "Tool"),
        };
    }

    private static int SumCategoryScore(ScreeningSubmission s, string category)
    {
        return s.SectionScores
            .Where(ss => string.Equals(ss.Section.Category, category, StringComparison.OrdinalIgnoreCase))
            .Sum(ss => ss.Score);
    }

    private async Task<int> GetRankAsync(int submissionId, int surveyId, int totalScore)
    {
        var higherCount = await _context.ScreeningSubmissions
            .CountAsync(s => s.IsCompleted
                && s.SurveyId == surveyId
                && (s.TotalScore > totalScore
                    || (s.TotalScore == totalScore && s.Id < submissionId)));

        return higherCount + 1;
    }
}
