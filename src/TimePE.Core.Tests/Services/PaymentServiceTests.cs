using DevExpress.Xpo;
using FluentAssertions;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class PaymentServiceTests : IDisposable
{
    private readonly PaymentService _paymentService;
    private readonly string _connectionString;
    private readonly IDataLayer _dataLayer;

    public PaymentServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _paymentService = new PaymentService(_connectionString);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldCreateNewPayment_WithoutNotes()
    {
        // Arrange
        var date = new DateTime(2025, 12, 1);
        var amount = 1500.00m;

        // Act
        var result = await _paymentService.CreatePaymentAsync(date, amount);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.Amount.Should().Be(amount);
        result.Notes.Should().BeNull();
        result.Oid.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldCreateNewPayment_WithNotes()
    {
        // Arrange
        var date = new DateTime(2025, 12, 1);
        var amount = 2000.00m;
        var notes = "Payment for November work";

        // Act
        var result = await _paymentService.CreatePaymentAsync(date, amount, notes);

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(date);
        result.Amount.Should().Be(amount);
        result.Notes.Should().Be(notes);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_ShouldReturnPayment_WhenExists()
    {
        // Arrange
        var payment = await _paymentService.CreatePaymentAsync(
            new DateTime(2025, 12, 1),
            1500.00m,
            "Test payment"
        );

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(payment.Oid);

        // Assert
        result.Should().NotBeNull();
        result!.Oid.Should().Be(payment.Oid);
        result.Amount.Should().Be(1500.00m);
        result.Notes.Should().Be("Test payment");
    }

    [Fact]
    public async Task GetPaymentByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _paymentService.GetPaymentByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaymentsByDateRangeAsync_ShouldReturnPaymentsInRange()
    {
        // Arrange
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 11, 15), 1000m, "November");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 5), 1500m, "Early December");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 15), 2000m, "Mid December");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 25), 2500m, "Late December");
        await _paymentService.CreatePaymentAsync(new DateTime(2026, 1, 5), 3000m, "January");

        // Act
        var result = await _paymentService.GetPaymentsByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var payments = result.ToList();
        payments.Should().HaveCount(3);
        payments.Should().Contain(p => p.Notes == "Early December");
        payments.Should().Contain(p => p.Notes == "Mid December");
        payments.Should().Contain(p => p.Notes == "Late December");
        payments.Should().NotContain(p => p.Notes == "November");
        payments.Should().NotContain(p => p.Notes == "January");
    }

    [Fact]
    public async Task GetPaymentsByDateRangeAsync_ShouldReturnOrderedByDateDescending()
    {
        // Arrange
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 5), 1000m, "First");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 25), 2000m, "Third");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 15), 1500m, "Second");

        // Act
        var result = await _paymentService.GetPaymentsByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        var payments = result.ToList();
        payments[0].Notes.Should().Be("Third");   // Dec 25
        payments[1].Notes.Should().Be("Second");  // Dec 15
        payments[2].Notes.Should().Be("First");   // Dec 5
    }

    [Fact]
    public async Task GetPaymentsByDateRangeAsync_ShouldReturnEmptyList_WhenNoPaymentsInRange()
    {
        // Arrange
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 11, 15), 1000m, "November");

        // Act
        var result = await _paymentService.GetPaymentsByDateRangeAsync(
            new DateTime(2025, 12, 1),
            new DateTime(2025, 12, 31)
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaymentsByDateRangeAsync_ShouldIncludeBoundaryDates()
    {
        // Arrange
        var startDate = new DateTime(2025, 12, 1);
        var endDate = new DateTime(2025, 12, 31);
        
        await _paymentService.CreatePaymentAsync(startDate, 1000m, "Start");
        await _paymentService.CreatePaymentAsync(endDate, 2000m, "End");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 15), 1500m, "Middle");

        // Act
        var result = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);

        // Assert
        var payments = result.ToList();
        payments.Should().HaveCount(3);
        payments.Should().Contain(p => p.Notes == "Start");
        payments.Should().Contain(p => p.Notes == "End");
        payments.Should().Contain(p => p.Notes == "Middle");
    }

    [Fact]
    public async Task GetAllPaymentsAsync_ShouldReturnAllPayments()
    {
        // Arrange
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 11, 15), 1000m, "First");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 5), 1500m, "Second");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 15), 2000m, "Third");

        // Act
        var result = await _paymentService.GetAllPaymentsAsync();

        // Assert
        var payments = result.ToList();
        payments.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_ShouldReturnOrderedByDateDescending()
    {
        // Arrange
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 11, 15), 1000m, "First");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 25), 2500m, "Third");
        await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 5), 1500m, "Second");

        // Act
        var result = await _paymentService.GetAllPaymentsAsync();

        // Assert
        var payments = result.ToList();
        payments[0].Notes.Should().Be("Third");   // Dec 25
        payments[1].Notes.Should().Be("Second");  // Dec 5
        payments[2].Notes.Should().Be("First");   // Nov 15
    }

    [Fact]
    public async Task GetAllPaymentsAsync_ShouldNotIncludeDeletedPayments()
    {
        // Arrange
        var payment1 = await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 1), 1000m, "Keep");
        var payment2 = await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 2), 1500m, "Delete");
        
        await _paymentService.DeletePaymentAsync(payment2.Oid);

        // Act
        var result = await _paymentService.GetAllPaymentsAsync();

        // Assert
        var payments = result.ToList();
        payments.Should().HaveCount(1);
        payments.Should().Contain(p => p.Notes == "Keep");
        payments.Should().NotContain(p => p.Notes == "Delete");
    }

    [Fact]
    public async Task UpdatePaymentAsync_ShouldUpdateAllFields()
    {
        // Arrange
        var payment = await _paymentService.CreatePaymentAsync(
            new DateTime(2025, 12, 1),
            1000m,
            "Original notes"
        );

        // Modify the payment
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toUpdate = uow.GetObjectByKey<Payment>(payment.Oid);
            toUpdate!.Date = new DateTime(2025, 12, 15);
            toUpdate.Amount = 2500.50m;
            toUpdate.Notes = "Updated notes";

            // Act
            await _paymentService.UpdatePaymentAsync(toUpdate);
        }

        // Assert
        var updated = await _paymentService.GetPaymentByIdAsync(payment.Oid);
        updated.Should().NotBeNull();
        updated!.Date.Should().Be(new DateTime(2025, 12, 15));
        updated.Amount.Should().Be(2500.50m);
        updated.Notes.Should().Be("Updated notes");
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdatePaymentAsync_ShouldHandleNullNotes()
    {
        // Arrange
        var payment = await _paymentService.CreatePaymentAsync(
            new DateTime(2025, 12, 1),
            1000m,
            "Original notes"
        );

        // Modify the payment
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var toUpdate = uow.GetObjectByKey<Payment>(payment.Oid);
            toUpdate!.Notes = null;

            // Act
            await _paymentService.UpdatePaymentAsync(toUpdate);
        }

        // Assert
        var updated = await _paymentService.GetPaymentByIdAsync(payment.Oid);
        updated.Should().NotBeNull();
        updated!.Notes.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePaymentAsync_ShouldNotThrow_WhenPaymentDoesNotExist()
    {
        // Arrange
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        var nonExistent = new Payment(uow)
        {
            Date = new DateTime(2025, 12, 1),
            Amount = 1000m,
            Notes = "Does not exist"
        };

        // Act
        var act = async () => await _paymentService.UpdatePaymentAsync(nonExistent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeletePaymentAsync_ShouldSoftDeletePayment()
    {
        // Arrange
        var payment = await _paymentService.CreatePaymentAsync(
            new DateTime(2025, 12, 1),
            1000m,
            "To be deleted"
        );

        // Act
        await _paymentService.DeletePaymentAsync(payment.Oid);

        // Assert
        var allPayments = await _paymentService.GetAllPaymentsAsync();
        allPayments.Should().NotContain(p => p.Oid == payment.Oid);
    }

    [Fact]
    public async Task DeletePaymentAsync_ShouldNotThrow_WhenPaymentDoesNotExist()
    {
        // Act
        var act = async () => await _paymentService.DeletePaymentAsync(99999);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldHandleVariousAmounts()
    {
        // Arrange & Act
        var small = await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 1), 0.01m, "Small");
        var large = await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 1), 999999.99m, "Large");
        var zero = await _paymentService.CreatePaymentAsync(new DateTime(2025, 12, 1), 0m, "Zero");

        // Assert
        small.Amount.Should().Be(0.01m);
        large.Amount.Should().Be(999999.99m);
        zero.Amount.Should().Be(0m);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldHandleLongNotes()
    {
        // Arrange
        var longNotes = new string('A', 1000);

        // Act
        var result = await _paymentService.CreatePaymentAsync(
            new DateTime(2025, 12, 1),
            1000m,
            longNotes
        );

        // Assert
        result.Notes.Should().Be(longNotes);
        result.Notes!.Length.Should().Be(1000);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldHandleEmptyStringNotes()
    {
        // Act
        var result = await _paymentService.CreatePaymentAsync(
            new DateTime(2025, 12, 1),
            1000m,
            ""
        );

        // Assert
        result.Notes.Should().Be("");
    }
}
