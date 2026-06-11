using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZunoksBackend.Data;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SurveyController : ControllerBase
{
    private readonly ZunoksDbContext _context;
    private readonly IExcelService _excelService;
    private readonly ILogger<SurveyController> _logger;

    public SurveyController(
        ZunoksDbContext context,
        IExcelService excelService,
        ILogger<SurveyController> logger)
    {
        _context = context;
        _excelService = excelService;
        _logger = logger;
    }

    private DateTime GetBangladeshTime()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.UtcNow.AddHours(6);
            }
        }
    }

    private async Task<ZunoksSubmission> SaveSubmissionAsync(ZunoksSubmissionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            throw new ArgumentException("Company name is required");

        var submission = new ZunoksSubmission
        {
            CompanyName = dto.CompanyName,
            SubmittedAt = GetBangladeshTime()
        };

        foreach (var module in dto.SelectedModules)
        {
            submission.SelectedModules.Add(new SelectedModule { ModuleName = module });
        }

        foreach (var moduleResponse in dto.Responses)
        {
            var module = moduleResponse.Key;
            foreach (var question in moduleResponse.Value)
            {
                string? questionLabel = null;
                if (dto.QuestionLabels != null &&
                    dto.QuestionLabels.ContainsKey(module) &&
                    dto.QuestionLabels[module].ContainsKey(question.Key))
                {
                    questionLabel = dto.QuestionLabels[module][question.Key];
                }

                submission.Responses.Add(new ZunoksResponse
                {
                    Module = module,
                    QuestionId = question.Key,
                    QuestionLabel = questionLabel,
                    Answer = question.Value ?? string.Empty
                });
            }
        }

        _context.ZunoksSubmissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitSurvey([FromBody] ZunoksSubmissionDto dto)
    {
        try
        {
            var submission = await SaveSubmissionAsync(dto);
            return Ok(new { message = "Survey submitted successfully", submissionId = submission.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting survey");
            return StatusCode(500, new { message = "An error occurred while submitting the survey" });
        }
    }

    [HttpGet("download/{submissionId}")]
    public async Task<IActionResult> DownloadExcel(int submissionId)
    {
        try
        {
            var submission = await _context.ZunoksSubmissions
                .Include(s => s.SelectedModules)
                .Include(s => s.Responses)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound(new { message = "Survey submission not found" });

            var excelBytes = _excelService.GenerateExcel(submission);
            var fileName = $"{submission.CompanyName.Replace(" ", "_")}_{submission.Id}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel file");
            return StatusCode(500, new { message = "An error occurred while generating the Excel file" });
        }
    }

    [HttpPost("submit-and-download")]
    public async Task<IActionResult> SubmitAndDownload([FromBody] ZunoksSubmissionDto dto)
    {
        try
        {
            var submission = await SaveSubmissionAsync(dto);
            var excelBytes = _excelService.GenerateExcel(submission);
            var fileName = $"{submission.CompanyName.Replace(" ", "_")}_{submission.Id}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting survey and generating Excel");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}
