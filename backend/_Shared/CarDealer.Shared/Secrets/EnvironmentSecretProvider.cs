using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Secrets;

/// <summary>
/// Proveedor de secretos que lee desde variables de entorno.
/// Esta es la opción recomendada para CI/CD y contenedores.
/// </summary>
public class EnvironmentSecretProvider : ISecretProvider
{
    private readonly ILogger<EnvironmentSecretProvider>? _logger;
    private readonly bool _logSecretAccess;

    /// <summary>
    /// Crea un nuevo proveedor de secretos de entorno.
    /// </summary>
    /// <param name="logger">Logger opcional para diagnóstico</param>
    /// <param name="logSecretAccess">Si true, loguea cuando se acceden secretos (solo nombres, no valores)</param>
    public EnvironmentSecretProvider(
        ILogger<EnvironmentSecretProvider>? logger = null,
        bool logSecretAccess = false)
    {
        _logger = logger;
        _logSecretAccess = logSecretAccess;
    }

    /// <inheritdoc />
    public string? GetSecret(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        // Normalizar el nombre de la variable (soportar tanto SNAKE_CASE como kebab-case)
        var normalizedKey = NormalizeKey(key);
        var value = Environment.GetEnvironmentVariable(normalizedKey);

        if (_logSecretAccess && _logger != null)
        {
            if (value != null)
            {
                _logger.LogDebug("Secret '{Key}' retrieved from environment variable", normalizedKey);
            }
            else
            {
                _logger.LogDebug("Secret '{Key}' not found in environment variables", normalizedKey);
            }
        }

        return value;
    }

    /// <inheritdoc />
    public string GetRequiredSecret(string key)
    {
        var value = GetSecret(key);

        if (string.IsNullOrEmpty(value))
        {
            _logger?.LogError(
                "Required secret '{Key}' not found. Set environment variable '{NormalizedKey}'",
                key,
                NormalizeKey(key));

            throw new SecretNotFoundException(key,
                $"Required secret '{key}' was not found. " +
                $"Set environment variable '{NormalizeKey(key)}' or configure via Docker secrets.");
        }

        return value;
    }

    /// <inheritdoc />
    public string GetSecretOrDefault(string key, string defaultValue)
    {
        var value = GetSecret(key);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    /// <inheritdoc />
    public bool HasSecret(string key)
    {
        var normalizedKey = NormalizeKey(key);
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(normalizedKey));
    }

    /// <inheritdoc />
    public IDictionary<string, string> GetSecretsWithPrefix(string prefix)
    {
        var normalizedPrefix = NormalizeKey(prefix);
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in Environment.GetEnvironmentVariables().Keys.Cast<string>())
        {
            if (key.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var value = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrEmpty(value))
                {
                    result[key] = value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Normaliza el nombre de la clave para variables de entorno.
    /// Convierte a UPPER_SNAKE_CASE.
    /// </summary>
    private static string NormalizeKey(string key)
    {
        // Reemplazar puntos y guiones por underscores
        return key
            .Replace('.', '_')
            .Replace('-', '_')
            .Replace(":", "__") // Soportar secciones de configuración (ConnectionStrings:Default)
            .ToUpperInvariant();
    }
}
