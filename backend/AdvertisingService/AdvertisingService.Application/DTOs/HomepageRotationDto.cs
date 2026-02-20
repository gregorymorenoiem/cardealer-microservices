using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Application.DTOs;

public record HomepageRotationDto(
    AdPlacementType Section,
    List<RotatedVehicleDto> Vehicles,
    RotationAlgorithmType AlgorithmUsed,
    DateTime GeneratedAt
);

public record RotatedVehicleDto(
    Guid? CampaignId,
    Guid VehicleId,
    Guid OwnerId,
    string OwnerType,
    int Position,
    decimal Score
);
