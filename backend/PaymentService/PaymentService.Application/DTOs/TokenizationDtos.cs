using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs;

// ============================================================================
// TOKENIZATION DTOs - Provider-specific tokenization flows
// ============================================================================

/// <summary>
/// Request to initiate tokenization with a specific provider
/// </summary>
public class InitiateTokenizationRequest
{
    /// <summary>
    /// Payment gateway to use for tokenization
    /// </summary>
    public string Gateway { get; set; } = "Azul";

    /// <summary>
    /// URL to return to after tokenization is complete
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL to return to if tokenization is cancelled
    /// </summary>
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Whether to set the new card as default
    /// </summary>
    public bool SetAsDefault { get; set; }

    /// <summary>
    /// Optional nickname for the payment method
    /// </summary>
    public string? NickName { get; set; }
}

/// <summary>
/// Response from initiating tokenization - contains provider-specific data
/// </summary>
public class InitiateTokenizationResponse
{
    /// <summary>
    /// Unique session ID for this tokenization attempt
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gateway being used
    /// </summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>
    /// Type of integration: redirect, iframe, sdk, or popup
    /// </summary>
    public string IntegrationType { get; set; } = "redirect";

    /// <summary>
    /// URL to redirect user to (for redirect/popup integrations)
    /// </summary>
    public string? TokenizationUrl { get; set; }

    /// <summary>
    /// URL for iframe source (for iframe integrations)
    /// </summary>
    public string? IframeUrl { get; set; }

    /// <summary>
    /// SDK configuration for client-side integrations
    /// </summary>
    public SdkConfiguration? SdkConfig { get; set; }

    /// <summary>
    /// Form data to post (for some providers)
    /// </summary>
    public Dictionary<string, string>? FormData { get; set; }

    /// <summary>
    /// Session expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Additional provider-specific data
    /// </summary>
    public Dictionary<string, object>? ProviderData { get; set; }
}

/// <summary>
/// SDK configuration for client-side integrations
/// </summary>
public class SdkConfiguration
{
    /// <summary>
    /// Public key for the SDK
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// Client ID for PayPal/similar
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Merchant ID
    /// </summary>
    public string? MerchantId { get; set; }

    /// <summary>
    /// Environment: sandbox or production
    /// </summary>
    public string Environment { get; set; } = "sandbox";

    /// <summary>
    /// SDK script URL to load
    /// </summary>
    public string? SdkUrl { get; set; }

    /// <summary>
    /// Container element ID for hosted fields
    /// </summary>
    public string? ContainerId { get; set; }

    /// <summary>
    /// Styles for hosted fields
    /// </summary>
    public Dictionary<string, object>? Styles { get; set; }
}

/// <summary>
/// Request to complete tokenization after provider callback
/// </summary>
public class CompleteTokenizationRequest
{
    /// <summary>
    /// Session ID from initiation
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Token returned by the provider
    /// </summary>
    public string? ProviderToken { get; set; }

    /// <summary>
    /// Gateway used (for verification)
    /// </summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>
    /// Set as default payment method
    /// </summary>
    public bool SetAsDefault { get; set; }

    /// <summary>
    /// Provider-specific response data
    /// </summary>
    public Dictionary<string, string>? ProviderResponse { get; set; }

    // ===== Azul-specific fields =====
    public string? AzulOrderId { get; set; }
    public string? AzulDataVaultToken { get; set; }
    public string? AzulResponseCode { get; set; }

    // ===== CardNET-specific fields =====
    public string? CardNetTransactionId { get; set; }
    public string? CardNetToken { get; set; }

    // ===== PixelPay-specific fields =====
    public string? PixelPayTransactionId { get; set; }
    public string? PixelPayToken { get; set; }

    // ===== Fygaro-specific fields =====
    public string? FygaroPaymentId { get; set; }
    public string? FygaroToken { get; set; }

    // ===== PayPal-specific fields =====
    public string? PayPalVaultId { get; set; }
    public string? PayPalPayerId { get; set; }
    public string? BraintreeNonce { get; set; }
}

/// <summary>
/// Stored tokenization session data
/// </summary>
public class TokenizationSession
{
    public string SessionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public PaymentGateway Gateway { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string? CancelUrl { get; set; }
    public bool SetAsDefault { get; set; }
    public string? NickName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Provider-specific session data
    /// </summary>
    public Dictionary<string, string> ProviderData { get; set; } = new();
}

/// <summary>
/// Provider-specific configuration for tokenization
/// </summary>
public class ProviderTokenizationConfig
{
    public string Gateway { get; set; } = string.Empty;
    public string IntegrationType { get; set; } = "redirect";
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool SupportsVaulting { get; set; } = true;
    public string[] SupportedCardBrands { get; set; } = Array.Empty<string>();
    public bool IsTestMode { get; set; }
}
