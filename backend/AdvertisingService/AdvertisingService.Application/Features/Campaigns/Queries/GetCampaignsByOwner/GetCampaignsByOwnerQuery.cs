using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Queries.GetCampaignsByOwner;

public record GetCampaignsByOwnerQuery(
    Guid OwnerId,
    string OwnerType,
    CampaignStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedResult<CampaignSummaryDto>>;
