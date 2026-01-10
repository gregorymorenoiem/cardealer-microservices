using Xunit;
using FluentAssertions;
using VehicleIntelligenceService.Domain.Entities;
using VehicleIntelligenceService.Domain.Interfaces;
using VehicleIntelligenceService.Infrastructure.Services;

namespace VehicleIntelligenceService.Tests;

public class VehicleIntelligenceServiceTests
{
    private readonly IPricingEngine _pricingEngine;

    public VehicleIntelligenceServiceTests()
    {
        _pricingEngine = new PricingEngine();
    }

    [Fact]
    public async Task PriceAnalysis_ShouldBeCreated_WithValidInput()
    {
        // Arrange
        var input = new VehiclePricingInput(
            VehicleId: Guid.NewGuid(),
            Make: "Toyota",
            Model: "Corolla",
            Year: 2021,
            Mileage: 30000,
            Condition: "Good",
            FuelType: "Gasoline",
            Transmission: "Automatic",
            CurrentPrice: 28000,
            PhotoCount: 10,
            ViewCount: 150,
            DaysListed: 15
        );

        // Act
        var analysis = await _pricingEngine.AnalyzePriceAsync(input);

        // Assert
        analysis.Should().NotBeNull();
        analysis.Id.Should().NotBeEmpty();
        analysis.VehicleId.Should().Be(input.VehicleId);
        analysis.CurrentPrice.Should().Be(28000);
        analysis.SuggestedPrice.Should().BeGreaterThan(0);
        analysis.MarketAvgPrice.Should().BeGreaterThan(0);
        analysis.ConfidenceScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task PriceAnalysis_ShouldCalculate_PricePosition()
    {
        // Arrange
        var input = new VehiclePricingInput(
            Guid.NewGuid(), "Honda", "Civic", 2022, 15000, 
            "Excellent", "Gasoline", "Automatic", 35000, 
            15, 200, 10
        );

        // Act
        var analysis = await _pricingEngine.AnalyzePriceAsync(input);

        // Assert
        analysis.PricePosition.Should().BeOneOf("Above Market", "Below Market", "Fair");
    }

    [Fact]
    public async Task PriceAnalysis_ShouldPredict_DaysToSale()
    {
        // Arrange
        var input = new VehiclePricingInput(
            Guid.NewGuid(), "Toyota", "RAV4", 2023, 5000, 
            "Excellent", "Hybrid", "Automatic", 38000, 
            20, 300, 5
        );

        // Act
        var analysis = await _pricingEngine.AnalyzePriceAsync(input);

        // Assert
        analysis.PredictedDaysToSaleAtCurrentPrice.Should().BeGreaterThan(0);
        analysis.PredictedDaysToSaleAtSuggestedPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PriceAnalysis_ShouldProvide_PriceRange()
    {
        // Arrange
        var input = new VehiclePricingInput(
            Guid.NewGuid(), "Nissan", "Altima", 2021, 40000, 
            "Good", "Gasoline", "Automatic", 24000, 
            8, 100, 20
        );

        // Act
        var analysis = await _pricingEngine.AnalyzePriceAsync(input);

        // Assert
        analysis.SuggestedPriceMin.Should().BeLessThan(analysis.SuggestedPrice);
        analysis.SuggestedPriceMax.Should().BeGreaterThan(analysis.SuggestedPrice);
        var range = analysis.SuggestedPriceMax - analysis.SuggestedPriceMin;
        range.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DemandPrediction_ShouldBeCreated_WithValidInput()
    {
        // Arrange
        var input = new DemandPredictionInput(
            Make: "Toyota",
            Model: "Camry",
            Year: 2022
        );

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.Should().NotBeNull();
        prediction.Id.Should().NotBeEmpty();
        prediction.Make.Should().Be("Toyota");
        prediction.Model.Should().Be("Camry");
        prediction.Year.Should().Be(2022);
        prediction.DemandScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task DemandPrediction_ShouldCalculate_DemandLevel()
    {
        // Arrange
        var input = new DemandPredictionInput("Honda", "CR-V", 2023);

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.CurrentDemand.Should().BeOneOf(
            DemandLevel.VeryHigh, 
            DemandLevel.High, 
            DemandLevel.Medium, 
            DemandLevel.Low, 
            DemandLevel.VeryLow
        );
    }

    [Fact]
    public async Task DemandPrediction_ShouldHave_Trend()
    {
        // Arrange
        var input = new DemandPredictionInput("Toyota", "Corolla", 2022);

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.Trend.Should().BeOneOf(
            TrendDirection.Rising, 
            TrendDirection.Stable, 
            TrendDirection.Falling
        );
        prediction.TrendStrength.Should().BeInRange(0, 1);
    }

    [Fact]
    public async Task DemandPrediction_ShouldPredict_FutureDemand()
    {
        // Arrange
        var input = new DemandPredictionInput("Nissan", "Sentra", 2021);

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.PredictedDemand30Days.Should().BeOneOf(
            DemandLevel.VeryHigh, DemandLevel.High, DemandLevel.Medium, 
            DemandLevel.Low, DemandLevel.VeryLow
        );
        prediction.PredictedDemand90Days.Should().BeOneOf(
            DemandLevel.VeryHigh, DemandLevel.High, DemandLevel.Medium, 
            DemandLevel.Low, DemandLevel.VeryLow
        );
    }

    [Fact]
    public async Task DemandPrediction_ShouldProvide_MarketStatistics()
    {
        // Arrange
        var input = new DemandPredictionInput("Toyota", "Highlander", 2023);

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.SearchesPerDay.Should().BeGreaterThanOrEqualTo(0);
        prediction.AvailableInventory.Should().BeGreaterThanOrEqualTo(0);
        prediction.AvgDaysToSale.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DemandPrediction_ShouldProvide_BuyRecommendation()
    {
        // Arrange
        var input = new DemandPredictionInput("Honda", "Accord", 2022);

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.BuyRecommendation.Should().BeOneOf(
            BuyRecommendation.StrongBuy,
            BuyRecommendation.Buy,
            BuyRecommendation.Hold,
            BuyRecommendation.Avoid
        );
        prediction.BuyRecommendationReason.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DemandPrediction_ShouldProvide_Insights()
    {
        // Arrange
        var input = new DemandPredictionInput("Toyota", "4Runner", 2023);

        // Act
        var prediction = await _pricingEngine.PredictDemandAsync(input);

        // Assert
        prediction.Insights.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Excellent", 1.10)]
    [InlineData("Good", 1.00)]
    [InlineData("Fair", 0.90)]
    [InlineData("Poor", 0.75)]
    public async Task PriceAnalysis_ShouldAdjust_ByCondition(string condition, decimal expectedMultiplier)
    {
        // Arrange
        var input1 = new VehiclePricingInput(
            Guid.NewGuid(), "Honda", "Civic", 2022, 20000, 
            "Good", "Gasoline", "Automatic", 30000, 
            10, 100, 10
        );
        
        var input2 = new VehiclePricingInput(
            Guid.NewGuid(), "Honda", "Civic", 2022, 20000, 
            condition, "Gasoline", "Automatic", 30000, 
            10, 100, 10
        );

        // Act
        var analysis1 = await _pricingEngine.AnalyzePriceAsync(input1);
        var analysis2 = await _pricingEngine.AnalyzePriceAsync(input2);

        // Assert
        if (expectedMultiplier > 1.0m)
        {
            analysis2.SuggestedPrice.Should().BeGreaterThan(analysis1.SuggestedPrice);
        }
        else if (expectedMultiplier < 1.0m)
        {
            analysis2.SuggestedPrice.Should().BeLessThan(analysis1.SuggestedPrice);
        }
    }

    [Fact]
    public async Task MarketComparables_ShouldBeRetrieved()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();

        // Act
        var comparables = await _pricingEngine.GetMarketComparablesAsync(vehicleId, limit: 5);

        // Assert
        comparables.Should().NotBeNull();
        comparables.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    public void PriceAnalysis_ShouldHave_RequiredProperties()
    {
        // Arrange & Act
        var analysis = new PriceAnalysis
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            CurrentPrice = 25000,
            SuggestedPrice = 26000,
            PricePosition = "Fair"
        };

        // Assert
        analysis.Id.Should().NotBeEmpty();
        analysis.VehicleId.Should().NotBeEmpty();
        analysis.CurrentPrice.Should().BeGreaterThan(0);
        analysis.SuggestedPrice.Should().BeGreaterThan(0);
        analysis.PricePosition.Should().NotBeEmpty();
    }

    [Fact]
    public void DemandPrediction_ShouldHave_RequiredProperties()
    {
        // Arrange & Act
        var prediction = new DemandPrediction
        {
            Id = Guid.NewGuid(),
            Make = "Toyota",
            Model = "Camry",
            Year = 2022,
            CurrentDemand = DemandLevel.High,
            DemandScore = 75
        };

        // Assert
        prediction.Id.Should().NotBeEmpty();
        prediction.Make.Should().NotBeEmpty();
        prediction.Model.Should().NotBeEmpty();
        prediction.Year.Should().BeGreaterThan(2000);
        prediction.DemandScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public void PriceRecommendation_ShouldHave_ValidTypes()
    {
        // Arrange & Act
        var recommendation = new PriceRecommendation
        {
            Id = Guid.NewGuid(),
            Type = RecommendationType.ReducePrice,
            Reason = "Price is above market average",
            Priority = 1
        };

        // Assert
        recommendation.Type.Should().BeOneOf(
            RecommendationType.ReducePrice,
            RecommendationType.MaintainPrice,
            RecommendationType.HighlightFeature,
            RecommendationType.AddPhotos,
            RecommendationType.ImproveDescription,
            RecommendationType.OfferFinancing
        );
    }
}
