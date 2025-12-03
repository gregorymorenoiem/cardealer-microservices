namespace FeatureToggleService.Domain.Enums;

/// <summary>
/// Status of an A/B test experiment
/// </summary>
public enum ExperimentStatus
{
    /// <summary>Experiment is being drafted</summary>
    Draft,

    /// <summary>Experiment is running</summary>
    Running,

    /// <summary>Experiment is paused</summary>
    Paused,

    /// <summary>Experiment has completed successfully</summary>
    Completed,

    /// <summary>Experiment was cancelled</summary>
    Cancelled
}
