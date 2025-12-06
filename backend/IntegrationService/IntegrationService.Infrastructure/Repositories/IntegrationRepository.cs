using Microsoft.EntityFrameworkCore;
using IntegrationService.Domain.Entities;
using IntegrationService.Domain.Interfaces;
using IntegrationService.Infrastructure.Persistence;

namespace IntegrationService.Infrastructure.Repositories;

public class IntegrationRepository : IIntegrationRepository
{
    private readonly IntegrationDbContext _context;

    public IntegrationRepository(IntegrationDbContext context)
    {
        _context = context;
    }

    public async Task<Integration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetByTypeAsync(IntegrationType type, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.Where(i => i.Type == type).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetByStatusAsync(IntegrationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.Where(i => i.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.Where(i => i.Status == IntegrationStatus.Active).ToListAsync(cancellationToken);
    }

    public async Task<Integration> AddAsync(Integration integration, CancellationToken cancellationToken = default)
    {
        await _context.Integrations.AddAsync(integration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return integration;
    }

    public async Task UpdateAsync(Integration integration, CancellationToken cancellationToken = default)
    {
        _context.Integrations.Update(integration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var integration = await GetByIdAsync(id, cancellationToken);
        if (integration != null)
        {
            _context.Integrations.Remove(integration);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Integrations.AnyAsync(i => i.Id == id, cancellationToken);
    }
}
