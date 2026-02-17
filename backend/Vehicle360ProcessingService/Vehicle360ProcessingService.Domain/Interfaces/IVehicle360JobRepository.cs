using Vehicle360ProcessingService.Domain.Entities;

namespace Vehicle360ProcessingService.Domain.Interfaces;

public interface IVehicle360JobRepository
{
    Task<Vehicle360Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle360Job> CreateAsync(Vehicle360Job job, CancellationToken cancellationToken = default);
    Task<Vehicle360Job> UpdateAsync(Vehicle360Job job, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle360Job>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Vehicle360Job?> GetLatestCompletedByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle360Job>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle360Job>> GetPendingJobsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle360Job>> GetByStatusAsync(Vehicle360JobStatus status, CancellationToken cancellationToken = default);
    Task<int> GetQueuePositionAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<int> GetTotalJobsCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetActiveJobsCountAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Vehicle360JobStatus, int>> GetJobsCountByStatusAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle360Job>> GetStuckJobsAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<bool> HasActiveJobForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
