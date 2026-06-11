using Microsoft.AspNetCore.Mvc;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IExcelService _excelService;
    private readonly ICsvService _csvService;

    public AdminController(
        IAdminService adminService,
        IExcelService excelService,
        ICsvService csvService)
    {
        _adminService = adminService;
        _excelService = excelService;
        _csvService = csvService;
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var submission = await _adminService.GetSubmissionDetailsAsync(id);
        if (submission == null) return NotFound();

        var excelBytes = _excelService.GenerateExcel(submission);
        var fileName = $"{submission.CompanyName.Replace(" ", "_")}_{submission.Id}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("api/submissions")]
    public async Task<IActionResult> GetSubmissionsApi()
    {
        var submissions = await _adminService.GetAllSubmissionsAsync();
        return Ok(submissions);
    }

    [HttpGet("api/submissions/paged")]
    public async Task<IActionResult> GetSubmissionsPagedApi(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? module = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null)
    {
        var result = await _adminService.GetSubmissionsPagedAsync(page, pageSize, search, module, sortBy, sortDir);
        return Ok(result);
    }

    [HttpGet("api/stats/summary")]
    public async Task<IActionResult> GetSummaryStatsApi()
    {
        var stats = await _adminService.GetSummaryStatsAsync();
        return Ok(stats);
    }

    [HttpGet("api/submissions/{id}")]
    public async Task<IActionResult> GetSubmissionDetailsApi(int id)
    {
        var submission = await _adminService.GetSubmissionDetailsAsync(id);
        if (submission == null) return NotFound();
        return Ok(submission);
    }

    [HttpDelete("api/submissions/{id}")]
    public async Task<IActionResult> DeleteSubmissionApi(int id)
    {
        var result = await _adminService.DeleteSubmissionAsync(id);
        if (!result) return NotFound();
        return Ok(new { message = "Deleted successfully" });
    }

    [HttpGet("api/export/excel/master")]
    public async Task<IActionResult> ExportMasterExcel([FromQuery] int[]? ids = null)
    {
        var submissions = await _adminService.GetSubmissionsForExportAsync(ids);
        if (submissions.Count == 0) return NotFound(new { message = "No submissions to export" });

        var bytes = _excelService.GenerateMasterWorkbook(submissions);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Zunoks_Master_Analysis_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet("api/export/csv/long")]
    public async Task<IActionResult> ExportLongCsv([FromQuery] int[]? ids = null)
    {
        var submissions = await _adminService.GetSubmissionsForExportAsync(ids);
        if (submissions.Count == 0) return NotFound(new { message = "No submissions to export" });

        var bytes = _csvService.GenerateLongCsv(submissions);
        return File(bytes, "text/csv; charset=utf-8", $"Zunoks_Long_Data_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    [HttpGet("api/export/excel/compare")]
    public async Task<IActionResult> ExportCompareExcel(
        [FromQuery] string module,
        [FromQuery] int[]? ids = null)
    {
        if (string.IsNullOrWhiteSpace(module))
            return BadRequest(new { message = "Module is required" });

        var submissions = await _adminService.GetSubmissionsForExportAsync(ids, module);
        if (submissions.Count == 0) return NotFound(new { message = "No submissions to export for this module" });

        var bytes = _excelService.GenerateCompareMatrixExcel(submissions, module);
        var safeModule = module.Replace(" ", "_");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Zunoks_Compare_{safeModule}_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
    }
}
