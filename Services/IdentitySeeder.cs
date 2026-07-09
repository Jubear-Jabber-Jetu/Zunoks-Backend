using Microsoft.AspNetCore.Identity;
using ZunoksBackend.Models;

namespace ZunoksBackend.Services;

public class IdentitySeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger<IdentitySeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        const string adminRole = "ReComAdmin";
        const string screeningAdminRole = "ScreeningAdmin";
        if (!await _roleManager.RoleExistsAsync(adminRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        if (!await _roleManager.RoleExistsAsync(screeningAdminRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(screeningAdminRole));
        }

        var email = _configuration["ReComAdmin:Email"];
        var password = _configuration["ReComAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("ReComAdmin credentials not configured; skipping admin seed");
            return;
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to seed ReCom admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            _logger.LogInformation("ReCom admin user created: {Email}", email);
        }

        if (!await _userManager.IsInRoleAsync(user, adminRole))
        {
            await _userManager.AddToRoleAsync(user, adminRole);
        }

        if (!await _userManager.IsInRoleAsync(user, screeningAdminRole))
        {
            await _userManager.AddToRoleAsync(user, screeningAdminRole);
        }
    }
}
