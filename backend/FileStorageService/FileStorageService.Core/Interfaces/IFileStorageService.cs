using FileStorageService.Core.Models;

namespace FileStorageService.Core.Interfaces;

/// <summary>
/// Interface for the main file storage service
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Initializes a file upload with presigned URL
    /// </summary>
    /// <param name="request">Upload request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload initialization response</returns>
    Task<UploadInitResponse> InitializeUploadAsync(PresignedUrlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalizes an upload after client completes upload to presigned URL
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result with processing status</returns>
    Task<UploadResult> FinalizeUploadAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file directly (server-side upload)
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">Content type</param>
    /// <param name="ownerId">Owner identifier</param>
    /// <param name="context">Optional context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload result</returns>
    Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, string ownerId, string? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download result</returns>
    Task<DownloadResult> DownloadAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a presigned download URL
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="expirationMinutes">URL expiration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Presigned URL</returns>
    Task<PresignedUrl> GetDownloadUrlAsync(string fileId, int expirationMinutes = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Delete result</returns>
    Task<DeleteResult> DeleteAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file information
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stored file info</returns>
    Task<StoredFile?> GetFileInfoAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files for an owner
    /// </summary>
    /// <param name="ownerId">Owner identifier</param>
    /// <param name="context">Optional context filter</param>
    /// <param name="skip">Skip count for pagination</param>
    /// <param name="take">Take count for pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stored files</returns>
    Task<IEnumerable<StoredFile>> ListFilesAsync(string ownerId, string? context = null, int skip = 0, int take = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file
    /// </summary>
    /// <param name="fileId">Source file identifier</param>
    /// <param name="newOwnerId">New owner (optional)</param>
    /// <param name="newContext">New context (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Copied file</returns>
    Task<StoredFile?> CopyFileAsync(string fileId, string? newOwnerId = null, string? newContext = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rescans a file for viruses
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result</returns>
    Task<ScanResult> RescanFileAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-extracts metadata from a file
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated metadata</returns>
    Task<FileMetadata?> RefreshMetadataAsync(string fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates variants (thumbnails) for an image
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="variants">Variant configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated variants</returns>
    Task<IEnumerable<FileVariant>> GenerateVariantsAsync(string fileId, IEnumerable<VariantConfig> variants, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates file tags
    /// </summary>
    /// <param name="fileId">File identifier</param>
    /// <param name="tags">Tags to set</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated file</returns>
    Task<StoredFile?> UpdateTagsAsync(string fileId, Dictionary<string, string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets storage statistics
    /// </summary>
    /// <param name="ownerId">Owner filter (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage statistics</returns>
    Task<StorageStatistics> GetStatisticsAsync(string? ownerId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for generating a file variant
/// </summary>
public class VariantConfig
{
    /// <summary>
    /// Variant name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Maximum width
    /// </summary>
    public int MaxWidth { get; set; }

    /// <summary>
    /// Maximum height
    /// </summary>
    public int MaxHeight { get; set; }

    /// <summary>
    /// Output format (jpeg, png, webp)
    /// </summary>
    public string Format { get; set; } = "jpeg";

    /// <summary>
    /// Quality (1-100)
    /// </summary>
    public int Quality { get; set; } = 85;

    /// <summary>
    /// Resize mode (max, crop, pad)
    /// </summary>
    public string ResizeMode { get; set; } = "max";
}

/// <summary>
/// Storage usage statistics
/// </summary>
public class StorageStatistics
{
    /// <summary>
    /// Total files count
    /// </summary>
    public long TotalFiles { get; set; }

    /// <summary>
    /// Total storage used in bytes
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Files by type
    /// </summary>
    public Dictionary<FileType, long> FilesByType { get; set; } = new();

    /// <summary>
    /// Size by type in bytes
    /// </summary>
    public Dictionary<FileType, long> SizeByType { get; set; } = new();

    /// <summary>
    /// Files by status
    /// </summary>
    public Dictionary<FileStatus, long> FilesByStatus { get; set; } = new();

    /// <summary>
    /// Quarantined files count
    /// </summary>
    public long QuarantinedFiles { get; set; }

    /// <summary>
    /// Total variants count
    /// </summary>
    public long TotalVariants { get; set; }

    /// <summary>
    /// Average file size
    /// </summary>
    public long AverageFileSizeBytes => TotalFiles > 0 ? TotalSizeBytes / TotalFiles : 0;

    /// <summary>
    /// Get formatted total size
    /// </summary>
    public string FormattedTotalSize
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = TotalSizeBytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
