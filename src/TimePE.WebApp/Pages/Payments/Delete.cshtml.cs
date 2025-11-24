using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Payments;

public class DeleteModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public DeleteModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public Payment? Payment { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Payment = await _paymentService.GetPaymentByIdAsync(id);
        if (Payment == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _paymentService.DeletePaymentAsync(id);
        TempData["SuccessMessage"] = "Payment deleted successfully!";
        return RedirectToPage("Index");
    }
}
