using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Projects;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IProjectService _projectService;

    public DeleteModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public Project? Project { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Project = await _projectService.GetProjectByIdAsync(id);
        if (Project == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _projectService.DeleteProjectAsync(id);
        TempData["SuccessMessage"] = "Project deleted successfully!";
        return RedirectToPage("Index");
    }
}
