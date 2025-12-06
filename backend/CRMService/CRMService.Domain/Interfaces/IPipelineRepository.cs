using CRMService.Domain.Entities;

namespace CRMService.Domain.Interfaces;

public interface IPipelineRepository
{
    Task<Pipeline?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Pipeline?> GetByIdWithStagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pipeline>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Pipeline?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<Pipeline> AddAsync(Pipeline pipeline, CancellationToken cancellationToken = default);
    Task UpdateAsync(Pipeline pipeline, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
