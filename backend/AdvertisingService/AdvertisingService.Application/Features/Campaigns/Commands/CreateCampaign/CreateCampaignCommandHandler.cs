using AdvertisingService.Application.Clients;
using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AdvertisingService.Application.Features.Campaigns.Commands.CreateCampaign;

public class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
    private readonly IAdCampaignRepository _campaignRepo;
    private readonly VehicleServiceClient _vehicleClient;
    private readonly AuditServiceClient _auditClient;
    private readonly ILogger<CreateCampaignCommandHandler> _logger;

    public CreateCampaignCommandHandler(
        IAdCampaignRepository campaignRepo,
        VehicleServiceClient vehicleClient,
        AuditServiceClient auditClient,
        ILogger<CreateCampaignCommandHandler> logger)
    {
        _campaignRepo = campaignRepo;
        _vehicleClient = vehicleClient;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<CampaignDto> Handle(CreateCampaignCommand request, CancellationToken ct)
    {
        // Determine pricing
        decimal pricePerView = 0;
        decimal fixedPrice = 0;

        switch (request.PricingModel)
        {
            case CampaignPricingModel.PerView:
                pricePerView = request.PlacementType == AdPlacementType.FeaturedSpot ? 0.50m : 1.00m;
                break;
            case CampaignPricingModel.FixedDaily:
                fixedPrice = request.PlacementType == AdPlacementType.FeaturedSpot ? 150m : 300m;
                break;
            case CampaignPricingModel.FixedWeekly:
                fixedPrice = request.PlacementType == AdPlacementType.FeaturedSpot ? 900m : 1800m;
                break;
            case CampaignPricingModel.FixedMonthly:
                fixedPrice = request.PlacementType == AdPlacementType.FeaturedSpot ? 3200m : 6500m;
                break;
        }

        var totalViewsPurchased = request.PricingModel == CampaignPricingModel.PerView && pricePerView > 0
            ? (int?)(int)(request.TotalBudget / pricePerView)
            : null;

        var campaign = AdCampaign.Create(
            vehicleId: request.VehicleId,
            ownerId: request.OwnerId,
            ownerType: request.OwnerType,
            placementType: request.PlacementType,
            pricingModel: request.PricingModel,
            totalBudget: request.TotalBudget,
            pricePerView: pricePerView,
            fixedPrice: fixedPrice,
            totalViewsPurchased: totalViewsPurchased,
            startDate: request.StartDate,
            endDate: request.EndDate,
            qualityScore: 0.5m);

        await _campaignRepo.AddAsync(campaign, ct);

        _logger.LogInformation(
            "Campaign {CampaignId} created for {OwnerType} {OwnerId}, vehicle {VehicleId}",
            campaign.Id, request.OwnerType, request.OwnerId, request.VehicleId);

        await _auditClient.LogActionAsync(new AuditLogRequest
        {
            UserId = request.OwnerId,
            Action = "CREATE_AD_CAMPAIGN",
            EntityType = "AdCampaign",
            EntityId = campaign.Id.ToString(),
            Details = $"Created {request.PlacementType} campaign with budget {request.TotalBudget}"
        }, ct);

        return MapToDto(campaign);
    }

    private static CampaignDto MapToDto(AdCampaign c) => new(
        c.Id, c.OwnerId, c.OwnerType, c.VehicleId, c.PlacementType, c.PricingModel,
        c.Status, c.PricePerView, c.FixedPrice, c.TotalBudget, c.SpentBudget,
        c.ViewsConsumed, c.Clicks, c.QualityScore, c.StartDate, c.EndDate,
        c.CreatedAt, c.UpdatedAt);
}
