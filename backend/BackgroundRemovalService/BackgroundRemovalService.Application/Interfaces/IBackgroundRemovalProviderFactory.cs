using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;

namespace BackgroundRemovalService.Application.Interfaces;

/// <summary>
/// Factory para obtener el proveedor correcto de remoción de fondo.
/// Implementa el Strategy Pattern para selección dinámica de proveedores.
/// </summary>
public interface IBackgroundRemovalProviderFactory
{
    /// <summary>
    /// Obtiene un proveedor específico por tipo
    /// </summary>
    IBackgroundRemovalProvider? GetProvider(BackgroundRemovalProvider providerType);
    
    /// <summary>
    /// Obtiene el mejor proveedor disponible basado en configuración y disponibilidad
    /// </summary>
    Task<IBackgroundRemovalProvider?> GetBestAvailableProviderAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los proveedores registrados
    /// </summary>
    IEnumerable<IBackgroundRemovalProvider> GetAllProviders();
    
    /// <summary>
    /// Obtiene todos los proveedores disponibles (enabled y no en circuit breaker)
    /// </summary>
    Task<IEnumerable<IBackgroundRemovalProvider>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra un nuevo proveedor
    /// </summary>
    void RegisterProvider(IBackgroundRemovalProvider provider);
}
