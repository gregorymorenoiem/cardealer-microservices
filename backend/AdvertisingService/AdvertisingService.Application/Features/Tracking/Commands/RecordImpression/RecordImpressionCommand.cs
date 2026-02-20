using MediatR;

namespace AdvertisingService.Application.Features.Tracking.Commands.RecordImpression;

public record RecordImpressionCommand(
    Guid CampaignId,
    string? SessionId,
    Guid? UserId,
    string? IpHash,
    string? Section,
    int Position
) : IRequest<bool>;
