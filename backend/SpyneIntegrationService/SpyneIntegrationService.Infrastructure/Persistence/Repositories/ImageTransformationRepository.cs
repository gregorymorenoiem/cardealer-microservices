using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Infrastructure.Persistence.Repositories;

public class ImageTransformationRepository : IImageTransformationRepository
{
    private readonly SpyneDbContext _context;

    public ImageTransformationRepository(SpyneDbContext context)
    {
        _context = context;
    }

    public async Task<ImageTransformation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ImageTransformation?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpyneJobId == spyneJobId, cancellationToken);
    }

    public async Task<List<ImageTransformation>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ImageTransformation>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .Where(x => x.DealerId == dealerId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ImageTransformation>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .Where(x => x.Status == TransformationStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ImageTransformation>> GetFailedForRetryAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .Where(x => x.Status == TransformationStatus.Failed && x.RetryCount < 3)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByDealerAsync(Guid dealerId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.ImageTransformations
            .AsNoTracking()
            .Where(x => x.DealerId == dealerId && x.CreatedAt >= since)
            .CountAsync(cancellationToken);
    }

    public async Task<ImageTransformation> AddAsync(ImageTransformation transformation, CancellationToken cancellationToken = default)
    {
        await _context.ImageTransformations.AddAsync(transformation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return transformation;
    }

    public async Task<ImageTransformation> UpdateAsync(ImageTransformation transformation, CancellationToken cancellationToken = default)
    {
        _context.ImageTransformations.Update(transformation);
        await _context.SaveChangesAsync(cancellationToken);
        return transformation;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ImageTransformations.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.ImageTransformations.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
