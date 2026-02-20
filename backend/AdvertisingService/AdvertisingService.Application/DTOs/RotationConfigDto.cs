using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Application.DTOs;

public record RotationConfigDto(
    Guid Id,
    AdPlacementType Section,
    RotationAlgorithmType AlgorithmType,
    int RefreshIntervalMinutes,
    int MaxVehiclesShown,
    decimal WeightRemainingBudget,
    decimal WeightCtr,
    decimal WeightQualityScore,
    decimal WeightRecency,
    bool IsActive,
    DateTime UpdatedAt
);
