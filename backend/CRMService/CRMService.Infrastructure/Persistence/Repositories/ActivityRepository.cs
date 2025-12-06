using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.Persistence.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly CRMDbContext _context;

    public ActivityRepository(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Activities
            .Include(a => a.Lead)
            .Include(a => a.Deal)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Activities
            .Include(a => a.Lead)
            .Include(a => a.Deal)
            .OrderByDescending(a => a.DueDate ?? a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.Activities
            .Where(a => a.LeadId == leadId)
            .OrderByDescending(a => a.DueDate ?? a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetByDealAsync(Guid dealId, CancellationToken cancellationToken = default)
    {
        return await _context.Activities
            .Where(a => a.DealId == dealId)
            .OrderByDescending(a => a.DueDate ?? a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetUpcomingAsync(int days, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);

        return await _context.Activities
            .Include(a => a.Lead)
            .Include(a => a.Deal)
            .Where(a => a.DueDate != null &&
                        a.DueDate >= now &&
                        a.DueDate <= endDate &&
                        !a.IsCompleted)
            .OrderBy(a => a.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.Activities
            .Include(a => a.Lead)
            .Include(a => a.Deal)
            .Where(a => a.DueDate != null &&
                        a.DueDate < now &&
                        !a.IsCompleted)
            .OrderBy(a => a.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Activity>> GetByAssignedUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Activities
            .Include(a => a.Lead)
            .Include(a => a.Deal)
            .Where(a => a.AssignedToUserId == userId)
            .OrderByDescending(a => a.DueDate ?? a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Activity> AddAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        await _context.Activities.AddAsync(activity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return activity;
    }

    public async Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activity = await _context.Activities.FindAsync(new object[] { id }, cancellationToken);
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetPendingCountAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Activities.Where(a => !a.IsCompleted);

        if (userId.HasValue)
        {
            query = query.Where(a => a.AssignedToUserId == userId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}