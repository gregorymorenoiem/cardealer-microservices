using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Video360Service.Domain.Enums;
using Video360Service.Domain.Interfaces;
using Video360Service.Infrastructure.Configuration;

namespace Video360Service.Infrastructure.Providers;

/// <summary>
/// Proveedor ApyHub Video API ($9/mes, $0.009/vehículo)
/// Calidad: Muy buena
/// Más económico pero con límites menores
/// </summary>
public class ApyHubProvider : IVideo360Provider
{
    private readonly HttpClient _httpClient;
    private readonly ApyHubSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<ApyHubProvider> _logger;

    public Video360Provider ProviderType => Video360Provider.ApyHub;
    public string ProviderName => "ApyHub Video API";
    public decimal CostPerVideoUsd => _settings.CostPerVideoUsd;

    public ApyHubProvider(
        HttpClient httpClient,
        IOptions<ApyHubSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<ApyHubProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("apy-token", _secrets.ApyHub.ApiKey);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled || string.IsNullOrEmpty(_secrets.ApyHub.ApiKey))
        {
            return false;
        }
        
        try
        {
            // ApyHub no tiene endpoint de status, verificamos con un ping simple
            var response = await _httpClient.GetAsync("/", cancellationToken);
            return response.StatusCode != System.Net.HttpStatusCode.Unauthorized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ApyHub availability check failed");
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
            _logger.LogInformation("Starting frame extraction with ApyHub for {FrameCount} frames", options.FrameCount);
            
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(videoBytes), "file", "input.mp4");
            content.Add(new StringContent(options.FrameCount.ToString()), "count");
            content.Add(new StringContent(GetFormatString(options.OutputFormat)), "format");
            
            var response = await _httpClient.PostAsync("/extract-frames", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ApyHub error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"API error: {response.StatusCode}",
                    ErrorCode = response.StatusCode.ToString(),
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var result = await response.Content.ReadFromJsonAsync<ApyHubResponse>(cancellationToken: cancellationToken);
            
            if (result?.Data?.Images == null || result.Data.Images.Count == 0)
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
            var videoDuration = result.Data.Duration ?? 30.0;
            var interval = videoDuration / options.FrameCount;
            
            for (int i = 0; i < result.Data.Images.Count && i < options.FrameCount; i++)
            {
                var imageBase64 = result.Data.Images[i];
                var angle = CalculateAngle(i, options.FrameCount);
                
                frames.Add(new ExtractedFrameData
                {
                    Index = i,
                    AngleDegrees = angle,
                    TimestampSeconds = i * interval,
                    ImageBytes = Convert.FromBase64String(imageBase64),
                    ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                    Width = result.Data.Width ?? 1920,
                    Height = result.Data.Height ?? 1080,
                    AngleLabel = Domain.Entities.ExtractedFrame.GetAngleLabelByIndex(i)
                });
            }
            
            return new Video360ExtractionResult
            {
                IsSuccess = true,
                Frames = frames,
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                CostUsd = _settings.CostPerVideoUsd,
                VideoInfo = new VideoMetadata
                {
                    DurationSeconds = videoDuration,
                    Width = result.Data.Width ?? 1920,
                    Height = result.Data.Height ?? 1080
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApyHub frame extraction failed");
            
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
            _logger.LogInformation("Starting frame extraction from URL with ApyHub");
            
            var requestBody = new
            {
                url = videoUrl,
                count = options.FrameCount,
                format = GetFormatString(options.OutputFormat)
            };
            
            var response = await _httpClient.PostAsJsonAsync("/extract-frames/url", requestBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ApyHub error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"API error: {response.StatusCode}",
                    ErrorCode = response.StatusCode.ToString(),
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            var result = await response.Content.ReadFromJsonAsync<ApyHubResponse>(cancellationToken: cancellationToken);
            
            if (result?.Data?.Images == null || result.Data.Images.Count == 0)
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
            var videoDuration = result.Data.Duration ?? 30.0;
            var interval = videoDuration / options.FrameCount;
            
            for (int i = 0; i < result.Data.Images.Count && i < options.FrameCount; i++)
            {
                var imageBase64 = result.Data.Images[i];
                var angle = CalculateAngle(i, options.FrameCount);
                
                frames.Add(new ExtractedFrameData
                {
                    Index = i,
                    AngleDegrees = angle,
                    TimestampSeconds = i * interval,
                    ImageBytes = Convert.FromBase64String(imageBase64),
                    ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                    Width = result.Data.Width ?? 1920,
                    Height = result.Data.Height ?? 1080,
                    AngleLabel = Domain.Entities.ExtractedFrame.GetAngleLabelByIndex(i)
                });
            }
            
            return new Video360ExtractionResult
            {
                IsSuccess = true,
                Frames = frames,
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                CostUsd = _settings.CostPerVideoUsd,
                VideoInfo = new VideoMetadata
                {
                    DurationSeconds = videoDuration,
                    Width = result.Data.Width ?? 1920,
                    Height = result.Data.Height ?? 1080
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApyHub frame extraction from URL failed");
            
            return new Video360ExtractionResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "EXCEPTION",
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
            };
        }
    }

    public Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        // ApyHub no tiene API de account info
        return Task.FromResult(new ProviderAccountInfo
        {
            IsActive = _settings.IsEnabled && !string.IsNullOrEmpty(_secrets.ApyHub.ApiKey),
            CurrentPlan = "Standard"
        });
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

// DTOs internos para ApyHub
internal class ApyHubResponse
{
    public ApyHubData? Data { get; set; }
}

internal class ApyHubData
{
    public List<string>? Images { get; set; }
    public double? Duration { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}
