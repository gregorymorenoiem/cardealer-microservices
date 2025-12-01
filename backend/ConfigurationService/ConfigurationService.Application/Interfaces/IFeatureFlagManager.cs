using ConfigurationService.Domain.Entities;

namespace ConfigurationService.Application.Interfaces;

public interface IFeatureFlagManager
{
    Task<bool> IsFeatureEnabledAsync(string key, string? environment = null, string? tenantId = null, string? userId = null);
    Task<FeatureFlag?> GetFeatureFlagAsync(string key, string? environment = null);
    Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync(string? environment = null);
    Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag flag);
    Task<FeatureFlag> UpdateFeatureFlagAsync(Guid id, bool isEnabled, int? rolloutPercentage = null);
    Task DeleteFeatureFlagAsync(Guid id);
}
