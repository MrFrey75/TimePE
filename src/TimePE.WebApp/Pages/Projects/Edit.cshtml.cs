using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Projects;

public class EditModel : PageModel
{
    private readonly IProjectService _projectService;

    public EditModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public bool IsActive { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        Id = project.Oid;
        Name = project.Name;
        Description = project.Description;
        IsActive = project.IsActive;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var project = await _projectService.GetProjectByIdAsync(Id);
        if (project == null)
            return NotFound();

        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var editProject = uow.GetObjectByKey<Project>(Id);
        if (editProject != null)
        {
            editProject.Name = Name;
            editProject.Description = Description;
            editProject.IsActive = IsActive;
        }

        await _projectService.UpdateProjectAsync(editProject!);
        TempData["SuccessMessage"] = "Project updated successfully!";
        return RedirectToPage("Index");
    }
}
