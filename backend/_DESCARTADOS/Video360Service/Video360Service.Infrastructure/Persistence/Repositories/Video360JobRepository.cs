using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de Video360Job
/// </summary>
public class Video360JobRepository : IVideo360JobRepository
{
    private readonly Video360DbContext _context;

    public Video360JobRepository(Video360DbContext context)
    {
        _context = context;
    }

    public async Task<Video360Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<Video360Job?> GetByIdWithFramesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Include(j => j.ExtractedFrames)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Include(j => j.ExtractedFrames)
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Include(j => j.ExtractedFrames)
            .Where(j => j.VehicleId == vehicleId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetPendingJobsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Where(j => j.Status == ProcessingStatus.Pending || j.Status == ProcessingStatus.Retrying)
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetByStatusAsync(ProcessingStatus status, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Include(j => j.ExtractedFrames)
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Video360Job> CreateAsync(Video360Job job, CancellationToken cancellationToken = default)
    {
        job.CreatedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        
        _context.Video360Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        
        return job;
    }

    public async Task<Video360Job> UpdateAsync(Video360Job job, CancellationToken cancellationToken = default)
    {
        job.UpdatedAt = DateTime.UtcNow;
        
        _context.Video360Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
        
        return job;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _context.Video360Jobs.FindAsync(new object[] { id }, cancellationToken);
        if (job != null)
        {
            _context.Video360Jobs.Remove(job);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetTotalCountAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Video360Jobs.AsQueryable();
        
        if (userId.HasValue)
        {
            query = query.Where(j => j.UserId == userId);
        }
        
        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(ProcessingStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .CountAsync(j => j.Status == status, cancellationToken);
    }
}
