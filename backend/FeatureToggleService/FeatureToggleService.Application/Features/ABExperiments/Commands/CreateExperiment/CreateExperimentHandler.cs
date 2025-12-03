using FeatureToggleService.Application.Interfaces;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.CreateExperiment;

public class CreateExperimentHandler : IRequestHandler<CreateExperimentCommand, CreateExperimentResponse>
{
    private readonly IABTestingService _abTestingService;

    public CreateExperimentHandler(IABTestingService abTestingService)
    {
        _abTestingService = abTestingService;
    }

    public async Task<CreateExperimentResponse> Handle(CreateExperimentCommand request, CancellationToken cancellationToken)
    {
        // Validate variants
        if (request.Variants.Count < 2)
        {
            throw new ArgumentException("An experiment must have at least 2 variants (control + treatment)");
        }

        var controlCount = request.Variants.Count(v => v.IsControl);
        if (controlCount != 1)
        {
            throw new ArgumentException("An experiment must have exactly one control variant");
        }

        var totalWeight = request.Variants.Sum(v => v.Weight);
        if (totalWeight != 100)
        {
            throw new ArgumentException($"Variant weights must sum to 100 (current: {totalWeight})");
        }

        // Create experiment entity
        var experiment = new Domain.Entities.ABExperiment
        {
            Key = request.Key,
            Name = request.Name,
            Hypothesis = request.Hypothesis,
            Description = request.Description,
            FeatureFlagId = request.FeatureFlagId,
            PrimaryMetric = request.PrimaryMetric,
            SecondaryMetrics = request.SecondaryMetrics,
            TrafficAllocation = request.TrafficAllocation,
            MinSampleSize = request.MinSampleSize,
            SignificanceLevel = request.SignificanceLevel,
            MinDetectableEffect = request.MinDetectableEffect ?? 0.05,
            UseStickyBucketing = request.UseStickyBucketing,
            SegmentationRules = request.SegmentationRules?.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty) ?? new Dictionary<string, string>(),
            Status = Domain.Enums.ExperimentStatus.Draft
        };

        // Add variants
        foreach (var variantDto in request.Variants)
        {
            var variant = new Domain.Entities.ExperimentVariant
            {
                Key = variantDto.Key,
                Name = variantDto.Name,
                Description = variantDto.Description,
                IsControl = variantDto.IsControl,
                Weight = variantDto.Weight,
                Payload = variantDto.Payload,
                FeatureFlagValue = variantDto.FeatureFlagValue
            };
            experiment.Variants.Add(variant);
        }

        var updatedExperiment = await _abTestingService.CreateExperimentAsync(experiment, cancellationToken);

        return new CreateExperimentResponse
        {
            Id = updatedExperiment.Id,
            Key = updatedExperiment.Key,
            Name = updatedExperiment.Name,
            Status = updatedExperiment.Status,
            Variants = updatedExperiment.Variants.Select(v => new VariantDto
            {
                Id = v.Id,
                Key = v.Key,
                Name = v.Name,
                IsControl = v.IsControl,
                Weight = v.Weight
            }).ToList(),
            CreatedAt = updatedExperiment.CreatedAt
        };
    }
}
