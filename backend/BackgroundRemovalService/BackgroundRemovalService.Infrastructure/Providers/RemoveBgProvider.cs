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
/// Configuración para Remove.bg API
/// </summary>
public class RemoveBgSettings
{
    public const string SectionName = "BackgroundRemoval:Providers:RemoveBg";
    
    public string BaseUrl { get; set; } = "https://api.remove.bg/v1.0";
    public int TimeoutSeconds { get; set; } = 60;
    public decimal CostPerImageUsd { get; set; } = 0.20m; // $0.20 por imagen con créditos
}

/// <summary>
/// Implementación del proveedor Remove.bg
/// https://www.remove.bg/api
/// </summary>
public class RemoveBgProvider : IBackgroundRemovalProvider
{
    private readonly HttpClient _httpClient;
    private readonly RemoveBgSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<RemoveBgProvider> _logger;

    public BackgroundRemovalProvider ProviderType => BackgroundRemovalProvider.RemoveBg;
    public string ProviderName => "Remove.bg";

    public RemoveBgProvider(
        HttpClient httpClient,
        IOptions<RemoveBgSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<RemoveBgProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _secrets.RemoveBg.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_secrets.RemoveBg.ApiKey))
        {
            _logger.LogWarning("Remove.bg API key is not configured in Secrets");
            return false;
        }
        
        try
        {
            var accountInfo = await GetAccountInfoAsync(cancellationToken);
            return accountInfo.IsActive && accountInfo.AvailableCredits > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Remove.bg availability");
            return false;
        }
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
            
            // Archivo de imagen
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(imageContent, "image_file", "image.png");
            
            // Opciones
            AddOptionsToContent(content, options);
            
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/removebg", 
                content, 
                cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Remove.bg API error: {StatusCode} - {Body}", 
                    response.StatusCode, errorBody);
                
                return BackgroundRemovalResult.Failure(
                    $"Remove.bg API error: {response.StatusCode}",
                    response.StatusCode.ToString());
            }
            
            var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            
            // Extraer créditos usados del header
            decimal creditsUsed = 1;
            if (response.Headers.TryGetValues("X-Credits-Charged", out var creditValues))
            {
                if (decimal.TryParse(creditValues.FirstOrDefault(), out var credits))
                {
                    creditsUsed = credits;
                }
            }
            
            // Créditos restantes
            decimal? remainingCredits = null;
            if (response.Headers.TryGetValues("X-Credits-Available", out var remainingValues))
            {
                if (decimal.TryParse(remainingValues.FirstOrDefault(), out var remaining))
                {
                    remainingCredits = remaining;
                }
            }
            
            _logger.LogInformation(
                "Remove.bg processing completed in {Time}ms, credits used: {Credits}",
                stopwatch.ElapsedMilliseconds, creditsUsed);
            
            return new BackgroundRemovalResult
            {
                IsSuccess = true,
                ImageBytes = resultBytes,
                ContentType = contentType,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                CreditsConsumed = creditsUsed,
                RemainingCredits = remainingCredits
            };
        }
        catch (TaskCanceledException)
        {
            return BackgroundRemovalResult.Failure("Request timeout", "TIMEOUT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Remove.bg API");
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
            
            // URL de imagen
            content.Add(new StringContent(imageUrl), "image_url");
            
            // Opciones
            AddOptionsToContent(content, options);
            
            var response = await _httpClient.PostAsync(
                $"{_settings.BaseUrl}/removebg", 
                content, 
                cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Remove.bg API error: {StatusCode} - {Body}", 
                    response.StatusCode, errorBody);
                
                return BackgroundRemovalResult.Failure(
                    $"Remove.bg API error: {response.StatusCode}",
                    response.StatusCode.ToString());
            }
            
            var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            
            decimal creditsUsed = 1;
            if (response.Headers.TryGetValues("X-Credits-Charged", out var creditValues))
            {
                if (decimal.TryParse(creditValues.FirstOrDefault(), out var credits))
                {
                    creditsUsed = credits;
                }
            }
            
            return new BackgroundRemovalResult
            {
                IsSuccess = true,
                ImageBytes = resultBytes,
                ContentType = contentType,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                CreditsConsumed = creditsUsed
            };
        }
        catch (TaskCanceledException)
        {
            return BackgroundRemovalResult.Failure("Request timeout", "TIMEOUT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Remove.bg API from URL");
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
            
            var data = root.GetProperty("data");
            var attributes = data.GetProperty("attributes");
            var credits = attributes.GetProperty("credits");
            
            return new ProviderAccountInfo
            {
                AvailableCredits = credits.GetProperty("total").GetDecimal(),
                UsedCredits = credits.GetProperty("used").GetDecimal(),
                SubscriptionPlan = credits.GetProperty("subscription").GetString(),
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Remove.bg account info");
            return new ProviderAccountInfo { IsActive = false };
        }
    }

    private void AddOptionsToContent(MultipartFormDataContent content, BackgroundRemovalOptions options)
    {
        // Formato de salida
        var format = options.OutputFormat switch
        {
            OutputFormat.Png => "png",
            OutputFormat.WebP => "png", // Remove.bg no soporta WebP directamente
            OutputFormat.Jpeg => "jpg",
            _ => "auto"
        };
        content.Add(new StringContent(format), "format");
        
        // Tamaño
        var size = options.OutputSize switch
        {
            ImageSize.Preview => "preview",
            ImageSize.Small => "small",
            ImageSize.Medium => "medium",
            ImageSize.Large => "hd",
            ImageSize.FullHD => "hd",
            ImageSize.UltraHD => "4k",
            _ => "auto"
        };
        content.Add(new StringContent(size), "size");
        
        // Color de fondo
        if (!string.IsNullOrEmpty(options.BackgroundColor))
        {
            content.Add(new StringContent(options.BackgroundColor), "bg_color");
        }
        
        // Imagen de fondo
        if (!string.IsNullOrEmpty(options.BackgroundImageUrl))
        {
            content.Add(new StringContent(options.BackgroundImageUrl), "bg_image_url");
        }
        
        // Sombra
        if (options.AddShadow)
        {
            content.Add(new StringContent("true"), "add_shadow");
            if (!string.IsNullOrEmpty(options.ShadowType))
            {
                content.Add(new StringContent(options.ShadowType), "shadow_type");
            }
        }
        
        // Recorte
        if (options.CropToForeground)
        {
            content.Add(new StringContent("true"), "crop");
            if (options.CropMargin > 0)
            {
                content.Add(new StringContent(options.CropMargin.ToString()), "crop_margin");
            }
        }
        
        // Tipo de objeto (para mejor detección)
        if (!string.IsNullOrEmpty(options.ObjectType))
        {
            var type = options.ObjectType.ToLower() switch
            {
                "car" => "car",
                "auto" => "car",
                "vehicle" => "car",
                "person" => "person",
                "product" => "product",
                _ => "auto"
            };
            content.Add(new StringContent(type), "type");
        }
    }
}
