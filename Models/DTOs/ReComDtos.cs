using System.ComponentModel.DataAnnotations;

namespace ZunoksBackend.Models.DTOs;

public class ReComLeadSubmitDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Company { get; set; } = string.Empty;

    [Required]
    public string CompanySize { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> Services { get; set; } = new();

    public string? Details { get; set; }
}

public class ReComLeadListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string CompanySize { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int ServiceCount { get; set; }
    public List<string> ServiceNames { get; set; } = new();
}

public class PaginatedReComLeadsDto
{
    public List<ReComLeadListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ReComSummaryStatsDto
{
    public int TotalLeads { get; set; }
    public int LeadsThisMonth { get; set; }
    public DateTime? LatestSubmittedAt { get; set; }
    public DateTime? EarliestSubmittedAt { get; set; }
    public List<ServiceStatDto> LeadsByService { get; set; } = new();
}

public class ServiceStatDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LoginDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
}
