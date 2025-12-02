using DevExpress.Xpo;
using FluentAssertions;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class PayRateServiceTests : IDisposable
{
    private readonly PayRateService _payRateService;
    private readonly string _connectionString;
    private readonly IDataLayer _dataLayer;

    public PayRateServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _payRateService = new PayRateService(_connectionString);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    [Fact]
    public async Task CreatePayRateAsync_ShouldCreateNewPayRate()
    {
        // Arrange
        var hourlyRate = 50.00m;
        var effectiveDate = new DateTime(2025, 1, 1);

        // Act
        var result = await _payRateService.CreatePayRateAsync(hourlyRate, effectiveDate);

        // Assert
        result.Should().NotBeNull();
        result.HourlyRate.Should().Be(hourlyRate);
        result.EffectiveDate.Should().Be(effectiveDate);
        result.EndDate.Should().BeNull();
        result.Oid.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreatePayRateAsync_ShouldSetEndDateOnPreviousRate_WhenNewRateCreated()
    {
        // Arrange
        var oldRate = await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 1, 1));
        var newEffectiveDate = new DateTime(2025, 6, 1);

        // Act
        var newRate = await _payRateService.CreatePayRateAsync(55.00m, newEffectiveDate);

        // Assert
        newRate.Should().NotBeNull();
        newRate.HourlyRate.Should().Be(55.00m);
        newRate.EndDate.Should().BeNull();

        var updatedOldRate = await _payRateService.GetPayRateByIdAsync(oldRate.Oid);
        updatedOldRate.Should().NotBeNull();
        updatedOldRate!.EndDate.Should().Be(newEffectiveDate.AddDays(-1));
        updatedOldRate.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreatePayRateAsync_ShouldHandleMultipleRateChanges()
    {
        // Arrange
        var rate1 = await _payRateService.CreatePayRateAsync(40.00m, new DateTime(2025, 1, 1));
        var rate2 = await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 3, 1));
        
        // Act
        var rate3 = await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        // Assert
        rate3.EndDate.Should().BeNull();

        var updated1 = await _payRateService.GetPayRateByIdAsync(rate1.Oid);
        updated1!.EndDate.Should().Be(new DateTime(2025, 2, 28)); // One day before rate2

        var updated2 = await _payRateService.GetPayRateByIdAsync(rate2.Oid);
        updated2!.EndDate.Should().Be(new DateTime(2025, 5, 31)); // One day before rate3
    }

    [Fact]
    public async Task GetPayRateByIdAsync_ShouldReturnPayRate_WhenExists()
    {
        // Arrange
        var payRate = await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 1, 1));

        // Act
        var result = await _payRateService.GetPayRateByIdAsync(payRate.Oid);

        // Assert
        result.Should().NotBeNull();
        result!.Oid.Should().Be(payRate.Oid);
        result.HourlyRate.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetPayRateByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _payRateService.GetPayRateByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentPayRateAsync_ShouldReturnRateWithNullEndDate()
    {
        // Arrange
        await _payRateService.CreatePayRateAsync(40.00m, new DateTime(2025, 1, 1));
        await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 3, 1));
        var currentRate = await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        // Act
        var result = await _payRateService.GetCurrentPayRateAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Oid.Should().Be(currentRate.Oid);
        result.HourlyRate.Should().Be(50.00m);
        result.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentPayRateAsync_ShouldReturnNull_WhenNoRatesExist()
    {
        // Act
        var result = await _payRateService.GetCurrentPayRateAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPayRateForDateAsync_ShouldReturnCorrectRate_ForSpecificDate()
    {
        // Arrange
        var rate1 = await _payRateService.CreatePayRateAsync(40.00m, new DateTime(2025, 1, 1));
        var rate2 = await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 3, 1));
        var rate3 = await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        // Act & Assert
        var resultJan = await _payRateService.GetPayRateForDateAsync(new DateTime(2025, 1, 15));
        resultJan!.HourlyRate.Should().Be(40.00m);

        var resultMar = await _payRateService.GetPayRateForDateAsync(new DateTime(2025, 3, 15));
        resultMar!.HourlyRate.Should().Be(45.00m);

        var resultJun = await _payRateService.GetPayRateForDateAsync(new DateTime(2025, 6, 15));
        resultJun!.HourlyRate.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetPayRateForDateAsync_ShouldReturnCorrectRate_OnEffectiveDate()
    {
        // Arrange
        var effectiveDate = new DateTime(2025, 6, 1);
        await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 1, 1));
        var newRate = await _payRateService.CreatePayRateAsync(50.00m, effectiveDate);

        // Act
        var result = await _payRateService.GetPayRateForDateAsync(effectiveDate);

        // Assert
        result.Should().NotBeNull();
        result!.Oid.Should().Be(newRate.Oid);
        result.HourlyRate.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetPayRateForDateAsync_ShouldReturnCorrectRate_OnEndDate()
    {
        // Arrange
        var rate1 = await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 1, 1));
        await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        var updated = await _payRateService.GetPayRateByIdAsync(rate1.Oid);
        var endDate = updated!.EndDate!.Value;

        // Act
        var result = await _payRateService.GetPayRateForDateAsync(endDate);

        // Assert
        result.Should().NotBeNull();
        result!.HourlyRate.Should().Be(45.00m); // Should still be the old rate on its end date
    }

    [Fact]
    public async Task GetPayRateForDateAsync_ShouldReturnNull_WhenNoRateForDate()
    {
        // Arrange
        await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        // Act - Query for date before any rates
        var result = await _payRateService.GetPayRateForDateAsync(new DateTime(2025, 1, 1));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllPayRatesAsync_ShouldReturnAllRates_OrderedByEffectiveDateDescending()
    {
        // Arrange
        await _payRateService.CreatePayRateAsync(40.00m, new DateTime(2025, 1, 1));
        await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 3, 1));
        await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        // Act
        var result = await _payRateService.GetAllPayRatesAsync();

        // Assert
        var rates = result.ToList();
        rates.Should().HaveCount(3);
        rates[0].HourlyRate.Should().Be(50.00m); // Most recent first
        rates[1].HourlyRate.Should().Be(45.00m);
        rates[2].HourlyRate.Should().Be(40.00m);
    }

    [Fact]
    public async Task GetAllPayRatesAsync_ShouldNotIncludeDeletedRates()
    {
        // Arrange
        var rate1 = await _payRateService.CreatePayRateAsync(40.00m, new DateTime(2025, 1, 1));
        var rate2 = await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 3, 1));
        
        await _payRateService.DeletePayRateAsync(rate1.Oid);

        // Act
        var result = await _payRateService.GetAllPayRatesAsync();

        // Assert
        var rates = result.ToList();
        rates.Should().HaveCount(1);
        rates.Should().Contain(r => r.Oid == rate2.Oid);
        rates.Should().NotContain(r => r.Oid == rate1.Oid);
    }

    [Fact]
    public async Task DeletePayRateAsync_ShouldSoftDeletePayRate()
    {
        // Arrange
        var payRate = await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 1, 1));

        // Act
        await _payRateService.DeletePayRateAsync(payRate.Oid);

        // Assert
        var allRates = await _payRateService.GetAllPayRatesAsync();
        allRates.Should().NotContain(r => r.Oid == payRate.Oid);
    }

    [Fact]
    public async Task DeletePayRateAsync_ShouldNotThrow_WhenPayRateDoesNotExist()
    {
        // Act
        var act = async () => await _payRateService.DeletePayRateAsync(99999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreatePayRateAsync_ShouldHandleVariousRates()
    {
        // Act
        var small = await _payRateService.CreatePayRateAsync(0.01m, new DateTime(2025, 1, 1));
        var large = await _payRateService.CreatePayRateAsync(999.99m, new DateTime(2025, 2, 1));
        var precise = await _payRateService.CreatePayRateAsync(52.75m, new DateTime(2025, 3, 1));

        // Assert
        small.HourlyRate.Should().Be(0.01m);
        large.HourlyRate.Should().Be(999.99m);
        precise.HourlyRate.Should().Be(52.75m);
    }

    [Fact]
    public async Task CreatePayRateAsync_ShouldOnlyCloseCurrentRate_NotHistoricalRates()
    {
        // Arrange
        var rate1 = await _payRateService.CreatePayRateAsync(40.00m, new DateTime(2025, 1, 1));
        var rate2 = await _payRateService.CreatePayRateAsync(45.00m, new DateTime(2025, 3, 1));
        
        // Act - Create third rate
        await _payRateService.CreatePayRateAsync(50.00m, new DateTime(2025, 6, 1));

        // Assert - Rate1 should still have its original end date
        var updated1 = await _payRateService.GetPayRateByIdAsync(rate1.Oid);
        updated1!.EndDate.Should().Be(new DateTime(2025, 2, 28)); // Set when rate2 was created
        
        // Rate2 should have new end date
        var updated2 = await _payRateService.GetPayRateByIdAsync(rate2.Oid);
        updated2!.EndDate.Should().Be(new DateTime(2025, 5, 31)); // Set when rate3 was created
    }
}