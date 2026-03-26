namespace CarDealer.Contracts.Enums;

/// <summary>
/// Single source of truth for OKLA Seller plan mapping.
///
/// 3 Seller plans aligned with frontend/web-next/src/lib/plan-config.ts SellerPlan:
///   Libre ($0 — 1 listing), Estándar ($9.99/listing), Verificado ($34.99/mes)
///
/// Note: Sellers are individual owners, DIFFERENT from Dealers (showrooms/concesionarios).
/// Dealer plans: PlanConfiguration.cs (6 plans, monthly subscription).
/// Seller plans: this file (3 plans, individual listing model for owners).
/// </summary>
public static class SellerPlanConfiguration
{
    // =========================================================================
    // SELLER PLAN KEYS (match frontend SellerPlan enum values)
    // =========================================================================

    public const string KeyLibre = "libre_seller";
    public const string KeyEstandar = "estandar";
    public const string KeyVerificado = "verificado";

    // =========================================================================
    // SELLER PLAN DISPLAY NAMES
    // =========================================================================

    public const string DisplayLibre = "Libre";
    public const string DisplayEstandar = "Estándar";
    public const string DisplayVerificado = "Verificado";

    // =========================================================================
    // SELLER PLAN PRICES (USD) — match frontend SELLER_PLAN_PRICES
    // =========================================================================

    public const decimal PriceLibre = 0m;
    public const decimal PriceEstandar = 9.99m;     // per listing (one-time)
    public const decimal PriceVerificado = 34.99m;  // monthly subscription

    // =========================================================================
    // PLAN TYPE
    // =========================================================================

    public const string BillingModelEstandar = "per_listing"; // one-time per listing
    public const string BillingModelVerificado = "monthly";   // monthly subscription

    /// <summary>
    /// Returns the display name for a given seller plan key.
    /// Case-insensitive, handles frontend keys and legacy variations.
    /// </summary>
    public static string GetDisplayName(string? planKey)
    {
        return (planKey ?? "").Trim().ToLowerInvariant() switch
        {
            "libre_seller" or "libre" or "free" or "" => DisplayLibre,
            "estandar" or "estándar" or "standard" or "basic" => DisplayEstandar,
            "verificado" or "verified" or "premium" => DisplayVerificado,
            _ => DisplayLibre
        };
    }

    /// <summary>
    /// Returns the monthly price (USD) for a seller plan.
    /// For Estándar this reflects cost per listing, not monthly.
    /// </summary>
    public static decimal GetPrice(string? planKey)
    {
        return (planKey ?? "").Trim().ToLowerInvariant() switch
        {
            "libre_seller" or "libre" or "free" or "" => PriceLibre,
            "estandar" or "estándar" or "standard" or "basic" => PriceEstandar,
            "verificado" or "verified" or "premium" => PriceVerificado,
            _ => PriceLibre
        };
    }

    /// <summary>
    /// Returns the canonical seller plan key for a given input.
    /// Used to normalize plan keys before storing or comparing.
    /// </summary>
    public static string GetCanonicalKey(string? planKey)
    {
        return (planKey ?? "").Trim().ToLowerInvariant() switch
        {
            "libre_seller" or "libre" or "free" or "" => KeyLibre,
            "estandar" or "estándar" or "standard" or "basic" => KeyEstandar,
            "verificado" or "verified" or "premium" => KeyVerificado,
            _ => KeyLibre
        };
    }

    /// <summary>
    /// All seller plan keys in tier order (ascending).
    /// </summary>
    public static IReadOnlyList<string> AllKeys { get; } = new[]
    {
        KeyLibre, KeyEstandar, KeyVerificado
    };

    /// <summary>
    /// All seller plan prices by canonical key.
    /// </summary>
    public static IReadOnlyDictionary<string, decimal> PricesByKey { get; } =
        new Dictionary<string, decimal>
        {
            [KeyLibre] = PriceLibre,
            [KeyEstandar] = PriceEstandar,
            [KeyVerificado] = PriceVerificado,
        };
}
