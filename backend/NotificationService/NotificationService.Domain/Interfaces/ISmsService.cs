namespace NotificationService.Domain.Interfaces;

// NotificationService.Domain/Interfaces/ISmsService.cs
public interface ISmsService
{
    Task<bool> SendSmsAsync(string to, string message);
}