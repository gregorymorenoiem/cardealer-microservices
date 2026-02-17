using Microsoft.EntityFrameworkCore;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Infrastructure.Persistence.Repositories;

public class VideoGenerationRepository : IVideoGenerationRepository
{
    private readonly SpyneDbContext _context;

    public VideoGenerationRepository(SpyneDbContext context)
    {
        _context = context;
    }

    public async Task<VideoGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VideoGenerations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<VideoGeneration?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default)
    {
        return await _context.VideoGenerations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpyneJobId == spyneJobId, cancellationToken);
    }

    public async Task<List<VideoGeneration>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.VideoGenerations
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VideoGeneration>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.VideoGenerations
            .AsNoTracking()
            .Where(x => x.DealerId == dealerId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VideoGeneration>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.VideoGenerations
            .AsNoTracking()
            .Where(x => x.Status == TransformationStatus.Pending || x.Status == TransformationStatus.Processing)
            .OrderBy(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<VideoGeneration> AddAsync(VideoGeneration video, CancellationToken cancellationToken = default)
    {
        await _context.VideoGenerations.AddAsync(video, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return video;
    }

    public async Task<VideoGeneration> UpdateAsync(VideoGeneration video, CancellationToken cancellationToken = default)
    {
        _context.VideoGenerations.Update(video);
        await _context.SaveChangesAsync(cancellationToken);
        return video;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.VideoGenerations.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.VideoGenerations.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
