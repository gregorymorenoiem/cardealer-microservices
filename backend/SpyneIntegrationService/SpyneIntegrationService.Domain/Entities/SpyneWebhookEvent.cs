using SpyneIntegrationService.Domain.Enums;

namespace SpyneIntegrationService.Domain.Entities;

/// <summary>
/// Represents a webhook event received from Spyne
/// </summary>
public class SpyneWebhookEvent
{
    public Guid Id { get; set; }
    
    /// <summary>Type of transformation this event relates to</summary>
    public TransformationType TransformationType { get; set; }
    
    /// <summary>ID of the related transformation record</summary>
    public Guid TransformationId { get; set; }
    
    /// <summary>Spyne's job ID</summary>
    public string SpyneJobId { get; set; } = string.Empty;
    
    /// <summary>Event type from Spyne (completed, failed, etc)</summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>Status of the event</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Raw webhook payload</summary>
    public string RawPayload { get; set; } = string.Empty;
    
    /// <summary>Whether event was successfully processed</summary>
    public bool Processed { get; set; }
    
    /// <summary>Alias for Processed</summary>
    public bool IsProcessed 
    { 
        get => Processed; 
        set => Processed = value; 
    }
    
    /// <summary>Whether processing was successful</summary>
    public bool ProcessedSuccessfully { get; set; }
    
    /// <summary>Processing error if any</summary>
    public string? ProcessingError { get; set; }
    
    /// <summary>Number of processing attempts</summary>
    public int ProcessingAttempts { get; set; }
    
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public SpyneWebhookEvent()
    {
        Id = Guid.NewGuid();
        ReceivedAt = DateTime.UtcNow;
    }

    public void MarkAsProcessed()
    {
        Processed = true;
        IsProcessed = true;
        ProcessedSuccessfully = true;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        ProcessingError = error;
        ProcessingAttempts++;
        ProcessedSuccessfully = false;
    }
}
