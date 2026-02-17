using BackupDRService.Core.Models;

namespace BackupDRService.Core.Interfaces;

/// <summary>
/// Interface for backup storage providers
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Storage provider type
    /// </summary>
    StorageType StorageType { get; }

    /// <summary>
    /// Upload a backup file to storage
    /// </summary>
    Task<StorageUploadResult> UploadAsync(string localFilePath, string destinationPath);

    /// <summary>
    /// Download a backup file from storage
    /// </summary>
    Task<StorageDownloadResult> DownloadAsync(string storagePath, string localDestinationPath);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task<bool> DeleteAsync(string storagePath);

    /// <summary>
    /// Check if a file exists in storage
    /// </summary>
    Task<bool> ExistsAsync(string storagePath);

    /// <summary>
    /// Get file information from storage
    /// </summary>
    Task<StorageFileInfo?> GetFileInfoAsync(string storagePath);

    /// <summary>
    /// List files in a directory
    /// </summary>
    Task<IEnumerable<StorageFileInfo>> ListFilesAsync(string directoryPath, string? pattern = null);

    /// <summary>
    /// Get total storage used
    /// </summary>
    Task<long> GetTotalStorageUsedAsync();

    /// <summary>
    /// Verify file integrity using checksum
    /// </summary>
    Task<bool> VerifyIntegrityAsync(string storagePath, string expectedChecksum);

    /// <summary>
    /// Calculate checksum for a file
    /// </summary>
    Task<string> CalculateChecksumAsync(string storagePath);
}

/// <summary>
/// Result of an upload operation
/// </summary>
public class StorageUploadResult
{
    public bool Success { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public static StorageUploadResult Succeeded(string storagePath, long fileSize, string checksum)
    {
        return new StorageUploadResult
        {
            Success = true,
            StoragePath = storagePath,
            FileSizeBytes = fileSize,
            Checksum = checksum
        };
    }

    public static StorageUploadResult Failed(string errorMessage)
    {
        return new StorageUploadResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Result of a download operation
/// </summary>
public class StorageDownloadResult
{
    public bool Success { get; set; }
    public string LocalPath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;

    public static StorageDownloadResult Succeeded(string localPath, long fileSize)
    {
        return new StorageDownloadResult
        {
            Success = true,
            LocalPath = localPath,
            FileSizeBytes = fileSize
        };
    }

    public static StorageDownloadResult Failed(string errorMessage)
    {
        return new StorageDownloadResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Information about a stored file
/// </summary>
public class StorageFileInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? Checksum { get; set; }
    public StorageType StorageType { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
