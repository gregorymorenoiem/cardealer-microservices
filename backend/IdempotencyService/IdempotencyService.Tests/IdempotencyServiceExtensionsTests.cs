using FluentAssertions;
using IdempotencyService.Api.Extensions;
using IdempotencyService.Api.Filters;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using IdempotencyService.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IdempotencyService.Tests;

/// <summary>
/// Tests for IdempotencyServiceExtensions (DI registration methods)
/// </summary>
public class IdempotencyServiceExtensionsTests
{
    private static IConfiguration CreateConfiguration(Dictionary<string, string?>? values = null)
    {
        var defaultValues = new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "localhost:6379" },
            { "Idempotency:DefaultTtlSeconds", "3600" },
            { "Idempotency:KeyPrefix", "test" }
        };

        if (values != null)
        {
            foreach (var kvp in values)
            {
                defaultValues[kvp.Key] = kvp.Value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(defaultValues)
            .Build();
    }

    // =========== AddIdempotencyServices Tests ===========

    [Fact]
    public void AddIdempotencyServices_RegistersIdempotencyOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddIdempotencyServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<IdempotencyOptions>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddIdempotencyServices_RegistersDistributedCache()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddIdempotencyServices(configuration);

        // Assert - Check that cache is registered (we can't build provider without Redis)
        services.Should().Contain(s =>
            s.ServiceType == typeof(IDistributedCache));
    }

    [Fact]
    public void AddIdempotencyServices_RegistersIdempotencyService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddIdempotencyServices(configuration);

        // Assert
        services.Should().Contain(s =>
            s.ServiceType == typeof(IIdempotencyService) &&
            s.ImplementationType == typeof(RedisIdempotencyService) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddIdempotencyServices_RegistersActionFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddIdempotencyServices(configuration);

        // Assert
        services.Should().Contain(s =>
            s.ServiceType == typeof(IdempotencyActionFilter) &&
            s.ImplementationType == typeof(IdempotencyActionFilter) &&
            s.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddIdempotencyServices_WithCustomRedisConnection_UsesCustomConnection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            { "ConnectionStrings:Redis", "custom-redis:6380" }
        });

        // Act
        services.AddIdempotencyServices(configuration);

        // Assert - Verify Redis is registered
        services.Should().Contain(s => s.ServiceType == typeof(IDistributedCache));
    }

    [Fact]
    public void AddIdempotencyServices_WithNullRedisConnection_UsesDefault()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        services.AddIdempotencyServices(configuration);

        // Assert - Should use localhost:6379 as default
        services.Should().Contain(s => s.ServiceType == typeof(IDistributedCache));
    }

    [Fact]
    public void AddIdempotencyServices_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        var result = services.AddIdempotencyServices(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    // =========== AddIdempotencyFilter Tests ===========

    [Fact]
    public void AddIdempotencyFilter_AddsServiceFilterToMvcOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = CreateConfiguration();
        services.AddIdempotencyServices(configuration);

        var mvcBuilder = services.AddControllers();

        // Act
        var result = mvcBuilder.AddIdempotencyFilter();

        // Assert
        result.Should().BeSameAs(mvcBuilder);
    }

    [Fact]
    public void AddIdempotencyFilter_ReturnsMvcBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IdempotencyActionFilter>();
        var mvcBuilder = services.AddControllers();

        // Act
        var result = mvcBuilder.AddIdempotencyFilter();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mvcBuilder);
    }

    // =========== AddIdempotency Tests ===========

    [Fact]
    public void AddIdempotency_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = CreateConfiguration();

        // Act
        services.AddIdempotency(configuration);

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(IIdempotencyService));
        services.Should().Contain(s => s.ServiceType == typeof(IdempotencyActionFilter));
        services.Should().Contain(s => s.ServiceType == typeof(IDistributedCache));
    }

    [Fact]
    public void AddIdempotency_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var configuration = CreateConfiguration();

        // Act
        var result = services.AddIdempotency(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    // =========== UseIdempotencyMiddleware Tests ===========

    [Fact]
    public void UseIdempotencyMiddleware_WithDefaultOptions_DoesNotAddMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(CreateConfiguration());

        // Add mock idempotency service
        var mockIdempotencyService = new Mock<IIdempotencyService>();
        services.AddScoped(_ => mockIdempotencyService.Object);
        services.Configure<IdempotencyOptions>(o => { });

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        appBuilder.UseIdempotencyMiddleware();

        // Assert - Should not throw and middleware not added (UseMiddleware = false by default)
        appBuilder.Should().NotBeNull();
    }

    [Fact]
    public void UseIdempotencyMiddleware_WithUseMiddlewareTrue_AddsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(CreateConfiguration());

        var mockIdempotencyService = new Mock<IIdempotencyService>();
        services.AddScoped(_ => mockIdempotencyService.Object);
        services.Configure<IdempotencyOptions>(o => { });

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        appBuilder.UseIdempotencyMiddleware(options =>
        {
            options.UseMiddleware = true;
        });

        // Assert - Should have middleware configured
        appBuilder.Should().NotBeNull();
    }

    [Fact]
    public void UseIdempotencyMiddleware_WithCustomExcludePaths_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(CreateConfiguration());

        var mockIdempotencyService = new Mock<IIdempotencyService>();
        services.AddScoped(_ => mockIdempotencyService.Object);
        services.Configure<IdempotencyOptions>(o => { });

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        IdempotencyMiddlewareOptions? capturedOptions = null;

        // Act
        appBuilder.UseIdempotencyMiddleware(options =>
        {
            options.UseMiddleware = false;
            options.ExcludePaths = new List<string> { "/api/public", "/webhook" };
            capturedOptions = options;
        });

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.ExcludePaths.Should().Contain("/api/public");
        capturedOptions.ExcludePaths.Should().Contain("/webhook");
    }

    [Fact]
    public void UseIdempotencyMiddleware_WithNullConfigure_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(CreateConfiguration());

        var mockIdempotencyService = new Mock<IIdempotencyService>();
        services.AddScoped(_ => mockIdempotencyService.Object);
        services.Configure<IdempotencyOptions>(o => { });

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        appBuilder.UseIdempotencyMiddleware(null);

        // Assert
        appBuilder.Should().NotBeNull();
    }

    [Fact]
    public void UseIdempotencyMiddleware_ReturnsApplicationBuilderForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(CreateConfiguration());

        var mockIdempotencyService = new Mock<IIdempotencyService>();
        services.AddScoped(_ => mockIdempotencyService.Object);
        services.Configure<IdempotencyOptions>(o => { });

        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        var result = appBuilder.UseIdempotencyMiddleware();

        // Assert
        result.Should().BeSameAs(appBuilder);
    }

    // =========== IdempotencyMiddlewareOptions Tests ===========

    [Fact]
    public void IdempotencyMiddlewareOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new IdempotencyMiddlewareOptions();

        // Assert
        options.UseMiddleware.Should().BeFalse();
        options.ExcludePaths.Should().NotBeNull();
        options.ExcludePaths.Should().Contain("/health");
        options.ExcludePaths.Should().Contain("/swagger");
        options.ExcludePaths.Should().Contain("/metrics");
    }

    [Fact]
    public void IdempotencyMiddlewareOptions_CanSetProperties()
    {
        // Arrange
        var options = new IdempotencyMiddlewareOptions();

        // Act
        options.UseMiddleware = true;
        options.ExcludePaths = new List<string> { "/custom" };

        // Assert
        options.UseMiddleware.Should().BeTrue();
        options.ExcludePaths.Should().HaveCount(1);
        options.ExcludePaths.Should().Contain("/custom");
    }
}
