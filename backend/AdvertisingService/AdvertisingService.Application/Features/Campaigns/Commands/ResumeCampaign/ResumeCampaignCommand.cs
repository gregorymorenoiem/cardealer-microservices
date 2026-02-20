using MediatR;

namespace AdvertisingService.Application.Features.Campaigns.Commands.ResumeCampaign;

public record ResumeCampaignCommand(Guid CampaignId, Guid RequesterId) : IRequest<bool>;
