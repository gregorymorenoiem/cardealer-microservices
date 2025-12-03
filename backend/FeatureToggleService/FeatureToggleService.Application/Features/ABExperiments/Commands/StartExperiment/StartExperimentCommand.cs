using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.StartExperiment;

public class StartExperimentCommand : IRequest<StartExperimentResponse>
{
    public Guid ExperimentId { get; set; }
}

public class StartExperimentResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
