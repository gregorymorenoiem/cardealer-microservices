namespace NotificationService.Application.DTOs
{
    public record SendPushNotificationResponse(Guid NotificationId, bool Success);
}
