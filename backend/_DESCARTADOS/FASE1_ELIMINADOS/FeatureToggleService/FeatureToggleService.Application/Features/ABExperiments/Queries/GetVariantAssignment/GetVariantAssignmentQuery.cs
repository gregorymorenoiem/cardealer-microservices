using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Queries.GetVariantAssignment;

public class GetVariantAssignmentQuery : IRequest<VariantAssignmentResponse>
{
    public string ExperimentKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? DeviceId { get; set; }
    public Dictionary<string, object>? UserAttributes { get; set; }
}

public class VariantAssignmentResponse
{
    public Guid ExperimentId { get; set; }
    public string ExperimentKey { get; set; } = string.Empty;
    public string ExperimentName { get; set; } = string.Empty;
    public Guid VariantId { get; set; }
    public string VariantKey { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public bool IsControl { get; set; }
    public string? Payload { get; set; }
    public bool? FeatureFlagValue { get; set; }
    public string? Parameters { get; set; }
    public string? StyleOverrides { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsExposed { get; set; }
    public bool HasConverted { get; set; }
}
