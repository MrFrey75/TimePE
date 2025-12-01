using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Incidentals;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IIncidentalService _incidentalService;

    public DeleteModel(IIncidentalService incidentalService)
    {
        _incidentalService = incidentalService;
    }

    public IncidentalDeleteViewModel? Incidental { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var incidental = await _incidentalService.GetIncidentalByIdAsync(id);
        if (incidental == null)
            return NotFound();

        // Create view model with the data we need (loaded while session is active)
        Incidental = new IncidentalDeleteViewModel
        {
            Id = incidental.Oid,
            Date = incidental.Date,
            Description = incidental.Description,
            Type = incidental.Type,
            Amount = incidental.Amount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _incidentalService.DeleteIncidentalAsync(id);
        TempData["SuccessMessage"] = "Incidental deleted successfully!";
        return RedirectToPage("Index");
    }

    public class IncidentalDeleteViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public IncidentalType Type { get; set; }
        public decimal Amount { get; set; }
    }
}
