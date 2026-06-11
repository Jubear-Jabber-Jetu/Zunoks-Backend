using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public interface IAdminService
{
    Task<List<ZunoksSubmission>> GetAllSubmissionsAsync();
    Task<PaginatedSubmissionsDto> GetSubmissionsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? module = null,
        string? sortBy = null,
        string? sortDir = null);
    Task<AdminSummaryStatsDto> GetSummaryStatsAsync();
    Task<List<ZunoksSubmission>> GetSubmissionsForExportAsync(IEnumerable<int>? ids = null, string? module = null);
    Task<ZunoksSubmission?> GetSubmissionDetailsAsync(int id);
    Task<bool> DeleteSubmissionAsync(int id);
}
