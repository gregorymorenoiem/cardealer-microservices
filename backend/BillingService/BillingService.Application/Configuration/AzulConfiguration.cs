namespace BillingService.Application.Configuration;

/// <summary>
/// Configuration settings for AZUL payment gateway integration
/// </summary>
public class AzulConfiguration
{
    /// <summary>
    /// AZUL merchant identifier
    /// </summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>
    /// Merchant business name
    /// </summary>
    public string MerchantName { get; set; } = string.Empty;

    /// <summary>
    /// Merchant type (E-Commerce)
    /// </summary>
    public string MerchantType { get; set; } = "E-Commerce";

    /// <summary>
    /// Currency code (214 for Dominican Peso)
    /// </summary>
    public string CurrencyCode { get; set; } = "214";

    /// <summary>
    /// Authentication key for hash generation
    /// </summary>
    public string AuthKey { get; set; } = string.Empty;

    /// <summary>
    /// Auth1 header for Webservices API
    /// </summary>
    public string Auth1 { get; set; } = string.Empty;

    /// <summary>
    /// Auth2 header for Webservices API
    /// </summary>
    public string Auth2 { get; set; } = string.Empty;

    /// <summary>
    /// Test environment flag
    /// </summary>
    public bool IsTestEnvironment { get; set; } = true;

    /// <summary>
    /// Payment Page URL (changes based on environment)
    /// </summary>
    public string PaymentPageUrl => IsTestEnvironment
        ? "https://pruebas.azul.com.do/PaymentPage/"
        : "https://pagos.azul.com.do/PaymentPage/";

    /// <summary>
    /// Webservices API URL (changes based on environment)
    /// </summary>
    public string WebservicesUrl => IsTestEnvironment
        ? "https://pruebas.azul.com.do/webservices/"
        : "https://pagos.azul.com.do/webservices/";

    /// <summary>
    /// Callback URL for approved payments
    /// </summary>
    public string ApprovedUrl { get; set; } = string.Empty;

    /// <summary>
    /// Callback URL for declined payments
    /// </summary>
    public string DeclinedUrl { get; set; } = string.Empty;

    /// <summary>
    /// Callback URL for cancelled payments
    /// </summary>
    public string CancelUrl { get; set; } = string.Empty;
}
