using ConfigurationService.Domain.Entities;

namespace ConfigurationService.Application.Interfaces;

public interface IConfigurationManager
{
    Task<ConfigurationItem?> GetConfigurationAsync(string key, string environment, string? tenantId = null);
    Task<IEnumerable<ConfigurationItem>> GetAllConfigurationsAsync(string environment, string? tenantId = null);
    Task<ConfigurationItem> CreateConfigurationAsync(ConfigurationItem item);
    Task<ConfigurationItem> UpdateConfigurationAsync(Guid id, string value, string updatedBy, string? changeReason = null);
    Task DeleteConfigurationAsync(Guid id);
    Task<IEnumerable<ConfigurationHistory>> GetConfigurationHistoryAsync(Guid configurationItemId);
}
