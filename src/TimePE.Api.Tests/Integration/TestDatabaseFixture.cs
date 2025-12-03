using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using TimePE.Core.Database;
using TimePE.Core.Models;

namespace TimePE.Api.Tests.Integration;

/// <summary>
/// Fixture for managing test database lifecycle across integration tests
/// </summary>
public class TestDatabaseFixture : IDisposable
{
    public string ConnectionString { get; }
    private readonly string _testDbPath;

    public TestDatabaseFixture()
    {
        // Use unique database for each test run to avoid conflicts
        _testDbPath = Path.Combine(Path.GetTempPath(), $"timepe_test_{Guid.NewGuid()}.db");
        ConnectionString = $"XpoProvider=SQLite;Data Source={_testDbPath}";

        // Initialize XPO
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Reset any existing data layer
        ConnectionHelper.ResetDataLayer();
        
        // Create data layer with auto-create schema
        var dataStore = XpoDefault.GetConnectionProvider(ConnectionString, AutoCreateOption.DatabaseAndSchema);
        var dataLayer = new SimpleDataLayer(dataStore);
        
        // Set as default for XPO
        XpoDefault.DataLayer = dataLayer;

        // Create schema
        using var session = new Session(dataLayer);
        session.UpdateSchema(
            typeof(User),
            typeof(Project),
            typeof(TimeEntry),
            typeof(Payment),
            typeof(PayRate),
            typeof(Incidental)
        );
        session.CreateObjectTypeRecords(
            typeof(User),
            typeof(Project),
            typeof(TimeEntry),
            typeof(Payment),
            typeof(PayRate),
            typeof(Incidental)
        );
    }

    public void SeedTestData()
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);

        // Create test user
        var testUser = new User(uow)
        {
            Username = "testuser",
            PasswordHash = HashPassword("testpassword"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create test projects
        var project1 = new Project(uow)
        {
            Name = "Test Project 1",
            Description = "Description for test project 1",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var project2 = new Project(uow)
        {
            Name = "Test Project 2",
            Description = "Description for test project 2",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create test time entries
        var timeEntry1 = new TimeEntry(uow)
        {
            Project = project1,
            Date = DateTime.UtcNow.Date,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            AppliedPayRate = 50.0m,
            CreatedAt = DateTime.UtcNow
        };

        uow.CommitChanges();
    }

    public void ClearDatabase()
    {
        using var uow = new UnitOfWork(XpoDefault.DataLayer);
        
        // Delete all data in reverse order of dependencies
        uow.Delete(uow.Query<TimeEntry>());
        uow.Delete(uow.Query<Payment>());
        uow.Delete(uow.Query<PayRate>());
        uow.Delete(uow.Query<Incidental>());
        uow.Delete(uow.Query<Project>());
        uow.Delete(uow.Query<User>());
        
        uow.CommitChanges();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public void Dispose()
    {
        // Clean up test database file
        ConnectionHelper.ResetDataLayer();
        XpoDefault.DataLayer = null;
        
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
