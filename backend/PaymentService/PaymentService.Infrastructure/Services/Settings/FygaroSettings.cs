namespace PaymentService.Infrastructure.Services.Settings;

/// <summary>
/// Configuration settings for Fygaro (Agregador) payment gateway
/// Supports multiple payment methods and subscription module
/// </summary>
public class FygaroSettings
{
    public bool IsEnabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.fygaro.com";
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public List<string> SupportedPaymentMethods { get; set; } = new() 
    { 
        "CreditCard", 
        "DebitCard", 
        "TokenizedCard", 
        "Subscription" 
    };
    public decimal CommissionPercentage { get; set; } = 3.0m;
    public decimal FixedCommission { get; set; } = 0m;
    public List<string> CurrencySupport { get; set; } = new() 
    { 
        "DOP", 
        "USD" 
    };
    public bool TestMode { get; set; } = true;
    public bool EnableSubscriptionModule { get; set; } = true;
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// Validates if the Fygaro provider is properly configured
    /// </summary>
    public bool IsConfigured =>
        IsEnabled &&
        !string.IsNullOrEmpty(ApiKey) &&
        !string.IsNullOrEmpty(MerchantId);

    /// <summary>
    /// Validates all required settings
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (!IsEnabled)
            return (true, string.Empty);

        if (string.IsNullOrEmpty(ApiKey))
            return (false, "Fygaro ApiKey is required");

        if (string.IsNullOrEmpty(MerchantId))
            return (false, "Fygaro MerchantId is required");

        if (CommissionPercentage < 0 || CommissionPercentage > 100)
            return (false, "CommissionPercentage must be between 0 and 100");

        if (FixedCommission < 0)
            return (false, "FixedCommission cannot be negative");

        if (string.IsNullOrEmpty(WebhookSecret) && !TestMode)
            return (false, "WebhookSecret is required in production mode");

        return (true, string.Empty);
    }
}
