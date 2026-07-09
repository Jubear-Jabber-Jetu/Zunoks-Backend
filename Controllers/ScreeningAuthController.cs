using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZunoksBackend.Models;
using ZunoksBackend.Models.DTOs;
using ZunoksBackend.Services;

namespace ZunoksBackend.Controllers;

[ApiController]
[Route("api/screening/auth")]
[AllowAnonymous]
public class ScreeningAuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public ScreeningAuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email and password are required" });

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password" });

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("ScreeningAdmin") && !roles.Contains("ReComAdmin"))
            return Unauthorized(new { message = "Access denied" });

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user, roles);
        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Email = user.Email ?? dto.Email,
        });
    }
}
