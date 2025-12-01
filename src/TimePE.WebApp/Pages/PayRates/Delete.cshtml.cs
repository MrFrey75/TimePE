using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.PayRates;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IPayRateService _payRateService;

    public DeleteModel(IPayRateService payRateService)
    {
        _payRateService = payRateService;
    }

    public PayRate? PayRate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        PayRate = await _payRateService.GetPayRateByIdAsync(id);
        if (PayRate == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _payRateService.DeletePayRateAsync(id);
        TempData["SuccessMessage"] = "Pay rate deleted successfully!";
        return RedirectToPage("Index");
    }
}
