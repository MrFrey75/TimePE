using Microsoft.AspNetCore.Mvc;
using TimePE.Api.DTOs;
using TimePE.Api.Services;
using TimePE.Core.Services;

namespace TimePE.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var user = await _authService.ValidateUserAsync(request.Username, request.Password);
            
            if (user == null)
            {
                return Unauthorized(new ErrorResponseDto
                {
                    Message = "Invalid username or password",
                    StatusCode = 401,
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            // Update last login
            await _authService.UpdateLastLoginAsync(user.Oid);

            var token = _jwtService.GenerateToken(user.Oid, user.Username);
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Oid,
                    Username = user.Username,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            throw;
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto request)
    {
        try
        {
            var success = await _authService.CreateUserAsync(request.Username, request.Password);
            
            if (!success)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "User registration failed. Username may already exist.",
                    StatusCode = 400,
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            // Get the newly created user
            var user = await _authService.GetUserByUsernameAsync(request.Username);
            
            if (user == null)
            {
                return StatusCode(500, new ErrorResponseDto
                {
                    Message = "User created but could not be retrieved.",
                    StatusCode = 500,
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            var token = _jwtService.GenerateToken(user.Oid, user.Username);
            var expiresAt = DateTime.UtcNow.AddMinutes(60);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Oid,
                    Username = user.Username,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            };

            _logger.LogInformation("User {Username} registered successfully", user.Username);
            
            return CreatedAtAction(nameof(Register), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
            throw;
        }
    }
}
