using VehicleIntelligenceService.Domain.Entities;
using VehicleIntelligenceService.Domain.Interfaces;

namespace VehicleIntelligenceService.Infrastructure.Services;

public class VehiclePricingEngine : IVehiclePricingEngine
{
    public PriceSuggestionRecord ComputeSuggestion(
        string make,
        string model,
        int year,
        int mileage,
        string? bodyType,
        string? location,
        decimal? askingPrice,
        int? categoryDemandOverride = null)
    {
        // Baseline deterministic model (placeholder for XGBoost model artifact)
        // The goal is to provide consistent suggestions for the UI and API.

        var age = Math.Max(0, DateTime.UtcNow.Year - year);
        var mileageFactor = Math.Clamp(1m - (mileage / 200_000m), 0.35m, 1.05m);

        // Base price by age (very rough baseline)
        var basePrice = 28_000m * (decimal)Math.Pow(0.90, age);
        basePrice *= mileageFactor;

        // Body type adjustment (market preference)
        var typeBoost = (bodyType ?? string.Empty).ToLowerInvariant() switch
        {
            "suv" => 1.08m,
            "camioneta" => 1.10m,
            "pickup" => 1.10m,
            "sedan" => 1.00m,
            "coupe" => 1.02m,
            "deportivo" => 1.06m,
            "electrico" => 1.05m,
            _ => 1.00m
        };

        // Location adjustment (placeholder)
        var locationBoost = (location ?? string.Empty).ToLowerInvariant().Contains("santo") ? 1.02m : 1.00m;

        var marketPrice = Math.Max(2_000m, decimal.Round(basePrice * typeBoost * locationBoost, 0));

        var demandScore = categoryDemandOverride ?? ComputeDemandScore(bodyType, age);
        var suggestedPrice = decimal.Round(marketPrice * (0.98m + (demandScore / 100m) * 0.06m), 0);

        var deltaPercent = 0m;
        if (askingPrice.HasValue && askingPrice.Value > 0)
        {
            deltaPercent = decimal.Round(((askingPrice.Value - marketPrice) / marketPrice) * 100m, 1);
        }

        var daysToSell = ComputeEstimatedDaysToSell(demandScore, deltaPercent, age);
        var confidence = ComputeConfidence(age, mileage, bodyType);

        return new PriceSuggestionRecord
        {
            Make = make.Trim(),
            Model = model.Trim(),
            Year = year,
            Mileage = mileage,
            BodyType = bodyType,
            Location = location,
            MarketPrice = marketPrice,
            SuggestedPrice = suggestedPrice,
            AskingPrice = askingPrice,
            DeltaPercent = deltaPercent,
            DemandScore = demandScore,
            EstimatedDaysToSell = daysToSell,
            Confidence = confidence,
            ModelVersion = "baseline-v1"
        };
    }

    public List<string> BuildSellingTips(PriceSuggestionRecord suggestion)
    {
        var tips = new List<string>();

        if (suggestion.DeltaPercent >= 10)
        {
            tips.Add("Baja el precio cerca del mercado para recibir más contactos.");
        }
        else if (suggestion.DeltaPercent <= -10)
        {
            tips.Add("Tu precio está por debajo del mercado; podrías subirlo un poco si el vehículo está en excelente estado.");
        }

        if (suggestion.Mileage > 150_000)
        {
            tips.Add("Incluye historial de mantenimiento y fotos del motor/interior para generar confianza.");
        }

        if (string.Equals(suggestion.BodyType, "SUV", StringComparison.OrdinalIgnoreCase))
        {
            tips.Add("Resalta espacio, seguridad y consumo; son claves para SUVs.");
        }

        tips.Add("Agrega 10+ fotos claras (exterior, interior, tablero, llantas) para vender más rápido.");
        tips.Add("Describe cualquier reparación reciente y qué incluye (matrícula, llaves extra, garantía si aplica).");

        return tips;
    }

    private static int ComputeDemandScore(string? bodyType, int age)
    {
        var baseDemand = (bodyType ?? string.Empty).ToLowerInvariant() switch
        {
            "suv" => 78,
            "camioneta" => 82,
            "pickup" => 82,
            "sedan" => 60,
            "deportivo" => 65,
            "electrico" => 70,
            _ => 62
        };

        // Older vehicles tend to have lower demand.
        var agePenalty = Math.Clamp(age * 2, 0, 25);
        return Math.Clamp(baseDemand - agePenalty, 30, 95);
    }

    private static int ComputeEstimatedDaysToSell(int demandScore, decimal deltaPercent, int age)
    {
        var baseDays = 30 - (demandScore / 5); // demand 80 => 14 days

        // Penalize overpriced listings
        if (deltaPercent > 0)
        {
            baseDays += (int)Math.Ceiling((double)deltaPercent / 2.5);
        }
        else if (deltaPercent < 0)
        {
            baseDays -= (int)Math.Ceiling((double)(-deltaPercent) / 4.0);
        }

        baseDays += Math.Clamp(age, 0, 10) / 2;

        return Math.Clamp(baseDays, 3, 90);
    }

    private static decimal ComputeConfidence(int age, int mileage, string? bodyType)
    {
        var confidence = 0.82m;

        if (age > 12) confidence -= 0.10m;
        if (mileage > 180_000) confidence -= 0.08m;
        if (string.IsNullOrWhiteSpace(bodyType)) confidence -= 0.05m;

        return Math.Clamp(confidence, 0.40m, 0.90m);
    }
}
