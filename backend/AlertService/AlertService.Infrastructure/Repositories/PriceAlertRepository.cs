using Microsoft.EntityFrameworkCore;
using AlertService.Domain.Entities;
using AlertService.Domain.Interfaces;
using AlertService.Infrastructure.Persistence;

namespace AlertService.Infrastructure.Repositories;

public class PriceAlertRepository : IPriceAlertRepository
{
    private readonly ApplicationDbContext _context;

    public PriceAlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PriceAlert?> GetByIdAsync(Guid id)
    {
        return await _context.PriceAlerts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<PriceAlert>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PriceAlerts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PriceAlert>> GetActiveAlertsAsync()
    {
        return await _context.PriceAlerts
            .Where(a => a.IsActive && !a.IsTriggered)
            .ToListAsync();
    }

    public async Task<List<PriceAlert>> GetActiveAlertsByVehicleIdAsync(Guid vehicleId)
    {
        return await _context.PriceAlerts
            .Where(a => a.VehicleId == vehicleId && a.IsActive && !a.IsTriggered)
            .ToListAsync();
    }

    public async Task CreateAsync(PriceAlert alert)
    {
        await _context.PriceAlerts.AddAsync(alert);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PriceAlert alert)
    {
        _context.PriceAlerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var alert = await GetByIdAsync(id);
        if (alert != null)
        {
            _context.PriceAlerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid vehicleId)
    {
        return await _context.PriceAlerts
            .AnyAsync(a => a.UserId == userId && a.VehicleId == vehicleId);
    }
}
