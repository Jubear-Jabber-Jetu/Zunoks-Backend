using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public class AdminService : IAdminService
{
    private readonly ZunoksDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(ZunoksDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ZunoksSubmission>> GetAllSubmissionsAsync()
    {
        return await _context.ZunoksSubmissions
            .Include(s => s.SelectedModules)
            .Include(s => s.Responses)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<PaginatedSubmissionsDto> GetSubmissionsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? module = null,
        string? sortBy = null,
        string? sortDir = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var query = _context.ZunoksSubmissions
            .Include(s => s.SelectedModules)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.CompanyName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(module) && module != "All")
        {
            query = query.Where(s => s.SelectedModules.Any(m => m.ModuleName == module));
        }

        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLower()) switch
        {
            "companyname" => descending
                ? query.OrderByDescending(s => s.CompanyName)
                : query.OrderBy(s => s.CompanyName),
            "id" => descending
                ? query.OrderByDescending(s => s.Id)
                : query.OrderBy(s => s.Id),
            "modules" => descending
                ? query.OrderByDescending(s => s.SelectedModules.Count)
                : query.OrderBy(s => s.SelectedModules.Count),
            _ => descending
                ? query.OrderByDescending(s => s.SubmittedAt)
                : query.OrderBy(s => s.SubmittedAt),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ZunoksSubmissionListItemDto
            {
                Id = s.Id,
                CompanyName = s.CompanyName,
                SubmittedAt = s.SubmittedAt,
                ModuleCount = s.SelectedModules.Count,
                ModuleNames = s.SelectedModules.Select(m => m.ModuleName).ToList(),
            })
            .ToListAsync();

        return new PaginatedSubmissionsDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<AdminSummaryStatsDto> GetSummaryStatsAsync()
    {
        var totalSubmissions = await _context.ZunoksSubmissions.CountAsync();
        var totalResponses = await _context.ZunoksResponses.CountAsync();
        var latest = await _context.ZunoksSubmissions.MaxAsync(s => (DateTime?)s.SubmittedAt);
        var earliest = await _context.ZunoksSubmissions.MinAsync(s => (DateTime?)s.SubmittedAt);

        var byModule = await _context.SelectedModules
            .GroupBy(m => m.ModuleName)
            .Select(g => new ModuleStatDto
            {
                ModuleName = g.Key,
                Count = g.Select(x => x.ZunoksSubmissionId).Distinct().Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return new AdminSummaryStatsDto
        {
            TotalSubmissions = totalSubmissions,
            TotalResponses = totalResponses,
            LatestSubmittedAt = latest,
            EarliestSubmittedAt = earliest,
            SubmissionsByModule = byModule,
        };
    }

    public async Task<List<ZunoksSubmission>> GetSubmissionsForExportAsync(
        IEnumerable<int>? ids = null,
        string? module = null)
    {
        var query = _context.ZunoksSubmissions
            .Include(s => s.SelectedModules)
            .Include(s => s.Responses)
            .AsQueryable();

        if (ids != null)
        {
            var idList = ids.ToList();
            if (idList.Count > 0)
                query = query.Where(s => idList.Contains(s.Id));
        }

        if (!string.IsNullOrWhiteSpace(module) && module != "All")
        {
            query = query.Where(s => s.SelectedModules.Any(m => m.ModuleName == module));
        }

        return await query
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    public async Task<ZunoksSubmission?> GetSubmissionDetailsAsync(int id)
    {
        return await _context.ZunoksSubmissions
            .Include(s => s.SelectedModules)
            .Include(s => s.Responses)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> DeleteSubmissionAsync(int id)
    {
        try
        {
            var submission = await _context.ZunoksSubmissions.FindAsync(id);
            if (submission == null) return false;

            _context.ZunoksSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting submission {Id}", id);
            throw;
        }
    }
}
