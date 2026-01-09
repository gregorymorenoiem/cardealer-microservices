using MediatR;
using FeatureStoreService.Application.DTOs;
using FeatureStoreService.Domain.Interfaces;

namespace FeatureStoreService.Application.Features.Queries;

public record GetUserFeaturesQuery(Guid UserId) : IRequest<List<UserFeatureDto>>;

public class GetUserFeaturesHandler : IRequestHandler<GetUserFeaturesQuery, List<UserFeatureDto>>
{
    private readonly IFeatureStoreRepository _repository;

    public GetUserFeaturesHandler(IFeatureStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserFeatureDto>> Handle(GetUserFeaturesQuery request, CancellationToken ct)
    {
        var features = await _repository.GetUserFeaturesAsync(request.UserId, ct);
        return features.Select(f => new UserFeatureDto(
            f.Id,
            f.UserId,
            f.FeatureName,
            f.FeatureValue,
            f.FeatureType,
            f.Version,
            f.ComputedAt,
            f.ExpiresAt,
            f.Source
        )).ToList();
    }
}

public record GetVehicleFeaturesQuery(Guid VehicleId) : IRequest<List<VehicleFeatureDto>>;

public class GetVehicleFeaturesHandler : IRequestHandler<GetVehicleFeaturesQuery, List<VehicleFeatureDto>>
{
    private readonly IFeatureStoreRepository _repository;

    public GetVehicleFeaturesHandler(IFeatureStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<VehicleFeatureDto>> Handle(GetVehicleFeaturesQuery request, CancellationToken ct)
    {
        var features = await _repository.GetVehicleFeaturesAsync(request.VehicleId, ct);
        return features.Select(f => new VehicleFeatureDto(
            f.Id,
            f.VehicleId,
            f.FeatureName,
            f.FeatureValue,
            f.FeatureType,
            f.Version,
            f.ComputedAt,
            f.ExpiresAt,
            f.Source
        )).ToList();
    }
}

public record GetFeatureDefinitionsQuery(string? Category = null) : IRequest<List<FeatureDefinitionDto>>;

public class GetFeatureDefinitionsHandler : IRequestHandler<GetFeatureDefinitionsQuery, List<FeatureDefinitionDto>>
{
    private readonly IFeatureStoreRepository _repository;

    public GetFeatureDefinitionsHandler(IFeatureStoreRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FeatureDefinitionDto>> Handle(GetFeatureDefinitionsQuery request, CancellationToken ct)
    {
        var definitions = await _repository.GetFeatureDefinitionsAsync(request.Category, ct);
        return definitions.Select(d => new FeatureDefinitionDto(
            d.Id,
            d.FeatureName,
            d.Category,
            d.Description,
            d.FeatureType,
            d.IsActive,
            d.ComputationLogic,
            d.RefreshIntervalHours,
            d.CreatedAt,
            d.UpdatedAt
        )).ToList();
    }
}
