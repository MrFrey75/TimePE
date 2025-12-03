using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimePE.Core.Models;
using TimePE.Core.Services;
using TimePE.Api.DTOs;

namespace TimePE.Api.Controllers
{
    [ApiController]
    [Route("api/v1/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                IEnumerable<Payment> payments;
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    payments = await _paymentService.GetPaymentsByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    payments = await _paymentService.GetAllPaymentsAsync();
                }

                var paymentDtos = payments.Select(p => new PaymentDto
                {
                    Id = p.Oid,
                    Date = p.Date,
                    Amount = p.Amount,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
                return Ok(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payments");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Payment with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var paymentDto = new PaymentDto
                {
                    Id = payment.Oid,
                    Date = payment.Date,
                    Amount = payment.Amount,
                    Notes = payment.Notes,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt
                };
                return Ok(paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment {PaymentId}", id);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(dto.Date, dto.Amount, dto.Notes);
                var paymentDto = new PaymentDto
                {
                    Id = payment.Oid,
                    Date = payment.Date,
                    Amount = payment.Amount,
                    Notes = payment.Notes,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt
                };
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Oid }, paymentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Payment with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                if (dto.Date.HasValue) payment.Date = dto.Date.Value;
                if (dto.Amount.HasValue) payment.Amount = dto.Amount.Value;
                if (dto.Notes != null) payment.Notes = dto.Notes;

                await _paymentService.UpdatePaymentAsync(payment);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment {PaymentId}", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Payment with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                await _paymentService.DeletePaymentAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
                throw;
            }
        }
    }
}
