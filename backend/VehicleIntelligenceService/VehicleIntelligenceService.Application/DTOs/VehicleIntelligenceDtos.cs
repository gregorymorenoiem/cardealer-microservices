namespace VehicleIntelligenceService.Application.DTOs;

public record PriceSuggestionRequestDto(
    string Make,
    string Model,
    int Year,
    int Mileage,
    string? BodyType,
    string? Location,
    decimal? AskingPrice);

public record PriceSuggestionDto(
    decimal MarketPrice,
    decimal SuggestedPrice,
    decimal DeltaPercent,
    int DemandScore,
    int EstimatedDaysToSell,
    decimal Confidence,
    string ModelVersion,
    List<string> Tips);

public record CategoryDemandDto(
    string Category,
    string DemandLevel,
    int DemandScore,
    int AvgDaysToSale,
    int TotalSearches,
    int ActiveListings,
    DateTime UpdatedAt);

public record MarketAnalysisDto(
    string Make,
    string Model,
    int Year,
    int TotalListings,
    decimal AvgPrice,
    decimal MinPrice,
    decimal MaxPrice,
    int AvgDaysToSale,
    int MedianDaysToSale,
    string PriceTrend,
    string DemandTrend,
    int CompetitorCount,
    decimal MarketShare,
    List<string> Recommendations);

public record MLStatisticsDto(
    int TotalInferences,
    decimal SuccessRate,
    int ErrorsLast24h,
    DateTime LastUpdated);

public record ModelPerformanceDto(
    string ModelName,
    decimal Accuracy,
    decimal Mae,
    decimal Rmse,
    DateTime LastTrained,
    DateTime NextTraining,
    string Status);

public record InferenceMetricsDto(
    int TotalInferences,
    decimal SuccessRate,
    double AvgLatencyMs,
    double P95LatencyMs,
    double P99LatencyMs,
    int ErrorsLast24h,
    DateTime LastUpdated);
