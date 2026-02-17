namespace NotificationService.Domain.Enums;

public enum NotificationProvider
{
    SendGrid = 1,
    Twilio = 2,
    Firebase = 3,
    Custom = 4,
    TwilioWhatsApp = 5,
    MetaWhatsApp = 6,
    Slack = 7,
    Teams = 8,
    Resend = 9
}