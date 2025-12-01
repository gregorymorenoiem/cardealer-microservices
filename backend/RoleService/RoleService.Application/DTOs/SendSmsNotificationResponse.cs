namespace NotificationService.Application.DTOs
{
    public record SendSmsNotificationResponse(Guid NotificationId, bool Success);
}
