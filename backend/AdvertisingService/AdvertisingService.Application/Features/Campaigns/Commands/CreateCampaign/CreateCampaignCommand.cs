using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Commands.CreateCampaign;

public record CreateCampaignCommand(
    Guid OwnerId,
    string OwnerType,
    Guid VehicleId,
    AdPlacementType PlacementType,
    CampaignPricingModel PricingModel,
    decimal TotalBudget,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<CampaignDto>;
