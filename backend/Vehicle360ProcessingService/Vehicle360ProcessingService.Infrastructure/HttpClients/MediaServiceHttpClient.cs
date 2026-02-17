using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vehicle360ProcessingService.Domain.Interfaces;

namespace Vehicle360ProcessingService.Infrastructure.HttpClients;

/// <summary>
/// Cliente HTTP resiliente para MediaService
/// </summary>
public class MediaServiceHttpClient : IMediaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MediaServiceHttpClient> _logger;
    private readonly MediaServiceOptions _options;

    public MediaServiceHttpClient(
        HttpClient httpClient,
        ILogger<MediaServiceHttpClient> logger,
        IOptions<MediaServiceOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<MediaUploadResult> UploadVideoAsync(
        Stream videoStream, 
        string fileName, 
        string contentType, 
        string folder, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Uploading video {FileName} to MediaService", fileName);

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(videoStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(folder), "folder");
            content.Add(new StringContent("video"), "type");

            var response = await _httpClient.PostAsync("/api/media/upload", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MediaUploadApiResponse>(cancellationToken: cancellationToken);
                return new MediaUploadResult
                {
                    Success = true,
                    Url = result?.Url ?? string.Empty,
                    PublicId = result?.PublicId ?? string.Empty,
                    ThumbnailUrl = result?.ThumbnailUrl
                };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("MediaService upload failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            
            return new MediaUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload failed: {response.StatusCode}",
                ErrorCode = response.StatusCode.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video to MediaService");
            return new MediaUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "UPLOAD_ERROR"
            };
        }
    }

    public async Task<MediaUploadResult> UploadImageAsync(
        Stream imageStream, 
        string fileName, 
        string contentType, 
        string folder, 
        CancellationToken cancellationToken)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(folder), "folder");
            content.Add(new StringContent("image"), "type");

            var response = await _httpClient.PostAsync("/api/media/upload/image", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MediaUploadApiResponse>(cancellationToken: cancellationToken);
                return new MediaUploadResult
                {
                    Success = true,
                    Url = result?.Url ?? string.Empty,
                    PublicId = result?.PublicId ?? string.Empty,
                    ThumbnailUrl = result?.ThumbnailUrl
                };
            }

            return new MediaUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to MediaService");
            return new MediaUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<MediaUploadResult> UploadImageFromUrlAsync(
        string imageUrl, 
        string fileName, 
        string folder, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Uploading image from URL to MediaService: {FileName}", fileName);

            var request = new
            {
                sourceUrl = imageUrl,
                fileName,
                folder,
                type = "image"
            };

            var response = await _httpClient.PostAsJsonAsync("/api/media/upload/from-url", request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MediaUploadApiResponse>(cancellationToken: cancellationToken);
                return new MediaUploadResult
                {
                    Success = true,
                    Url = result?.Url ?? string.Empty,
                    PublicId = result?.PublicId ?? string.Empty
                };
            }

            return new MediaUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload from URL failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image from URL to MediaService");
            return new MediaUploadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<string?> GetFileUrlAsync(string publicId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/media/{publicId}/url", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UrlResponse>(cancellationToken: cancellationToken);
                return result?.Url;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL from MediaService");
            return null;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string publicId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/media/{publicId}/download", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from MediaService");
            return null;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Internal DTOs for API responses
    private class MediaUploadApiResponse
    {
        public string? Url { get; set; }
        public string? PublicId { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    private class UrlResponse
    {
        public string? Url { get; set; }
    }
}

public class MediaServiceOptions
{
    public string BaseUrl { get; set; } = "http://mediaservice:8080";
    public int TimeoutSeconds { get; set; } = 120;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}
