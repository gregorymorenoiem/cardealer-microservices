using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Infrastructure.BackgroundJobs;

public class CampaignExpirationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignExpirationJob> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public CampaignExpirationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<CampaignExpirationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CampaignExpirationJob started, checking every {Interval}", CheckInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireCampaignsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking campaign expirations");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAndExpireCampaignsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var campaignRepo = scope.ServiceProvider.GetRequiredService<IAdCampaignRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<IHomepageRotationCacheService>();

        var expiredCount = 0;

        foreach (var section in Enum.GetValues<AdPlacementType>())
        {
            var activeCampaigns = await campaignRepo.GetActiveByPlacementAsync(section, ct);
            foreach (var campaign in activeCampaigns.Where(c => c.IsExpired()))
            {
                campaign.MarkExpired();
                await campaignRepo.UpdateAsync(campaign, ct);
                expiredCount++;
            }

            if (expiredCount > 0)
            {
                await cacheService.InvalidateAsync(section);
            }
        }

        if (expiredCount > 0)
        {
            _logger.LogInformation("Expired {Count} campaigns", expiredCount);
        }
    }
}
