using System.Globalization;
using System.Text;
using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public interface ICsvService
{
    byte[] ExportTimeEntriesToCsv(IEnumerable<TimeEntry> entries);
    Task<(bool Success, string Message, int ImportedCount)> ImportTimeEntriesFromCsvAsync(Stream csvStream, string connectionString);
    byte[] GenerateSampleCsv();
    byte[] ExportProjectsToCsv(IEnumerable<Project> projects);
    Task<(bool Success, string Message, int ImportedCount)> ImportProjectsFromCsvAsync(Stream csvStream);
    byte[] GenerateSampleProjectsCsv();
}

public class CsvService : ICsvService
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly IProjectService _projectService;
    private readonly string _connectionString;

    public CsvService(ITimeEntryService timeEntryService, IProjectService projectService, string connectionString)
    {
        _timeEntryService = timeEntryService;
        _projectService = projectService;
        _connectionString = connectionString;
    }

    public byte[] ExportTimeEntriesToCsv(IEnumerable<TimeEntry> entries)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes");

        // Data rows
        foreach (var entry in entries.OrderBy(e => e.Date))
        {
            var duration = entry.Duration.TotalHours.ToString("F2", CultureInfo.InvariantCulture);
            var payRate = entry.AppliedPayRate.ToString("F2", CultureInfo.InvariantCulture);
            var amount = entry.AmountOwed.ToString("F2", CultureInfo.InvariantCulture);
            var notes = EscapeCsvField(entry.Notes ?? "");
            
            csv.AppendLine($"{entry.Date:yyyy-MM-dd},{EscapeCsvField(entry.Project?.Name ?? "")},{entry.StartTime:hh\\:mm},{entry.EndTime:hh\\:mm},{duration},{payRate},{amount},{notes}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<(bool Success, string Message, int ImportedCount)> ImportTimeEntriesFromCsvAsync(Stream csvStream, string connectionString)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            var lines = new List<string>();
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }

            if (lines.Count < 2)
            {
                return (false, "CSV file is empty or has no data rows", 0);
            }

            // Skip header
            var dataLines = lines.Skip(1).ToList();
            var importedCount = 0;
            var errors = new List<string>();

            foreach (var line in dataLines)
            {
                try
                {
                    var fields = ParseCsvLine(line);
                    
                    if (fields.Length < 4)
                    {
                        errors.Add($"Invalid row (not enough fields): {line}");
                        continue;
                    }

                    // Parse fields
                    if (!DateTime.TryParse(fields[0], out var date))
                    {
                        errors.Add($"Invalid date format: {fields[0]}");
                        continue;
                    }

                    var projectName = fields[1].Trim();
                    if (string.IsNullOrEmpty(projectName))
                    {
                        errors.Add($"Project name is required for row: {line}");
                        continue;
                    }

                    if (!TimeSpan.TryParse(fields[2], out var startTime))
                    {
                        errors.Add($"Invalid start time format: {fields[2]}");
                        continue;
                    }

                    if (!TimeSpan.TryParse(fields[3], out var endTime))
                    {
                        errors.Add($"Invalid end time format: {fields[3]}");
                        continue;
                    }

                    var notes = fields.Length > 7 ? fields[7].Trim() : "";

                    // Find or create project
                    var projects = await _projectService.GetActiveProjectsAsync();
                    var project = projects.FirstOrDefault(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase));
                    
                    if (project == null)
                    {
                        // Create new project
                        project = await _projectService.CreateProjectAsync(projectName, $"Auto-created from CSV import");
                    }

                    // Create time entry
                    await _timeEntryService.CreateTimeEntryAsync(date, startTime, endTime, project.Oid, notes);
                    importedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing row: {line} - {ex.Message}");
                }
            }

            var message = $"Successfully imported {importedCount} time entries";
            if (errors.Any())
            {
                message += $". {errors.Count} errors occurred: " + string.Join("; ", errors.Take(5));
                if (errors.Count > 5)
                {
                    message += $" (and {errors.Count - 5} more...)";
                }
            }

            return (true, message, importedCount);
        }
        catch (Exception ex)
        {
            return (false, $"Error reading CSV file: {ex.Message}", 0);
        }
    }

    public byte[] GenerateSampleCsv()
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes");
        
        // Sample rows
        csv.AppendLine("2025-12-01,Sample Project,09:00,17:00,8.00,50.00,400.00,Regular work day");
        csv.AppendLine("2025-12-02,Another Project,10:00,15:30,5.50,50.00,275.00,Half day");
        csv.AppendLine("2025-12-03,Sample Project,09:30,18:00,8.50,50.00,425.00,");

        // Instructions as comments
        csv.AppendLine();
        csv.AppendLine("# IMPORT INSTRUCTIONS:");
        csv.AppendLine("# - Date format: YYYY-MM-DD");
        csv.AppendLine("# - Time format: HH:MM (24-hour)");
        csv.AppendLine("# - Duration, Pay Rate, and Amount Owed are auto-calculated (you can leave them as shown)");
        csv.AppendLine("# - Projects will be auto-created if they don't exist");
        csv.AppendLine("# - Notes are optional");
        csv.AppendLine("# - Delete these instruction lines and the sample data before importing your own data");

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    public byte[] ExportProjectsToCsv(IEnumerable<Project> projects)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Name,Description,IsActive,StreetLine1,StreetLine2,City,State,PostalCode,AddressType,CreatedAt");

        // Data rows
        foreach (var project in projects.OrderBy(p => p.Name))
        {
            var name = EscapeCsvField(project.Name);
            var description = EscapeCsvField(project.Description ?? "");
            var isActive = project.IsActive ? "Yes" : "No";
            var streetLine1 = EscapeCsvField(project.Address?.StreetLine1 ?? "");
            var streetLine2 = EscapeCsvField(project.Address?.StreetLine2 ?? "");
            var city = EscapeCsvField(project.Address?.City ?? "");
            var state = project.Address?.StateProvince.ToString() ?? "";
            var postalCode = EscapeCsvField(project.Address?.PostalCode ?? "");
            var addressType = project.Address?.AddressType.ToString() ?? "";
            var createdAt = project.CreatedAt.ToString("yyyy-MM-dd");
            
            csv.AppendLine($"{name},{description},{isActive},{streetLine1},{streetLine2},{city},{state},{postalCode},{addressType},{createdAt}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<(bool Success, string Message, int ImportedCount)> ImportProjectsFromCsvAsync(Stream csvStream)
    {
        try
        {
            using var reader = new StreamReader(csvStream);
            var lines = new List<string>();
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }

            if (lines.Count < 2)
            {
                return (false, "CSV file is empty or has no data rows", 0);
            }

            // Skip header
            var dataLines = lines.Skip(1).ToList();
            var importedCount = 0;
            var errors = new List<string>();

            foreach (var line in dataLines)
            {
                try
                {
                    var fields = ParseCsvLine(line);
                    
                    if (fields.Length < 1)
                    {
                        errors.Add($"Invalid row (no project name): {line}");
                        continue;
                    }

                    // Parse fields
                    var name = fields[0].Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        errors.Add($"Project name is required for row: {line}");
                        continue;
                    }

                    var description = fields.Length > 1 ? fields[1].Trim() : null;
                    var isActive = fields.Length > 2 && fields[2].Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase);

                    Address? address = null;
                    if (fields.Length > 3)
                    {
                        var streetLine1 = fields.Length > 3 ? fields[3].Trim() : "";
                        var streetLine2 = fields.Length > 4 ? fields[4].Trim() : "";
                        var city = fields.Length > 5 ? fields[5].Trim() : "";
                        var stateStr = fields.Length > 6 ? fields[6].Trim() : "";
                        var postalCode = fields.Length > 7 ? fields[7].Trim() : "";
                        var addressTypeStr = fields.Length > 8 ? fields[8].Trim() : "";

                        if (!string.IsNullOrWhiteSpace(streetLine1) || !string.IsNullOrWhiteSpace(city))
                        {
                            using var session = new Session(XpoDefault.DataLayer);
                            
                            var stateProvince = StateProvince.Unknown;
                            if (!string.IsNullOrEmpty(stateStr) && Enum.TryParse<StateProvince>(stateStr, out var parsedState))
                            {
                                stateProvince = parsedState;
                            }

                            var addressType = AddressType.Commercial;
                            if (!string.IsNullOrEmpty(addressTypeStr) && Enum.TryParse<AddressType>(addressTypeStr, out var parsedType))
                            {
                                addressType = parsedType;
                            }

                            address = new Address(session)
                            {
                                StreetLine1 = streetLine1,
                                StreetLine2 = streetLine2,
                                City = city,
                                StateProvince = stateProvince,
                                PostalCode = postalCode,
                                AddressType = addressType
                            };
                        }
                    }

                    // Check if project already exists
                    var existingProjects = await _projectService.GetAllProjectsAsync();
                    var existingProject = existingProjects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    
                    if (existingProject == null)
                    {
                        // Create new project
                        await _projectService.CreateProjectAsync(name, description, address);
                        importedCount++;
                    }
                    else
                    {
                        errors.Add($"Project '{name}' already exists, skipping");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing row: {line} - {ex.Message}");
                }
            }

            var message = $"Successfully imported {importedCount} projects";
            if (errors.Any())
            {
                message += $". {errors.Count} errors/skips occurred: " + string.Join("; ", errors.Take(5));
                if (errors.Count > 5)
                {
                    message += $" (and {errors.Count - 5} more...)";
                }
            }

            return (true, message, importedCount);
        }
        catch (Exception ex)
        {
            return (false, $"Error reading CSV file: {ex.Message}", 0);
        }
    }

    public byte[] GenerateSampleProjectsCsv()
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Name,Description,IsActive,StreetLine1,StreetLine2,City,State,PostalCode,AddressType,CreatedAt");
        
        // Sample rows
        csv.AppendLine("\"Office Renovation\",\"Main office building renovation project\",Yes,\"123 Main St\",\"Suite 100\",\"Springfield\",IL,62701,Commercial,2025-01-01");
        csv.AppendLine("\"Home Office\",\"Remote work setup\",Yes,\"456 Oak Ave\",,\"Chicago\",IL,60601,Residential,2025-01-15");
        csv.AppendLine("\"Client Site - ABC Corp\",\"On-site client work\",Yes,\"789 Business Pkwy\",\"Building A\",\"Naperville\",IL,60540,Commercial,2025-02-01");

        // Instructions as comments
        csv.AppendLine();
        csv.AppendLine("# IMPORT INSTRUCTIONS:");
        csv.AppendLine("# - Name is required");
        csv.AppendLine("# - Description is optional");
        csv.AppendLine("# - IsActive: Yes or No (default: No)");
        csv.AppendLine("# - Address fields are all optional");
        csv.AppendLine("# - State: Use 2-letter abbreviation (e.g., IL, CA, NY)");
        csv.AppendLine("# - AddressType: Residential, Commercial, or Rental");
        csv.AppendLine("# - Duplicate project names will be skipped");
        csv.AppendLine("# - Delete these instruction lines and the sample data before importing your own data");

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
