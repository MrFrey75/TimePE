using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public IndexModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public IEnumerable<Payment> Payments { get; set; } = new List<Payment>();
    public decimal TotalPaid { get; set; }

    public async Task OnGetAsync()
    {
        Payments = await _paymentService.GetAllPaymentsAsync();
        TotalPaid = Payments.Sum(p => p.Amount);
    }
}
