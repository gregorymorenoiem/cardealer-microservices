namespace FeatureStoreService.Application.DTOs;

public record UserFeatureDto(
    Guid Id,
    Guid UserId,
    string FeatureName,
    string FeatureValue,
    string FeatureType,
    int Version,
    DateTime ComputedAt,
    DateTime? ExpiresAt,
    string Source
);

public record VehicleFeatureDto(
    Guid Id,
    Guid VehicleId,
    string FeatureName,
    string FeatureValue,
    string FeatureType,
    int Version,
    DateTime ComputedAt,
    DateTime? ExpiresAt,
    string Source
);

public record FeatureDefinitionDto(
    Guid Id,
    string FeatureName,
    string Category,
    string Description,
    string FeatureType,
    bool IsActive,
    string ComputationLogic,
    int RefreshIntervalHours,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record FeatureBatchDto(
    Guid Id,
    string BatchName,
    List<string> FeatureNames,
    string TargetEntity,
    int TotalEntitiesProcessed,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Status
);

public record UpsertUserFeatureRequest(
    Guid UserId,
    string FeatureName,
    string FeatureValue,
    string FeatureType = "Numeric",
    int Version = 1,
    DateTime? ExpiresAt = null
);

public record UpsertVehicleFeatureRequest(
    Guid VehicleId,
    string FeatureName,
    string FeatureValue,
    string FeatureType = "Numeric",
    int Version = 1,
    DateTime? ExpiresAt = null
);

public record RegisterFeatureDefinitionRequest(
    string FeatureName,
    string Category,
    string Description,
    string FeatureType,
    string ComputationLogic,
    int RefreshIntervalHours = 24
);
