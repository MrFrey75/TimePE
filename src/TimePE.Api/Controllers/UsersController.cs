using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimePE.Core.Models;
using TimePE.Core.Services;
using TimePE.Api.DTOs;

namespace TimePE.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AuthService authService, ILogger<UsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            // Note: AuthService doesn't have a GetAllUsers method
            // This would need to be added to the service or use a dedicated UserService
            return BadRequest(new ErrorResponseDto
            {
                Message = "Listing all users is not currently supported",
                StatusCode = 400,
                Details = "Use GET /api/v1/users/{id} to get a specific user",
                Timestamp = DateTime.UtcNow,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            // Note: AuthService doesn't have a GetUserById method
            // This would need to be added to the service
            return BadRequest(new ErrorResponseDto
            {
                Message = "Getting user by ID is not currently supported",
                StatusCode = 400,
                Details = "Use the auth endpoints for user management",
                Timestamp = DateTime.UtcNow,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var success = await _authService.CreateUserAsync(dto.Username, dto.Password);
                
                if (!success)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Message = "User creation failed. Username may already exist.",
                        StatusCode = 400,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var user = await _authService.GetUserByUsernameAsync(dto.Username);
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

                var userDto = new UserDto
                {
                    Id = user.Oid,
                    Username = user.Username,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
                
                return CreatedAtAction(nameof(GetUser), new { id = user.Oid }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (dto.Username != null)
                {
                    var success = await _authService.UpdateUsernameAsync(id, dto.Username);
                    if (!success)
                    {
                        return NotFound(new ErrorResponseDto
                        {
                            Message = $"User with ID {id} not found or username already exists",
                            StatusCode = 404,
                            Timestamp = DateTime.UtcNow,
                            TraceId = HttpContext.TraceIdentifier
                        });
                    }
                }

                if (dto.Password != null)
                {
                    var success = await _authService.UpdatePasswordAsync(id, dto.Password);
                    if (!success)
                    {
                        return NotFound(new ErrorResponseDto
                        {
                            Message = $"User with ID {id} not found",
                            StatusCode = 404,
                            Timestamp = DateTime.UtcNow,
                            TraceId = HttpContext.TraceIdentifier
                        });
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            // Note: AuthService doesn't have a delete user method
            // Soft delete would need to be implemented in the service
            return BadRequest(new ErrorResponseDto
            {
                Message = "Deleting users is not currently supported",
                StatusCode = 400,
                Details = "Consider deactivating the user instead by updating IsActive to false",
                Timestamp = DateTime.UtcNow,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}
