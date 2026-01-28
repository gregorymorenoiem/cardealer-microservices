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
/// Configuración para ClipDrop API (Stability AI)
/// https://clipdrop.co/apis/docs/remove-background
/// </summary>
public class ClipDropSettings
{
    public const string SectionName = "BackgroundRemoval:Providers:ClipDrop";
    
    /// <summary>
    /// URL base de la API
    /// </summary>
    public string BaseUrl { get; set; } = "https://clipdrop-api.co";
    
    /// <summary>
    /// Timeout en segundos para las peticiones
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Costo estimado por imagen en USD
    /// ClipDrop cobra aproximadamente $0.03-$0.10 por imagen dependiendo del plan
    /// </summary>
    public decimal CostPerImageUsd { get; set; } = 0.05m;
    
    /// <summary>
    /// Si es true, este proveedor es el default
    /// </summary>
    public bool IsDefault { get; set; } = true;
    
    /// <summary>
    /// Prioridad del proveedor (menor = mayor prioridad)
    /// </summary>
    public int Priority { get; set; } = 0;
}

/// <summary>
/// Implementación del proveedor ClipDrop (Stability AI)
/// API Documentation: https://clipdrop.co/apis/docs/remove-background
/// 
/// Características:
/// - Alta calidad de remoción de fondo
/// - Soporta imágenes hasta 25MB
/// - Formatos soportados: JPEG, PNG, WebP
/// - Respuesta en formato PNG con transparencia
/// </summary>
public class ClipDropProvider : IBackgroundRemovalProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClipDropSettings _settings;
    private readonly SecretsSettings _secrets;
    private readonly ILogger<ClipDropProvider> _logger;
    
    private const string RemoveBackgroundEndpoint = "/remove-background/v1";

    public BackgroundRemovalProvider ProviderType => BackgroundRemovalProvider.ClipDrop;
    public string ProviderName => "ClipDrop (Stability AI)";

    public ClipDropProvider(
        HttpClient httpClient,
        IOptions<ClipDropSettings> settings,
        IOptions<SecretsSettings> secrets,
        ILogger<ClipDropProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _secrets = secrets.Value;
        _logger = logger;
        
        // Configurar headers por defecto - API key desde Secrets
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _secrets.ClipDrop.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    /// <summary>
    /// Verifica si ClipDrop está disponible y configurado
    /// </summary>
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_secrets.ClipDrop.ApiKey))
        {
            _logger.LogWarning("ClipDrop API key is not configured in Secrets");
            return Task.FromResult(false);
        }
        
        // ClipDrop no tiene un endpoint de health check público,
        // pero verificamos que el API key tenga el formato correcto
        if (_secrets.ClipDrop.ApiKey.Length < 32)
        {
            _logger.LogWarning("ClipDrop API key appears to be invalid (too short)");
            return Task.FromResult(false);
        }
        
        _logger.LogDebug("ClipDrop provider is configured and available");
        return Task.FromResult(true);
    }

    /// <summary>
    /// Remueve el fondo de una imagen usando ClipDrop API
    /// </summary>
    public async Task<BackgroundRemovalResult> RemoveBackgroundAsync(
        byte[] imageBytes,
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting background removal with ClipDrop. Image size: {Size} bytes", 
                imageBytes.Length);
            
            // Validar tamaño (máximo 25MB según docs de ClipDrop)
            if (imageBytes.Length > 25 * 1024 * 1024)
            {
                return BackgroundRemovalResult.Failure(
                    "Image exceeds maximum size of 25MB",
                    "IMAGE_TOO_LARGE");
            }

            using var content = new MultipartFormDataContent();
            
            // Agregar imagen como form-data con el nombre "image_file"
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(imageBytes));
            content.Add(imageContent, "image_file", "image.png");
            
            var requestUrl = $"{_settings.BaseUrl}{RemoveBackgroundEndpoint}";
            
            _logger.LogDebug("Sending request to ClipDrop: {Url}", requestUrl);
            
            var response = await _httpClient.PostAsync(requestUrl, content, cancellationToken);
            
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("ClipDrop API error: {StatusCode} - {Body}", 
                    response.StatusCode, errorBody);
                
                var errorMessage = ParseErrorMessage(errorBody, response.StatusCode);
                
                return BackgroundRemovalResult.Failure(
                    errorMessage,
                    response.StatusCode.ToString());
            }
            
            var resultBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            
            _logger.LogInformation(
                "ClipDrop background removal successful. Input: {InputSize} bytes, Output: {OutputSize} bytes, Time: {Time}ms",
                imageBytes.Length, resultBytes.Length, stopwatch.ElapsedMilliseconds);
            
            // Convertir formato si es necesario
            var outputBytes = options.OutputFormat switch
            {
                OutputFormat.Png => resultBytes, // ClipDrop ya devuelve PNG
                OutputFormat.WebP => ConvertToWebp(resultBytes),
                OutputFormat.Jpeg => ConvertToJpeg(resultBytes, options.BackgroundColor),
                _ => resultBytes
            };
            
            return BackgroundRemovalResult.Success(
                outputBytes,
                GetOutputContentType(options.OutputFormat),
                stopwatch.ElapsedMilliseconds,
                _settings.CostPerImageUsd);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("ClipDrop request was cancelled");
            throw;
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            _logger.LogError("ClipDrop request timed out after {Timeout}s", _settings.TimeoutSeconds);
            return BackgroundRemovalResult.Failure(
                $"Request timed out after {_settings.TimeoutSeconds} seconds",
                "TIMEOUT");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP error calling ClipDrop API");
            return BackgroundRemovalResult.Failure(
                $"HTTP error: {ex.Message}",
                "HTTP_ERROR");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error during ClipDrop background removal");
            return BackgroundRemovalResult.Failure(
                $"Unexpected error: {ex.Message}",
                "UNKNOWN_ERROR");
        }
    }

    /// <summary>
    /// Remueve el fondo de una imagen desde una URL
    /// ClipDrop no soporta URLs directamente, descargamos primero
    /// </summary>
    public async Task<BackgroundRemovalResult> RemoveBackgroundFromUrlAsync(
        string imageUrl, 
        BackgroundRemovalOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Downloading image from URL: {Url}", imageUrl);
            
            using var downloadClient = new HttpClient();
            downloadClient.Timeout = TimeSpan.FromSeconds(30);
            
            var imageBytes = await downloadClient.GetByteArrayAsync(imageUrl, cancellationToken);
            
            _logger.LogDebug("Downloaded {Size} bytes from URL", imageBytes.Length);
            
            return await RemoveBackgroundAsync(imageBytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image from URL: {Url}", imageUrl);
            return BackgroundRemovalResult.Failure(
                $"Failed to download image: {ex.Message}",
                "DOWNLOAD_ERROR");
        }
    }

    /// <summary>
    /// Obtiene información de la cuenta (ClipDrop no tiene endpoint público para esto)
    /// </summary>
    public Task<ProviderAccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        // ClipDrop no expone información de cuenta vía API pública
        // Retornamos información básica
        return Task.FromResult(new ProviderAccountInfo
        {
            IsActive = !string.IsNullOrEmpty(_secrets.ClipDrop.ApiKey),
            AvailableCredits = -1, // Desconocido
            UsedCredits = 0,
            SubscriptionPlan = "ClipDrop API - Check dashboard at https://clipdrop.co/apis"
        });
    }

    #region Private Helper Methods

    private string GetContentType(byte[] imageBytes)
    {
        // Detectar tipo de imagen por magic bytes
        if (imageBytes.Length >= 8)
        {
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return "image/png";
            
            // JPEG: FF D8 FF
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
                return "image/jpeg";
            
            // WebP: RIFF....WEBP
            if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 && imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 && imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
                return "image/webp";
        }
        
        return "image/png"; // Default
    }

    private string GetOutputContentType(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Png => "image/png",
            OutputFormat.WebP => "image/webp",
            OutputFormat.Jpeg => "image/jpeg",
            _ => "image/png"
        };
    }

    private string ParseErrorMessage(string errorBody, System.Net.HttpStatusCode statusCode)
    {
        try
        {
            // Intentar parsear JSON de error
            var jsonDoc = JsonDocument.Parse(errorBody);
            if (jsonDoc.RootElement.TryGetProperty("error", out var errorProp))
            {
                return errorProp.GetString() ?? $"ClipDrop API error: {statusCode}";
            }
            if (jsonDoc.RootElement.TryGetProperty("message", out var messageProp))
            {
                return messageProp.GetString() ?? $"ClipDrop API error: {statusCode}";
            }
        }
        catch
        {
            // Si no es JSON válido, usar el body directamente
        }
        
        return statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Invalid API key",
            System.Net.HttpStatusCode.PaymentRequired => "Insufficient credits. Please top up your ClipDrop account.",
            System.Net.HttpStatusCode.BadRequest => "Invalid request. Check image format and size.",
            System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded. Please try again later.",
            System.Net.HttpStatusCode.InternalServerError => "ClipDrop service error. Please try again.",
            _ => $"ClipDrop API error: {statusCode} - {errorBody}"
        };
    }

    private byte[] ConvertToWebp(byte[] pngBytes)
    {
        // TODO: Implementar conversión real con SixLabors.ImageSharp
        // Por ahora retornamos el PNG original
        _logger.LogWarning("WebP conversion not implemented, returning PNG");
        return pngBytes;
    }

    private byte[] ConvertToJpeg(byte[] pngBytes, string? backgroundColor)
    {
        // TODO: Implementar conversión real con SixLabors.ImageSharp
        // JPEG no soporta transparencia, necesitamos agregar fondo
        _logger.LogWarning("JPEG conversion not implemented, returning PNG");
        return pngBytes;
    }

    #endregion
}
