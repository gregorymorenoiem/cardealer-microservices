namespace VehicleIntelligenceService.Domain.Entities;

public class PriceSuggestionRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string? BodyType { get; set; }
    public string? Location { get; set; }

    public decimal MarketPrice { get; set; }
    public decimal SuggestedPrice { get; set; }
    public decimal? AskingPrice { get; set; }
    public decimal DeltaPercent { get; set; }
    public int DemandScore { get; set; }
    public int EstimatedDaysToSell { get; set; }
    public decimal Confidence { get; set; }
    public string ModelVersion { get; set; } = "baseline-v1";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
