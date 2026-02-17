namespace VehicleIntelligenceService.Domain.Entities;

public class PriceAnalysis
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public decimal CurrentPrice { get; set; }
    
    // Precio sugerido
    public decimal SuggestedPrice { get; set; }
    public decimal SuggestedPriceMin { get; set; }
    public decimal SuggestedPriceMax { get; set; }
    
    // Comparación con mercado
    public decimal MarketAvgPrice { get; set; }
    public decimal PriceVsMarket { get; set; }        // 1.05 = 5% arriba
    public string PricePosition { get; set; } = "Fair"; // "Above Market", "Below Market", "Fair"
    
    // Predicción
    public int PredictedDaysToSaleAtCurrentPrice { get; set; }
    public int PredictedDaysToSaleAtSuggestedPrice { get; set; }
    
    // Metadata
    public DateTime AnalysisDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Confianza del modelo
    public decimal ConfidenceScore { get; set; }      // 0-100
    
    // Factores que influyen en el precio
    public string InfluencingFactors { get; set; } = "{}"; // JSON
}
