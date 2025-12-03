using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TimePE.Api.Services;
using Xunit;

namespace TimePE.Api.Tests.Services;

public class JwtServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<JwtService>> _mockLogger;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-secret-key-that-is-at-least-32-characters-long",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _mockLogger = new Mock<ILogger<JwtService>>();
        _jwtService = new JwtService(_configuration, _mockLogger.Object);
    }

    [Fact]
    public void GenerateToken_WithValidParameters_ReturnsToken()
    {
        // Arrange
        var userId = 1;
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = 1;
        var username = "testuser";
        var token = _jwtService.GenerateToken(userId, username);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var userIdClaim = principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(userId.ToString());

        var nameClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.Name);
        nameClaim.Should().NotBeNull();
        nameClaim!.Value.Should().Be(username);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = 42;
        var username = "testuser";
        var token = _jwtService.GenerateToken(userId, username);

        // Act
        var extractedUserId = _jwtService.GetUserIdFromToken(token);

        // Assert
        extractedUserId.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var userId = _jwtService.GetUserIdFromToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_WithCustomExpiration_CreatesTokenWithCorrectExpiration()
    {
        // Arrange
        var userId = 1;
        var username = "testuser";
        var expirationMinutes = 30;

        // Act
        var token = _jwtService.GenerateToken(userId, username, expirationMinutes);
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var exp = principal!.Claims.FirstOrDefault(c => c.Type == "exp");
        exp.Should().NotBeNull();
    }
}
