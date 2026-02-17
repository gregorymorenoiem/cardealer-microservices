namespace PaymentService.Infrastructure.Services.Settings;

/// <summary>
/// Configuration settings for CardNET (Bancaria RD) payment gateway
/// </summary>
public class CardNETSettings
{
    public bool IsEnabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string TerminalId { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cardnet.com.do";
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public List<string> SupportedPaymentMethods { get; set; } = new() 
    { 
        "CreditCard", 
        "DebitCard", 
        "ACH", 
        "TokenizedCard" 
    };
    public decimal CommissionPercentage { get; set; } = 3.0m;
    public decimal FixedCommission { get; set; } = 0m;
    public List<string> CurrencySupport { get; set; } = new() 
    { 
        "DOP", 
        "USD" 
    };
    public bool TestMode { get; set; } = true;

    /// <summary>
    /// Validates if the CardNET provider is properly configured
    /// </summary>
    public bool IsConfigured =>
        IsEnabled &&
        !string.IsNullOrEmpty(ApiKey) &&
        !string.IsNullOrEmpty(TerminalId);

    /// <summary>
    /// Validates all required settings
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        if (!IsEnabled)
            return (true, string.Empty);

        if (string.IsNullOrEmpty(ApiKey))
            return (false, "CardNET ApiKey is required");

        if (string.IsNullOrEmpty(TerminalId))
            return (false, "CardNET TerminalId is required");

        if (CommissionPercentage < 0 || CommissionPercentage > 100)
            return (false, "CommissionPercentage must be between 0 and 100");

        if (FixedCommission < 0)
            return (false, "FixedCommission cannot be negative");

        return (true, string.Empty);
    }
}
