using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Secrets;

/// <summary>
/// Proveedor de secretos que lee desde Docker Secrets.
/// Los secretos de Docker se montan en /run/secrets/ como archivos.
/// Compatible con Docker Swarm y Kubernetes Secrets.
/// </summary>
public class DockerSecretProvider : ISecretProvider
{
    private readonly string _secretsPath;
    private readonly ILogger<DockerSecretProvider>? _logger;
    private readonly Dictionary<string, string> _secretCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly bool _cacheSecrets;

    /// <summary>
    /// Path por defecto donde Docker monta los secretos.
    /// </summary>
    public const string DefaultSecretsPath = "/run/secrets";

    /// <summary>
    /// Crea un nuevo proveedor de Docker Secrets.
    /// </summary>
    /// <param name="secretsPath">Path donde están montados los secretos (default: /run/secrets)</param>
    /// <param name="logger">Logger opcional para diagnóstico</param>
    /// <param name="cacheSecrets">Si true, cachea los secretos en memoria después de leerlos</param>
    public DockerSecretProvider(
        string? secretsPath = null,
        ILogger<DockerSecretProvider>? logger = null,
        bool cacheSecrets = true)
    {
        _secretsPath = secretsPath ?? DefaultSecretsPath;
        _logger = logger;
        _cacheSecrets = cacheSecrets;

        // En Windows, usar un path alternativo para desarrollo
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && !Directory.Exists(_secretsPath))
        {
            _secretsPath = Path.Combine(AppContext.BaseDirectory, "secrets");
            _logger?.LogDebug(
                "Running on Windows, using alternative secrets path: {Path}",
                _secretsPath);
        }
    }

    /// <inheritdoc />
    public string? GetSecret(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        // Verificar cache primero
        if (_cacheSecrets && _secretCache.TryGetValue(key, out var cachedValue))
        {
            return cachedValue;
        }

        var normalizedKey = NormalizeKey(key);
        var secretPath = Path.Combine(_secretsPath, normalizedKey);

        if (!File.Exists(secretPath))
        {
            _logger?.LogDebug(
                "Docker secret '{Key}' not found at path: {Path}",
                key,
                secretPath);
            return null;
        }

        try
        {
            var value = File.ReadAllText(secretPath).Trim();

            if (_cacheSecrets)
            {
                _secretCache[key] = value;
            }

            _logger?.LogDebug("Docker secret '{Key}' loaded from {Path}", key, secretPath);
            return value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reading Docker secret '{Key}' from {Path}", key, secretPath);
            return null;
        }
    }

    /// <inheritdoc />
    public string GetRequiredSecret(string key)
    {
        var value = GetSecret(key);

        if (string.IsNullOrEmpty(value))
        {
            throw new SecretNotFoundException(key,
                $"Required Docker secret '{key}' was not found at '{Path.Combine(_secretsPath, NormalizeKey(key))}'. " +
                $"Ensure the secret is configured in your Docker Swarm or Kubernetes deployment.");
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
        var secretPath = Path.Combine(_secretsPath, normalizedKey);
        return File.Exists(secretPath);
    }

    /// <inheritdoc />
    public IDictionary<string, string> GetSecretsWithPrefix(string prefix)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_secretsPath))
        {
            return result;
        }

        var normalizedPrefix = NormalizeKey(prefix);

        foreach (var file in Directory.GetFiles(_secretsPath))
        {
            var fileName = Path.GetFileName(file);
            if (fileName.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var value = File.ReadAllText(file).Trim();
                    result[fileName] = value;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error reading secret file: {File}", file);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Verifica si el directorio de secretos existe y es accesible.
    /// </summary>
    public bool IsAvailable()
    {
        return Directory.Exists(_secretsPath);
    }

    /// <summary>
    /// Normaliza el nombre del secreto para el sistema de archivos.
    /// Docker secrets típicamente usan snake_case en minúsculas.
    /// </summary>
    private static string NormalizeKey(string key)
    {
        // Docker secrets típicamente están en minúsculas con underscores
        return key
            .Replace('.', '_')
            .Replace('-', '_')
            .Replace(":", "_")
            .ToLowerInvariant();
    }
}
