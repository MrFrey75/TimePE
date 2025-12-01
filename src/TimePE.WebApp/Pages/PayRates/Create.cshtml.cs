using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.PayRates;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IPayRateService _payRateService;

    public CreateModel(IPayRateService payRateService)
    {
        _payRateService = payRateService;
    }

    [BindProperty]
    public decimal HourlyRate { get; set; }

    [BindProperty]
    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    public async Task OnGetAsync()
    {
        var currentRate = await _payRateService.GetCurrentPayRateAsync();
        if (currentRate != null)
        {
            HourlyRate = currentRate.HourlyRate;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (HourlyRate <= 0)
        {
            ModelState.AddModelError(nameof(HourlyRate), "Hourly rate must be greater than zero.");
            return Page();
        }

        await _payRateService.CreatePayRateAsync(HourlyRate, EffectiveDate);
        TempData["SuccessMessage"] = "Pay rate created successfully!";
        return RedirectToPage("Index");
    }
}
