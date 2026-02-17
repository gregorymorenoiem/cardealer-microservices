using FeatureToggleService.Application.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.TrackExposure;

public class TrackExposureHandler : IRequestHandler<TrackExposureCommand, TrackExposureResponse>
{
    private readonly IABTestingService _abTestingService;

    public TrackExposureHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<TrackExposureResponse> Handle(TrackExposureCommand request, CancellationToken cancellationToken)
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

        // Get or create assignment
        var assignment = await _abTestingService.AssignVariantAsync(
            experiment.Id,
            request.UserId,
            request.SessionId,
            request.DeviceId,
            request.UserAttributes,
            cancellationToken
        );

        // Track exposure
        await _abTestingService.TrackExposureAsync(
            experiment.Id,
            request.UserId,
            cancellationToken
        );

        return new TrackExposureResponse
        {
            ExperimentId = experiment.Id,
            VariantId = assignment.VariantId,
            VariantKey = assignment.Variant?.Key ?? string.Empty,
            ExposedAt = DateTime.UtcNow,
            Success = true,
            Message = "Exposure tracked successfully"
        };
    }
}
