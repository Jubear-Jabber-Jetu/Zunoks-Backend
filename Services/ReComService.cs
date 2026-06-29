using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;

namespace ZunoksBackend.Services;

public class ReComService : IReComService
{
    public static readonly HashSet<string> AllowedServices = new(StringComparer.Ordinal)
    {
        "Provident Fund",
        "Payroll Management",
        "HRMS",
    };

    private readonly ZunoksDbContext _context;

    public ReComService(ZunoksDbContext context)
    {
        _context = context;
    }

    public async Task<ReComLead> CreateLeadAsync(ReComLeadSubmitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");
        if (string.IsNullOrWhiteSpace(dto.Company))
            throw new ArgumentException("Company is required");
        if (string.IsNullOrWhiteSpace(dto.CompanySize))
            throw new ArgumentException("Company size is required");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new ArgumentException("Phone is required");
        if (dto.Services == null || dto.Services.Count == 0)
            throw new ArgumentException("At least one service is required");

        var services = dto.Services
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (services.Count == 0)
            throw new ArgumentException("At least one service is required");

        foreach (var service in services)
        {
            if (!AllowedServices.Contains(service))
                throw new ArgumentException($"Invalid service: {service}");
        }

        var lead = new ReComLead
        {
            Name = dto.Name.Trim(),
            Company = dto.Company.Trim(),
            CompanySize = dto.CompanySize.Trim(),
            Phone = dto.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Details = string.IsNullOrWhiteSpace(dto.Details) ? null : dto.Details.Trim(),
            SubmittedAt = BangladeshTime.Now(),
        };

        foreach (var service in services)
        {
            lead.Services.Add(new ReComLeadService { ServiceName = service });
        }

        _context.ReComLeads.Add(lead);
        await _context.SaveChangesAsync();
        return lead;
    }
}

public class ReComAdminService : IReComAdminService
{
    private readonly ZunoksDbContext _context;
    private readonly ILogger<ReComAdminService> _logger;

    public ReComAdminService(ZunoksDbContext context, ILogger<ReComAdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedReComLeadsDto> GetLeadsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? service = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? sortBy = null,
        string? sortDir = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var query = _context.ReComLeads
            .Include(l => l.Services)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(l =>
                l.Name.ToLower().Contains(term) ||
                l.Company.ToLower().Contains(term) ||
                l.Phone.ToLower().Contains(term) ||
                (l.Email != null && l.Email.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(service) && service != "All")
        {
            query = query.Where(l => l.Services.Any(s => s.ServiceName == service));
        }

        if (dateFrom.HasValue)
        {
            var from = dateFrom.Value.Date;
            query = query.Where(l => l.SubmittedAt >= from);
        }

        if (dateTo.HasValue)
        {
            var toExclusive = dateTo.Value.Date.AddDays(1);
            query = query.Where(l => l.SubmittedAt < toExclusive);
        }

        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLower()) switch
        {
            "name" => descending ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
            "company" => descending ? query.OrderByDescending(l => l.Company) : query.OrderBy(l => l.Company),
            "id" => descending ? query.OrderByDescending(l => l.Id) : query.OrderBy(l => l.Id),
            "services" => descending
                ? query.OrderByDescending(l => l.Services.Count)
                : query.OrderBy(l => l.Services.Count),
            _ => descending ? query.OrderByDescending(l => l.SubmittedAt) : query.OrderBy(l => l.SubmittedAt),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ReComLeadListItemDto
            {
                Id = l.Id,
                Name = l.Name,
                Company = l.Company,
                CompanySize = l.CompanySize,
                Phone = l.Phone,
                Email = l.Email,
                SubmittedAt = l.SubmittedAt,
                ServiceCount = l.Services.Count,
                ServiceNames = l.Services.Select(s => s.ServiceName).ToList(),
            })
            .ToListAsync();

        return new PaginatedReComLeadsDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<ReComSummaryStatsDto> GetSummaryStatsAsync()
    {
        var totalLeads = await _context.ReComLeads.CountAsync();
        var now = BangladeshTime.Now();
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var leadsThisMonth = await _context.ReComLeads
            .CountAsync(l => l.SubmittedAt >= monthStart);
        var latest = await _context.ReComLeads.MaxAsync(l => (DateTime?)l.SubmittedAt);
        var earliest = await _context.ReComLeads.MinAsync(l => (DateTime?)l.SubmittedAt);

        var byService = await _context.ReComLeadServices
            .GroupBy(s => s.ServiceName)
            .Select(g => new ServiceStatDto
            {
                ServiceName = g.Key,
                Count = g.Select(x => x.ReComLeadId).Distinct().Count(),
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return new ReComSummaryStatsDto
        {
            TotalLeads = totalLeads,
            LeadsThisMonth = leadsThisMonth,
            LatestSubmittedAt = latest,
            EarliestSubmittedAt = earliest,
            LeadsByService = byService,
        };
    }

    public async Task<ReComLead?> GetLeadDetailsAsync(int id)
    {
        return await _context.ReComLeads
            .Include(l => l.Services)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> DeleteLeadAsync(int id)
    {
        try
        {
            var lead = await _context.ReComLeads.FindAsync(id);
            if (lead == null) return false;

            _context.ReComLeads.Remove(lead);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ReCom lead {Id}", id);
            throw;
        }
    }
}
