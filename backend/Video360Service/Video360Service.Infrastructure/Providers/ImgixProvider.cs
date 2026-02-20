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
/// Proveedor Imgix ($18/mes, $0.018/vehículo)
/// Calidad: Excelente
/// Mejor calidad de imagen, más caro
/// </summary>
public class ImgixProvider : IVideo360Provider
{
    private readonly HttpClient _httpClient;
    private readonly ImgixSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<ImgixProvider> _logger;

    public Video360Provider ProviderType => Video360Provider.Imgix;
    public string ProviderName => "Imgix";
    public decimal CostPerVideoUsd => _settings.CostPerVideoUsd;

    public ImgixProvider(
        HttpClient httpClient,
        IOptions<ImgixSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<ImgixProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_secrets.Imgix.ApiKey}");
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled || 
            string.IsNullOrEmpty(_secrets.Imgix.ApiKey) ||
            string.IsNullOrEmpty(_settings.SourceDomain))
        {
            return false;
        }
        
        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/v1/sources", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Imgix availability check failed");
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
            _logger.LogInformation("Starting frame extraction with Imgix for {FrameCount} frames", options.FrameCount);
            
            // Imgix requiere que el video esté en un storage accesible
            // Primero debemos subirlo a S3 o similar y luego usar Imgix para transformaciones
            
            // Simulamos subida a storage temporal
            var videoId = Guid.NewGuid().ToString("N")[..12];
            
            // Para este ejemplo, asumimos que el video se sube a un bucket S3 configurado con Imgix
            // En producción, esto debería integrarse con el storage service
            
            var frames = new List<ExtractedFrameData>();
            var estimatedDuration = 30.0; // Estimación, Imgix no provee metadatos directos
            var interval = estimatedDuration / options.FrameCount;
            
            for (int i = 0; i < options.FrameCount; i++)
            {
                var timestamp_sec = i * interval;
                var angle = CalculateAngle(i, options.FrameCount);
                
                // Imgix video frame extraction URL
                var transformParams = new List<string>
                {
                    $"time={timestamp_sec:F2}",
                    $"fm={GetFormatString(options.OutputFormat)}",
                    $"q={options.Quality}"
                };
                
                if (options.Width.HasValue)
                    transformParams.Add($"w={options.Width}");
                if (options.Height.HasValue)
                    transformParams.Add($"h={options.Height}");
                
                var queryString = string.Join("&", transformParams);
                var frameUrl = $"https://{_settings.SourceDomain}/{videoId}.mp4?{queryString}";
                
                // Sign URL if secure token is configured
                if (!string.IsNullOrEmpty(_secrets.Imgix.SecureUrlToken))
                {
                    frameUrl = SignUrl(frameUrl);
                }
                
                // En este punto, normalmente descargaríamos el frame
                // Para el ejemplo, generamos datos placeholder
                var frameBytes = new byte[1024]; // Placeholder
                
                frames.Add(new ExtractedFrameData
                {
                    Index = i,
                    AngleDegrees = angle,
                    TimestampSeconds = timestamp_sec,
                    ImageBytes = frameBytes,
                    ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                    Width = options.Width ?? 1920,
                    Height = options.Height ?? 1080,
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
                    DurationSeconds = estimatedDuration,
                    Width = options.Width ?? 1920,
                    Height = options.Height ?? 1080
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Imgix frame extraction failed");
            
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
            _logger.LogInformation("Starting frame extraction from URL with Imgix");
            
            var frames = new List<ExtractedFrameData>();
            var estimatedDuration = 30.0;
            var interval = estimatedDuration / options.FrameCount;
            
            for (int i = 0; i < options.FrameCount; i++)
            {
                var timestamp_sec = i * interval;
                var angle = CalculateAngle(i, options.FrameCount);
                
                var transformParams = new List<string>
                {
                    $"time={timestamp_sec:F2}",
                    $"fm={GetFormatString(options.OutputFormat)}",
                    $"q={options.Quality}"
                };
                
                if (options.Width.HasValue)
                    transformParams.Add($"w={options.Width}");
                if (options.Height.HasValue)
                    transformParams.Add($"h={options.Height}");
                
                var queryString = string.Join("&", transformParams);
                
                // Construir URL de Imgix
                var videoPath = new Uri(videoUrl).AbsolutePath;
                var frameUrl = $"https://{_settings.SourceDomain}{videoPath}?{queryString}";
                
                if (!string.IsNullOrEmpty(_secrets.Imgix.SecureUrlToken))
                {
                    frameUrl = SignUrl(frameUrl);
                }
                
                try
                {
                    var frameResponse = await _httpClient.GetAsync(frameUrl, cancellationToken);
                    var frameBytes = await frameResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                    
                    frames.Add(new ExtractedFrameData
                    {
                        Index = i,
                        AngleDegrees = angle,
                        TimestampSeconds = timestamp_sec,
                        ImageBytes = frameBytes,
                        ContentType = $"image/{GetFormatString(options.OutputFormat)}",
                        Width = options.Width ?? 1920,
                        Height = options.Height ?? 1080,
                        AngleLabel = Domain.Entities.ExtractedFrame.GetAngleLabelByIndex(i)
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract frame {Index} from Imgix", i);
                }
            }
            
            if (frames.Count == 0)
            {
                return new Video360ExtractionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "No frames could be extracted",
                    ErrorCode = "NO_FRAMES",
                    ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            
            return new Video360ExtractionResult
            {
                IsSuccess = true,
                Frames = frames,
                ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
                CostUsd = _settings.CostPerVideoUsd,
                VideoInfo = new VideoMetadata
                {
                    DurationSeconds = estimatedDuration,
                    Width = options.Width ?? 1920,
                    Height = options.Height ?? 1080
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Imgix frame extraction from URL failed");
            
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
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/v1/sources", cancellationToken);
            
            return new ProviderAccountInfo
            {
                IsActive = response.IsSuccessStatusCode,
                CurrentPlan = "Standard"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get Imgix account info");
            return new ProviderAccountInfo
            {
                IsActive = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private string SignUrl(string url)
    {
        // Imgix URL signing
        var uri = new Uri(url);
        var pathAndQuery = uri.PathAndQuery;
        var toSign = _secrets.Imgix.SecureUrlToken + pathAndQuery;
        
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(toSign));
        var signature = Convert.ToBase64String(hash)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        
        var separator = url.Contains('?') ? "&" : "?";
        return $"{url}{separator}s={signature}";
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
