using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public interface IScreeningSurveyService
{
    Task<ScreeningSurveyDto?> GetActiveSurveyAsync();
    Task<ScreeningSurveyDto?> GetSurveyQuestionsAsync(int surveyId);
    Task<ScreeningSubmission> SubmitSurveyAsync(ScreeningSubmitDto dto);
}

public interface IScreeningAdminService
{
    Task<ScreeningDashboardStatsDto> GetDashboardStatsAsync(int? surveyId = null);
    Task<PaginatedScreeningSubmissionsDto> GetCandidatesPagedAsync(
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
        string? sortDir = null);
    Task<ScreeningSubmissionDetailDto?> GetCandidateDetailsAsync(int id);
    Task<ScreeningRankingDto> GetRankingsAsync(string rankBy, int? surveyId = null, int limit = 50);
    Task<List<ScreeningSubmission>> GetSubmissionsForExportAsync(int? surveyId = null, IEnumerable<int>? ids = null);
}

public interface IScreeningExportService
{
    byte[] GenerateExcel(IEnumerable<ScreeningSubmission> submissions);
    byte[] GenerateCsv(IEnumerable<ScreeningSubmission> submissions);
}
