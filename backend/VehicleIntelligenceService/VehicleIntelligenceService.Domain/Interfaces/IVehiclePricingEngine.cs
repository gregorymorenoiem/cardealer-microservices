using VehicleIntelligenceService.Domain.Entities;

namespace VehicleIntelligenceService.Domain.Interfaces;

public interface IVehiclePricingEngine
{
    PriceSuggestionRecord ComputeSuggestion(
        string make,
        string model,
        int year,
        int mileage,
        string? bodyType,
        string? location,
        decimal? askingPrice,
        int? categoryDemandOverride = null);

    List<string> BuildSellingTips(PriceSuggestionRecord suggestion);
}
