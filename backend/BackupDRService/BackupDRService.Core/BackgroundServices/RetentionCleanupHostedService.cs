using BackupDRService.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackupDRService.Core.BackgroundServices;

/// <summary>
/// Background service for automatic cleanup of expired backups based on retention policies
/// </summary>
public class RetentionCleanupHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RetentionCleanupHostedService> _logger;
    private readonly TimeSpan _checkInterval;
    private int _totalCleanupsRun;
    private int _totalBackupsDeleted;
    private DateTime _lastCleanupTime;

    public RetentionCleanupHostedService(
        IServiceProvider serviceProvider,
        ILogger<RetentionCleanupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _checkInterval = TimeSpan.FromHours(2); // Check every 2 hours by default
        _lastCleanupTime = DateTime.UtcNow;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "🧹 Retention Cleanup Service started - Auto cleanup every {Hours} hours",
            _checkInterval.TotalHours);

        // Wait before first run to avoid startup overhead
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cleanupStart = DateTime.UtcNow;
                await RunRetentionCleanupAsync(stoppingToken);
                _lastCleanupTime = DateTime.UtcNow;

                var duration = DateTime.UtcNow - cleanupStart;
                _logger.LogInformation(
                    "✅ Retention cleanup completed in {Duration}s. Total runs: {Runs}, Total deleted: {Deleted}",
                    duration.TotalSeconds,
                    _totalCleanupsRun,
                    _totalBackupsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in retention cleanup service");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation(
            "Retention Cleanup Service stopped. Total cleanups: {Cleanups}, Total deleted: {Deleted}",
            _totalCleanupsRun,
            _totalBackupsDeleted);
    }

    private async Task RunRetentionCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var retentionService = scope.ServiceProvider.GetRequiredService<RetentionService>();

        _logger.LogInformation("🧹 Starting automatic retention cleanup...");

        var result = await retentionService.CleanupExpiredBackupsAsync();

        _totalCleanupsRun++;
        _totalBackupsDeleted += result.DeletedCount;

        if (result.DeletedCount > 0)
        {
            _logger.LogInformation(
                "🗑️ Cleanup removed {Count} expired backup(s), freed {SizeMB} MB",
                result.DeletedCount,
                result.FreedSpaceBytes / 1024.0 / 1024.0);
        }
        else
        {
            _logger.LogDebug("No expired backups found during cleanup");
        }

        if (result.Errors.Any())
        {
            _logger.LogWarning(
                "⚠️ Cleanup completed with {ErrorCount} error(s): {Errors}",
                result.Errors.Count,
                string.Join(", ", result.Errors));
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Retention Cleanup Service stopping...");
        return base.StopAsync(cancellationToken);
    }
}
