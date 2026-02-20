using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Queries.GetCampaignById;

public class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, CampaignDto?>
{
    private readonly IAdCampaignRepository _campaignRepo;

    public GetCampaignByIdQueryHandler(IAdCampaignRepository campaignRepo) => _campaignRepo = campaignRepo;

    public async Task<CampaignDto?> Handle(GetCampaignByIdQuery request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) return null;

        return MapToDto(campaign);
    }

    private static CampaignDto MapToDto(AdCampaign c) => new(
        c.Id, c.OwnerId, c.OwnerType, c.VehicleId, c.PlacementType, c.PricingModel,
        c.Status, c.PricePerView, c.FixedPrice, c.TotalBudget, c.SpentBudget,
        c.ViewsConsumed, c.Clicks, c.QualityScore, c.StartDate, c.EndDate,
        c.CreatedAt, c.UpdatedAt);
}
