using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackgroundRemovalService.Infrastructure.Persistence.Repositories;

public class ProviderConfigurationRepository : IProviderConfigurationRepository
{
    private readonly BackgroundRemovalDbContext _context;

    public ProviderConfigurationRepository(BackgroundRemovalDbContext context)
    {
        _context = context;
    }

    public async Task<ProviderConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<ProviderConfiguration?> GetByProviderAsync(
        BackgroundRemovalProvider provider, 
        CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .FirstOrDefaultAsync(p => p.Provider == provider, cancellationToken);
    }

    public async Task<IEnumerable<ProviderConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .OrderBy(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProviderConfiguration>> GetEnabledProvidersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProviderConfigurations
            .Where(p => p.IsEnabled)
            .OrderBy(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProviderConfiguration?> GetBestAvailableProviderAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.ProviderConfigurations
            .Where(p => p.IsEnabled)
            .Where(p => !p.IsCircuitBreakerOpen || p.CircuitBreakerResetAt <= now)
            .Where(p => p.RequestsUsedToday < p.RateLimitPerDay)
            .OrderBy(p => p.Priority)
            .ThenByDescending(p => p.SuccessRate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProviderConfiguration> CreateAsync(
        ProviderConfiguration config, 
        CancellationToken cancellationToken = default)
    {
        _context.ProviderConfigurations.Add(config);
        await _context.SaveChangesAsync(cancellationToken);
        return config;
    }

    public async Task<ProviderConfiguration> UpdateAsync(
        ProviderConfiguration config, 
        CancellationToken cancellationToken = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.ProviderConfigurations.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
        return config;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await GetByIdAsync(id, cancellationToken);
        if (config != null)
        {
            _context.ProviderConfigurations.Remove(config);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
