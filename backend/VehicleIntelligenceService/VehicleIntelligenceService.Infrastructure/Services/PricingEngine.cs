using VehicleIntelligenceService.Domain.Entities;
using VehicleIntelligenceService.Domain.Interfaces;
using System.Text.Json;

namespace VehicleIntelligenceService.Infrastructure.Services;

/// <summary>
/// Motor de pricing con algoritmos de Machine Learning simplificados
/// En producción, esto se reemplazaría con modelos ML reales (XGBoost, TensorFlow, etc.)
/// </summary>
public class PricingEngine : IPricingEngine
{
    public async Task<PriceAnalysis> AnalyzePriceAsync(VehiclePricingInput input, CancellationToken cancellationToken = default)
    {
        // Simular delay de ML processing
        await Task.Delay(100, cancellationToken);

        // Calcular precio base por año
        var basePrice = CalculateBasePrice(input.Make, input.Model, input.Year);
        
        // Ajustar por mileage
        var mileageAdjustment = CalculateMileageAdjustment(input.Mileage, input.Year);
        
        // Ajustar por condición
        var conditionMultiplier = input.Condition switch
        {
            "Excellent" => 1.10m,
            "Good" => 1.00m,
            "Fair" => 0.90m,
            "Poor" => 0.75m,
            _ => 1.00m
        };
        
        // Ajustar por listing quality (photos, views)
        var qualityBonus = CalculateQualityBonus(input.PhotoCount, input.ViewCount);
        
        // Calcular precio sugerido
        var suggestedPrice = (basePrice + mileageAdjustment) * conditionMultiplier + qualityBonus;
        var priceRange = suggestedPrice * 0.05m; // ±5% de rango
        
        // Comparar con precio actual
        var priceVsMarket = input.CurrentPrice / suggestedPrice;
        var pricePosition = priceVsMarket switch
        {
            > 1.10m => "Above Market",
            < 0.90m => "Below Market",
            _ => "Fair"
        };
        
        // Predecir días de venta basado en precio
        var predictedDaysAtCurrent = CalculatePredictedDays(priceVsMarket, input.DaysListed);
        var predictedDaysAtSuggested = CalculatePredictedDays(1.0m, input.DaysListed);
        
        // Calcular score de confianza
        var confidenceScore = CalculateConfidence(input.ViewCount, input.DaysListed);
        
        return new PriceAnalysis
        {
            Id = Guid.NewGuid(),
            VehicleId = input.VehicleId,
            CurrentPrice = input.CurrentPrice,
            SuggestedPrice = Math.Round(suggestedPrice, 2),
            SuggestedPriceMin = Math.Round(suggestedPrice - priceRange, 2),
            SuggestedPriceMax = Math.Round(suggestedPrice + priceRange, 2),
            MarketAvgPrice = Math.Round(basePrice, 2),
            PriceVsMarket = Math.Round(priceVsMarket, 2),
            PricePosition = pricePosition,
            PredictedDaysToSaleAtCurrentPrice = predictedDaysAtCurrent,
            PredictedDaysToSaleAtSuggestedPrice = predictedDaysAtSuggested,
            ConfidenceScore = confidenceScore,
            AnalysisDate = DateTime.UtcNow,
            InfluencingFactors = JsonSerializer.Serialize(new
            {
                basePrice,
                mileageAdjustment,
                conditionMultiplier,
                qualityBonus
            })
        };
    }

    public async Task<DemandPrediction> PredictDemandAsync(DemandPredictionInput input, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        
        // Calcular demanda basada en marca y año
        var demandScore = CalculateDemandScore(input.Make, input.Model, input.Year);
        var currentDemand = demandScore switch
        {
            >= 80 => DemandLevel.VeryHigh,
            >= 60 => DemandLevel.High,
            >= 40 => DemandLevel.Medium,
            >= 20 => DemandLevel.Low,
            _ => DemandLevel.VeryLow
        };
        
        // Calcular tendencia (simulada)
        var trend = demandScore >= 60 ? TrendDirection.Rising : 
                    demandScore <= 30 ? TrendDirection.Falling : 
                    TrendDirection.Stable;
        
        var trendStrength = trend == TrendDirection.Stable ? 0.1m : 0.7m;
        
        // Predicciones futuras
        var predicted30 = PredictFutureDemand(currentDemand, trend, 30);
        var predicted90 = PredictFutureDemand(currentDemand, trend, 90);
        
        // Estadísticas simuladas
        var searchesPerDay = (int)(demandScore * 0.5m);
        var availableInventory = 100 - (int)(demandScore * 0.5m);
        var avgDaysToSale = demandScore >= 60 ? 15 : demandScore >= 40 ? 35 : 60;
        
        // Recomendación de compra
        var buyRec = demandScore >= 70 ? BuyRecommendation.StrongBuy :
                     demandScore >= 50 ? BuyRecommendation.Buy :
                     demandScore >= 30 ? BuyRecommendation.Hold :
                     BuyRecommendation.Avoid;
        
        var insights = GenerateInsights(demandScore, currentDemand, trend, buyRec);
        
        return new DemandPrediction
        {
            Id = Guid.NewGuid(),
            Make = input.Make,
            Model = input.Model,
            Year = input.Year,
            CurrentDemand = currentDemand,
            DemandScore = demandScore,
            Trend = trend,
            TrendStrength = trendStrength,
            PredictedDemand30Days = predicted30,
            PredictedDemand90Days = predicted90,
            SearchesPerDay = searchesPerDay,
            AvailableInventory = availableInventory,
            AvgDaysToSale = avgDaysToSale,
            BuyRecommendation = buyRec,
            BuyRecommendationReason = GetBuyRecommendationReason(buyRec, demandScore, avgDaysToSale),
            Insights = JsonSerializer.Serialize(insights),
            PredictionDate = DateTime.UtcNow
        };
    }

    public Task<List<MarketComparable>> GetMarketComparablesAsync(Guid vehicleId, int limit = 10, CancellationToken cancellationToken = default)
    {
        // En producción, esto consultaría APIs externas o DB de comparables
        var comparables = new List<MarketComparable>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Source = "OKLA",
                Make = "Toyota",
                Model = "Corolla",
                Year = 2021,
                Mileage = 45000,
                Price = 28500,
                SimilarityScore = 95,
                Status = "Active",
                DaysOnMarket = 12
            }
        };
        
        return Task.FromResult(comparables.Take(limit).ToList());
    }

    // Métodos privados de cálculo
    private decimal CalculateBasePrice(string make, string model, int year)
    {
        // Precios base simplificados por marca
        var basePrices = new Dictionary<string, decimal>
        {
            ["Toyota"] = 30000,
            ["Honda"] = 28000,
            ["Nissan"] = 25000,
            ["Hyundai"] = 23000,
            ["Kia"] = 22000,
            ["Ford"] = 26000,
            ["Chevrolet"] = 25000
        };
        
        var basePrice = basePrices.GetValueOrDefault(make, 25000);
        
        // Ajustar por año (depreciación ~10% anual)
        var currentYear = DateTime.Now.Year;
        var age = currentYear - year;
        var depreciationFactor = Math.Pow(0.90, age);
        
        return basePrice * (decimal)depreciationFactor;
    }
    
    private decimal CalculateMileageAdjustment(int mileage, int year)
    {
        var currentYear = DateTime.Now.Year;
        var age = Math.Max(1, currentYear - year);
        var expectedMileage = age * 12000; // 12k millas por año promedio
        
        var mileageDiff = mileage - expectedMileage;
        
        // $0.10 por cada milla de diferencia
        return mileageDiff * -0.10m;
    }
    
    private decimal CalculateQualityBonus(int photoCount, int viewCount)
    {
        var photoBonus = Math.Min(photoCount * 50, 500); // Max $500 por fotos
        var viewBonus = Math.Min(viewCount * 2, 300);    // Max $300 por views
        return photoBonus + viewBonus;
    }
    
    private int CalculatePredictedDays(decimal priceVsMarket, int daysListed)
    {
        var baseDays = 30;
        
        if (priceVsMarket > 1.15m) return baseDays + 30; // 60 días
        if (priceVsMarket > 1.05m) return baseDays + 15; // 45 días
        if (priceVsMarket < 0.90m) return baseDays - 15; // 15 días
        
        return baseDays; // 30 días
    }
    
    private decimal CalculateConfidence(int viewCount, int daysListed)
    {
        // Más views y más tiempo listado = más confianza
        var viewScore = Math.Min(viewCount / 10.0m, 50);
        var timeScore = Math.Min(daysListed / 3.0m, 50);
        
        return Math.Round(viewScore + timeScore, 2);
    }
    
    private decimal CalculateDemandScore(string make, string model, int year)
    {
        // Marcas populares
        var popularBrands = new[] { "Toyota", "Honda", "Nissan" };
        var brandScore = popularBrands.Contains(make) ? 30 : 20;
        
        // Vehículos más nuevos = mayor demanda
        var currentYear = DateTime.Now.Year;
        var age = currentYear - year;
        var yearScore = age <= 3 ? 40 : age <= 5 ? 30 : age <= 10 ? 20 : 10;
        
        // Factor aleatorio de mercado
        var marketFactor = new Random().Next(20, 31);
        
        return brandScore + yearScore + marketFactor;
    }
    
    private DemandLevel PredictFutureDemand(DemandLevel current, TrendDirection trend, int days)
    {
        if (trend == TrendDirection.Stable) return current;
        
        var change = trend == TrendDirection.Rising ? 1 : -1;
        var newLevel = (int)current + change;
        
        return newLevel switch
        {
            <= 0 => DemandLevel.VeryHigh,
            >= 4 => DemandLevel.VeryLow,
            _ => (DemandLevel)newLevel
        };
    }
    
    private List<string> GenerateInsights(decimal demandScore, DemandLevel level, TrendDirection trend, BuyRecommendation buyRec)
    {
        var insights = new List<string>();
        
        if (demandScore >= 70)
            insights.Add($"Alta demanda: {demandScore}/100 score");
        
        if (trend == TrendDirection.Rising)
            insights.Add("Tendencia al alza, buen momento para inventario");
        
        if (level == DemandLevel.VeryHigh)
            insights.Add("Se vende rápido, menos de 15 días promedio");
        
        if (buyRec == BuyRecommendation.StrongBuy)
            insights.Add("Excelente oportunidad de compra para dealers");
        
        return insights;
    }
    
    private string GetBuyRecommendationReason(BuyRecommendation rec, decimal score, decimal avgDays)
    {
        return rec switch
        {
            BuyRecommendation.StrongBuy => $"Alta demanda ({score}/100), se vende en {avgDays} días promedio",
            BuyRecommendation.Buy => $"Buena demanda ({score}/100), rotación aceptable",
            BuyRecommendation.Hold => $"Demanda media ({score}/100), esperar mejores condiciones",
            BuyRecommendation.Avoid => $"Baja demanda ({score}/100), difícil de vender",
            _ => "Sin razón disponible"
        };
    }
}
