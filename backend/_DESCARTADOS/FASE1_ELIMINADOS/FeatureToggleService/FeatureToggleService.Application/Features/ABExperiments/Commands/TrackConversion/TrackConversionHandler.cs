using FeatureToggleService.Application.Interfaces;
using MediatR;
using System.Text.Json;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.TrackConversion;

public class TrackConversionHandler : IRequestHandler<TrackConversionCommand, TrackConversionResponse>
{
    private readonly IABTestingService _abTestingService;

    public TrackConversionHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<TrackConversionResponse> Handle(TrackConversionCommand request, CancellationToken cancellationToken)
    {
        // Get experiment
        var experiment = await _abTestingService.GetExperimentByKeyAsync(request.ExperimentKey, cancellationToken);

        if (experiment == null)
        {
            throw new KeyNotFoundException($"Experiment with key '{request.ExperimentKey}' not found");
        }

        if (!experiment.IsRunning())
        {
            throw new InvalidOperationException($"Experiment is not running (status: {experiment.Status})");
        }

        // Get user assignment
        var assignment = await _abTestingService.GetAssignmentAsync(
            experiment.Id,
            request.UserId,
            cancellationToken
        );

        if (assignment == null)
        {
            throw new InvalidOperationException($"User {request.UserId} is not assigned to experiment {request.ExperimentKey}");
        }

        // Track conversion
        await _abTestingService.TrackConversionAsync(
            experiment.Id,
            request.UserId,
            request.MetricKey,
            cancellationToken
        );

        // Track metric with value
        string? metadataJson = request.Metadata != null
            ? JsonSerializer.Serialize(request.Metadata)
            : null;

        await _abTestingService.TrackMetricAsync(
            experiment.Id,
            assignment.VariantId,
            request.UserId,
            request.MetricKey,
            request.Value,
            "counter",
            metadataJson,
            cancellationToken
        );

        return new TrackConversionResponse
        {
            ExperimentId = experiment.Id,
            VariantId = assignment.VariantId,
            MetricKey = request.MetricKey,
            Value = request.Value,
            RecordedAt = DateTime.UtcNow,
            Success = true,
            Message = "Conversion tracked successfully"
        };
    }
}
