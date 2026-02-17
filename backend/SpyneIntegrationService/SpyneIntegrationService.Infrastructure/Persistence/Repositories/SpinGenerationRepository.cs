using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Infrastructure.Persistence.Repositories;

public class SpinGenerationRepository : ISpinGenerationRepository
{
    private readonly SpyneDbContext _context;

    public SpinGenerationRepository(SpyneDbContext context)
    {
        _context = context;
    }

    public async Task<SpinGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SpinGenerations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SpinGeneration?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default)
    {
        return await _context.SpinGenerations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpyneJobId == spyneJobId, cancellationToken);
    }

    public async Task<SpinGeneration?> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.SpinGenerations
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.Status == TransformationStatus.Completed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SpinGeneration>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.SpinGenerations
            .AsNoTracking()
            .Where(x => x.DealerId == dealerId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SpinGeneration>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.SpinGenerations
            .AsNoTracking()
            .Where(x => x.Status == TransformationStatus.Pending || x.Status == TransformationStatus.Processing)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<SpinGeneration> AddAsync(SpinGeneration spin, CancellationToken cancellationToken = default)
    {
        await _context.SpinGenerations.AddAsync(spin, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return spin;
    }

    public async Task<SpinGeneration> UpdateAsync(SpinGeneration spin, CancellationToken cancellationToken = default)
    {
        _context.SpinGenerations.Update(spin);
        await _context.SaveChangesAsync(cancellationToken);
        return spin;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SpinGenerations.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.SpinGenerations.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
