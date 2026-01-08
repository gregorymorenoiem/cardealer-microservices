using Microsoft.EntityFrameworkCore;
using MaintenanceService.Domain.Entities;
using MaintenanceService.Domain.Interfaces;
using MaintenanceService.Infrastructure.Persistence;

namespace MaintenanceService.Infrastructure.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly ApplicationDbContext _context;

    public MaintenanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceWindow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceWindows
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceWindow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceWindows
            .OrderByDescending(m => m.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceWindow>> GetUpcomingAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        return await _context.MaintenanceWindows
            .Where(m => m.Status == MaintenanceStatus.Scheduled &&
                       m.ScheduledStart >= DateTime.UtcNow &&
                       m.ScheduledStart <= endDate)
            .OrderBy(m => m.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceWindow?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceWindows
            .FirstOrDefaultAsync(m => m.Status == MaintenanceStatus.InProgress, cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceWindow>> GetByStatusAsync(
        MaintenanceStatus status, 
        CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceWindows
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceWindow> CreateAsync(
        MaintenanceWindow window, 
        CancellationToken cancellationToken = default)
    {
        await _context.MaintenanceWindows.AddAsync(window, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return window;
    }

    public async Task UpdateAsync(MaintenanceWindow window, CancellationToken cancellationToken = default)
    {
        _context.MaintenanceWindows.Update(window);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var window = await GetByIdAsync(id, cancellationToken);
        if (window != null)
        {
            _context.MaintenanceWindows.Remove(window);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsMaintenanceModeActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceWindows
            .AnyAsync(m => m.Status == MaintenanceStatus.InProgress, cancellationToken);
    }
}
