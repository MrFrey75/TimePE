using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TimePE.Api.DTOs;
using TimePE.Api.Middleware;
using Xunit;

namespace TimePE.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;

    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentException_ReturnsBadRequest()
    {
        // Arrange
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new ArgumentException("Invalid argument");
        };

        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = CreateServiceProvider();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var error = JsonSerializer.Deserialize<ErrorResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        error.Should().NotBeNull();
        error!.StatusCode.Should().Be(400);
        error.Message.Should().Be("Invalid request.");
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new KeyNotFoundException("Resource not found");
        };

        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = CreateServiceProvider();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(404);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var error = JsonSerializer.Deserialize<ErrorResponseDto>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        error.Should().NotBeNull();
        error!.StatusCode.Should().Be(404);
        error.Message.Should().Be("Resource not found.");
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new UnauthorizedAccessException();
        };

        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = CreateServiceProvider();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_ReturnsInternalServerError()
    {
        // Arrange
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new Exception("Unexpected error");
        };

        var middleware = new ExceptionHandlingMiddleware(next, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = CreateServiceProvider();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }

    private IServiceProvider CreateServiceProvider()
    {
        var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)))
            .Returns(mockEnv.Object);

        return services.Object;
    }
}
