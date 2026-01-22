using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SpyneIntegrationService.Application.Features.Webhooks.Commands;

namespace SpyneIntegrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IMediator mediator, ILogger<WebhooksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Receive Spyne webhook callback (v2.0 format)
    /// </summary>
    /// <remarks>
    /// This endpoint is called by Spyne when async operations complete.
    /// Triggers: after_ai_done, after_qc_done, after_upload_done
    /// 
    /// Your endpoint should return 2xx to acknowledge. Spyne uses exponential backoff for retries.
    /// </remarks>
    [HttpPost("spyne")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SpyneWebhook()
    {
        string rawBody;
        using (var reader = new StreamReader(Request.Body))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        _logger.LogInformation("Received Spyne webhook v2.0: {Body}", rawBody);

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            
            var payload = JsonSerializer.Deserialize<SpyneWebhookPayloadV2>(rawBody, options);

            if (payload == null)
            {
                _logger.LogWarning("Failed to deserialize Spyne webhook payload");
                return BadRequest("Invalid payload");
            }

            // Log key information
            _logger.LogInformation(
                "Spyne webhook received - DealerVinId: {DealerVinId}, SKU: {SkuId}, Status: {Status}",
                payload.DealerVinId,
                payload.SkuId,
                payload.SkuStatus);

            // Determine event type based on status and media data
            var eventType = DetermineEventType(payload);

            var command = new ProcessSpyneWebhookCommand
            {
                EventType = eventType,
                JobId = payload.DealerVinId ?? payload.SkuId ?? string.Empty,
                Status = payload.SkuStatus ?? "unknown",
                ResultUrl = GetFirstOutputUrl(payload),
                ThumbnailUrl = null,
                EmbedCode = null,
                ErrorMessage = null,
                ProcessingTimeMs = null,
                FileSizeBytes = null,
                DurationSeconds = null,
                RawPayload = rawBody
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _logger.LogInformation("Webhook processed successfully for DealerVinId: {DealerVinId}", payload.DealerVinId);
                return Ok(new { success = true, entityId = result.EntityId });
            }
            else
            {
                _logger.LogWarning("Webhook processing failed: {Message}", result.Message);
                return Ok(new { success = false, message = result.Message }); // Return 200 to prevent retry
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Spyne webhook payload");
            return BadRequest("Invalid JSON");
        }
    }

    private static string DetermineEventType(SpyneWebhookPayloadV2 payload)
    {
        // Check what type of media was processed
        if (payload.MediaData?.Image?.ImageData?.Any() == true)
        {
            var firstImage = payload.MediaData.Image.ImageData.First();
            if (firstImage.AiStatus == "DONE") return "image.completed";
            if (firstImage.AiStatus == "FAILED") return "image.failed";
        }
        
        if (payload.MediaData?.Spin != null)
        {
            if (payload.MediaData.Spin.SpinAiStatus == "DONE") return "spin.completed";
            if (payload.MediaData.Spin.SpinAiStatus == "FAILED") return "spin.failed";
        }
        
        if (payload.MediaData?.Video != null)
        {
            return "video.completed"; // Simplified
        }

        return payload.SkuStatus?.ToLower() switch
        {
            "done" => "processing.completed",
            "failed" => "processing.failed",
            "processing" => "processing.started",
            _ => "unknown"
        };
    }

    private static string? GetFirstOutputUrl(SpyneWebhookPayloadV2 payload)
    {
        // Try to get the first output URL from image data
        if (payload.MediaData?.Image?.ImageData?.Any() == true)
        {
            return payload.MediaData.Image.ImageData.First().OutputUrl;
        }
        
        // Try spin embed URL
        if (!string.IsNullOrEmpty(payload.MediaData?.Spin?.EmbedUrl))
        {
            return payload.MediaData.Spin.EmbedUrl;
        }
        
        // Try video output URL
        if (!string.IsNullOrEmpty(payload.MediaData?.Video?.OutputUrl))
        {
            return payload.MediaData.Video.OutputUrl;
        }

        return null;
    }

    /// <summary>
    /// Health check for webhook endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Test endpoint to simulate a Spyne webhook (for development/testing)
    /// </summary>
    [HttpPost("spyne/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult TestWebhook([FromBody] SpyneWebhookPayloadV2 payload)
    {
        _logger.LogInformation("Test webhook received with DealerVinId: {DealerVinId}", payload.DealerVinId);
        return Ok(new
        {
            received = true,
            dealerVinId = payload.DealerVinId,
            skuId = payload.SkuId,
            status = payload.SkuStatus,
            eventType = DetermineEventType(payload),
            timestamp = DateTime.UtcNow
        });
    }
}

#region Spyne Webhook v2.0 Models

/// <summary>
/// Spyne webhook payload v2.0 format
/// Documentation: https://docs.spyne.ai/docs/explaining-api-and-webhook-response
/// </summary>
public class SpyneWebhookPayloadV2
{
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }
    
    [JsonPropertyName("sku_id")]
    public string? SkuId { get; set; }
    
    [JsonPropertyName("dealer_vin_id")]
    public string? DealerVinId { get; set; }
    
    [JsonPropertyName("sku_status")]
    public string? SkuStatus { get; set; }
    
    [JsonPropertyName("image_data")]
    public List<ImageDataItem>? ImageData { get; set; }
    
    [JsonPropertyName("media_data")]
    public MediaDataV2? MediaData { get; set; }
    
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

public class MediaDataV2
{
    [JsonPropertyName("image")]
    public ImageMediaV2? Image { get; set; }
    
    [JsonPropertyName("spin")]
    public SpinMediaV2? Spin { get; set; }
    
    [JsonPropertyName("video")]
    public VideoMediaV2? Video { get; set; }
}

public class ImageMediaV2
{
    [JsonPropertyName("image_data")]
    public List<ImageDataItem>? ImageData { get; set; }
}

public class ImageDataItem
{
    [JsonPropertyName("image_id")]
    public string? ImageId { get; set; }
    
    [JsonPropertyName("input_url")]
    public string? InputUrl { get; set; }
    
    [JsonPropertyName("output_url")]
    public string? OutputUrl { get; set; }
    
    [JsonPropertyName("ai_status")]
    public string? AiStatus { get; set; }
    
    [JsonPropertyName("qc_status")]
    public string? QcStatus { get; set; }
    
    [JsonPropertyName("category")]
    public string? Category { get; set; }
    
    [JsonPropertyName("view_angle")]
    public string? ViewAngle { get; set; }
    
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

public class SpinMediaV2
{
    [JsonPropertyName("spin_id")]
    public string? SpinId { get; set; }
    
    [JsonPropertyName("embed_url")]
    public string? EmbedUrl { get; set; }
    
    [JsonPropertyName("spin_ai_status")]
    public string? SpinAiStatus { get; set; }
    
    [JsonPropertyName("spin_qc_status")]
    public string? SpinQcStatus { get; set; }
    
    [JsonPropertyName("hotspot_urls")]
    public List<HotspotItem>? HotspotUrls { get; set; }
}

public class HotspotItem
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class VideoMediaV2
{
    [JsonPropertyName("video_id")]
    public string? VideoId { get; set; }
    
    [JsonPropertyName("output_url")]
    public string? OutputUrl { get; set; }
    
    [JsonPropertyName("video_ai_status")]
    public string? VideoAiStatus { get; set; }
    
    [JsonPropertyName("video_qc_status")]
    public string? VideoQcStatus { get; set; }
}

#endregion

// Keep legacy model for backward compatibility
public class SpyneWebhookPayload
{
    public string? EventType { get; set; }
    public string? JobId { get; set; }
    public string? Status { get; set; }
    public string? ResultUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? EmbedCode { get; set; }
    public string? ErrorMessage { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public long? FileSizeBytes { get; set; }
    public int? DurationSeconds { get; set; }
}
