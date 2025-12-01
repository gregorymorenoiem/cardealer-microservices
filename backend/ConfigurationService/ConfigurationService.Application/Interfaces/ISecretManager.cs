using ConfigurationService.Domain.Entities;

namespace ConfigurationService.Application.Interfaces;

public interface ISecretManager
{
    Task<string?> GetDecryptedSecretAsync(string key, string environment, string? tenantId = null);
    Task<EncryptedSecret> CreateSecretAsync(string key, string plainValue, string environment, string createdBy, string? description = null, string? tenantId = null, DateTime? expiresAt = null);
    Task<EncryptedSecret> UpdateSecretAsync(Guid id, string plainValue, string updatedBy);
    Task DeleteSecretAsync(Guid id);
    Task<IEnumerable<EncryptedSecret>> GetAllSecretsAsync(string environment, string? tenantId = null);
}
