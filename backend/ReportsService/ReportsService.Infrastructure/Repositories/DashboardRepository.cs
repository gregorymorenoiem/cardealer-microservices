using Microsoft.EntityFrameworkCore;
using ReportsService.Domain.Entities;
using ReportsService.Domain.Interfaces;
using ReportsService.Infrastructure.Persistence;

namespace ReportsService.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ReportsDbContext _context;

    public DashboardRepository(ReportsDbContext context)
    {
        _context = context;
    }

    public async Task<Dashboard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Dashboards.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Dashboard?> GetByIdWithWidgetsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Dashboards
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Dashboard>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dashboards.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Dashboard>> GetByTypeAsync(DashboardType type, CancellationToken cancellationToken = default)
    {
        return await _context.Dashboards.Where(d => d.Type == type).ToListAsync(cancellationToken);
    }

    public async Task<Dashboard?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Dashboards
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.IsDefault, cancellationToken);
    }

    public async Task<Dashboard> AddAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        await _context.Dashboards.AddAsync(dashboard, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return dashboard;
    }

    public async Task UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        _context.Dashboards.Update(dashboard);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetByIdAsync(id, cancellationToken);
        if (dashboard != null)
        {
            _context.Dashboards.Remove(dashboard);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Dashboards.AnyAsync(d => d.Id == id, cancellationToken);
    }
}
