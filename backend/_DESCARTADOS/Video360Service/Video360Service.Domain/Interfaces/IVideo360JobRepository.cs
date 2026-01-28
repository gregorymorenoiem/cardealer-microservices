using Video360Service.Domain.Entities;
using Video360Service.Domain.Enums;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Repositorio para trabajos de procesamiento de video 360
/// </summary>
public interface IVideo360JobRepository
{
    Task<Video360Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Video360Job?> GetByIdWithFramesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetByStatusAsync(Video360JobStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetPendingJobsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<Video360Job> CreateAsync(Video360Job job, CancellationToken cancellationToken = default);
    Task<Video360Job> UpdateAsync(Video360Job job, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetQueuePositionAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}
