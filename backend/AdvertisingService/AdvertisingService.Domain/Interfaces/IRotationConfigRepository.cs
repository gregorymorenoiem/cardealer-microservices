using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;

namespace AdvertisingService.Domain.Interfaces;

public interface IRotationConfigRepository
{
    Task<RotationConfig?> GetBySectionAsync(AdPlacementType section, CancellationToken ct = default);
    Task UpdateAsync(RotationConfig config, CancellationToken ct = default);
}
