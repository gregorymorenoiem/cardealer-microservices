using MediatR;

namespace AdvertisingService.Application.Features.Tracking.Commands.RecordClick;

public record RecordClickCommand(
    Guid CampaignId,
    Guid? ImpressionId,
    Guid? UserId
) : IRequest<bool>;
