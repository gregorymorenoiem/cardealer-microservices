
namespace AuthService.Shared.NotificationMessages;

public class NotificationEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty; // "Email", "SMS", "Push"
    public string TemplateName { get; set; } = string.Empty; // "Welcome", "PasswordReset", "EmailVerification"
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High
}
