namespace TimePE.Api.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePaymentDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePaymentDto
{
    public DateTime? Date { get; set; }
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}
