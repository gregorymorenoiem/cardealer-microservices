using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarDealer.Shared.Secrets;

/// <summary>
/// Extensiones para configurar el sistema de secretos en la aplicación.
/// </summary>
public static class SecretProviderExtensions
{
    /// <summary>
    /// Añade el sistema de secretos con la configuración por defecto.
    /// Registra ISecretProvider como singleton.
    /// </summary>
    public static IServiceCollection AddSecretProvider(this IServiceCollection services)
    {
        services.AddSingleton<ISecretProvider>(sp =>
        {
            var logger = sp.GetService<ILogger<CompositeSecretProvider>>();
            return CompositeSecretProvider.CreateDefault(logger);
        });

        return services;
    }

    /// <summary>
    /// Añade el sistema de secretos con configuración personalizada.
    /// </summary>
    public static IServiceCollection AddSecretProvider(
        this IServiceCollection services,
        Action<SecretProviderOptions> configure)
    {
        var options = new SecretProviderOptions();
        configure(options);

        services.AddSingleton<ISecretProvider>(sp =>
        {
            var logger = sp.GetService<ILogger<CompositeSecretProvider>>();
            return options.Build(sp, logger);
        });

        return services;
    }

    /// <summary>
    /// Añade secretos como fuente de configuración de IConfiguration.
    /// Permite usar IConfiguration para leer secretos.
    /// </summary>
    public static IConfigurationBuilder AddSecrets(
        this IConfigurationBuilder builder,
        ISecretProvider? secretProvider = null)
    {
        secretProvider ??= CompositeSecretProvider.CreateDefault();
        builder.Add(new SecretConfigurationSource(secretProvider));
        return builder;
    }

    /// <summary>
    /// Obtiene una configuración segura que combina IConfiguration con ISecretProvider.
    /// Los secretos tienen prioridad sobre la configuración estática.
    /// </summary>
    public static string GetSecretOrConfig(
        this IConfiguration configuration,
        ISecretProvider secretProvider,
        string key,
        string? defaultValue = null)
    {
        // Primero buscar en secretos
        var secret = secretProvider.GetSecret(key);
        if (!string.IsNullOrEmpty(secret))
        {
            return secret;
        }

        // Luego en configuración
        var configValue = configuration[key];
        if (!string.IsNullOrEmpty(configValue))
        {
            return configValue;
        }

        // Finalmente el valor por defecto
        return defaultValue ?? string.Empty;
    }

    /// <summary>
    /// Verifica que todos los secretos requeridos estén configurados.
    /// Útil para validación al startup.
    /// </summary>
    public static void ValidateRequiredSecrets(
        this ISecretProvider secretProvider,
        params string[] requiredSecrets)
    {
        var missingSecrets = new List<string>();

        foreach (var key in requiredSecrets)
        {
            if (!secretProvider.HasSecret(key))
            {
                missingSecrets.Add(key);
            }
        }

        if (missingSecrets.Count > 0)
        {
            throw new SecretNotFoundException(
                string.Join(", ", missingSecrets),
                $"The following required secrets are missing: {string.Join(", ", missingSecrets)}. " +
                $"Configure them via environment variables or Docker secrets before starting the application.");
        }
    }

    /// <summary>
    /// Verifica secretos requeridos con logging de los faltantes.
    /// </summary>
    public static bool TryValidateRequiredSecrets(
        this ISecretProvider secretProvider,
        ILogger logger,
        params string[] requiredSecrets)
    {
        var missingSecrets = new List<string>();

        foreach (var key in requiredSecrets)
        {
            if (!secretProvider.HasSecret(key))
            {
                missingSecrets.Add(key);
            }
        }

        if (missingSecrets.Count > 0)
        {
            logger.LogError(
                "Missing required secrets: {MissingSecrets}. " +
                "Configure them via environment variables or Docker secrets.",
                string.Join(", ", missingSecrets));
            return false;
        }

        logger.LogInformation("All required secrets validated successfully");
        return true;
    }
}

/// <summary>
/// Opciones para configurar el proveedor de secretos.
/// </summary>
public class SecretProviderOptions
{
    private readonly List<Func<IServiceProvider, ISecretProvider>> _providerFactories = new();

    /// <summary>
    /// Incluir variables de entorno como fuente de secretos (por defecto: true).
    /// </summary>
    public bool UseEnvironmentVariables { get; set; } = true;

    /// <summary>
    /// Incluir Docker Secrets como fuente (por defecto: true).
    /// </summary>
    public bool UseDockerSecrets { get; set; } = true;

    /// <summary>
    /// Path personalizado para Docker Secrets.
    /// </summary>
    public string? DockerSecretsPath { get; set; }

    /// <summary>
    /// Añade un proveedor de secretos personalizado.
    /// </summary>
    public SecretProviderOptions AddProvider<T>() where T : ISecretProvider, new()
    {
        _providerFactories.Add(_ => new T());
        return this;
    }

    /// <summary>
    /// Añade un proveedor de secretos con factory.
    /// </summary>
    public SecretProviderOptions AddProvider(Func<IServiceProvider, ISecretProvider> factory)
    {
        _providerFactories.Add(factory);
        return this;
    }

    internal ISecretProvider Build(IServiceProvider sp, ILogger<CompositeSecretProvider>? logger)
    {
        var providers = new List<ISecretProvider>();

        if (UseEnvironmentVariables)
        {
            var envLogger = sp.GetService<ILogger<EnvironmentSecretProvider>>();
            providers.Add(new EnvironmentSecretProvider(envLogger));
        }

        if (UseDockerSecrets)
        {
            var dockerLogger = sp.GetService<ILogger<DockerSecretProvider>>();
            providers.Add(new DockerSecretProvider(DockerSecretsPath, dockerLogger));
        }

        foreach (var factory in _providerFactories)
        {
            providers.Add(factory(sp));
        }

        if (providers.Count == 0)
        {
            throw new InvalidOperationException(
                "No secret providers configured. Enable at least one provider.");
        }

        return new CompositeSecretProvider(providers, logger);
    }
}

/// <summary>
/// Fuente de configuración que lee de ISecretProvider.
/// </summary>
internal class SecretConfigurationSource : IConfigurationSource
{
    private readonly ISecretProvider _secretProvider;

    public SecretConfigurationSource(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SecretConfigurationProvider(_secretProvider);
    }
}

/// <summary>
/// Proveedor de configuración que lee de ISecretProvider.
/// </summary>
internal class SecretConfigurationProvider : ConfigurationProvider
{
    private readonly ISecretProvider _secretProvider;

    public SecretConfigurationProvider(ISecretProvider secretProvider)
    {
        _secretProvider = secretProvider;
    }

    public override bool TryGet(string key, out string? value)
    {
        value = _secretProvider.GetSecret(key);
        return value != null;
    }
}
