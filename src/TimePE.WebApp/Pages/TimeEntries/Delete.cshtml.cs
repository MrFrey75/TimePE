using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.TimeEntries;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;

    public DeleteModel(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    public TimeEntryDeleteViewModel? TimeEntry { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        TimeEntry = await _timeEntryService.GetTimeEntryDeleteInfoAsync(id);
        if (TimeEntry == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _timeEntryService.DeleteTimeEntryAsync(id);
        TempData["SuccessMessage"] = "Time entry deleted successfully!";
        return RedirectToPage("Index");
    }
}
