using CRMService.Domain.Entities;

namespace CRMService.Domain.Interfaces;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetByLeadAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetByDealAsync(Guid dealId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetUpcomingAsync(int days, CancellationToken cancellationToken = default);
    Task<IEnumerable<Activity>> GetOverdueAsync(CancellationToken cancellationToken = default);
    Task<Activity> AddAsync(Activity activity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(Guid? userId = null, CancellationToken cancellationToken = default);
}
