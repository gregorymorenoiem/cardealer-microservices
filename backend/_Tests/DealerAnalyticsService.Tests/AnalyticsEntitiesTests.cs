using DealerAnalyticsService.Domain.Entities;
using DealerAnalyticsService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace DealerAnalyticsService.Tests;

/// <summary>
/// Additional tests for Analytics Domain Entities
/// </summary>
public class AnalyticsEntitiesTests
{
    #region ProfileView Tests

    [Fact]
    public void ProfileView_ShouldBeCreated_WithDefaultValues()
    {
        // Arrange & Act
        var view = new ProfileView();

        // Assert
        view.Id.Should().NotBeEmpty();
        view.ViewedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ProfileView_IsDuplicateView_ShouldReturnTrue_WhenSameIPWithin30Minutes()
    {
        // Arrange
        var ipAddress = "192.168.1.1";
        var view = new ProfileView
        {
            ViewerIpAddress = ipAddress,
            ViewedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        // Act
        var isDuplicate = view.IsDuplicateView(ipAddress, 30);

        // Assert
        isDuplicate.Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsDuplicateView_ShouldReturnFalse_WhenDifferentIP()
    {
        // Arrange
        var view = new ProfileView
        {
            ViewerIpAddress = "192.168.1.1",
            ViewedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        // Act
        var isDuplicate = view.IsDuplicateView("192.168.1.2", 30);

        // Assert
        isDuplicate.Should().BeFalse();
    }

    [Fact]
    public void ProfileView_IsBounce_ShouldReturnTrue_WhenDurationLessThan10Seconds()
    {
        // Arrange
        var view = new ProfileView { DurationSeconds = 5 };

        // Act
        var isBounce = view.IsBounce();

        // Assert
        isBounce.Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsBounce_ShouldReturnFalse_WhenDurationMoreThan10Seconds()
    {
        // Arrange
        var view = new ProfileView { DurationSeconds = 45 };

        // Act
        var isBounce = view.IsBounce();

        // Assert
        isBounce.Should().BeFalse();
    }

    [Fact]
    public void ProfileView_IsEngagedVisit_ShouldReturnTrue_WhenDurationMoreThan2Minutes()
    {
        // Arrange
        var view = new ProfileView { DurationSeconds = 150 };

        // Act
        var isEngaged = view.IsEngagedVisit();

        // Assert
        isEngaged.Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsEngagedVisit_ShouldReturnFalse_WhenDurationLessThan2Minutes()
    {
        // Arrange
        var view = new ProfileView { DurationSeconds = 60 };

        // Act
        var isEngaged = view.IsEngagedVisit();

        // Assert
        isEngaged.Should().BeFalse();
    }

    #endregion

    #region DealerSnapshot Tests

    [Fact]
    public void DealerSnapshot_ClickThroughRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            SearchImpressions = 1000,
            SearchClicks = 50
        };

        // Assert
        snapshot.ClickThroughRate.Should().BeApproximately(5.0, 0.01);
    }

    [Fact]
    public void DealerSnapshot_ContactRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            TotalViews = 200,
            TotalContacts = 10
        };

        // Assert
        snapshot.ContactRate.Should().BeApproximately(5.0, 0.01);
    }

    [Fact]
    public void DealerSnapshot_FavoriteRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            TotalViews = 500,
            TotalFavorites = 25
        };

        // Assert
        snapshot.FavoriteRate.Should().BeApproximately(5.0, 0.01);
    }

    [Fact]
    public void DealerSnapshot_AllRates_ShouldBeZero_WhenNoData()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            SearchImpressions = 0,
            TotalViews = 0
        };

        // Assert
        snapshot.ClickThroughRate.Should().Be(0);
        snapshot.ContactRate.Should().Be(0);
        snapshot.FavoriteRate.Should().Be(0);
    }

    #endregion

    #region DealerBenchmark Tests

    [Fact]
    public void DealerBenchmark_CalculateTier_ShouldReturnDiamond_WhenAvgPercentileOver90()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            ConversionRatePercentile = 95,
            ResponseTimePercentile = 92,
            SatisfactionPercentile = 93,
            ListingQualityPercentile = 90,
            EngagementPercentile = 91
        };

        // Act
        benchmark.CalculateTier();

        // Assert
        benchmark.Tier.Should().Be(DealerTier.Diamond);
    }

    [Fact]
    public void DealerBenchmark_CalculateTier_ShouldReturnPlatinum_WhenAvgPercentileBetween75And90()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            ConversionRatePercentile = 80,
            ResponseTimePercentile = 82,
            SatisfactionPercentile = 78,
            ListingQualityPercentile = 85,
            EngagementPercentile = 80
        };

        // Act
        benchmark.CalculateTier();

        // Assert
        benchmark.Tier.Should().Be(DealerTier.Platinum);
    }

    [Fact]
    public void DealerBenchmark_CalculateTier_ShouldReturnGold_WhenAvgPercentileBetween50And75()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            ConversionRatePercentile = 60,
            ResponseTimePercentile = 55,
            SatisfactionPercentile = 65,
            ListingQualityPercentile = 58,
            EngagementPercentile = 62
        };

        // Act
        benchmark.CalculateTier();

        // Assert
        benchmark.Tier.Should().Be(DealerTier.Gold);
    }

    [Fact]
    public void DealerBenchmark_CalculateTier_ShouldReturnSilver_WhenAvgPercentileBetween25And50()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            ConversionRatePercentile = 35,
            ResponseTimePercentile = 40,
            SatisfactionPercentile = 30,
            ListingQualityPercentile = 38,
            EngagementPercentile = 32
        };

        // Act
        benchmark.CalculateTier();

        // Assert
        benchmark.Tier.Should().Be(DealerTier.Silver);
    }

    [Fact]
    public void DealerBenchmark_CalculateTier_ShouldReturnBronze_WhenAvgPercentileUnder25()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            ConversionRatePercentile = 15,
            ResponseTimePercentile = 20,
            SatisfactionPercentile = 18,
            ListingQualityPercentile = 12,
            EngagementPercentile = 10
        };

        // Act
        benchmark.CalculateTier();

        // Assert
        benchmark.Tier.Should().Be(DealerTier.Bronze);
    }

    [Fact]
    public void DealerBenchmark_Comparisons_ShouldWork()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            AvgDaysOnMarket = 20,
            MarketAvgDaysOnMarket = 30,
            ConversionRate = 5.0,
            MarketAvgConversionRate = 3.0,
            AvgResponseTimeMinutes = 30,
            MarketAvgResponseTime = 60,
            CustomerSatisfaction = 4.5,
            MarketAvgSatisfaction = 4.0
        };

        // Assert
        benchmark.IsBetterThanMarketDaysOnMarket.Should().BeTrue();
        benchmark.IsBetterThanMarketConversion.Should().BeTrue();
        benchmark.IsBetterThanMarketResponseTime.Should().BeTrue();
        benchmark.IsBetterThanMarketSatisfaction.Should().BeTrue();
    }

    #endregion

    #region LeadFunnelMetrics Tests

    [Fact]
    public void LeadFunnelMetrics_ImpressionsToViews_ShouldCalculateCorrectly()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Impressions = 10000,
            Views = 2000
        };

        // Assert
        funnel.ImpressionsToViews.Should().BeApproximately(20.0, 0.01);
    }

    [Fact]
    public void LeadFunnelMetrics_ViewsToContacts_ShouldCalculateCorrectly()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Views = 1000,
            Contacts = 100
        };

        // Assert
        funnel.ViewsToContacts.Should().BeApproximately(10.0, 0.01);
    }

    [Fact]
    public void LeadFunnelMetrics_ContactsToQualified_ShouldCalculateCorrectly()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Contacts = 100,
            Qualified = 30
        };

        // Assert
        funnel.ContactsToQualified.Should().BeApproximately(30.0, 0.01);
    }

    [Fact]
    public void LeadFunnelMetrics_QualifiedToNegotiation_ShouldCalculateCorrectly()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Qualified = 50,
            Negotiation = 20
        };

        // Assert
        funnel.QualifiedToNegotiation.Should().BeApproximately(40.0, 0.01);
    }

    [Fact]
    public void LeadFunnelMetrics_NegotiationToConverted_ShouldCalculateCorrectly()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Negotiation = 20,
            Converted = 10
        };

        // Assert
        funnel.NegotiationToConverted.Should().BeApproximately(50.0, 0.01);
    }

    [Fact]
    public void LeadFunnelMetrics_OverallConversion_ShouldCalculateCorrectly()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Impressions = 10000,
            Converted = 10
        };

        // Assert
        funnel.OverallConversion.Should().BeApproximately(0.1, 0.01);
    }

    #endregion

    #region InventoryAging Tests

    [Fact]
    public void InventoryAging_TotalVehicles_ShouldSumAllBuckets()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Vehicles0To15Days = 10,
            Vehicles16To30Days = 8,
            Vehicles31To45Days = 6,
            Vehicles46To60Days = 4,
            Vehicles61To90Days = 2,
            VehiclesOver90Days = 1
        };

        // Assert
        aging.TotalVehicles.Should().Be(31);
    }

    [Fact]
    public void InventoryAging_TotalValue_ShouldSumAllBuckets()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Value0To15Days = 1000000m,
            Value16To30Days = 800000m,
            Value31To45Days = 600000m,
            Value46To60Days = 400000m,
            Value61To90Days = 200000m,
            ValueOver90Days = 100000m
        };

        // Assert
        aging.TotalValue.Should().Be(3100000m);
    }

    [Fact]
    public void InventoryAging_PercentFresh_ShouldCalculateCorrectly()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Vehicles0To15Days = 20,
            Vehicles16To30Days = 20,
            Vehicles31To45Days = 10,
            Vehicles46To60Days = 10,
            Vehicles61To90Days = 10,
            VehiclesOver90Days = 10
        };

        // Assert - Fresh = 40 out of 80 = 50%
        aging.PercentFresh.Should().BeApproximately(50.0, 0.1);
    }

    [Fact]
    public void InventoryAging_PercentAging_ShouldCalculateCorrectly()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Vehicles0To15Days = 10,
            Vehicles16To30Days = 10,
            Vehicles31To45Days = 10,
            Vehicles46To60Days = 20,
            Vehicles61To90Days = 25,
            VehiclesOver90Days = 25
        };

        // Assert - Aging (46-60 + 61-90 + 90+) = 70 out of 100 = 70%
        aging.PercentAging.Should().BeApproximately(70.0, 0.1);
    }

    [Fact]
    public void InventoryAging_AtRiskCount_ShouldSum61AndOver90()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Vehicles61To90Days = 5,
            VehiclesOver90Days = 3
        };

        // Assert
        aging.AtRiskCount.Should().Be(8);
    }

    [Fact]
    public void InventoryAging_AtRiskValue_ShouldSum61AndOver90()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Value61To90Days = 500000m,
            ValueOver90Days = 300000m
        };

        // Assert
        aging.AtRiskValue.Should().Be(800000m);
    }

    #endregion

    #region VehiclePerformance Tests

    [Fact]
    public void VehiclePerformance_ClickThroughRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var perf = new VehiclePerformance
        {
            SearchImpressions = 1000,
            SearchClicks = 50
        };

        // Assert
        perf.ClickThroughRate.Should().BeApproximately(5.0, 0.01);
    }

    [Fact]
    public void VehiclePerformance_ContactRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var perf = new VehiclePerformance
        {
            Views = 200,
            Contacts = 10
        };

        // Assert
        perf.ContactRate.Should().BeApproximately(5.0, 0.01);
    }

    [Fact]
    public void VehiclePerformance_FavoriteRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var perf = new VehiclePerformance
        {
            Views = 100,
            Favorites = 15
        };

        // Assert
        perf.FavoriteRate.Should().BeApproximately(15.0, 0.01);
    }

    [Fact]
    public void VehiclePerformance_Rates_ShouldBeZero_WhenNoData()
    {
        // Arrange
        var perf = new VehiclePerformance
        {
            SearchImpressions = 0,
            Views = 0
        };

        // Assert
        perf.ClickThroughRate.Should().Be(0);
        perf.ContactRate.Should().Be(0);
        perf.FavoriteRate.Should().Be(0);
    }

    [Fact]
    public void VehiclePerformance_Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var perf = new VehiclePerformance();

        // Assert
        perf.Id.Should().NotBeEmpty();
        perf.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region AlertEnums Tests

    [Fact]
    public void DealerAlertType_ShouldHaveInventoryTypes()
    {
        // Assert
        ((int)DealerAlertType.LowInventory).Should().Be(1);
        ((int)DealerAlertType.AgingInventory).Should().Be(2);
        ((int)DealerAlertType.PriceAdjustmentNeeded).Should().Be(3);
        ((int)DealerAlertType.InventoryImbalance).Should().Be(4);
    }

    [Fact]
    public void DealerAlertType_ShouldHaveEngagementTypes()
    {
        // Assert
        ((int)DealerAlertType.ViewsDropping).Should().Be(10);
        ((int)DealerAlertType.LowContactRate).Should().Be(11);
        ((int)DealerAlertType.LowFavoriteRate).Should().Be(12);
    }

    [Fact]
    public void DealerAlertType_ShouldHaveLeadTypes()
    {
        // Assert
        ((int)DealerAlertType.LeadResponseSlow).Should().Be(20);
        ((int)DealerAlertType.LeadsNotFollowedUp).Should().Be(21);
        ((int)DealerAlertType.ConversionDropping).Should().Be(22);
    }

    [Fact]
    public void AlertSeverity_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)AlertSeverity.Info).Should().Be(1);
        ((int)AlertSeverity.Low).Should().Be(2);
        ((int)AlertSeverity.Medium).Should().Be(3);
        ((int)AlertSeverity.Warning).Should().Be(4);
        ((int)AlertSeverity.High).Should().Be(5);
        ((int)AlertSeverity.Critical).Should().Be(6);
    }

    [Fact]
    public void AlertStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)AlertStatus.Active).Should().Be(1);
        ((int)AlertStatus.Read).Should().Be(2);
        ((int)AlertStatus.Dismissed).Should().Be(3);
        ((int)AlertStatus.Resolved).Should().Be(4);
        ((int)AlertStatus.Expired).Should().Be(5);
    }

    #endregion

    #region DealerAlert Tests

    [Fact]
    public void DealerAlert_IsExpired_ShouldReturnTrue_WhenPastExpirationDate()
    {
        // Arrange
        var alert = new DealerAlert
        {
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Assert
        alert.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void DealerAlert_IsExpired_ShouldReturnFalse_WhenBeforeExpirationDate()
    {
        // Arrange
        var alert = new DealerAlert
        {
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        alert.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void DealerAlert_IsExpired_ShouldReturnFalse_WhenNoExpirationDate()
    {
        // Arrange
        var alert = new DealerAlert
        {
            ExpiresAt = null
        };

        // Assert
        alert.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void DealerAlert_ShouldCreateWithAllProperties()
    {
        // Arrange & Act
        var alert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = DealerAlertType.LowInventory,
            Severity = AlertSeverity.Warning,
            Status = AlertStatus.Active,
            Title = "Inventario bajo",
            Message = "Solo tiene 2 vehículos activos",
            ActionUrl = "/dealer/inventory/add",
            ActionLabel = "Agregar vehículo",
            IsRead = false,
            IsDismissed = false,
            IsActedUpon = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.Id.Should().NotBeEmpty();
        alert.Type.Should().Be(DealerAlertType.LowInventory);
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.Title.Should().NotBeEmpty();
        alert.ActionUrl.Should().NotBeNull();
    }

    #endregion

    #region DealerInsight Tests

    [Fact]
    public void InsightType_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(InsightType), InsightType.PricingRecommendation).Should().BeTrue();
        Enum.IsDefined(typeof(InsightType), InsightType.InventoryOptimization).Should().BeTrue();
        Enum.IsDefined(typeof(InsightType), InsightType.MarketingStrategy).Should().BeTrue();
        Enum.IsDefined(typeof(InsightType), InsightType.SeasonalTrend).Should().BeTrue();
        Enum.IsDefined(typeof(InsightType), InsightType.CompetitorAnalysis).Should().BeTrue();
    }

    [Fact]
    public void InsightPriority_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(InsightPriority), InsightPriority.Low).Should().BeTrue();
        Enum.IsDefined(typeof(InsightPriority), InsightPriority.Medium).Should().BeTrue();
        Enum.IsDefined(typeof(InsightPriority), InsightPriority.High).Should().BeTrue();
        Enum.IsDefined(typeof(InsightPriority), InsightPriority.Critical).Should().BeTrue();
    }

    [Fact]
    public void DealerInsight_ShouldCreateWithAllProperties()
    {
        // Arrange & Act
        var insight = new DealerInsight
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = InsightType.PricingRecommendation,
            Priority = InsightPriority.High,
            Title = "Reducir precio",
            Description = "Su vehículo está por encima del mercado",
            ActionRecommendation = "Reducir el precio en un 10%",
            SupportingData = "{}",
            PotentialImpact = 15.5m,
            Confidence = 85.0m,
            IsRead = false,
            IsActedUpon = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        insight.Id.Should().NotBeEmpty();
        insight.Type.Should().Be(InsightType.PricingRecommendation);
        insight.Priority.Should().Be(InsightPriority.High);
        insight.PotentialImpact.Should().Be(15.5m);
        insight.Confidence.Should().Be(85.0m);
    }

    #endregion
}
