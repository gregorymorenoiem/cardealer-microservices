using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Application.DTOs;

public record CampaignDto(
    Guid Id,
    Guid OwnerId,
    string OwnerType,
    Guid VehicleId,
    AdPlacementType PlacementType,
    CampaignPricingModel PricingModel,
    CampaignStatus Status,
    decimal? PricePerView,
    decimal? FixedPrice,
    decimal TotalBudget,
    decimal SpentBudget,
    int ViewsConsumed,
    int Clicks,
    decimal QualityScore,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public double Ctr => ViewsConsumed > 0 ? (double)Clicks / ViewsConsumed : 0.0;
    public decimal RemainingBudget => TotalBudget - SpentBudget;
}
