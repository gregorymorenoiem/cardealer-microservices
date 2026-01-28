using Video360Service.Domain.Entities;

namespace Video360Service.Domain.Interfaces;

/// <summary>
/// Repositorio para frames extraídos
/// </summary>
public interface IExtractedFrameRepository
{
    Task<ExtractedFrame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExtractedFrame>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<ExtractedFrame?> GetPrimaryFrameByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<ExtractedFrame> CreateAsync(ExtractedFrame frame, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExtractedFrame>> CreateManyAsync(IEnumerable<ExtractedFrame> frames, CancellationToken cancellationToken = default);
    Task<ExtractedFrame> UpdateAsync(ExtractedFrame frame, CancellationToken cancellationToken = default);
    Task DeleteByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
}
