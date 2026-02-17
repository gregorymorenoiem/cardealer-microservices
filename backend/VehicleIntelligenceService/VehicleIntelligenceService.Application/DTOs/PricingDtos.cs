using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Application.DTOs;

public record PriceAnalysisDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal SuggestedPrice { get; init; }
    public decimal SuggestedPriceMin { get; init; }
    public decimal SuggestedPriceMax { get; init; }
    public decimal MarketAvgPrice { get; init; }
    public decimal PriceVsMarket { get; init; }
    public string PricePosition { get; init; } = string.Empty;
    public int PredictedDaysToSaleAtCurrentPrice { get; init; }
    public int PredictedDaysToSaleAtSuggestedPrice { get; init; }
    public decimal ConfidenceScore { get; init; }
    public DateTime AnalysisDate { get; init; }
    public List<PriceRecommendationDto> Recommendations { get; init; } = new();
    public List<MarketComparableDto> Comparables { get; init; } = new();
}

public record PriceRecommendationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public decimal? SuggestedValue { get; init; }
    public string ImpactDescription { get; init; } = string.Empty;
    public int Priority { get; init; }
}

public record MarketComparableDto
{
    public Guid Id { get; init; }
    public string Source { get; init; } = string.Empty;
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Mileage { get; init; }
    public decimal Price { get; init; }
    public decimal SimilarityScore { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? DaysOnMarket { get; init; }
    public string? ExternalUrl { get; init; }
}

public record DemandPredictionDto
{
    public Guid Id { get; init; }
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string CurrentDemand { get; init; } = string.Empty;
    public decimal DemandScore { get; init; }
    public string Trend { get; init; } = string.Empty;
    public decimal TrendStrength { get; init; }
    public string PredictedDemand30Days { get; init; } = string.Empty;
    public string PredictedDemand90Days { get; init; } = string.Empty;
    public int SearchesPerDay { get; init; }
    public int AvailableInventory { get; init; }
    public decimal AvgDaysToSale { get; init; }
    public string BuyRecommendation { get; init; } = string.Empty;
    public string BuyRecommendationReason { get; init; } = string.Empty;
    public List<string> Insights { get; init; } = new();
    public DateTime PredictionDate { get; init; }
}

public record CreatePriceAnalysisRequest
{
    public Guid VehicleId { get; init; }
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Mileage { get; init; }
    public string Condition { get; init; } = "Good";
    public string FuelType { get; init; } = "Gasoline";
    public string Transmission { get; init; } = "Automatic";
    public decimal CurrentPrice { get; init; }
    public int PhotoCount { get; init; }
    public int ViewCount { get; init; }
    public int DaysListed { get; init; }
}

public record CreateDemandPredictionRequest
{
    public string Make { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public string? FuelType { get; init; }
    public string? Transmission { get; init; }
}
