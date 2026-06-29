using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public interface IReComService
{
    Task<ReComLead> CreateLeadAsync(ReComLeadSubmitDto dto);
}

public interface IReComAdminService
{
    Task<PaginatedReComLeadsDto> GetLeadsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? service = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? sortBy = null,
        string? sortDir = null);

    Task<ReComSummaryStatsDto> GetSummaryStatsAsync();
    Task<ReComLead?> GetLeadDetailsAsync(int id);
    Task<bool> DeleteLeadAsync(int id);
}
