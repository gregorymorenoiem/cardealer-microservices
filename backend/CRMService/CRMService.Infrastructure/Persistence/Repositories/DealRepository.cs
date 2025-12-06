using CRMService.Domain.Entities;
using CRMService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.Persistence.Repositories;

public class DealRepository : IDealRepository
{
    private readonly CRMDbContext _context;

    public DealRepository(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<Deal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Deals.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Deal?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Include(d => d.Pipeline)
            .Include(d => d.Stage)
            .Include(d => d.Lead)
            .Include(d => d.Activities.OrderByDescending(a => a.CreatedAt).Take(10))
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Deal>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Include(d => d.Pipeline)
            .Include(d => d.Stage)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Deal>> GetByPipelineAsync(Guid pipelineId, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Include(d => d.Stage)
            .Where(d => d.PipelineId == pipelineId)
            .OrderBy(d => d.StageOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Deal>> GetByStageAsync(Guid stageId, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Where(d => d.StageId == stageId)
            .OrderBy(d => d.StageOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Include(d => d.Pipeline)
            .Include(d => d.Stage)
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Deal>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Include(d => d.Pipeline)
            .Include(d => d.Stage)
            .Where(d => d.AssignedToUserId == userId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Deal> AddAsync(Deal deal, CancellationToken cancellationToken = default)
    {
        await _context.Deals.AddAsync(deal, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return deal;
    }

    public async Task UpdateAsync(Deal deal, CancellationToken cancellationToken = default)
    {
        _context.Deals.Update(deal);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deal = await _context.Deals.FindAsync(new object[] { id }, cancellationToken);
        if (deal != null)
        {
            _context.Deals.Remove(deal);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<decimal> GetTotalValueByStatusAsync(DealStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .Where(d => d.Status == status)
            .SumAsync(d => d.Value, cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(DealStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Deals
            .CountAsync(d => d.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<Deal>> GetClosingSoonAsync(int days, CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.UtcNow.AddDays(days);
        return await _context.Deals
            .Include(d => d.Pipeline)
            .Include(d => d.Stage)
            .Where(d => d.Status == DealStatus.Open &&
                        d.ExpectedCloseDate.HasValue &&
                        d.ExpectedCloseDate.Value <= targetDate)
            .OrderBy(d => d.ExpectedCloseDate)
            .ToListAsync(cancellationToken);
    }
}
