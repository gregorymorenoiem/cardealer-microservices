using BackgroundRemovalService.Application.Interfaces;
using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundRemovalService.Infrastructure.Providers;

/// <summary>
/// Configuración general para el servicio de Background Removal
/// </summary>
public class BackgroundRemovalSettings
{
    public const string SectionName = "BackgroundRemoval";
    
    /// <summary>
    /// Proveedor por defecto (ClipDrop si no se especifica)
    /// </summary>
    public string DefaultProvider { get; set; } = "ClipDrop";
    
    /// <summary>
    /// Lista de proveedores de fallback en orden de prioridad
    /// </summary>
    public string[] FallbackProviders { get; set; } = { "RemoveBg", "Photoroom", "Slazzer" };
    
    /// <summary>
    /// Si está habilitado, usa proveedores de fallback cuando el principal falla
    /// </summary>
    public bool EnableFallback { get; set; } = true;
    
    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Factory para obtener proveedores de remoción de fondo.
/// Implementa el Strategy Pattern para selección dinámica.
/// ClipDrop es el proveedor por defecto según configuración.
/// </summary>
public class BackgroundRemovalProviderFactory : IBackgroundRemovalProviderFactory
{
    private readonly Dictionary<BackgroundRemovalProvider, IBackgroundRemovalProvider> _providers = new();
    private readonly IProviderConfigurationRepository _configRepository;
    private readonly ILogger<BackgroundRemovalProviderFactory> _logger;
    private readonly BackgroundRemovalSettings _settings;

    public BackgroundRemovalProviderFactory(
        IEnumerable<IBackgroundRemovalProvider> providers,
        IProviderConfigurationRepository configRepository,
        IConfiguration configuration,
        ILogger<BackgroundRemovalProviderFactory> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
        
        // Cargar configuración
        _settings = new BackgroundRemovalSettings();
        configuration.GetSection(BackgroundRemovalSettings.SectionName).Bind(_settings);
        
        _logger.LogInformation("Default provider configured: {DefaultProvider}", _settings.DefaultProvider);
        
        // Registrar todos los proveedores inyectados
        foreach (var provider in providers)
        {
            _providers[provider.ProviderType] = provider;
            _logger.LogInformation("Registered background removal provider: {Provider} (Type: {Type})", 
                provider.ProviderName, provider.ProviderType);
        }
    }
    
    /// <summary>
    /// Obtiene el proveedor por defecto según configuración (ClipDrop por defecto)
    /// </summary>
    public IBackgroundRemovalProvider? GetDefaultProvider()
    {
        var defaultProviderName = _settings.DefaultProvider;
        
        // Mapear nombre de configuración a enum
        if (Enum.TryParse<BackgroundRemovalProvider>(defaultProviderName, ignoreCase: true, out var providerType))
        {
            var provider = GetProvider(providerType);
            if (provider != null)
            {
                _logger.LogDebug("Using configured default provider: {Provider}", provider.ProviderName);
                return provider;
            }
        }
        
        // Fallback a ClipDrop si el configurado no está disponible
        if (_providers.TryGetValue(BackgroundRemovalProvider.ClipDrop, out var clipDropProvider))
        {
            _logger.LogWarning("Configured default provider '{ConfiguredProvider}' not found, falling back to ClipDrop", 
                defaultProviderName);
            return clipDropProvider;
        }
        
        // Último recurso: primer proveedor disponible
        var firstProvider = _providers.Values.FirstOrDefault();
        if (firstProvider != null)
        {
            _logger.LogWarning("ClipDrop not available, using first available provider: {Provider}", 
                firstProvider.ProviderName);
        }
        
        return firstProvider;
    }

    public IBackgroundRemovalProvider? GetProvider(BackgroundRemovalProvider providerType)
    {
        return _providers.GetValueOrDefault(providerType);
    }

    public async Task<IBackgroundRemovalProvider?> GetBestAvailableProviderAsync(
        CancellationToken cancellationToken = default)
    {
        // Primero intentar el proveedor por defecto de la configuración
        var defaultProvider = GetDefaultProvider();
        if (defaultProvider != null && await defaultProvider.IsAvailableAsync(cancellationToken))
        {
            _logger.LogDebug("Using default provider: {Provider}", defaultProvider.ProviderName);
            return defaultProvider;
        }
        
        // Si fallback está habilitado, intentar proveedores de fallback
        if (_settings.EnableFallback)
        {
            foreach (var fallbackName in _settings.FallbackProviders)
            {
                if (Enum.TryParse<BackgroundRemovalProvider>(fallbackName, ignoreCase: true, out var providerType))
                {
                    var fallbackProvider = GetProvider(providerType);
                    if (fallbackProvider != null && await fallbackProvider.IsAvailableAsync(cancellationToken))
                    {
                        _logger.LogInformation("Using fallback provider: {Provider}", fallbackProvider.ProviderName);
                        return fallbackProvider;
                    }
                }
            }
        }
        
        // Obtener la mejor configuración disponible de la base de datos
        var config = await _configRepository.GetBestAvailableProviderAsync(cancellationToken);
        
        if (config == null)
        {
            _logger.LogWarning("No available provider configuration found in database");
            
            // Último fallback: buscar el primer proveedor disponible ordenado por tipo (ClipDrop = 0)
            foreach (var provider in _providers.Values.OrderBy(p => (int)p.ProviderType))
            {
                if (await provider.IsAvailableAsync(cancellationToken))
                {
                    _logger.LogInformation("Using first available provider: {Provider}", provider.ProviderName);
                    return provider;
                }
            }
            
            return null;
        }
        
        return GetProvider(config.Provider);
    }

    public IEnumerable<IBackgroundRemovalProvider> GetAllProviders()
    {
        return _providers.Values;
    }

    public async Task<IEnumerable<IBackgroundRemovalProvider>> GetAvailableProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        var availableProviders = new List<IBackgroundRemovalProvider>();
        
        foreach (var provider in _providers.Values)
        {
            try
            {
                if (await provider.IsAvailableAsync(cancellationToken))
                {
                    availableProviders.Add(provider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking availability for provider {Provider}", 
                    provider.ProviderName);
            }
        }
        
        // Ordenar por prioridad (ClipDrop = 0 primero)
        return availableProviders.OrderBy(p => (int)p.ProviderType);
    }

    public void RegisterProvider(IBackgroundRemovalProvider provider)
    {
        _providers[provider.ProviderType] = provider;
        _logger.LogInformation("Dynamically registered provider: {Provider}", provider.ProviderName);
    }
    
    /// <summary>
    /// Obtiene los proveedores de fallback configurados
    /// </summary>
    public IEnumerable<string> GetFallbackProviders() => _settings.FallbackProviders;
    
    /// <summary>
    /// Indica si el fallback está habilitado
    /// </summary>
    public bool IsFallbackEnabled => _settings.EnableFallback;
}
