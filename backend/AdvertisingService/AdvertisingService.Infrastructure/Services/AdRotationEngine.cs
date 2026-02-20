using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Infrastructure.Services;

public class AdRotationEngine : IAdRotationEngine
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly ILogger<AdRotationEngine> _logger;
    private static int _roundRobinIndex;

    public AdRotationEngine(
        IAdCampaignRepository campaignRepo,
        ILogger<AdRotationEngine> logger)
    {
        _campaignRepo = campaignRepo;
        _logger = logger;
    }

    public async Task<HomepageRotationResult> ComputeRotationAsync(
        AdPlacementType section,
        RotationConfig config,
        CancellationToken ct = default)
    {
        var activeCampaigns = await _campaignRepo.GetActiveByPlacementAsync(section, ct);

        var eligibleCampaigns = activeCampaigns
            .Where(c => c.IsActive() && !c.IsExpired())
            .ToList();

        if (eligibleCampaigns.Count == 0)
        {
            _logger.LogInformation("No eligible campaigns for {Section}, returning empty result", section);
            return new HomepageRotationResult
            {
                Section = section,
                Vehicles = new List<RotatedVehicleItem>(),
                AlgorithmUsed = config.AlgorithmType,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var selectedCampaigns = config.AlgorithmType switch
        {
            RotationAlgorithmType.WeightedRandom => ApplyWeightedRandom(eligibleCampaigns, config),
            RotationAlgorithmType.RoundRobin => ApplyRoundRobin(eligibleCampaigns, config),
            RotationAlgorithmType.CTROptimized => ApplyCtrOptimized(eligibleCampaigns, config),
            RotationAlgorithmType.BudgetPriority => ApplyBudgetPriority(eligibleCampaigns, config),
            _ => ApplyWeightedRandom(eligibleCampaigns, config)
        };

        var vehicles = selectedCampaigns.Select((c, idx) => new RotatedVehicleItem
        {
            CampaignId = c.Id,
            VehicleId = c.VehicleId,
            OwnerId = c.OwnerId,
            OwnerType = c.OwnerType,
            Position = idx + 1,
            Score = CalculateWeightedScore(c, config)
        }).ToList();

        _logger.LogInformation(
            "Rotation computed for {Section}: {Count} vehicles via {Algorithm}",
            section, vehicles.Count, config.AlgorithmType);

        return new HomepageRotationResult
        {
            Section = section,
            Vehicles = vehicles,
            AlgorithmUsed = config.AlgorithmType,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private List<AdCampaign> ApplyWeightedRandom(List<AdCampaign> campaigns, RotationConfig config)
    {
        var scored = campaigns
            .Select(c => new { Campaign = c, Score = CalculateWeightedScore(c, config) })
            .ToList();

        var totalScore = scored.Sum(s => s.Score);
        if (totalScore <= 0)
            return campaigns.Take(config.MaxVehiclesShown).ToList();

        var random = new Random();
        var selected = new List<AdCampaign>();
        var remaining = new List<(AdCampaign Campaign, decimal Score)>(
            scored.Select(s => (s.Campaign, s.Score)));

        while (selected.Count < config.MaxVehiclesShown && remaining.Count > 0)
        {
            var currentTotal = remaining.Sum(r => r.Score);
            if (currentTotal <= 0) break;

            var roll = (decimal)(random.NextDouble()) * currentTotal;
            decimal cumulative = 0;

            for (int i = 0; i < remaining.Count; i++)
            {
                cumulative += remaining[i].Score;
                if (roll <= cumulative)
                {
                    selected.Add(remaining[i].Campaign);
                    remaining.RemoveAt(i);
                    break;
                }
            }
        }

        return selected;
    }

    private List<AdCampaign> ApplyRoundRobin(List<AdCampaign> campaigns, RotationConfig config)
    {
        var sorted = campaigns.OrderBy(c => c.CreatedAt).ToList();
        var startIndex = Interlocked.Increment(ref _roundRobinIndex) % sorted.Count;

        var selected = new List<AdCampaign>();
        for (int i = 0; i < Math.Min(config.MaxVehiclesShown, sorted.Count); i++)
        {
            var idx = (startIndex + i) % sorted.Count;
            selected.Add(sorted[idx]);
        }

        return selected;
    }

    private List<AdCampaign> ApplyCtrOptimized(List<AdCampaign> campaigns, RotationConfig config)
    {
        return campaigns
            .OrderByDescending(c => c.GetCtr())
            .ThenByDescending(c => c.QualityScore)
            .Take(config.MaxVehiclesShown)
            .ToList();
    }

    private List<AdCampaign> ApplyBudgetPriority(List<AdCampaign> campaigns, RotationConfig config)
    {
        return campaigns
            .OrderByDescending(c => c.GetRemainingBudgetRatio())
            .ThenByDescending(c => c.TotalBudget)
            .Take(config.MaxVehiclesShown)
            .ToList();
    }

    private decimal CalculateWeightedScore(AdCampaign campaign, RotationConfig config)
    {
        var budgetRatio = campaign.GetRemainingBudgetRatio();
        var ctr = campaign.GetCtr();
        var qualityScore = campaign.QualityScore;
        var daysSinceCreation = (DateTime.UtcNow - campaign.CreatedAt).TotalDays;
        var recencyFactor = Math.Max(0, 1.0m - (decimal)(daysSinceCreation / 30.0));

        var score =
            config.WeightRemainingBudget * budgetRatio +
            config.WeightCtr * (decimal)ctr +
            config.WeightQualityScore * qualityScore +
            config.WeightRecency * recencyFactor;

        return Math.Max(0.01m, score);
    }
}
