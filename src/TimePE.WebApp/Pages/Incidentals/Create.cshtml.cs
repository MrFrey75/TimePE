using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Incidentals;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IIncidentalService _incidentalService;

    public CreateModel(IIncidentalService incidentalService)
    {
        _incidentalService = incidentalService;
    }

    [BindProperty]
    public DateTime Date { get; set; } = DateTime.Today;

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public IncidentalType Type { get; set; } = IncidentalType.Owed;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _incidentalService.CreateIncidentalAsync(Date, Amount, Description, Type);
        TempData["SuccessMessage"] = "Incidental created successfully!";
        return RedirectToPage("Index");
    }
}
