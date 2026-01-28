using AdminService.Domain.Entities;

namespace AdminService.Domain.Interfaces;

/// <summary>
/// Interface para repositorio de estadísticas
/// </summary>
public interface IStatisticsRepository
{
    Task<PlatformStats> GetPlatformStatsAsync();
    Task<IEnumerable<PendingDealerInfo>> GetPendingDealersAsync(int limit);
    Task<PlatformSettings> GetPlatformSettingsAsync();
    Task UpdatePlatformSettingsAsync(Dictionary<string, object> settings);
}