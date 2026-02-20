using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Application.DTOs;

public record PricingEstimateDto(
    AdPlacementType PlacementType,
    CampaignPricingModel PricingModel,
    decimal PricePerView,
    decimal FixedPriceDaily,
    decimal FixedPriceWeekly,
    decimal FixedPriceMonthly,
    int EstimatedDailyViews,
    string Currency
);
