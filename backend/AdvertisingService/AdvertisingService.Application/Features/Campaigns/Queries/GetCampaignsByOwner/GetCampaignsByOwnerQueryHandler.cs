using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Queries.GetCampaignsByOwner;

public class GetCampaignsByOwnerQueryHandler : IRequestHandler<GetCampaignsByOwnerQuery, PaginatedResult<CampaignSummaryDto>>
{
    private readonly IAdCampaignRepository _campaignRepo;

    public GetCampaignsByOwnerQueryHandler(IAdCampaignRepository campaignRepo) => _campaignRepo = campaignRepo;

    public async Task<PaginatedResult<CampaignSummaryDto>> Handle(GetCampaignsByOwnerQuery request, CancellationToken ct)
    {
        var campaigns = await _campaignRepo.GetByOwnerAsync(
            request.OwnerId, request.OwnerType, request.Status,
            request.Page, request.PageSize, ct);

        var totalCount = await _campaignRepo.CountByOwnerAsync(
            request.OwnerId, request.OwnerType, request.Status, ct);

        var items = campaigns.Select(c => new CampaignSummaryDto(
            c.Id, c.VehicleId, c.PlacementType, c.Status, c.TotalBudget,
            c.SpentBudget, c.ViewsConsumed, c.Clicks, c.StartDate, c.EndDate
        )).ToList();

        return new PaginatedResult<CampaignSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
