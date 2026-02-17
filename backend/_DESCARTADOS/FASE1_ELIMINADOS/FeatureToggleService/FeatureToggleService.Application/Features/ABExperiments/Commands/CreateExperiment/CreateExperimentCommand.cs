using FeatureToggleService.Domain.Entities;
using FeatureToggleService.Domain.Enums;
using MediatR;

namespace FeatureToggleService.Application.Features.ABExperiments.Commands.CreateExperiment;

/// <summary>
/// Command to create a new A/B test experiment with variants
/// </summary>
public class CreateExperimentCommand : IRequest<CreateExperimentResponse>
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Hypothesis { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? FeatureFlagId { get; set; }
    public string PrimaryMetric { get; set; } = "conversion";
    public List<string> SecondaryMetrics { get; set; } = new();

    public int TrafficAllocation { get; set; } = 100;
    public int MinSampleSize { get; set; } = 1000;
    public double SignificanceLevel { get; set; } = 0.95;
    public double? MinDetectableEffect { get; set; }

    public bool UseStickyBucketing { get; set; } = true;
    public Dictionary<string, object>? SegmentationRules { get; set; }

    public List<CreateVariantDto> Variants { get; set; } = new();
}

public class CreateVariantDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsControl { get; set; }
    public int Weight { get; set; } = 50;
    public string? Payload { get; set; }
    public bool? FeatureFlagValue { get; set; }
    public string? StyleOverrides { get; set; }
    public string? Parameters { get; set; }
    public string? MockupUrl { get; set; }
}

public class CreateExperimentResponse
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ExperimentStatus Status { get; set; }
    public List<VariantDto> Variants { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class VariantDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsControl { get; set; }
    public int Weight { get; set; }
}
