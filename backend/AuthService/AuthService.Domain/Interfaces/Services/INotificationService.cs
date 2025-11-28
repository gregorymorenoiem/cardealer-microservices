// AuthService.Domain/Interfaces/Services/INotificationService.cs
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Services;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendSmsAsync(string to, string message);
    Task SendPushAsync(string deviceToken, string title, string body);
    Task<bool> IsHealthyAsync();
}

