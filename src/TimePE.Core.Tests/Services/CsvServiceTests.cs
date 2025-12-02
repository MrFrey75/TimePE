using System.Text;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using Moq;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class CsvServiceTests : IDisposable
{
    private readonly CsvService _csvService;
    private readonly Mock<ITimeEntryService> _mockTimeEntryService;
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly IDataLayer _dataLayer;
    private readonly string _connectionString;

    public CsvServiceTests()
    {
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _mockTimeEntryService = new Mock<ITimeEntryService>();
        _mockProjectService = new Mock<IProjectService>();
        _csvService = new CsvService(_mockTimeEntryService.Object, _mockProjectService.Object, _connectionString);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    [Fact]
    public void ExportTimeEntriesToCsv_ShouldGenerateValidCsv_WithCorrectHeaders()
    {
        // Arrange
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;
        
        var project = new Project(uow) { Name = "Test Project" };
        
        var entry = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 1, 15),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            Project = project,
            AppliedPayRate = 50m,
            Notes = "Test work"
        };
        
        var entries = new List<TimeEntry> { entry };

        // Act
        var csvBytes = _csvService.ExportTimeEntriesToCsv(entries);
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csvContent.Should().Contain("Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes");
        csvContent.Should().Contain("2025-01-15");
        csvContent.Should().Contain("Test Project");
        csvContent.Should().Contain("09:00");
        csvContent.Should().Contain("17:00");
        csvContent.Should().Contain("8.00");
        csvContent.Should().Contain("50.00");
        csvContent.Should().Contain("400.00");
        csvContent.Should().Contain("Test work");
    }

    [Fact]
    public void ExportTimeEntriesToCsv_ShouldHandleEmptyList()
    {
        // Arrange
        var entries = new List<TimeEntry>();

        // Act
        var csvBytes = _csvService.ExportTimeEntriesToCsv(entries);
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csvContent.Should().Contain("Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes");
        csvContent.Trim().Split('\n').Should().HaveCount(1); // Only header
    }

    [Fact]
    public void ExportTimeEntriesToCsv_ShouldEscapeFieldsWithCommas()
    {
        // Arrange
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;
        
        var project = new Project(uow) { Name = "Project, Inc." };
        
        var entry = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 1, 15),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Project = project,
            AppliedPayRate = 50m,
            Notes = "Work on feature A, B, and C"
        };
        
        var entries = new List<TimeEntry> { entry };

        // Act
        var csvBytes = _csvService.ExportTimeEntriesToCsv(entries);
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csvContent.Should().Contain("\"Project, Inc.\"");
        csvContent.Should().Contain("\"Work on feature A, B, and C\"");
    }

    [Fact]
    public void ExportTimeEntriesToCsv_ShouldHandleNullNotes()
    {
        // Arrange
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;
        
        var project = new Project(uow) { Name = "Test Project" };
        
        var entry = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 1, 15),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Project = project,
            AppliedPayRate = 50m,
            Notes = null
        };
        
        var entries = new List<TimeEntry> { entry };

        // Act
        var csvBytes = _csvService.ExportTimeEntriesToCsv(entries);
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csvContent.Should().NotBeNull();
        var lines = csvContent.Trim().Split('\n');
        lines.Should().HaveCount(2); // Header + 1 data row
    }

    [Fact]
    public void ExportTimeEntriesToCsv_ShouldOrderByDate()
    {
        // Arrange
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;
        
        var project = new Project(uow) { Name = "Test Project" };
        
        var entry1 = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 1, 20),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Project = project,
            AppliedPayRate = 50m
        };
        
        var entry2 = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 1, 10),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Project = project,
            AppliedPayRate = 50m
        };
        
        var entry3 = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 1, 15),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0),
            Project = project,
            AppliedPayRate = 50m
        };
        
        var entries = new List<TimeEntry> { entry1, entry2, entry3 };

        // Act
        var csvBytes = _csvService.ExportTimeEntriesToCsv(entries);
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        var lines = csvContent.Trim().Split('\n');
        lines[1].Should().StartWith("2025-01-10");
        lines[2].Should().StartWith("2025-01-15");
        lines[3].Should().StartWith("2025-01-20");
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldImportValidEntries()
    {
        // Arrange
        var csvContent = @"Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes
2025-01-15,Test Project,09:00,17:00,8.00,50.00,400.00,Test notes";
        
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        
        var project = new Project(new UnitOfWork()) { Oid = 1, Name = "Test Project" };
        _mockProjectService.Setup(x => x.GetActiveProjectsAsync())
            .ReturnsAsync(new List<Project> { project });

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(1);
        _mockTimeEntryService.Verify(x => x.CreateTimeEntryAsync(
            new DateTime(2025, 1, 15),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            1,
            "Test notes"), Times.Once);
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldCreateNewProject_WhenNotFound()
    {
        // Arrange
        var csvContent = @"Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes
2025-01-15,New Project,09:00,17:00,8.00,50.00,400.00,";
        
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        
        var newProject = new Project(new UnitOfWork()) { Oid = 2, Name = "New Project" };
        _mockProjectService.Setup(x => x.GetActiveProjectsAsync())
            .ReturnsAsync(new List<Project>());
        _mockProjectService.Setup(x => x.CreateProjectAsync("New Project", "Auto-created from CSV import"))
            .ReturnsAsync(newProject);

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(1);
        _mockProjectService.Verify(x => x.CreateProjectAsync("New Project", "Auto-created from CSV import"), Times.Once);
        _mockTimeEntryService.Verify(x => x.CreateTimeEntryAsync(
            new DateTime(2025, 1, 15),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            2,
            ""), Times.Once);
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldReturnError_WhenCsvIsEmpty()
    {
        // Arrange
        var csvContent = "";
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("empty");
        result.ImportedCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldReturnError_WhenOnlyHeaderPresent()
    {
        // Arrange
        var csvContent = "Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes";
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("empty");
        result.ImportedCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldSkipInvalidRows_AndReportErrors()
    {
        // Arrange
        var csvContent = @"Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes
invalid-date,Test Project,09:00,17:00,8.00,50.00,400.00,Bad date
2025-01-15,Test Project,09:00,17:00,8.00,50.00,400.00,Good row";
        
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        
        var project = new Project(new UnitOfWork()) { Oid = 1, Name = "Test Project" };
        _mockProjectService.Setup(x => x.GetActiveProjectsAsync())
            .ReturnsAsync(new List<Project> { project });

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(1); // Only the valid row
        result.Message.Should().Contain("1 errors occurred");
        result.Message.Should().Contain("Invalid date format");
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldValidateRequiredFields()
    {
        // Arrange - Missing project name
        var csvContent = @"Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes
2025-01-15,,09:00,17:00,8.00,50.00,400.00,Missing project";
        
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(0);
        result.Message.Should().Contain("errors occurred");
        result.Message.Should().Contain("Project name is required");
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldValidateTimeFormat()
    {
        // Arrange
        var csvContent = @"Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes
2025-01-15,Test Project,invalid,17:00,8.00,50.00,400.00,Bad start time
2025-01-15,Test Project,09:00,invalid,8.00,50.00,400.00,Bad end time";
        
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(0);
        result.Message.Should().Contain("errors occurred");
        result.Message.Should().Contain("Invalid");
        result.Message.Should().Contain("time format");
    }

    [Fact]
    public async Task ImportTimeEntriesFromCsvAsync_ShouldHandleMultipleValidRows()
    {
        // Arrange
        var csvContent = @"Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes
2025-01-15,Project A,09:00,17:00,8.00,50.00,400.00,First entry
2025-01-16,Project B,10:00,15:00,5.00,60.00,300.00,Second entry
2025-01-17,Project A,08:00,16:00,8.00,50.00,400.00,Third entry";
        
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        
        var projectA = new Project(new UnitOfWork()) { Oid = 1, Name = "Project A" };
        var projectB = new Project(new UnitOfWork()) { Oid = 2, Name = "Project B" };
        _mockProjectService.Setup(x => x.GetActiveProjectsAsync())
            .ReturnsAsync(new List<Project> { projectA, projectB });

        // Act
        var result = await _csvService.ImportTimeEntriesFromCsvAsync(csvStream, _connectionString);

        // Assert
        result.Success.Should().BeTrue();
        result.ImportedCount.Should().Be(3);
        _mockTimeEntryService.Verify(x => x.CreateTimeEntryAsync(
            It.IsAny<DateTime>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<int>(),
            It.IsAny<string>()), Times.Exactly(3));
    }

    [Fact]
    public void GenerateSampleCsv_ShouldReturnValidCsvWithSampleData()
    {
        // Act
        var csvBytes = _csvService.GenerateSampleCsv();
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csvContent.Should().Contain("Date,Project,Start Time,End Time,Duration (Hours),Pay Rate,Amount Owed,Notes");
        csvContent.Should().Contain("Sample Project");
        csvContent.Should().Contain("Another Project");
        csvContent.Should().Contain("2025-12-01");
        csvContent.Should().Contain("IMPORT INSTRUCTIONS");
        csvContent.Should().Contain("Date format: YYYY-MM-DD");
    }

    [Fact]
    public void GenerateSampleCsv_ShouldIncludeInstructions()
    {
        // Act
        var csvBytes = _csvService.GenerateSampleCsv();
        var csvContent = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csvContent.Should().Contain("# IMPORT INSTRUCTIONS:");
        csvContent.Should().Contain("# - Date format: YYYY-MM-DD");
        csvContent.Should().Contain("# - Time format: HH:MM (24-hour)");
        csvContent.Should().Contain("# - Projects will be auto-created if they don't exist");
    }
}
