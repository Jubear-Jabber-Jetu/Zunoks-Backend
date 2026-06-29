using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[Route("admin/recom/api")]
[ApiController]
[Authorize(Roles = "ReComAdmin")]
public class ReComAdminController : ControllerBase
{
    private readonly IReComAdminService _reComAdminService;

    public ReComAdminController(IReComAdminService reComAdminService)
    {
        _reComAdminService = reComAdminService;
    }

    [HttpGet("leads/paged")]
    public async Task<IActionResult> GetLeadsPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? service = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var result = await _reComAdminService.GetLeadsPagedAsync(
            page, pageSize, search, service, dateFrom, dateTo, sortBy, sortDir);
        return Ok(result);
    }

    [HttpGet("stats/summary")]
    public async Task<IActionResult> GetSummaryStats()
    {
        var stats = await _reComAdminService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("leads/{id}")]
    public async Task<IActionResult> GetLeadDetails(int id)
    {
        var lead = await _reComAdminService.GetLeadDetailsAsync(id);
        if (lead == null) return NotFound();
        return Ok(lead);
    }

    [HttpDelete("leads/{id}")]
    public async Task<IActionResult> DeleteLead(int id)
    {
        var result = await _reComAdminService.DeleteLeadAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Deleted successfully" });
    }
}
