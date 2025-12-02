using DevExpress.Xpo;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class ProjectServiceTests : IDisposable
{
    private readonly ProjectService _projectService;
    private readonly string _connectionString;
    private readonly IDataLayer _dataLayer;

    public ProjectServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        
        _projectService = new ProjectService(_connectionString);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldCreateNewProject()
    {
        // Arrange
        var projectName = "Test Project";
        var description = "Test Description";

        // Act
        var result = await _projectService.CreateProjectAsync(projectName, description);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(projectName);
        result.Description.Should().Be(description);
        result.Oid.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAllProjectsAsync_ShouldReturnOnlyNonDeletedProjects()
    {
        // Arrange
        await _projectService.CreateProjectAsync("Active Project 1");
        await _projectService.CreateProjectAsync("Active Project 2");
        var deletedProject = await _projectService.CreateProjectAsync("Deleted Project");
        await _projectService.DeleteProjectAsync(deletedProject.Oid);

        // Act
        var projects = await _projectService.GetAllProjectsAsync();

        // Assert
        projects.Should().HaveCount(2);
        projects.Should().NotContain(p => p.Name == "Deleted Project");
    }

    [Fact]
    public async Task GetProjectByIdAsync_ShouldReturnProject_WhenExists()
    {
        // Arrange
        var created = await _projectService.CreateProjectAsync("Find Me");

        // Act
        var found = await _projectService.GetProjectByIdAsync(created.Oid);

        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Find Me");
        found.Oid.Should().Be(created.Oid);
    }

    [Fact]
    public async Task GetProjectByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var found = await _projectService.GetProjectByIdAsync(99999);

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProjectAsync_ShouldSoftDeleteProject()
    {
        // Arrange
        var project = await _projectService.CreateProjectAsync("To Delete");
        var projectId = project.Oid;

        // Act
        await _projectService.DeleteProjectAsync(projectId);

        // Assert
        // Verify soft delete - project should not appear in GetAll
        var allProjects = await _projectService.GetAllProjectsAsync();
        allProjects.Should().NotContain(p => p.Oid == projectId);
    }

    [Fact]
    public async Task GetAllProjectSummariesAsync_ShouldReturnProjects()
    {
        // Arrange
        var project = await _projectService.CreateProjectAsync("Project with Summary");

        // Act
        var summaries = await _projectService.GetAllProjectSummariesAsync();

        // Assert
        summaries.Should().NotBeEmpty();
        summaries.Should().Contain(s => s.Name == "Project with Summary");
    }
}
