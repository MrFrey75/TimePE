using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public interface IIncidentalService
{
    Task<Incidental> CreateIncidentalAsync(DateTime date, decimal amount, string description, IncidentalType type = IncidentalType.Owed);
    Task<Incidental?> GetIncidentalByIdAsync(int id);
    Task<IEnumerable<Incidental>> GetIncidentalsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Incidental>> GetAllIncidentalsAsync(bool includeDeleted = false);
    Task UpdateIncidentalAsync(Incidental incidental);
    Task DeleteIncidentalAsync(int id);
}

public class IncidentalService : IIncidentalService
{
    private readonly string _connectionString;

    public IncidentalService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Incidental> CreateIncidentalAsync(DateTime date, decimal amount, string description, IncidentalType type = IncidentalType.Owed)
    {
        return await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var incidental = new Incidental(uow)
            {
                Date = date,
                Amount = amount,
                Description = description,
                Type = type
            };
            uow.CommitChanges();
            return incidental;
        });
    }

    public async Task<Incidental?> GetIncidentalByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return session.GetObjectByKey<Incidental>(id);
        });
    }

    public async Task<IEnumerable<Incidental>> GetIncidentalsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return new XPQuery<Incidental>(session)
                .Where(i => i.Date >= startDate && i.Date <= endDate && i.DeletedAt == null)
                .OrderByDescending(i => i.Date)
                .ToList();
        });
    }

    public async Task<IEnumerable<Incidental>> GetAllIncidentalsAsync(bool includeDeleted = false)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var query = new XPQuery<Incidental>(session);
            if (!includeDeleted)
            {
                query = (XPQuery<Incidental>)query.Where(i => i.DeletedAt == null);
            }
            return query.OrderByDescending(i => i.Date).ToList();
        });
    }

    public async Task UpdateIncidentalAsync(Incidental incidental)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var existing = uow.GetObjectByKey<Incidental>(incidental.Oid);
            if (existing != null)
            {
                existing.Date = incidental.Date;
                existing.Amount = incidental.Amount;
                existing.Description = incidental.Description;
                existing.Type = incidental.Type;
                existing.UpdatedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }

    public async Task DeleteIncidentalAsync(int id)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var incidental = uow.GetObjectByKey<Incidental>(id);
            if (incidental != null)
            {
                incidental.DeletedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }
}
