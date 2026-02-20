using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Application.DTOs;

public record CampaignSummaryDto(
    Guid Id,
    Guid VehicleId,
    AdPlacementType PlacementType,
    CampaignStatus Status,
    decimal TotalBudget,
    decimal SpentBudget,
    int ViewsConsumed,
    int Clicks,
    DateTime StartDate,
    DateTime? EndDate
);
