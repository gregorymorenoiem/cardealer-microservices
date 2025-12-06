using CRMService.Domain.Entities;

namespace CRMService.Domain.Interfaces;

public interface IDealRepository
{
    Task<Deal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Deal?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deal>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Deal>> GetByPipelineAsync(Guid pipelineId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deal>> GetByStageAsync(Guid stageId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deal>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Deal> AddAsync(Deal deal, CancellationToken cancellationToken = default);
    Task UpdateAsync(Deal deal, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalValueByStatusAsync(DealStatus status, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(DealStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Deal>> GetClosingSoonAsync(int days, CancellationToken cancellationToken = default);
}
