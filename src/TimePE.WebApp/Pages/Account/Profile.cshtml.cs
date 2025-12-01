using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(IAuthService authService, ILogger<ProfileModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public string CurrentUsername { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "New username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public string NewUsername { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string UsernamePassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        CurrentUsername = User.Identity?.Name ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostChangeUsernameAsync()
    {
        CurrentUsername = User.Identity?.Name ?? string.Empty;

        if (string.IsNullOrEmpty(NewUsername) || string.IsNullOrEmpty(UsernamePassword))
        {
            ErrorMessage = "Please fill in all required fields";
            return Page();
        }

        // Validate current password
        var user = await _authService.ValidateUserAsync(CurrentUsername, UsernamePassword);
        if (user == null)
        {
            ErrorMessage = "Current password is incorrect";
            _logger.LogWarning("Failed username change attempt - invalid password for user: {Username}", CurrentUsername);
            return Page();
        }

        // Check if new username is already taken
        var existingUser = await _authService.GetUserByUsernameAsync(NewUsername);
        if (existingUser != null && existingUser.Oid != user.Oid)
        {
            ErrorMessage = "Username is already taken";
            return Page();
        }

        // Update username
        var success = await _authService.UpdateUsernameAsync(user.Oid, NewUsername);
        if (!success)
        {
            ErrorMessage = "Failed to update username";
            return Page();
        }

        // Update claims and re-sign in with new username
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Oid.ToString()),
            new Claim(ClaimTypes.Name, NewUsername),
        };

        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignOutAsync("CookieAuth");
        await HttpContext.SignInAsync("CookieAuth", claimsPrincipal);

        _logger.LogInformation("User {OldUsername} changed username to {NewUsername}", CurrentUsername, NewUsername);

        SuccessMessage = "Username updated successfully!";
        CurrentUsername = NewUsername;
        NewUsername = string.Empty;
        UsernamePassword = string.Empty;

        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        CurrentUsername = User.Identity?.Name ?? string.Empty;

        if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
        {
            ErrorMessage = "Please fill in all required fields";
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "New password and confirmation do not match";
            return Page();
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters long";
            return Page();
        }

        // Validate current password
        var user = await _authService.ValidateUserAsync(CurrentUsername, CurrentPassword);
        if (user == null)
        {
            ErrorMessage = "Current password is incorrect";
            _logger.LogWarning("Failed password change attempt - invalid password for user: {Username}", CurrentUsername);
            return Page();
        }

        // Update password
        var success = await _authService.UpdatePasswordAsync(user.Oid, NewPassword);
        if (!success)
        {
            ErrorMessage = "Failed to update password";
            return Page();
        }

        _logger.LogInformation("User {Username} changed their password", CurrentUsername);

        SuccessMessage = "Password updated successfully!";
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;

        return Page();
    }
}
