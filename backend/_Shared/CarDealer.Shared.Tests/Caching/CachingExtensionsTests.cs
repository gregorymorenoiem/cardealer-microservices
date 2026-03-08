using CarDealer.Shared.Caching.Extensions;
using CarDealer.Shared.Caching.Interfaces;
using CarDealer.Shared.Caching.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CarDealer.Shared.Tests.Caching;

/// <summary>
/// Tests for AddStandardCaching DI registration.
/// Validates service registration, options binding, and in-memory fallback.
/// </summary>
public class CachingExtensionsTests
{
    [Fact]
    public void AddStandardCaching_WithEmptyConfig_RegistersInMemoryFallback()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(config, "TestService");

        var provider = services.BuildServiceProvider();

        // ICacheService should be registered
        var cacheService = provider.GetService<ICacheService>();
        cacheService.Should().NotBeNull();

        // IDistributedCache should be the in-memory implementation
        var distributedCache = provider.GetService<IDistributedCache>();
        distributedCache.Should().NotBeNull();
    }

    [Fact]
    public void AddStandardCaching_SetsInstanceName_WithServicePrefix()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(config, "VehiclesSaleService");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

        options.InstanceName.Should().Be("okla:vehiclessaleservice:");
    }

    [Fact]
    public void AddStandardCaching_BindsConfigValues()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:DefaultTtlSeconds"] = "600",
                ["Caching:MaxTtlSeconds"] = "7200",
                ["Caching:EnableMetrics"] = "false",
                ["Caching:UseSlidingExpiration"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(config, "MyService");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

        options.DefaultTtlSeconds.Should().Be(600);
        options.MaxTtlSeconds.Should().Be(7200);
        options.EnableMetrics.Should().BeFalse();
        options.UseSlidingExpiration.Should().BeTrue();
    }

    [Fact]
    public void AddStandardCaching_StringOverload_RegistersCorrectly()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(
            redisConnectionString: "",
            serviceName: "ContactService",
            defaultTtlSeconds: 120);

        var provider = services.BuildServiceProvider();

        var cacheService = provider.GetService<ICacheService>();
        cacheService.Should().NotBeNull();

        var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
        options.InstanceName.Should().Be("okla:contactservice:");
        options.DefaultTtlSeconds.Should().Be(120);
    }

    [Fact]
    public void AddStandardCaching_IsIdempotent_DoesNotRegisterDuplicates()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(config, "TestService");
        services.AddStandardCaching(config, "TestService"); // second call

        var provider = services.BuildServiceProvider();

        // Should resolve without ambiguity exception
        var cacheService = provider.GetService<ICacheService>();
        cacheService.Should().NotBeNull();
    }

    [Fact]
    public void AddStandardCaching_WithRedisConfig_SetsConnectionString()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:RedisConnectionString"] = "redis.internal:6379"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(config, "AuthService");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

        options.RedisConnectionString.Should().Be("redis.internal:6379");
        options.InstanceName.Should().Be("okla:authservice:");
    }

    [Fact]
    public async Task AddStandardCaching_InMemoryFallback_WorksEndToEnd()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardCaching(config, "IntegrationTest");

        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetRequiredService<ICacheService>();

        // Full round-trip
        await cacheService.SetAsync("test:key", new { Name = "Test", Value = 42 });
        var exists = await cacheService.ExistsAsync("test:key");
        exists.Should().BeTrue();

        await cacheService.RemoveAsync("test:key");
        exists = await cacheService.ExistsAsync("test:key");
        exists.Should().BeFalse();
    }
}
