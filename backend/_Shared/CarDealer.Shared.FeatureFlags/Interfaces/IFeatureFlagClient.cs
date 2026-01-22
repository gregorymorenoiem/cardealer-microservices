using CarDealer.Shared.FeatureFlags.Models;

namespace CarDealer.Shared.FeatureFlags.Interfaces;

/// <summary>
/// Cliente para evaluar feature flags
/// </summary>
public interface IFeatureFlagClient
{
    /// <summary>
    /// Verifica si un feature flag está habilitado
    /// </summary>
    Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un feature flag está habilitado para un contexto específico
    /// </summary>
    Task<bool> IsEnabledAsync(string key, FeatureFlagContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evalúa un feature flag y retorna el resultado completo
    /// </summary>
    Task<FeatureFlagEvaluationResult> EvaluateAsync(string key, FeatureFlagContext? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los feature flags
    /// </summary>
    Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un feature flag por su clave
    /// </summary>
    Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fuerza la recarga del cache de feature flags
    /// </summary>
    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
