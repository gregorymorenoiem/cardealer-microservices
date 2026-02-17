namespace FileStorageService.Core.Models;

/// <summary>
/// File status enumeration
/// </summary>
public enum FileStatus
{
    /// <summary>
    /// File is pending upload
    /// </summary>
    Pending = 0,

    /// <summary>
    /// File is being uploaded
    /// </summary>
    Uploading = 1,

    /// <summary>
    /// File is being processed (scanning, optimization)
    /// </summary>
    Processing = 2,

    /// <summary>
    /// File is ready for use
    /// </summary>
    Ready = 3,

    /// <summary>
    /// File processing failed
    /// </summary>
    Failed = 4,

    /// <summary>
    /// File was quarantined (virus detected)
    /// </summary>
    Quarantined = 5,

    /// <summary>
    /// File was deleted
    /// </summary>
    Deleted = 6
}

/// <summary>
/// File type classification
/// </summary>
public enum FileType
{
    /// <summary>
    /// Unknown file type
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Image files (JPEG, PNG, GIF, WebP)
    /// </summary>
    Image = 1,

    /// <summary>
    /// Video files (MP4, WebM)
    /// </summary>
    Video = 2,

    /// <summary>
    /// Audio files (MP3, WAV)
    /// </summary>
    Audio = 3,

    /// <summary>
    /// Document files (PDF, Word, Excel)
    /// </summary>
    Document = 4,

    /// <summary>
    /// Archive files (ZIP, RAR)
    /// </summary>
    Archive = 5
}

/// <summary>
/// Represents a stored file with metadata
/// </summary>
public class StoredFile
{
    /// <summary>
    /// Unique file identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Original file name
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Storage key (path in storage)
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// Content type (MIME type)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// File type classification
    /// </summary>
    public FileType FileType { get; set; } = FileType.Unknown;

    /// <summary>
    /// Current file status
    /// </summary>
    public FileStatus Status { get; set; } = FileStatus.Pending;

    /// <summary>
    /// Owner identifier
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Context or category
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Storage provider type
    /// </summary>
    public StorageProviderType ProviderType { get; set; }

    /// <summary>
    /// Public URL (via CDN if configured)
    /// </summary>
    public string? PublicUrl { get; set; }

    /// <summary>
    /// SHA-256 hash of file content
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Virus scan result
    /// </summary>
    public ScanResult? ScanResult { get; set; }

    /// <summary>
    /// Extracted metadata
    /// </summary>
    public FileMetadata? Metadata { get; set; }

    /// <summary>
    /// Generated thumbnails/variants
    /// </summary>
    public List<FileVariant> Variants { get; set; } = new();

    /// <summary>
    /// Custom tags
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new();

    /// <summary>
    /// Upload timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Expiration timestamp (for temporary files)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Error message if status is Failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Determines file type from content type
    /// </summary>
    public void DetermineFileType()
    {
        FileType = ContentType.ToLowerInvariant() switch
        {
            var ct when ct.StartsWith("image/") => FileType.Image,
            var ct when ct.StartsWith("video/") => FileType.Video,
            var ct when ct.StartsWith("audio/") => FileType.Audio,
            "application/pdf" => FileType.Document,
            var ct when ct.Contains("document") || ct.Contains("spreadsheet") || ct.Contains("presentation") => FileType.Document,
            var ct when ct.Contains("zip") || ct.Contains("rar") || ct.Contains("tar") || ct.Contains("gzip") => FileType.Archive,
            _ => FileType.Unknown
        };
    }

    /// <summary>
    /// Gets human-readable file size
    /// </summary>
    public string GetFormattedSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = SizeBytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Check if file is an image
    /// </summary>
    public bool IsImage => FileType == FileType.Image;

    /// <summary>
    /// Check if file is a video
    /// </summary>
    public bool IsVideo => FileType == FileType.Video;

    /// <summary>
    /// Check if file is ready for use
    /// </summary>
    public bool IsReady => Status == FileStatus.Ready;

    /// <summary>
    /// Check if file is safe (passed virus scan)
    /// </summary>
    public bool IsSafe => ScanResult?.IsClean ?? false;
}

/// <summary>
/// Represents a file variant (thumbnail, optimized version)
/// </summary>
public class FileVariant
{
    /// <summary>
    /// Variant name (e.g., "thumbnail", "small", "medium")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Storage key for this variant
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// Content type
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Width (for images/videos)
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Height (for images/videos)
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Public URL
    /// </summary>
    public string? PublicUrl { get; set; }
}
