using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.TrackExposure;

public class TrackExposureCommand : IRequest<TrackExposureResponse>
{
    public string ExperimentKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? DeviceId { get; set; }
    public Dictionary<string, object>? UserAttributes { get; set; }
}

public class TrackExposureResponse
{
    public Guid ExperimentId { get; set; }
    public Guid VariantId { get; set; }
    public string VariantKey { get; set; } = string.Empty;
    public DateTime ExposedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
