using TimePE.Core.Models;

namespace TimePE.Api.DTOs;

public class IncidentalDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncidentalType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateIncidentalDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public IncidentalType Type { get; set; }
}

public class UpdateIncidentalDto
{
    public DateTime? Date { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public IncidentalType? Type { get; set; }
}
