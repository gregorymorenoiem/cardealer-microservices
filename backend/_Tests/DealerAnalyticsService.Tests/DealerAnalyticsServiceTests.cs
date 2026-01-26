using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using DealerAnalyticsService.Domain.Entities;
using DealerAnalyticsService.Domain.Enums;
using DealerAnalyticsService.Infrastructure.Persistence;

namespace DealerAnalyticsService.Tests;

/// <summary>
/// Sprint 12 - Dashboard Avanzado: Tests Completos del DealerAnalyticsService
/// Updated to match current Domain Entities
/// </summary>
public class DealerAnalyticsServiceTests
{
    #region Domain Tests - DealerAnalytic

    [Fact]
    public void DealerAnalytic_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        // Act
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Date = date,
            TotalViews = 1250,
            UniqueViews = 850,
            TotalContacts = 45,
            ActualSales = 12,
            TotalRevenue = 125000.50m,
            AverageVehiclePrice = 10416.71m,
            ActiveListings = 25,
            AverageDaysOnMarket = 15.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        analytic.Should().NotBeNull();
        analytic.DealerId.Should().Be(dealerId);
        analytic.TotalViews.Should().Be(1250);
        analytic.UniqueViews.Should().Be(850);
        analytic.TotalContacts.Should().Be(45);
        analytic.ActualSales.Should().Be(12);
    }

    [Fact]
    public void DealerAnalytic_ShouldTrack_AllContactTypes()
    {
        // Arrange & Act
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            PhoneCalls = 20,
            WhatsAppMessages = 15,
            EmailInquiries = 10,
            TotalContacts = 45
        };

        // Assert
        var totalContactTypes = analytic.PhoneCalls + analytic.WhatsAppMessages + analytic.EmailInquiries;
        totalContactTypes.Should().Be(analytic.TotalContacts);
    }

    #endregion

    #region Domain Tests - ConversionFunnel

    [Fact]
    public void ConversionFunnel_ShouldStoreCorrectRates()
    {
        // Arrange & Act - ConversionFunnel stores rates, doesn't calculate them
        var funnel = new ConversionFunnel
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            TotalViews = 5000,
            TotalContacts = 150,
            ViewToContactRate = 3.0m, // Pre-calculated: 150/5000 * 100
            TestDriveRequests = 75,
            ContactToTestDriveRate = 50.0m, // Pre-calculated: 75/150 * 100
            ActualSales = 25,
            TestDriveToSaleRate = 33.33m, // Pre-calculated: 25/75 * 100
            OverallConversionRate = 0.5m, // Pre-calculated: 25/5000 * 100
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        funnel.TotalViews.Should().Be(5000);
        funnel.TotalContacts.Should().Be(150);
        funnel.TestDriveRequests.Should().Be(75);
        funnel.ActualSales.Should().Be(25);
        funnel.ViewToContactRate.Should().Be(3.0m);
        funnel.ContactToTestDriveRate.Should().Be(50.0m);
        funnel.TestDriveToSaleRate.Should().Be(33.33m);
        funnel.OverallConversionRate.Should().Be(0.5m);
    }

    [Fact]
    public void ConversionFunnel_ViewToContactRate_ShouldBeZero_WhenNoViews()
    {
        // Arrange & Act
        var funnel = new ConversionFunnel
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            TotalViews = 0,
            TotalContacts = 0
        };

        // Assert
        funnel.ViewToContactRate.Should().Be(0);
    }

    #endregion

    #region Domain Tests - MarketBenchmark

    [Fact]
    public void MarketBenchmark_ShouldHaveValidData()
    {
        // Arrange & Act
        var benchmark = new MarketBenchmark
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            VehicleCategory = "SUV",
            PriceRange = "1M-2M",
            MarketAveragePrice = 1500000m,
            MarketAverageDaysOnMarket = 25.5m,
            MarketAverageViews = 150m,
            MarketConversionRate = 2.5m,
            PricePercentile25 = 1200000m,
            PricePercentile50 = 1500000m,
            PricePercentile75 = 1800000m,
            TotalDealersInSample = 50,
            TotalVehiclesInSample = 500,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        benchmark.Should().NotBeNull();
        benchmark.VehicleCategory.Should().Be("SUV");
        benchmark.MarketAveragePrice.Should().Be(1500000m);
        benchmark.TotalDealersInSample.Should().Be(50);
    }

    [Fact]
    public void MarketBenchmark_ShouldHaveCorrectPercentiles()
    {
        // Arrange & Act
        var benchmark = new MarketBenchmark
        {
            Id = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            PricePercentile25 = 100000m,
            PricePercentile50 = 150000m,
            PricePercentile75 = 200000m
        };

        // Assert
        benchmark.PricePercentile25.Should().BeLessThan(benchmark.PricePercentile50);
        benchmark.PricePercentile50.Should().BeLessThan(benchmark.PricePercentile75);
    }

    #endregion

    #region Domain Tests - DealerInsight

    [Fact]
    public void DealerInsight_ShouldBeCreated_WithCorrectPriority()
    {
        // Arrange & Act
        var insight = new DealerInsight
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Type = InsightType.PricingRecommendation,
            Priority = InsightPriority.High,
            Title = "Ajuste de precio recomendado",
            Description = "Su vehículo está por encima del precio de mercado en un 15%",
            ActionRecommendation = "Reducir el precio en RD$50,000 para mejorar visibilidad",
            PotentialImpact = 23m,
            Confidence = 85m,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        insight.Should().NotBeNull();
        insight.Type.Should().Be(InsightType.PricingRecommendation);
        insight.Priority.Should().Be(InsightPriority.High);
        insight.IsRead.Should().BeFalse();
        insight.Title.Should().NotBeNullOrEmpty();
        insight.ActionRecommendation.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DealerInsight_AllInsightTypes_ShouldBeValid()
    {
        // Arrange & Act
        var types = Enum.GetValues<InsightType>();

        // Assert
        types.Should().Contain(InsightType.PricingRecommendation);
        types.Should().Contain(InsightType.InventoryOptimization);
        types.Should().Contain(InsightType.MarketingStrategy);
        types.Should().Contain(InsightType.SeasonalTrend);
    }

    [Fact]
    public void DealerInsight_AllPriorities_ShouldBeValid()
    {
        // Arrange & Act
        var priorities = Enum.GetValues<InsightPriority>();

        // Assert
        priorities.Should().Contain(InsightPriority.Low);
        priorities.Should().Contain(InsightPriority.Medium);
        priorities.Should().Contain(InsightPriority.High);
        priorities.Should().Contain(InsightPriority.Critical);
    }

    #endregion

    #region Domain Tests - DealerAlert

    [Fact]
    public void DealerAlert_ShouldBeCreated_WithValidEnums()
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
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        alert.Should().NotBeNull();
        alert.Type.Should().Be(DealerAlertType.LowInventory);
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.Status.Should().Be(AlertStatus.Active);
    }

    [Fact]
    public void DealerAlert_IsExpired_ShouldWork()
    {
        // Arrange
        var expiredAlert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        
        var activeAlert = new DealerAlert
        {
            Id = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        expiredAlert.IsExpired.Should().BeTrue();
        activeAlert.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void DealerAlertType_ShouldHaveInventoryAlerts()
    {
        // Assert
        Enum.IsDefined(typeof(DealerAlertType), DealerAlertType.LowInventory).Should().BeTrue();
        Enum.IsDefined(typeof(DealerAlertType), DealerAlertType.AgingInventory).Should().BeTrue();
        Enum.IsDefined(typeof(DealerAlertType), DealerAlertType.PriceAdjustmentNeeded).Should().BeTrue();
    }

    [Fact]
    public void DealerAlertType_ShouldHaveEngagementAlerts()
    {
        // Assert
        Enum.IsDefined(typeof(DealerAlertType), DealerAlertType.ViewsDropping).Should().BeTrue();
        Enum.IsDefined(typeof(DealerAlertType), DealerAlertType.LowContactRate).Should().BeTrue();
    }

    #endregion

    #region Domain Tests - DealerSnapshot

    [Fact]
    public void DealerSnapshot_ShouldCalculate_ClickThroughRate()
    {
        // Arrange & Act
        var snapshot = new DealerSnapshot
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            SnapshotDate = DateTime.UtcNow.Date,
            SearchImpressions = 1000,
            SearchClicks = 50,
            TotalViews = 500,
            TotalContacts = 25,
            TotalFavorites = 10
        };

        // Assert
        snapshot.ClickThroughRate.Should().BeApproximately(5.0, 0.01); // 50/1000 * 100
        snapshot.ContactRate.Should().BeApproximately(5.0, 0.01); // 25/500 * 100
        snapshot.FavoriteRate.Should().BeApproximately(2.0, 0.01); // 10/500 * 100
    }

    [Fact]
    public void DealerSnapshot_Rates_ShouldBeZero_WhenNoData()
    {
        // Arrange & Act
        var snapshot = new DealerSnapshot
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            SearchImpressions = 0,
            TotalViews = 0
        };

        // Assert
        snapshot.ClickThroughRate.Should().Be(0);
        snapshot.ContactRate.Should().Be(0);
        snapshot.FavoriteRate.Should().Be(0);
    }

    #endregion

    #region Domain Tests - DealerBenchmark

    [Fact]
    public void DealerBenchmark_IsBetterThanMarket_ShouldWork()
    {
        // Arrange & Act
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
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
        benchmark.IsBetterThanMarketDaysOnMarket.Should().BeTrue(); // 20 < 30
        benchmark.IsBetterThanMarketConversion.Should().BeTrue(); // 5 > 3
        benchmark.IsBetterThanMarketResponseTime.Should().BeTrue(); // 30 < 60
        benchmark.IsBetterThanMarketSatisfaction.Should().BeTrue(); // 4.5 > 4.0
    }

    [Fact]
    public void DealerBenchmark_CalculateTier_ShouldWork()
    {
        // Arrange
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            ConversionRatePercentile = 95,
            ResponseTimePercentile = 90,
            SatisfactionPercentile = 92,
            ListingQualityPercentile = 88,
            EngagementPercentile = 85
        };

        // Act
        benchmark.CalculateTier();

        // Assert
        benchmark.Tier.Should().Be(DealerTier.Diamond); // avg = 90, >= 90 = Diamond
    }

    [Fact]
    public void DealerTier_ShouldHaveCorrectValues()
    {
        // Assert
        Enum.IsDefined(typeof(DealerTier), DealerTier.Bronze).Should().BeTrue();
        Enum.IsDefined(typeof(DealerTier), DealerTier.Silver).Should().BeTrue();
        Enum.IsDefined(typeof(DealerTier), DealerTier.Gold).Should().BeTrue();
        Enum.IsDefined(typeof(DealerTier), DealerTier.Platinum).Should().BeTrue();
        Enum.IsDefined(typeof(DealerTier), DealerTier.Diamond).Should().BeTrue();
    }

    #endregion

    #region Domain Tests - LeadFunnelMetrics

    [Fact]
    public void LeadFunnelMetrics_ShouldCalculate_ConversionRates()
    {
        // Arrange & Act
        var funnel = new LeadFunnelMetrics
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            PeriodStart = DateTime.UtcNow.AddDays(-30),
            PeriodEnd = DateTime.UtcNow,
            Impressions = 10000,
            Views = 2000,
            Contacts = 200,
            Qualified = 50,
            Negotiation = 20,
            Converted = 10
        };

        // Assert
        funnel.ImpressionsToViews.Should().BeApproximately(20.0, 0.01); // 2000/10000*100
        funnel.ViewsToContacts.Should().BeApproximately(10.0, 0.01); // 200/2000*100
        funnel.ContactsToQualified.Should().BeApproximately(25.0, 0.01); // 50/200*100
        funnel.QualifiedToNegotiation.Should().BeApproximately(40.0, 0.01); // 20/50*100
        funnel.NegotiationToConverted.Should().BeApproximately(50.0, 0.01); // 10/20*100
        funnel.OverallConversion.Should().BeApproximately(0.1, 0.01); // 10/10000*100
    }

    [Fact]
    public void LeadFunnelMetrics_Rates_ShouldBeZero_WhenNoData()
    {
        // Arrange & Act
        var funnel = new LeadFunnelMetrics
        {
            Id = Guid.NewGuid(),
            Impressions = 0,
            Views = 0,
            Contacts = 0
        };

        // Assert
        funnel.ImpressionsToViews.Should().Be(0);
        funnel.ViewsToContacts.Should().Be(0);
        funnel.OverallConversion.Should().Be(0);
    }

    #endregion

    #region Domain Tests - InventoryAging

    [Fact]
    public void InventoryAging_ShouldCalculate_TotalVehicles()
    {
        // Arrange & Act
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            Vehicles0To15Days = 10,
            Vehicles16To30Days = 8,
            Vehicles31To45Days = 5,
            Vehicles46To60Days = 3,
            Vehicles61To90Days = 2,
            VehiclesOver90Days = 1
        };

        // Assert
        aging.TotalVehicles.Should().Be(29);
    }

    [Fact]
    public void InventoryAging_ShouldCalculate_PercentFresh()
    {
        // Arrange & Act
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            Vehicles0To15Days = 15,
            Vehicles16To30Days = 10,
            Vehicles31To45Days = 5,
            Vehicles46To60Days = 5,
            Vehicles61To90Days = 3,
            VehiclesOver90Days = 2
        };

        // Assert - Fresh = 0-15 + 16-30 = 25 out of 40 = 62.5%
        aging.PercentFresh.Should().BeApproximately(62.5, 0.1);
    }

    [Fact]
    public void InventoryAging_ShouldCalculate_AtRiskMetrics()
    {
        // Arrange & Act
        var aging = new InventoryAging
        {
            Id = Guid.NewGuid(),
            Vehicles61To90Days = 3,
            VehiclesOver90Days = 2,
            Value61To90Days = 3000000m,
            ValueOver90Days = 2000000m
        };

        // Assert
        aging.AtRiskCount.Should().Be(5);
        aging.AtRiskValue.Should().Be(5000000m);
    }

    #endregion

    #region Domain Tests - VehiclePerformance

    [Fact]
    public void VehiclePerformance_ShouldCalculate_ClickThroughRate()
    {
        // Arrange & Act
        var perf = new VehiclePerformance
        {
            VehicleId = Guid.NewGuid(),
            DealerId = Guid.NewGuid(),
            Date = DateTime.UtcNow.Date,
            SearchImpressions = 500,
            SearchClicks = 25,
            Views = 100,
            Contacts = 5,
            Favorites = 10
        };

        // Assert
        perf.ClickThroughRate.Should().BeApproximately(5.0, 0.01); // 25/500*100
        perf.ContactRate.Should().BeApproximately(5.0, 0.01); // 5/100*100
        perf.FavoriteRate.Should().BeApproximately(10.0, 0.01); // 10/100*100
    }

    [Fact]
    public void VehiclePerformance_Constructor_ShouldSetDefaults()
    {
        // Arrange & Act
        var perf = new VehiclePerformance();

        // Assert
        perf.Id.Should().NotBeEmpty();
        perf.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Domain Tests - ProfileView

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
    public void ProfileView_IsBounce_ShouldReturnTrue_WhenDurationLessThan10Seconds()
    {
        // Arrange
        var view = new ProfileView { DurationSeconds = 5 };

        // Act & Assert
        view.IsBounce().Should().BeTrue();
    }

    [Fact]
    public void ProfileView_IsEngagedVisit_ShouldWork()
    {
        // Arrange
        var engagedView = new ProfileView { DurationSeconds = 150 };
        var quickView = new ProfileView { DurationSeconds = 30 };

        // Assert
        engagedView.IsEngagedVisit().Should().BeTrue();
        quickView.IsEngagedVisit().Should().BeFalse();
    }

    #endregion

    #region Infrastructure Tests - DbContext

    [Fact]
    public async Task DealerAnalyticsDbContext_ShouldSaveAndRetrieve_DealerAnalytic()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dealerId = Guid.NewGuid();
        var analytic = new DealerAnalytic
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Date = DateTime.UtcNow.Date,
            TotalViews = 1000,
            TotalContacts = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await using (var context = new DealerAnalyticsDbContext(options))
        {
            context.DealerAnalytics.Add(analytic);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new DealerAnalyticsDbContext(options))
        {
            var retrieved = await context.DealerAnalytics
                .FirstOrDefaultAsync(a => a.DealerId == dealerId);
            
            retrieved.Should().NotBeNull();
            retrieved!.TotalViews.Should().Be(1000);
            retrieved.TotalContacts.Should().Be(50);
        }
    }

    [Fact]
    public async Task DealerAnalyticsDbContext_ShouldSaveAndRetrieve_DealerBenchmark()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DealerAnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dealerId = Guid.NewGuid();
        var benchmark = new DealerBenchmark
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Date = DateTime.UtcNow.Date,
            ConversionRate = 5.5,
            Tier = DealerTier.Gold,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await using (var context = new DealerAnalyticsDbContext(options))
        {
            context.DealerBenchmarks.Add(benchmark);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new DealerAnalyticsDbContext(options))
        {
            var retrieved = await context.DealerBenchmarks
                .FirstOrDefaultAsync(b => b.DealerId == dealerId);
            
            retrieved.Should().NotBeNull();
            retrieved!.ConversionRate.Should().Be(5.5);
            retrieved.Tier.Should().Be(DealerTier.Gold);
        }
    }

    #endregion
}
