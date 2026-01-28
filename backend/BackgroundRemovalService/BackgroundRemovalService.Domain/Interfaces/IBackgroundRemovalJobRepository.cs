using BackgroundRemovalService.Domain.Entities;
using BackgroundRemovalService.Domain.Enums;

namespace BackgroundRemovalService.Domain.Interfaces;

/// <summary>
/// Repositorio para jobs de remoción de fondo
/// </summary>
public interface IBackgroundRemovalJobRepository
{
    Task<BackgroundRemovalJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BackgroundRemovalJob?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BackgroundRemovalJob>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<BackgroundRemovalJob>> GetPendingJobsAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<BackgroundRemovalJob>> GetJobsByStatusAsync(ProcessingStatus status, int limit = 100, CancellationToken cancellationToken = default);
    Task<BackgroundRemovalJob> CreateAsync(BackgroundRemovalJob job, CancellationToken cancellationToken = default);
    Task<BackgroundRemovalJob> UpdateAsync(BackgroundRemovalJob job, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(ProcessingStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<BackgroundRemovalJob>> GetExpiredJobsAsync(DateTime expirationDate, int limit = 100, CancellationToken cancellationToken = default);
}
