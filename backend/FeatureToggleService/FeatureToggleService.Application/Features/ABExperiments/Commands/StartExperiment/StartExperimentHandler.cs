using FeatureToggleService.Application.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.StartExperiment;

public class StartExperimentHandler : IRequestHandler<StartExperimentCommand, StartExperimentResponse>
{
    private readonly IABTestingService _abTestingService;

    public StartExperimentHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<StartExperimentResponse> Handle(StartExperimentCommand request, CancellationToken cancellationToken)
    {
        var experiment = await _abTestingService.GetExperimentAsync(request.ExperimentId, cancellationToken);

        if (experiment == null)
        {
            throw new KeyNotFoundException($"Experiment with ID {request.ExperimentId} not found");
        }

        if (!experiment.Variants.Any())
        {
            throw new InvalidOperationException("Cannot start experiment without variants");
        }

        if (experiment.Variants.Count < 2)
        {
            throw new InvalidOperationException("Experiment must have at least 2 variants");
        }

        var totalWeight = experiment.Variants.Sum(v => v.Weight);
        if (totalWeight != 100)
        {
            throw new InvalidOperationException($"Variant weights must sum to 100 (current: {totalWeight})");
        }

        await _abTestingService.StartExperimentAsync(request.ExperimentId, "system", cancellationToken);

        return new StartExperimentResponse
        {
            Id = experiment.Id,
            Key = experiment.Key,
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            Message = $"Experiment '{experiment.Name}' started successfully"
        };
    }
}
