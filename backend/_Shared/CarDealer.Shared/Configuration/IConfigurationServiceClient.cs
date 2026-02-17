namespace CarDealer.Shared.Configuration;

/// <summary>
/// Shared client to read configuration values and feature flags from ConfigurationService.
/// All microservices can use this to dynamically read platform config set via the admin panel.
/// Uses in-memory caching (60s TTL) and fails open when ConfigurationService is unavailable.
/// </summary>
public interface IConfigurationServiceClient
{
    /// <summary>
    /// Get a configuration value by key. Returns null if not found or service unavailable.
    /// </summary>
    Task<string?> GetValueAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Get a configuration value by key with a fallback default.
    /// </summary>
    Task<string> GetValueAsync(string key, string defaultValue, CancellationToken ct = default);

    /// <summary>
    /// Get a configuration value as integer. Returns defaultValue if not found or not parseable.
    /// </summary>
    Task<int> GetIntAsync(string key, int defaultValue, CancellationToken ct = default);

    /// <summary>
    /// Get a configuration value as decimal. Returns defaultValue if not found or not parseable.
    /// </summary>
    Task<decimal> GetDecimalAsync(string key, decimal defaultValue, CancellationToken ct = default);

    /// <summary>
    /// Check if a configuration toggle is enabled (value == "true").
    /// Returns defaultValue if the key is not found (configurable fail-open/fail-closed).
    /// </summary>
    Task<bool> IsEnabledAsync(string key, bool defaultValue = true, CancellationToken ct = default);

    /// <summary>
    /// Get multiple configuration values in parallel.
    /// </summary>
    Task<Dictionary<string, string?>> GetValuesAsync(IEnumerable<string> keys, CancellationToken ct = default);

    /// <summary>
    /// Get all configuration values for a category prefix (e.g., "vehicles", "kyc").
    /// </summary>
    Task<Dictionary<string, string>> GetByCategoryAsync(string category, CancellationToken ct = default);

    /// <summary>
    /// Invalidate cached values for a specific key or all keys.
    /// </summary>
    void InvalidateCache(string? key = null);
}
