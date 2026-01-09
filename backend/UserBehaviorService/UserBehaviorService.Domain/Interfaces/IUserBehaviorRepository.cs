using UserBehaviorService.Domain.Entities;

namespace UserBehaviorService.Domain.Interfaces;

public interface IUserBehaviorRepository
{
    // UserBehaviorProfile
    Task<UserBehaviorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<List<UserBehaviorProfile>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<UserBehaviorProfile> CreateOrUpdateAsync(UserBehaviorProfile profile, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, CancellationToken ct = default);
    
    // User Actions
    Task<UserAction> AddActionAsync(UserAction action, CancellationToken ct = default);
    Task<List<UserAction>> GetUserActionsAsync(Guid userId, int limit = 50, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetActionCountsByTypeAsync(Guid userId, DateTime since, CancellationToken ct = default);
    
    // Segmentation
    Task<List<UserBehaviorProfile>> GetUsersBySegmentAsync(string segmentName, int page, int pageSize, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetSegmentDistributionAsync(CancellationToken ct = default);
    
    // Analytics
    Task<Dictionary<string, int>> GetTopPreferredMakesAsync(int top = 10, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetTopPreferredModelsAsync(int top = 10, CancellationToken ct = default);
    Task<(decimal Min, decimal Max)> GetAveragePriceRangeAsync(CancellationToken ct = default);
    Task<int> GetActiveUsersCountAsync(int days = 7, CancellationToken ct = default);
}
