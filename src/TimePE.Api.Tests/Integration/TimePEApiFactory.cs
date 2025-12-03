using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.Xpo;

namespace TimePE.Api.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing with test database
/// </summary>
public class TimePEApiFactory : WebApplicationFactory<Program>
{
    private readonly TestDatabaseFixture _dbFixture;

    public TimePEApiFactory(TestDatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override connection string with test database
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbFixture.ConnectionString,
                ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-characters-long-for-testing",
                ["Jwt:Issuer"] = "TimePE.Api.Test",
                ["Jwt:Audience"] = "TimePE.Test",
                ["Jwt:ExpirationMinutes"] = "60"
            }!);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Set XPO DataLayer globally for test database
            XpoDefault.DataLayer = TimePE.Core.Database.ConnectionHelper.GetDataLayer(_dbFixture.ConnectionString);
            
            // Remove the default service registrations and re-add with correct connection string
            var serviceDescriptors = services.Where(d => 
                d.ServiceType.Namespace?.StartsWith("TimePE.Core.Services") == true &&
                d.ImplementationType != null
            ).ToList();

            foreach (var descriptor in serviceDescriptors)
            {
                services.Remove(descriptor);
            }

            // Re-register services with connection string
            services.AddScoped<TimePE.Core.Services.IPayRateService, TimePE.Core.Services.PayRateService>(sp => 
                new TimePE.Core.Services.PayRateService(_dbFixture.ConnectionString));
            services.AddScoped<TimePE.Core.Services.AuthService>(sp => 
                new TimePE.Core.Services.AuthService(_dbFixture.ConnectionString));
            services.AddScoped<TimePE.Core.Services.ProjectService>(sp => 
                new TimePE.Core.Services.ProjectService(_dbFixture.ConnectionString));
            services.AddScoped<TimePE.Core.Services.ITimeEntryService, TimePE.Core.Services.TimeEntryService>(sp => 
                new TimePE.Core.Services.TimeEntryService(_dbFixture.ConnectionString, sp.GetRequiredService<TimePE.Core.Services.IPayRateService>()));
            services.AddScoped<TimePE.Core.Services.PaymentService>(sp => 
                new TimePE.Core.Services.PaymentService(_dbFixture.ConnectionString));
            services.AddScoped<TimePE.Core.Services.IncidentalService>(sp => 
                new TimePE.Core.Services.IncidentalService(_dbFixture.ConnectionString));
            services.AddScoped<TimePE.Core.Services.DashboardService>(sp => 
                new TimePE.Core.Services.DashboardService(_dbFixture.ConnectionString));
        });
    }
}
