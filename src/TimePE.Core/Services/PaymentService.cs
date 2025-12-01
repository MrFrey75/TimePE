using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(DateTime date, decimal amount, string? notes = null);
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(bool includeDeleted = false);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);
}

public class PaymentService : IPaymentService
{
    private readonly string _connectionString;

    public PaymentService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Payment> CreatePaymentAsync(DateTime date, decimal amount, string? notes = null)
    {
        return await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var payment = new Payment(uow)
            {
                Date = date,
                Amount = amount,
                Notes = notes
            };
            uow.CommitChanges();
            return payment;
        });
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return session.GetObjectByKey<Payment>(id);
        });
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return new XPQuery<Payment>(session)
                .Where(p => p.Date >= startDate && p.Date <= endDate && p.DeletedAt == null)
                .OrderByDescending(p => p.Date)
                .ToList();
        });
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(bool includeDeleted = false)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var query = new XPQuery<Payment>(session);
            if (!includeDeleted)
            {
                query = (XPQuery<Payment>)query.Where(p => p.DeletedAt == null);
            }
            return query.OrderByDescending(p => p.Date).ToList();
        });
    }

                payRate.Delete();
    public async Task UpdatePaymentAsync(Payment payment)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var existing = uow.GetObjectByKey<Payment>(payment.Oid);
            if (existing != null)
            {
                existing.Date = payment.Date;
                existing.Amount = payment.Amount;
                existing.Notes = payment.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }

    public async Task DeletePaymentAsync(int id)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var payment = uow.GetObjectByKey<Payment>(id);
            if (payment != null)
            {
                payment.DeletedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }
}
