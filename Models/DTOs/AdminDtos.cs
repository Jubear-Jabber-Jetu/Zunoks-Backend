namespace ZunoksBackend.Models.DTOs;

public class ZunoksSubmissionListItemDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public int ModuleCount { get; set; }
    public List<string> ModuleNames { get; set; } = new();
}

public class PaginatedSubmissionsDto
{
    public List<ZunoksSubmissionListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdminSummaryStatsDto
{
    public int TotalSubmissions { get; set; }
    public int TotalResponses { get; set; }
    public DateTime? LatestSubmittedAt { get; set; }
    public DateTime? EarliestSubmittedAt { get; set; }
    public List<ModuleStatDto> SubmissionsByModule { get; set; } = new();
}

public class ModuleStatDto
{
    public string ModuleName { get; set; } = string.Empty;
    public int Count { get; set; }
}
