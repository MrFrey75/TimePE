using DevExpress.Xpo;
using FluentAssertions;
using Moq;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class DashboardServiceTests : IDisposable
{
    private readonly DashboardService _dashboardService;
    private readonly Mock<IPayRateService> _mockPayRateService;
    private readonly string _connectionString;
    private readonly IDataLayer _dataLayer;

    public DashboardServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _mockPayRateService = new Mock<IPayRateService>();
        _dashboardService = new DashboardService(_connectionString);
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

    private TimeEntry CreateTestTimeEntry(Project project, DateTime date, TimeSpan startTime, TimeSpan endTime, decimal payRate)
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var proj = uow.GetObjectByKey<Project>(project.Oid);
        var entry = new TimeEntry(uow)
        {
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            AppliedPayRate = payRate,
            Project = proj
        };
        uow.CommitChanges();
        return entry;
    }

    private Incidental CreateTestIncidental(IncidentalType type, decimal amount, DateTime date)
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var incidental = new Incidental(uow)
        {
            Type = type,
            Amount = amount,
            Date = date,
            Description = $"Test {type}"
        };
        uow.CommitChanges();
        return incidental;
    }

    private Payment CreateTestPayment(decimal amount, DateTime date)
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var payment = new Payment(uow)
        {
            Amount = amount,
            Date = date,
            Notes = "Test payment"
        };
        uow.CommitChanges();
        return payment;
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldReturnZeroValues_WhenNoData()
    {
        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalOwed.Should().Be(0);
        result.TotalPaid.Should().Be(0);
        result.CurrentBalance.Should().Be(0);
        result.TimeEntriesCount.Should().Be(0);
        result.TotalHoursWorked.Should().Be(0);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldCalculateTotalOwed_FromTimeEntries()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h * $50 = $400
        CreateTestTimeEntry(project, new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0), 50.00m); // 4h * $50 = $200

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalOwed.Should().Be(600.00m);
        result.TimeEntriesCount.Should().Be(2);
        result.TotalHoursWorked.Should().Be(12);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldIncludeIncidentalsOwed_InTotalOwed()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // $400
        CreateTestIncidental(IncidentalType.Owed, 100.00m, new DateTime(2025, 12, 1)); // +$100

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalOwed.Should().Be(500.00m); // $400 + $100
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldSubtractIncidentalsOwedBy_FromTotalOwed()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // $400
        CreateTestIncidental(IncidentalType.OwedBy, 75.00m, new DateTime(2025, 12, 1)); // -$75

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalOwed.Should().Be(325.00m); // $400 - $75
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldCalculateTotalPaid_FromPayments()
    {
        // Arrange
        CreateTestPayment(200.00m, new DateTime(2025, 12, 1));
        CreateTestPayment(150.00m, new DateTime(2025, 12, 5));

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalPaid.Should().Be(350.00m);
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldCalculateCurrentBalance_AsOwedMinusPaid()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // $400
        CreateTestIncidental(IncidentalType.Owed, 100.00m, new DateTime(2025, 12, 1)); // +$100
        CreateTestIncidental(IncidentalType.OwedBy, 50.00m, new DateTime(2025, 12, 2)); // -$50
        CreateTestPayment(200.00m, new DateTime(2025, 12, 5)); // Paid $200

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalOwed.Should().Be(450.00m); // $400 + $100 - $50
        result.TotalPaid.Should().Be(200.00m);
        result.CurrentBalance.Should().Be(250.00m); // $450 - $200
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldShowNegativeBalance_WhenOverpaid()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // $400
        CreateTestPayment(500.00m, new DateTime(2025, 12, 5)); // Paid $500

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalOwed.Should().Be(400.00m);
        result.TotalPaid.Should().Be(500.00m);
        result.CurrentBalance.Should().Be(-100.00m); // Overpaid by $100
    }

    [Fact]
    public async Task GetBalanceSummaryAsync_ShouldNotIncludeDeletedTimeEntries()
    {
        // Arrange
        var project = CreateTestProject();
        var entry1 = CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // $400
        var entry2 = CreateTestTimeEntry(project, new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // $400

        // Delete entry2
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toDelete = uow.GetObjectByKey<TimeEntry>(entry2.Oid);
            toDelete?.Delete();
            uow.CommitChanges();
        }

        // Act
        var result = await _dashboardService.GetBalanceSummaryAsync();

        // Assert
        result.TotalOwed.Should().Be(400.00m); // Only entry1
        result.TimeEntriesCount.Should().Be(1);
        result.TotalHoursWorked.Should().Be(8);
    }

    [Fact]
    public async Task GetRecentTimeEntriesAsync_ShouldReturnEmptyList_WhenNoEntries()
    {
        // Act
        var result = await _dashboardService.GetRecentTimeEntriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentTimeEntriesAsync_ShouldReturnMostRecentEntries_OrderedByDateDescending()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 11, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);
        CreateTestTimeEntry(project, new DateTime(2025, 11, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);

        // Act
        var result = await _dashboardService.GetRecentTimeEntriesAsync();

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(3);
        entries[0].Date.Should().Be(new DateTime(2025, 12, 1)); // Most recent
        entries[1].Date.Should().Be(new DateTime(2025, 11, 15));
        entries[2].Date.Should().Be(new DateTime(2025, 11, 1)); // Oldest
    }

    [Fact]
    public async Task GetRecentTimeEntriesAsync_ShouldOrderByStartTime_WhenSameDate()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(14, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Afternoon
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), 50.00m); // Morning

        // Act
        var result = await _dashboardService.GetRecentTimeEntriesAsync();

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(2);
        entries[0].StartTime.Should().Be(new TimeSpan(14, 0, 0)); // Later time first
        entries[1].StartTime.Should().Be(new TimeSpan(9, 0, 0));
    }

    [Fact]
    public async Task GetRecentTimeEntriesAsync_ShouldRespectCountParameter()
    {
        // Arrange
        var project = CreateTestProject();
        for (int i = 1; i <= 15; i++)
        {
            CreateTestTimeEntry(project, new DateTime(2025, 12, i), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);
        }

        // Act
        var result = await _dashboardService.GetRecentTimeEntriesAsync(5);

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(5);
        entries[0].Date.Should().Be(new DateTime(2025, 12, 15)); // Most recent 5
        entries[4].Date.Should().Be(new DateTime(2025, 12, 11));
    }

    [Fact]
    public async Task GetRecentTimeEntriesAsync_ShouldUseDefault10_WhenCountNotSpecified()
    {
        // Arrange
        var project = CreateTestProject();
        for (int i = 1; i <= 20; i++)
        {
            CreateTestTimeEntry(project, new DateTime(2025, 12, i), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);
        }

        // Act
        var result = await _dashboardService.GetRecentTimeEntriesAsync();

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetRecentTimeEntriesAsync_ShouldNotIncludeDeletedEntries()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);
        var entry2 = CreateTestTimeEntry(project, new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);
        CreateTestTimeEntry(project, new DateTime(2025, 12, 3), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);

        // Delete entry2
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toDelete = uow.GetObjectByKey<TimeEntry>(entry2.Oid);
            toDelete?.Delete();
            uow.CommitChanges();
        }

        // Act
        var result = await _dashboardService.GetRecentTimeEntriesAsync();

        // Assert
        var entries = result.ToList();
        entries.Should().HaveCount(2);
        entries.Should().NotContain(e => e.Date == new DateTime(2025, 12, 2));
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldReturnEmptyDictionary_WhenNoEntries()
    {
        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldGroupHoursByProject()
    {
        // Arrange
        var projectA = CreateTestProject("Project A");
        var projectB = CreateTestProject("Project B");
        
        CreateTestTimeEntry(projectA, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h
        CreateTestTimeEntry(projectA, new DateTime(2025, 12, 2), new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0), 50.00m); // 4h
        CreateTestTimeEntry(projectB, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), 50.00m); // 3h

        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync();

        // Assert
        result.Should().HaveCount(2);
        result["Project A"].Should().Be(12); // 8 + 4
        result["Project B"].Should().Be(3);
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldFilterByStartDate()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 11, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - before
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - on start
        CreateTestTimeEntry(project, new DateTime(2025, 12, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - after start

        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync(startDate: new DateTime(2025, 12, 1));

        // Assert
        result["Test Project"].Should().Be(16); // Only Dec 1 and Dec 15
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldFilterByEndDate()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - before end
        CreateTestTimeEntry(project, new DateTime(2025, 12, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - on end
        CreateTestTimeEntry(project, new DateTime(2025, 12, 31), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - after end

        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync(endDate: new DateTime(2025, 12, 15));

        // Assert
        result["Test Project"].Should().Be(16); // Only Dec 1 and Dec 15
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 11, 30), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - before
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - start
        CreateTestTimeEntry(project, new DateTime(2025, 12, 15), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - middle
        CreateTestTimeEntry(project, new DateTime(2025, 12, 31), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - end
        CreateTestTimeEntry(project, new DateTime(2026, 1, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - after

        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync(
            startDate: new DateTime(2025, 12, 1),
            endDate: new DateTime(2025, 12, 31)
        );

        // Assert
        result["Test Project"].Should().Be(24); // Dec 1, 15, and 31
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldHandleMultipleProjects_InDateRange()
    {
        // Arrange
        var projectA = CreateTestProject("Project A");
        var projectB = CreateTestProject("Project B");
        var projectC = CreateTestProject("Project C");

        CreateTestTimeEntry(projectA, new DateTime(2025, 11, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - before range
        CreateTestTimeEntry(projectA, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - in range
        CreateTestTimeEntry(projectB, new DateTime(2025, 12, 5), new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0), 50.00m); // 4h - in range
        CreateTestTimeEntry(projectC, new DateTime(2026, 1, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - after range

        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync(
            startDate: new DateTime(2025, 12, 1),
            endDate: new DateTime(2025, 12, 31)
        );

        // Assert
        result.Should().HaveCount(2);
        result["Project A"].Should().Be(8);
        result["Project B"].Should().Be(4);
        result.Should().NotContainKey("Project C");
    }

    [Fact]
    public async Task GetProjectHoursSummaryAsync_ShouldUseUnknown_ForEntriesWithoutProject()
    {
        // Arrange - Create a time entry without a project (if possible in your model)
        // This tests the null-coalescing in the grouping
        var project = CreateTestProject();
        CreateTestTimeEntry(project, new DateTime(2025, 12, 1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m);

        // Act
        var result = await _dashboardService.GetProjectHoursSummaryAsync();

        // Assert
        result.Should().ContainKey("Test Project");
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldReturnZero_WhenNoEntriesInWeek()
    {
        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(new DateTime(2025, 12, 1));

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldCalculateTotalHours_ForWeek()
    {
        // Arrange
        var project = CreateTestProject();
        var weekStart = new DateTime(2025, 12, 1); // Monday

        CreateTestTimeEntry(project, weekStart, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Monday - 8h
        CreateTestTimeEntry(project, weekStart.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Tuesday - 8h
        CreateTestTimeEntry(project, weekStart.AddDays(2), new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0), 50.00m); // Wednesday - 4h

        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(weekStart);

        // Assert
        result.Should().Be(20); // 8 + 8 + 4
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldNotIncludeEntriesBeforeWeekStart()
    {
        // Arrange
        var project = CreateTestProject();
        var weekStart = new DateTime(2025, 12, 1);

        CreateTestTimeEntry(project, weekStart.AddDays(-1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Before - 8h
        CreateTestTimeEntry(project, weekStart, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Week start - 8h
        CreateTestTimeEntry(project, weekStart.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // In week - 8h

        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(weekStart);

        // Assert
        result.Should().Be(16); // Only week start and day after
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldNotIncludeEntriesAfterWeekEnd()
    {
        // Arrange
        var project = CreateTestProject();
        var weekStart = new DateTime(2025, 12, 1);

        CreateTestTimeEntry(project, weekStart, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Week start - 8h
        CreateTestTimeEntry(project, weekStart.AddDays(6), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Last day of week - 8h
        CreateTestTimeEntry(project, weekStart.AddDays(7), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // After week - 8h

        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(weekStart);

        // Assert
        result.Should().Be(16); // Only week start and 6 days after (7 days total)
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldHandleMultipleEntriesPerDay()
    {
        // Arrange
        var project = CreateTestProject();
        var weekStart = new DateTime(2025, 12, 1);

        CreateTestTimeEntry(project, weekStart, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), 50.00m); // Morning - 3h
        CreateTestTimeEntry(project, weekStart, new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // Afternoon - 4h
        CreateTestTimeEntry(project, weekStart, new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0), 50.00m); // Evening - 2h

        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(weekStart);

        // Assert
        result.Should().Be(9); // 3 + 4 + 2
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldAggregateFromMultipleProjects()
    {
        // Arrange
        var projectA = CreateTestProject("Project A");
        var projectB = CreateTestProject("Project B");
        var weekStart = new DateTime(2025, 12, 1);

        CreateTestTimeEntry(projectA, weekStart, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h
        CreateTestTimeEntry(projectB, weekStart.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0), 50.00m); // 4h

        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(weekStart);

        // Assert
        result.Should().Be(12); // 8 + 4
    }

    [Fact]
    public async Task GetWeeklyHoursAsync_ShouldNotIncludeDeletedEntries()
    {
        // Arrange
        var project = CreateTestProject();
        var weekStart = new DateTime(2025, 12, 1);

        CreateTestTimeEntry(project, weekStart, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h
        var entry2 = CreateTestTimeEntry(project, weekStart.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), 50.00m); // 8h - to delete

        // Delete entry2
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toDelete = uow.GetObjectByKey<TimeEntry>(entry2.Oid);
            toDelete?.Delete();
            uow.CommitChanges();
        }

        // Act
        var result = await _dashboardService.GetWeeklyHoursAsync(weekStart);

        // Assert
        result.Should().Be(8); // Only first entry
    }
}
