using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimePE.Core.Services;
using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.Projects;

[Authorize]
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

    [BindProperty]
    public string? StreetLine1 { get; set; }

    [BindProperty]
    public string? StreetLine2 { get; set; }

    [BindProperty]
    public string? City { get; set; }

    [BindProperty]
    public StateProvince? StateProvince { get; set; }

    [BindProperty]
    public string? PostalCode { get; set; }

    [BindProperty]
    public AddressType AddressType { get; set; } = AddressType.Commercial;

    public SelectList? StateProvinceList { get; set; }
    public SelectList? AddressTypeList { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
            return NotFound();

        Id = project.Oid;
        Name = project.Name;
        Description = project.Description;
        IsActive = project.IsActive;

        if (project.Address != null)
        {
            StreetLine1 = project.Address.StreetLine1;
            StreetLine2 = project.Address.StreetLine2;
            City = project.Address.City;
            StateProvince = project.Address.StateProvince;
            PostalCode = project.Address.PostalCode;
            AddressType = project.Address.AddressType;
        }

        StateProvinceList = new SelectList(Enum.GetValues(typeof(StateProvince)));
        AddressTypeList = new SelectList(Enum.GetValues(typeof(AddressType)));

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            StateProvinceList = new SelectList(Enum.GetValues(typeof(StateProvince)));
            AddressTypeList = new SelectList(Enum.GetValues(typeof(AddressType)));
            return Page();
        }

        var project = await _projectService.GetProjectByIdAsync(Id);
        if (project == null)
            return NotFound();

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(StreetLine1) || !string.IsNullOrWhiteSpace(City))
        {
            using var session = new Session(XpoDefault.DataLayer);
            address = new Address(session)
            {
                StreetLine1 = StreetLine1 ?? string.Empty,
                StreetLine2 = StreetLine2 ?? string.Empty,
                City = City ?? string.Empty,
                StateProvince = StateProvince ?? TimePE.Core.Models.StateProvince.Unknown,
                PostalCode = PostalCode ?? string.Empty,
                AddressType = AddressType
            };
        }

        await _projectService.UpdateProjectWithAddressAsync(Id, Name, Description, IsActive, address);
        TempData["SuccessMessage"] = "Project updated successfully!";
        return RedirectToPage("Index");
    }
}
