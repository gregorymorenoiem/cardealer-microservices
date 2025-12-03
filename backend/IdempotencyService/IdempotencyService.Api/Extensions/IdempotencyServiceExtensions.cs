using IdempotencyService.Api.Filters;
using IdempotencyService.Api.Middleware;
using IdempotencyService.Core.Interfaces;
using IdempotencyService.Core.Models;
using IdempotencyService.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace IdempotencyService.Api.Extensions;

/// <summary>
/// Extension methods for configuring idempotency services
/// </summary>
public static class IdempotencyServiceExtensions
{
    /// <summary>
    /// Adds idempotency services to the service collection
    /// </summary>
    public static IServiceCollection AddIdempotencyServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<IdempotencyOptions>(
            configuration.GetSection(IdempotencyOptions.SectionName));

        // Add Redis cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "idempotency:";
        });

        // Register idempotency service
        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

        // Register action filter
        services.AddScoped<IdempotencyActionFilter>();

        return services;
    }

    /// <summary>
    /// Adds idempotency action filter to MVC options
    /// </summary>
    public static IMvcBuilder AddIdempotencyFilter(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options =>
        {
            options.Filters.AddService<IdempotencyActionFilter>();
        });

        return builder;
    }

    /// <summary>
    /// Adds complete idempotency support (services + filter)
    /// </summary>
    public static IServiceCollection AddIdempotency(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdempotencyServices(configuration);
        services.AddControllers().AddIdempotencyFilter();

        return services;
    }

    /// <summary>
    /// Uses idempotency middleware for all requests
    /// </summary>
    public static IApplicationBuilder UseIdempotencyMiddleware(
        this IApplicationBuilder app,
        Action<IdempotencyMiddlewareOptions>? configure = null)
    {
        var options = new IdempotencyMiddlewareOptions();
        configure?.Invoke(options);

        if (options.UseMiddleware)
        {
            app.UseMiddleware<IdempotencyMiddleware>();
        }

        return app;
    }
}

/// <summary>
/// Options for configuring idempotency middleware
/// </summary>
public class IdempotencyMiddlewareOptions
{
    /// <summary>
    /// Whether to use the middleware (default: false, use attributes instead)
    /// </summary>
    public bool UseMiddleware { get; set; } = false;

    /// <summary>
    /// Paths to exclude from middleware processing
    /// </summary>
    public List<string> ExcludePaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/metrics"
    };
}
