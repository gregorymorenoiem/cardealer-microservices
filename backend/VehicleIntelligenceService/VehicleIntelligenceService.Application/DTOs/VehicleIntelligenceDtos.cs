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
    int DemandScore,
    string Trend,
    DateTime UpdatedAt);
