using AdvertisingService.Domain.Entities;
using AdvertisingService.Domain.Enums;
using AdvertisingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingService.Infrastructure.Persistence.Repositories;

public class RotationConfigRepository : IRotationConfigRepository
{
    private readonly AdvertisingDbContext _context;

    public RotationConfigRepository(AdvertisingDbContext context) => _context = context;

    public async Task<RotationConfig?> GetBySectionAsync(AdPlacementType section, CancellationToken ct = default)
        => await _context.RotationConfigs.FirstOrDefaultAsync(r => r.Section == section, ct);

    public async Task UpdateAsync(RotationConfig config, CancellationToken ct = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _context.RotationConfigs.Update(config);
        await _context.SaveChangesAsync(ct);
    }
}
