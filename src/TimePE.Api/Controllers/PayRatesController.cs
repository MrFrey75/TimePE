using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimePE.Core.Models;
using TimePE.Core.Services;
using TimePE.Api.DTOs;

namespace TimePE.Api.Controllers
{
    [ApiController]
    [Route("api/v1/payrates")]
    [Authorize]
    public class PayRatesController : ControllerBase
    {
        private readonly PayRateService _payRateService;
        private readonly ILogger<PayRatesController> _logger;

        public PayRatesController(PayRateService payRateService, ILogger<PayRatesController> logger)
        {
            _payRateService = payRateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayRates()
        {
            try
            {
                var payRates = await _payRateService.GetAllPayRatesAsync();
                var payRateDtos = payRates.Select(pr => new PayRateDto
                {
                    Id = pr.Oid,
                    HourlyRate = pr.HourlyRate,
                    EffectiveDate = pr.EffectiveDate,
                    EndDate = pr.EndDate,
                    CreatedAt = pr.CreatedAt,
                    UpdatedAt = pr.UpdatedAt
                });
                return Ok(payRateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pay rates");
                throw;
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentPayRate()
        {
            try
            {
                var payRate = await _payRateService.GetCurrentPayRateAsync();
                if (payRate == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = "No current pay rate found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var payRateDto = new PayRateDto
                {
                    Id = payRate.Oid,
                    HourlyRate = payRate.HourlyRate,
                    EffectiveDate = payRate.EffectiveDate,
                    EndDate = payRate.EndDate,
                    CreatedAt = payRate.CreatedAt,
                    UpdatedAt = payRate.UpdatedAt
                };
                return Ok(payRateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current pay rate");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayRate(int id)
        {
            try
            {
                var payRate = await _payRateService.GetPayRateByIdAsync(id);
                if (payRate == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Pay rate with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var payRateDto = new PayRateDto
                {
                    Id = payRate.Oid,
                    HourlyRate = payRate.HourlyRate,
                    EffectiveDate = payRate.EffectiveDate,
                    EndDate = payRate.EndDate,
                    CreatedAt = payRate.CreatedAt,
                    UpdatedAt = payRate.UpdatedAt
                };
                return Ok(payRateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pay rate {PayRateId}", id);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayRate([FromBody] CreatePayRateDto dto)
        {
            try
            {
                var payRate = await _payRateService.CreatePayRateAsync(dto.HourlyRate, dto.EffectiveDate);
                var payRateDto = new PayRateDto
                {
                    Id = payRate.Oid,
                    HourlyRate = payRate.HourlyRate,
                    EffectiveDate = payRate.EffectiveDate,
                    EndDate = payRate.EndDate,
                    CreatedAt = payRate.CreatedAt,
                    UpdatedAt = payRate.UpdatedAt
                };
                return CreatedAtAction(nameof(GetPayRate), new { id = payRate.Oid }, payRateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pay rate");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayRate(int id, [FromBody] UpdatePayRateDto dto)
        {
            try
            {
                var payRate = await _payRateService.GetPayRateByIdAsync(id);
                if (payRate == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Pay rate with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                return BadRequest(new ErrorResponseDto
                {
                    Message = "Pay rates cannot be updated directly. Create a new pay rate instead.",
                    StatusCode = 400,
                    Details = "To change pay rates, create a new pay rate with a new effective date.",
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pay rate {PayRateId}", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayRate(int id)
        {
            try
            {
                var payRate = await _payRateService.GetPayRateByIdAsync(id);
                if (payRate == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Pay rate with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                await _payRateService.DeletePayRateAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pay rate {PayRateId}", id);
                throw;
            }
        }
    }
}
