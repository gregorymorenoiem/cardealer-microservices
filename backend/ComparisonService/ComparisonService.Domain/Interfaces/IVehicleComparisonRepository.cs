using ComparisonService.Domain.Entities;

namespace ComparisonService.Domain.Interfaces;

public interface IVehicleComparisonRepository
{
    Task<List<VehicleComparison>> GetByUserIdAsync(Guid userId);
    Task<VehicleComparison?> GetByIdAsync(Guid id);
    Task<VehicleComparison?> GetByShareTokenAsync(string shareToken);
    Task<VehicleComparison> CreateAsync(VehicleComparison comparison);
    Task<VehicleComparison> UpdateAsync(VehicleComparison comparison);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetUserComparisonCountAsync(Guid userId);
    Task<List<VehicleComparison>> GetRecentByUserIdAsync(Guid userId, int limit = 10);
}