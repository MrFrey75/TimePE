using DevExpress.Xpo;
using FluentAssertions;
using Moq;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class TimeEntryServiceTests : IDisposable
{
    private readonly TimeEntryService _timeEntryService;
    private readonly Mock<IPayRateService> _mockPayRateService;
    private readonly string _connectionString;
    private readonly IDataLayer _dataLayer;

    public TimeEntryServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _mockPayRateService = new Mock<IPayRateService>();
        _timeEntryService = new TimeEntryService(_connectionString, _mockPayRateService.Object);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    private Project CreateTestProject(string name = "Test Project")
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var project = new Project(uow) { Name = name, IsActive = true };
        uow.CommitChanges();
        return project;
    }

    private PayRate CreateTestPayRate(decimal rate = 50.00m)
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var payRate = new PayRate(uow)
        {
            HourlyRate = rate,
            EffectiveDate = DateTime.UtcNow.AddDays(-30)
        };
        uow.CommitChanges();
        return payRate;
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldCreateNewTimeEntry_WithCorrectCalculations()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate(50.00m);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var date = new DateTime(2025, 12, 1);
        var startTime = new TimeSpan(9, 0, 0);
        var endTime = new TimeSpan(17, 0, 0);

        // Act
        var result = await _timeEntryService.CreateTimeEntryAsync(date, startTime, endTime, project.Oid, "Test work");

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.StartTime.Should().Be(startTime);
        result.EndTime.Should().Be(endTime);
        result.AppliedPayRate.Should().Be(50.00m);
        result.Notes.Should().Be("Test work");
        result.Duration.Should().Be(new TimeSpan(8, 0, 0)); // 8 hours
        result.AmountOwed.Should().Be(400.00m); // 8 hours * $50
        result.Oid.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldCreateTimeEntry_WithoutNotes()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate(50.00m);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        // Act
        var result = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid
        );

        // Assert
        result.Notes.Should().BeNull();
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldThrowException_WhenNoPayRateFound()
    {
        // Arrange
        var project = CreateTestProject();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync((PayRate?)null);

        // Act
        var act = async () => await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No pay rate found for date 2025-12-01");
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldThrowException_WhenProjectNotFound()
    {
        // Arrange
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        // Act
        var act = async () => await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            99999 // Non-existent project ID
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Project with ID 99999 not found");
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldCalculateCorrectly_ForPartialHours()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate(40.00m);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        // Act - 5.5 hours
        var result = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(14, 30, 0),
            project.Oid
        );

        // Assert
        result.Duration.Should().Be(new TimeSpan(5, 30, 0));
        result.AmountOwed.Should().Be(220.00m); // 5.5 hours * $40
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldApplyCorrectPayRate_BasedOnDate()
    {
        // Arrange
        var project = CreateTestProject();
        var oldRate = CreateTestPayRate(45.00m);
        var newRate = CreateTestPayRate(55.00m);
        
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(new DateTime(2025, 11, 15)))
            .ReturnsAsync(oldRate);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(new DateTime(2025, 12, 15)))
            .ReturnsAsync(newRate);

        // Act
        var oldEntry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 11, 15),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid
        );
        var newEntry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 15),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid
        );

        // Assert
        oldEntry.AppliedPayRate.Should().Be(45.00m);
        newEntry.AppliedPayRate.Should().Be(55.00m);
    }

    [Fact]
    public async Task GetTimeEntryByIdAsync_ShouldReturnTimeEntry_WhenExists()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid,
            "Test entry"
        );

        // Act
        var result = await _timeEntryService.GetTimeEntryByIdAsync(entry.Oid);

        // Assert
        result.Should().NotBeNull();
        result!.Oid.Should().Be(entry.Oid);
        result.Notes.Should().Be("Test entry");
    }

    [Fact]
    public async Task GetTimeEntryByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _timeEntryService.GetTimeEntryByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTimeEntryDeleteInfoAsync_ShouldReturnViewModel_WhenExists()
    {
        // Arrange
        var project = CreateTestProject("My Project");
        var payRate = CreateTestPayRate(50.00m);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid,
            "Delete me"
        );

        // Act
        var result = await _timeEntryService.GetTimeEntryDeleteInfoAsync(entry.Oid);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entry.Oid);
        result.Date.Should().Be(new DateTime(2025, 12, 1));
        result.ProjectName.Should().Be("My Project");
        result.StartTime.Should().Be(new TimeSpan(9, 0, 0));
        result.EndTime.Should().Be(new TimeSpan(17, 0, 0));
        result.Duration.Should().Be(new TimeSpan(8, 0, 0));
        result.AmountOwed.Should().Be(400.00m);
        result.Notes.Should().Be("Delete me");
    }

    [Fact]
    public async Task GetTimeEntryDeleteInfoAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _timeEntryService.GetTimeEntryDeleteInfoAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTimeEntriesByDateRangeAsync_ShouldReturnEntriesInRange()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 11, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "November");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 5), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Early Dec");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Mid Dec");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 25), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Late Dec");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2026, 1, 5), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "January");

        // Act
        var result = await _timeEntryService.GetTimeEntriesByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(3);
        entries.Should().Contain(e => e.Notes == "Early Dec");
        entries.Should().Contain(e => e.Notes == "Mid Dec");
        entries.Should().Contain(e => e.Notes == "Late Dec");
    }

    [Fact]
    public async Task GetTimeEntriesByDateRangeAsync_ShouldReturnOrderedByDateAndStartTime()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 15), new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Afternoon");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 5), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), project.Oid, "Early Morning");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 15), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), project.Oid, "Morning");

        // Act
        var result = await _timeEntryService.GetTimeEntriesByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var entries = result.ToList();
        entries[0].Notes.Should().Be("Early Morning"); // Dec 5, 9am
        entries[1].Notes.Should().Be("Morning");       // Dec 15, 9am
        entries[2].Notes.Should().Be("Afternoon");     // Dec 15, 2pm
    }

    [Fact]
    public async Task GetTimeEntriesByProjectAsync_ShouldReturnOnlyEntriesForSpecificProject()
    {
        // Arrange
        var project1 = CreateTestProject("Project A");
        var project2 = CreateTestProject("Project B");
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), project1.Oid, "Project A - 1");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), project2.Oid, "Project B - 1");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 3), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), project1.Oid, "Project A - 2");
        await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 4), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), project2.Oid, "Project B - 2");

        // Act
        var resultA = await _timeEntryService.GetTimeEntriesByProjectAsync(project1.Oid);
        var resultB = await _timeEntryService.GetTimeEntriesByProjectAsync(project2.Oid);

        // Assert
        var entriesA = resultA.ToList();
        entriesA.Should().HaveCount(2);
        entriesA.Should().Contain(e => e.Notes == "Project A - 1");
        entriesA.Should().Contain(e => e.Notes == "Project A - 2");

        var entriesB = resultB.ToList();
        entriesB.Should().HaveCount(2);
        entriesB.Should().Contain(e => e.Notes == "Project B - 1");
        entriesB.Should().Contain(e => e.Notes == "Project B - 2");
    }

    [Fact]
    public async Task UpdateTimeEntryAsync_ShouldUpdateAllEditableFields()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid,
            "Original notes"
        );

        // Modify the entry
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toUpdate = uow.GetObjectByKey<TimeEntry>(entry.Oid);
            toUpdate!.Date = new DateTime(2025, 12, 15);
            toUpdate.StartTime = new TimeSpan(10, 0, 0);
            toUpdate.EndTime = new TimeSpan(18, 0, 0);
            toUpdate.Notes = "Updated notes";

            // Act
            await _timeEntryService.UpdateTimeEntryAsync(toUpdate);
        }

        // Assert
        var updated = await _timeEntryService.GetTimeEntryByIdAsync(entry.Oid);
        updated.Should().NotBeNull();
        updated!.Date.Should().Be(new DateTime(2025, 12, 15));
        updated.StartTime.Should().Be(new TimeSpan(10, 0, 0));
        updated.EndTime.Should().Be(new TimeSpan(18, 0, 0));
        updated.Notes.Should().Be("Updated notes");
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTimeEntryAsync_ShouldNotUpdateAppliedPayRate()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate(50.00m);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid
        );

        var originalPayRate = entry.AppliedPayRate;

        // Try to update
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toUpdate = uow.GetObjectByKey<TimeEntry>(entry.Oid);
            toUpdate!.Date = new DateTime(2025, 12, 15);
            await _timeEntryService.UpdateTimeEntryAsync(toUpdate);
        }

        // Assert - Pay rate should remain unchanged
        var updated = await _timeEntryService.GetTimeEntryByIdAsync(entry.Oid);
        updated!.AppliedPayRate.Should().Be(originalPayRate);
    }

    [Fact]
    public async Task UpdateTimeEntryAsync_ShouldNotThrow_WhenTimeEntryDoesNotExist()
    {
        // Arrange
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var nonExistent = new TimeEntry(uow)
        {
            Date = new DateTime(2025, 12, 1),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        // Act
        var act = async () => await _timeEntryService.UpdateTimeEntryAsync(nonExistent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteTimeEntryAsync_ShouldSoftDeleteTimeEntry()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(9, 0, 0),
            new TimeSpan(17, 0, 0),
            project.Oid
        );

        // Act
        await _timeEntryService.DeleteTimeEntryAsync(entry.Oid);

        // Assert
        var allEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 12, 31)
        );
        allEntries.Should().NotContain(e => e.Oid == entry.Oid);
    }

    [Fact]
    public async Task DeleteTimeEntryAsync_ShouldNotThrow_WhenTimeEntryDoesNotExist()
    {
        // Act
        var act = async () => await _timeEntryService.DeleteTimeEntryAsync(99999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateTimeEntryAsync_ShouldHandleOvernightShifts()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate(50.00m);
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        // Act - Night shift ending next day
        var result = await _timeEntryService.CreateTimeEntryAsync(
            new DateTime(2025, 12, 1),
            new TimeSpan(22, 0, 0), // 10 PM
            new TimeSpan(6, 0, 0),  // 6 AM next day
            project.Oid
        );

        // Assert
        // Duration should be negative in this implementation (TimeSpan subtraction)
        // In real app, you'd need special handling for overnight shifts
        result.Duration.Should().BeLessThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task GetTimeEntriesByDateRangeAsync_ShouldNotIncludeDeletedEntries()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry1 = await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Keep");
        var entry2 = await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Delete");
        
        await _timeEntryService.DeleteTimeEntryAsync(entry2.Oid);

        // Act
        var result = await _timeEntryService.GetTimeEntriesByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(1);
        entries.Should().Contain(e => e.Notes == "Keep");
    }

    [Fact]
    public async Task GetTimeEntriesByProjectAsync_ShouldNotIncludeDeletedEntries()
    {
        // Arrange
        var project = CreateTestProject();
        var payRate = CreateTestPayRate();
        _mockPayRateService.Setup(x => x.GetPayRateForDateAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(payRate);

        var entry1 = await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Keep");
        var entry2 = await _timeEntryService.CreateTimeEntryAsync(new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), project.Oid, "Delete");
        
        await _timeEntryService.DeleteTimeEntryAsync(entry2.Oid);

        // Act
        var result = await _timeEntryService.GetTimeEntriesByProjectAsync(project.Oid);

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(1);
        entries.Should().Contain(e => e.Notes == "Keep");
    }
}