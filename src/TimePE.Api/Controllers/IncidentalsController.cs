using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimePE.Core.Models;
using TimePE.Core.Services;
using TimePE.Api.DTOs;

namespace TimePE.Api.Controllers
{
    [ApiController]
    [Route("api/v1/incidentals")]
    [Authorize]
    public class IncidentalsController : ControllerBase
    {
        private readonly IncidentalService _incidentalService;
        private readonly ILogger<IncidentalsController> _logger;

        public IncidentalsController(IncidentalService incidentalService, ILogger<IncidentalsController> logger)
        {
            _incidentalService = incidentalService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetIncidentals([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                IEnumerable<Incidental> incidentals;
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    incidentals = await _incidentalService.GetIncidentalsByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    incidentals = await _incidentalService.GetAllIncidentalsAsync();
                }

                var incidentalDtos = incidentals.Select(i => new IncidentalDto
                {
                    Id = i.Oid,
                    Date = i.Date,
                    Amount = i.Amount,
                    Description = i.Description,
                    Type = i.Type,
                    CreatedAt = i.CreatedAt,
                    UpdatedAt = i.UpdatedAt
                });
                return Ok(incidentalDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching incidentals");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncidental(int id)
        {
            try
            {
                var incidental = await _incidentalService.GetIncidentalByIdAsync(id);
                if (incidental == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Incidental with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var incidentalDto = new IncidentalDto
                {
                    Id = incidental.Oid,
                    Date = incidental.Date,
                    Amount = incidental.Amount,
                    Description = incidental.Description,
                    Type = incidental.Type,
                    CreatedAt = incidental.CreatedAt,
                    UpdatedAt = incidental.UpdatedAt
                };
                return Ok(incidentalDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching incidental {IncidentalId}", id);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateIncidental([FromBody] CreateIncidentalDto dto)
        {
            try
            {
                var incidental = await _incidentalService.CreateIncidentalAsync(
                    dto.Date,
                    dto.Amount,
                    dto.Description,
                    dto.Type
                );

                var incidentalDto = new IncidentalDto
                {
                    Id = incidental.Oid,
                    Date = incidental.Date,
                    Amount = incidental.Amount,
                    Description = incidental.Description,
                    Type = incidental.Type,
                    CreatedAt = incidental.CreatedAt,
                    UpdatedAt = incidental.UpdatedAt
                };
                return CreatedAtAction(nameof(GetIncidental), new { id = incidental.Oid }, incidentalDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incidental");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncidental(int id, [FromBody] UpdateIncidentalDto dto)
        {
            try
            {
                var incidental = await _incidentalService.GetIncidentalByIdAsync(id);
                if (incidental == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Incidental with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                if (dto.Date.HasValue) incidental.Date = dto.Date.Value;
                if (dto.Amount.HasValue) incidental.Amount = dto.Amount.Value;
                if (dto.Description != null) incidental.Description = dto.Description;
                if (dto.Type.HasValue) incidental.Type = dto.Type.Value;

                await _incidentalService.UpdateIncidentalAsync(incidental);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incidental {IncidentalId}", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncidental(int id)
        {
            try
            {
                var incidental = await _incidentalService.GetIncidentalByIdAsync(id);
                if (incidental == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Incidental with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                await _incidentalService.DeleteIncidentalAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting incidental {IncidentalId}", id);
                throw;
            }
        }
    }
}
