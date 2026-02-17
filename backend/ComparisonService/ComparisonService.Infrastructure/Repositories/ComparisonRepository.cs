using Microsoft.EntityFrameworkCore;
using ComparisonService.Domain.Entities;
using ComparisonService.Domain.Interfaces;
using ComparisonService.Infrastructure.Persistence;

namespace ComparisonService.Infrastructure.Repositories;

public class ComparisonRepository : IComparisonRepository
{
    private readonly ApplicationDbContext _context;

    public ComparisonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleComparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleComparisons
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<VehicleComparison?> GetByShareTokenAsync(string shareToken, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleComparisons
            .FirstOrDefaultAsync(c => c.ShareToken == shareToken && c.IsPublic, cancellationToken);
    }

    public async Task<IEnumerable<VehicleComparison>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleComparisons
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleComparison> CreateAsync(VehicleComparison comparison, CancellationToken cancellationToken = default)
    {
        await _context.VehicleComparisons.AddAsync(comparison, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return comparison;
    }

    public async Task UpdateAsync(VehicleComparison comparison, CancellationToken cancellationToken = default)
    {
        _context.VehicleComparisons.Update(comparison);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comparison = await GetByIdAsync(id, cancellationToken);
        if (comparison != null)
        {
            _context.VehicleComparisons.Remove(comparison);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
