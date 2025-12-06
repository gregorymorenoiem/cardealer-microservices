using CarDealer.Shared.MultiTenancy;

namespace IntegrationService.Domain.Entities;

public enum IntegrationType
{
    WhatsApp,
    Facebook,
    Instagram,
    GoogleAds,
    MercadoLibre,
    Slack,
    Email,
    Sms,
    Webhook,
    Api
}

public enum IntegrationStatus
{
    Pending,
    Active,
    Inactive,
    Error,
    Expired
}

public class Integration : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IntegrationType Type { get; private set; }
    public IntegrationStatus Status { get; private set; }

    // Connection settings (encrypted)
    public string? ApiKey { get; private set; }
    public string? ApiSecret { get; private set; }
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }

    public string? WebhookUrl { get; private set; }
    public string? WebhookSecret { get; private set; }

    public string? Configuration { get; private set; } // JSON

    public DateTime? LastSyncAt { get; private set; }
    public string? LastSyncStatus { get; private set; }
    public string? LastError { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    private Integration() { }

    public Integration(
        Guid dealerId,
        string name,
        IntegrationType type,
        Guid createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Type = type;
        CreatedBy = createdBy;
        Description = description;
        Status = IntegrationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetCredentials(string? apiKey, string? apiSecret)
    {
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTokens(string accessToken, string? refreshToken, DateTime? expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetWebhook(string webhookUrl, string? webhookSecret)
    {
        WebhookUrl = webhookUrl;
        WebhookSecret = webhookSecret;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetConfiguration(string configuration)
    {
        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = IntegrationStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = IntegrationStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsError(string errorMessage)
    {
        Status = IntegrationStatus.Error;
        LastError = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSync(bool success, string? statusMessage = null)
    {
        LastSyncAt = DateTime.UtcNow;
        LastSyncStatus = success ? "Success" : "Failed";
        if (!success && !string.IsNullOrEmpty(statusMessage))
            LastError = statusMessage;
        UpdatedAt = DateTime.UtcNow;
    }
}
