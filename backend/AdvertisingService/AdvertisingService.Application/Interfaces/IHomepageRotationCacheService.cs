using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;

namespace AdvertisingService.Application.Interfaces;

/// <summary>
/// Abstraction for homepage rotation cache operations.
/// Implemented in Infrastructure layer by HomepageRotationCacheService.
/// </summary>
public interface IHomepageRotationCacheService
{
    Task<HomepageRotationResult?> GetRotationAsync(AdPlacementType section, CancellationToken ct = default);
    Task<HomepageRotationResult?> RefreshRotationAsync(AdPlacementType section, CancellationToken ct = default);
    Task InvalidateAsync(AdPlacementType section);
    Task InvalidateAllAsync();
}
