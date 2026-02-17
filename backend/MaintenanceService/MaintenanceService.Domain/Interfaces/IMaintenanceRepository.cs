using MaintenanceService.Domain.Entities;

namespace MaintenanceService.Domain.Interfaces;

public interface IMaintenanceRepository
{
    Task<MaintenanceWindow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceWindow>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceWindow>> GetUpcomingAsync(int days = 7, CancellationToken cancellationToken = default);
    Task<MaintenanceWindow?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceWindow>> GetByStatusAsync(MaintenanceStatus status, CancellationToken cancellationToken = default);
    Task<MaintenanceWindow> CreateAsync(MaintenanceWindow window, CancellationToken cancellationToken = default);
    Task UpdateAsync(MaintenanceWindow window, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsMaintenanceModeActiveAsync(CancellationToken cancellationToken = default);
}
