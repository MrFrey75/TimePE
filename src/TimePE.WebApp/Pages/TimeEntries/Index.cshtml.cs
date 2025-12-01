using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimePE.Core.Services;
using TimePE.Core.Models;

namespace TimePE.WebApp.Pages.TimeEntries;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ICsvService _csvService;

    public IndexModel(ITimeEntryService timeEntryService, ICsvService csvService)
    {
        _timeEntryService = timeEntryService;
        _csvService = csvService;
    }

    public IEnumerable<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [BindProperty]
    public IFormFile? CsvFile { get; set; }

    public string? ImportMessage { get; set; }
    public bool ImportSuccess { get; set; }

    public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
    {
        EndDate = endDate ?? DateTime.Today;
        StartDate = startDate ?? EndDate.AddDays(-30);

        TimeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(StartDate, EndDate);
    }

    public async Task<IActionResult> OnPostExportAsync(DateTime? startDate, DateTime? endDate)
    {
        EndDate = endDate ?? DateTime.Today;
        StartDate = startDate ?? EndDate.AddDays(-30);

        var entries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(StartDate, EndDate);
        var csvData = _csvService.ExportTimeEntriesToCsv(entries);

        var fileName = $"TimeEntries_{StartDate:yyyy-MM-dd}_to_{EndDate:yyyy-MM-dd}.csv";
        return File(csvData, "text/csv", fileName);
    }

    public IActionResult OnGetDownloadSample()
    {
        var sampleCsv = _csvService.GenerateSampleCsv();
        return File(sampleCsv, "text/csv", "TimeEntries_Sample.csv");
    }

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (CsvFile == null || CsvFile.Length == 0)
        {
            ImportSuccess = false;
            ImportMessage = "Please select a CSV file to import";
            
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
            TimeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(StartDate, EndDate);
            return Page();
        }

        using var stream = CsvFile.OpenReadStream();
        var connectionString = HttpContext.RequestServices.GetRequiredService<string>();
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(stream, connectionString);

        ImportSuccess = result.Success;
        ImportMessage = result.Message;

        if (result.Success)
        {
            TempData["SuccessMessage"] = ImportMessage;
            return RedirectToPage();
        }

        EndDate = DateTime.Today;
        StartDate = EndDate.AddDays(-30);
        TimeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(StartDate, EndDate);
        return Page();
    }
}
