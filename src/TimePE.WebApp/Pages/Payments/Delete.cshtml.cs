using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.Payments;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public DeleteModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public PaymentDeleteViewModel? Payment { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound();

        // Create view model with the data we need (loaded while session is active)
        Payment = new PaymentDeleteViewModel
        {
            Id = payment.Oid,
            Date = payment.Date,
            Amount = payment.Amount,
            Notes = payment.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _paymentService.DeletePaymentAsync(id);
        TempData["SuccessMessage"] = "Payment deleted successfully!";
        return RedirectToPage("Index");
    }

    public class PaymentDeleteViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
