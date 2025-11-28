using MediaService.Domain.Entities;

namespace MediaService.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for media variant operations
/// </summary>
public interface IMediaVariantRepository
{
    /// <summary>
    /// Gets a media variant by its unique identifier
    /// </summary>
    Task<MediaVariant?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all variants for a specific media asset
    /// </summary>
    Task<IEnumerable<MediaVariant>> GetByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific variant by name for a media asset
    /// </summary>
    Task<MediaVariant?> GetByMediaIdAndNameAsync(string mediaId, string variantName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new media variant
    /// </summary>
    Task AddAsync(MediaVariant mediaVariant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple media variants
    /// </summary>
    Task AddRangeAsync(IEnumerable<MediaVariant> mediaVariants, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing media variant
    /// </summary>
    Task UpdateAsync(MediaVariant mediaVariant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a media variant
    /// </summary>
    Task DeleteAsync(MediaVariant mediaVariant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all variants for a media asset
    /// </summary>
    Task DeleteByMediaIdAsync(string mediaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a variant exists for a media asset
    /// </summary>
    Task<bool> ExistsAsync(string mediaId, string variantName, CancellationToken cancellationToken = default);
}