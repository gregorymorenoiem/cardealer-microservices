using Microsoft.EntityFrameworkCore;
using DataProtectionService.Domain.Entities;
using DataProtectionService.Domain.Interfaces;
using DataProtectionService.Infrastructure.Persistence;

namespace DataProtectionService.Infrastructure.Repositories;

public class ARCORequestRepository : IARCORequestRepository
{
    private readonly DataProtectionDbContext _context;

    public ARCORequestRepository(DataProtectionDbContext context)
    {
        _context = context;
    }

    public async Task<ARCORequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ARCORequests
            .Include(r => r.Attachments)
            .Include(r => r.StatusHistory)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ARCORequest?> GetByRequestNumberAsync(string requestNumber, CancellationToken cancellationToken = default)
    {
        return await _context.ARCORequests
            .Include(r => r.Attachments)
            .Include(r => r.StatusHistory)
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber, cancellationToken);
    }

    public async Task<List<ARCORequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ARCORequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<ARCORequest> Items, int TotalCount)> GetPendingRequestsAsync(
        int page, int pageSize, bool overdueOnly = false, CancellationToken cancellationToken = default)
    {
        // Pending statuses based on actual ARCOStatus enum values
        var pendingStatuses = new List<ARCOStatus> { ARCOStatus.Received, ARCOStatus.IdentityVerification, ARCOStatus.InProgress, ARCOStatus.PendingInformation };

        var query = _context.ARCORequests
            .Where(r => pendingStatuses.Contains(r.Status));

        if (overdueOnly)
            query = query.Where(r => r.Deadline < DateTime.UtcNow);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.Deadline)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<ARCORequest>> GetOverdueRequestsAsync(CancellationToken cancellationToken = default)
    {
        var pendingStatuses = new List<ARCOStatus> { ARCOStatus.Received, ARCOStatus.IdentityVerification, ARCOStatus.InProgress, ARCOStatus.PendingInformation };

        return await _context.ARCORequests
            .Where(r => pendingStatuses.Contains(r.Status) && r.Deadline < DateTime.UtcNow)
            .OrderBy(r => r.Deadline)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ARCORequest request, CancellationToken cancellationToken = default)
    {
        await _context.ARCORequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ARCORequest request, CancellationToken cancellationToken = default)
    {
        _context.ARCORequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ARCOStatisticsResult> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var query = _context.ARCORequests.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(r => r.RequestedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(r => r.RequestedAt <= toDate.Value);

        var allRequests = await query.ToListAsync(cancellationToken);

        var pendingStatuses = new List<ARCOStatus> { ARCOStatus.Received, ARCOStatus.IdentityVerification, ARCOStatus.InProgress, ARCOStatus.PendingInformation };

        var stats = new ARCOStatisticsResult
        {
            TotalRequests = allRequests.Count,
            PendingRequests = allRequests.Count(r => pendingStatuses.Contains(r.Status)),
            CompletedRequests = allRequests.Count(r => r.Status == ARCOStatus.Completed),
            RejectedRequests = allRequests.Count(r => r.Status == ARCOStatus.Rejected),
            OverdueRequests = allRequests.Count(r => pendingStatuses.Contains(r.Status) && r.Deadline < DateTime.UtcNow),
            RequestsByType = allRequests.GroupBy(r => r.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            RequestsByStatus = allRequests.GroupBy(r => r.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // Calculate average processing days for completed requests
        var completedWithDates = allRequests
            .Where(r => r.CompletedAt.HasValue)
            .Select(r => (r.CompletedAt!.Value - r.RequestedAt).TotalDays)
            .ToList();

        stats.AverageProcessingDays = completedWithDates.Any() ? completedWithDates.Average() : 0;
        stats.ComplianceRate = CalculateComplianceRate(allRequests);

        return stats;
    }

    private static double CalculateComplianceRate(List<ARCORequest> requests)
    {
        var completedRequests = requests.Where(r => r.CompletedAt.HasValue).ToList();
        if (!completedRequests.Any()) return 100;

        var onTimeCount = completedRequests.Count(r => r.CompletedAt!.Value <= r.Deadline);
        return (double)onTimeCount / completedRequests.Count * 100;
    }
}
