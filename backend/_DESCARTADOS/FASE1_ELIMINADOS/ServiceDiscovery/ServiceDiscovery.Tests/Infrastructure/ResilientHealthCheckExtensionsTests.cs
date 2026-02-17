using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Extensions;
using ServiceDiscovery.Infrastructure.Services;
using Xunit;

namespace ServiceDiscovery.Tests.Infrastructure;

/// <summary>
/// Tests for ResilientHealthCheckExtensions
/// </summary>
public class ResilientHealthCheckExtensionsTests
{
    [Fact]
    public void AddResilientHealthCheck_WithDefaultOptions_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Add mock for IServiceDiscovery
        services.AddSingleton<IServiceDiscovery>(sp =>
            new Moq.Mock<IServiceDiscovery>().Object);

        // Act
        services.AddResilientHealthCheck();
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<ResilientHealthCheckerOptions>();
        options.Should().NotBeNull();
        options!.MaxRetryAttempts.Should().Be(3);
        options.TimeoutSeconds.Should().Be(10);
    }

    [Fact]
    public void AddResilientHealthCheck_WithCustomOptions_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IServiceDiscovery>(sp =>
            new Moq.Mock<IServiceDiscovery>().Object);

        // Act
        services.AddResilientHealthCheck(options =>
        {
            options.MaxRetryAttempts = 5;
            options.TimeoutSeconds = 30;
            options.CircuitBreakerBreakDurationSeconds = 60;
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<ResilientHealthCheckerOptions>();
        options.Should().NotBeNull();
        options!.MaxRetryAttempts.Should().Be(5);
        options.TimeoutSeconds.Should().Be(30);
        options.CircuitBreakerBreakDurationSeconds.Should().Be(60);
    }

    [Fact]
    public void AddResilientHealthCheck_RegistersHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IServiceDiscovery>(sp =>
            new Moq.Mock<IServiceDiscovery>().Object);

        // Act
        services.AddResilientHealthCheck();
        var provider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = provider.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddResilientHealthCheck_RegistersHealthChecker()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IServiceDiscovery>(sp =>
            new Moq.Mock<IServiceDiscovery>().Object);

        // Act
        services.AddResilientHealthCheck();
        var provider = services.BuildServiceProvider();

        // Assert
        var healthChecker = provider.GetService<IHealthChecker>();
        healthChecker.Should().NotBeNull();
        healthChecker.Should().BeOfType<ResilientHealthChecker>();
    }

    [Fact]
    public void AddResilientHealthCheck_NullOptions_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IServiceDiscovery>(sp =>
            new Moq.Mock<IServiceDiscovery>().Object);

        // Act
        services.AddResilientHealthCheck(null);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<ResilientHealthCheckerOptions>();
        options.Should().NotBeNull();
        options!.MaxRetryAttempts.Should().Be(3); // Default
    }
}
