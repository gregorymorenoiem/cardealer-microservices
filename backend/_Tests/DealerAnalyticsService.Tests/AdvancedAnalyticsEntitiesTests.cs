using Xunit;
using FluentAssertions;
using DealerAnalyticsService.Domain.Entities;
using DealerAnalyticsService.Domain.Enums;

namespace DealerAnalyticsService.Tests;

/// <summary>
/// Tests for Advanced Analytics Domain Entities
/// Covers: DealerSnapshot, VehiclePerformance, LeadFunnelMetrics, DealerBenchmark, DealerAlert, InventoryAging
/// </summary>
public class AdvancedAnalyticsEntitiesTests
{
    #region DealerSnapshot Tests

    [Fact]
    public void DealerSnapshot_CreateEmpty_ShouldInitializeWithDefaults()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        // Act
        var snapshot = DealerSnapshot.CreateEmpty(dealerId, date);

        // Assert
        snapshot.Should().NotBeNull();
        snapshot.DealerId.Should().Be(dealerId);
        snapshot.Date.Should().Be(date);
        snapshot.ActiveVehicles.Should().Be(0);
        snapshot.TotalViews.Should().Be(0);
        snapshot.CTR.Should().Be(0);
    }

    [Fact]
    public void DealerSnapshot_ShouldCalculateCTR_Correctly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Impressions = 10000,
            TotalViews = 350,
            TotalContacts = 28
        };

        // Assert
        snapshot.CTR.Should().BeApproximately(3.5m, 0.01m); // 350/10000 * 100
    }

    [Fact]
    public void DealerSnapshot_ShouldCalculateContactRate_Correctly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            TotalViews = 1000,
            TotalContacts = 85
        };

        // Assert
        snapshot.ContactRate.Should().BeApproximately(8.5m, 0.01m); // 85/1000 * 100
    }

    [Fact]
    public void DealerSnapshot_ShouldCalculateTurnoverRate_Correctly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            ActiveVehicles = 50,
            SoldVehicles = 8
        };

        // Assert
        snapshot.TurnoverRate.Should().BeApproximately(16.0m, 0.01m); // 8/50 * 100
    }

    [Fact]
    public void DealerSnapshot_ShouldCalculateAgingRate_Correctly()
    {
        // Arrange
        var snapshot = new DealerSnapshot
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            ActiveVehicles = 45,
            VehiclesOver60Days = 9
        };

        // Assert
        snapshot.AgingRate.Should().BeApproximately(20.0m, 0.01m); // 9/45 * 100
    }

    #endregion

    #region VehiclePerformance Tests

    [Fact]
    public void VehiclePerformance_ShouldCalculateEngagementScore()
    {
        // Arrange
        var performance = new VehiclePerformance
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            TotalViews = 500,
            TotalContacts = 25,
            Favorites = 40,
            Shares = 10,
            DaysOnMarket = 30
        };

        // Act
        var engagementScore = performance.EngagementScore;

        // Assert - Score is weighted formula
        engagementScore.Should().BeGreaterThan(0);
        engagementScore.Should().BeLessThan(100);
    }

    [Fact]
    public void VehiclePerformance_ShouldCalculatePerformanceScore()
    {
        // Arrange
        var performance = new VehiclePerformance
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            TotalViews = 1000,
            TotalContacts = 50,
            Favorites = 80,
            Shares = 20,
            SearchImpressions = 5000,
            SearchClicks = 200,
            DaysOnMarket = 15
        };

        // Act
        var performanceScore = performance.PerformanceScore;

        // Assert
        performanceScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public void VehiclePerformance_ShouldIdentifyLowPerformer()
    {
        // Arrange
        var lowPerformer = new VehiclePerformance
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            TotalViews = 20,
            TotalContacts = 0,
            DaysOnMarket = 90
        };

        // Assert
        lowPerformer.IsLowPerformer(threshold: 50).Should().BeTrue();
    }

    #endregion

    #region LeadFunnelMetrics Tests

    [Fact]
    public void LeadFunnelMetrics_ShouldCalculateConversionRates()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            PeriodStart = DateTime.UtcNow.AddDays(-30),
            PeriodEnd = DateTime.UtcNow,
            TotalImpressions = 100000,
            TotalViews = 5000,
            TotalContacts = 250,
            QualifiedLeads = 100,
            InNegotiation = 30,
            Converted = 12
        };

        // Assert
        funnel.ImpressionToViewRate.Should().BeApproximately(5.0m, 0.01m);
        funnel.ViewToContactRate.Should().BeApproximately(5.0m, 0.01m);
        funnel.ContactToQualifiedRate.Should().BeApproximately(40.0m, 0.01m);
        funnel.QualifiedToNegotiationRate.Should().BeApproximately(30.0m, 0.01m);
        funnel.NegotiationToConversionRate.Should().BeApproximately(40.0m, 0.01m);
        funnel.OverallConversionRate.Should().BeApproximately(0.012m, 0.001m);
    }

    [Fact]
    public void LeadFunnelMetrics_GetFunnelStages_ShouldReturnAllStages()
    {
        // Arrange
        var funnel = new LeadFunnelMetrics
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            TotalImpressions = 50000,
            TotalViews = 2500,
            TotalContacts = 125,
            QualifiedLeads = 50,
            InNegotiation = 15,
            Converted = 5
        };

        // Act
        var stages = funnel.GetFunnelStages();

        // Assert
        stages.Should().HaveCount(6);
        stages[0].Name.Should().Be("Impresiones");
        stages[0].Value.Should().Be(50000);
        stages[5].Name.Should().Be("Conversiones");
        stages[5].Value.Should().Be(5);
    }

    #endregion

    #region DealerBenchmark Tests

    [Fact]
    public void DealerBenchmark_ShouldCalculateTier_Bronze()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            OverallScore = 45
        };

        // Assert
        benchmark.CalculatedTier.Should().Be(DealerTier.Bronze);
    }

    [Fact]
    public void DealerBenchmark_ShouldCalculateTier_Silver()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            OverallScore = 55
        };

        // Assert
        benchmark.CalculatedTier.Should().Be(DealerTier.Silver);
    }

    [Fact]
    public void DealerBenchmark_ShouldCalculateTier_Gold()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            OverallScore = 72
        };

        // Assert
        benchmark.CalculatedTier.Should().Be(DealerTier.Gold);
    }

    [Fact]
    public void DealerBenchmark_ShouldCalculateTier_Platinum()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            OverallScore = 88
        };

        // Assert
        benchmark.CalculatedTier.Should().Be(DealerTier.Platinum);
    }

    [Fact]
    public void DealerBenchmark_ShouldCalculateTier_Diamond()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            OverallScore = 96
        };

        // Assert
        benchmark.CalculatedTier.Should().Be(DealerTier.Diamond);
    }

    [Fact]
    public void DealerBenchmark_IsAboveAverage_ShouldBeTrue_WhenScoreOver50()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            ViewsPercentile = 65,
            ContactsPercentile = 70,
            ConversionPercentile = 55,
            ResponseTimePercentile = 80
        };

        // Assert
        benchmark.IsAboveAverage.Should().BeTrue();
    }

    #endregion

    #region DealerAlert Tests

    [Fact]
    public void DealerAlert_CreateSlowInventoryAlert_ShouldSetCorrectProperties()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var vehicleCount = 5;
        var avgDays = 75;

        // Act
        var alert = DealerAlert.CreateSlowInventoryAlert(dealerId, vehicleCount, avgDays);

        // Assert
        alert.DealerId.Should().Be(dealerId);
        alert.Type.Should().Be(DealerAlertType.SlowInventory);
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.Status.Should().Be(AlertStatus.Unread);
        alert.Title.Should().Contain("Inventario Lento");
    }

    [Fact]
    public void DealerAlert_CreateLeadResponseSlowAlert_ShouldBeHighSeverity()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var pendingLeads = 5;
        var avgWaitMinutes = 180;

        // Act
        var alert = DealerAlert.CreateLeadResponseSlowAlert(dealerId, pendingLeads, avgWaitMinutes);

        // Assert
        alert.Severity.Should().Be(AlertSeverity.Critical);
        alert.Type.Should().Be(DealerAlertType.LeadResponseSlow);
    }

    [Fact]
    public void DealerAlert_MarkAsRead_ShouldUpdateStatus()
    {
        // Arrange
        var alert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = DealerAlertType.LowEngagement,
            Status = AlertStatus.Unread
        };

        // Act
        alert.MarkAsRead();

        // Assert
        alert.Status.Should().Be(AlertStatus.Read);
        alert.ReadAt.Should().NotBeNull();
    }

    [Fact]
    public void DealerAlert_Dismiss_ShouldUpdateStatus()
    {
        // Arrange
        var alert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = DealerAlertType.PriceDropNeeded,
            Status = AlertStatus.Read
        };

        // Act
        alert.Dismiss();

        // Assert
        alert.Status.Should().Be(AlertStatus.Dismissed);
    }

    [Fact]
    public void DealerAlert_MarkAsActedUpon_ShouldUpdateStatus()
    {
        // Arrange
        var alert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = DealerAlertType.Opportunity,
            Status = AlertStatus.Read
        };

        // Act
        alert.MarkAsActedUpon();

        // Assert
        alert.Status.Should().Be(AlertStatus.Acted);
    }

    [Fact]
    public void DealerAlert_IsExpired_ShouldReturnTrue_WhenPastExpiryDate()
    {
        // Arrange
        var alert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = DealerAlertType.PaymentDue,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Assert
        alert.IsExpired.Should().BeTrue();
    }

    #endregion

    #region InventoryAging Tests

    [Fact]
    public void InventoryAging_ShouldCalculateAvgDaysOnMarket()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Bucket0_15Days = 10,
            Bucket16_30Days = 8,
            Bucket31_45Days = 5,
            Bucket46_60Days = 3,
            Bucket61_90Days = 2,
            BucketOver90Days = 1
        };

        // Act
        var avgDays = aging.AvgDaysOnMarket;

        // Assert
        avgDays.Should().BeGreaterThan(0);
    }

    [Fact]
    public void InventoryAging_ShouldCalculateTotalVehicles()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Bucket0_15Days = 15,
            Bucket16_30Days = 12,
            Bucket31_45Days = 8,
            Bucket46_60Days = 4,
            Bucket61_90Days = 3,
            BucketOver90Days = 2
        };

        // Assert
        aging.TotalVehicles.Should().Be(44);
    }

    [Fact]
    public void InventoryAging_ShouldCalculateAgingRate()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Bucket0_15Days = 20,
            Bucket16_30Days = 15,
            Bucket31_45Days = 5,
            Bucket46_60Days = 4,
            Bucket61_90Days = 3,
            BucketOver90Days = 3
        };

        // Act - Aging rate = vehicles over 60 days / total * 100
        var agingRate = aging.AgingRate;

        // Assert - (3+3) / 50 * 100 = 12%
        agingRate.Should().BeApproximately(12.0m, 0.1m);
    }

    [Fact]
    public void InventoryAging_GetAgingBuckets_ShouldReturnAllBuckets()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Bucket0_15Days = 10,
            Bucket16_30Days = 8,
            Bucket31_45Days = 5,
            Bucket46_60Days = 3,
            Bucket61_90Days = 2,
            BucketOver90Days = 1,
            Value0_15Days = 250000,
            Value16_30Days = 200000,
            Value31_45Days = 125000,
            Value46_60Days = 75000,
            Value61_90Days = 50000,
            ValueOver90Days = 25000
        };

        // Act
        var buckets = aging.GetAgingBuckets();

        // Assert
        buckets.Should().HaveCount(6);
        buckets[0].Label.Should().Be("0-15 días");
        buckets[0].Count.Should().Be(10);
        buckets[0].Value.Should().Be(250000);
        buckets[5].Label.Should().Be("90+ días");
    }

    [Fact]
    public void InventoryAging_AtRiskValue_ShouldCalculateCorrectly()
    {
        // Arrange
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Value61_90Days = 150000,
            ValueOver90Days = 100000
        };

        // Assert
        aging.AtRiskValue.Should().Be(250000);
    }

    #endregion

    #region Enum Tests

    [Fact]
    public void DealerAlertType_ShouldHaveAllExpectedValues()
    {
        // Assert
        Enum.GetValues<DealerAlertType>().Should().HaveCountGreaterThan(10);
        Enum.IsDefined(DealerAlertType.SlowInventory).Should().BeTrue();
        Enum.IsDefined(DealerAlertType.PriceDropNeeded).Should().BeTrue();
        Enum.IsDefined(DealerAlertType.LowEngagement).Should().BeTrue();
        Enum.IsDefined(DealerAlertType.LeadResponseSlow).Should().BeTrue();
        Enum.IsDefined(DealerAlertType.CompetitorPrice).Should().BeTrue();
    }

    [Fact]
    public void AlertSeverity_ShouldHaveCorrectValues()
    {
        // Assert
        AlertSeverity.Info.Should().BeDefined();
        AlertSeverity.Warning.Should().BeDefined();
        AlertSeverity.Critical.Should().BeDefined();
    }

    [Fact]
    public void DealerTier_ShouldHaveCorrectOrder()
    {
        // Assert
        DealerTier.Bronze.Should().BeLessThan(DealerTier.Silver);
        DealerTier.Silver.Should().BeLessThan(DealerTier.Gold);
        DealerTier.Gold.Should().BeLessThan(DealerTier.Platinum);
        DealerTier.Platinum.Should().BeLessThan(DealerTier.Diamond);
    }

    #endregion
}
