using DevExpress.Xpo;
using FluentAssertions;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class IncidentalServiceTests : IDisposable
{
    private readonly IncidentalService _incidentalService;
    private readonly string _connectionString;
    private readonly IDataLayer _dataLayer;

    public IncidentalServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _incidentalService = new IncidentalService(_connectionString);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    [Fact]
    public async Task CreateIncidentalAsync_ShouldCreateNewIncidental_WithOwedType()
    {
        // Arrange
        var date = new DateTime(2025, 12, 1);
        var amount = 150.50m;
        var description = "Office supplies";

        // Act
        var result = await _incidentalService.CreateIncidentalAsync(date, amount, description, IncidentalType.Owed);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.Amount.Should().Be(amount);
        result.Description.Should().Be(description);
        result.Type.Should().Be(IncidentalType.Owed);
        result.Oid.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateIncidentalAsync_ShouldCreateNewIncidental_WithOwedByType()
    {
        // Arrange
        var date = new DateTime(2025, 12, 1);
        var amount = 75.00m;
        var description = "Reimbursement for parking";

        // Act
        var result = await _incidentalService.CreateIncidentalAsync(date, amount, description, IncidentalType.OwedBy);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(IncidentalType.OwedBy);
        result.Amount.Should().Be(amount);
        result.Description.Should().Be(description);
    }

    [Fact]
    public async Task CreateIncidentalAsync_ShouldUseOwedTypeAsDefault()
    {
        // Arrange
        var date = new DateTime(2025, 12, 1);
        var amount = 100.00m;
        var description = "Default type test";

        // Act
        var result = await _incidentalService.CreateIncidentalAsync(date, amount, description);

        // Assert
        result.Type.Should().Be(IncidentalType.Owed);
    }

    [Fact]
    public async Task GetIncidentalByIdAsync_ShouldReturnIncidental_WhenExists()
    {
        // Arrange
        var incidental = await _incidentalService.CreateIncidentalAsync(
            new DateTime(2025, 12, 1),
            50.00m,
            "Test incidental",
            IncidentalType.Owed
        );

        // Act
        var result = await _incidentalService.GetIncidentalByIdAsync(incidental.Oid);

        // Assert
        result.Should().NotBeNull();
        result!.Oid.Should().Be(incidental.Oid);
        result.Description.Should().Be("Test incidental");
        result.Amount.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetIncidentalByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _incidentalService.GetIncidentalByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetIncidentalsByDateRangeAsync_ShouldReturnIncidentalsInRange()
    {
        // Arrange
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 11, 15), 100m, "November", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 5), 200m, "Early December", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 15), 300m, "Mid December", IncidentalType.OwedBy);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 25), 400m, "Late December", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2026, 1, 5), 500m, "January", IncidentalType.Owed);

        // Act
        var result = await _incidentalService.GetIncidentalsByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var incidentals = result.ToList();
        incidentals.Should().HaveCount(3);
        incidentals.Should().Contain(i => i.Description == "Early December");
        incidentals.Should().Contain(i => i.Description == "Mid December");
        incidentals.Should().Contain(i => i.Description == "Late December");
        incidentals.Should().NotContain(i => i.Description == "November");
        incidentals.Should().NotContain(i => i.Description == "January");
    }

    [Fact]
    public async Task GetIncidentalsByDateRangeAsync_ShouldReturnOrderedByDateDescending()
    {
        // Arrange
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 5), 100m, "First", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 25), 200m, "Third", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 15), 300m, "Second", IncidentalType.Owed);

        // Act
        var result = await _incidentalService.GetIncidentalsByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var incidentals = result.ToList();
        incidentals[0].Description.Should().Be("Third");   // Dec 25
        incidentals[1].Description.Should().Be("Second");  // Dec 15
        incidentals[2].Description.Should().Be("First");   // Dec 5
    }

    [Fact]
    public async Task GetIncidentalsByDateRangeAsync_ShouldReturnEmptyList_WhenNoIncidentalsInRange()
    {
        // Arrange
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 11, 15), 100m, "November", IncidentalType.Owed);

        // Act
        var result = await _incidentalService.GetIncidentalsByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllIncidentalsAsync_ShouldReturnAllIncidentals()
    {
        // Arrange
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 11, 15), 100m, "First", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 5), 200m, "Second", IncidentalType.OwedBy);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 15), 300m, "Third", IncidentalType.Owed);

        // Act
        var result = await _incidentalService.GetAllIncidentalsAsync();

        // Assert
        var incidentals = result.ToList();
        incidentals.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllIncidentalsAsync_ShouldReturnOrderedByDateDescending()
    {
        // Arrange
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 11, 15), 100m, "First", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 25), 300m, "Third", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 5), 200m, "Second", IncidentalType.Owed);

        // Act
        var result = await _incidentalService.GetAllIncidentalsAsync();

        // Assert
        var incidentals = result.ToList();
        incidentals[0].Description.Should().Be("Third");   // Dec 25
        incidentals[1].Description.Should().Be("Second");  // Dec 5
        incidentals[2].Description.Should().Be("First");   // Nov 15
    }

    [Fact]
    public async Task GetAllIncidentalsAsync_ShouldNotIncludeDeletedIncidentals()
    {
        // Arrange
        var incidental1 = await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 1), 100m, "Keep", IncidentalType.Owed);
        var incidental2 = await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 2), 200m, "Delete", IncidentalType.Owed);
        
        await _incidentalService.DeleteIncidentalAsync(incidental2.Oid);

        // Act
        var result = await _incidentalService.GetAllIncidentalsAsync();

        // Assert
        var incidentals = result.ToList();
        incidentals.Should().HaveCount(1);
        incidentals.Should().Contain(i => i.Description == "Keep");
        incidentals.Should().NotContain(i => i.Description == "Delete");
    }

    [Fact]
    public async Task UpdateIncidentalAsync_ShouldUpdateAllFields()
    {
        // Arrange
        var incidental = await _incidentalService.CreateIncidentalAsync(
            new DateTime(2025, 12, 1),
            100m,
            "Original description",
            IncidentalType.Owed
        );

        // Modify the incidental
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toUpdate = uow.GetObjectByKey<Incidental>(incidental.Oid);
            toUpdate!.Date = new DateTime(2025, 12, 15);
            toUpdate.Amount = 250.75m;
            toUpdate.Description = "Updated description";
            toUpdate.Type = IncidentalType.OwedBy;

            // Act
            await _incidentalService.UpdateIncidentalAsync(toUpdate);
        }

        // Assert
        var updated = await _incidentalService.GetIncidentalByIdAsync(incidental.Oid);
        updated.Should().NotBeNull();
        updated!.Date.Should().Be(new DateTime(2025, 12, 15));
        updated.Amount.Should().Be(250.75m);
        updated.Description.Should().Be("Updated description");
        updated.Type.Should().Be(IncidentalType.OwedBy);
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateIncidentalAsync_ShouldNotThrow_WhenIncidentalDoesNotExist()
    {
        // Arrange
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var nonExistent = new Incidental(uow)
        {
            Date = new DateTime(2025, 12, 1),
            Amount = 100m,
            Description = "Does not exist",
            Type = IncidentalType.Owed
        };

        // Act
        var act = async () => await _incidentalService.UpdateIncidentalAsync(nonExistent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteIncidentalAsync_ShouldSoftDeleteIncidental()
    {
        // Arrange
        var incidental = await _incidentalService.CreateIncidentalAsync(
            new DateTime(2025, 12, 1),
            100m,
            "To be deleted",
            IncidentalType.Owed
        );

        // Act
        await _incidentalService.DeleteIncidentalAsync(incidental.Oid);

        // Assert
        var allIncidentals = await _incidentalService.GetAllIncidentalsAsync();
        allIncidentals.Should().NotContain(i => i.Oid == incidental.Oid);
    }

    [Fact]
    public async Task DeleteIncidentalAsync_ShouldNotThrow_WhenIncidentalDoesNotExist()
    {
        // Act
        var act = async () => await _incidentalService.DeleteIncidentalAsync(99999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateIncidentalAsync_ShouldHandleVariousAmounts()
    {
        // Arrange & Act
        var small = await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 1), 0.01m, "Small", IncidentalType.Owed);
        var large = await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 1), 99999.99m, "Large", IncidentalType.Owed);
        var zero = await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 1), 0m, "Zero", IncidentalType.Owed);

        // Assert
        small.Amount.Should().Be(0.01m);
        large.Amount.Should().Be(99999.99m);
        zero.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task CreateIncidentalAsync_ShouldHandleLongDescriptions()
    {
        // Arrange
        var longDescription = new string('A', 500); // Max size

        // Act
        var result = await _incidentalService.CreateIncidentalAsync(
            new DateTime(2025, 12, 1),
            100m,
            longDescription,
            IncidentalType.Owed
        );

        // Assert
        result.Description.Should().Be(longDescription);
        result.Description.Length.Should().Be(500);
    }

    [Fact]
    public async Task GetIncidentalsByDateRangeAsync_ShouldIncludeBoundaryDates()
    {
        // Arrange
        var startDate = new DateTime(2025, 12, 1);
        var endDate = new DateTime(2025, 12, 31);
        
        await _incidentalService.CreateIncidentalAsync(startDate, 100m, "Start", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(endDate, 200m, "End", IncidentalType.Owed);
        await _incidentalService.CreateIncidentalAsync(new DateTime(2025, 12, 15), 300m, "Middle", IncidentalType.Owed);

        // Act
        var result = await _incidentalService.GetIncidentalsByDateRangeAsync(startDate, endDate);

        // Assert
        var incidentals = result.ToList();
        incidentals.Should().HaveCount(3);
        incidentals.Should().Contain(i => i.Description == "Start");
        incidentals.Should().Contain(i => i.Description == "End");
        incidentals.Should().Contain(i => i.Description == "Middle");
    }
}