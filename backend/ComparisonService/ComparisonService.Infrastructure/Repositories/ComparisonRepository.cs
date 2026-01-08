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

    public async Task<Comparison?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Comparison?> GetByShareTokenAsync(string shareToken, CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .FirstOrDefaultAsync(c => c.ShareToken == shareToken && c.IsPublic, cancellationToken);
    }

    public async Task<IEnumerable<Comparison>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Comparisons
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Comparison> CreateAsync(Comparison comparison, CancellationToken cancellationToken = default)
    {
        await _context.Comparisons.AddAsync(comparison, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return comparison;
    }

    public async Task UpdateAsync(Comparison comparison, CancellationToken cancellationToken = default)
    {
        _context.Comparisons.Update(comparison);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comparison = await GetByIdAsync(id, cancellationToken);
        if (comparison != null)
        {
            _context.Comparisons.Remove(comparison);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
