using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de configuración de proveedores
/// </summary>
public class ProviderConfigurationRepository : IProviderConfigurationRepository
{
    private readonly Video360DbContext _context;

    public ProviderConfigurationRepository(Video360DbContext context)
    {
        _context = context;
    }

    public async Task<ProviderConfiguration?> GetByProviderAsync(Video360Provider provider, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Provider == provider, cancellationToken);
    }

    public async Task<IEnumerable<ProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProviderConfiguration>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .Where(p => p.IsEnabled)
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProviderConfiguration> CreateOrUpdateAsync(ProviderConfiguration config, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Provider == config.Provider, cancellationToken);
        
        if (existing != null)
        {
            existing.IsEnabled = config.IsEnabled;
            existing.Priority = config.Priority;
            existing.CostPerVideoUsd = config.CostPerVideoUsd;
            existing.DailyLimit = config.DailyLimit;
            existing.MaxVideoSizeMb = config.MaxVideoSizeMb;
            existing.MaxVideoDurationSeconds = config.MaxVideoDurationSeconds;
            existing.TimeoutSeconds = config.TimeoutSeconds;
            existing.SupportedFormats = config.SupportedFormats;
            existing.Notes = config.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            
            _context.ProviderConfigurations.Update(existing);
        }
        else
        {
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            _context.ProviderConfigurations.Add(config);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return existing ?? config;
    }

    public async Task UpdateAsync(ProviderConfiguration config, CancellationToken cancellationToken = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.ProviderConfigurations.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProviderConfiguration?> GetBestAvailableAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _context.ProviderConfigurations
            .Where(p => p.IsEnabled)
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
        
        foreach (var config in configs)
        {
            if (config.CanProcessToday())
            {
                return config;
            }
        }
        
        return null;
    }
}
