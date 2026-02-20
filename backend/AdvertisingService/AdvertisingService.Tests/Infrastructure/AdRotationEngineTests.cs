using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using AdvertisingService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvertisingService.Tests.Infrastructure;

public class AdRotationEngineTests
{
    private readonly Mock<IAdCampaignRepository> _campaignRepo;
    private readonly Mock<ILogger<AdRotationEngine>> _logger;
    private readonly AdRotationEngine _engine;

    public AdRotationEngineTests()
    {
        _campaignRepo = new Mock<IAdCampaignRepository>();
        _logger = new Mock<ILogger<AdRotationEngine>>();
        _engine = new AdRotationEngine(_campaignRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task ComputeRotationAsync_WithNoActiveCampaigns_ShouldReturnEmptyRotation()
    {
        var section = AdPlacementType.FeaturedSpot;
        var config = CreateDefaultConfig(section);

        _campaignRepo.Setup(r => r.GetActiveByPlacementAsync(section, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdCampaign>());

        var result = await _engine.ComputeRotationAsync(section, config);

        result.Should().NotBeNull();
        result.Vehicles.Should().BeEmpty();
    }

    [Fact]
    public async Task ComputeRotationAsync_WithActiveCampaigns_ShouldReturnItems()
    {
        var section = AdPlacementType.FeaturedSpot;
        var config = CreateDefaultConfig(section, maxVehicles: 6);
        var campaigns = CreateTestCampaigns(10);

        _campaignRepo.Setup(r => r.GetActiveByPlacementAsync(section, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _engine.ComputeRotationAsync(section, config);

        result.Should().NotBeNull();
        result.Vehicles.Should().HaveCountLessThanOrEqualTo(6);
        result.Vehicles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ComputeRotationAsync_ShouldRespectMaxSlots()
    {
        var section = AdPlacementType.FeaturedSpot;
        var config = CreateDefaultConfig(section, algorithm: RotationAlgorithmType.RoundRobin, maxVehicles: 3);
        var campaigns = CreateTestCampaigns(20);

        _campaignRepo.Setup(r => r.GetActiveByPlacementAsync(section, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _engine.ComputeRotationAsync(section, config);

        result.Vehicles.Should().HaveCount(3);
    }

    [Fact]
    public async Task ComputeRotationAsync_WithDefaults_ShouldReturnResults()
    {
        var section = AdPlacementType.PremiumSpot;
        var config = CreateDefaultConfig(section);
        var campaigns = CreateTestCampaigns(5);

        _campaignRepo.Setup(r => r.GetActiveByPlacementAsync(section, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _engine.ComputeRotationAsync(section, config);

        result.Should().NotBeNull();
        result.Vehicles.Should().NotBeEmpty();
    }

    private static RotationConfig CreateDefaultConfig(
        AdPlacementType section,
        RotationAlgorithmType algorithm = RotationAlgorithmType.WeightedRandom,
        int maxVehicles = 6)
    {
        return new RotationConfig
        {
            Id = Guid.NewGuid(),
            Section = section,
            AlgorithmType = algorithm,
            MaxVehiclesShown = maxVehicles,
            RefreshIntervalMinutes = 30,
            WeightRemainingBudget = 0.30m,
            WeightCtr = 0.25m,
            WeightQualityScore = 0.25m,
            WeightRecency = 0.20m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static List<AdCampaign> CreateTestCampaigns(int count)
    {
        var campaigns = new List<AdCampaign>();
        for (int i = 0; i < count; i++)
        {
            var campaign = AdCampaign.Create(
                Guid.NewGuid(), Guid.NewGuid(), "Individual",
                AdPlacementType.FeaturedSpot, CampaignPricingModel.PerView,
                500m, 0.50m, null, 1000,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0.5m + (i * 0.03m));
            campaign.Activate(Guid.NewGuid());
            campaigns.Add(campaign);
        }
        return campaigns;
    }
}
