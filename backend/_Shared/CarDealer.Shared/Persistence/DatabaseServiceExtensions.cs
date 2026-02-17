using CarDealer.Shared.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Persistence;

/// <summary>
/// Centralized extension to register a DbContext with ALL recommended database features:
/// - EnableRetryOnFailure (always)
/// - PostgreSQL Health Check (always)
/// - AuditableEntityInterceptor (auto timestamps, soft delete, concurrency)
/// - SlowQueryInterceptor (log queries > threshold)
/// - Auto-migration service (if configured)
/// 
/// Usage in Program.cs:
///   builder.Services.AddOklaDatabaseServices&lt;MyDbContext&gt;(builder.Configuration);
/// </summary>
public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Registers a DbContext with full OKLA database stack.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to register.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="connectionStringKey">Key to read connection string. Supports "ConnectionStrings:X" or "Database" section.</param>
    /// <param name="slowQueryThresholdMs">Slow query threshold in milliseconds (default: 500).</param>
    public static IServiceCollection AddOklaDatabaseServices<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? connectionStringKey = null,
        int slowQueryThresholdMs = 500)
        where TContext : DbContext
    {
        // Resolve connection string from multiple possible sources
        var connectionString = ResolveConnectionString(configuration, connectionStringKey);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"No database connection string found for {typeof(TContext).Name}. " +
                "Configure via 'Database:ConnectionStrings:PostgreSQL', 'ConnectionStrings:DefaultConnection', " +
                "or environment variable 'DATABASE_CONNECTION_STRING'.");
        }

        // Read optional config
        var dbSection = configuration.GetSection("Database");
        var maxRetryCount = dbSection.GetValue("MaxRetryCount", 3);
        var maxRetryDelay = dbSection.GetValue("MaxRetryDelay", 30);
        var commandTimeout = dbSection.GetValue("CommandTimeout", 30);
        var autoMigrate = dbSection.GetValue("AutoMigrate", false);
        var enableSensitiveLogging = dbSection.GetValue("EnableSensitiveDataLogging", false);
        var enableDetailedErrors = dbSection.GetValue("EnableDetailedErrors", false);

        // Register interceptors as singletons so they're reusable
        services.AddSingleton<AuditableEntityInterceptor>(sp =>
        {
            var logger = sp.GetService<ILogger<AuditableEntityInterceptor>>();
            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();

            Func<string?> currentUserProvider = () =>
            {
                var user = httpContextAccessor?.HttpContext?.User;
                return user?.FindFirst("sub")?.Value
                    ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            };

            return new AuditableEntityInterceptor(logger, currentUserProvider);
        });

        services.AddSingleton<SlowQueryInterceptor>(sp =>
        {
            var logger = sp.GetService<ILogger<SlowQueryInterceptor>>();
            return new SlowQueryInterceptor(logger, slowQueryThresholdMs);
        });

        // Register DbContext
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
            var slowQueryInterceptor = serviceProvider.GetRequiredService<SlowQueryInterceptor>();

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: maxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(commandTimeout);
                npgsqlOptions.MigrationsAssembly(GetMigrationsAssembly<TContext>());
            });

            options.AddInterceptors(auditInterceptor, slowQueryInterceptor);

            if (enableSensitiveLogging)
                options.EnableSensitiveDataLogging();

            if (enableDetailedErrors)
                options.EnableDetailedErrors();
        });

        // Register PostgreSQL health check
        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: $"postgresql-{typeof(TContext).Name.ToLowerInvariant()}",
                tags: new[] { "db", "postgresql", "ready" });

        // Auto-migrate if configured
        if (autoMigrate)
        {
            services.AddHostedService<DatabaseMigrationService<TContext>>();
        }

        return services;
    }

    private static string? ResolveConnectionString(IConfiguration configuration, string? key)
    {
        // 1. Explicit key provided
        if (!string.IsNullOrEmpty(key))
        {
            return configuration[key] ?? configuration.GetConnectionString(key);
        }

        // 2. Try Database:ConnectionStrings:PostgreSQL (shared pattern)
        var dbSection = configuration.GetSection("Database:ConnectionStrings");
        var pgConn = dbSection["PostgreSQL"];
        if (!string.IsNullOrEmpty(pgConn))
            return pgConn;

        // 3. Try ConnectionStrings:DefaultConnection (standard .NET pattern)
        var defaultConn = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(defaultConn))
            return defaultConn;

        // 4. Try environment variable
        var envConn = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envConn))
            return envConn;

        return null;
    }

    private static string GetMigrationsAssembly<TContext>() where TContext : DbContext
    {
        var contextAssembly = typeof(TContext).Assembly;
        var assemblyName = contextAssembly.GetName().Name;

        if (assemblyName?.Contains("Infrastructure") == true)
            return assemblyName;

        var infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.Contains("Infrastructure") == true
                && a.GetName().Name?.StartsWith(assemblyName?.Split('.')[0] ?? "") == true);

        return infrastructureAssembly?.GetName().Name ?? contextAssembly.GetName().Name!;
    }
}
