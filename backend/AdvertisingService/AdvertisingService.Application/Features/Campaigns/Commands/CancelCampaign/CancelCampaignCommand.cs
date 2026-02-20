using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Commands.CancelCampaign;

public record CancelCampaignCommand(Guid CampaignId, Guid RequesterId) : IRequest<bool>;
