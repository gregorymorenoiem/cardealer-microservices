using CRMService.Domain.Entities;

namespace CRMService.Domain.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Lead> AddAsync(Lead lead, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lead lead, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(LeadStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lead>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}
