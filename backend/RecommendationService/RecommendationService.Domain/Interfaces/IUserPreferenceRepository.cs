using System;
using System.Threading.Tasks;
using RecommendationService.Domain.Entities;

namespace RecommendationService.Domain.Interfaces;

public interface IUserPreferenceRepository
{
    Task<UserPreference?> GetByUserIdAsync(Guid userId);
    Task<UserPreference> CreateAsync(UserPreference preference);
    Task<UserPreference> UpdateAsync(UserPreference preference);
    Task<bool> DeleteAsync(Guid id);
}
