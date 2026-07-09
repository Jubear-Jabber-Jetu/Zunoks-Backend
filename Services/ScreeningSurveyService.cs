using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public class ScreeningSurveyService : IScreeningSurveyService
{
    private readonly ZunoksDbContext _context;

    public ScreeningSurveyService(ZunoksDbContext context)
    {
        _context = context;
    }

    public async Task<ScreeningSurveyDto?> GetActiveSurveyAsync()
    {
        var survey = await _context.ScreeningSurveys
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive);

        return survey == null ? null : MapSurveyToDto(survey.Id, await LoadSurveyGraphAsync(survey.Id));
    }

    public async Task<ScreeningSurveyDto?> GetSurveyQuestionsAsync(int surveyId)
    {
        var survey = await _context.ScreeningSurveys
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        if (survey == null) return null;

        return MapSurveyToDto(survey.Id, await LoadSurveyGraphAsync(surveyId));
    }

    public async Task<ScreeningSubmission> SubmitSurveyAsync(ScreeningSubmitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new ArgumentException("Full name is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new ArgumentException("Phone is required");
        if (dto.Answers == null || dto.Answers.Count == 0)
            throw new ArgumentException("At least one answer is required");

        var email = dto.Email.Trim().ToLowerInvariant();

        var survey = await _context.ScreeningSurveys
            .Include(s => s.Sections)
                .ThenInclude(sec => sec.Questions)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == dto.SurveyId && s.IsActive);

        if (survey == null)
            throw new ArgumentException("Survey not found or is not active");

        var duplicate = await _context.ScreeningSubmissions
            .AnyAsync(s => s.SurveyId == dto.SurveyId
                && s.Email.ToLower() == email
                && s.IsCompleted);

        if (duplicate)
            throw new ArgumentException("A submission with this email already exists for this survey");

        var questions = survey.Sections
            .SelectMany(s => s.Questions)
            .ToDictionary(q => q.Id);

        foreach (var question in questions.Values.Where(q => q.IsRequired))
        {
            var answer = dto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (answer == null)
                throw new ArgumentException($"Answer required for: {question.Text}");

            if (question.QuestionType == "Radio" && !answer.OptionId.HasValue)
                throw new ArgumentException($"Option required for: {question.Text}");

            if (question.QuestionType == "Text" && string.IsNullOrWhiteSpace(answer.TextAnswer))
                throw new ArgumentException($"Text answer required for: {question.Text}");
        }

        var submission = new ScreeningSubmission
        {
            SurveyId = dto.SurveyId,
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = dto.Phone.Trim(),
            YearsOfExperience = string.IsNullOrWhiteSpace(dto.YearsOfExperience) ? null : dto.YearsOfExperience.Trim(),
            NoticePeriod = string.IsNullOrWhiteSpace(dto.NoticePeriod) ? null : dto.NoticePeriod.Trim(),
            PortfolioLinks = string.IsNullOrWhiteSpace(dto.PortfolioLinks) ? null : dto.PortfolioLinks.Trim(),
            ExpectedSalary = string.IsNullOrWhiteSpace(dto.ExpectedSalary) ? null : dto.ExpectedSalary.Trim(),
            SubmittedAt = BangladeshTime.Now(),
            IsCompleted = true,
        };

        foreach (var answerDto in dto.Answers)
        {
            if (!questions.TryGetValue(answerDto.QuestionId, out var question))
                throw new ArgumentException($"Invalid question id: {answerDto.QuestionId}");

            var answer = new ScreeningAnswer
            {
                QuestionId = question.Id,
            };

            if (question.QuestionType == "Radio")
            {
                if (!answerDto.OptionId.HasValue)
                    throw new ArgumentException($"Option required for: {question.Text}");

                var option = question.Options.FirstOrDefault(o => o.Id == answerDto.OptionId.Value);
                if (option == null)
                    throw new ArgumentException($"Invalid option for question: {question.Text}");

                answer.OptionId = option.Id;
                answer.ScoreEarned = question.IsScored ? option.Score : 0;
            }
            else
            {
                answer.TextAnswer = answerDto.TextAnswer?.Trim();
                answer.ScoreEarned = 0;
            }

            submission.Answers.Add(answer);
        }

        foreach (var section in survey.Sections.Where(s => s.IsScored))
        {
            var scoredQuestions = section.Questions.Where(q => q.IsScored).ToList();
            var sectionScore = scoredQuestions.Sum(q =>
                submission.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.ScoreEarned ?? 0);
            var sectionMax = scoredQuestions.Sum(q => q.Options.Max(o => o.Score));

            submission.SectionScores.Add(new ScreeningSectionScore
            {
                SectionId = section.Id,
                Score = sectionScore,
                MaxScore = sectionMax,
            });
        }

        submission.TotalScore = submission.SectionScores.Sum(s => s.Score);
        submission.MaxScore = submission.SectionScores.Sum(s => s.MaxScore);
        submission.Percentage = submission.MaxScore > 0
            ? Math.Round((decimal)submission.TotalScore / submission.MaxScore * 100, 2)
            : 0;

        _context.ScreeningSubmissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    private async Task<ScreeningSurvey> LoadSurveyGraphAsync(int surveyId)
    {
        return await _context.ScreeningSurveys
            .AsNoTracking()
            .Include(s => s.Sections.OrderBy(sec => sec.SortOrder))
                .ThenInclude(sec => sec.Questions.OrderBy(q => q.SortOrder))
                    .ThenInclude(q => q.Options.OrderBy(o => o.SortOrder))
            .FirstAsync(s => s.Id == surveyId);
    }

    private static ScreeningSurveyDto MapSurveyToDto(int surveyId, ScreeningSurvey survey)
    {
        return new ScreeningSurveyDto
        {
            Id = surveyId,
            Title = survey.Title,
            Description = survey.Description,
            Sections = survey.Sections
                .OrderBy(s => s.SortOrder)
                .Select(s => new ScreeningSurveySectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    PartLabel = s.PartLabel,
                    Category = s.Category,
                    SortOrder = s.SortOrder,
                    IsScored = s.IsScored,
                    Questions = s.Questions
                        .OrderBy(q => q.SortOrder)
                        .Select(q => new ScreeningSurveyQuestionDto
                        {
                            Id = q.Id,
                            Text = q.Text,
                            QuestionType = q.QuestionType,
                            SortOrder = q.SortOrder,
                            IsRequired = q.IsRequired,
                            IsScored = q.IsScored,
                            Options = q.Options
                                .OrderBy(o => o.SortOrder)
                                .Select(o => new ScreeningSurveyOptionDto
                                {
                                    Id = o.Id,
                                    Label = o.Label,
                                    SortOrder = o.SortOrder,
                                })
                                .ToList(),
                        })
                        .ToList(),
                })
                .ToList(),
        };
    }
}
