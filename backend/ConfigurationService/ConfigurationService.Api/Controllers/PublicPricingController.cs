using ConfigurationService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

/// <summary>
/// Public pricing endpoint - No authentication required.
/// Returns platform pricing configuration for use by checkout, landing pages, etc.
/// NOTE: individual_listing_price is now under the "pricing" category (not billing).
/// Billing category is reserved for payment provider configuration (Azul, Stripe).
/// </summary>
[ApiController]
[Route("api/public/pricing")]
public class PublicPricingController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicPricingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all platform pricing configuration (public, no auth required).
    /// All pricing/commission values come from the "pricing" category.
    /// Only billing.stripe_trial_days is read from billing (it affects pricing display).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPricing([FromQuery] string environment = "Development")
    {
        // Get pricing category configs (includes individual_listing_price now)
        var pricingQuery = new GetConfigurationsByCategoryQuery("pricing", environment, null);
        var pricingConfigs = await _mediator.Send(pricingQuery);

        // Get only trial days from billing (needed for pricing display)
        var billingQuery = new GetConfigurationsByCategoryQuery("billing", environment, null);
        var billingConfigs = await _mediator.Send(billingQuery);

        // Build a clean pricing response with only key-value pairs
        var pricing = new Dictionary<string, string>();
        foreach (var config in pricingConfigs)
        {
            var shortKey = config.Key.Replace("pricing.", "");
            pricing[shortKey] = config.Value;
        }

        // Extract only trial days from billing (not provider secrets)
        var trialDays = billingConfigs
            .FirstOrDefault(c => c.Key == "billing.stripe_trial_days")?.Value ?? "14";

        return Ok(new PlatformPricingResponse
        {
            // Publicaciones
            BasicListing = ParseDecimal(pricing.GetValueOrDefault("basic_listing", "0")),
            FeaturedListing = ParseDecimal(pricing.GetValueOrDefault("featured_listing", "1499")),
            PremiumListing = ParseDecimal(pricing.GetValueOrDefault("premium_listing", "2999")),
            SellerPremiumPrice = ParseDecimal(pricing.GetValueOrDefault("seller_premium_price", "1699")),
            IndividualListingPrice = ParseDecimal(pricing.GetValueOrDefault("individual_listing_price", "1699")),
            // Planes Dealer
            DealerStarter = ParseDecimal(pricing.GetValueOrDefault("dealer_starter", "2899")),
            DealerPro = ParseDecimal(pricing.GetValueOrDefault("dealer_pro", "7499")),
            DealerEnterprise = ParseDecimal(pricing.GetValueOrDefault("dealer_enterprise", "17499")),
            // Boosts
            BoostBasicPrice = ParseDecimal(pricing.GetValueOrDefault("boost_basic_price", "499")),
            BoostBasicDays = (int)ParseDecimal(pricing.GetValueOrDefault("boost_basic_days", "3")),
            BoostProPrice = ParseDecimal(pricing.GetValueOrDefault("boost_pro_price", "999")),
            BoostProDays = (int)ParseDecimal(pricing.GetValueOrDefault("boost_pro_days", "7")),
            BoostPremiumPrice = ParseDecimal(pricing.GetValueOrDefault("boost_premium_price", "1999")),
            BoostPremiumDays = (int)ParseDecimal(pricing.GetValueOrDefault("boost_premium_days", "14")),
            // Duraciones
            BasicListingDays = (int)ParseDecimal(pricing.GetValueOrDefault("basic_listing_days", "30")),
            IndividualListingDays = (int)ParseDecimal(pricing.GetValueOrDefault("individual_listing_days", "45")),
            // Límites por plan
            StarterMaxVehicles = (int)ParseDecimal(pricing.GetValueOrDefault("starter_max_vehicles", "20")),
            ProMaxVehicles = (int)ParseDecimal(pricing.GetValueOrDefault("pro_max_vehicles", "75")),
            FreeMaxPhotos = (int)ParseDecimal(pricing.GetValueOrDefault("free_max_photos", "10")),
            StarterMaxPhotos = (int)ParseDecimal(pricing.GetValueOrDefault("starter_max_photos", "25")),
            ProMaxPhotos = (int)ParseDecimal(pricing.GetValueOrDefault("pro_max_photos", "40")),
            EnterpriseMaxPhotos = (int)ParseDecimal(pricing.GetValueOrDefault("enterprise_max_photos", "50")),
            // Comisiones
            PlatformCommission = ParseDecimal(pricing.GetValueOrDefault("platform_commission", "2.5")),
            ItbisPercentage = ParseDecimal(pricing.GetValueOrDefault("itbis_percentage", "18")),
            Currency = pricing.GetValueOrDefault("currency", "DOP"),
            // Early Bird
            EarlyBirdDiscount = ParseDecimal(pricing.GetValueOrDefault("early_bird_discount", "25")),
            EarlyBirdDeadline = pricing.GetValueOrDefault("early_bird_deadline", "2026-12-31"),
            EarlyBirdFreeMonths = (int)ParseDecimal(pricing.GetValueOrDefault("early_bird_free_months", "2")),
            // Trial (from billing config)
            StripeTrialDays = (int)ParseDecimal(trialDays),
        });
    }

    /// <summary>
    /// Get payment provider configuration (admin only).
    /// Returns non-secret billing config for admin dashboard display.
    /// Secrets (API keys, etc.) are managed via SecretsController.
    /// </summary>
    [HttpGet("providers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPaymentProviders([FromQuery] string environment = "Development")
    {
        var billingQuery = new GetConfigurationsByCategoryQuery("billing", environment, null);
        var billingConfigs = await _mediator.Send(billingQuery);

        var billing = new Dictionary<string, string>();
        foreach (var config in billingConfigs)
        {
            var shortKey = config.Key.Replace("billing.", "");
            billing[shortKey] = config.Value;
        }

        return Ok(new PaymentProvidersResponse
        {
            StripeTrialDays = (int)ParseDecimal(billing.GetValueOrDefault("stripe_trial_days", "14")),
            // Azul (Banco Popular RD)
            AzulEnabled = billing.GetValueOrDefault("azul_enabled", "true") == "true",
            AzulEnvironment = billing.GetValueOrDefault("azul_environment", "Test"),
            AzulMerchantName = billing.GetValueOrDefault("azul_merchant_name", "OKLA Marketplace"),
            AzulMerchantType = billing.GetValueOrDefault("azul_merchant_type", "eCommerce"),
            AzulCurrencyCode = billing.GetValueOrDefault("azul_currency_code", "$"),
            // CardNET (Bancaria RD)
            CardnetEnabled = billing.GetValueOrDefault("cardnet_enabled", "false") == "true",
            CardnetEnvironment = billing.GetValueOrDefault("cardnet_environment", "Test"),
            // PixelPay (Fintech)
            PixelpayEnabled = billing.GetValueOrDefault("pixelpay_enabled", "true") == "true",
            PixelpayEnvironment = billing.GetValueOrDefault("pixelpay_environment", "Test"),
            // Fygaro (Agregador)
            FygaroEnabled = billing.GetValueOrDefault("fygaro_enabled", "false") == "true",
            FygaroEnvironment = billing.GetValueOrDefault("fygaro_environment", "Test"),
            FygaroEnableSubscriptions = billing.GetValueOrDefault("fygaro_enable_subscriptions", "true") == "true",
            // Stripe (Internacional + Suscripciones)
            StripeEnabled = billing.GetValueOrDefault("stripe_enabled", "true") == "true",
            StripeEnvironment = billing.GetValueOrDefault("stripe_environment", "Test"),
            // PayPal (Internacional)
            PaypalEnabled = billing.GetValueOrDefault("paypal_enabled", "false") == "true",
            PaypalEnvironment = billing.GetValueOrDefault("paypal_environment", "sandbox"),
            // General billing
            InvoicePrefix = billing.GetValueOrDefault("invoice_prefix", "OKLA-INV"),
            InvoiceNcfEnabled = billing.GetValueOrDefault("invoice_ncf_enabled", "false") == "true",
            AutoRetryFailedPayments = billing.GetValueOrDefault("auto_retry_failed_payments", "true") == "true",
            MaxPaymentRetries = (int)ParseDecimal(billing.GetValueOrDefault("max_payment_retries", "3")),
            PaymentRetryIntervalHours = (int)ParseDecimal(billing.GetValueOrDefault("payment_retry_interval_hours", "24")),
        });
    }

    private static decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, out var result) ? result : 0;
    }
}

/// <summary>
/// Clean DTO for platform pricing - consumed by checkout, landing pages, etc.
/// NOTE: IndividualListingPrice now comes from "pricing" category.
/// </summary>
public record PlatformPricingResponse
{
    // Publicaciones
    public decimal BasicListing { get; init; }
    public decimal FeaturedListing { get; init; }
    public decimal PremiumListing { get; init; }
    public decimal SellerPremiumPrice { get; init; }
    public decimal IndividualListingPrice { get; init; }
    // Planes Dealer
    public decimal DealerStarter { get; init; }
    public decimal DealerPro { get; init; }
    public decimal DealerEnterprise { get; init; }
    // Boosts
    public decimal BoostBasicPrice { get; init; }
    public int BoostBasicDays { get; init; }
    public decimal BoostProPrice { get; init; }
    public int BoostProDays { get; init; }
    public decimal BoostPremiumPrice { get; init; }
    public int BoostPremiumDays { get; init; }
    // Duraciones
    public int BasicListingDays { get; init; }
    public int IndividualListingDays { get; init; }
    // Límites por plan
    public int StarterMaxVehicles { get; init; }
    public int ProMaxVehicles { get; init; }
    public int FreeMaxPhotos { get; init; }
    public int StarterMaxPhotos { get; init; }
    public int ProMaxPhotos { get; init; }
    public int EnterpriseMaxPhotos { get; init; }
    // Comisiones
    public decimal PlatformCommission { get; init; }
    public decimal ItbisPercentage { get; init; }
    public string Currency { get; init; } = "DOP";
    // Early Bird
    public decimal EarlyBirdDiscount { get; init; }
    public string EarlyBirdDeadline { get; init; } = "2026-06-30";
    public int EarlyBirdFreeMonths { get; init; }
    // Trial
    public int StripeTrialDays { get; init; }
}

/// <summary>
/// Payment provider configuration - Admin only.
/// Does NOT include secrets (API keys are managed via SecretsController).
/// Covers all 6 payment providers: Azul, CardNET, PixelPay, Fygaro, Stripe, PayPal.
/// </summary>
public record PaymentProvidersResponse
{
    public int StripeTrialDays { get; init; }
    // Azul (Banco Popular RD)
    public bool AzulEnabled { get; init; }
    public string AzulEnvironment { get; init; } = "Test";
    public string AzulMerchantName { get; init; } = "OKLA Marketplace";
    public string AzulMerchantType { get; init; } = "eCommerce";
    public string AzulCurrencyCode { get; init; } = "$";
    // CardNET (Bancaria RD)
    public bool CardnetEnabled { get; init; }
    public string CardnetEnvironment { get; init; } = "Test";
    // PixelPay (Fintech)
    public bool PixelpayEnabled { get; init; }
    public string PixelpayEnvironment { get; init; } = "Test";
    // Fygaro (Agregador)
    public bool FygaroEnabled { get; init; }
    public string FygaroEnvironment { get; init; } = "Test";
    public bool FygaroEnableSubscriptions { get; init; }
    // Stripe (Internacional + Suscripciones)
    public bool StripeEnabled { get; init; }
    public string StripeEnvironment { get; init; } = "Test";
    // PayPal (Internacional)
    public bool PaypalEnabled { get; init; }
    public string PaypalEnvironment { get; init; } = "sandbox";
    // General billing
    public string InvoicePrefix { get; init; } = "OKLA-INV";
    public bool InvoiceNcfEnabled { get; init; }
    public bool AutoRetryFailedPayments { get; init; }
    public int MaxPaymentRetries { get; init; }
    public int PaymentRetryIntervalHours { get; init; }
}
