using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Enums;
using MediatR;

namespace AdvertisingService.Application.Features.Rotation.Commands.UpdateRotationConfig;

public record UpdateRotationConfigCommand(
    AdPlacementType Section,
    RotationAlgorithmType? AlgorithmType,
    int? RefreshIntervalMinutes,
    int? MaxVehiclesShown,
    decimal? WeightRemainingBudget,
    decimal? WeightCtr,
    decimal? WeightQualityScore,
    decimal? WeightRecency,
    bool? IsActive
) : IRequest<RotationConfigDto>;
