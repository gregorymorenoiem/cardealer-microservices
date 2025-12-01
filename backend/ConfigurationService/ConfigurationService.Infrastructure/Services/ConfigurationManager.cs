using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using ConfigurationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationService.Infrastructure.Services;

public class ConfigurationManager : IConfigurationManager
{
    private readonly ConfigurationDbContext _context;

    public ConfigurationManager(ConfigurationDbContext context)
    {
        _context = context;
    }

    public async Task<ConfigurationItem?> GetConfigurationAsync(string key, string environment, string? tenantId = null)
    {
        return await _context.ConfigurationItems
            .Where(c => c.Key == key && c.Environment == environment &&
                       (tenantId == null || c.TenantId == tenantId) && c.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ConfigurationItem>> GetAllConfigurationsAsync(string environment, string? tenantId = null)
    {
        return await _context.ConfigurationItems
            .Where(c => c.Environment == environment &&
                       (tenantId == null || c.TenantId == tenantId) && c.IsActive)
            .ToListAsync();
    }

    public async Task<ConfigurationItem> CreateConfigurationAsync(ConfigurationItem item)
    {
        item.Id = Guid.NewGuid();
        item.CreatedAt = DateTime.UtcNow;
        item.Version = 1;

        await _context.ConfigurationItems.AddAsync(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<ConfigurationItem> UpdateConfigurationAsync(Guid id, string value, string updatedBy, string? changeReason = null)
    {
        var item = await _context.ConfigurationItems.FindAsync(id);
        if (item == null)
            throw new InvalidOperationException($"Configuration item with ID {id} not found");

        // Save history
        var history = new ConfigurationHistory
        {
            Id = Guid.NewGuid(),
            ConfigurationItemId = id,
            Key = item.Key,
            OldValue = item.Value,
            NewValue = value,
            Environment = item.Environment,
            ChangedBy = updatedBy,
            ChangedAt = DateTime.UtcNow,
            ChangeReason = changeReason
        };

        await _context.ConfigurationHistories.AddAsync(history);

        // Update item
        item.Value = value;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = updatedBy;
        item.Version++;

        await _context.SaveChangesAsync();

        return item;
    }

    public async Task DeleteConfigurationAsync(Guid id)
    {
        var item = await _context.ConfigurationItems.FindAsync(id);
        if (item != null)
        {
            item.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ConfigurationHistory>> GetConfigurationHistoryAsync(Guid configurationItemId)
    {
        return await _context.ConfigurationHistories
            .Where(h => h.ConfigurationItemId == configurationItemId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
}
