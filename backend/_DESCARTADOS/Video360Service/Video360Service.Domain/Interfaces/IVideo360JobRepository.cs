using Video360Service.Domain.Entities;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Repositorio para Video360Job
/// </summary>
public interface IVideo360JobRepository
{
    Task<Video360Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Video360Job?> GetByIdWithFramesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetPendingJobsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Video360Job>> GetByStatusAsync(Enums.ProcessingStatus status, int limit = 100, CancellationToken cancellationToken = default);
    Task<Video360Job> CreateAsync(Video360Job job, CancellationToken cancellationToken = default);
    Task<Video360Job> UpdateAsync(Video360Job job, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(Enums.ProcessingStatus status, CancellationToken cancellationToken = default);
}
