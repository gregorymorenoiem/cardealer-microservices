namespace AuthService.Shared;

public class NotificationServiceSettings
{
    /// <summary>Base URL of the Notification Service</summary>
    public string BaseUrl { get; set; } = "http://notificationservice:80"; // Valor por defecto

    /// <summary>Request timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 30; // Valor por defecto

    /// <summary>
    /// Base URL of the Frontend application (used for links in emails).
    /// Override via env var: NotificationService__FrontendBaseUrl
    ///   - Local dev: http://localhost:3000
    ///   - Staging/Production: https://okla.com.do
    /// </summary>
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>Whether to enable notifications via the service</summary>
    public bool EnableNotifications { get; set; } = true; // Valor por defecto
}
