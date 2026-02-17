using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for storage provider operations
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Gets the provider type
    /// </summary>
    StorageProviderType ProviderType { get; }

    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    /// <param name="storageKey">Storage key (path)</param>
    /// <param name="fileStream">File content stream</param>
    /// <param name="contentType">Content type</param>
    /// <param name="metadata">Optional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UploadAsync(string storageKey, Stream fileStream, string contentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream</returns>
    Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists</returns>
    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file within storage
    /// </summary>
    /// <param name="sourceKey">Source storage key</param>
    /// <param name="destinationKey">Destination storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CopyAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a file within storage
    /// </summary>
    /// <param name="sourceKey">Source storage key</param>
    /// <param name="destinationKey">Destination storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task MoveAsync(string sourceKey, string destinationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the public URL for a file
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <returns>Public URL</returns>
    Task<string> GetPublicUrlAsync(string storageKey);

    /// <summary>
    /// Generates a presigned URL for upload
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="contentType">Content type</param>
    /// <param name="expiry">URL expiration</param>
    /// <returns>Presigned URL</returns>
    Task<PresignedUrl> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan expiry);

    /// <summary>
    /// Generates a presigned URL for download
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="expiry">URL expiration</param>
    /// <returns>Presigned URL</returns>
    Task<PresignedUrl> GenerateDownloadUrlAsync(string storageKey, TimeSpan expiry);

    /// <summary>
    /// Gets file metadata from storage
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Metadata dictionary</returns>
    Task<Dictionary<string, string>> GetMetadataAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets file metadata in storage
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="metadata">Metadata to set</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetMetadataAsync(string storageKey, Dictionary<string, string> metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file size
    /// </summary>
    /// <param name="storageKey">Storage key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File size in bytes</returns>
    Task<long> GetFileSizeAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files in a path/prefix
    /// </summary>
    /// <param name="prefix">Path prefix</param>
    /// <param name="maxResults">Maximum results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of storage keys</returns>
    Task<IEnumerable<string>> ListAsync(string prefix, int maxResults = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the storage provider is healthy
    /// </summary>
    /// <returns>True if healthy</returns>
    Task<bool> IsHealthyAsync();
}
