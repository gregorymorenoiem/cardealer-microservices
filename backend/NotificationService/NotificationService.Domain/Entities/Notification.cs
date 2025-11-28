using NotificationService.Domain.Enums;
using System.Text.Json;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public NotificationProvider Provider { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TemplateName { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }

    // Constructor
    public Notification()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = NotificationStatus.Pending;
        Priority = PriorityLevel.Medium;
        RetryCount = 0;
        Metadata = new Dictionary<string, object>();
    }

    // Factory methods
    public static Notification CreateEmailNotification(string to, string subject, string body, 
        NotificationProvider provider = NotificationProvider.SendGrid, 
        PriorityLevel priority = PriorityLevel.Medium,
        Dictionary<string, object>? metadata = null)
    {
        return new Notification
        {
            Type = NotificationType.Email,
            Recipient = to,
            Subject = subject,
            Content = body,
            Provider = provider,
            Priority = priority,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    public static Notification CreateSmsNotification(string to, string message,
        NotificationProvider provider = NotificationProvider.Twilio,
        PriorityLevel priority = PriorityLevel.Medium,
        Dictionary<string, object>? metadata = null)
    {
        return new Notification
        {
            Type = NotificationType.Sms,
            Recipient = to,
            Content = message,
            Provider = provider,
            Priority = priority,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    public static Notification CreatePushNotification(string deviceToken, string title, string body,
        NotificationProvider provider = NotificationProvider.Firebase,
        PriorityLevel priority = PriorityLevel.Medium,
        Dictionary<string, object>? metadata = null)
    {
        return new Notification
        {
            Type = NotificationType.Push,
            Recipient = deviceToken,
            Subject = title,
            Content = body,
            Provider = provider,
            Priority = priority,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    // Business methods
    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
    }

    public void MarkAsDelivered()
    {
        Status = NotificationStatus.Delivered;
    }

    public bool CanRetry()
    {
        return Status == NotificationStatus.Failed && RetryCount < 3;
    }

    public void UpdateMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }
}