using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using System.Text.Json;

namespace PaymentService.Application.Services;

/// <summary>
/// Service for handling card tokenization with various payment providers
/// Each provider has its own secure tokenization page/SDK
/// </summary>
public interface ITokenizationService
{
    /// <summary>
    /// Initiate tokenization session for a specific provider
    /// </summary>
    Task<InitiateTokenizationResponse> InitiateAsync(
        Guid userId,
        InitiateTokenizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete tokenization and return the token
    /// </summary>
    Task<(string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName)> CompleteAsync(
        Guid userId,
        CompleteTokenizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get session data
    /// </summary>
    Task<TokenizationSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provider configuration
    /// </summary>
    ProviderTokenizationConfig GetProviderConfig(PaymentGateway gateway);
}

public class TokenizationService : ITokenizationService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<TokenizationService> _logger;

    // Session TTL - 30 minutes
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(30);

    public TokenizationService(
        IDistributedCache cache,
        IConfiguration config,
        ILogger<TokenizationService> logger)
    {
        _cache = cache;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Helper to get config value from multiple keys, handling empty strings as null
    /// </summary>
    private string? GetConfigValue(params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = _config[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    public async Task<InitiateTokenizationResponse> InitiateAsync(
        Guid userId,
        InitiateTokenizationRequest request,
        CancellationToken cancellationToken = default)
    {
        var gateway = ParseGateway(request.Gateway);
        var sessionId = GenerateSessionId();
        var expiresAt = DateTime.UtcNow.Add(SessionTtl);

        // Create session
        var session = new TokenizationSession
        {
            SessionId = sessionId,
            UserId = userId,
            Gateway = gateway,
            ReturnUrl = request.ReturnUrl,
            CancelUrl = request.CancelUrl,
            SetAsDefault = request.SetAsDefault,
            NickName = request.NickName,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsCompleted = false
        };

        // Build provider-specific response
        var response = gateway switch
        {
            PaymentGateway.Azul => await BuildAzulResponseAsync(session, cancellationToken),
            PaymentGateway.CardNET => await BuildCardNetResponseAsync(session, cancellationToken),
            PaymentGateway.PixelPay => await BuildPixelPayResponseAsync(session, cancellationToken),
            PaymentGateway.Fygaro => await BuildFygaroResponseAsync(session, cancellationToken),
            PaymentGateway.PayPal => await BuildPayPalResponseAsync(session, cancellationToken),
            _ => throw new ArgumentException($"Unsupported gateway: {gateway}")
        };

        // Store session in cache
        var sessionJson = JsonSerializer.Serialize(session);
        await _cache.SetStringAsync(
            GetSessionKey(sessionId),
            sessionJson,
            new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt },
            cancellationToken);

        _logger.LogInformation(
            "Initiated tokenization session {SessionId} for user {UserId} with gateway {Gateway}",
            sessionId, userId, gateway);

        return response;
    }

    public async Task<(string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName)> CompleteAsync(
        Guid userId,
        CompleteTokenizationRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get session
        var session = await GetSessionAsync(request.SessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException("Tokenization session not found or expired");
        }

        if (session.UserId != userId)
        {
            throw new UnauthorizedAccessException("Session does not belong to this user");
        }

        if (session.IsCompleted)
        {
            throw new InvalidOperationException("Session has already been completed");
        }

        // Process based on provider
        var result = session.Gateway switch
        {
            PaymentGateway.Azul => ProcessAzulCallback(request),
            PaymentGateway.CardNET => ProcessCardNetCallback(request),
            PaymentGateway.PixelPay => ProcessPixelPayCallback(request),
            PaymentGateway.Fygaro => ProcessFygaroCallback(request),
            PaymentGateway.PayPal => ProcessPayPalCallback(request),
            _ => throw new ArgumentException($"Unsupported gateway: {session.Gateway}")
        };

        // Mark session as completed
        session.IsCompleted = true;
        var sessionJson = JsonSerializer.Serialize(session);
        await _cache.SetStringAsync(
            GetSessionKey(request.SessionId),
            sessionJson,
            new DistributedCacheEntryOptions { AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5) },
            cancellationToken);

        _logger.LogInformation(
            "Completed tokenization session {SessionId} for user {UserId}",
            request.SessionId, userId);

        return result;
    }

    public async Task<TokenizationSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var sessionJson = await _cache.GetStringAsync(GetSessionKey(sessionId), cancellationToken);
        if (string.IsNullOrEmpty(sessionJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<TokenizationSession>(sessionJson);
    }

    public ProviderTokenizationConfig GetProviderConfig(PaymentGateway gateway)
    {
        var isTestMode = _config.GetValue<bool>("PaymentService:TestMode", true);

        return gateway switch
        {
            PaymentGateway.Azul => new ProviderTokenizationConfig
            {
                Gateway = "Azul",
                IntegrationType = "redirect",
                DisplayName = "Azul WebPay",
                Description = "Página segura de Azul con Cybersource tokenization",
                SupportsVaulting = true,
                SupportedCardBrands = new[] { "Visa", "Mastercard", "Discover", "Amex" },
                IsTestMode = isTestMode
            },
            PaymentGateway.CardNET => new ProviderTokenizationConfig
            {
                Gateway = "CardNET",
                IntegrationType = "redirect",
                DisplayName = "CardNET Secure Page",
                Description = "Página segura de CardNET para tarjetas dominicanas",
                SupportsVaulting = true,
                SupportedCardBrands = new[] { "Visa", "Mastercard" },
                IsTestMode = isTestMode
            },
            PaymentGateway.PixelPay => new ProviderTokenizationConfig
            {
                Gateway = "PixelPay",
                IntegrationType = "iframe",
                DisplayName = "PixelPay Checkout",
                Description = "Checkout seguro embebido de PixelPay",
                SupportsVaulting = true,
                SupportedCardBrands = new[] { "Visa", "Mastercard" },
                IsTestMode = isTestMode
            },
            PaymentGateway.Fygaro => new ProviderTokenizationConfig
            {
                Gateway = "Fygaro",
                IntegrationType = "redirect",
                DisplayName = "Fygaro Hosted Checkout",
                Description = "Checkout hosted de Fygaro",
                SupportsVaulting = true,
                SupportedCardBrands = new[] { "Visa", "Mastercard" },
                IsTestMode = isTestMode
            },
            PaymentGateway.PayPal => new ProviderTokenizationConfig
            {
                Gateway = "PayPal",
                IntegrationType = "sdk",
                DisplayName = "PayPal / Braintree",
                Description = "Vault API con Braintree SDK",
                SupportsVaulting = true,
                SupportedCardBrands = new[] { "Visa", "Mastercard", "Amex", "Discover", "PayPal" },
                IsTestMode = isTestMode
            },
            _ => throw new ArgumentException($"Unknown gateway: {gateway}")
        };
    }

    // ===== Provider-specific response builders =====

    private Task<InitiateTokenizationResponse> BuildAzulResponseAsync(
        TokenizationSession session,
        CancellationToken cancellationToken)
    {
        // Get MerchantId from multiple possible config locations, with proper empty string handling
        var merchantId = GetConfigValue("Azul:MerchantId", "PaymentGateway:Azul:MerchantId") ?? "39038540035";
        var baseUrl = GetConfigValue("Azul:WebPayUrl", "Azul:ECommerceUrl") ?? "https://pruebas.azul.com.do/webpay";
        var isTestMode = _config.GetValue<bool>("Azul:TestMode", true);

        // Build Azul WebPay URL with DataVault tokenization
        var tokenizationUrl = $"{baseUrl}/tokenization" +
            $"?merchantId={merchantId}" +
            $"&sessionId={session.SessionId}" +
            $"&returnUrl={Uri.EscapeDataString(session.ReturnUrl)}" +
            $"&cancelUrl={Uri.EscapeDataString(session.CancelUrl ?? session.ReturnUrl)}";

        session.ProviderData["MerchantId"] = merchantId;

        return Task.FromResult(new InitiateTokenizationResponse
        {
            SessionId = session.SessionId,
            Gateway = "Azul",
            IntegrationType = "redirect",
            TokenizationUrl = tokenizationUrl,
            ExpiresAt = session.ExpiresAt,
            ProviderData = new Dictionary<string, object>
            {
                ["merchantId"] = merchantId,
                ["testMode"] = isTestMode,
                ["dataVaultEnabled"] = true
            }
        });
    }

    private Task<InitiateTokenizationResponse> BuildCardNetResponseAsync(
        TokenizationSession session,
        CancellationToken cancellationToken)
    {
        var merchantId = GetConfigValue("CardNET:MerchantId", "PaymentGateway:CardNET:MerchantId") ?? "TEST_CARDNET_MERCHANT";
        var baseUrl = GetConfigValue("CardNET:SecurePageUrl") ?? "https://lab.cardnet.com.do/securepage";
        var isTestMode = _config.GetValue<bool>("CardNET:TestMode", true);

        var tokenizationUrl = $"{baseUrl}/tokenize" +
            $"?mid={merchantId}" +
            $"&session={session.SessionId}" +
            $"&redirect={Uri.EscapeDataString(session.ReturnUrl)}";

        session.ProviderData["MerchantId"] = merchantId;

        return Task.FromResult(new InitiateTokenizationResponse
        {
            SessionId = session.SessionId,
            Gateway = "CardNET",
            IntegrationType = "redirect",
            TokenizationUrl = tokenizationUrl,
            ExpiresAt = session.ExpiresAt,
            ProviderData = new Dictionary<string, object>
            {
                ["merchantId"] = merchantId,
                ["testMode"] = isTestMode
            }
        });
    }

    private Task<InitiateTokenizationResponse> BuildPixelPayResponseAsync(
        TokenizationSession session,
        CancellationToken cancellationToken)
    {
        var merchantKey = GetConfigValue("PixelPay:MerchantKey", "PaymentGateway:PixelPay:ApiKey") ?? "TEST_PIXELPAY_KEY";
        var baseUrl = GetConfigValue("PixelPay:CheckoutUrl") ?? "https://checkout.pixel-pay.com";
        var isTestMode = _config.GetValue<bool>("PixelPay:TestMode", true);

        // PixelPay uses iframe integration
        var iframeUrl = $"{baseUrl}/v2/iframe" +
            $"?key={merchantKey}" +
            $"&sessionId={session.SessionId}" +
            $"&mode=tokenize" +
            $"&callback={Uri.EscapeDataString(session.ReturnUrl)}";

        session.ProviderData["MerchantKey"] = merchantKey;

        return Task.FromResult(new InitiateTokenizationResponse
        {
            SessionId = session.SessionId,
            Gateway = "PixelPay",
            IntegrationType = "iframe",
            IframeUrl = iframeUrl,
            ExpiresAt = session.ExpiresAt,
            ProviderData = new Dictionary<string, object>
            {
                ["merchantKey"] = merchantKey,
                ["testMode"] = isTestMode,
                ["iframeHeight"] = 400,
                ["iframeStyles"] = new Dictionary<string, string>
                {
                    ["border"] = "none",
                    ["width"] = "100%"
                }
            }
        });
    }

    private Task<InitiateTokenizationResponse> BuildFygaroResponseAsync(
        TokenizationSession session,
        CancellationToken cancellationToken)
    {
        var merchantId = GetConfigValue("Fygaro:MerchantId", "PaymentGateway:Fygaro:MerchantId") ?? "TEST_FYGARO_MERCHANT";
        var apiKey = GetConfigValue("Fygaro:ApiKey", "PaymentGateway:Fygaro:ApiKey") ?? "TEST_FYGARO_KEY";
        var baseUrl = GetConfigValue("Fygaro:CheckoutUrl") ?? "https://checkout.fygaro.com";
        var isTestMode = _config.GetValue<bool>("Fygaro:TestMode", true);

        var tokenizationUrl = $"{baseUrl}/hosted/card" +
            $"?merchant={merchantId}" +
            $"&session={session.SessionId}" +
            $"&vault=true" +
            $"&success_url={Uri.EscapeDataString(session.ReturnUrl)}" +
            $"&cancel_url={Uri.EscapeDataString(session.CancelUrl ?? session.ReturnUrl)}";

        session.ProviderData["MerchantId"] = merchantId;

        return Task.FromResult(new InitiateTokenizationResponse
        {
            SessionId = session.SessionId,
            Gateway = "Fygaro",
            IntegrationType = "redirect",
            TokenizationUrl = tokenizationUrl,
            ExpiresAt = session.ExpiresAt,
            ProviderData = new Dictionary<string, object>
            {
                ["merchantId"] = merchantId,
                ["testMode"] = isTestMode
            }
        });
    }

    private Task<InitiateTokenizationResponse> BuildPayPalResponseAsync(
        TokenizationSession session,
        CancellationToken cancellationToken)
    {
        var clientId = GetConfigValue("PayPal:ClientId", "PaymentGateway:PayPal:ClientId") ?? "TEST_PAYPAL_CLIENT";
        var merchantId = GetConfigValue("PayPal:MerchantId", "PaymentGateway:PayPal:MerchantId");
        var isTestMode = _config.GetValue<bool>("PayPal:TestMode", true);
        var environment = isTestMode ? "sandbox" : "production";

        // PayPal uses Braintree SDK for card vaulting
        var braintreeEnv = isTestMode ? "sandbox" : "production";
        var sdkUrl = $"https://js.braintreegateway.com/web/3.94.0/js/client.min.js";

        return Task.FromResult(new InitiateTokenizationResponse
        {
            SessionId = session.SessionId,
            Gateway = "PayPal",
            IntegrationType = "sdk",
            ExpiresAt = session.ExpiresAt,
            SdkConfig = new SdkConfiguration
            {
                ClientId = clientId,
                MerchantId = merchantId,
                Environment = environment,
                SdkUrl = sdkUrl,
                ContainerId = "paypal-card-form",
                Styles = new Dictionary<string, object>
                {
                    ["input"] = new { fontSize = "16px", fontFamily = "system-ui" },
                    ["input.invalid"] = new { color = "red" },
                    [":focus"] = new { color = "black" }
                }
            },
            ProviderData = new Dictionary<string, object>
            {
                ["clientId"] = clientId,
                ["testMode"] = isTestMode,
                ["braintreeEnvironment"] = braintreeEnv,
                ["vaultFlow"] = true
            }
        });
    }

    // ===== Provider-specific callback processors =====

    private (string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName) ProcessAzulCallback(
        CompleteTokenizationRequest request)
    {
        // Azul returns DataVault token via redirect params
        var token = request.AzulDataVaultToken ?? request.ProviderToken ?? "";
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Azul DataVault token not received");
        }

        // In production, we would call Azul API to get card details
        // For now, return placeholder data that would come from the callback
        return (
            Token: token,
            CardBrand: request.ProviderResponse?.GetValueOrDefault("cardBrand") ?? "Visa",
            CardLast4: request.ProviderResponse?.GetValueOrDefault("cardLast4") ?? "****",
            ExpMonth: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expMonth"), out var em) ? em : 12,
            ExpYear: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expYear"), out var ey) ? ey : 2030,
            CardHolderName: request.ProviderResponse?.GetValueOrDefault("cardHolderName")
        );
    }

    private (string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName) ProcessCardNetCallback(
        CompleteTokenizationRequest request)
    {
        var token = request.CardNetToken ?? request.ProviderToken ?? "";
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("CardNET token not received");
        }

        return (
            Token: token,
            CardBrand: request.ProviderResponse?.GetValueOrDefault("cardBrand") ?? "Visa",
            CardLast4: request.ProviderResponse?.GetValueOrDefault("cardLast4") ?? "****",
            ExpMonth: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expMonth"), out var em) ? em : 12,
            ExpYear: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expYear"), out var ey) ? ey : 2030,
            CardHolderName: request.ProviderResponse?.GetValueOrDefault("cardHolderName")
        );
    }

    private (string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName) ProcessPixelPayCallback(
        CompleteTokenizationRequest request)
    {
        var token = request.PixelPayToken ?? request.ProviderToken ?? "";
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("PixelPay token not received");
        }

        return (
            Token: token,
            CardBrand: request.ProviderResponse?.GetValueOrDefault("cardBrand") ?? "Visa",
            CardLast4: request.ProviderResponse?.GetValueOrDefault("cardLast4") ?? "****",
            ExpMonth: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expMonth"), out var em) ? em : 12,
            ExpYear: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expYear"), out var ey) ? ey : 2030,
            CardHolderName: request.ProviderResponse?.GetValueOrDefault("cardHolderName")
        );
    }

    private (string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName) ProcessFygaroCallback(
        CompleteTokenizationRequest request)
    {
        var token = request.FygaroToken ?? request.ProviderToken ?? "";
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Fygaro token not received");
        }

        return (
            Token: token,
            CardBrand: request.ProviderResponse?.GetValueOrDefault("cardBrand") ?? "Visa",
            CardLast4: request.ProviderResponse?.GetValueOrDefault("cardLast4") ?? "****",
            ExpMonth: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expMonth"), out var em) ? em : 12,
            ExpYear: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expYear"), out var ey) ? ey : 2030,
            CardHolderName: request.ProviderResponse?.GetValueOrDefault("cardHolderName")
        );
    }

    private (string Token, string CardBrand, string CardLast4, int ExpMonth, int ExpYear, string? CardHolderName) ProcessPayPalCallback(
        CompleteTokenizationRequest request)
    {
        // PayPal/Braintree returns nonce or vault ID
        var token = request.PayPalVaultId ?? request.BraintreeNonce ?? request.ProviderToken ?? "";
        
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("PayPal/Braintree token not received");
        }

        return (
            Token: token,
            CardBrand: request.ProviderResponse?.GetValueOrDefault("cardBrand") ?? "PayPal",
            CardLast4: request.ProviderResponse?.GetValueOrDefault("cardLast4") ?? "****",
            ExpMonth: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expMonth"), out var em) ? em : 12,
            ExpYear: int.TryParse(request.ProviderResponse?.GetValueOrDefault("expYear"), out var ey) ? ey : 2030,
            CardHolderName: request.ProviderResponse?.GetValueOrDefault("cardHolderName")
        );
    }

    // ===== Helpers =====

    private static string GenerateSessionId()
    {
        return $"tok_{Guid.NewGuid():N}";
    }

    private static string GetSessionKey(string sessionId)
    {
        return $"tokenization:{sessionId}";
    }

    private static PaymentGateway ParseGateway(string gateway)
    {
        return Enum.TryParse<PaymentGateway>(gateway, true, out var result)
            ? result
            : PaymentGateway.Azul;
    }
}
