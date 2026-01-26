using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;
using SpyneIntegrationService.Infrastructure.Persistence;

namespace SpyneIntegrationService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Video360Spin entities
/// </summary>
public class Video360SpinRepository : IVideo360SpinRepository
{
    private readonly SpyneDbContext _context;

    public Video360SpinRepository(SpyneDbContext context)
    {
        _context = context;
    }

    public async Task<Video360Spin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Video360Spin?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpyneJobId == spyneJobId, cancellationToken);
    }

    public async Task<List<Video360Spin>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Video360Spin>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .Where(x => x.DealerId == dealerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Video360Spin?> GetLatestByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Video360Spin>> GetPendingAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .Where(x => x.Status == TransformationStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Video360Spin>> GetProcessingAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AsNoTracking()
            .Where(x => x.Status == TransformationStatus.Processing || 
                        x.Status == TransformationStatus.ExtractingFrames ||
                        x.Status == TransformationStatus.Uploading)
            .OrderBy(x => x.StartedAt ?? x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Video360Spin> AddAsync(Video360Spin entity, CancellationToken cancellationToken = default)
    {
        await _context.Video360Spins.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Video360Spin entity, CancellationToken cancellationToken = default)
    {
        _context.Video360Spins.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Video360Spins.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Video360Spins.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Spins
            .AnyAsync(x => x.VehicleId == vehicleId && x.Status == TransformationStatus.Completed, cancellationToken);
    }
}
