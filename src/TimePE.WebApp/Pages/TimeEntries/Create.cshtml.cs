using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.TimeEntries;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IProjectService _projectService;
    private readonly IPayRateService _payRateService;

    public CreateModel(
        ITimeEntryService timeEntryService,
        IProjectService projectService,
        IPayRateService payRateService)
    {
        _timeEntryService = timeEntryService;
        _projectService = projectService;
        _payRateService = payRateService;
    }

    [BindProperty]
    public DateTime Date { get; set; } = DateTime.Today;

    [BindProperty]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [BindProperty]
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

    [BindProperty]
    public int ProjectId { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    public SelectList Projects { get; set; } = new SelectList(Enumerable.Empty<object>());
    public decimal? CurrentPayRate { get; set; }

    public async Task OnGetAsync()
    {
        await LoadProjectsAsync();
        var payRate = await _payRateService.GetCurrentPayRateAsync();
        CurrentPayRate = payRate?.HourlyRate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync();
            return Page();
        }

        try
        {
            await _timeEntryService.CreateTimeEntryAsync(Date, StartTime, EndTime, ProjectId, Notes);
            TempData["SuccessMessage"] = "Time entry created successfully!";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadProjectsAsync();
            return Page();
        }
    }

    private async Task LoadProjectsAsync()
    {
        var projects = await _projectService.GetActiveProjectsAsync();
        Projects = new SelectList(projects, "Oid", "Name");
    }
}
