using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Net.Http.Json;
using TimePE.Api.DTOs;
using TimePE.Api.Tests.Integration;
using TimePE.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace TimePE.Api.Tests.Performance;

/// <summary>
/// Performance benchmarks for API endpoints
/// Run with: dotnet run -c Release --project TimePE.Api.Tests -- --filter *ApiPerformanceBenchmarks*
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class ApiPerformanceBenchmarks
{
    private TestDatabaseFixture _dbFixture = null!;
    private TimePEApiFactory _factory = null!;
    private HttpClient _client = null!;
    private string _authToken = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _dbFixture = new TestDatabaseFixture();
        _dbFixture.SeedTestData();
        
        _factory = new TimePEApiFactory(_dbFixture);
        _client = _factory.CreateClient();

        // Get auth token
        var username = $"benchuser_{Guid.NewGuid()}";
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register",
            new CreateUserDto { Username = username, Password = "BenchPassword123!" });
        var authResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        _authToken = authResult!.Token;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _client.Dispose();
        _factory.Dispose();
        _dbFixture.Dispose();
    }

    [Benchmark(Description = "GET /api/v1/projects - List all projects")]
    public async Task<List<ProjectDto>?> GetProjects()
    {
        var response = await _client.GetAsync("/api/v1/projects");
        return await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
    }

    [Benchmark(Description = "GET /api/v1/projects/{id} - Get single project")]
    public async Task<ProjectDto?> GetProjectById()
    {
        var response = await _client.GetAsync("/api/v1/projects/1");
        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }

    [Benchmark(Description = "POST /api/v1/projects - Create project")]
    public async Task<ProjectDto?> CreateProject()
    {
        var createDto = new CreateProjectDto
        {
            Name = $"Bench Project {Guid.NewGuid()}",
            Description = "Performance test project",
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/projects", createDto);
        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }

    [Benchmark(Description = "POST /api/v1/auth/login - User authentication")]
    public async Task<LoginResponseDto?> Login()
    {
        var loginDto = new LoginRequestDto
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
    }

    [Benchmark(Description = "GET /api/v1/timeentries - List time entries")]
    public async Task<List<TimeEntryDto>?> GetTimeEntries()
    {
        var response = await _client.GetAsync("/api/v1/timeentries");
        return await response.Content.ReadFromJsonAsync<List<TimeEntryDto>>();
    }
}

/// <summary>
/// Benchmarks for JWT service performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class JwtServiceBenchmarks
{
    private JwtService _jwtService = null!;
    private string _validToken = null!;

    [GlobalSetup]
    public void Setup()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "benchmark-secret-key-that-is-at-least-32-characters-long",
                ["Jwt:Issuer"] = "BenchmarkIssuer",
                ["Jwt:Audience"] = "BenchmarkAudience"
            }!)
            .Build();

        var logger = new NullLogger<JwtService>();
        _jwtService = new JwtService(configuration, logger);
        _validToken = _jwtService.GenerateToken(1, "benchuser");
    }

    [Benchmark(Description = "Generate JWT token")]
    public string GenerateToken()
    {
        return _jwtService.GenerateToken(1, "benchuser");
    }

    [Benchmark(Description = "Validate JWT token")]
    public System.Security.Claims.ClaimsPrincipal? ValidateToken()
    {
        return _jwtService.ValidateToken(_validToken);
    }

    [Benchmark(Description = "Extract user ID from token")]
    public int? GetUserIdFromToken()
    {
        return _jwtService.GetUserIdFromToken(_validToken);
    }
}

// Note: Run benchmarks with: dotnet run -c Release --filter="*" -- --benchmark
// Or use BenchmarkDotNet CLI: dotnet run -c Release --project TimePE.Api.Tests --filter="*Performance*"
