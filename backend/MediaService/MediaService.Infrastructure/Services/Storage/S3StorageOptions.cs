namespace MediaService.Infrastructure.Services.Storage;

public class S3StorageOptions
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = "media-service";
    public string CdnBaseUrl { get; set; } = string.Empty;
    /// <summary>
    /// Custom S3-compatible endpoint URL (e.g., https://nyc3.digitaloceanspaces.com for DO Spaces).
    /// Leave empty to use standard AWS S3 endpoints.
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;
    public long MaxUploadSizeBytes { get; set; } = 104857600;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public int PreSignedUrlExpirationMinutes { get; set; } = 60;
}