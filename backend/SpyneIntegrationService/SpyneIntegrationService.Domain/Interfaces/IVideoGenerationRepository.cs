using SpyneIntegrationService.Domain.Entities;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// Repository interface for video generations
/// </summary>
public interface IVideoGenerationRepository
{
    Task<VideoGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VideoGeneration?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default);
    Task<List<VideoGeneration>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<List<VideoGeneration>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<VideoGeneration>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<VideoGeneration> AddAsync(VideoGeneration video, CancellationToken cancellationToken = default);
    Task<VideoGeneration> UpdateAsync(VideoGeneration video, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
