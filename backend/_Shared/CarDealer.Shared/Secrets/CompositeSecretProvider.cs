using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Secrets;

/// <summary>
/// Proveedor de secretos compuesto que combina múltiples fuentes con fallback.
/// Orden de prioridad por defecto:
/// 1. Variables de entorno (mayor prioridad - permite override en runtime)
/// 2. Docker Secrets
/// 3. Proveedores adicionales (Vault, Azure Key Vault, etc.)
/// </summary>
public class CompositeSecretProvider : ISecretProvider
{
    private readonly List<ISecretProvider> _providers;
    private readonly ILogger<CompositeSecretProvider>? _logger;

    /// <summary>
    /// Crea un proveedor compuesto con los proveedores por defecto.
    /// </summary>
    public CompositeSecretProvider(ILogger<CompositeSecretProvider>? logger = null)
    {
        _logger = logger;
        _providers = new List<ISecretProvider>
        {
            new EnvironmentSecretProvider(),
            new DockerSecretProvider()
        };
    }

    /// <summary>
    /// Crea un proveedor compuesto con proveedores personalizados.
    /// </summary>
    /// <param name="providers">Lista de proveedores en orden de prioridad</param>
    /// <param name="logger">Logger opcional</param>
    public CompositeSecretProvider(
        IEnumerable<ISecretProvider> providers,
        ILogger<CompositeSecretProvider>? logger = null)
    {
        _providers = providers.ToList();
        _logger = logger;

        if (_providers.Count == 0)
        {
            throw new ArgumentException("At least one secret provider must be specified", nameof(providers));
        }
    }

    /// <summary>
    /// Añade un proveedor adicional al final de la lista.
    /// </summary>
    public CompositeSecretProvider AddProvider(ISecretProvider provider)
    {
        _providers.Add(provider);
        return this;
    }

    /// <summary>
    /// Inserta un proveedor al inicio de la lista (mayor prioridad).
    /// </summary>
    public CompositeSecretProvider InsertProvider(ISecretProvider provider)
    {
        _providers.Insert(0, provider);
        return this;
    }

    /// <inheritdoc />
    public string? GetSecret(string key)
    {
        foreach (var provider in _providers)
        {
            var value = provider.GetSecret(key);
            if (!string.IsNullOrEmpty(value))
            {
                _logger?.LogDebug(
                    "Secret '{Key}' found in provider: {Provider}",
                    key,
                    provider.GetType().Name);
                return value;
            }
        }

        _logger?.LogDebug("Secret '{Key}' not found in any provider", key);
        return null;
    }

    /// <inheritdoc />
    public string GetRequiredSecret(string key)
    {
        var value = GetSecret(key);

        if (string.IsNullOrEmpty(value))
        {
            var providerNames = string.Join(", ", _providers.Select(p => p.GetType().Name));
            throw new SecretNotFoundException(key,
                $"Required secret '{key}' was not found in any configured provider. " +
                $"Searched providers: {providerNames}. " +
                $"Configure the secret via environment variable or Docker secret.");
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
        return _providers.Any(p => p.HasSecret(key));
    }

    /// <inheritdoc />
    public IDictionary<string, string> GetSecretsWithPrefix(string prefix)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Iterar en orden inverso para que los de mayor prioridad sobrescriban
        foreach (var provider in _providers.AsEnumerable().Reverse())
        {
            var secrets = provider.GetSecretsWithPrefix(prefix);
            foreach (var kvp in secrets)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Crea un proveedor compuesto con la configuración por defecto optimizada para contenedores.
    /// </summary>
    public static CompositeSecretProvider CreateDefault(ILogger<CompositeSecretProvider>? logger = null)
    {
        return new CompositeSecretProvider(logger);
    }
}
