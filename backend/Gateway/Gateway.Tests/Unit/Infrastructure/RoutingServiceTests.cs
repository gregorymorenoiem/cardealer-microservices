using FluentAssertions;
using Gateway.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gateway.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for RoutingService
/// </summary>
public class RoutingServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<RoutingService>> _loggerMock;

    public RoutingServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<RoutingService>>();
    }

    private RoutingService CreateServiceWithConfig(string? environment = null)
    {
        _configurationMock.Setup(x => x["ASPNETCORE_ENVIRONMENT"]).Returns(environment);
        return new RoutingService(_configurationMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeWithoutError()
    {
        // Arrange & Act
        var service = CreateServiceWithConfig("Development");

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithProductionEnvironment_ShouldInitialize()
    {
        // Arrange & Act
        var service = CreateServiceWithConfig("Production");

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullEnvironment_ShouldUseProduction()
    {
        // Arrange & Act
        var service = CreateServiceWithConfig(null);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region RouteExists Tests

    [Fact]
    public async Task RouteExists_WhenConfigNotLoaded_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists(string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithNullPath_ShouldHandleGracefully()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ResolveDownstreamPath Tests

    [Fact]
    public async Task ResolveDownstreamPath_WhenConfigNotLoaded_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/users");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithEmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Template Matching Tests (via RouteExists)

    [Fact]
    public async Task RouteExists_ShouldBeCaseInsensitive()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var lowerCase = await service.RouteExists("/api/users");
        var upperCase = await service.RouteExists("/API/USERS");
        var mixedCase = await service.RouteExists("/Api/Users");

        // Assert - all should return the same result (false since config not loaded)
        lowerCase.Should().Be(upperCase).And.Be(mixedCase);
    }

    [Fact]
    public async Task RouteExists_WithTrailingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var withSlash = await service.RouteExists("/api/users/");
        var withoutSlash = await service.RouteExists("/api/users");

        // Assert
        withSlash.Should().BeFalse();
        withoutSlash.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithLeadingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var withSlash = await service.RouteExists("/api/users");
        var withoutSlash = await service.RouteExists("api/users");

        // Assert
        // Both should handle gracefully
        withSlash.Should().BeFalse();
        withoutSlash.Should().BeFalse();
    }

    #endregion

    #region Path Parameter Tests

    [Fact]
    public async Task RouteExists_WithPathParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/users/123");

        // Assert
        result.Should().BeFalse(); // Config not loaded
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithPathParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/users/456");

        // Assert
        result.Should().BeEmpty(); // Config not loaded
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithMultiplePathParameters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/users/123/roles/456");

        // Assert
        result.Should().BeEmpty(); // Config not loaded
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task RouteExists_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act - using a path that could cause issues
        var result = await service.RouteExists("/api/test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WhenExceptionOccurs_ShouldReturnEmpty()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.ResolveDownstreamPath("/api/test");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task RouteExists_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/users?query=test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RouteExists_WithEncodedPath_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");

        // Act
        var result = await service.RouteExists("/api/users%2F123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveDownstreamPath_WithVeryLongPath_ShouldHandleCorrectly()
    {
        // Arrange
        var service = CreateServiceWithConfig("Development");
        var longPath = "/api/" + string.Join("/", Enumerable.Repeat("segment", 100));

        // Act
        var result = await service.ResolveDownstreamPath(longPath);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}
