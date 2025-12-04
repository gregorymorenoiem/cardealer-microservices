using FluentAssertions;
using Gateway.Domain.Entities;
using Xunit;

namespace Gateway.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Route entity and related classes
/// </summary>
public class RouteTests
{
    [Fact]
    public void Route_DefaultConstructor_ShouldHaveDefaultValues()
    {
        // Act
        var route = new Route();

        // Assert
        route.DownstreamPathTemplate.Should().BeEmpty();
        route.UpstreamPathTemplate.Should().BeEmpty();
        route.UpstreamHttpMethod.Should().BeEmpty();
        route.DownstreamScheme.Should().Be("http");
        route.DownstreamHostAndPort.Should().BeEmpty();
        route.ServiceName.Should().BeEmpty();
        route.Options.Should().NotBeNull();
    }

    [Fact]
    public void Route_WithValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var route = new Route
        {
            DownstreamPathTemplate = "/api/users/{id}",
            UpstreamPathTemplate = "/users/{id}",
            UpstreamHttpMethod = new[] { "GET", "POST" },
            DownstreamScheme = "https",
            DownstreamHostAndPort = "userservice:5001",
            ServiceName = "UserService"
        };

        // Assert
        route.DownstreamPathTemplate.Should().Be("/api/users/{id}");
        route.UpstreamPathTemplate.Should().Be("/users/{id}");
        route.UpstreamHttpMethod.Should().BeEquivalentTo(new[] { "GET", "POST" });
        route.DownstreamScheme.Should().Be("https");
        route.DownstreamHostAndPort.Should().Be("userservice:5001");
        route.ServiceName.Should().Be("UserService");
    }

    [Fact]
    public void Route_Options_ShouldBeInitialized()
    {
        // Act
        var route = new Route();

        // Assert
        route.Options.Should().NotBeNull();
        route.Options.RequiresAuthentication.Should().BeFalse();
        route.Options.AllowedRoles.Should().BeEmpty();
        route.Options.TimeoutMs.Should().Be(30000);
        route.Options.RateLimit.Should().BeNull();
    }

    [Fact]
    public void Route_CanSetCustomOptions()
    {
        // Arrange
        var options = new RouteOptions
        {
            RequiresAuthentication = true,
            AllowedRoles = new[] { "Admin", "Manager" },
            TimeoutMs = 60000,
            RateLimit = new RateLimitOptions { Limit = 100, PeriodSeconds = 60 }
        };

        // Act
        var route = new Route { Options = options };

        // Assert
        route.Options.RequiresAuthentication.Should().BeTrue();
        route.Options.AllowedRoles.Should().BeEquivalentTo(new[] { "Admin", "Manager" });
        route.Options.TimeoutMs.Should().Be(60000);
        route.Options.RateLimit.Should().NotBeNull();
        route.Options.RateLimit!.Limit.Should().Be(100);
        route.Options.RateLimit.PeriodSeconds.Should().Be(60);
    }
}

/// <summary>
/// Unit tests for RouteOptions class
/// </summary>
public class RouteOptionsTests
{
    [Fact]
    public void RouteOptions_DefaultConstructor_ShouldHaveDefaultValues()
    {
        // Act
        var options = new RouteOptions();

        // Assert
        options.RequiresAuthentication.Should().BeFalse();
        options.AllowedRoles.Should().BeEmpty();
        options.TimeoutMs.Should().Be(30000);
        options.RateLimit.Should().BeNull();
    }

    [Fact]
    public void RouteOptions_CanSetAuthentication()
    {
        // Act
        var options = new RouteOptions
        {
            RequiresAuthentication = true
        };

        // Assert
        options.RequiresAuthentication.Should().BeTrue();
    }

    [Fact]
    public void RouteOptions_CanSetMultipleRoles()
    {
        // Act
        var options = new RouteOptions
        {
            AllowedRoles = new[] { "Admin", "Manager", "User" }
        };

        // Assert
        options.AllowedRoles.Should().HaveCount(3);
        options.AllowedRoles.Should().Contain("Admin");
        options.AllowedRoles.Should().Contain("Manager");
        options.AllowedRoles.Should().Contain("User");
    }

    [Fact]
    public void RouteOptions_CanSetCustomTimeout()
    {
        // Act
        var options = new RouteOptions
        {
            TimeoutMs = 120000
        };

        // Assert
        options.TimeoutMs.Should().Be(120000);
    }
}

/// <summary>
/// Unit tests for RateLimitOptions class
/// </summary>
public class RateLimitOptionsTests
{
    [Fact]
    public void RateLimitOptions_DefaultConstructor_ShouldHaveZeroValues()
    {
        // Act
        var rateLimit = new RateLimitOptions();

        // Assert
        rateLimit.Limit.Should().Be(0);
        rateLimit.PeriodSeconds.Should().Be(0);
    }

    [Fact]
    public void RateLimitOptions_CanSetLimitAndPeriod()
    {
        // Act
        var rateLimit = new RateLimitOptions
        {
            Limit = 1000,
            PeriodSeconds = 3600
        };

        // Assert
        rateLimit.Limit.Should().Be(1000);
        rateLimit.PeriodSeconds.Should().Be(3600);
    }

    [Fact]
    public void RateLimitOptions_CanBeAttachedToRouteOptions()
    {
        // Arrange
        var rateLimit = new RateLimitOptions
        {
            Limit = 50,
            PeriodSeconds = 30
        };

        // Act
        var options = new RouteOptions
        {
            RateLimit = rateLimit
        };

        // Assert
        options.RateLimit.Should().NotBeNull();
        options.RateLimit!.Limit.Should().Be(50);
        options.RateLimit.PeriodSeconds.Should().Be(30);
    }
}
