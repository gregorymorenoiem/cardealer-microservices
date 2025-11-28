using CarDealer.Contracts.Abstractions;

namespace CarDealer.Contracts.Events.Notification;

/// <summary>
/// Event published when a Microsoft Teams alert is successfully sent.
/// This is a specialized notification for critical alerts.
/// </summary>
public class TeamsAlertSentEvent : EventBase
{
    public override string EventType => "notification.teams.alert.sent";
    
    public Guid AlertId { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // Critical, Warning, Info
    public string ServiceName { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
