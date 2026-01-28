using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using BackgroundRemovalService.Domain.Enums;
using BackgroundRemovalService.Domain.Interfaces;
using BackgroundRemovalService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackgroundRemovalService.Infrastructure.Providers;

/// <summary>
/// Configuración para Slazzer API
/// </summary>
public class SlazzerSettings
{
    public const string SectionName = "BackgroundRemoval:Providers:Slazzer";
    
    public string BaseUrl { get; set; } = "https://api.slazzer.com/v2.0";
    public int TimeoutSeconds { get; set; } = 60;
    public decimal CostPerImageUsd { get; set; } = 0.02m; // Más económico
}

/// <summary>
/// Implementación del proveedor Slazzer
/// https://www.slazzer.com/api
/// </summary>
public class SlazzerProvider : IBackgroundRemovalProvider
{
    private readonly HttpClient _httpClient;
    private readonly SlazzerSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<SlazzerProvider> _logger;

    public BackgroundRemovalProvider ProviderType => BackgroundRemovalProvider.Slazzer;
    public string ProviderName => "Slazzer";

    public SlazzerProvider(
        HttpClient httpClient,
        IOptions<SlazzerSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<SlazzerProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("API-KEY", _secrets.Slazzer.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return !string.IsNullOrEmpty(_secrets.Slazzer.ApiKey);
    }

    public async Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        byte[] imageBytes,
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var content = new MultipartFormDataContent();
            
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(imageContent, "source_image_file", "image.png");
            
            // Formato de salida
            var format = options.OutputFormat switch
            {
                OutputFormat.Png => "png",
                OutputFormat.Jpeg => "jpg",
                _ => "png"
            };
            content.Add(new StringContent(format), "format");
            
            // Color de fondo
            if (!string.IsNullOrEmpty(options.BackgroundColor))
            {
                content.Add(new StringContent(options.BackgroundColor.TrimStart('#')), "bg_color_code");
            }
            
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/remove_image_background", 
                content, 
                cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Slazzer API error: {StatusCode} - {Body}", 
                    response.StatusCode, errorBody);
                
                return BackgroundRemovalResult.Failure(
                    $"Slazzer API error: {response.StatusCode}",
                    response.StatusCode.ToString());
            }
            
            var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            
            return new BackgroundRemovalResult
            {
                IsSuccess = true,
                ImageBytes = resultBytes,
                ContentType = contentType,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                CreditsConsumed = 1
            };
        }
        catch (TaskCanceledException)
        {
            return BackgroundRemovalResult.Failure("Request timeout", "TIMEOUT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Slazzer API");
            return BackgroundRemovalResult.Failure(ex.Message, "EXCEPTION");
        }
    }

    public async Task<BackgroundRemovalResult> RemoveBackgroundFromUrlAsync(
        string imageUrl,
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(imageUrl), "source_image_url");
            
            var format = options.OutputFormat switch
            {
                OutputFormat.Png => "png",
                OutputFormat.Jpeg => "jpg",
                _ => "png"
            };
            content.Add(new StringContent(format), "format");
            
            if (!string.IsNullOrEmpty(options.BackgroundColor))
            {
                content.Add(new StringContent(options.BackgroundColor.TrimStart('#')), "bg_color_code");
            }
            
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/remove_image_background", 
                content, 
                cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return BackgroundRemovalResult.Failure(
                    $"Slazzer API error: {response.StatusCode}",
                    response.StatusCode.ToString());
            }
            
            var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            
            return new BackgroundRemovalResult
            {
                IsSuccess = true,
                ImageBytes = resultBytes,
                ContentType = contentType,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                CreditsConsumed = 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Slazzer API from URL");
            return BackgroundRemovalResult.Failure(ex.Message, "EXCEPTION");
        }
    }

    public async Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_settings.BaseUrl}/account", 
                cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return new ProviderAccountInfo { IsActive = false };
            }
            
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            return new ProviderAccountInfo
            {
                AvailableCredits = root.TryGetProperty("credits", out var credits) 
                    ? credits.GetDecimal() 
                    : 0,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Slazzer account info");
            return new ProviderAccountInfo { IsActive = false };
        }
    }
}
