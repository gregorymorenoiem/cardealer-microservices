using AlertService.Domain.Entities;

namespace AlertService.Domain.Interfaces;

public interface IPriceAlertRepository
{
    Task<PriceAlert?> GetByIdAsync(Guid id);
    Task<List<PriceAlert>> GetByUserIdAsync(Guid userId);
    Task<List<PriceAlert>> GetActiveAlertsAsync();
    Task<List<PriceAlert>> GetActiveAlertsByVehicleIdAsync(Guid vehicleId);
    Task CreateAsync(PriceAlert alert);
    Task UpdateAsync(PriceAlert alert);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid userId, Guid vehicleId);
}
