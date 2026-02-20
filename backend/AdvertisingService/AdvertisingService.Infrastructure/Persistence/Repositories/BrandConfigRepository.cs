using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence.Repositories;

public class BrandConfigRepository : IBrandConfigRepository
{
    private readonly AdvertisingDbContext _context;

    public BrandConfigRepository(AdvertisingDbContext context) => _context = context;

    public async Task<List<BrandConfig>> GetAllVisibleAsync(CancellationToken ct = default)
        => await _context.BrandConfigs
            .Where(b => b.IsVisible)
            .OrderBy(b => b.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<BrandConfig>> GetAllAsync(CancellationToken ct = default)
        => await _context.BrandConfigs
            .OrderBy(b => b.DisplayOrder)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<BrandConfig?> GetByKeyAsync(string brandKey, CancellationToken ct = default)
        => await _context.BrandConfigs.FirstOrDefaultAsync(b => b.BrandKey == brandKey, ct);

    public async Task UpdateAsync(BrandConfig config, CancellationToken ct = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.BrandConfigs.Update(config);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddAsync(BrandConfig config, CancellationToken ct = default)
    {
        await _context.BrandConfigs.AddAsync(config, ct);
        await _context.SaveChangesAsync(ct);
    }
}
