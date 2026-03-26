namespace CarDealer.Contracts.Enums;

/// <summary>
/// Backend enforcement limits for OKLA Seller plans.
/// Mirrors frontend/web-next/src/lib/plan-config.ts SELLER_PLAN_LIMITS.
///
/// Use SellerPlanFeatureLimits.GetLimits(planKey) to retrieve limits for a seller.
/// Use IsWithinLimit() to gate feature usage before processing.
/// </summary>
public static class SellerPlanFeatureLimits
{
    /// <summary>
    /// Returns the feature limits for a given seller plan key.
    /// </summary>
    public static SellerPlanLimits GetLimits(string? planKey)
    {
        var canonical = SellerPlanConfiguration.GetCanonicalKey(planKey);
        return canonical switch
        {
            SellerPlanConfiguration.KeyLibre => LibreLimits,
            SellerPlanConfiguration.KeyEstandar => EstandarLimits,
            SellerPlanConfiguration.KeyVerificado => VerificadoLimits,
            _ => LibreLimits, // Default to most restrictive
        };
    }

    /// <summary>
    /// Check if a specific boolean feature is allowed for the given seller plan.
    /// </summary>
    public static bool IsFeatureAllowed(string? planKey, string featureName)
    {
        var limits = GetLimits(planKey);
        return featureName.ToLowerInvariant() switch
        {
            "analytics_access" => limits.AnalyticsAccess,
            "search_priority" => limits.SearchPriority,
            "verified_badge" => limits.VerifiedBadge,
            "whatsapp_contact" => limits.WhatsAppContact,
            "detailed_stats" => limits.DetailedStats,
            "boost_available" => limits.BoostAvailable,
            "social_sharing" => limits.SocialSharing,
            "price_drop_alerts" => limits.PriceDropAlerts,
            "view_360_available" => limits.View360Available,
            _ => false,
        };
    }

    /// <summary>
    /// Check if a numeric limit is within bounds for the seller plan.
    /// Returns true if the current count is below the limit. -1 = unlimited.
    /// </summary>
    public static bool IsWithinLimit(string? planKey, string limitName, int currentCount)
    {
        var limits = GetLimits(planKey);
        var maxValue = limitName.ToLowerInvariant() switch
        {
            "max_listings" => limits.MaxListings,
            "max_images" => limits.MaxImages,
            "max_videos" => limits.MaxVideos,
            "featured_listings" => limits.FeaturedListings,
            _ => 0,
        };

        if (maxValue == -1) return true; // unlimited
        return currentCount < maxValue;
    }

    // ═══════════════════════════════════════════
    // PLAN DEFINITIONS — Mirror of frontend SELLER_PLAN_LIMITS
    // ═══════════════════════════════════════════

    public static readonly SellerPlanLimits LibreLimits = new()
    {
        PlanKey = SellerPlanConfiguration.KeyLibre,
        DisplayName = SellerPlanConfiguration.DisplayLibre,
        PricePerMonth = SellerPlanConfiguration.PriceLibre,
        MaxListings = 1,
        MaxImages = 5,
        ListingDurationDays = 30,
        AnalyticsAccess = false,
        SearchPriority = false,
        VerifiedBadge = false,
        FeaturedListings = 0,
        WhatsAppContact = true,
        DetailedStats = false,
        BoostAvailable = false,
        SocialSharing = false,
        PriceDropAlerts = false,
        MaxVideos = 0,
        View360Available = false,
    };

    public static readonly SellerPlanLimits EstandarLimits = new()
    {
        PlanKey = SellerPlanConfiguration.KeyEstandar,
        DisplayName = SellerPlanConfiguration.DisplayEstandar,
        PricePerMonth = SellerPlanConfiguration.PriceEstandar, // per listing
        MaxListings = 1,  // 1 per payment (per-listing billing)
        MaxImages = 10,
        ListingDurationDays = 60,
        AnalyticsAccess = false,
        SearchPriority = true,
        VerifiedBadge = true,
        FeaturedListings = 0,
        WhatsAppContact = true,
        DetailedStats = false,
        BoostAvailable = true,
        SocialSharing = true,
        PriceDropAlerts = false,
        MaxVideos = 0,
        View360Available = false,
    };

    public static readonly SellerPlanLimits VerificadoLimits = new()
    {
        PlanKey = SellerPlanConfiguration.KeyVerificado,
        DisplayName = SellerPlanConfiguration.DisplayVerificado,
        PricePerMonth = SellerPlanConfiguration.PriceVerificado, // monthly
        MaxListings = 3,
        MaxImages = 12,
        ListingDurationDays = 90,
        AnalyticsAccess = true,
        SearchPriority = true,
        VerifiedBadge = true,
        FeaturedListings = 0,
        WhatsAppContact = true,
        DetailedStats = true,
        BoostAvailable = true,
        SocialSharing = true,
        PriceDropAlerts = true,
        MaxVideos = 0,
        View360Available = true,
    };
}

/// <summary>
/// Feature limits for a single OKLA Seller plan.
/// Mirrors frontend SellerPlanFeatures interface.
/// </summary>
public class SellerPlanLimits
{
    public string PlanKey { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public decimal PricePerMonth { get; set; }

    // Numeric limits (-1 = unlimited)
    public int MaxListings { get; set; }
    public int MaxImages { get; set; }
    public int ListingDurationDays { get; set; }
    public int FeaturedListings { get; set; }
    public int MaxVideos { get; set; }

    // Boolean feature flags
    public bool AnalyticsAccess { get; set; }
    public bool SearchPriority { get; set; }
    public bool VerifiedBadge { get; set; }
    public bool WhatsAppContact { get; set; }
    public bool DetailedStats { get; set; }
    public bool BoostAvailable { get; set; }
    public bool SocialSharing { get; set; }
    public bool PriceDropAlerts { get; set; }
    public bool View360Available { get; set; }
}
