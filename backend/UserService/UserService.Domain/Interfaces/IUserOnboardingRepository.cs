using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

public interface IUserOnboardingRepository
{
    Task<UserOnboarding?> GetByIdAsync(Guid id);
    Task<UserOnboarding?> GetByUserIdAsync(Guid userId);
    Task<List<UserOnboarding>> GetIncompleteAsync();
    Task CreateAsync(UserOnboarding onboarding);
    Task UpdateAsync(UserOnboarding onboarding);
    Task<bool> ExistsForUserAsync(Guid userId);
    Task<int> GetCompletionRateAsync(); // Porcentaje de usuarios que completaron
}
