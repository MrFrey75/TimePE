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
        // We need to load the data inside a service method to avoid disposed object issues
        var deleteInfo = await _projectService.GetProjectDeleteInfoAsync(id);
        if (deleteInfo == null)
            return NotFound();

        Project = deleteInfo;
        return Page()
;
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        await _projectService.DeleteProjectAsync(id);
        TempData["SuccessMessage"] = "Project deleted successfully!";
        return RedirectToPage("Index");
    }
}
