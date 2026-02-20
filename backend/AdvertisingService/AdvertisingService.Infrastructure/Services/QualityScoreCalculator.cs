using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Infrastructure.Services;

public class QualityScoreCalculator
{
    private readonly IAdImpressionRepository _impressionRepo;
    private readonly IAdClickRepository _clickRepo;
    private readonly ILogger<QualityScoreCalculator> _logger;

    // Weights from spec Part 4
    private const decimal WeightImageQuality = 0.25m;
    private const decimal WeightCompleteness = 0.20m;
    private const decimal WeightCtrHistory = 0.20m;
    private const decimal WeightFreshness = 0.15m;
    private const decimal WeightPriceCompetitive = 0.10m;
    private const decimal WeightSellerRating = 0.10m;

    public QualityScoreCalculator(
        IAdImpressionRepository impressionRepo,
        IAdClickRepository clickRepo,
        ILogger<QualityScoreCalculator> logger)
    {
        _impressionRepo = impressionRepo;
        _clickRepo = clickRepo;
        _logger = logger;
    }

    public async Task<decimal> CalculateAsync(
        AdCampaign campaign,
        int imageCount = 0,
        bool hasDescription = false,
        bool hasPrice = true,
        decimal averageMarketPrice = 0m,
        decimal sellerRating = 0.5m,
        CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-30);
        var impressions = await _impressionRepo.CountByCampaignAsync(campaign.Id, since, ct);
        var clicks = await _clickRepo.CountByCampaignAsync(campaign.Id, since, ct);

        // Image Quality: 0-1 based on image count (0 = 0.0, 5+ = 1.0)
        var imageQuality = Math.Min(1.0m, imageCount / 5.0m);

        // Completeness: based on filled fields
        var completenessFactors = new[] { hasDescription, hasPrice, imageCount > 0 };
        var completeness = completenessFactors.Count(f => f) / (decimal)completenessFactors.Length;

        // CTR History: 0-1 scale, capped at 0.15 (15% CTR is exceptional)
        var ctr = impressions > 0 ? (decimal)clicks / impressions : 0m;
        var ctrScore = Math.Min(1.0m, ctr / 0.15m);

        // Freshness: 1.0 if new, 0.0 if 30+ days old
        var daysSinceCreation = (DateTime.UtcNow - campaign.CreatedAt).TotalDays;
        var freshness = Math.Max(0m, 1.0m - (decimal)(daysSinceCreation / 30.0));

        // Price Competitiveness: how competitive vs market average
        decimal priceCompetitive;
        if (averageMarketPrice > 0 && campaign.FixedPrice.GetValueOrDefault() > 0)
        {
            var ratio = campaign.FixedPrice!.Value / averageMarketPrice;
            priceCompetitive = ratio <= 1.0m ? 1.0m : Math.Max(0m, 2.0m - ratio);
        }
        else
        {
            priceCompetitive = 0.5m; // Neutral
        }

        // Seller Rating: passed in, 0-1 scale
        var sellerScore = Math.Clamp(sellerRating, 0m, 1.0m);

        // Weighted sum
        var qualityScore =
            WeightImageQuality * imageQuality +
            WeightCompleteness * completeness +
            WeightCtrHistory * ctrScore +
            WeightFreshness * freshness +
            WeightPriceCompetitive * priceCompetitive +
            WeightSellerRating * sellerScore;

        qualityScore = Math.Clamp(qualityScore, 0.01m, 1.0m);

        _logger.LogDebug(
            "Quality score for campaign {CampaignId}: {Score} (img={Img}, comp={Comp}, ctr={Ctr}, fresh={Fresh}, price={Price}, seller={Seller})",
            campaign.Id, qualityScore, imageQuality, completeness, ctrScore, freshness, priceCompetitive, sellerScore);

        return qualityScore;
    }
}
