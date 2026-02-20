using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Commands.PauseCampaign;

public record PauseCampaignCommand(Guid CampaignId, Guid RequesterId) : IRequest<bool>;
