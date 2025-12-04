using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimePE.Core.Services;
using TimePE.Core.Models;
using DevExpress.Xpo;

namespace TimePE.WebApp.Pages.Projects;

[Authorize]
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

    public void OnGet()
    {
        StateProvinceList = new SelectList(Enum.GetValues(typeof(StateProvince)));
        AddressTypeList = new SelectList(Enum.GetValues(typeof(AddressType)));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            StateProvinceList = new SelectList(Enum.GetValues(typeof(StateProvince)));
            AddressTypeList = new SelectList(Enum.GetValues(typeof(AddressType)));
            return Page();
        }

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

        await _projectService.CreateProjectAsync(Name, Description, address);
        TempData["SuccessMessage"] = "Project created successfully!";
        return RedirectToPage("Index");
    }
}
