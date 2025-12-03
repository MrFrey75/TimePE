using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TimePE.Api.DTOs;
using Xunit;

namespace TimePE.Api.Tests.Integration;

/// <summary>
/// Integration tests for Projects CRUD operations
/// Tests complete request/response cycle with real database
/// </summary>
public class ProjectsIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _dbFixture;

    public ProjectsIntegrationTests(TestDatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    private async Task<string> GetAuthTokenAsync(HttpClient client)
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "TestPassword123!";
        
        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register",
            new CreateUserDto { Username = username, Password = password });
        
        var authResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        return authResult!.Token;
    }

    [Fact]
    public async Task GetProjects_WithAuthentication_ReturnsOkWithProjects()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();
        _dbFixture.SeedTestData();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        projects.Should().NotBeNull();
        projects!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProject_WithValidData_ReturnsCreated()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateProjectDto
        {
            Name = $"New Project {Guid.NewGuid()}",
            Description = "Test project description",
            IsActive = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/projects", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Name.Should().Be(createDto.Name);
        project.Description.Should().Be(createDto.Description);
        project.IsActive.Should().Be(createDto.IsActive);
    }

    [Fact]
    public async Task GetProject_WithValidId_ReturnsOkWithProject()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a project first
        var createDto = new CreateProjectDto
        {
            Name = "Test Project for Get",
            Description = "Description",
            IsActive = true
        };
        
        var createResponse = await client.PostAsJsonAsync("/api/v1/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await client.GetAsync($"/api/v1/projects/{createdProject!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        project.Should().NotBeNull();
        project!.Id.Should().Be(createdProject.Id);
        project.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task GetProject_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/projects/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProject_WithValidData_ReturnsNoContent()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a project first
        var createDto = new CreateProjectDto
        {
            Name = "Original Name",
            Description = "Original Description",
            IsActive = true
        };
        
        var createResponse = await client.PostAsJsonAsync("/api/v1/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Prepare update
        var updateDto = new UpdateProjectDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsActive = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/projects/{createdProject!.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await client.GetAsync($"/api/v1/projects/{createdProject.Id}");
        var updatedProject = await getResponse.Content.ReadFromJsonAsync<ProjectDto>();
        updatedProject!.Name.Should().Be(updateDto.Name);
        updatedProject.Description.Should().Be(updateDto.Description);
        updatedProject.IsActive.Should().Be(updateDto.IsActive ?? false);
    }

    [Fact]
    public async Task DeleteProject_WithValidId_ReturnsNoContent()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a project first
        var createDto = new CreateProjectDto
        {
            Name = "Project to Delete",
            Description = "Will be deleted",
            IsActive = true
        };
        
        var createResponse = await client.PostAsJsonAsync("/api/v1/projects", createDto);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act
        var response = await client.DeleteAsync($"/api/v1/projects/{createdProject!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion (soft delete, so should return NotFound)
        var getResponse = await client.GetAsync($"/api/v1/projects/{createdProject.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteCRUDFlow_CreateReadUpdateDelete_AllSucceed()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var token = await GetAuthTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act 1: Create
        var createDto = new CreateProjectDto
        {
            Name = "CRUD Flow Project",
            Description = "Testing complete flow",
            IsActive = true
        };
        
        var createResponse = await client.PostAsJsonAsync("/api/v1/projects", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var project = await createResponse.Content.ReadFromJsonAsync<ProjectDto>();

        // Act 2: Read
        var readResponse = await client.GetAsync($"/api/v1/projects/{project!.Id}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 3: Update
        var updateDto = new UpdateProjectDto
        {
            Name = "Updated CRUD Project",
            Description = "Updated description",
            IsActive = false
        };
        var updateResponse = await client.PutAsJsonAsync($"/api/v1/projects/{project.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act 4: Delete
        var deleteResponse = await client.DeleteAsync($"/api/v1/projects/{project.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: Verify deleted
        var finalReadResponse = await client.GetAsync($"/api/v1/projects/{project.Id}");
        finalReadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
