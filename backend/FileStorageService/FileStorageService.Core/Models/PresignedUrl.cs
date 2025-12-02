namespace FileStorageService.Core.Models;

/// <summary>
/// Type of presigned URL operation
/// </summary>
public enum PresignedUrlType
{
    /// <summary>
    /// URL for uploading a file
    /// </summary>
    Upload = 0,

    /// <summary>
    /// URL for downloading a file
    /// </summary>
    Download = 1,

    /// <summary>
    /// URL for deleting a file
    /// </summary>
    Delete = 2
}

/// <summary>
/// Represents a presigned URL for secure file operations
/// </summary>
public class PresignedUrl
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The presigned URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Type of operation this URL allows
    /// </summary>
    public PresignedUrlType Type { get; set; }

    /// <summary>
    /// Storage key (file path)
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// Required HTTP method
    /// </summary>
    public string HttpMethod { get; set; } = "PUT";

    /// <summary>
    /// Required headers
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// URL expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// URL creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expected content type for upload
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Maximum file size allowed for upload (bytes)
    /// </summary>
    public long? MaxSizeBytes { get; set; }

    /// <summary>
    /// Token for validation
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Owner identifier
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    /// Context or category
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Callback URL after upload completion
    /// </summary>
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// Custom metadata to attach to file
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Check if URL is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Check if URL is valid
    /// </summary>
    public bool IsValid => !IsExpired && !string.IsNullOrEmpty(Url);

    /// <summary>
    /// Time remaining until expiration
    /// </summary>
    public TimeSpan TimeRemaining => ExpiresAt - DateTime.UtcNow;

    /// <summary>
    /// Creates a new upload presigned URL
    /// </summary>
    public static PresignedUrl ForUpload(string url, string storageKey, string contentType, TimeSpan expiry) => new()
    {
        Url = url,
        Type = PresignedUrlType.Upload,
        StorageKey = storageKey,
        HttpMethod = "PUT",
        ContentType = contentType,
        ExpiresAt = DateTime.UtcNow.Add(expiry),
        Headers = new Dictionary<string, string>
        {
            ["Content-Type"] = contentType
        }
    };

    /// <summary>
    /// Creates a new download presigned URL
    /// </summary>
    public static PresignedUrl ForDownload(string url, string storageKey, TimeSpan expiry) => new()
    {
        Url = url,
        Type = PresignedUrlType.Download,
        StorageKey = storageKey,
        HttpMethod = "GET",
        ExpiresAt = DateTime.UtcNow.Add(expiry)
    };

    /// <summary>
    /// Get summary for logging
    /// </summary>
    public string GetSummary() =>
        $"{Type} URL for '{StorageKey}' expires in {TimeRemaining.TotalMinutes:F0} minutes";
}

/// <summary>
/// Request for creating a presigned URL
/// </summary>
public class PresignedUrlRequest
{
    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Content type
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Owner identifier
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Context or category
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Expiration time in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Custom metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Callback URL after upload
    /// </summary>
    public string? CallbackUrl { get; set; }
}

/// <summary>
/// Response for upload initialization
/// </summary>
public class UploadInitResponse
{
    /// <summary>
    /// File identifier
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Presigned URL for upload
    /// </summary>
    public PresignedUrl UploadUrl { get; set; } = new();

    /// <summary>
    /// Storage key
    /// </summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>
    /// Instructions for client
    /// </summary>
    public UploadInstructions Instructions { get; set; } = new();
}

/// <summary>
/// Instructions for client upload
/// </summary>
public class UploadInstructions
{
    /// <summary>
    /// HTTP method to use
    /// </summary>
    public string Method { get; set; } = "PUT";

    /// <summary>
    /// Required headers
    /// </summary>
    public Dictionary<string, string> RequiredHeaders { get; set; } = new();

    /// <summary>
    /// Maximum file size
    /// </summary>
    public long MaxSizeBytes { get; set; }

    /// <summary>
    /// Allowed content types
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Finalization endpoint
    /// </summary>
    public string? FinalizeEndpoint { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}
