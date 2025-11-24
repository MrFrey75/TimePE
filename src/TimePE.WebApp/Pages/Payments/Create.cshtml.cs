using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.Payments;

public class CreateModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public CreateModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [BindProperty]
    public DateTime Date { get; set; } = DateTime.Today;

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _paymentService.CreatePaymentAsync(Date, Amount, Notes);
        TempData["SuccessMessage"] = "Payment recorded successfully!";
        return RedirectToPage("Index");
    }
}
