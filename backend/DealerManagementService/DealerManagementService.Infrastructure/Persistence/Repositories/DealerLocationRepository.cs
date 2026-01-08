using DealerManagementService.Domain.Entities;
using DealerManagementService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DealerManagementService.Infrastructure.Persistence.Repositories;

public class DealerLocationRepository : IDealerLocationRepository
{
    private readonly DealerDbContext _context;

    public DealerLocationRepository(DealerDbContext context)
    {
        _context = context;
    }

    public async Task<DealerLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DealerLocations.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<DealerLocation>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.DealerLocations
            .Where(l => l.DealerId == dealerId && l.IsActive)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DealerLocation?> GetPrimaryLocationAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.DealerLocations
            .FirstOrDefaultAsync(l => l.DealerId == dealerId && l.IsPrimary && l.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<DealerLocation>> GetByProvinceAsync(string province, CancellationToken cancellationToken = default)
    {
        return await _context.DealerLocations
            .Where(l => l.Province == province && l.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<DealerLocation> AddAsync(DealerLocation location, CancellationToken cancellationToken = default)
    {
        await _context.DealerLocations.AddAsync(location, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return location;
    }

    public async Task UpdateAsync(DealerLocation location, CancellationToken cancellationToken = default)
    {
        _context.DealerLocations.Update(location);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await _context.DealerLocations.FindAsync(new object[] { id }, cancellationToken);
        if (location != null)
        {
            location.IsDeleted = true;
            location.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
