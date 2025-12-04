using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Notification;

/// <summary>
/// Event published when an email notification is requested.
/// This event is consumed by NotificationService to send the email.
/// </summary>
public class EmailNotificationRequestedEvent : EventBase
{
    public override string EventType => "notification.email.requested";

    /// <summary>
    /// Email recipient address
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Whether the body contains HTML content
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// Optional template name for templated emails
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Additional data for template substitution or tracking
    /// </summary>
    public Dictionary<string, string>? Data { get; set; }
}
