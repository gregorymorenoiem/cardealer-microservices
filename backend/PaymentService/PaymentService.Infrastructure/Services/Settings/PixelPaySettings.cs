namespace PaymentService.Infrastructure.Services.Settings;

/// <summary>
/// Configuration settings for PixelPay (Fintech) payment gateway - RECOMMENDED FOR OKLA
/// Lowest commissions (1.0%-3.5%) and fastest integration
/// </summary>
public class PixelPaySettings
{
    public bool IsEnabled { get; set; } = true;
    public string PublicKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.pixelpay.com";
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public List<string> SupportedPaymentMethods { get; set; } = new() 
    { 
        "CreditCard", 
        "DebitCard", 
        "MobilePayment", 
        "EWallet" 
    };
    public decimal CommissionPercentage { get; set; } = 2.5m;
    public decimal FixedCommission { get; set; } = 0.15m;
    public List<string> CurrencySupport { get; set; } = new() 
    { 
        "DOP", 
        "USD", 
        "EUR" 
    };
    public bool TestMode { get; set; } = true;
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Validates if the PixelPay provider is properly configured
    /// </summary>
    public bool IsConfigured =>
        IsEnabled &&
        !string.IsNullOrEmpty(PublicKey) &&
        !string.IsNullOrEmpty(SecretKey);

    /// <summary>
    /// Validates all required settings
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (!IsEnabled)
            return (true, string.Empty);

        if (string.IsNullOrEmpty(PublicKey))
            return (false, "PixelPay PublicKey is required");

        if (string.IsNullOrEmpty(SecretKey))
            return (false, "PixelPay SecretKey is required");

        if (CommissionPercentage < 0 || CommissionPercentage > 100)
            return (false, "CommissionPercentage must be between 0 and 100");

        if (FixedCommission < 0)
            return (false, "FixedCommission cannot be negative");

        if (string.IsNullOrEmpty(WebhookSecret) && !TestMode)
            return (false, "WebhookSecret is required in production mode");

        return (true, string.Empty);
    }
}
