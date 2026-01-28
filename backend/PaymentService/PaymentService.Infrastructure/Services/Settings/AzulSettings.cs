namespace PaymentService.Infrastructure.Services.Settings;

/// <summary>
/// Configuration settings for AZUL (Banco Popular RD) payment gateway
/// </summary>
public class AzulSettings
{
    public bool IsEnabled { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantName { get; set; } = "OKLA Marketplace";
    public string BaseUrl { get; set; } = "https://api.azul.com.do";
    public string VaultUrl { get; set; } = "https://vault.azul.com.do/";
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public List<string> SupportedPaymentMethods { get; set; } = new() 
    { 
        "CreditCard", 
        "DebitCard", 
        "TokenizedCard" 
    };
    public decimal CommissionPercentage { get; set; } = 3.5m;
    public decimal FixedCommission { get; set; } = 0m;
    public List<string> CurrencySupport { get; set; } = new() 
    { 
        "DOP", 
        "USD" 
    };
    public bool TestMode { get; set; } = true;

    /// <summary>
    /// Validates if the AZUL provider is properly configured
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
            return (false, "AZUL ApiKey is required");

        if (string.IsNullOrEmpty(MerchantId))
            return (false, "AZUL MerchantId is required");

        if (CommissionPercentage < 0 || CommissionPercentage > 100)
            return (false, "CommissionPercentage must be between 0 and 100");

        if (FixedCommission < 0)
            return (false, "FixedCommission cannot be negative");

        return (true, string.Empty);
    }
}
