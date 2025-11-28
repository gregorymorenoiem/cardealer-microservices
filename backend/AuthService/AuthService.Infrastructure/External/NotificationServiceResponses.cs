namespace AuthService.Infrastructure.External;

public class EmailNotificationResponse
{
    public Guid NotificationId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
}

public class SmsNotificationResponse
{
    public Guid NotificationId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
}

public class PushNotificationResponse
{
    public Guid NotificationId { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
}

public class NotificationStatusResponse
{
    public Guid NotificationId { get; set; }
    public string? Status { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
