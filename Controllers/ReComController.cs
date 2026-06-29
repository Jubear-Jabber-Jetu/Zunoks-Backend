using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZunoksBackend.Models.DTOs;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[ApiController]
[Route("api/recom")]
[AllowAnonymous]
public class ReComController : ControllerBase
{
    private readonly IReComService _reComService;
    private readonly ILogger<ReComController> _logger;

    public ReComController(IReComService reComService, ILogger<ReComController> logger)
    {
        _reComService = reComService;
        _logger = logger;
    }

    [HttpPost("leads")]
    public async Task<IActionResult> SubmitLead([FromBody] ReComLeadSubmitDto dto)
    {
        try
        {
            var lead = await _reComService.CreateLeadAsync(dto);
            return Ok(new { message = "Lead submitted successfully", leadId = lead.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting ReCom lead");
            return StatusCode(500, new { message = "An error occurred while submitting your request" });
        }
    }
}
