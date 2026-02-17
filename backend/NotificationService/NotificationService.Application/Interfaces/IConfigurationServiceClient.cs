namespace NotificationService.Application.Interfaces;

/// <summary>
/// Client to read configuration toggles and secrets from ConfigurationService.
/// Used to check if notification channels (SMS, Email, Push, WhatsApp) are enabled/disabled
/// from the admin configuration panel, and to read webhook URLs and API tokens.
/// </summary>
public interface IConfigurationServiceClient
{
    /// <summary>
    /// Get a configuration value by key. Returns null if not found or service unavailable.
    /// </summary>
    Task<string?> GetValueAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Check if a configuration toggle is enabled (value == "true").
    /// Returns true by default if the key is not found (fail-open).
    /// </summary>
    Task<bool> IsEnabledAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Get an encrypted secret value by key from ConfigurationService.
    /// Returns null if not found or service unavailable.
    /// </summary>
    Task<string?> GetSecretAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Get multiple configuration values in parallel.
    /// </summary>
    Task<Dictionary<string, string?>> GetValuesAsync(IEnumerable<string> keys, CancellationToken ct = default);
}
