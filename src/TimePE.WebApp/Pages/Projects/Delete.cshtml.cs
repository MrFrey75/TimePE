using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.Projects;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IProjectService _projectService;

    public DeleteModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public ProjectDeleteViewModel? Project { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        // Load the count before the session is disposed
        var timeEntriesCount = project.TimeEntries?.Count ?? 0;

        // Create view model with the data we need
        Project = new ProjectDeleteViewModel
        {
            Id = project.Oid,
            Name = project.Name,
            Description = project.Description,
            IsActive = project.IsActive,
            TimeEntriesCount = timeEntriesCount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _projectService.DeleteProjectAsync(id);
        TempData["SuccessMessage"] = "Project deleted successfully!";
        return RedirectToPage("Index");
    }

    public class ProjectDeleteViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int TimeEntriesCount { get; set; }
    }
}
