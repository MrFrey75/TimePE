using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using TimePE.Core.Models;
using TimePE.Core.DTOs;

namespace TimePE.Core.Services;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(string name, string? description = null, Address? address = null);
    Task<Project?> GetProjectByIdAsync(int id);
    Task<ProjectDeleteViewModel?> GetProjectDeleteInfoAsync(int id);
    Task<IEnumerable<Project>> GetActiveProjectsAsync();
    Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDeleted = false);
    Task<IEnumerable<ProjectSummaryDto>> GetAllProjectSummariesAsync(bool includeDeleted = false);
    Task UpdateProjectAsync(Project project);
    Task UpdateProjectWithAddressAsync(int projectId, string name, string? description, bool isActive, Address? address);
    Task DeleteProjectAsync(int id);
}

public class ProjectDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TimeEntriesCount { get; set; }
    public Address? Address { get; set; }
}

public class ProjectService : IProjectService
{
    private readonly string _connectionString;

    public ProjectService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Project> CreateProjectAsync(string name, string? description = null, Address? address = null)
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
            
            if (address != null)
            {
                var projectAddress = new Address(uow)
                {
                    StreetLine1 = address.StreetLine1,
                    StreetLine2 = address.StreetLine2,
                    City = address.City,
                    StateProvince = address.StateProvince,
                    PostalCode = address.PostalCode,
                    AddressType = address.AddressType
                };
                project.Address = projectAddress;
            }
            
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

    public async Task<ProjectDeleteViewModel?> GetProjectDeleteInfoAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var project = session.GetObjectByKey<Project>(id);
            if (project == null)
                return null;

            // Load the collection count while the session is still active
            var timeEntriesCount = project.TimeEntries?.Count ?? 0;

            return new ProjectDeleteViewModel
            {
                Id = project.Oid,
                Name = project.Name,
                Description = project.Description,
                IsActive = project.IsActive,
                TimeEntriesCount = timeEntriesCount,
                Address = project.Address
            };
        });
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync()
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            // XPO automatically filters out soft-deleted records (GCRecord != null)
            var criteria = CriteriaOperator.Parse("IsActive = True");
            return new XPCollection<Project>(session, criteria).ToList();
        });
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(bool includeDeleted = false)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            // Note: With DeferredDeletion(true), XPO automatically filters deleted records
            // To include deleted items, we need to query differently or use session.Delete functionality
            // For now, includeDeleted parameter won't work with XPO's automatic filtering
            var projects = new XPCollection<Project>(session).ToList();
            
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
            // XPO automatically filters deleted records with DeferredDeletion(true)
            var projects = new XPCollection<Project>(session);
            
            return projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Oid,
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                TimeEntriesCount = p.TimeEntries.Count,
                Address = p.Address
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

    public async Task UpdateProjectWithAddressAsync(int projectId, string name, string? description, bool isActive, Address? address)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var existingProject = uow.GetObjectByKey<Project>(projectId);
            if (existingProject != null)
            {
                existingProject.Name = name;
                existingProject.Description = description;
                existingProject.IsActive = isActive;
                existingProject.UpdatedAt = DateTime.UtcNow;
                
                if (address != null)
                {
                    if (existingProject.Address == null)
                    {
                        // Create new address
                        existingProject.Address = new Address(uow)
                        {
                            StreetLine1 = address.StreetLine1,
                            StreetLine2 = address.StreetLine2,
                            City = address.City,
                            StateProvince = address.StateProvince,
                            PostalCode = address.PostalCode,
                            AddressType = address.AddressType
                        };
                    }
                    else
                    {
                        // Update existing address
                        existingProject.Address.StreetLine1 = address.StreetLine1;
                        existingProject.Address.StreetLine2 = address.StreetLine2;
                        existingProject.Address.City = address.City;
                        existingProject.Address.StateProvince = address.StateProvince;
                        existingProject.Address.PostalCode = address.PostalCode;
                        existingProject.Address.AddressType = address.AddressType;
                        existingProject.Address.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else if (existingProject.Address != null)
                {
                    // Remove address if null is passed
                    existingProject.Address.Delete();
                    existingProject.Address = null;
                }
                
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
                // XPO's built-in soft delete - sets GCRecord automatically
                project.Delete();
                uow.CommitChanges();
            }
        });
    }
}
