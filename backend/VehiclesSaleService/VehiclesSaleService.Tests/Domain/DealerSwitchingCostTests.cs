using VehiclesSaleService.Domain.Entities;
using Xunit;

namespace VehiclesSaleService.Tests.Domain;

/// <summary>
/// Tests for the Dealer Switching Cost system — reviews, badges,
/// reputation non-portability, and retention level calculation.
/// </summary>
public class DealerSwitchingCostTests
{
    // ========================================
    // HasSignificantSwitchingCost Tests
    // ========================================

    [Fact]
    public void HasSignificantSwitchingCost_TrueWhen3OrMoreReviews()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 3,
            DaysAsMember = 5,
            TotalViewsAcrossVehicles = 10,
            TotalSalesCompleted = 0
        };

        Assert.True(summary.HasSignificantSwitchingCost);
    }

    [Fact]
    public void HasSignificantSwitchingCost_TrueWhen30DaysMember()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 0,
            DaysAsMember = 30,
            TotalViewsAcrossVehicles = 0,
            TotalSalesCompleted = 0
        };

        Assert.True(summary.HasSignificantSwitchingCost);
    }

    [Fact]
    public void HasSignificantSwitchingCost_TrueWhen50PlusViews()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 0,
            DaysAsMember = 10,
            TotalViewsAcrossVehicles = 50,
            TotalSalesCompleted = 0
        };

        Assert.True(summary.HasSignificantSwitchingCost);
    }

    [Fact]
    public void HasSignificantSwitchingCost_TrueWhenHasSales()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 0,
            DaysAsMember = 1,
            TotalViewsAcrossVehicles = 0,
            TotalSalesCompleted = 1
        };

        Assert.True(summary.HasSignificantSwitchingCost);
    }

    [Fact]
    public void HasSignificantSwitchingCost_FalseWhenNew()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 0,
            DaysAsMember = 5,
            TotalViewsAcrossVehicles = 20,
            TotalSalesCompleted = 0
        };

        Assert.False(summary.HasSignificantSwitchingCost);
    }

    // ========================================
    // RetentionLevel Tests
    // ========================================

    [Fact]
    public void RetentionPriority_Critical_When50PlusReviews()
    {
        var summary = new DealerSwitchingCostSummary { TotalReviews = 55 };
        Assert.Equal(RetentionLevel.Critical, summary.RetentionPriority);
    }

    [Fact]
    public void RetentionPriority_High_When20To49Reviews()
    {
        var summary = new DealerSwitchingCostSummary { TotalReviews = 25 };
        Assert.Equal(RetentionLevel.High, summary.RetentionPriority);
    }

    [Fact]
    public void RetentionPriority_Medium_When5To19Reviews()
    {
        var summary = new DealerSwitchingCostSummary { TotalReviews = 10 };
        Assert.Equal(RetentionLevel.Medium, summary.RetentionPriority);
    }

    [Fact]
    public void RetentionPriority_Low_WhenFewReviewsButHasCost()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 2,
            DaysAsMember = 60,
            TotalSalesCompleted = 3
        };
        Assert.Equal(RetentionLevel.Low, summary.RetentionPriority);
    }

    [Fact]
    public void RetentionPriority_None_WhenBrandNew()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 0,
            DaysAsMember = 5,
            TotalViewsAcrossVehicles = 10,
            TotalSalesCompleted = 0
        };
        Assert.Equal(RetentionLevel.None, summary.RetentionPriority);
    }

    // ========================================
    // CancellationWarning Tests
    // ========================================

    [Fact]
    public void CancellationWarning_ContainsReviewData()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 156,
            AverageRating = 4.8m,
            TotalBadges = 3,
            ActiveBadges = new List<string> { "Top Rated", "Trusted Dealer", "5-Star Seller" },
            TotalViewsAcrossVehicles = 5000,
            TotalInquiriesAcrossVehicles = 120,
            TotalSalesCompleted = 45,
            DaysAsMember = 365
        };

        var warning = summary.CancellationWarning;

        Assert.Contains("156 reseñas", warning);
        Assert.Contains("4.8★", warning);
        Assert.Contains("3 insignias", warning);
        Assert.Contains("Top Rated", warning);
        Assert.Contains("5,000 vistas", warning);
        Assert.Contains("120 consultas", warning);
        Assert.Contains("45 ventas", warning);
        Assert.Contains("365 días", warning);
        Assert.Contains("EXCLUSIVA de OKLA", warning);
        Assert.Contains("NO es transferible", warning);
    }

    [Fact]
    public void CancellationWarning_NewDealer_NoSignificantHistory()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 0,
            DaysAsMember = 2,
            TotalViewsAcrossVehicles = 5
        };

        Assert.Contains("no tiene historial significativo", summary.CancellationWarning);
    }

    [Fact]
    public void CancellationWarning_MinimalDealer_ShowsOnlyRelevantData()
    {
        var summary = new DealerSwitchingCostSummary
        {
            TotalReviews = 5,
            AverageRating = 4.0m,
            TotalBadges = 0,
            ActiveBadges = new List<string>(),
            TotalViewsAcrossVehicles = 0,
            TotalInquiriesAcrossVehicles = 0,
            TotalSalesCompleted = 0,
            DaysAsMember = 0
        };

        var warning = summary.CancellationWarning;

        // Should show reviews but not other empty metrics
        Assert.Contains("5 reseñas", warning);
        Assert.DoesNotContain("insignias", warning);
        Assert.DoesNotContain("vistas", warning);
        Assert.DoesNotContain("consultas", warning);
        Assert.DoesNotContain("ventas", warning);
        Assert.DoesNotContain("días", warning);
    }

    // ========================================
    // Real-world Scenario Tests
    // ========================================

    [Fact]
    public void Scenario_TopDealer_MaxRetention()
    {
        var summary = new DealerSwitchingCostSummary
        {
            DealerId = Guid.NewGuid(),
            DealerName = "Auto Plaza Santo Domingo",
            TotalReviews = 200,
            VerifiedPurchaseReviews = 180,
            AverageRating = 4.9m,
            TotalBadges = 5,
            ActiveBadges = new List<string>
            {
                "Top Rated", "Trusted Dealer", "5-Star Seller",
                "Volume Leader", "Community Favorite"
            },
            PositiveReviewPercentage = 98.5m,
            ActiveVehicles = 45,
            TotalVehiclesEverPublished = 200,
            TotalViewsAcrossVehicles = 50000,
            TotalInquiriesAcrossVehicles = 800,
            TotalFavoritesAcrossVehicles = 2500,
            TotalPriceHistoryRecords = 350,
            AveragePlatformScore = 92,
            TotalLeadsReceived = 1200,
            TotalSalesCompleted = 150,
            DaysAsMember = 730
        };

        Assert.True(summary.HasSignificantSwitchingCost);
        Assert.Equal(RetentionLevel.Critical, summary.RetentionPriority);
        Assert.Contains("EXCLUSIVA de OKLA", summary.CancellationWarning);
        Assert.Contains("200 reseñas", summary.CancellationWarning);
    }

    [Fact]
    public void Scenario_NewDealer_FirstWeek()
    {
        var summary = new DealerSwitchingCostSummary
        {
            DealerId = Guid.NewGuid(),
            DealerName = "Nuevo Motors",
            TotalReviews = 0,
            AverageRating = 0,
            TotalBadges = 0,
            ActiveBadges = new List<string>(),
            ActiveVehicles = 3,
            TotalVehiclesEverPublished = 3,
            TotalViewsAcrossVehicles = 15,
            TotalInquiriesAcrossVehicles = 0,
            TotalSalesCompleted = 0,
            DaysAsMember = 5
        };

        Assert.False(summary.HasSignificantSwitchingCost);
        Assert.Equal(RetentionLevel.None, summary.RetentionPriority);
    }

    // ========================================
    // Enum Tests
    // ========================================

    [Fact]
    public void RetentionLevel_AllValues_AreDefined()
    {
        var values = Enum.GetValues<RetentionLevel>();
        Assert.Equal(5, values.Length);
        Assert.Contains(RetentionLevel.None, values);
        Assert.Contains(RetentionLevel.Low, values);
        Assert.Contains(RetentionLevel.Medium, values);
        Assert.Contains(RetentionLevel.High, values);
        Assert.Contains(RetentionLevel.Critical, values);
    }
}
