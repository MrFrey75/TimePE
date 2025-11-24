using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IDashboardService _dashboardService;
    private readonly IIncidentalService _incidentalService;
    private readonly IPaymentService _paymentService;

    public IndexModel(
        ITimeEntryService timeEntryService,
        IDashboardService dashboardService,
        IIncidentalService incidentalService,
        IPaymentService paymentService)
    {
        _timeEntryService = timeEntryService;
        _dashboardService = dashboardService;
        _incidentalService = incidentalService;
        _paymentService = paymentService;
    }

    [BindProperty]
    public DateTime StartDate { get; set; }

    [BindProperty]
    public DateTime EndDate { get; set; }

    public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public IEnumerable<Incidental> Incidentals { get; set; } = new List<Incidental>();
    public IEnumerable<Payment> Payments { get; set; } = new List<Payment>();
    public Dictionary<string, decimal> ProjectHours { get; set; } = new Dictionary<string, decimal>();
    
    public decimal TotalHours { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalIncidentalsOwed { get; set; }
    public decimal TotalIncidentalsOwedBy { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal NetBalance { get; set; }

    public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
    {
        // Default to current week
        var today = DateTime.Today;
        var dayOfWeek = (int)today.DayOfWeek;
        StartDate = startDate ?? today.AddDays(-dayOfWeek);
        EndDate = endDate ?? StartDate.AddDays(6);

        await LoadReportDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadReportDataAsync();
        return Page();
    }

    private async Task LoadReportDataAsync()
    {
        TimeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(StartDate, EndDate);
        Incidentals = await _incidentalService.GetIncidentalsByDateRangeAsync(StartDate, EndDate);
        Payments = await _paymentService.GetPaymentsByDateRangeAsync(StartDate, EndDate);
        ProjectHours = await _dashboardService.GetProjectHoursSummaryAsync(StartDate, EndDate);

        TotalHours = (decimal)TimeEntries.Sum(te => te.Duration.TotalHours);
        TotalEarned = TimeEntries.Sum(te => te.AmountOwed);
        TotalIncidentalsOwed = Incidentals.Where(i => i.Type == IncidentalType.Owed).Sum(i => i.Amount);
        TotalIncidentalsOwedBy = Incidentals.Where(i => i.Type == IncidentalType.OwedBy).Sum(i => i.Amount);
        TotalPaid = Payments.Sum(p => p.Amount);
        NetBalance = TotalEarned + TotalIncidentalsOwed - TotalIncidentalsOwedBy - TotalPaid;
    }
}
