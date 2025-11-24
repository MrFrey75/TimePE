using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Incidentals;

public class DeleteModel : PageModel
{
    private readonly IIncidentalService _incidentalService;

    public DeleteModel(IIncidentalService incidentalService)
    {
        _incidentalService = incidentalService;
    }

    public Incidental? Incidental { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Incidental = await _incidentalService.GetIncidentalByIdAsync(id);
        if (Incidental == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _incidentalService.DeleteIncidentalAsync(id);
        TempData["SuccessMessage"] = "Incidental deleted successfully!";
        return RedirectToPage("Index");
    }
}
