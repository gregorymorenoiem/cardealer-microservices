using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence.Repositories;

public class CategoryConfigRepository : ICategoryConfigRepository
{
    private readonly AdvertisingDbContext _context;

    public CategoryConfigRepository(AdvertisingDbContext context) => _context = context;

    public async Task<List<CategoryImageConfig>> GetAllVisibleAsync(CancellationToken ct = default)
        => await _context.CategoryImageConfigs
            .Where(c => c.IsVisible)
            .OrderBy(c => c.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<CategoryImageConfig>> GetAllAsync(CancellationToken ct = default)
        => await _context.CategoryImageConfigs
            .OrderBy(c => c.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<CategoryImageConfig?> GetByKeyAsync(string categoryKey, CancellationToken ct = default)
        => await _context.CategoryImageConfigs.FirstOrDefaultAsync(c => c.CategoryKey == categoryKey, ct);

    public async Task UpdateAsync(CategoryImageConfig config, CancellationToken ct = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.CategoryImageConfigs.Update(config);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddAsync(CategoryImageConfig config, CancellationToken ct = default)
    {
        await _context.CategoryImageConfigs.AddAsync(config, ct);
        await _context.SaveChangesAsync(ct);
    }
}
