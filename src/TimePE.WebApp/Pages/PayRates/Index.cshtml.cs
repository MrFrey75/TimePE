using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.PayRates;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IPayRateService _payRateService;

    public IndexModel(IPayRateService payRateService)
    {
        _payRateService = payRateService;
    }

    public IEnumerable<PayRate> PayRates { get; set; } = new List<PayRate>();
    public PayRate? CurrentPayRate { get; set; }

    public async Task OnGetAsync()
    {
        PayRates = await _payRateService.GetAllPayRatesAsync(includeDeleted: false);
        CurrentPayRate = await _payRateService.GetCurrentPayRateAsync();
    }
}
