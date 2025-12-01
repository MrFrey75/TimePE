using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;
using DevExpress.Xpo;

namespace TimePE.WebApp.Pages.Incidentals;

[Authorize]
public class EditModel : PageModel
{
    private readonly IIncidentalService _incidentalService;

    public EditModel(IIncidentalService incidentalService)
    {
        _incidentalService = incidentalService;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public DateTime Date { get; set; }

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public IncidentalType Type { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var incidental = await _incidentalService.GetIncidentalByIdAsync(id);
        if (incidental == null)
            return NotFound();

        Id = incidental.Oid;
        Date = incidental.Date;
        Amount = incidental.Amount;
        Description = incidental.Description;
        Type = incidental.Type;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var incidental = await _incidentalService.GetIncidentalByIdAsync(Id);
        if (incidental == null)
            return NotFound();

        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var editIncidental = uow.GetObjectByKey<Incidental>(Id);
        if (editIncidental != null)
        {
            editIncidental.Date = Date;
            editIncidental.Amount = Amount;
            editIncidental.Description = Description;
            editIncidental.Type = Type;
        }

        await _incidentalService.UpdateIncidentalAsync(editIncidental!);
        TempData["SuccessMessage"] = "Incidental updated successfully!";
        return RedirectToPage("Index");
    }
}
