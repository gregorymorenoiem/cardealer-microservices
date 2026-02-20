using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Infrastructure.BackgroundJobs;

public class RotationRefreshJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RotationRefreshJob> _logger;
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(5);

    public RotationRefreshJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RotationRefreshJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RotationRefreshJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan nextDelay = DefaultInterval;
            try
            {
                nextDelay = await RefreshAllRotationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing rotations");
            }

            _logger.LogDebug("Next rotation refresh in {Delay}", nextDelay);
            await Task.Delay(nextDelay, stoppingToken);
        }

        _logger.LogInformation("RotationRefreshJob stopped");
    }

    /// <summary>
    /// Refreshes all rotation sections and returns the minimum RefreshIntervalMinutes
    /// from all active configs (so the job runs as often as the most frequent section needs).
    /// </summary>
    private async Task<TimeSpan> RefreshAllRotationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<IHomepageRotationCacheService>();
        var configRepo = scope.ServiceProvider.GetRequiredService<IRotationConfigRepository>();

        var minInterval = DefaultInterval;

        foreach (var section in Enum.GetValues<AdPlacementType>())
        {
            try
            {
                await cacheService.RefreshRotationAsync(section, ct);
                _logger.LogDebug("Rotation refreshed for {Section}", section);

                // Read the interval from DB for this section
                var config = await configRepo.GetBySectionAsync(section, ct);
                if (config is { IsActive: true, RefreshIntervalMinutes: > 0 })
                {
                    var configInterval = TimeSpan.FromMinutes(config.RefreshIntervalMinutes);
                    if (configInterval < minInterval && configInterval >= MinInterval)
                        minInterval = configInterval;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh rotation for {Section}", section);
            }
        }

        return minInterval;
    }
}
