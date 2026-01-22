namespace CarDealer.Shared.FeatureFlags.Models;

/// <summary>
/// Representa un Feature Flag obtenido del FeatureToggleService
/// </summary>
public class FeatureFlagDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? Environment { get; set; }
    public int? RolloutPercentage { get; set; }
    public List<string>? TargetUsers { get; set; }
    public List<string>? TargetGroups { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Resultado de evaluación de un feature flag
/// </summary>
public class FeatureFlagEvaluationResult
{
    public string Key { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? Variant { get; set; }
    public string Reason { get; set; } = "default";
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Contexto para evaluación de feature flags
/// </summary>
public class FeatureFlagContext
{
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public List<string>? UserGroups { get; set; }
    public string? Environment { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
}
