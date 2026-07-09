using System.ComponentModel.DataAnnotations;

namespace ZunoksBackend.Models.DTOs;

public class ScreeningSurveyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ScreeningSurveySectionDto> Sections { get; set; } = new();
}

public class ScreeningSurveySectionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PartLabel { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsScored { get; set; }
    public List<ScreeningSurveyQuestionDto> Questions { get; set; } = new();
}

public class ScreeningSurveyQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
    public bool IsScored { get; set; }
    public List<ScreeningSurveyOptionDto> Options { get; set; } = new();
}

public class ScreeningSurveyOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ScreeningSubmitDto
{
    [Required]
    public int SurveyId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string? YearsOfExperience { get; set; }

    public string? NoticePeriod { get; set; }

    public string? PortfolioLinks { get; set; }

    public string? ExpectedSalary { get; set; }

    [Required]
    [MinLength(1)]
    public List<ScreeningAnswerSubmitDto> Answers { get; set; } = new();
}

public class ScreeningAnswerSubmitDto
{
    public int QuestionId { get; set; }
    public int? OptionId { get; set; }
    public string? TextAnswer { get; set; }
}

public class ScreeningSubmissionListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? YearsOfExperience { get; set; }
    public string? ExpectedSalary { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public int Rank { get; set; }
    public int TechnicalScore { get; set; }
    public int LeadershipScore { get; set; }
    public int ToolScore { get; set; }
}

public class PaginatedScreeningSubmissionsDto
{
    public List<ScreeningSubmissionListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ScreeningDashboardStatsDto
{
    public int TotalCandidates { get; set; }
    public decimal AverageScore { get; set; }
    public int HighestScore { get; set; }
    public int LowestScore { get; set; }
    public List<ScreeningSubmissionListItemDto> TopCandidates { get; set; } = new();
    public List<ScreeningSubmissionListItemDto> RecentSubmissions { get; set; } = new();
}

public class ScreeningSectionScoreDto
{
    public int SectionId { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
}

public class ScreeningAnswerDetailDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string? SelectedOptionLabel { get; set; }
    public string? TextAnswer { get; set; }
    public int ScoreEarned { get; set; }
}

public class ScreeningSubmissionDetailDto
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public string SurveyTitle { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? YearsOfExperience { get; set; }
    public string? NoticePeriod { get; set; }
    public string? PortfolioLinks { get; set; }
    public string? ExpectedSalary { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int TotalScore { get; set; }
    public int MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public int Rank { get; set; }
    public List<ScreeningSectionScoreDto> SectionScores { get; set; } = new();
    public List<ScreeningAnswerDetailDto> Answers { get; set; } = new();
}

public class ScreeningRankingDto
{
    public List<ScreeningSubmissionListItemDto> Items { get; set; } = new();
    public string RankedBy { get; set; } = string.Empty;
}
