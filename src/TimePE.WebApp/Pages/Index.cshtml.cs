using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDashboardService _dashboardService;
    private readonly ITimeEntryService _timeEntryService;

    public IndexModel(
        ILogger<IndexModel> logger, 
        IDashboardService dashboardService,
        ITimeEntryService timeEntryService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
        _timeEntryService = timeEntryService;
    }

    public BalanceSummary? BalanceSummary { get; set; }
    public IEnumerable<TimeEntry> RecentTimeEntries { get; set; } = new List<TimeEntry>();
    public Dictionary<string, decimal> ProjectHours { get; set; } = new Dictionary<string, decimal>();
    public decimal ThisWeekHours { get; set; }
    public decimal LastWeekHours { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            BalanceSummary = await _dashboardService.GetBalanceSummaryAsync();
            RecentTimeEntries = await _dashboardService.GetRecentTimeEntriesAsync(10);
            
            var thisWeekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var lastWeekStart = thisWeekStart.AddDays(-7);
            
            ThisWeekHours = await _dashboardService.GetWeeklyHoursAsync(thisWeekStart);
            LastWeekHours = await _dashboardService.GetWeeklyHoursAsync(lastWeekStart);
            
            var thirtyDaysAgo = DateTime.Today.AddDays(-30);
            ProjectHours = await _dashboardService.GetProjectHoursSummaryAsync(thirtyDaysAgo, DateTime.Today);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
        }
    }
}
