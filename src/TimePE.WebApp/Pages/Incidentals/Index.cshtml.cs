using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Incidentals;

public class IndexModel : PageModel
{
    private readonly IIncidentalService _incidentalService;

    public IndexModel(IIncidentalService incidentalService)
    {
        _incidentalService = incidentalService;
    }

    public IEnumerable<Incidental> Incidentals { get; set; } = new List<Incidental>();
    public decimal TotalOwed { get; set; }
    public decimal TotalOwedBy { get; set; }
    public decimal NetIncidentals { get; set; }

    public async Task OnGetAsync()
    {
        Incidentals = await _incidentalService.GetAllIncidentalsAsync();
        TotalOwed = Incidentals.Where(i => i.Type == IncidentalType.Owed).Sum(i => i.Amount);
        TotalOwedBy = Incidentals.Where(i => i.Type == IncidentalType.OwedBy).Sum(i => i.Amount);
        NetIncidentals = TotalOwed - TotalOwedBy;
    }
}
