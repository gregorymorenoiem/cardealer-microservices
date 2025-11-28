
namespace AuthService.Shared.NotificationMessages;


public class EmailNotificationEvent : NotificationEvent
{
    public EmailNotificationEvent()
    {
        Type = "Email";
    }

    public bool IsHtml { get; set; } = true;
    public List<string> CC { get; set; } = new();
    public List<string> BCC { get; set; } = new();
}