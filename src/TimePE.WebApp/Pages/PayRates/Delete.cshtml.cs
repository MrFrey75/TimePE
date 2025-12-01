using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.PayRates;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IPayRateService _payRateService;

    public DeleteModel(IPayRateService payRateService)
    {
        _payRateService = payRateService;
    }

    public PayRateDeleteViewModel? PayRate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var payRate = await _payRateService.GetPayRateByIdAsync(id);
        if (payRate == null)
            return NotFound();

        // Create view model with the data we need
        PayRate = new PayRateDeleteViewModel
        {
            Id = payRate.Oid,
            HourlyRate = payRate.HourlyRate,
            EffectiveDate = payRate.EffectiveDate,
            EndDate = payRate.EndDate
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _payRateService.DeletePayRateAsync(id);
        TempData["SuccessMessage"] = "Pay rate deleted successfully!";
        return RedirectToPage("Index");
    }

    public class PayRateDeleteViewModel
    {
        public int Id { get; set; }
        public decimal HourlyRate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
