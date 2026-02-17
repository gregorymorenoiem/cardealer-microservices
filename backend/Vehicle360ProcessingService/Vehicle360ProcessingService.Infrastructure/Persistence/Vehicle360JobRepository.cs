using Microsoft.EntityFrameworkCore;
using Vehicle360ProcessingService.Domain.Entities;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Infrastructure.Persistence;

public class Vehicle360JobRepository : IVehicle360JobRepository
{
    private readonly Vehicle360ProcessingDbContext _context;

    public Vehicle360JobRepository(Vehicle360ProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle360Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<Vehicle360Job> CreateAsync(Vehicle360Job job, CancellationToken cancellationToken)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<Vehicle360Job> UpdateAsync(Vehicle360Job job, CancellationToken cancellationToken)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs.FindAsync(new object[] { id }, cancellationToken);
        if (job == null) return false;

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<Vehicle360Job>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .Where(j => j.VehicleId == vehicleId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vehicle360Job?> GetLatestCompletedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .Where(j => j.VehicleId == vehicleId && j.Status == Vehicle360JobStatus.Completed)
            .OrderByDescending(j => j.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle360Job>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle360Job>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .Where(j => j.Status == Vehicle360JobStatus.Queued || 
                       j.Status == Vehicle360JobStatus.Pending ||
                       j.Status == Vehicle360JobStatus.VideoUploaded)
            .OrderBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle360Job>> GetByStatusAsync(Vehicle360JobStatus status, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .Where(j => j.Status == status)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetQueuePositionAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs.FindAsync(new object[] { jobId }, cancellationToken);
        if (job == null) return 0;

        return await _context.Jobs
            .CountAsync(j => 
                (j.Status == Vehicle360JobStatus.Queued || 
                 j.Status == Vehicle360JobStatus.Pending ||
                 j.Status == Vehicle360JobStatus.VideoUploaded) &&
                j.CreatedAt <= job.CreatedAt, 
                cancellationToken);
    }

    public async Task<int> GetTotalJobsCountAsync(CancellationToken cancellationToken)
    {
        return await _context.Jobs.CountAsync(cancellationToken);
    }

    public async Task<int> GetActiveJobsCountAsync(CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .CountAsync(j => 
                j.Status != Vehicle360JobStatus.Completed && 
                j.Status != Vehicle360JobStatus.Failed &&
                j.Status != Vehicle360JobStatus.Cancelled, 
                cancellationToken);
    }

    public async Task<Dictionary<Vehicle360JobStatus, int>> GetJobsCountByStatusAsync(CancellationToken cancellationToken)
    {
        var counts = await _context.Jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(c => c.Status, c => c.Count);
    }

    public async Task<IReadOnlyList<Vehicle360Job>> GetStuckJobsAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeout);
        
        return await _context.Jobs
            .Where(j => 
                j.Status == Vehicle360JobStatus.Processing ||
                j.Status == Vehicle360JobStatus.ExtractingFrames ||
                j.Status == Vehicle360JobStatus.RemovingBackgrounds ||
                j.Status == Vehicle360JobStatus.UploadingResults)
            .Where(j => j.StartedAt < cutoffTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveJobForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        return await _context.Jobs
            .AnyAsync(j => 
                j.VehicleId == vehicleId &&
                j.Status != Vehicle360JobStatus.Completed &&
                j.Status != Vehicle360JobStatus.Failed &&
                j.Status != Vehicle360JobStatus.Cancelled,
                cancellationToken);
    }
}
