using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Interfaces;

/// <summary>
/// Repositorio para configuración de proveedores
/// </summary>
public interface IProviderConfigurationRepository
{
    Task<ProviderConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProviderConfiguration?> GetByProviderAsync(BackgroundRemovalProvider provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderConfiguration>> GetEnabledProvidersAsync(CancellationToken cancellationToken = default);
    Task<ProviderConfiguration?> GetBestAvailableProviderAsync(CancellationToken cancellationToken = default);
    Task<ProviderConfiguration> CreateAsync(ProviderConfiguration config, CancellationToken cancellationToken = default);
    Task<ProviderConfiguration> UpdateAsync(ProviderConfiguration config, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
