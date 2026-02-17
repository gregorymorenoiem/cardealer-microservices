using FeatureToggleService.Application.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Queries.GetVariantAssignment;

public class GetVariantAssignmentHandler : IRequestHandler<GetVariantAssignmentQuery, VariantAssignmentResponse>
{
    private readonly IABTestingService _abTestingService;

    public GetVariantAssignmentHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<VariantAssignmentResponse> Handle(GetVariantAssignmentQuery request, CancellationToken cancellationToken)
    {
        var experiment = await _abTestingService.GetExperimentByKeyAsync(request.ExperimentKey, cancellationToken);

        if (experiment == null)
        {
            throw new KeyNotFoundException($"Experiment with key '{request.ExperimentKey}' not found");
        }

        if (!experiment.IsRunning())
        {
            throw new InvalidOperationException($"Experiment is not running (status: {experiment.Status})");
        }

        var assignment = await _abTestingService.AssignVariantAsync(
            experiment.Id,
            request.UserId,
            request.SessionId,
            request.DeviceId,
            request.UserAttributes,
            cancellationToken
        );

        return new VariantAssignmentResponse
        {
            ExperimentId = experiment.Id,
            ExperimentKey = experiment.Key,
            ExperimentName = experiment.Name,
            VariantId = assignment.VariantId,
            VariantKey = assignment.Variant?.Key ?? string.Empty,
            VariantName = assignment.Variant?.Name ?? string.Empty,
            IsControl = assignment.Variant?.IsControl ?? false,
            Payload = assignment.Variant?.Payload,
            FeatureFlagValue = assignment.Variant?.FeatureFlagValue,
            Parameters = assignment.Variant?.Parameters,
            StyleOverrides = assignment.Variant?.StyleOverrides,
            AssignedAt = assignment.AssignedAt,
            IsExposed = assignment.IsExposed,
            HasConverted = assignment.HasConverted
        };
    }
}
