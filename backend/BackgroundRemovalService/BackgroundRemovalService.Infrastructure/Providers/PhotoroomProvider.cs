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
/// Configuración para Photoroom API
/// </summary>
public class PhotoroomSettings
{
    public const string SectionName = "BackgroundRemoval:Providers:Photoroom";
    
    public string BaseUrl { get; set; } = "https://sdk.photoroom.com/v1";
    public int TimeoutSeconds { get; set; } = 60;
    public decimal CostPerImageUsd { get; set; } = 0.05m;
}

/// <summary>
/// Implementación del proveedor Photoroom
/// https://www.photoroom.com/api
/// </summary>
public class PhotoroomProvider : IBackgroundRemovalProvider
{
    private readonly HttpClient _httpClient;
    private readonly PhotoroomSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<PhotoroomProvider> _logger;

    public BackgroundRemovalProvider ProviderType => BackgroundRemovalProvider.Photoroom;
    public string ProviderName => "Photoroom";

    public PhotoroomProvider(
        HttpClient httpClient,
        IOptions<PhotoroomSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<PhotoroomProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _secrets.Photoroom.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return !string.IsNullOrEmpty(_secrets.Photoroom.ApiKey);
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
            content.Add(imageContent, "image_file", "image.png");
            
            // Opciones de Photoroom
            if (!string.IsNullOrEmpty(options.BackgroundColor))
            {
                content.Add(new StringContent(options.BackgroundColor), "bg_color");
            }
            
            if (options.CropToForeground)
            {
                content.Add(new StringContent("true"), "crop");
            }
            
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/segment", 
                content, 
                cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Photoroom API error: {StatusCode} - {Body}", 
                    response.StatusCode, errorBody);
                
                return BackgroundRemovalResult.Failure(
                    $"Photoroom API error: {response.StatusCode}",
                    response.StatusCode.ToString());
            }
            
            var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            
            _logger.LogInformation(
                "Photoroom processing completed in {Time}ms",
                stopwatch.ElapsedMilliseconds);
            
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
            _logger.LogError(ex, "Error calling Photoroom API");
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
            
            content.Add(new StringContent(imageUrl), "image_url");
            
            if (!string.IsNullOrEmpty(options.BackgroundColor))
            {
                content.Add(new StringContent(options.BackgroundColor), "bg_color");
            }
            
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/segment", 
                content, 
                cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return BackgroundRemovalResult.Failure(
                    $"Photoroom API error: {response.StatusCode}",
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
            _logger.LogError(ex, "Error calling Photoroom API from URL");
            return BackgroundRemovalResult.Failure(ex.Message, "EXCEPTION");
        }
    }

    public Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        // Photoroom no tiene endpoint de account info en su API básica
        return Task.FromResult(new ProviderAccountInfo
        {
            IsActive = !string.IsNullOrEmpty(_secrets.Photoroom.ApiKey),
            AvailableCredits = decimal.MaxValue // Sin límite conocido
        });
    }
}
