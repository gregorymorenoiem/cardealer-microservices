using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Notification;

/// <summary>
/// Event published when a notification is successfully sent.
/// </summary>
public class NotificationSentEvent : EventBase
{
    public override string EventType => "notification.sent";

    public Guid NotificationId { get; set; }
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, Teams
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
