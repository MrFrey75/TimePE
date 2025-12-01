using DevExpress.Xpo;
using Serilog;
using TimePE.Core.Models;

namespace TimePE.Core.Database.Migrations;

public class PayRateInitializer
{
    private readonly string _connectionString;

    public PayRateInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeDefaultPayRateAsync()
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        // Check if any pay rate exists
        var payRateCount = await Task.Run(() => uow.Query<PayRate>()
            .Count(p => p.DeletedAt == null));

        if (payRateCount == 0)
        {
            var payRate = new PayRate(uow)
            {
                HourlyRate = 20.00m,
                EffectiveDate = DateTime.Today,
                CreatedAt = DateTime.UtcNow
            };

            await uow.CommitChangesAsync();
            Log.Information("Default pay rate of $20.00/hour created successfully");
        }
        else
        {
            Log.Information("Pay rate(s) already exist, skipping default pay rate creation");
        }
    }
}
