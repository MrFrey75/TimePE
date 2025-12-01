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
        var timeEntry = await _timeEntryService.GetTimeEntryByIdAsync(id);
        if (timeEntry == null)
            return NotFound();

        // Load related data before the session is disposed
        var projectName = timeEntry.Project?.Name;

        // Create view model with the data we need
        TimeEntry = new TimeEntryDeleteViewModel
        {
            Id = timeEntry.Oid,
            Date = timeEntry.Date,
            ProjectName = projectName,
            StartTime = timeEntry.StartTime,
            EndTime = timeEntry.EndTime,
            Duration = timeEntry.Duration,
            AmountOwed = timeEntry.AmountOwed,
            Notes = timeEntry.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _timeEntryService.DeleteTimeEntryAsync(id);
        TempData["SuccessMessage"] = "Time entry deleted successfully!";
        return RedirectToPage("Index");
    }

    public class TimeEntryDeleteViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? ProjectName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal AmountOwed { get; set; }
        public string? Notes { get; set; }
    }
}
