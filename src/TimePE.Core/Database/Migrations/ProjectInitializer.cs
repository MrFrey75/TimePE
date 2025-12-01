using DevExpress.Xpo;
using Serilog;
using TimePE.Core.Models;

namespace TimePE.Core.Database.Migrations;

public class ProjectInitializer
{
    private readonly string _connectionString;

    public ProjectInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeDefaultProjectsAsync()
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        // Check if General project exists
        var generalProject = await Task.Run(() => uow.Query<Project>()
            .FirstOrDefault(p => p.Name == "General" && p.DeletedAt == null));

        if (generalProject == null)
        {
            var project = new Project(uow)
            {
                Name = "General",
                Description = "Default general project for miscellaneous work",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await uow.CommitChangesAsync();
            Log.Information("Default 'General' project created successfully");
        }
        else
        {
            Log.Information("Default 'General' project already exists");
        }
    }
}
