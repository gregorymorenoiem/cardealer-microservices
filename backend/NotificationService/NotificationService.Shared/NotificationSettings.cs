namespace NotificationService.Shared;

public class NotificationSettings
{
    public SendGridSettings SendGrid { get; set; } = new();
    public ResendSettings Resend { get; set; } = new();
    public TwilioSettings Twilio { get; set; } = new();
    public FirebaseSettings Firebase { get; set; } = new();
    public WhatsAppSettings WhatsApp { get; set; } = new();
    public TeamsSettings Teams { get; set; } = new();
    public SlackSettings Slack { get; set; } = new();
    public string TemplatesPath { get; set; } = "Templates";
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayInSeconds { get; set; } = 60;
    public bool EnableQueueProcessing { get; set; } = true;
    public int QueueBatchSize { get; set; } = 100;
}

public class SendGridSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableTracking { get; set; } = true;
    public string WebhookSecret { get; set; } = string.Empty;
}

public class ResendSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class TwilioSettings
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class FirebaseSettings
{
    public string ServiceAccountKeyPath { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

public class WhatsAppSettings
{
    public bool Enabled { get; set; }
    /// <summary>twilio or meta</summary>
    public string Provider { get; set; } = "twilio";
    public string BusinessNumber { get; set; } = string.Empty;
    public string TwilioWhatsAppNumber { get; set; } = string.Empty;
    public string MetaPhoneNumberId { get; set; } = string.Empty;
    public string MetaAccessToken { get; set; } = string.Empty;
    public string MetaBusinessAccountId { get; set; } = string.Empty;
    public string WelcomeTemplate { get; set; } = "okla_bienvenida";
    public string VerificationTemplate { get; set; } = "okla_verificacion";
    public string PaymentTemplate { get; set; } = "okla_pago_confirmado";
}

public class TeamsSettings
{
    public string WebhookUrl { get; set; } = string.Empty;
}

public class SlackSettings
{
    public string WebhookUrl { get; set; } = string.Empty;
}