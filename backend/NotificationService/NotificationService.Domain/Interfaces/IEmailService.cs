// NotificationService.Domain/Interfaces/IEmailService.cs
namespace NotificationService.Domain.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}
