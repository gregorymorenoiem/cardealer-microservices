namespace PaymentService.Infrastructure.Services.Settings;

/// <summary>
/// Configuration settings for PayPal (International Payment Gateway)
/// Global coverage in 200+ countries with native vault tokenization
/// </summary>
public class PayPalSettings
{
    public bool IsEnabled { get; set; } = false;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.paypal.com";
    public string Mode { get; set; } = "sandbox"; // sandbox or live
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public List<string> SupportedPaymentMethods { get; set; } = new() 
    { 
        "CreditCard", 
        "DebitCard", 
        "PayPalWallet", 
        "TokenizedCard" 
    };
    public decimal CommissionPercentage { get; set; } = 2.9m;
    public decimal FixedCommission { get; set; } = 0.30m;
    public List<string> CurrencySupport { get; set; } = new() 
    { 
        "USD", 
        "EUR", 
        "DOP"
    };
    public string WebhookId { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public bool EnableVault { get; set; } = true;
    public bool EnableSubscriptions { get; set; } = true;

    /// <summary>
    /// Validates if the PayPal provider is properly configured
    /// </summary>
    public bool IsConfigured =>
        IsEnabled &&
        !string.IsNullOrEmpty(ClientId) &&
        !string.IsNullOrEmpty(ClientSecret);

    /// <summary>
    /// Validates all required settings
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (!IsEnabled)
            return (true, string.Empty);

        if (string.IsNullOrEmpty(ClientId))
            return (false, "PayPal ClientId is required");

        if (string.IsNullOrEmpty(ClientSecret))
            return (false, "PayPal ClientSecret is required");

        if (Mode != "sandbox" && Mode != "live")
            return (false, "PayPal Mode must be 'sandbox' or 'live'");

        if (CommissionPercentage < 0 || CommissionPercentage > 100)
            return (false, "CommissionPercentage must be between 0 and 100");

        if (FixedCommission < 0)
            return (false, "FixedCommission cannot be negative");

        if (Mode == "live" && (string.IsNullOrEmpty(WebhookId) || string.IsNullOrEmpty(WebhookSecret)))
            return (false, "WebhookId and WebhookSecret are required in live mode");

        return (true, string.Empty);
    }
}
