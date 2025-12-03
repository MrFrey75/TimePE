using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TimePE.Core.Models;
using TimePE.Core.Services;
using TimePE.Api.DTOs;

namespace TimePE.Api.Controllers
{
    [ApiController]
    [Route("api/v1/timeentries")]
    [Authorize]
    public class TimeEntriesController : ControllerBase
    {
        private readonly TimeEntryService _timeEntryService;
        private readonly ILogger<TimeEntriesController> _logger;

        public TimeEntriesController(TimeEntryService timeEntryService, ILogger<TimeEntriesController> logger)
        {
            _timeEntryService = timeEntryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeEntries([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                IEnumerable<TimeEntry> timeEntries;
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    timeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    // Default to current month if no dates provided
                    var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    timeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(start, end);
                }

                var timeEntryDtos = timeEntries.Select(te => new TimeEntryDto
                {
                    Id = te.Oid,
                    Date = te.Date,
                    StartTime = te.StartTime,
                    EndTime = te.EndTime,
                    ProjectId = te.Project?.Oid,
                    ProjectName = te.Project?.Name,
                    AppliedPayRate = te.AppliedPayRate,
                    Notes = te.Notes,
                    Duration = te.Duration,
                    AmountOwed = te.AmountOwed,
                    CreatedAt = te.CreatedAt,
                    UpdatedAt = te.UpdatedAt
                });
                return Ok(timeEntryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time entries");
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimeEntry(int id)
        {
            try
            {
                var timeEntry = await _timeEntryService.GetTimeEntryByIdAsync(id);
                if (timeEntry == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Time entry with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var timeEntryDto = new TimeEntryDto
                {
                    Id = timeEntry.Oid,
                    Date = timeEntry.Date,
                    StartTime = timeEntry.StartTime,
                    EndTime = timeEntry.EndTime,
                    ProjectId = timeEntry.Project?.Oid,
                    ProjectName = timeEntry.Project?.Name,
                    AppliedPayRate = timeEntry.AppliedPayRate,
                    Notes = timeEntry.Notes,
                    Duration = timeEntry.Duration,
                    AmountOwed = timeEntry.AmountOwed,
                    CreatedAt = timeEntry.CreatedAt,
                    UpdatedAt = timeEntry.UpdatedAt
                };
                return Ok(timeEntryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching time entry {TimeEntryId}", id);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimeEntry([FromBody] CreateTimeEntryDto dto)
        {
            try
            {
                if (!dto.ProjectId.HasValue)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Message = "Project ID is required",
                        StatusCode = 400,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                var timeEntry = await _timeEntryService.CreateTimeEntryAsync(
                    dto.Date,
                    dto.StartTime,
                    dto.EndTime,
                    dto.ProjectId.Value,
                    dto.Notes
                );

                var timeEntryDto = new TimeEntryDto
                {
                    Id = timeEntry.Oid,
                    Date = timeEntry.Date,
                    StartTime = timeEntry.StartTime,
                    EndTime = timeEntry.EndTime,
                    ProjectId = timeEntry.Project?.Oid,
                    ProjectName = timeEntry.Project?.Name,
                    AppliedPayRate = timeEntry.AppliedPayRate,
                    Notes = timeEntry.Notes,
                    Duration = timeEntry.Duration,
                    AmountOwed = timeEntry.AmountOwed,
                    CreatedAt = timeEntry.CreatedAt,
                    UpdatedAt = timeEntry.UpdatedAt
                };
                return CreatedAtAction(nameof(GetTimeEntry), new { id = timeEntry.Oid }, timeEntryDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating time entry");
                return BadRequest(new ErrorResponseDto
                {
                    Message = ex.Message,
                    StatusCode = 400,
                    Timestamp = DateTime.UtcNow,
                    TraceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating time entry");
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeEntry(int id, [FromBody] UpdateTimeEntryDto dto)
        {
            try
            {
                var timeEntry = await _timeEntryService.GetTimeEntryByIdAsync(id);
                if (timeEntry == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Time entry with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                if (dto.Date.HasValue) timeEntry.Date = dto.Date.Value;
                if (dto.StartTime.HasValue) timeEntry.StartTime = dto.StartTime.Value;
                if (dto.EndTime.HasValue) timeEntry.EndTime = dto.EndTime.Value;
                if (dto.Notes != null) timeEntry.Notes = dto.Notes;

                await _timeEntryService.UpdateTimeEntryAsync(timeEntry);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating time entry {TimeEntryId}", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeEntry(int id)
        {
            try
            {
                var timeEntry = await _timeEntryService.GetTimeEntryByIdAsync(id);
                if (timeEntry == null)
                {
                    return NotFound(new ErrorResponseDto
                    {
                        Message = $"Time entry with ID {id} not found",
                        StatusCode = 404,
                        Timestamp = DateTime.UtcNow,
                        TraceId = HttpContext.TraceIdentifier
                    });
                }

                await _timeEntryService.DeleteTimeEntryAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting time entry {TimeEntryId}", id);
                throw;
            }
        }
    }
}
