using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MediaService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediaService.Infrastructure.Services.Storage;

namespace MediaService.Infrastructure.Services.Storage;

public class AzureBlobStorageService : IMediaStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobStorageOptions _options;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IOptions<AzureBlobStorageOptions> options, ILogger<AzureBlobStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _blobServiceClient = new BlobServiceClient(_options.ConnectionString);
    }

    public async Task<UploadUrlResponse> GenerateUploadUrlAsync(string storageKey, string contentType, TimeSpan? expiry = null)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobClient = containerClient.GetBlobClient(storageKey);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = storageKey,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(_options.SasTokenExpirationMinutes))
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        var response = new UploadUrlResponse
        {
            UploadUrl = sasUri.ToString(),
            ExpiresAt = sasBuilder.ExpiresOn.UtcDateTime,
            Headers = new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob",
                ["Content-Type"] = contentType
            },
            StorageKey = storageKey
        };

        return response;
    }

    public async Task<bool> ValidateFileAsync(string contentType, long fileSize)
    {
        var allowedTypes = _options.AllowedContentTypes ?? new[] { "image/jpeg", "image/png", "video/mp4" };
        var maxSize = _options.MaxUploadSizeBytes;

        var isValidType = allowedTypes.Contains(contentType);
        var isValidSize = fileSize <= maxSize;

        return isValidType && isValidSize;
    }

    public async Task<string> GenerateStorageKeyAsync(string ownerId, string? context, string fileName)
    {
        var safeFileName = Path.GetFileNameWithoutExtension(fileName)
            .Replace(" ", "_")
            .ToLowerInvariant();
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var random = Path.GetRandomFileName().Replace(".", "").Substring(0, 8);

        var key = $"{ownerId}/{context ?? "default"}/{timestamp}_{random}_{safeFileName}{extension}";
        return key;
    }

    public async Task<bool> FileExistsAsync(string storageKey)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = containerClient.GetBlobClient(storageKey);
        return await blobClient.ExistsAsync();
    }

    public async Task<string> GetFileUrlAsync(string storageKey)
    {
        if (!string.IsNullOrEmpty(_options.CdnBaseUrl))
        {
            return $"{_options.CdnBaseUrl}/{storageKey}";
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = containerClient.GetBlobClient(storageKey);
        return blobClient.Uri.ToString();
    }

    public async Task UploadFileAsync(string storageKey, Stream fileStream, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobClient = containerClient.GetBlobClient(storageKey);

        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
    }

    public async Task<Stream> DownloadFileAsync(string storageKey)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = containerClient.GetBlobClient(storageKey);

        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public async Task DeleteFileAsync(string storageKey)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = containerClient.GetBlobClient(storageKey);

        await blobClient.DeleteIfExistsAsync();
    }

    public async Task CopyFileAsync(string sourceKey, string destinationKey)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var sourceBlob = containerClient.GetBlobClient(sourceKey);
        var destBlob = containerClient.GetBlobClient(destinationKey);

        await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            await _blobServiceClient.GetPropertiesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Blob Storage health check failed");
            return false;
        }
    }
}