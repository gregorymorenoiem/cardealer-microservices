using SpyneIntegrationService.Domain.Entities;

namespace SpyneIntegrationService.Domain.Interfaces;

/// <summary>
/// Repository interface for image transformations
/// </summary>
public interface IImageTransformationRepository
{
    Task<ImageTransformation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImageTransformation?> GetBySpyneJobIdAsync(string spyneJobId, CancellationToken cancellationToken = default);
    Task<List<ImageTransformation>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<List<ImageTransformation>> GetByDealerIdAsync(Guid dealerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<ImageTransformation>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<List<ImageTransformation>> GetFailedForRetryAsync(int limit = 50, CancellationToken cancellationToken = default);
    Task<int> GetCountByDealerAsync(Guid dealerId, DateTime since, CancellationToken cancellationToken = default);
    Task<ImageTransformation> AddAsync(ImageTransformation transformation, CancellationToken cancellationToken = default);
    Task<ImageTransformation> UpdateAsync(ImageTransformation transformation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
