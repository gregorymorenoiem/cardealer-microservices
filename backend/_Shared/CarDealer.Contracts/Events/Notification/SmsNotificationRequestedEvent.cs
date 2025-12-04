using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Notification;

/// <summary>
/// Event published when an SMS notification is requested.
/// This event is consumed by NotificationService to send the SMS.
/// </summary>
public class SmsNotificationRequestedEvent : EventBase
{
    public override string EventType => "notification.sms.requested";

    /// <summary>
    /// Phone number to send SMS to
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// SMS message content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional data for tracking
    /// </summary>
    public Dictionary<string, string>? Data { get; set; }
}
