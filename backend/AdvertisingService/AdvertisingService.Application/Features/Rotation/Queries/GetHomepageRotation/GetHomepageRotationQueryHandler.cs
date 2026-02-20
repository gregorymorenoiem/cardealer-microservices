using AdvertisingService.Application.DTOs;
using AdvertisingService.Application.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.Rotation.Queries.GetHomepageRotation;

public class GetHomepageRotationQueryHandler : IRequestHandler<GetHomepageRotationQuery, HomepageRotationDto?>
{
    private readonly IHomepageRotationCacheService _cacheService;

    public GetHomepageRotationQueryHandler(IHomepageRotationCacheService cacheService)
        => _cacheService = cacheService;

    public async Task<HomepageRotationDto?> Handle(GetHomepageRotationQuery request, CancellationToken ct)
    {
        var result = await _cacheService.GetRotationAsync(request.Section, ct);
        if (result == null) return null;

        return new HomepageRotationDto(
            result.Section,
            result.Vehicles.Select(v => new RotatedVehicleDto(
                v.CampaignId, v.VehicleId, v.OwnerId, v.OwnerType, v.Position, v.Score
            )).ToList(),
            result.AlgorithmUsed,
            result.GeneratedAt
        );
    }
}
