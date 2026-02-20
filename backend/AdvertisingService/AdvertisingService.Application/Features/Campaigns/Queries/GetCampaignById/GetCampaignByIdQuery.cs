using AdvertisingService.Application.DTOs;
using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Queries.GetCampaignById;

public record GetCampaignByIdQuery(Guid CampaignId) : IRequest<CampaignDto?>;
