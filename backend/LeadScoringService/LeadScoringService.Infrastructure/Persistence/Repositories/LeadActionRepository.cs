using LeadScoringService.Domain.Entities;
using LeadScoringService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LeadScoringService.Infrastructure.Persistence.Repositories;

public class LeadActionRepository : ILeadActionRepository
{
    private readonly LeadScoringDbContext _context;

    public LeadActionRepository(LeadScoringDbContext context)
    {
        _context = context;
    }

    public async Task<LeadAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LeadActions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<LeadAction>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.LeadActions
            .Where(a => a.LeadId == leadId)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadAction> CreateAsync(LeadAction action, CancellationToken cancellationToken = default)
    {
        _context.LeadActions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);
        return action;
    }

    public async Task<List<LeadAction>> GetRecentActionsByLeadAsync(Guid leadId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.LeadActions
            .Where(a => a.LeadId == leadId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LeadAction>> GetActionsByTypeAsync(Guid leadId, LeadActionType actionType, CancellationToken cancellationToken = default)
    {
        return await _context.LeadActions
            .Where(a => a.LeadId == leadId && a.ActionType == actionType)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActionCountAsync(Guid leadId, LeadActionType? actionType = null, CancellationToken cancellationToken = default)
    {
        var query = _context.LeadActions.Where(a => a.LeadId == leadId);

        if (actionType.HasValue)
        {
            query = query.Where(a => a.ActionType == actionType.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
