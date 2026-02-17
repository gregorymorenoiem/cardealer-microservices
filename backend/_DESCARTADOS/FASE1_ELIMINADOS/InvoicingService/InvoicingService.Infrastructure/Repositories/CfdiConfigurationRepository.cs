using InvoicingService.Domain.Entities;
using InvoicingService.Domain.Interfaces;
using InvoicingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoicingService.Infrastructure.Repositories;

public class CfdiConfigurationRepository : ICfdiConfigurationRepository
{
    private readonly InvoicingDbContext _context;

    public CfdiConfigurationRepository(InvoicingDbContext context)
    {
        _context = context;
    }

    public async Task<CfdiConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CfdiConfigurations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CfdiConfiguration?> GetByDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.CfdiConfigurations
            .FirstOrDefaultAsync(c => c.DealerId == dealerId, cancellationToken);
    }

    public async Task<CfdiConfiguration?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CfdiConfigurations
            .FirstOrDefaultAsync(c => c.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsForDealerAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.CfdiConfigurations
            .AnyAsync(c => c.DealerId == dealerId, cancellationToken);
    }

    public async Task AddAsync(CfdiConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _context.CfdiConfigurations.AddAsync(configuration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CfdiConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _context.CfdiConfigurations.Update(configuration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await GetByIdAsync(id, cancellationToken);
        if (config != null)
        {
            _context.CfdiConfigurations.Remove(config);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
