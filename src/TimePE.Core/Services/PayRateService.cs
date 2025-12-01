using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public interface IPayRateService
{
    Task<PayRate> CreatePayRateAsync(decimal hourlyRate, DateTime effectiveDate);
    Task<PayRate?> GetPayRateByIdAsync(int id);
    Task<PayRate?> GetCurrentPayRateAsync();
    Task<PayRate?> GetPayRateForDateAsync(DateTime date);
    Task<IEnumerable<PayRate>> GetAllPayRatesAsync(bool includeDeleted = false);
    Task DeletePayRateAsync(int id);
}

public class PayRateService : IPayRateService
{
    private readonly string _connectionString;

    public PayRateService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<PayRate> CreatePayRateAsync(decimal hourlyRate, DateTime effectiveDate)
    {
        return await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            
            var criteria = CriteriaOperator.Parse("EndDate Is Null And DeletedAt Is Null");
            var currentRate = uow.FindObject<PayRate>(criteria);

            if (currentRate != null)
            {
                currentRate.EndDate = effectiveDate.AddDays(-1);
                currentRate.UpdatedAt = DateTime.UtcNow;
            }

            var payRate = new PayRate(uow)
            {
                HourlyRate = hourlyRate,
                EffectiveDate = effectiveDate
            };
            uow.CommitChanges();
            return payRate;
        });
    }

    public async Task<PayRate?> GetPayRateByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return session.GetObjectByKey<PayRate>(id);
        });
    }

    public async Task<PayRate?> GetCurrentPayRateAsync()
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = CriteriaOperator.Parse("EndDate Is Null And DeletedAt Is Null");
            var collection = new XPCollection<PayRate>(session, criteria);
            collection.Sorting.Add(new SortProperty("EffectiveDate", DevExpress.Xpo.DB.SortingDirection.Descending));
            return collection.FirstOrDefault();
        });
    }

    public async Task<PayRate?> GetPayRateForDateAsync(DateTime date)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = CriteriaOperator.Parse("EffectiveDate <= ? And (EndDate Is Null Or EndDate >= ?) And DeletedAt Is Null", date, date);
            var collection = new XPCollection<PayRate>(session, criteria);
            collection.Sorting.Add(new SortProperty("EffectiveDate", DevExpress.Xpo.DB.SortingDirection.Descending));
            return collection.FirstOrDefault();
        });
    }

    public async Task<IEnumerable<PayRate>> GetAllPayRatesAsync(bool includeDeleted = false)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = includeDeleted ? null : CriteriaOperator.Parse("DeletedAt Is Null");
            var collection = new XPCollection<PayRate>(session, criteria);
            collection.Sorting.Add(new SortProperty("EffectiveDate", DevExpress.Xpo.DB.SortingDirection.Descending));
            return collection.ToList();
        });
    }

    public async Task DeletePayRateAsync(int id)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var payRate = uow.GetObjectByKey<PayRate>(id);
            if (payRate != null)
            {
                payRate.DeletedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }
}
