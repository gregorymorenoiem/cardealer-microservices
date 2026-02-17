using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repository interface for dealer onboarding processes
/// </summary>
public interface IDealerOnboardingRepository
{
    Task<DealerOnboardingProcess?> GetByDealerIdAsync(Guid dealerId);
    Task<DealerOnboardingProcess> CreateAsync(DealerOnboardingProcess process);
    Task UpdateAsync(DealerOnboardingProcess process);
    Task<bool> ExistsAsync(Guid dealerId);
}
