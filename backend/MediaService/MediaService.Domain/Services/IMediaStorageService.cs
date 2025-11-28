namespace MediaService.Domain.Interfaces.Services;

/// <summary>
/// Interface for media storage operations
/// </summary>
public interface IMediaStorageService
{
    /// <summary>
    /// Generates an upload URL for direct client upload
    /// </summary>
    Task<UploadUrlResponse> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan? expiry = null);

    /// <summary>
    /// Validates a file before upload
    /// </summary>
    Task<bool> ValidateFileAsync(string contentType, long fileSize);

    /// <summary>
    /// Generates a storage key for a file
    /// </summary>
    Task<string> GenerateStorageKeyAsync(string ownerId, string? context, string fileName);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    Task<bool> FileExistsAsync(string storageKey);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    Task<Stream> DownloadFileAsync(string storageKey);

    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    Task UploadFileAsync(string storageKey, Stream fileStream, string contentType);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    Task DeleteFileAsync(string storageKey);

    /// <summary>
    /// Gets the public URL for a file
    /// </summary>
    Task<string> GetFileUrlAsync(string storageKey);

    /// <summary>
    /// Copies a file within storage
    /// </summary>
    Task CopyFileAsync(string sourceKey, string destinationKey);

    /// <summary>
    /// Checks if the storage service is healthy
    /// </summary>
    Task<bool> IsHealthyAsync();
}

/// <summary>
/// Response for upload URL generation
/// </summary>
public class UploadUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string StorageKey { get; set; } = string.Empty;
}