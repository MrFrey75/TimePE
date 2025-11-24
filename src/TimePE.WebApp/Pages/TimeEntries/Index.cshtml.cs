using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.TimeEntries;

public class IndexModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;

    public IndexModel(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
    {
        EndDate = endDate ?? DateTime.Today;
        StartDate = startDate ?? EndDate.AddDays(-30);

        TimeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(StartDate, EndDate);
    }
}
