using CarDealer.Shared.Caching.Interfaces;
using CarDealer.Shared.Caching.Models;
using CarDealer.Shared.Caching.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CarDealer.Shared.Caching.Extensions;

/// <summary>
/// DI extensions for the standard OKLA caching layer.
/// 
/// Usage in Program.cs:
///   builder.Services.AddStandardCaching(builder.Configuration, "VehiclesSaleService");
/// 
/// Configuration in appsettings.json:
///   "Caching": {
///     "RedisConnectionString": "redis:6379",
///     "DefaultTtlSeconds": 300,
///     "MaxTtlSeconds": 86400,
///     "EnableMetrics": true
///   }
/// 
/// Environment variable fallback:
///   REDIS_CONNECTION_STRING (from K8s secrets)
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Registers the standard caching layer with Redis support and in-memory fallback.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="serviceName">Service name for key prefixing (e.g., "VehiclesSaleService").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddStandardCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        // Bind options
        var cacheSection = configuration.GetSection(CacheOptions.SectionName);
        services.Configure<CacheOptions>(cacheSection);

        var options = cacheSection.Get<CacheOptions>() ?? new CacheOptions();

        // Resolve Redis connection string: config → env var → empty (fallback)
        var redisConnection = !string.IsNullOrEmpty(options.RedisConnectionString)
            ? options.RedisConnectionString
            : Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
              ?? configuration.GetConnectionString("Redis")
              ?? string.Empty;

        // Set instance name with service prefix
        var instanceName = $"okla:{serviceName.ToLowerInvariant()}:";

        if (!string.IsNullOrEmpty(redisConnection))
        {
            // Register Redis distributed cache
            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = redisConnection;
                opts.InstanceName = instanceName;
            });

            // Register IConnectionMultiplexer for pattern-based invalidation
            services.TryAddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
                try
                {
                    var configOptions = ConfigurationOptions.Parse(redisConnection);
                    configOptions.AbortOnConnectFail = false;
                    configOptions.ConnectRetry = 3;
                    configOptions.ConnectTimeout = 5000;
                    return ConnectionMultiplexer.Connect(configOptions);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to connect to Redis at {Connection}. Pattern invalidation will be unavailable.", redisConnection);
                    return null!;
                }
            });
        }
        else
        {
            // Fallback to in-memory distributed cache (single-pod only)
            services.AddDistributedMemoryCache();
        }

        // Override InstanceName in options
        services.PostConfigure<CacheOptions>(opts =>
        {
            opts.InstanceName = instanceName;
            if (!string.IsNullOrEmpty(redisConnection))
                opts.RedisConnectionString = redisConnection;
        });

        // Register ICacheService
        services.TryAddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Registers caching with explicit Redis connection string (no config binding).
    /// Useful for tests or simple setups.
    /// </summary>
    public static IServiceCollection AddStandardCaching(
        this IServiceCollection services,
        string redisConnectionString,
        string serviceName,
        int defaultTtlSeconds = 300)
    {
        var instanceName = $"okla:{serviceName.ToLowerInvariant()}:";

        services.Configure<CacheOptions>(opts =>
        {
            opts.RedisConnectionString = redisConnectionString;
            opts.InstanceName = instanceName;
            opts.DefaultTtlSeconds = defaultTtlSeconds;
        });

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = redisConnectionString;
                opts.InstanceName = instanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.TryAddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
