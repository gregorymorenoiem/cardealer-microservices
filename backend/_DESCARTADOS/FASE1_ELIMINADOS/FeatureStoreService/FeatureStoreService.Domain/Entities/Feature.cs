namespace FeatureStoreService.Domain.Entities;

/// <summary>
/// Feature de un usuario para ML/Analytics
/// </summary>
public class UserFeature
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureValue { get; set; } = string.Empty; // JSON o primitivo
    public string FeatureType { get; set; } = "Numeric"; // "Numeric", "Categorical", "Boolean", "List"
    public int Version { get; set; } = 1;
    public DateTime ComputedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Source { get; set; } = "System"; // "System", "Manual", "EventPipeline"
}

/// <summary>
/// Feature de un vehículo para ML/Analytics
/// </summary>
public class VehicleFeature
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureValue { get; set; } = string.Empty; // JSON o primitivo
    public string FeatureType { get; set; } = "Numeric"; // "Numeric", "Categorical", "Boolean", "List"
    public int Version { get; set; } = 1;
    public DateTime ComputedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Source { get; set; } = "System";
}

/// <summary>
/// Definición de feature (metadata)
/// </summary>
public class FeatureDefinition
{
    public Guid Id { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "User", "Vehicle", "Behavioral", "Statistical"
    public string Description { get; set; } = string.Empty;
    public string FeatureType { get; set; } = "Numeric";
    public bool IsActive { get; set; } = true;
    public string ComputationLogic { get; set; } = string.Empty; // SQL, code reference, etc.
    public int RefreshIntervalHours { get; set; } = 24; // Cuántas horas antes de recomputar
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Batch de features (para ML pipelines)
/// </summary>
public class FeatureBatch
{
    public Guid Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public List<string> FeatureNames { get; set; } = new();
    public string TargetEntity { get; set; } = string.Empty; // "User" o "Vehicle"
    public int TotalEntitiesProcessed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Running"; // "Running", "Completed", "Failed"
}
