using MediatR;
using FeatureStoreService.Application.DTOs;
using FeatureStoreService.Domain.Entities;
using FeatureStoreService.Domain.Interfaces;

namespace FeatureStoreService.Application.Features.Commands;

public record UpsertUserFeatureCommand(
    Guid UserId,
    string FeatureName,
    string FeatureValue,
    string FeatureType = "Numeric",
    int Version = 1,
    DateTime? ExpiresAt = null
) : IRequest<UserFeatureDto>;

public class UpsertUserFeatureHandler : IRequestHandler<UpsertUserFeatureCommand, UserFeatureDto>
{
    private readonly IFeatureStoreRepository _repository;

    public UpsertUserFeatureHandler(IFeatureStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserFeatureDto> Handle(UpsertUserFeatureCommand request, CancellationToken ct)
    {
        var feature = new UserFeature
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FeatureName = request.FeatureName,
            FeatureValue = request.FeatureValue,
            FeatureType = request.FeatureType,
            Version = request.Version,
            ComputedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            Source = "Manual"
        };

        var result = await _repository.UpsertUserFeatureAsync(feature, ct);

        return new UserFeatureDto(
            result.Id,
            result.UserId,
            result.FeatureName,
            result.FeatureValue,
            result.FeatureType,
            result.Version,
            result.ComputedAt,
            result.ExpiresAt,
            result.Source
        );
    }
}

public record UpsertVehicleFeatureCommand(
    Guid VehicleId,
    string FeatureName,
    string FeatureValue,
    string FeatureType = "Numeric",
    int Version = 1,
    DateTime? ExpiresAt = null
) : IRequest<VehicleFeatureDto>;

public class UpsertVehicleFeatureHandler : IRequestHandler<UpsertVehicleFeatureCommand, VehicleFeatureDto>
{
    private readonly IFeatureStoreRepository _repository;

    public UpsertVehicleFeatureHandler(IFeatureStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<VehicleFeatureDto> Handle(UpsertVehicleFeatureCommand request, CancellationToken ct)
    {
        var feature = new VehicleFeature
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            FeatureName = request.FeatureName,
            FeatureValue = request.FeatureValue,
            FeatureType = request.FeatureType,
            Version = request.Version,
            ComputedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            Source = "Manual"
        };

        var result = await _repository.UpsertVehicleFeatureAsync(feature, ct);

        return new VehicleFeatureDto(
            result.Id,
            result.VehicleId,
            result.FeatureName,
            result.FeatureValue,
            result.FeatureType,
            result.Version,
            result.ComputedAt,
            result.ExpiresAt,
            result.Source
        );
    }
}
