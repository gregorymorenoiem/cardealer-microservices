using Amazon.S3;
using Amazon.S3.Model;
using BackgroundRemovalService.Application.Interfaces;
using BackgroundRemovalService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundRemovalService.Infrastructure.Services;

public class S3StorageSettings
{
    public const string SectionName = "Storage:S3";
    
    public string BucketName { get; set; } = "cardealer-media";
    public string Region { get; set; } = "us-east-1";
    public string BaseUrl { get; set; } = string.Empty;
    public bool UseLocalPath { get; set; } = false;
    public string LocalPath { get; set; } = "./media-files";
}

/// <summary>
/// Servicio de almacenamiento usando S3 o almacenamiento local
/// </summary>
public class ImageStorageService : IImageStorageService
{
    private readonly S3StorageSettings _settings;
    private readonly IAmazonS3? _s3Client;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageStorageService> _logger;

    public ImageStorageService(
        IOptions<S3StorageSettings> settings,
        IAmazonS3? s3Client,
        HttpClient httpClient,
        ILogger<ImageStorageService> logger)
    {
        _settings = settings.Value;
        _s3Client = s3Client;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(
        byte[] imageBytes,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var key = string.IsNullOrEmpty(folder) ? fileName : $"{folder}/{fileName}";
        
        if (_settings.UseLocalPath)
        {
            return await SaveLocalAsync(imageBytes, key, cancellationToken);
        }
        
        return await SaveToS3Async(imageBytes, key, contentType, cancellationToken);
    }

    public async Task<byte[]?> DownloadImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Si es una URL local, leer del disco
            if (imageUrl.StartsWith("file://") || imageUrl.StartsWith("/") || imageUrl.StartsWith("./"))
            {
                var localPath = imageUrl.Replace("file://", "");
                if (File.Exists(localPath))
                {
                    return await File.ReadAllBytesAsync(localPath, cancellationToken);
                }
                return null;
            }
            
            // Si es una URL de S3, descargar directamente
            if (imageUrl.Contains(_settings.BucketName) && _s3Client != null)
            {
                var key = ExtractS3Key(imageUrl);
                var response = await _s3Client.GetObjectAsync(_settings.BucketName, key, cancellationToken);
                using var ms = new MemoryStream();
                await response.ResponseStream.CopyToAsync(ms, cancellationToken);
                return ms.ToArray();
            }
            
            // URL externa, descargar via HTTP
            var httpResponse = await _httpClient.GetAsync(imageUrl, cancellationToken);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download image from {Url}: {Status}", 
                    imageUrl, httpResponse.StatusCode);
                return null;
            }
            
            return await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image from {Url}", imageUrl);
            return null;
        }
    }

    public async Task<bool> DeleteImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_settings.UseLocalPath)
            {
                var localPath = imageUrl.Replace("file://", "");
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                    return true;
                }
                return false;
            }
            
            if (_s3Client != null)
            {
                var key = ExtractS3Key(imageUrl);
                await _s3Client.DeleteObjectAsync(_settings.BucketName, key, cancellationToken);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {Url}", imageUrl);
            return false;
        }
    }

    public async Task<string> GenerateUploadUrlAsync(
        string fileName,
        string contentType,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        if (_s3Client == null)
        {
            throw new InvalidOperationException("S3 client not configured");
        }
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = $"uploads/{Guid.NewGuid()}/{fileName}",
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
        
        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public async Task<bool> ImageExistsAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_settings.UseLocalPath)
            {
                var localPath = imageUrl.Replace("file://", "");
                return File.Exists(localPath);
            }
            
            if (_s3Client != null)
            {
                var key = ExtractS3Key(imageUrl);
                var response = await _s3Client.GetObjectMetadataAsync(_settings.BucketName, key, cancellationToken);
                return true;
            }
            
            // Check via HTTP HEAD
            var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
            var httpResponse = await _httpClient.SendAsync(request, cancellationToken);
            return httpResponse.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // === Private Methods ===

    private async Task<string> SaveLocalAsync(
        byte[] imageBytes,
        string key,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_settings.LocalPath, key);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllBytesAsync(fullPath, imageBytes, cancellationToken);
        
        _logger.LogDebug("Saved image locally to {Path}", fullPath);
        
        return $"file://{fullPath}";
    }

    private async Task<string> SaveToS3Async(
        byte[] imageBytes,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (_s3Client == null)
        {
            throw new InvalidOperationException("S3 client not configured");
        }
        
        using var stream = new MemoryStream(imageBytes);
        
        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };
        
        await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        
        var url = string.IsNullOrEmpty(_settings.BaseUrl)
            ? $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{key}"
            : $"{_settings.BaseUrl}/{key}";
        
        _logger.LogDebug("Saved image to S3: {Url}", url);
        
        return url;
    }

    private string ExtractS3Key(string url)
    {
        // Extract key from S3 URL
        // https://bucket.s3.region.amazonaws.com/key/path
        // or https://cdn.example.com/key/path
        
        var uri = new Uri(url);
        var path = uri.AbsolutePath.TrimStart('/');
        
        // Remove bucket name if present in path
        if (path.StartsWith(_settings.BucketName))
        {
            path = path.Substring(_settings.BucketName.Length).TrimStart('/');
        }
        
        return path;
    }
}
