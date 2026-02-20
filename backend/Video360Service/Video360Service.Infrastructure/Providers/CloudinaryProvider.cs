using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;
using Video360Service.Infrastructure.Configuration;

namespace Video360Service.Infrastructure.Providers;

/// <summary>
/// Proveedor Cloudinary ($12/mes, $0.012/vehículo)
/// Calidad: Buena
/// Buena integración con ecosistema de medios
/// </summary>
public class CloudinaryProvider : IVideo360Provider
{
    private readonly HttpClient _httpClient;
    private readonly CloudinarySettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<CloudinaryProvider> _logger;

    public Video360Provider ProviderType => Video360Provider.Cloudinary;
    public string ProviderName => "Cloudinary";
    public decimal CostPerVideoUsd => _settings.CostPerVideoUsd;

    public CloudinaryProvider(
        HttpClient httpClient,
        IOptions<CloudinarySettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<CloudinaryProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled || 
            string.IsNullOrEmpty(_secrets.Cloudinary.CloudName) ||
            string.IsNullOrEmpty(_secrets.Cloudinary.ApiKey))
        {
            return false;
        }
        
        try
        {
            var url = $"{_settings.BaseUrl}/{_secrets.Cloudinary.CloudName}/usage";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return response.StatusCode != System.Net.HttpStatusCode.Unauthorized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cloudinary availability check failed");
            return false;
        }
    }

    public async Task<Video360ExtractionResult> ExtractFramesAsync(
        byte[] videoBytes,
        Video360ExtractionOptions options,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Starting frame extraction with Cloudinary for {FrameCount} frames", options.FrameCount);
            
            // 1. Upload video to Cloudinary
            var uploadUrl = $"{_settings.BaseUrl}/{_secrets.Cloudinary.CloudName}/video/upload";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateSignature(timestamp, "video");
            
            using var uploadContent = new MultipartFormDataContent();
            uploadContent.Add(new ByteArrayContent(videoBytes), "file", "input.mp4");
            uploadContent.Add(new StringContent(_secrets.Cloudinary.ApiKey), "api_key");
            uploadContent.Add(new StringContent(timestamp), "timestamp");
            uploadContent.Add(new StringContent(signature), "signature");
            uploadContent.Add(new StringContent("video360_temp"), "folder");
            
            var uploadResponse = await _httpClient.PostAsync(uploadUrl, uploadContent, cancellationToken);
            
            if (!uploadResponse.IsSuccessStatusCode)
            {
                var errorContent = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Cloudinary upload error: {StatusCode} - {Error}", uploadResponse.StatusCode, errorContent);
                
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Upload error: {uploadResponse.StatusCode}",
                    ErrorCode = uploadResponse.StatusCode.ToString(),
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<CloudinaryUploadResult>(cancellationToken: cancellationToken);
            
            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Upload failed - no public_id returned",
                    ErrorCode = "UPLOAD_FAILED",
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            // 2. Generate frame URLs using Cloudinary transformations
            var frames = new List<ExtractedFrameData>();
            var duration = uploadResult.Duration ?? 30.0;
            var interval = duration / options.FrameCount;
            
            for (int i = 0; i < options.FrameCount; i++)
            {
                var timestamp_sec = i * interval;
                var angle = CalculateAngle(i, options.FrameCount);
                
                // Cloudinary URL transformation for extracting frame at specific time
                var transformation = $"so_{timestamp_sec:F2},f_{GetFormatString(options.OutputFormat)},q_{options.Quality}";
                if (options.Width.HasValue)
                    transformation += $",w_{options.Width}";
                if (options.Height.HasValue)
                    transformation += $",h_{options.Height}";
                
                var frameUrl = $"https://res.cloudinary.com/{_secrets.Cloudinary.CloudName}/video/upload/{transformation}/{uploadResult.PublicId}.{GetFormatString(options.OutputFormat)}";
                
                // Download the frame
                var frameResponse = await _httpClient.GetAsync(frameUrl, cancellationToken);
                var frameBytes = await frameResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                
                frames.Add(new ExtractedFrameData
                {
                    Index = i,
                    AngleDegrees = angle,
                    TimestampSeconds = timestamp_sec,
                    ImageBytes = frameBytes,
                    ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                    Width = options.Width ?? uploadResult.Width ?? 1920,
                    Height = options.Height ?? uploadResult.Height ?? 1080,
                    AngleLabel = Domain.Entities.ExtractedFrame.GetAngleLabelByIndex(i)
                });
            }
            
            // 3. Delete temporary video
            await DeleteVideoAsync(uploadResult.PublicId, cancellationToken);
            
            return new Video360ExtractionResult
            {
                IsSuccess = true,
                Frames = frames,
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                CostUsd = _settings.CostPerVideoUsd,
                VideoInfo = new VideoMetadata
                {
                    DurationSeconds = duration,
                    Width = uploadResult.Width ?? 1920,
                    Height = uploadResult.Height ?? 1080,
                    Fps = (int)(uploadResult.FrameRate ?? 30)
                },
                ProviderOperationId = uploadResult.PublicId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary frame extraction failed");
            
            return new Video360ExtractionResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "EXCEPTION",
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    public async Task<Video360ExtractionResult> ExtractFramesFromUrlAsync(
        string videoUrl,
        Video360ExtractionOptions options,
        CancellationToken cancellationToken = default)
    {
        // Download video first, then process
        try
        {
            var videoBytes = await _httpClient.GetByteArrayAsync(videoUrl, cancellationToken);
            return await ExtractFramesAsync(videoBytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download video from URL for Cloudinary processing");
            return new Video360ExtractionResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to download video: {ex.Message}",
                ErrorCode = "DOWNLOAD_FAILED"
            };
        }
    }

    public async Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/{_secrets.Cloudinary.CloudName}/usage";
            
            // Add basic auth
            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_secrets.Cloudinary.ApiKey}:{_secrets.Cloudinary.ApiSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new ProviderAccountInfo
                {
                    IsActive = false,
                    ErrorMessage = $"Failed to get account info: {response.StatusCode}"
                };
            }
            
            var usage = await response.Content.ReadFromJsonAsync<CloudinaryUsage>(cancellationToken: cancellationToken);
            
            return new ProviderAccountInfo
            {
                IsActive = true,
                RemainingCredits = usage?.Credits?.Remaining,
                CreditLimit = usage?.Credits?.Limit,
                CurrentPlan = usage?.Plan
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Cloudinary account info");
            return new ProviderAccountInfo
            {
                IsActive = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private string GenerateSignature(string timestamp, string resourceType)
    {
        var toSign = $"timestamp={timestamp}{_secrets.Cloudinary.ApiSecret}";
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(toSign));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
    
    private async Task DeleteVideoAsync(string publicId, CancellationToken cancellationToken)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateSignature(timestamp, "video");
            
            var deleteUrl = $"{_settings.BaseUrl}/{_secrets.Cloudinary.CloudName}/video/destroy";
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("public_id", publicId),
                new KeyValuePair<string, string>("api_key", _secrets.Cloudinary.ApiKey),
                new KeyValuePair<string, string>("timestamp", timestamp),
                new KeyValuePair<string, string>("signature", signature)
            });
            
            await _httpClient.PostAsync(deleteUrl, content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temporary video {PublicId} from Cloudinary", publicId);
        }
    }
    
    private static string GetFormatString(ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => "jpg",
        ImageFormat.Png => "png",
        ImageFormat.WebP => "webp",
        _ => "jpg"
    };
    
    private static int CalculateAngle(int frameIndex, int totalFrames)
    {
        return (360 / totalFrames) * frameIndex;
    }
}

// DTOs internos para Cloudinary
internal class CloudinaryUploadResult
{
    public string? PublicId { get; set; }
    public double? Duration { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FrameRate { get; set; }
}

internal class CloudinaryUsage
{
    public CloudinaryCredits? Credits { get; set; }
    public string? Plan { get; set; }
}

internal class CloudinaryCredits
{
    public int? Remaining { get; set; }
    public int? Limit { get; set; }
}
