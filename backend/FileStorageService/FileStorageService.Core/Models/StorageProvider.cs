namespace FileStorageService.Core.Models;

/// <summary>
/// Supported storage providers
/// </summary>
public enum StorageProviderType
{
    /// <summary>
    /// Local file system storage
    /// </summary>
    Local = 0,

    /// <summary>
    /// Azure Blob Storage
    /// </summary>
    AzureBlob = 1,

    /// <summary>
    /// Amazon S3
    /// </summary>
    AmazonS3 = 2,

    /// <summary>
    /// Google Cloud Storage
    /// </summary>
    GoogleCloud = 3
}

/// <summary>
/// Storage provider configuration
/// </summary>
public class StorageProviderConfig
{
    /// <summary>
    /// Type of storage provider
    /// </summary>
    public StorageProviderType ProviderType { get; set; } = StorageProviderType.Local;

    /// <summary>
    /// Connection string or endpoint URL
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Container or bucket name
    /// </summary>
    public string ContainerName { get; set; } = "files";

    /// <summary>
    /// Base path for local storage
    /// </summary>
    public string BasePath { get; set; } = "./uploads";

    /// <summary>
    /// CDN base URL for public file access
    /// </summary>
    public string? CdnBaseUrl { get; set; }

    /// <summary>
    /// Access key for cloud providers
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Secret key for cloud providers
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// Region for cloud providers
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Maximum file size in bytes (default 100MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// Allowed content types
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = new[]
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "video/mp4", "video/webm",
        "application/pdf",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    /// <summary>
    /// Default expiration time for presigned URLs in minutes
    /// </summary>
    public int PresignedUrlExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Enable virus scanning
    /// </summary>
    public bool EnableVirusScan { get; set; } = true;

    /// <summary>
    /// Enable metadata extraction
    /// </summary>
    public bool EnableMetadataExtraction { get; set; } = true;

    /// <summary>
    /// Enable image optimization
    /// </summary>
    public bool EnableImageOptimization { get; set; } = true;
}
