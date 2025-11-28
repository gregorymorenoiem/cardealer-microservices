namespace MediaService.Infrastructure.Services.Storage;

public class AzureBlobStorageOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "media";
    public string CdnBaseUrl { get; set; } = string.Empty;
    public long MaxUploadSizeBytes { get; set; } = 104857600;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    public int SasTokenExpirationMinutes { get; set; } = 60;
}