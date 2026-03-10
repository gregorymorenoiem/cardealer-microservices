using VehiclesSaleService.Domain.Entities;
using Xunit;

namespace VehiclesSaleService.Tests.Domain;

/// <summary>
/// Tests for the OKLA Platform Score system — vehicle history tracking,
/// price transparency, buyer engagement, and switching cost mechanism.
/// </summary>
public class OklaPlatformScoreTests
{
    // ========================================
    // VehiclePriceHistory Entity Tests
    // ========================================

    [Fact]
    public void VehiclePriceHistory_PriceDifference_CalculatesCorrectly()
    {
        var history = new VehiclePriceHistory
        {
            OldPrice = 25000m,
            NewPrice = 22000m,
            Currency = "DOP"
        };

        Assert.Equal(-3000m, history.PriceDifference);
    }

    [Fact]
    public void VehiclePriceHistory_ChangePercentage_CalculatesCorrectly()
    {
        var history = new VehiclePriceHistory
        {
            OldPrice = 25000m,
            NewPrice = 22500m
        };

        // (22500 - 25000) / 25000 * 100 = -10.00
        Assert.Equal(-10.00m, history.ChangePercentage);
    }

    [Fact]
    public void VehiclePriceHistory_ChangePercentage_ZeroOldPrice_ReturnsZero()
    {
        var history = new VehiclePriceHistory
        {
            OldPrice = 0,
            NewPrice = 15000m
        };

        Assert.Equal(0m, history.ChangePercentage);
    }

    [Fact]
    public void VehiclePriceHistory_PriceIncrease_PositiveDifference()
    {
        var history = new VehiclePriceHistory
        {
            OldPrice = 20000m,
            NewPrice = 22000m
        };

        Assert.True(history.PriceDifference > 0);
        Assert.True(history.ChangePercentage > 0);
        Assert.Equal(10.00m, history.ChangePercentage);
    }

    [Fact]
    public void VehiclePriceHistory_DefaultValues_AreCorrect()
    {
        var history = new VehiclePriceHistory();

        Assert.NotEqual(Guid.Empty, history.Id);
        Assert.Equal("DOP", history.Currency);
        Assert.Equal(PriceChangeType.Manual, history.ChangeType);
        Assert.True(history.ChangedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void VehiclePriceHistory_InitialListing_HasZeroOldPrice()
    {
        var history = new VehiclePriceHistory
        {
            OldPrice = 0,
            NewPrice = 35000m,
            ChangeType = PriceChangeType.InitialListing
        };

        Assert.Equal(PriceChangeType.InitialListing, history.ChangeType);
        Assert.Equal(0m, history.OldPrice);
        Assert.Equal(35000m, history.NewPrice);
    }

    // ========================================
    // OklaPlatformScore Tests
    // ========================================

    [Fact]
    public void OklaPlatformScore_TotalScore_SumsAllDimensions()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 25,
            BuyerEngagementPoints = 20,
            PriceTransparencyPoints = 15,
            SellerReputationPoints = 10,
            CompletenessPoints = 8
        };

        Assert.Equal(78, score.TotalScore);
    }

    [Fact]
    public void OklaPlatformScore_Level_Platinum_Above85()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 25,
            BuyerEngagementPoints = 30,
            PriceTransparencyPoints = 20,
            SellerReputationPoints = 15,
            CompletenessPoints = 10
        };

        Assert.Equal(100, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.Platinum, score.Level);
    }

    [Fact]
    public void OklaPlatformScore_Level_Gold_65To84()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 25,
            BuyerEngagementPoints = 20,
            PriceTransparencyPoints = 15,
            SellerReputationPoints = 5,
            CompletenessPoints = 5
        };

        Assert.Equal(70, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.Gold, score.Level);
    }

    [Fact]
    public void OklaPlatformScore_Level_Silver_45To64()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 15,
            BuyerEngagementPoints = 15,
            PriceTransparencyPoints = 10,
            SellerReputationPoints = 5,
            CompletenessPoints = 5
        };

        Assert.Equal(50, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.Silver, score.Level);
    }

    [Fact]
    public void OklaPlatformScore_Level_Bronze_25To44()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 5,
            BuyerEngagementPoints = 10,
            PriceTransparencyPoints = 10,
            SellerReputationPoints = 5,
            CompletenessPoints = 0
        };

        Assert.Equal(30, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.Bronze, score.Level);
    }

    [Fact]
    public void OklaPlatformScore_Level_New_Below25()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 5,
            BuyerEngagementPoints = 5,
            PriceTransparencyPoints = 10,
            SellerReputationPoints = 0,
            CompletenessPoints = 0
        };

        Assert.Equal(20, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.New, score.Level);
    }

    [Fact]
    public void OklaPlatformScore_LevelDescription_ContainsRelevantText()
    {
        var score = new OklaPlatformScore
        {
            AntiquityPoints = 25,
            BuyerEngagementPoints = 30,
            PriceTransparencyPoints = 20,
            SellerReputationPoints = 15,
            CompletenessPoints = 10
        };

        Assert.Contains("excepcional", score.LevelDescription);
        Assert.Contains("transparencia", score.LevelDescription);
    }

    [Fact]
    public void OklaPlatformScore_NewVehicle_HasZeroScore()
    {
        var score = new OklaPlatformScore();

        Assert.Equal(0, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.New, score.Level);
        Assert.Contains("nuevo", score.LevelDescription);
    }

    // ========================================
    // SwitchingCostSummary Tests
    // ========================================

    [Fact]
    public void SwitchingCostSummary_HasSignificantCost_TrueWhenDaysAbove7()
    {
        var summary = new SwitchingCostSummary
        {
            DaysAccumulated = 10,
            ViewsAccumulated = 0,
            InquiriesAccumulated = 0,
            FavoritesAccumulated = 0
        };

        Assert.True(summary.HasSignificantCost);
    }

    [Fact]
    public void SwitchingCostSummary_HasSignificantCost_TrueWhenViewsAbove10()
    {
        var summary = new SwitchingCostSummary
        {
            DaysAccumulated = 3,
            ViewsAccumulated = 15,
            InquiriesAccumulated = 0,
            FavoritesAccumulated = 0
        };

        Assert.True(summary.HasSignificantCost);
    }

    [Fact]
    public void SwitchingCostSummary_HasSignificantCost_TrueWhenHasInquiries()
    {
        var summary = new SwitchingCostSummary
        {
            DaysAccumulated = 1,
            ViewsAccumulated = 5,
            InquiriesAccumulated = 1,
            FavoritesAccumulated = 0
        };

        Assert.True(summary.HasSignificantCost);
    }

    [Fact]
    public void SwitchingCostSummary_HasSignificantCost_FalseWhenNew()
    {
        var summary = new SwitchingCostSummary
        {
            DaysAccumulated = 2,
            ViewsAccumulated = 5,
            InquiriesAccumulated = 0,
            FavoritesAccumulated = 0
        };

        Assert.False(summary.HasSignificantCost);
    }

    [Fact]
    public void SwitchingCostSummary_WarningMessage_ContainsAccumulatedData()
    {
        var summary = new SwitchingCostSummary
        {
            DaysAccumulated = 45,
            ViewsAccumulated = 230,
            InquiriesAccumulated = 12,
            FavoritesAccumulated = 8,
            PriceHistoryRecords = 3,
            CurrentLevel = PlatformScoreLevel.Gold
        };

        Assert.Contains("45 días", summary.WarningMessage);
        Assert.Contains("230 vistas", summary.WarningMessage);
        Assert.Contains("12 consultas", summary.WarningMessage);
        Assert.Contains("8 favoritos", summary.WarningMessage);
        Assert.Contains("Gold", summary.WarningMessage);
        Assert.Contains("NO es transferible", summary.WarningMessage);
    }

    [Fact]
    public void SwitchingCostSummary_WarningMessage_NewVehicle_HasNoHistory()
    {
        var summary = new SwitchingCostSummary
        {
            DaysAccumulated = 0,
            ViewsAccumulated = 0,
            InquiriesAccumulated = 0,
            FavoritesAccumulated = 0
        };

        Assert.Contains("no tiene historial", summary.WarningMessage);
    }

    // ========================================
    // PriceTrend Tests
    // ========================================

    [Fact]
    public void PriceTrend_AllValues_AreDefined()
    {
        var values = Enum.GetValues<PriceTrend>();
        Assert.Contains(PriceTrend.Stable, values);
        Assert.Contains(PriceTrend.Decreasing, values);
        Assert.Contains(PriceTrend.Increasing, values);
        Assert.Contains(PriceTrend.Volatile, values);
    }

    [Fact]
    public void PriceChangeType_AllValues_AreDefined()
    {
        var values = Enum.GetValues<PriceChangeType>();
        Assert.Contains(PriceChangeType.InitialListing, values);
        Assert.Contains(PriceChangeType.Manual, values);
        Assert.Contains(PriceChangeType.CampaignAdjustment, values);
        Assert.Contains(PriceChangeType.AutoReduction, values);
        Assert.Contains(PriceChangeType.AdminAdjustment, values);
    }

    [Fact]
    public void PlatformScoreLevel_AllValues_AreDefined()
    {
        var values = Enum.GetValues<PlatformScoreLevel>();
        Assert.Contains(PlatformScoreLevel.New, values);
        Assert.Contains(PlatformScoreLevel.Bronze, values);
        Assert.Contains(PlatformScoreLevel.Silver, values);
        Assert.Contains(PlatformScoreLevel.Gold, values);
        Assert.Contains(PlatformScoreLevel.Platinum, values);
    }

    // ========================================
    // Vehicle PriceHistory Navigation Tests
    // ========================================

    [Fact]
    public void Vehicle_PriceHistory_IsInitializedAsEmptyCollection()
    {
        var vehicle = new Vehicle();
        Assert.NotNull(vehicle.PriceHistory);
        Assert.Empty(vehicle.PriceHistory);
    }

    [Fact]
    public void Vehicle_PriceHistory_CanAddMultipleEntries()
    {
        var vehicle = new Vehicle();
        vehicle.PriceHistory.Add(new VehiclePriceHistory
        {
            OldPrice = 0,
            NewPrice = 30000m,
            ChangeType = PriceChangeType.InitialListing
        });
        vehicle.PriceHistory.Add(new VehiclePriceHistory
        {
            OldPrice = 30000m,
            NewPrice = 28000m,
            ChangeType = PriceChangeType.Manual
        });
        vehicle.PriceHistory.Add(new VehiclePriceHistory
        {
            OldPrice = 28000m,
            NewPrice = 25000m,
            ChangeType = PriceChangeType.Manual
        });

        Assert.Equal(3, vehicle.PriceHistory.Count);
    }

    // ========================================
    // Integration-like scoring scenarios
    // ========================================

    [Fact]
    public void PlatformScore_RealWorldScenario_DealerWith45Days()
    {
        // Scenario: A dealer has a Toyota Camry listed for 45 days
        // with good engagement metrics
        var score = new OklaPlatformScore
        {
            DaysOnPlatform = 45,
            AntiquityPoints = 25,  // >= 31 days

            TotalViews = 230,
            TotalFavorites = 8,
            TotalInquiries = 12,
            BuyerEngagementPoints = 30, // max

            PriceChangeCount = 2,
            PriceTrend = PriceTrend.Decreasing,
            TotalPriceVariation = -8.5m,
            PriceTransparencyPoints = 20,

            SellerRating = 4.5m,
            SellerReviewCount = 15,
            SellerVerified = true,
            SellerReputationPoints = 15,

            PhotoCount = 12,
            DescriptionLength = 350,
            HasVerifiedVin = true,
            HasExternalHistory = true,
            CompletenessPoints = 10,

            SwitchingCost = new SwitchingCostSummary
            {
                DaysAccumulated = 45,
                ViewsAccumulated = 230,
                InquiriesAccumulated = 12,
                FavoritesAccumulated = 8,
                PriceHistoryRecords = 3,
                CurrentPlatformScore = 100,
                CurrentLevel = PlatformScoreLevel.Platinum
            }
        };

        Assert.Equal(100, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.Platinum, score.Level);
        Assert.True(score.SwitchingCost.HasSignificantCost);
        Assert.Contains("45 días", score.SwitchingCost.WarningMessage);
        Assert.Contains("NO es transferible", score.SwitchingCost.WarningMessage);
    }

    [Fact]
    public void PlatformScore_NewListing_JustPublished()
    {
        // Scenario: Vehicle just published today
        var score = new OklaPlatformScore
        {
            DaysOnPlatform = 0,
            AntiquityPoints = 0,

            TotalViews = 0,
            TotalFavorites = 0,
            TotalInquiries = 0,
            BuyerEngagementPoints = 0,

            PriceChangeCount = 0,
            PriceTrend = PriceTrend.Stable,
            PriceTransparencyPoints = 10, // Base for no changes

            SellerRating = null,
            SellerReviewCount = null,
            SellerVerified = false,
            SellerReputationPoints = 0,

            PhotoCount = 3,
            DescriptionLength = 50,
            HasVerifiedVin = false,
            HasExternalHistory = false,
            CompletenessPoints = 3 // 3 photos + short description
        };

        Assert.Equal(13, score.TotalScore);
        Assert.Equal(PlatformScoreLevel.New, score.Level);
        Assert.False(score.SwitchingCost.HasSignificantCost);
    }
}
