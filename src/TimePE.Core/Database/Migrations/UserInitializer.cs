using DevExpress.Xpo;
using Serilog;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Database.Migrations;

public class UserInitializer
{
    private readonly string _connectionString;

    public UserInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeDefaultUserAsync()
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        // Check if any user exists
        var userCount = await Task.Run(() => uow.Query<User>().Count());
        
        if (userCount == 0)
        {
            var authService = new AuthService(_connectionString);
            
            // Create default user: admin / admin123
            var success = await authService.CreateUserAsync("admin", "admin123");
            
            if (success)
            {
                Log.Information("Default user 'admin' created successfully");
                Console.WriteLine("==============================================");
                Console.WriteLine("Default user created:");
                Console.WriteLine("  Username: admin");
                Console.WriteLine("  Password: admin123");
                Console.WriteLine("==============================================");
            }
            else
            {
                Log.Warning("Failed to create default user");
            }
        }
        else
        {
            Log.Information("User account(s) already exist, skipping default user creation");
        }
    }
}
