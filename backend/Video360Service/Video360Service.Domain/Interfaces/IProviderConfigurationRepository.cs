using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Repositorio para configuración de proveedores
/// </summary>
public interface IProviderConfigurationRepository
{
    Task<ProviderConfiguration?> GetByProviderAsync(Video360Provider provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderConfiguration>> GetEnabledAsync(CancellationToken cancellationToken = default);
    Task<ProviderConfiguration> CreateOrUpdateAsync(ProviderConfiguration config, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProviderConfiguration config, CancellationToken cancellationToken = default);
    Task<ProviderConfiguration?> GetBestAvailableAsync(CancellationToken cancellationToken = default);
}
