using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarDealer.Shared.Persistence;

/// <summary>
/// Extensions for configuring data retention policies (auto-cleanup of old records).
/// Used by services like ErrorService, AuditService, NotificationService.
/// </summary>
public static class DataRetentionExtensions
{
    /// <summary>
    /// Registers a hosted service that periodically cleans up old records.
    /// </summary>
    public static IServiceCollection AddDataRetention<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = "DataRetention")
        where TContext : DbContext
    {
        var section = configuration.GetSection(configSection);
        var config = new DataRetentionConfig
        {
            Enabled = section.GetValue("Enabled", false),
            RetentionDays = section.GetValue("RetentionDays", 90),
            CleanupIntervalHours = section.GetValue("CleanupIntervalHours", 24),
            BatchSize = section.GetValue("BatchSize", 1000)
        };

        if (config.Enabled)
        {
            services.AddSingleton(config);
        }

        return services;
    }
}

/// <summary>
/// Configuration for data retention policies.
/// </summary>
public class DataRetentionConfig
{
    /// <summary>Whether automatic cleanup is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Number of days to keep records (default: 90).</summary>
    public int RetentionDays { get; set; } = 90;

    /// <summary>How often to run cleanup in hours (default: 24).</summary>
    public int CleanupIntervalHours { get; set; } = 24;

    /// <summary>Max records to delete per batch (default: 1000).</summary>
    public int BatchSize { get; set; } = 1000;
}
