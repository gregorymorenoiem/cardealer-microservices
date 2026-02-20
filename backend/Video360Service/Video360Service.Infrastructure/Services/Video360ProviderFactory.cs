using Microsoft.Extensions.Logging;
using Video360Service.Application.Interfaces;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Services;

/// <summary>
/// Factory para crear y obtener proveedores de extracción de frames
/// </summary>
public class Video360ProviderFactory : IVideo360ProviderFactory
{
    private readonly IEnumerable<IVideo360Provider> _providers;
    private readonly IProviderConfigurationRepository _configRepository;
    private readonly ILogger<Video360ProviderFactory> _logger;

    public Video360ProviderFactory(
        IEnumerable<IVideo360Provider> providers,
        IProviderConfigurationRepository configRepository,
        ILogger<Video360ProviderFactory> logger)
    {
        _providers = providers;
        _configRepository = configRepository;
        _logger = logger;
    }

    public IVideo360Provider? GetProvider(Video360Provider providerType)
    {
        return _providers.FirstOrDefault(p => p.ProviderType == providerType);
    }

    public async Task<IVideo360Provider?> GetBestAvailableProviderAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Finding best available provider...");
        
        // Obtener configuración de prioridades
        var configs = await _configRepository.GetEnabledAsync(cancellationToken);
        var configDict = configs.ToDictionary(c => c.Provider);
        
        // Ordenar proveedores por prioridad y costo
        var orderedProviders = _providers
            .Where(p => !configDict.ContainsKey(p.ProviderType) || configDict[p.ProviderType].IsEnabled)
            .OrderByDescending(p => configDict.TryGetValue(p.ProviderType, out var config) ? config.Priority : 0)
            .ThenBy(p => p.CostPerVideoUsd);
        
        foreach (var provider in orderedProviders)
        {
            try
            {
                var isAvailable = await provider.IsAvailableAsync(cancellationToken);
                
                if (isAvailable)
                {
                    // Verificar límite diario
                    if (configDict.TryGetValue(provider.ProviderType, out var config))
                    {
                        if (!config.CanProcessToday())
                        {
                            _logger.LogDebug("Provider {Provider} has reached daily limit", provider.ProviderName);
                            continue;
                        }
                    }
                    
                    _logger.LogInformation("Selected provider: {Provider} (cost: ${Cost}/video)", 
                        provider.ProviderName, provider.CostPerVideoUsd);
                    
                    return provider;
                }
                
                _logger.LogDebug("Provider {Provider} is not available", provider.ProviderName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking availability of provider {Provider}", provider.ProviderName);
            }
        }
        
        _logger.LogWarning("No providers available!");
        return null;
    }

    public IEnumerable<IVideo360Provider> GetAllProviders()
    {
        return _providers;
    }

    public async Task<IEnumerable<IVideo360Provider>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default)
    {
        var available = new List<IVideo360Provider>();
        
        foreach (var provider in _providers)
        {
            try
            {
                var isAvailable = await provider.IsAvailableAsync(cancellationToken);
                if (isAvailable)
                {
                    available.Add(provider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking availability of provider {Provider}", provider.ProviderName);
            }
        }
        
        return available;
    }
}
