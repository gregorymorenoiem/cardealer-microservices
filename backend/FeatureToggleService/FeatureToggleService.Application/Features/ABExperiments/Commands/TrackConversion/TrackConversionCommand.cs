using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.TrackConversion;

public class TrackConversionCommand : IRequest<TrackConversionResponse>
{
    public string ExperimentKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string MetricKey { get; set; } = "conversion";
    public double Value { get; set; } = 1.0;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TrackConversionResponse
{
    public Guid ExperimentId { get; set; }
    public Guid VariantId { get; set; }
    public string MetricKey { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime RecordedAt { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
