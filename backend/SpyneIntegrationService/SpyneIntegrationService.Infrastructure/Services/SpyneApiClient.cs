using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpyneIntegrationService.Domain.Entities;
using SpyneIntegrationService.Domain.Enums;
using SpyneIntegrationService.Domain.Interfaces;

namespace SpyneIntegrationService.Infrastructure.Services;

/// <summary>
/// HTTP client for Spyne AI API (Unified Merchandise API)
/// API Documentation: https://docs.spyne.ai/docs/transform-your-first-vehicle-1
/// Base URL: https://api.spyne.ai/api
/// </summary>
public class SpyneApiClient : ISpyneApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpyneApiClient> _logger;
    private readonly SpyneApiClientOptions _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public SpyneApiClient(
        HttpClient httpClient, 
        ILogger<SpyneApiClient> logger,
        IOptions<SpyneApiClientOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = options.Value;
        
        // Configure HttpClient - Spyne Unified API base URL
        _httpClient.BaseAddress = new Uri("https://api.spyne.ai/api/");
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        
        // Spyne uses Bearer Token Authentication (not x-api-key)
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Unified Merchandise API (New v2)

    /// <summary>
    /// Transform vehicle images using the unified merchandise/process API
    /// This is the recommended API for all vehicle image processing
    /// </summary>
    public async Task<SpyneTransformResponse> TransformVehicleAsync(
        SpyneMerchandiseRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing vehicle with VIN/Stock: {Vin}/{Stock}", 
            request.Vin, request.StockNumber);

        var payload = new
        {
            vin = request.Vin,
            stockNumber = request.StockNumber,
            registrationNumber = request.RegistrationNumber,
            dealerId = request.DealerId,
            media = new
            {
                image = request.ProcessImages,
                spin = request.Process360Spin,
                featureVideo = request.ProcessFeatureVideo
            },
            mediaInput = new
            {
                imageData = request.ImageUrls.Select(url => new { url }).ToArray(),
                videoData = request.VideoUrls?.Select(url => new { url }).ToArray()
            },
            processingDetails = new
            {
                backgroundId = request.BackgroundId ?? "20883",
                numberPlateLogo = request.MaskLicensePlate ? "1" : "0",
                bannerId = request.BannerId,
                image = new
                {
                    backgroundType = "legacy",
                    extractCatalogCount = request.ExtractCatalogCount?.ToString() ?? "8"
                },
                spin = request.Process360Spin ? new
                {
                    hotspot = request.EnableHotspots,
                    spinFrameCount = request.SpinFrameCount?.ToString() ?? "72"
                } : null,
                featureVideo = request.ProcessFeatureVideo ? new
                {
                    templateId = request.VideoTemplateId ?? "6758566c41fa6398621c9563",
                    musicId = request.VideoMusicId ?? "66a8e815200257e9dcff1c90",
                    voiceId = request.VideoVoiceId ?? "voice-f22ad31e7bc84888bb96fa8b4db35cd8"
                } : null
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "pv1/merchandise/process",
            payload,
            _jsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Spyne Merchandise API Response: {Response}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Spyne API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
            throw new SpyneApiException($"Spyne API error: {response.StatusCode}", responseContent);
        }

        var result = JsonSerializer.Deserialize<SpyneMerchandiseApiResponse>(responseContent, _jsonOptions);
        
        return new SpyneTransformResponse
        {
            JobId = result?.DealerVinId ?? result?.SkuId ?? Guid.NewGuid().ToString(),
            Status = "processing",
            EstimatedSeconds = 60,
            Message = result?.Message ?? "Processing started"
        };
    }

    /// <summary>
    /// Get the status and results of a processed vehicle
    /// Using correct endpoint: GET /pv1/merchandise?dealerVinId=xxx
    /// </summary>
    public async Task<SpyneMediaResult> GetVehicleMediaAsync(
        string dealerVinId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting media for vehicle: {DealerVinId}", dealerVinId);

        // Correct endpoint: /pv1/merchandise (NOT /get-media)
        var response = await _httpClient.GetAsync(
            $"pv1/merchandise?dealerVinId={dealerVinId}",
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Get media response: {Response}", responseContent);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Get media failed: {StatusCode}", response.StatusCode);
            return new SpyneMediaResult
            {
                DealerVinID = dealerVinId,
                ErrorMessage = $"Failed to get media: {response.StatusCode}"
            };
        }

        var result = JsonSerializer.Deserialize<SpyneMediaResult>(responseContent, _jsonOptions);
        _logger.LogDebug("Parsed result: Status={Status}, HasOutputData={HasData}", 
            result?.Status, result?.OutputData != null);
        return result ?? new SpyneMediaResult { DealerVinID = dealerVinId };
    }

    #endregion

    #region Legacy Image Transformation (Backward Compatibility)

    public async Task<SpyneTransformResponse> TransformImageAsync(
        SpyneTransformRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transforming image: {ImageUrl}", request.ImageUrl);

        // Build Spyne API request body
        var spyneRequest = new
        {
            auth_key = _settings.ApiKey,
            sku_name = request.StockNumber ?? $"okla-{Guid.NewGuid():N}",
            category_id = "Automobile",
            image_data = new[]
            {
                new
                {
                    category = "Exterior",
                    image_urls = new[] { request.ImageUrl }
                }
            },
            background_type = "legacy",
            background = request.BackgroundId,
            number_plate_logo = request.MaskLicensePlate ? "1" : "0",
            webhook = _settings.WebhookUrl
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/pv1/image/replace-bg",
            spyneRequest,
            _jsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Spyne API Response: {Response}", responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Spyne API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Spyne API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<SpyneApiResponse>(responseContent, _jsonOptions);
        
        return new SpyneTransformResponse
        {
            JobId = result?.RequestId ?? Guid.NewGuid().ToString(),
            Status = result?.Status ?? "processing",
            EstimatedSeconds = 30
        };
    }

    public async Task<SpyneTransformResponse> TransformBatchAsync(
        List<SpyneTransformRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Batch transforming {Count} images", requests.Count);

        var imageUrls = requests.Select(r => r.ImageUrl).ToList();
        var firstRequest = requests.First();

        var spyneRequest = new
        {
            auth_key = _settings.ApiKey,
            sku_name = firstRequest.StockNumber ?? $"okla-batch-{Guid.NewGuid():N}",
            category_id = "Automobile",
            image_data = new[]
            {
                new
                {
                    category = "Exterior",
                    image_urls = imageUrls.ToArray()
                }
            },
            background_type = "legacy",
            background = firstRequest.BackgroundId,
            number_plate_logo = firstRequest.MaskLicensePlate ? "1" : "0",
            webhook = _settings.WebhookUrl
        };

        var response = await _httpClient.PostAsJsonAsync(
            "/pv1/image/replace-bg",
            spyneRequest,
            _jsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Spyne Batch API error: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Spyne API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<SpyneApiResponse>(responseContent, _jsonOptions);

        return new SpyneTransformResponse
        {
            JobId = result?.RequestId ?? Guid.NewGuid().ToString(),
            Status = result?.Status ?? "processing",
            EstimatedSeconds = 30 * requests.Count
        };
    }

    public async Task<SpyneJobStatusResponse> GetImageStatusAsync(
        string jobId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting status for image job: {JobId}", jobId);

        var response = await _httpClient.GetAsync(
            $"/pv1/image/status?auth_key={_settings.ApiKey}&request_id={jobId}",
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new SpyneJobStatusResponse
            {
                JobId = jobId,
                Status = "error",
                ErrorMessage = $"Status check failed: {response.StatusCode}"
            };
        }

        var result = JsonSerializer.Deserialize<SpyneStatusResponse>(responseContent, _jsonOptions);

        return new SpyneJobStatusResponse
        {
            JobId = jobId,
            Status = result?.Status ?? "unknown",
            ResultUrl = result?.OutputImages?.FirstOrDefault(),
            ProcessingTimeMs = result?.ProcessingTimeMs
        };
    }

    #endregion

    #region 360 Spin Generation

    public async Task<SpyneSpinResponse> GenerateSpinAsync(
        SpyneSpinRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating 360 spin with {Count} images", request.ImageUrls.Count);

        var response = await _httpClient.PostAsJsonAsync(
            "/v2/spins/generate",
            request,
            _jsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneSpinResponse>(
            _jsonOptions, 
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    public async Task<SpyneJobStatusResponse> GetSpinStatusAsync(
        string jobId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting status for spin job: {JobId}", jobId);

        var response = await _httpClient.GetAsync(
            $"/v2/spins/status/{jobId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneJobStatusResponse>(
            _jsonOptions, 
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    #endregion

    #region Video Generation

    public async Task<SpyneVideoResponse> GenerateVideoAsync(
        SpyneVideoRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating video with {Count} images, style: {Style}", 
            request.ImageUrls.Count, request.Style);

        var response = await _httpClient.PostAsJsonAsync(
            "/v2/videos/generate",
            request,
            _jsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneVideoResponse>(
            _jsonOptions, 
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    public async Task<SpyneJobStatusResponse> GetVideoStatusAsync(
        string jobId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting status for video job: {JobId}", jobId);

        var response = await _httpClient.GetAsync(
            $"/v2/videos/status/{jobId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneJobStatusResponse>(
            _jsonOptions, 
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    #endregion

    #region Chat AI (Fase 4 - Backend only)

    public async Task<SpyneChatInitResponse> InitializeChatAsync(
        SpyneChatRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[FASE 4] Initializing chat session with language: {Language}", request.Language);

        var response = await _httpClient.PostAsJsonAsync(
            "/v2/chat/initialize",
            request,
            _jsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneChatInitResponse>(
            _jsonOptions,
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    public async Task<SpyneChatSessionResponse> StartChatSessionAsync(
        SpyneChatStartRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting chat session");

        var response = await _httpClient.PostAsJsonAsync(
            "/v2/chat/sessions",
            request,
            _jsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneChatSessionResponse>(
            _jsonOptions, 
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    public async Task<SpyneChatMessageResponse> SendChatMessageAsync(
        string sessionToken, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending chat message to session: {SessionToken}", sessionToken);

        var response = await _httpClient.PostAsJsonAsync(
            $"/v2/chat/sessions/{sessionToken}/messages",
            new { Message = message },
            _jsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SpyneChatMessageResponse>(
            _jsonOptions, 
            cancellationToken);

        return result ?? throw new InvalidOperationException("Spyne API returned null response");
    }

    public async Task EndChatSessionAsync(
        string sessionToken, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ending chat session: {SessionToken}", sessionToken);

        var response = await _httpClient.PostAsync(
            $"/v2/chat/sessions/{sessionToken}/end",
            null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Background Presets

    public async Task<List<SpyneBackgroundPreset>> GetBackgroundPresetsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting background presets");

        // Return predefined Spyne backgrounds
        // Note: backgroundId "20883" is confirmed working from Spyne documentation
        // Contact Spyne support to get your enterprise-specific background IDs
        return await Task.FromResult(new List<SpyneBackgroundPreset>
        {
            new() { Id = "20883", Name = "Studio Default", Category = "studio", PreviewUrl = "https://spyne-static.s3.amazonaws.com/console/bg_thumbnails/20883.jpg" },
            new() { Id = "16570", Name = "Clean White", Category = "studio", PreviewUrl = "https://spyne-static.s3.amazonaws.com/console/bg_thumbnails/16570.jpg" },
            // Add more backgrounds as provided by Spyne team for your enterprise
        });
    }

    #endregion
}

#region Spyne API Response Models

/// <summary>
/// Response from Spyne replace-bg API
/// </summary>
public class SpyneApiResponse
{
    public string? RequestId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
    public int? EstimatedTime { get; set; }
}

/// <summary>
/// Response from Spyne status API
/// </summary>
public class SpyneStatusResponse
{
    public string? Status { get; set; }
    public List<string>? OutputImages { get; set; }
    public int? ProcessingTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response from Spyne merchandise/process API (internal use)
/// </summary>
public class SpyneMerchandiseApiResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public SpyneMerchandiseDataResponse? Data { get; set; }
    
    // Fallback for direct properties
    public string? DealerVinId => Data?.DealerVinID;
    public string? SkuId => Data?.StockNumber;
}

public class SpyneMerchandiseDataResponse
{
    public string? StockNumber { get; set; }
    public string? DealerVinID { get; set; }
    public string? Source { get; set; }
    public string? DealerId { get; set; }
    public SpyneValidationSummary? ValidationSummary { get; set; }
}

public class SpyneValidationSummary
{
    public bool IsRequestRejected { get; set; }
    public SpyneDisplayError? DisplayError { get; set; }
}

public class SpyneDisplayError
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public string? Product { get; set; }
}

/// <summary>
/// Custom exception for Spyne API errors
/// </summary>
public class SpyneApiException : Exception
{
    public string? ResponseContent { get; }
    
    public SpyneApiException(string message, string? responseContent = null) 
        : base(message)
    {
        ResponseContent = responseContent;
    }
}

#endregion
