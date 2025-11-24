using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.DTOs;

namespace TimePE.WebApp.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;

    public IndexModel(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public IEnumerable<ProjectSummaryDto> Projects { get; set; } = new List<ProjectSummaryDto>();

    public async Task OnGetAsync()
    {
        Projects = await _projectService.GetAllProjectSummariesAsync(includeDeleted: false);
    }
}
