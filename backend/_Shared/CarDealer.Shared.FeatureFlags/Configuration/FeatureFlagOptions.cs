namespace CarDealer.Shared.FeatureFlags.Configuration;

/// <summary>
/// Opciones de configuración para el cliente de Feature Flags
/// </summary>
public class FeatureFlagOptions
{
    public const string SectionName = "FeatureFlags";

    /// <summary>
    /// URL base del FeatureToggleService
    /// </summary>
    public string ServiceUrl { get; set; } = "http://featuretoggleservice";

    /// <summary>
    /// Tiempo de cache en segundos (default: 60)
    /// </summary>
    public int CacheTimeSeconds { get; set; } = 60;

    /// <summary>
    /// Intervalo de polling en segundos (default: 30)
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Habilitar cache local
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// Habilitar polling automático para actualizaciones
    /// </summary>
    public bool EnablePolling { get; set; } = true;

    /// <summary>
    /// Timeout de HTTP en segundos
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Ambiente actual (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Valor por defecto cuando hay error de conexión
    /// </summary>
    public bool DefaultValueOnError { get; set; } = false;
}
