using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimePE.Core.Services;
using TimePE.Core.Models;
using DevExpress.Xpo;

namespace TimePE.WebApp.Pages.TimeEntries;

public class EditModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IProjectService _projectService;

    public EditModel(ITimeEntryService timeEntryService, IProjectService projectService)
    {
        _timeEntryService = timeEntryService;
        _projectService = projectService;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public DateTime Date { get; set; }

    [BindProperty]
    public TimeSpan StartTime { get; set; }

    [BindProperty]
    public TimeSpan EndTime { get; set; }

    [BindProperty]
    public int ProjectId { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    public SelectList Projects { get; set; } = new SelectList(Enumerable.Empty<object>());
    public decimal AppliedPayRate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var entry = await _timeEntryService.GetTimeEntryByIdAsync(id);
        if (entry == null)
            return NotFound();

        Id = entry.Oid;
        Date = entry.Date;
        StartTime = entry.StartTime;
        EndTime = entry.EndTime;
        ProjectId = entry.Project?.Oid ?? 0;
        Notes = entry.Notes;
        AppliedPayRate = entry.AppliedPayRate;

        await LoadProjectsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync();
            return Page();
        }

        var entry = await _timeEntryService.GetTimeEntryByIdAsync(Id);
        if (entry == null)
            return NotFound();

        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var editEntry = uow.GetObjectByKey<TimeEntry>(Id);
        if (editEntry != null)
        {
            editEntry.Date = Date;
            editEntry.StartTime = StartTime;
            editEntry.EndTime = EndTime;
            editEntry.Notes = Notes;
        }

        await _timeEntryService.UpdateTimeEntryAsync(editEntry!);

        TempData["SuccessMessage"] = "Time entry updated successfully!";
        return RedirectToPage("Index");
    }

    private async Task LoadProjectsAsync()
    {
        var projects = await _projectService.GetActiveProjectsAsync();
        Projects = new SelectList(projects, "Oid", "Name");
    }
}
