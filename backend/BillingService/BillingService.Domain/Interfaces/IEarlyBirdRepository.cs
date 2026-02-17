using BillingService.Domain.Entities;

namespace BillingService.Domain.Interfaces;

public interface IEarlyBirdRepository
{
    Task<EarlyBirdMember?> GetByIdAsync(Guid id);
    Task<EarlyBirdMember?> GetByUserIdAsync(Guid userId);
    Task<List<EarlyBirdMember>> GetAllActiveAsync();
    Task<bool> IsUserEnrolledAsync(Guid userId);
    Task CreateAsync(EarlyBirdMember member);
    Task UpdateAsync(EarlyBirdMember member);
    Task<int> GetTotalEnrolledCountAsync();
}
