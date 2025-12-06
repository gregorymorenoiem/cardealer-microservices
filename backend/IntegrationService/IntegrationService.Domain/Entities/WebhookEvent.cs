using CarDealer.Shared.MultiTenancy;

namespace IntegrationService.Domain.Entities;

public enum WebhookEventType
{
    Inbound,
    Outbound
}

public enum WebhookStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Retrying
}

public class WebhookEvent : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public Guid IntegrationId { get; private set; }

    public WebhookEventType EventType { get; private set; }
    public string EventName { get; private set; } = string.Empty;
    public WebhookStatus Status { get; private set; }

    public string Payload { get; private set; } = string.Empty; // JSON
    public string? Headers { get; private set; } // JSON
    public string? Response { get; private set; }

    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public DateTime? NextRetryAt { get; private set; }

    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public string? ErrorMessage { get; private set; }

    // Navigation
    public Integration? Integration { get; private set; }

    private WebhookEvent() { }

    public WebhookEvent(
        Guid dealerId,
        Guid integrationId,
        WebhookEventType eventType,
        string eventName,
        string payload,
        string? headers = null,
        int maxRetries = 3)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            throw new ArgumentException("Event name is required", nameof(eventName));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        IntegrationId = integrationId;
        EventType = eventType;
        EventName = eventName;
        Payload = payload;
        Headers = headers;
        MaxRetries = maxRetries;
        Status = WebhookStatus.Pending;
        ReceivedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        Status = WebhookStatus.Processing;
    }

    public void Complete(string? response = null)
    {
        Status = WebhookStatus.Completed;
        Response = response;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        ErrorMessage = errorMessage;

        if (RetryCount < MaxRetries)
        {
            Status = WebhookStatus.Retrying;
            RetryCount++;
            NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, RetryCount));
        }
        else
        {
            Status = WebhookStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
        }
    }

    public void Retry()
    {
        Status = WebhookStatus.Pending;
        NextRetryAt = null;
    }
}
