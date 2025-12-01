using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using TimePE.Core.Models;
using TimePE.Core.DTOs;

namespace TimePE.Core.Services;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(string name, string? description = null);
    Task<Project?> GetProjectByIdAsync(int id);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDeleted = false);
    Task<IEnumerable<ProjectSummaryDto>> GetAllProjectSummariesAsync(bool includeDeleted = false);
    Task UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(int id);
}

public class ProjectService : IProjectService
{
    private readonly string _connectionString;

    public ProjectService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Project> CreateProjectAsync(string name, string? description = null)
    {
        return await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var project = new Project(uow)
            {
                Name = name,
                Description = description,
                IsActive = true
            };
            uow.CommitChanges();
            return project;
        });
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return session.GetObjectByKey<Project>(id);
        });
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = CriteriaOperator.Parse("IsActive = True And DeletedAt Is Null");
            return new XPCollection<Project>(session, criteria).ToList();
        });
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDeleted = false)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = includeDeleted ? null : CriteriaOperator.Parse("DeletedAt Is Null");
            var projects = new XPCollection<Project>(session, criteria).ToList();
            
            // Force load TimeEntries count before session is disposed
            foreach (var project in projects)
            {
                _ = project.TimeEntries.Count;
            }
            
            return projects;
        });
    }

    public async Task<IEnumerable<ProjectSummaryDto>> GetAllProjectSummariesAsync(bool includeDeleted = false)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = includeDeleted ? null : CriteriaOperator.Parse("DeletedAt Is Null");
            var projects = new XPCollection<Project>(session, criteria);
            
            return projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Oid,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                TimeEntriesCount = p.TimeEntries.Count
            }).ToList();
        });
    }

    public async Task UpdateProjectAsync(Project project)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var existingProject = uow.GetObjectByKey<Project>(project.Oid);
            if (existingProject != null)
            {
                existingProject.Name = project.Name;
                existingProject.Description = project.Description;
                existingProject.IsActive = project.IsActive;
                existingProject.UpdatedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }

    public async Task DeleteProjectAsync(int id)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var project = uow.GetObjectByKey<Project>(id);
            if (project != null)
            {
                project.DeletedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }
}
