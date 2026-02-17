using LeadScoringService.Domain.Entities;

namespace LeadScoringService.Domain.Interfaces;

/// <summary>
/// Repositorio para acciones de leads
/// </summary>
public interface ILeadActionRepository
{
    Task<LeadAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LeadAction>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task<LeadAction> CreateAsync(LeadAction action, CancellationToken cancellationToken = default);
    Task<List<LeadAction>> GetRecentActionsByLeadAsync(Guid leadId, int count, CancellationToken cancellationToken = default);
    Task<List<LeadAction>> GetActionsByTypeAsync(Guid leadId, LeadActionType actionType, CancellationToken cancellationToken = default);
    Task<int> GetActionCountAsync(Guid leadId, LeadActionType? actionType = null, CancellationToken cancellationToken = default);
}
