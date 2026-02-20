using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Video360Service.Application.Interfaces;
using Video360Service.Infrastructure.Configuration;

namespace Video360Service.Infrastructure.Services;

/// <summary>
/// Servicio de almacenamiento para videos e imágenes usando S3
/// </summary>
public class VideoStorageService : IVideoStorageService
{
    private readonly S3StorageSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly IAmazonS3 _s3Client;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VideoStorageService> _logger;

    public VideoStorageService(
        IOptions<S3StorageSettings> settings,
        IOptions<SecretsSettings> secrets,
        HttpClient httpClient,
        ILogger<VideoStorageService> logger)
    {
        _settings = settings.Value;
        _secrets = secrets.Value;
        _httpClient = httpClient;
        _logger = logger;
        
        var s3Config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_settings.Region),
            ServiceURL = string.IsNullOrEmpty(_settings.ServiceUrl) ? null : _settings.ServiceUrl,
            ForcePathStyle = _settings.ForcePathStyle
        };
        
        _s3Client = new AmazonS3Client(
            _secrets.S3.AccessKey,
            _secrets.S3.SecretKey,
            s3Config);
    }

    public async Task<string> UploadVideoAsync(
        byte[] videoBytes, 
        string fileName, 
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var key = $"videos/{Guid.NewGuid():N}/{fileName}";
        
        try
        {
            _logger.LogInformation("Uploading video {Key} ({Size} bytes)", key, videoBytes.Length);
            
            using var stream = new MemoryStream(videoBytes);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = _settings.BucketName,
                Key = key,
                ContentType = contentType,
                StorageClass = S3StorageClass.Standard,
                CannedACL = S3CannedACL.Private
            };
            
            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest, cancellationToken);
            
            _logger.LogInformation("Video uploaded successfully: {Key}", key);
            
            return GetPublicUrl(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video {Key}", key);
            throw;
        }
    }

    public async Task<string> UploadImageAsync(
        byte[] imageBytes, 
        string fileName, 
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var key = $"frames/{Guid.NewGuid():N}/{fileName}";
        
        try
        {
            _logger.LogDebug("Uploading image {FileName}", fileName);
            
            using var stream = new MemoryStream(imageBytes);
            var putRequest = new PutObjectRequest
            {
                InputStream = stream,
                BucketName = _settings.BucketName,
                Key = key,
                ContentType = contentType,
                StorageClass = S3StorageClass.Standard,
                CannedACL = S3CannedACL.PublicRead
            };
            
            await _s3Client.PutObjectAsync(putRequest, cancellationToken);
            
            return GetPublicUrl(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image {FileName}", fileName);
            throw;
        }
    }

    public async Task<byte[]> DownloadAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Si es una URL de S3, descargar directamente desde S3
            if (url.Contains(_settings.BucketName))
            {
                var key = ExtractKeyFromUrl(url);
                var getRequest = new GetObjectRequest
                {
                    BucketName = _settings.BucketName,
                    Key = key
                };
                
                using var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
                
                return memoryStream.ToArray();
            }
            
            // Si es una URL externa, descargar vía HTTP
            return await _httpClient.GetByteArrayAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download from {Url}", url);
            throw;
        }
    }

    public async Task DeleteAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(url);
            
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key
            };
            
            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            
            _logger.LogInformation("Deleted file {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete {Url}", url);
            throw;
        }
    }

    public async Task<string> GenerateSignedUrlAsync(
        string url,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var key = ExtractKeyFromUrl(url);
        
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiration)
        };
        
        var signedUrl = await _s3Client.GetPreSignedURLAsync(request);
        
        _logger.LogDebug("Generated signed URL for {Key}, expires in {Expiration}", key, expiration);
        
        return signedUrl;
    }
    
    private string GetPublicUrl(string key)
    {
        if (!string.IsNullOrEmpty(_settings.CdnBaseUrl))
        {
            return $"{_settings.CdnBaseUrl.TrimEnd('/')}/{key}";
        }
        
        return $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{key}";
    }
    
    private string ExtractKeyFromUrl(string url)
    {
        if (url.Contains(_settings.BucketName))
        {
            var uri = new Uri(url);
            return uri.AbsolutePath.TrimStart('/');
        }
        
        if (!string.IsNullOrEmpty(_settings.CdnBaseUrl) && url.StartsWith(_settings.CdnBaseUrl))
        {
            return url.Replace(_settings.CdnBaseUrl.TrimEnd('/') + "/", "");
        }
        
        return url;
    }
}
