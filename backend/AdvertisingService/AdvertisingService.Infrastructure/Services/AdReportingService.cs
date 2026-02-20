using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Interfaces;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Infrastructure.Services;

public class AdReportingService : IAdReportingService
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly IAdImpressionRepository _impressionRepo;
    private readonly IAdClickRepository _clickRepo;
    private readonly ILogger<AdReportingService> _logger;

    public AdReportingService(
        IAdCampaignRepository campaignRepo,
        IAdImpressionRepository impressionRepo,
        IAdClickRepository clickRepo,
        ILogger<AdReportingService> logger)
    {
        _campaignRepo = campaignRepo;
        _impressionRepo = impressionRepo;
        _clickRepo = clickRepo;
        _logger = logger;
    }

    public async Task<CampaignReportData> GetCampaignReportAsync(Guid campaignId, DateTime since, CancellationToken ct = default)
    {
        var campaign = await _campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign == null)
            return new CampaignReportData();

        var impressions = await _impressionRepo.CountByCampaignAsync(campaignId, since, ct);
        var clicks = await _clickRepo.CountByCampaignAsync(campaignId, since, ct);
        var dailyImpressions = await _impressionRepo.GetDailyCountsByCampaignAsync(campaignId, since, ct);
        var dailyClicks = await _clickRepo.GetDailyCountsByCampaignAsync(campaignId, since, ct);

        return new CampaignReportData
        {
            CampaignId = campaignId,
            TotalImpressions = impressions,
            TotalClicks = clicks,
            Ctr = impressions > 0 ? (double)clicks / impressions : 0.0,
            SpentBudget = campaign.SpentBudget,
            RemainingBudget = campaign.TotalBudget - campaign.SpentBudget,
            DailyImpressions = dailyImpressions.Select(d => new DailyDataPoint { Date = d.Date, Count = d.Count }).ToList(),
            DailyClicks = dailyClicks.Select(d => new DailyDataPoint { Date = d.Date, Count = d.Count }).ToList()
        };
    }

    public async Task<OwnerReportData> GetOwnerReportAsync(Guid ownerId, string ownerType, DateTime since, CancellationToken ct = default)
    {
        var campaigns = await _campaignRepo.GetByOwnerAsync(ownerId, ownerType, null, 1, 100, ct);
        var dailyImpressions = await _impressionRepo.GetDailyCountsByOwnerAsync(ownerId, since, ct);
        var dailyClicks = await _clickRepo.GetDailyCountsByOwnerAsync(ownerId, since, ct);

        var activeCampaigns = campaigns.Count(c => c.Status == Domain.Enums.CampaignStatus.Active);
        var totalImpressions = campaigns.Sum(c => c.ViewsConsumed);
        var totalClicks = campaigns.Sum(c => c.Clicks);
        var totalSpent = campaigns.Sum(c => c.SpentBudget);

        return new OwnerReportData
        {
            OwnerId = ownerId,
            OwnerType = ownerType,
            ActiveCampaigns = activeCampaigns,
            TotalCampaigns = campaigns.Count,
            TotalImpressions = totalImpressions,
            TotalClicks = totalClicks,
            OverallCtr = totalImpressions > 0 ? (double)totalClicks / totalImpressions : 0.0,
            TotalSpent = totalSpent,
            DailyImpressions = dailyImpressions.Select(d => new DailyDataPoint { Date = d.Date, Count = d.Count }).ToList(),
            DailyClicks = dailyClicks.Select(d => new DailyDataPoint { Date = d.Date, Count = d.Count }).ToList()
        };
    }

    public async Task<PlatformReportData> GetPlatformReportAsync(DateTime since, CancellationToken ct = default)
    {
        var allCampaigns = await _campaignRepo.GetActiveCampaignsForReportingAsync(since, ct);
        var totalImpressions = await _impressionRepo.GetTotalImpressionsSinceAsync(since, ct);
        var totalClicks = await _clickRepo.GetTotalClicksSinceAsync(since, ct);
        var distinctOwners = await _campaignRepo.GetDistinctOwnerIdsWithActiveCampaignsAsync(ct);

        return new PlatformReportData
        {
            TotalActiveCampaigns = allCampaigns.Count(c => c.Status == Domain.Enums.CampaignStatus.Active),
            TotalCampaigns = allCampaigns.Count,
            TotalImpressions = totalImpressions,
            TotalClicks = totalClicks,
            OverallCtr = totalImpressions > 0 ? (double)totalClicks / totalImpressions : 0.0,
            TotalRevenue = allCampaigns.Sum(c => c.SpentBudget),
            ActiveAdvertisers = distinctOwners.Count,
            ReportPeriodStart = since,
            ReportPeriodEnd = DateTime.UtcNow
        };
    }
}
