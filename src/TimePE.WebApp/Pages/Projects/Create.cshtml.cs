using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;

namespace TimePE.WebApp.Pages.Projects;

public class CreateModel : PageModel
{
    private readonly IProjectService _projectService;

    public CreateModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public bool IsActive { get; set; } = true;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _projectService.CreateProjectAsync(Name, Description);
        TempData["SuccessMessage"] = "Project created successfully!";
        return RedirectToPage("Index");
    }
}
