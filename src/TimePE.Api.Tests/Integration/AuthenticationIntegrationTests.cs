using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TimePE.Api.DTOs;
using Xunit;

namespace TimePE.Api.Tests.Integration;

/// <summary>
/// Integration tests for authentication endpoints
/// Tests the complete authentication flow with real database
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _dbFixture;

    public AuthenticationIntegrationTests(TestDatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Fact]
    public async Task Register_WithValidCredentials_ReturnsCreatedAndToken()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var registerDto = new CreateUserDto
        {
            Username = $"newuser_{Guid.NewGuid()}",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Username.Should().Be(registerDto.Username);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var username = $"duplicateuser_{Guid.NewGuid()}";
        var registerDto = new CreateUserDto { Username = username, Password = "Password123!" };

        // Register first user
        await client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Act - Try to register same username again
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkAndToken()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var username = $"loginuser_{Guid.NewGuid()}";
        var password = "LoginPassword123!";
        
        // Register user first
        await client.PostAsJsonAsync("/api/v1/auth/register", 
            new CreateUserDto { Username = username, Password = password });

        var loginDto = new LoginRequestDto { Username = username, Password = password };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Username.Should().Be(username);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var username = $"testuser_{Guid.NewGuid()}";
        
        // Register user
        await client.PostAsJsonAsync("/api/v1/auth/register",
            new CreateUserDto { Username = username, Password = "CorrectPassword123!" });

        var loginDto = new LoginRequestDto { Username = username, Password = "WrongPassword123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var loginDto = new LoginRequestDto 
        { 
            Username = "nonexistent_user", 
            Password = "SomePassword123!" 
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthenticationFlow_RegisterLoginAndAccessProtectedEndpoint_Succeeds()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();
        _dbFixture.ClearDatabase();

        var username = $"flowuser_{Guid.NewGuid()}";
        var password = "FlowPassword123!";

        // Act 1: Register
        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register",
            new CreateUserDto { Username = username, Password = password });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        var token = registerResult!.Token;

        // Act 2: Access protected endpoint with token
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var projectsResponse = await client.GetAsync("/api/v1/projects");

        // Assert
        projectsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        await using var factory = new TimePEApiFactory(_dbFixture);
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await client.GetAsync("/api/v1/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
