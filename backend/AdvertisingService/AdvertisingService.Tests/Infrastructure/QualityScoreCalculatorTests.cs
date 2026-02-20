using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using AdvertisingService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AdvertisingService.Tests.Infrastructure;

public class QualityScoreCalculatorTests
{
    private readonly QualityScoreCalculator _calculator;
    private readonly Mock<IAdImpressionRepository> _impressionRepo;
    private readonly Mock<IAdClickRepository> _clickRepo;

    public QualityScoreCalculatorTests()
    {
        _impressionRepo = new Mock<IAdImpressionRepository>();
        _clickRepo = new Mock<IAdClickRepository>();
        var logger = new Mock<ILogger<QualityScoreCalculator>>();
        _calculator = new QualityScoreCalculator(_impressionRepo.Object, _clickRepo.Object, logger.Object);
    }

    [Fact]
    public async Task CalculateAsync_WithGoodCampaign_ShouldReturnHighScore()
    {
        var campaign = CreateCampaign(totalBudget: 500m, pricePerView: 0.50m, clicks: 0, viewsConsumed: 0);

        _impressionRepo.Setup(r => r.CountByCampaignAsync(campaign.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _clickRepo.Setup(r => r.CountByCampaignAsync(campaign.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var score = await _calculator.CalculateAsync(
            campaign, imageCount: 10, hasDescription: true, hasPrice: true,
            averageMarketPrice: 500m, sellerRating: 0.9m);

        score.Should().BeGreaterThan(0.5m);
        score.Should().BeLessThanOrEqualTo(1.0m);
    }

    [Fact]
    public async Task CalculateAsync_WithPoorCampaign_ShouldReturnLowScore()
    {
        var campaign = CreateCampaign(totalBudget: 10m, pricePerView: 0.50m, clicks: 0, viewsConsumed: 0);

        _impressionRepo.Setup(r => r.CountByCampaignAsync(campaign.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _clickRepo.Setup(r => r.CountByCampaignAsync(campaign.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var score = await _calculator.CalculateAsync(
            campaign, imageCount: 0, hasDescription: false, hasPrice: false,
            averageMarketPrice: 0m, sellerRating: 0m);

        score.Should().BeGreaterThanOrEqualTo(0.01m);
        score.Should().BeLessThan(0.5m);
    }

    [Fact]
    public async Task CalculateAsync_ShouldBeBetween001And1()
    {
        var campaign = CreateCampaign(totalBudget: 200m, pricePerView: 0.50m, clicks: 0, viewsConsumed: 0);

        _impressionRepo.Setup(r => r.CountByCampaignAsync(campaign.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);
        _clickRepo.Setup(r => r.CountByCampaignAsync(campaign.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var score = await _calculator.CalculateAsync(
            campaign, imageCount: 5, hasDescription: true, hasPrice: true,
            averageMarketPrice: 300m, sellerRating: 0.7m);

        score.Should().BeInRange(0.01m, 1.0m);
    }

    private static AdCampaign CreateCampaign(decimal totalBudget, decimal pricePerView, int clicks, int viewsConsumed)
    {
        return AdCampaign.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Individual",
            AdPlacementType.FeaturedSpot, CampaignPricingModel.PerView,
            totalBudget, pricePerView, null, 1000,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0.5m);
    }
}
