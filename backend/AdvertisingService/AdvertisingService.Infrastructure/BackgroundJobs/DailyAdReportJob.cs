using AdvertisingService.Application.Clients;
using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Infrastructure.BackgroundJobs;

public class DailyAdReportJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyAdReportJob> _logger;

    // 12:00 UTC = 8:00 AM Dominican Republic time
    private static readonly TimeSpan TargetTimeUtc = new(12, 0, 0);

    public DailyAdReportJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyAdReportJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyAdReportJob started, will run daily at {Time} UTC", TargetTimeUtc);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.Add(TargetTimeUtc);
            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogDebug("Next daily report run in {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await GenerateDailyReportAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily ad report");
            }
        }

        _logger.LogInformation("DailyAdReportJob stopped");
    }

    private async Task GenerateDailyReportAsync(CancellationToken ct)
    {
        _logger.LogInformation("Generating daily advertising report...");

        using var scope = _scopeFactory.CreateScope();
        var reportingService = scope.ServiceProvider.GetRequiredService<IAdReportingService>();
        var campaignRepo = scope.ServiceProvider.GetRequiredService<IAdCampaignRepository>();
        var notificationClient = scope.ServiceProvider.GetRequiredService<NotificationServiceClient>();

        var yesterday = DateTime.UtcNow.AddDays(-1).Date;
        var platformReport = await reportingService.GetPlatformReportAsync(yesterday, ct);

        _logger.LogInformation(
            "Daily Report: {ActiveCampaigns} active campaigns, {Impressions} impressions, {Clicks} clicks, CTR={Ctr:P2}, Revenue={Revenue:C2}",
            platformReport.TotalActiveCampaigns,
            platformReport.TotalImpressions,
            platformReport.TotalClicks,
            platformReport.OverallCtr,
            platformReport.TotalRevenue);

        // Check for campaigns that need expiration
        var activeCampaigns = await campaignRepo.GetActiveCampaignsForReportingAsync(yesterday, ct);
        foreach (var campaign in activeCampaigns.Where(c => c.IsExpired()))
        {
            campaign.MarkExpired();
            await campaignRepo.UpdateAsync(campaign, ct);
            _logger.LogInformation("Campaign {CampaignId} marked as expired", campaign.Id);
        }

        // Send daily report email to each owner with active campaigns
        var ownerIds = await campaignRepo.GetDistinctOwnerIdsWithActiveCampaignsAsync(ct);
        _logger.LogInformation("Sending daily report emails to {OwnerCount} owners", ownerIds.Count);

        foreach (var ownerId in ownerIds)
        {
            try
            {
                // Determine ownerType from campaigns (Individual or Dealer)
                var ownerCampaigns = await campaignRepo.GetByOwnerAsync(ownerId, "Individual", null, 1, 1, ct);
                var ownerType = ownerCampaigns.Count > 0 ? "Individual" : "Dealer";
                if (ownerCampaigns.Count == 0)
                {
                    ownerCampaigns = await campaignRepo.GetByOwnerAsync(ownerId, "Dealer", null, 1, 1, ct);
                    if (ownerCampaigns.Count == 0) continue; // No campaigns found for this owner
                }

                var ownerReport = await reportingService.GetOwnerReportAsync(ownerId, ownerType, yesterday, ct);

                var placeholders = new Dictionary<string, string>
                {
                    ["period"] = yesterday.ToString("dd/MM/yyyy"),
                    ["totalViews"] = ownerReport.TotalImpressions.ToString("N0"),
                    ["totalClicks"] = ownerReport.TotalClicks.ToString("N0"),
                    ["averageCtr"] = (ownerReport.OverallCtr * 100).ToString("F2"),
                    ["activeCampaigns"] = ownerReport.ActiveCampaigns.ToString(),
                    ["totalSpent"] = ownerReport.TotalSpent.ToString("N2"),
                    ["portalUrl"] = "https://okla.com.do/impulsar/mis-campanas"
                };

                await notificationClient.SendNotificationAsync(
                    ownerId,
                    "advertising-daily-report",
                    placeholders,
                    ct);

                _logger.LogDebug("Daily report email sent to owner {OwnerId}", ownerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send daily report to owner {OwnerId}", ownerId);
            }
        }
    }
}
