namespace DealerManagementService.Domain.Entities;

public class Dealer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // FK to UserService
    public string BusinessName { get; set; } = string.Empty;
    public string RNC { get; set; } = string.Empty; // Registro Nacional de Contribuyentes (RD Tax ID)
    public string? LegalName { get; set; }
    public string? TradeName { get; set; }
    public DealerType Type { get; set; } = DealerType.Independent;
    public DealerStatus Status { get; set; } = DealerStatus.Pending;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotVerified;
    
    // Contact Info
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? Website { get; set; }
    
    // Address
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "República Dominicana";
    
    // Business Details
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public int? EmployeeCount { get; set; }
    
    // Public Profile (Sprint 7)
    public string? Slogan { get; set; }
    public string? AboutUs { get; set; }
    public List<string> Specialties { get; set; } = new();
    public List<string> SupportedBrands { get; set; } = new();
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? YouTubeUrl { get; set; }
    public string? WhatsAppNumber { get; set; }
    public bool ShowPhoneOnProfile { get; set; } = true;
    public bool ShowEmailOnProfile { get; set; } = true;
    public bool AcceptsTradeIns { get; set; }
    public bool OffersFinancing { get; set; }
    public bool OffersWarranty { get; set; }
    public bool OffersHomeDelivery { get; set; }
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string Slug { get; set; } = string.Empty;
    
    // Badges & Trust
    public bool IsTrustedDealer { get; set; }
    public bool IsFoundingMember { get; set; } // Early Bird participant
    public DateTime? TrustedDealerSince { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalSales { get; set; }
    
    // Verification Documents
    public List<DealerDocument> Documents { get; set; } = new();
    
    // Locations/Branches
    public List<DealerLocation> Locations { get; set; } = new();
    
    // Subscription
    public Guid? SubscriptionId { get; set; } // FK to BillingService
    public DealerPlan CurrentPlan { get; set; } = DealerPlan.Free;
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public bool IsSubscriptionActive { get; set; }
    
    // Limits (based on plan)
    public int MaxActiveListings { get; set; }
    public int CurrentActiveListings { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; } // Admin user ID
    
    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Helper Methods
    public bool CanAddListing()
    {
        return IsSubscriptionActive && CurrentActiveListings < MaxActiveListings;
    }
    
    public int GetRemainingListings()
    {
        return Math.Max(0, MaxActiveListings - CurrentActiveListings);
    }
    
    public bool IsVerified()
    {
        return VerificationStatus == VerificationStatus.Verified && Status == DealerStatus.Active;
    }
    
    // Sprint 7: Public Profile Methods
    public string GenerateSlug()
    {
        var slug = BusinessName.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ñ", "n");
        
        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        
        return slug;
    }
    
    public void MarkAsTrusted()
    {
        IsTrustedDealer = true;
        TrustedDealerSince = DateTime.UtcNow;
    }
    
    public void RemoveTrustedBadge()
    {
        IsTrustedDealer = false;
        TrustedDealerSince = null;
    }
    
    public bool IsProfileComplete()
    {
        return !string.IsNullOrWhiteSpace(BusinessName)
            && !string.IsNullOrWhiteSpace(Description)
            && !string.IsNullOrWhiteSpace(LogoUrl)
            && !string.IsNullOrWhiteSpace(Address)
            && !string.IsNullOrWhiteSpace(Phone);
    }
    
    public int GetProfileCompletionPercentage()
    {
        int totalFields = 20;
        int completedFields = 0;
        
        if (!string.IsNullOrWhiteSpace(BusinessName)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Description)) completedFields++;
        if (!string.IsNullOrWhiteSpace(AboutUs)) completedFields++;
        if (!string.IsNullOrWhiteSpace(LogoUrl)) completedFields++;
        if (!string.IsNullOrWhiteSpace(BannerUrl)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Address)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Phone)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Email)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Website)) completedFields++;
        if (!string.IsNullOrWhiteSpace(WhatsAppNumber)) completedFields++;
        if (!string.IsNullOrWhiteSpace(FacebookUrl)) completedFields++;
        if (!string.IsNullOrWhiteSpace(InstagramUrl)) completedFields++;
        if (Specialties.Any()) completedFields++;
        if (SupportedBrands.Any()) completedFields++;
        if (EstablishedDate.HasValue) completedFields++;
        if (EmployeeCount.HasValue) completedFields++;
        if (Locations.Any()) completedFields++;
        if (AcceptsTradeIns) completedFields++;
        if (OffersFinancing) completedFields++;
        if (OffersWarranty) completedFields++;
        
        return (int)((completedFields / (double)totalFields) * 100);
    }
}

public enum DealerType
{
    Independent = 0,    // Single dealership
    Chain = 1,          // Multiple locations, same brand
    MultipleStore = 2,  // Multiple locations, different brands
    Franchise = 3       // Franchised dealer
}

public enum DealerStatus
{
    Pending = 0,        // Registration submitted, awaiting review
    UnderReview = 1,    // Admin is reviewing
    Active = 2,         // Approved and operational
    Suspended = 3,      // Temporarily suspended (payment issues, violations)
    Rejected = 4,       // Registration rejected
    Inactive = 5        // Dealer chose to deactivate
}

public enum VerificationStatus
{
    NotVerified = 0,
    DocumentsUploaded = 1,
    UnderReview = 2,
    Verified = 3,
    Rejected = 4,
    RequiresMoreInfo = 5
}

/// <summary>
/// Planes de dealer OKLA — El acceso básico siempre es gratuito.
/// Los planes se diferencian por visibilidad y herramientas.
/// DB-compatible: int values preserved (Free=0, Basic/Visible=1, Pro=2, Enterprise/Elite=3).
/// </summary>
public enum DealerPlan
{
    Free = 0,           // Libre — $0/mes — Publicaciones ilimitadas, 10 fotos
    Basic = 1,          // Visible — $29/mes — Prioridad media, 3 destacados
    Pro = 2,            // Pro — $89/mes — Alta prioridad, 10 destacados, ChatAgent
    Enterprise = 3      // Elite — $199/mes — Top prioridad, 25 destacados, ChatAgent ilimitado
}

/// <summary>
/// Límites y features por plan — Estructura OKLA v2 (4 tiers: Libre/Visible/Pro/Elite)
/// </summary>
public static class DealerPlanLimits
{
    public static DealerPlanFeatures GetFeatures(DealerPlan plan)
    {
        return plan switch
        {
            // ═══════════════════════════════════════════
            // LIBRE ($0/mes) — Acceso básico gratuito
            // ═══════════════════════════════════════════
            DealerPlan.Free => new DealerPlanFeatures
            {
                MaxListings = 999999,       // ILIMITADAS
                MaxImages = 10,             // Hasta 10 fotos
                MaxFeaturedListings = 0,    // Sin destacados incluidos
                SearchPriority = SearchPriorityLevel.Standard,
                MonthlyOklaCoinsCredits = 0m,
                BadgeType = DealerBadgeType.None,
                ChatAgentWebConversations = 0,
                ChatAgentWhatsAppConversations = 0,
                AutoScheduling = false,
                WhatsAppReminders = false,
                PricingAgentFreeUsages = 1,
                PricingAgentMonthlyUsages = 0,
                PricingAgentPdfReport = false,
                DashboardAnalytics = DashboardLevel.None,
                CanExportAnalytics = false,
                IncludesVideoTour = false,
                AnalyticsAccess = false,
                BulkUpload = false,
                PrioritySupport = false,
                CustomBranding = false,
                ApiAccess = false,
                LeadManagement = false,
                EmailAutomation = false,
                MarketPriceAnalysis = false,
                AdvancedReporting = false,
                WhiteLabel = false,
                DedicatedAccountManager = false
            },

            // ═══════════════════════════════════════════
            // VISIBLE ($29/mes) — Para dealers en crecimiento
            // ═══════════════════════════════════════════
            DealerPlan.Basic => new DealerPlanFeatures
            {
                MaxListings = 999999,       // ILIMITADAS
                MaxImages = 20,             // Hasta 20 fotos
                MaxFeaturedListings = 3,    // 3 destacados/mes
                SearchPriority = SearchPriorityLevel.Medium,
                MonthlyOklaCoinsCredits = 15m,  // $15 en créditos OKLA Coins
                BadgeType = DealerBadgeType.Verified,
                ChatAgentWebConversations = 0,  // No incluido
                ChatAgentWhatsAppConversations = 0,
                AutoScheduling = false,
                WhatsAppReminders = false,
                PricingAgentFreeUsages = 0,
                PricingAgentMonthlyUsages = 5,  // 5/mes
                PricingAgentPdfReport = false,
                DashboardAnalytics = DashboardLevel.Basic,
                CanExportAnalytics = false,
                IncludesVideoTour = false,
                AnalyticsAccess = true,
                BulkUpload = true,
                PrioritySupport = false,
                CustomBranding = false,
                ApiAccess = false,
                LeadManagement = true,
                EmailAutomation = false,
                MarketPriceAnalysis = false,
                AdvancedReporting = false,
                WhiteLabel = false,
                DedicatedAccountManager = false
            },

            // ═══════════════════════════════════════════
            // PRO ($89/mes) — Para dealers establecidos
            // ═══════════════════════════════════════════
            DealerPlan.Pro => new DealerPlanFeatures
            {
                MaxListings = 999999,       // ILIMITADAS
                MaxImages = 30,             // Hasta 30 fotos
                MaxFeaturedListings = 10,   // 10 destacados/mes
                SearchPriority = SearchPriorityLevel.High,
                MonthlyOklaCoinsCredits = 45m,  // $45 en créditos OKLA Coins
                BadgeType = DealerBadgeType.VerifiedGold,
                ChatAgentWebConversations = 500,   // 500 conv/mes
                ChatAgentWhatsAppConversations = 500,
                AutoScheduling = true,
                WhatsAppReminders = false,
                PricingAgentFreeUsages = 0,
                PricingAgentMonthlyUsages = 999999, // Ilimitada
                PricingAgentPdfReport = false,
                DashboardAnalytics = DashboardLevel.Advanced,
                CanExportAnalytics = false,
                IncludesVideoTour = false,
                AnalyticsAccess = true,
                BulkUpload = true,
                PrioritySupport = true,
                CustomBranding = true,
                ApiAccess = false,
                LeadManagement = true,
                EmailAutomation = true,
                MarketPriceAnalysis = true,
                AdvancedReporting = true,
                WhiteLabel = false,
                DedicatedAccountManager = false
            },

            // ═══════════════════════════════════════════
            // ELITE ($199/mes) — Para grandes operaciones
            // ═══════════════════════════════════════════
            DealerPlan.Enterprise => new DealerPlanFeatures
            {
                MaxListings = 999999,       // ILIMITADAS
                MaxImages = 40,             // Hasta 40 fotos + video tour
                MaxFeaturedListings = 25,   // 25 destacados/mes
                SearchPriority = SearchPriorityLevel.Top,
                MonthlyOklaCoinsCredits = 120m, // $120 en créditos OKLA Coins
                BadgeType = DealerBadgeType.VerifiedPremium,
                ChatAgentWebConversations = 999999,   // ILIMITADO
                ChatAgentWhatsAppConversations = 999999,
                AutoScheduling = true,
                WhatsAppReminders = true,   // + recordatorios WA
                PricingAgentFreeUsages = 0,
                PricingAgentMonthlyUsages = 999999, // Ilimitada
                PricingAgentPdfReport = true,       // + informe PDF
                DashboardAnalytics = DashboardLevel.Complete,
                CanExportAnalytics = true,  // Completo + exportar
                IncludesVideoTour = true,   // Fotos + video tour
                AnalyticsAccess = true,
                BulkUpload = true,
                PrioritySupport = true,
                CustomBranding = true,
                ApiAccess = true,
                LeadManagement = true,
                EmailAutomation = true,
                MarketPriceAnalysis = true,
                AdvancedReporting = true,
                WhiteLabel = true,
                DedicatedAccountManager = true
            },
            _ => GetFeatures(DealerPlan.Free)
        };
    }
    
    public static decimal GetMonthlyPrice(DealerPlan plan)
    {
        return plan switch
        {
            DealerPlan.Free => 0m,          // Libre
            DealerPlan.Basic => 29m,        // Visible
            DealerPlan.Pro => 89m,          // Pro
            DealerPlan.Enterprise => 199m,  // Elite
            _ => 0m
        };
    }
    
    public static string GetPlanDisplayName(DealerPlan plan)
    {
        return plan switch
        {
            DealerPlan.Free => "Libre",
            DealerPlan.Basic => "Visible",
            DealerPlan.Pro => "Pro",
            DealerPlan.Enterprise => "Elite",
            _ => "Libre"
        };
    }

    /// <summary>
    /// Costo operativo estimado de OKLA por dealer/mes
    /// </summary>
    public static decimal GetOklaCostPerDealer(DealerPlan plan)
    {
        return plan switch
        {
            DealerPlan.Free => 0.50m,       // $0.05–$1.00 promedio
            DealerPlan.Basic => 1.50m,
            DealerPlan.Pro => 68m,
            DealerPlan.Enterprise => 228m,
            _ => 0m
        };
    }
}

/// <summary>
/// Nivel de prioridad en búsquedas
/// </summary>
public enum SearchPriorityLevel
{
    Standard = 0,
    Medium = 1,
    High = 2,
    Top = 3
}

/// <summary>
/// Tipo de badge de verificación del dealer
/// </summary>
public enum DealerBadgeType
{
    None = 0,
    Verified = 1,           // "Verificado" — Plan Visible
    VerifiedGold = 2,       // "Verificado Dorado" — Plan Pro
    VerifiedPremium = 3     // "Verificado Premium" — Plan Elite
}

/// <summary>
/// Nivel del dashboard de analytics
/// </summary>
public enum DashboardLevel
{
    None = 0,
    Basic = 1,
    Advanced = 2,
    Complete = 3
}

/// <summary>
/// Features disponibles por plan — OKLA v2 con visibilidad, ChatAgent, OKLA Coins
/// </summary>
public class DealerPlanFeatures
{
    // ── Listings & Media ──
    public int MaxListings { get; set; }
    public int MaxImages { get; set; }
    public int MaxFeaturedListings { get; set; }
    public bool IncludesVideoTour { get; set; }

    // ── Visibilidad ──
    public SearchPriorityLevel SearchPriority { get; set; }
    public DealerBadgeType BadgeType { get; set; }

    // ── OKLA Coins ──
    public decimal MonthlyOklaCoinsCredits { get; set; }

    // ── ChatAgent (IA 24/7) ──
    public int ChatAgentWebConversations { get; set; }
    public int ChatAgentWhatsAppConversations { get; set; }

    // ── Agendamiento ──
    public bool AutoScheduling { get; set; }
    public bool WhatsAppReminders { get; set; }

    // ── PricingAgent (Valoración IA) ──
    public int PricingAgentFreeUsages { get; set; }
    public int PricingAgentMonthlyUsages { get; set; }
    public bool PricingAgentPdfReport { get; set; }

    // ── Analytics ──
    public DashboardLevel DashboardAnalytics { get; set; }
    public bool CanExportAnalytics { get; set; }

    // ── Legacy fields (backward compatibility) ──
    public bool AnalyticsAccess { get; set; }
    public bool BulkUpload { get; set; }
    public bool PrioritySupport { get; set; }
    public bool CustomBranding { get; set; }
    public bool ApiAccess { get; set; }
    public bool LeadManagement { get; set; }
    public bool EmailAutomation { get; set; }
    public bool MarketPriceAnalysis { get; set; }
    public bool AdvancedReporting { get; set; }
    public bool WhiteLabel { get; set; }
    public bool DedicatedAccountManager { get; set; }
}
