namespace AuthService.Shared;

/// <summary>
/// Email service configuration settings
/// </summary>
public class EmailSettings
{
    /// <summary>SMTP server host</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>SMTP server port</summary>
    public int Port { get; set; } = 25;

    /// <summary>Whether to use SSL/TLS</summary>
    public bool UseSsl { get; set; }

    /// <summary>SMTP username</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>SMTP password</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Sender email address</summary>
    public string From { get; set; } = "noreply@example.com";

    /// <summary>Sender display name</summary>
    public string FromName { get; set; } = "Auth Service";

    /// <summary>Email template directory path</summary>
    public string TemplatePath { get; set; } = "EmailTemplates";

    /// <summary>Whether to enable email sending</summary>
    public bool EnableEmailSending { get; set; } = true;
}