namespace MediaService.Shared.Settings;

/// <summary>
/// Configuration settings for storage providers
/// </summary>
public class StorageSettings
{
    /// <summary>
    /// Storage provider (Azure, AWS, Local, etc.)
    /// </summary>
    public string Provider { get; set; } = "Azure";

    /// <summary>
    /// Storage connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Container/bucket name for media assets
    /// </summary>
    public string ContainerName { get; set; } = "media-assets";

    /// <summary>
    /// Base URL for CDN distribution
    /// </summary>
    public string CdnBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Maximum upload size in bytes
    /// </summary>
    public long MaxUploadSizeBytes { get; set; } = 104857600; // 100MB

    /// <summary>
    /// Allowed content types for upload
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = new[]
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "video/mp4",
        "video/avi",
        "video/mov",
        "application/pdf"
    };

    /// <summary>
    /// SAS token expiration time in minutes
    /// </summary>
    public int SasTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Whether to enable versioning
    /// </summary>
    public bool EnableVersioning { get; set; } = false;

    /// <summary>
    /// Whether to enable soft delete
    /// </summary>
    public bool EnableSoftDelete { get; set; } = true;

    /// <summary>
    /// Retention period for deleted files in days
    /// </summary>
    public int RetentionDays { get; set; } = 30;
}