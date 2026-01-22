using SpyneIntegrationService.Domain.Entities;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// Repository interface for 360° spin generations
/// </summary>
public interface ISpinGenerationRepository
{
    Task<SpinGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SpinGeneration?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default);
    Task<SpinGeneration?> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<List<SpinGeneration>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<SpinGeneration>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<SpinGeneration> AddAsync(SpinGeneration spin, CancellationToken cancellationToken = default);
    Task<SpinGeneration> UpdateAsync(SpinGeneration spin, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
