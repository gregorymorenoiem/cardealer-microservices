using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Video360Service.Domain.Interfaces;

namespace Video360Service.Infrastructure.Services;

/// <summary>
/// Configuración de S3
/// </summary>
public class S3StorageSettings
{
    public const string SectionName = "S3Storage";
    
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? ServiceUrl { get; set; }
    public string CdnBaseUrl { get; set; } = string.Empty;
    public int PresignedUrlExpirationMinutes { get; set; } = 60;
}

/// <summary>
/// Implementación de storage usando Amazon S3
/// </summary>
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3StorageSettings _settings;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<S3StorageSettings> settings,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<StorageUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = string.IsNullOrEmpty(folder) 
                ? fileName 
                : $"{folder.TrimEnd('/')}/{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            var url = !string.IsNullOrEmpty(_settings.CdnBaseUrl)
                ? $"{_settings.CdnBaseUrl.TrimEnd('/')}/{key}"
                : $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{key}";

            _logger.LogInformation("Archivo subido exitosamente: {Key}", key);

            return new StorageUploadResult
            {
                Success = true,
                Url = url,
                Key = key,
                FileSizeBytes = fileStream.Length,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo archivo {FileName}", fileName);
            return new StorageUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<StorageUploadResult> UploadFromPathAsync(
        string localPath,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(localPath);
        if (!fileInfo.Exists)
        {
            return new StorageUploadResult
            {
                Success = false,
                ErrorMessage = $"Archivo no encontrado: {localPath}"
            };
        }

        var fileName = $"{Guid.NewGuid()}{fileInfo.Extension}";
        var contentType = GetContentType(fileInfo.Extension);

        await using var stream = File.OpenRead(localPath);
        return await UploadAsync(stream, fileName, contentType, folder, cancellationToken);
    }

    public async Task<string> DownloadToPathAsync(
        string fileUrl,
        string localPath,
        CancellationToken cancellationToken = default)
    {
        var key = ExtractKeyFromUrl(fileUrl);

        var request = new GetObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        
        var directory = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await response.WriteResponseStreamToFileAsync(localPath, false, cancellationToken);

        _logger.LogInformation("Archivo descargado: {Key} -> {LocalPath}", key, localPath);
        
        return localPath;
    }

    public async Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);

            var request = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            
            _logger.LogInformation("Archivo eliminado: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando archivo {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        string fileUrl,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var key = ExtractKeyFromUrl(fileUrl);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    private string ExtractKeyFromUrl(string url)
    {
        // Soportar varios formatos de URL
        if (url.StartsWith("http"))
        {
            var uri = new Uri(url);
            return uri.AbsolutePath.TrimStart('/');
        }
        
        return url;
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".webm" => "video/webm",
            ".mkv" => "video/x-matroska",
            _ => "application/octet-stream"
        };
    }
}
