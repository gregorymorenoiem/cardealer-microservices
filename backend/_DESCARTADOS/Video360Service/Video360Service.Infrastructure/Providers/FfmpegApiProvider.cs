using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;
using Video360Service.Infrastructure.Configuration;

namespace Video360Service.Infrastructure.Providers;

/// <summary>
/// Proveedor FFmpeg-API.com (Starter Plan - $11/mes, $0.011/vehículo)
/// Calidad: Excelente
/// DEFAULT - Mejor relación calidad/precio
/// </summary>
public class FfmpegApiProvider : IVideo360Provider
{
    private readonly HttpClient _httpClient;
    private readonly FfmpegApiSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<FfmpegApiProvider> _logger;

    public Video360Provider ProviderType => Video360Provider.FfmpegApi;
    public string ProviderName => "FFmpeg-API.com";
    public decimal CostPerVideoUsd => _settings.CostPerVideoUsd;

    public FfmpegApiProvider(
        HttpClient httpClient,
        IOptions<FfmpegApiSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<FfmpegApiProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secrets.FfmpegApi.ApiKey}");
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled || string.IsNullOrEmpty(_secrets.FfmpegApi.ApiKey))
        {
            return false;
        }
        
        try
        {
            var response = await _httpClient.GetAsync("/v1/status", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FFmpeg-API availability check failed");
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
            _logger.LogInformation("Starting frame extraction with FFmpeg-API for {FrameCount} frames", options.FrameCount);
            
            // Crear multipart form con el video
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(videoBytes), "video", "input.mp4");
            content.Add(new StringContent(options.FrameCount.ToString()), "frame_count");
            content.Add(new StringContent(GetFormatString(options.OutputFormat)), "output_format");
            content.Add(new StringContent(options.Quality.ToString()), "quality");
            
            if (options.Width.HasValue)
                content.Add(new StringContent(options.Width.Value.ToString()), "width");
            if (options.Height.HasValue)
                content.Add(new StringContent(options.Height.Value.ToString()), "height");
            
            var response = await _httpClient.PostAsync("/v1/video/extract-frames", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("FFmpeg-API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"API error: {response.StatusCode}",
                    ErrorCode = response.StatusCode.ToString(),
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var result = await response.Content.ReadFromJsonAsync<FfmpegApiResponse>(cancellationToken: cancellationToken);
            
            if (result?.Frames == null || result.Frames.Count == 0)
            {
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "No frames returned from API",
                    ErrorCode = "NO_FRAMES",
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var frames = new List<ExtractedFrameData>();
            for (int i = 0; i < result.Frames.Count; i++)
            {
                var frameData = result.Frames[i];
                var angle = ExtractedFrameData.CalculateAngle(i, options.FrameCount);
                
                frames.Add(new ExtractedFrameData
                {
                    Index = i,
                    AngleDegrees = angle,
                    TimestampSeconds = frameData.Timestamp,
                    ImageBytes = Convert.FromBase64String(frameData.Base64Data),
                    ThumbnailBytes = !string.IsNullOrEmpty(frameData.ThumbnailBase64) 
                        ? Convert.FromBase64String(frameData.ThumbnailBase64) 
                        : null,
                    ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                    Width = frameData.Width,
                    Height = frameData.Height,
                    AngleLabel = Domain.Entities.ExtractedFrame.GetAngleLabelByIndex(i)
                });
            }
            
            return new Video360ExtractionResult
            {
                IsSuccess = true,
                Frames = frames,
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                CostUsd = _settings.CostPerVideoUsd,
                VideoInfo = result.VideoInfo != null ? new VideoMetadata
                {
                    DurationSeconds = result.VideoInfo.Duration,
                    Width = result.VideoInfo.Width,
                    Height = result.VideoInfo.Height,
                    Fps = result.VideoInfo.Fps,
                    Codec = result.VideoInfo.Codec
                } : null,
                ProviderOperationId = result.OperationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFmpeg-API frame extraction failed");
            
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
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Starting frame extraction from URL with FFmpeg-API");
            
            var requestBody = new
            {
                video_url = videoUrl,
                frame_count = options.FrameCount,
                output_format = GetFormatString(options.OutputFormat),
                quality = options.Quality,
                width = options.Width,
                height = options.Height,
                generate_thumbnails = options.GenerateThumbnails
            };
            
            var response = await _httpClient.PostAsJsonAsync("/v1/video/extract-frames-url", requestBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("FFmpeg-API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"API error: {response.StatusCode}",
                    ErrorCode = response.StatusCode.ToString(),
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var result = await response.Content.ReadFromJsonAsync<FfmpegApiResponse>(cancellationToken: cancellationToken);
            
            if (result?.Frames == null || result.Frames.Count == 0)
            {
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "No frames returned from API",
                    ErrorCode = "NO_FRAMES",
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var frames = new List<ExtractedFrameData>();
            for (int i = 0; i < result.Frames.Count; i++)
            {
                var frameData = result.Frames[i];
                var angle = ExtractedFrameData.CalculateAngle(i, options.FrameCount);
                
                frames.Add(new ExtractedFrameData
                {
                    Index = i,
                    AngleDegrees = angle,
                    TimestampSeconds = frameData.Timestamp,
                    ImageBytes = Convert.FromBase64String(frameData.Base64Data),
                    ThumbnailBytes = !string.IsNullOrEmpty(frameData.ThumbnailBase64) 
                        ? Convert.FromBase64String(frameData.ThumbnailBase64) 
                        : null,
                    ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                    Width = frameData.Width,
                    Height = frameData.Height,
                    AngleLabel = Domain.Entities.ExtractedFrame.GetAngleLabelByIndex(i)
                });
            }
            
            return new Video360ExtractionResult
            {
                IsSuccess = true,
                Frames = frames,
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                CostUsd = _settings.CostPerVideoUsd,
                VideoInfo = result.VideoInfo != null ? new VideoMetadata
                {
                    DurationSeconds = result.VideoInfo.Duration,
                    Width = result.VideoInfo.Width,
                    Height = result.VideoInfo.Height,
                    Fps = result.VideoInfo.Fps,
                    Codec = result.VideoInfo.Codec
                } : null,
                ProviderOperationId = result.OperationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FFmpeg-API frame extraction from URL failed");
            
            return new Video360ExtractionResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "EXCEPTION",
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    public async Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/v1/account", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new ProviderAccountInfo
                {
                    IsActive = false,
                    ErrorMessage = $"Failed to get account info: {response.StatusCode}"
                };
            }
            
            var account = await response.Content.ReadFromJsonAsync<FfmpegApiAccountInfo>(cancellationToken: cancellationToken);
            
            return new ProviderAccountInfo
            {
                IsActive = account?.IsActive ?? false,
                RemainingCredits = account?.RemainingCredits,
                CreditLimit = account?.CreditLimit,
                CreditResetDate = account?.ResetDate,
                CurrentPlan = account?.Plan
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get FFmpeg-API account info");
            return new ProviderAccountInfo
            {
                IsActive = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private static string GetFormatString(ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => "jpeg",
        ImageFormat.Png => "png",
        ImageFormat.WebP => "webp",
        _ => "jpeg"
    };
    
    private static int CalculateAngle(int frameIndex, int totalFrames)
    {
        return (360 / totalFrames) * frameIndex;
    }
}

// DTOs internos para la respuesta de FFmpeg-API
internal class FfmpegApiResponse
{
    public string? OperationId { get; set; }
    public List<FfmpegFrameData>? Frames { get; set; }
    public FfmpegVideoInfo? VideoInfo { get; set; }
}

internal class FfmpegFrameData
{
    public int Index { get; set; }
    public double Timestamp { get; set; }
    public string Base64Data { get; set; } = string.Empty;
    public string? ThumbnailBase64 { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

internal class FfmpegVideoInfo
{
    public double Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Fps { get; set; }
    public string? Codec { get; set; }
}

internal class FfmpegApiAccountInfo
{
    public bool IsActive { get; set; }
    public int? RemainingCredits { get; set; }
    public int? CreditLimit { get; set; }
    public DateTime? ResetDate { get; set; }
    public string? Plan { get; set; }
}
