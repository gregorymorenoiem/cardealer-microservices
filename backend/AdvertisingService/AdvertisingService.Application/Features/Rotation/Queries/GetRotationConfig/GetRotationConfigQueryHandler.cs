using AdvertisingService.Application.DTOs;
using AdvertisingService.Domain.Interfaces;
using MediatR;

namespace AdvertisingService.Application.Features.Rotation.Queries.GetRotationConfig;

public class GetRotationConfigQueryHandler : IRequestHandler<GetRotationConfigQuery, RotationConfigDto?>
{
    private readonly IRotationConfigRepository _configRepo;

    public GetRotationConfigQueryHandler(IRotationConfigRepository configRepo) => _configRepo = configRepo;

    public async Task<RotationConfigDto?> Handle(GetRotationConfigQuery request, CancellationToken ct)
    {
        var config = await _configRepo.GetBySectionAsync(request.Section, ct);
        if (config == null) return null;

        return new RotationConfigDto(
            config.Id, config.Section, config.AlgorithmType, config.RefreshIntervalMinutes,
            config.MaxVehiclesShown, config.WeightRemainingBudget, config.WeightCtr,
            config.WeightQualityScore, config.WeightRecency, config.IsActive, config.UpdatedAt
        );
    }
}
