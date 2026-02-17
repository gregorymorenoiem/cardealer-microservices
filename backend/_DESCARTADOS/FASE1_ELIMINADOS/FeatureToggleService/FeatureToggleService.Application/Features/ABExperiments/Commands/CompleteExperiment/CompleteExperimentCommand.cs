using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.CompleteExperiment;

public class CompleteExperimentCommand : IRequest<CompleteExperimentResponse>
{
    public Guid ExperimentId { get; set; }
    public Guid? WinningVariantId { get; set; }
    public string? CompletionNotes { get; set; }
}

public class CompleteExperimentResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public Guid? WinningVariantId { get; set; }
    public string? WinningVariantKey { get; set; }
    public double? WinnerConfidence { get; set; }
    public string Message { get; set; } = string.Empty;
}
