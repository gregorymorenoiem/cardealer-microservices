using AuditService.Shared.Enums;
using System.Text.Json.Serialization;

namespace AuditService.Shared.AuditMessages;

/// <summary>
/// Represents an audit event that can be published to the audit service
/// </summary>
public class AuditEvent
{
    /// <summary>
    /// Unique identifier for the audit event
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the user who performed the action
    /// </summary>
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The action performed (e.g., "Login", "Register", "UpdateUser")
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The resource that was affected by the action
    /// </summary>
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the user who performed the action
    /// </summary>
    [JsonPropertyName("userIp")]
    public string UserIp { get; set; } = string.Empty;

    /// <summary>
    /// User agent string from the request
    /// </summary>
    [JsonPropertyName("userAgent")]
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional context data for the audit event
    /// </summary>
    [JsonPropertyName("additionalData")]
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    /// <summary>
    /// Name of the service that generated the audit event
    /// </summary>
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } = "Unknown";

    /// <summary>
    /// Whether the action was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of the operation in milliseconds
    /// </summary>
    [JsonPropertyName("durationMs")]
    public long? DurationMs { get; set; }

    /// <summary>
    /// Correlation ID for tracing requests across services
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }


    /// <summary>
    /// Severity level of the audit event
    /// </summary>
    [JsonPropertyName("severity")]
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;


    /// <summary>
    /// Creates a new instance of AuditEvent
    /// </summary>
    public AuditEvent() { }

    /// <summary>
    /// Creates a new instance of AuditEvent with basic information
    /// </summary>
    public AuditEvent(string userId, string action, string resource, string userIp, string userAgent)
    {
        UserId = userId;
        Action = action;
        Resource = resource;
        UserIp = userIp;
        UserAgent = userAgent;
        ServiceName = "AuthService"; // Default, can be overridden
    }

    /// <summary>
    /// Adds additional data to the audit event
    /// </summary>
    public AuditEvent AddData(string key, object value)
    {
        AdditionalData[key] = value;
        return this;
    }

    /// <summary>
    /// Marks the audit event as failed with an error message
    /// </summary>
    public AuditEvent MarkAsFailed(string errorMessage, Enums.AuditSeverity severity = Enums.AuditSeverity.Error)
    {
        Success = false;
        ErrorMessage = errorMessage;
        Severity = severity;
        return this;
    }

    /// <summary>
    /// Sets the duration of the operation
    /// </summary>
    public AuditEvent SetDuration(long durationMs)
    {
        DurationMs = durationMs;
        return this;
    }

    /// <summary>
    /// Sets the correlation ID for distributed tracing
    /// </summary>
    public AuditEvent SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }

    /// <summary>
    /// Sets the service name that generated the event
    /// </summary>
    public AuditEvent SetServiceName(string serviceName)
    {
        ServiceName = serviceName;
        return this;
    }

    /// <summary>
    /// Sets the severity level of the audit event
    /// </summary>
    public AuditEvent SetSeverity(Enums.AuditSeverity severity)
    {
        Severity = severity;
        return this;
    }
}