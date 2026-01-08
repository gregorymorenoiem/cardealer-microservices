using ComparisonService.Domain.Entities;
using ComparisonService.Domain.Interfaces;
using ComparisonService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComparisonService.Infrastructure.Repositories;

public class VehicleComparisonRepository : IVehicleComparisonRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleComparisonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleComparison>> GetByUserIdAsync(Guid userId)
    {
        return await _context.VehicleComparisons
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<VehicleComparison?> GetByIdAsync(Guid id)
    {
        return await _context.VehicleComparisons
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<VehicleComparison?> GetByShareTokenAsync(string shareToken)
    {
        return await _context.VehicleComparisons
            .FirstOrDefaultAsync(c => c.ShareToken == shareToken);
    }

    public async Task<VehicleComparison> CreateAsync(VehicleComparison comparison)
    {
        _context.VehicleComparisons.Add(comparison);
        await _context.SaveChangesAsync();
        return comparison;
    }

    public async Task<VehicleComparison> UpdateAsync(VehicleComparison comparison)
    {
        comparison.UpdatedAt = DateTime.UtcNow;
        _context.VehicleComparisons.Update(comparison);
        await _context.SaveChangesAsync();
        return comparison;
    }

    public async Task DeleteAsync(Guid id)
    {
        var comparison = await GetByIdAsync(id);
        if (comparison != null)
        {
            _context.VehicleComparisons.Remove(comparison);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.VehicleComparisons
            .AnyAsync(c => c.Id == id);
    }

    public async Task<int> GetUserComparisonCountAsync(Guid userId)
    {
        return await _context.VehicleComparisons
            .CountAsync(c => c.UserId == userId);
    }

    public async Task<List<VehicleComparison>> GetRecentByUserIdAsync(Guid userId, int limit = 10)
    {
        return await _context.VehicleComparisons
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}