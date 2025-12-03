namespace TimePE.Api.DTOs;

public class TimeEntryDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public decimal AppliedPayRate { get; set; }
    public string? Notes { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal AmountOwed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTimeEntryDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int? ProjectId { get; set; }
    public decimal AppliedPayRate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTimeEntryDto
{
    public DateTime? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? ProjectId { get; set; }
    public decimal? AppliedPayRate { get; set; }
    public string? Notes { get; set; }
}
