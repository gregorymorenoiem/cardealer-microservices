using MarketingService.Domain.Entities;

namespace MarketingService.Domain.Interfaces;

public interface IAudienceRepository
{
    Task<Audience?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Audience?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Audience>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Audience>> GetByTypeAsync(AudienceType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Audience>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Audience> AddAsync(Audience audience, CancellationToken cancellationToken = default);
    Task UpdateAsync(Audience audience, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetMemberCountAsync(Guid audienceId, CancellationToken cancellationToken = default);
    Task AddMemberAsync(AudienceMember member, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
}
