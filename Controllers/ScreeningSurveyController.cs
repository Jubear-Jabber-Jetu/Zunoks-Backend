using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZunoksBackend.Models.DTOs;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[ApiController]
[Route("api/screening")]
[AllowAnonymous]
public class ScreeningSurveyController : ControllerBase
{
    private readonly IScreeningSurveyService _screeningSurveyService;
    private readonly ILogger<ScreeningSurveyController> _logger;

    public ScreeningSurveyController(
        IScreeningSurveyService screeningSurveyService,
        ILogger<ScreeningSurveyController> logger)
    {
        _screeningSurveyService = screeningSurveyService;
        _logger = logger;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSurvey()
    {
        var survey = await _screeningSurveyService.GetActiveSurveyAsync();
        if (survey == null) return NotFound(new { message = "No active survey found" });
        return Ok(survey);
    }

    [HttpGet("{surveyId}/questions")]
    public async Task<IActionResult> GetQuestions(int surveyId)
    {
        var survey = await _screeningSurveyService.GetSurveyQuestionsAsync(surveyId);
        if (survey == null) return NotFound(new { message = "Survey not found" });
        return Ok(survey);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ScreeningSubmitDto dto)
    {
        try
        {
            var submission = await _screeningSurveyService.SubmitSurveyAsync(dto);
            return Ok(new
            {
                message = "Survey submitted successfully",
                submissionId = submission.Id,
                totalScore = submission.TotalScore,
                maxScore = submission.MaxScore,
                percentage = submission.Percentage,
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting screening survey");
            return StatusCode(500, new { message = "An error occurred while submitting the survey" });
        }
    }
}
