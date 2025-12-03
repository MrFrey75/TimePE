namespace TimePE.Api.DTOs;

public class PayRateDto
{
    public int Id { get; set; }
    public decimal HourlyRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePayRateDto
{
    public decimal HourlyRate { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class UpdatePayRateDto
{
    public decimal? HourlyRate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}
