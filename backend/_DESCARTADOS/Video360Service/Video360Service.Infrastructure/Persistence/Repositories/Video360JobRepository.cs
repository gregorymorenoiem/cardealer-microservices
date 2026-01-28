using Microsoft.EntityFrameworkCore;
using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Persistence.Repositories;

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
            .Include(j => j.ExtractedFrames.OrderBy(f => f.SequenceNumber))
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Include(j => j.ExtractedFrames.OrderBy(f => f.SequenceNumber))
            .Where(j => j.VehicleId == vehicleId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetByStatusAsync(Video360JobStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Where(j => j.Status == status)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Video360Job>> GetPendingJobsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .Where(j => j.Status == Video360JobStatus.Queued || j.Status == Video360JobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Video360Job> CreateAsync(Video360Job job, CancellationToken cancellationToken = default)
    {
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

    public async Task<int> GetQueuePositionAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _context.Video360Jobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job == null || job.Status != Video360JobStatus.Queued)
            return 0;

        return await _context.Video360Jobs
            .CountAsync(j => 
                (j.Status == Video360JobStatus.Queued || j.Status == Video360JobStatus.Pending) 
                && j.CreatedAt < job.CreatedAt, 
                cancellationToken) + 1;
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Video360Jobs
            .CountAsync(j => 
                j.Status == Video360JobStatus.Queued 
                || j.Status == Video360JobStatus.Pending 
                || j.Status == Video360JobStatus.Processing,
                cancellationToken);
    }
}
