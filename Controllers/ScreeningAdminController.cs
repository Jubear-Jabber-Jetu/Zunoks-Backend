using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[Route("admin/screening/api")]
[ApiController]
[Authorize(Roles = "ScreeningAdmin,ReComAdmin")]
public class ScreeningAdminController : ControllerBase
{
    private readonly IScreeningAdminService _screeningAdminService;
    private readonly IScreeningExportService _screeningExportService;

    public ScreeningAdminController(
        IScreeningAdminService screeningAdminService,
        IScreeningExportService screeningExportService)
    {
        _screeningAdminService = screeningAdminService;
        _screeningExportService = screeningExportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int? surveyId = null)
    {
        var stats = await _screeningAdminService.GetDashboardStatsAsync(surveyId);
        return Ok(stats);
    }

    [HttpGet("candidates/paged")]
    public async Task<IActionResult> GetCandidatesPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] int? surveyId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] decimal? minScore = null,
        [FromQuery] decimal? maxScore = null,
        [FromQuery] string? experience = null,
        [FromQuery] string? expectedSalary = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var result = await _screeningAdminService.GetCandidatesPagedAsync(
            page, pageSize, surveyId, search, dateFrom, dateTo,
            minScore, maxScore, experience, expectedSalary, sortBy, sortDir);
        return Ok(result);
    }

    [HttpGet("candidates/{id}")]
    public async Task<IActionResult> GetCandidateDetails(int id)
    {
        var details = await _screeningAdminService.GetCandidateDetailsAsync(id);
        if (details == null) return NotFound();
        return Ok(details);
    }

    [HttpGet("rankings")]
    public async Task<IActionResult> GetRankings(
        [FromQuery] string rankBy = "total",
        [FromQuery] int? surveyId = null,
        [FromQuery] int limit = 50)
    {
        var rankings = await _screeningAdminService.GetRankingsAsync(rankBy, surveyId, limit);
        return Ok(rankings);
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel([FromQuery] int? surveyId = null, [FromQuery] int[]? ids = null)
    {
        var submissions = await _screeningAdminService.GetSubmissionsForExportAsync(surveyId, ids);
        if (submissions.Count == 0) return NotFound(new { message = "No submissions to export" });

        var bytes = _screeningExportService.GenerateExcel(submissions);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Screening_Results_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] int? surveyId = null, [FromQuery] int[]? ids = null)
    {
        var submissions = await _screeningAdminService.GetSubmissionsForExportAsync(surveyId, ids);
        if (submissions.Count == 0) return NotFound(new { message = "No submissions to export" });

        var bytes = _screeningExportService.GenerateCsv(submissions);
        return File(bytes, "text/csv; charset=utf-8",
            $"Screening_Results_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }
}
