using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.DTOs;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Projects;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;
    private readonly ICsvService _csvService;

    public IndexModel(IProjectService projectService, ICsvService csvService)
    {
        _projectService = projectService;
        _csvService = csvService;
    }

    public IEnumerable<ProjectSummaryDto> Projects { get; set; } = new List<ProjectSummaryDto>();

    [BindProperty]
    public IFormFile? ImportFile { get; set; }

    public async Task OnGetAsync()
    {
        Projects = await _projectService.GetAllProjectSummariesAsync(includeDeleted: false);
    }

    public async Task<IActionResult> OnPostExportAsync()
    {
        var projects = await _projectService.GetAllProjectsAsync(includeDeleted: false);
        var csvBytes = _csvService.ExportProjectsToCsv(projects);
        
        return File(csvBytes, "text/csv", $"projects-export-{DateTime.Now:yyyy-MM-dd}.csv");
    }

    public IActionResult OnGetDownloadSample()
    {
        var csvBytes = _csvService.GenerateSampleProjectsCsv();
        return File(csvBytes, "text/csv", "projects-sample-import.csv");
    }

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (ImportFile == null || ImportFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a CSV file to import.";
            Projects = await _projectService.GetAllProjectSummariesAsync(includeDeleted: false);
            return Page();
        }

        if (!ImportFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Only CSV files are supported.";
            Projects = await _projectService.GetAllProjectSummariesAsync(includeDeleted: false);
            return Page();
        }

        using var stream = ImportFile.OpenReadStream();
        var result = await _csvService.ImportProjectsFromCsvAsync(stream);

        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToPage();
    }
}
