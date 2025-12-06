using System;

namespace UserService.Domain.Entities
{
    /// <summary>
    /// Planes de suscripción para dealers
    /// </summary>
    public enum DealerPlan
    {
        Free,           // Plan gratuito inicial
        Basic,          // Plan básico pagado
        Pro,            // Plan profesional
        Enterprise      // Plan enterprise
    }

    /// <summary>
    /// Estado de la suscripción
    /// </summary>
    public enum SubscriptionStatus
    {
        Active,
        Canceled,
        Expired,
        Trial
    }

    /// <summary>
    /// Suscripción de dealer con control de features y límites
    /// </summary>
    public class DealerSubscription
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public DealerPlan Plan { get; set; } = DealerPlan.Free;
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        // Fechas
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }          // null si es FREE (permanente)
        public DateTime? TrialEndDate { get; set; }

        // Features habilitadas (JSON)
        public string Features { get; set; } = "{}";    // DealerPlanFeatures serializado

        // Uso actual
        public int CurrentListings { get; set; }
        public int ListingsThisMonth { get; set; }
        public int FeaturedUsed { get; set; }

        // Billing
        public string? StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // Métodos helper
        public bool IsActive() => Status == SubscriptionStatus.Active &&
                                  (EndDate == null || EndDate > DateTime.UtcNow);

        public bool IsExpired() => Status == SubscriptionStatus.Expired ||
                                   (EndDate.HasValue && EndDate.Value <= DateTime.UtcNow);
    }

    /// <summary>
    /// Historial de cambios de plan
    /// </summary>
    public class SubscriptionHistory
    {
        public Guid Id { get; set; }
        public Guid DealerSubscriptionId { get; set; }
        public DealerPlan FromPlan { get; set; }
        public DealerPlan ToPlan { get; set; }
        public string Reason { get; set; } = string.Empty; // "upgrade", "downgrade", "canceled"
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Guid? ChangedBy { get; set; }
        public string? Notes { get; set; }

        // Navegación
        public DealerSubscription? DealerSubscription { get; set; }
    }

    /// <summary>
    /// Límites y features por plan (configuración)
    /// Esto podría ser una tabla de configuración o hardcoded
    /// </summary>
    public static class DealerPlanLimits
    {
        public static DealerPlanFeatures GetFeatures(DealerPlan plan)
        {
            return plan switch
            {
                DealerPlan.Free => new DealerPlanFeatures
                {
                    // Marketplace
                    MaxListings = 3,
                    MaxImages = 5,
                    FeaturedListings = 0,
                    BulkUpload = false,
                    CustomBranding = false,

                    // Analytics
                    AnalyticsAccess = false,
                    MarketPriceAnalysis = false,
                    AdvancedReports = false,
                    CustomDashboards = false,

                    // CRM
                    CRMBasic = false,
                    CRMAdvanced = false,
                    MaxLeads = 10,
                    LeadAutomation = false,

                    // Invoicing
                    InvoicingBasic = false,
                    InvoicingCFDI = false,
                    MaxInvoicesPerMonth = 5,

                    // Finance
                    FinanceBasic = false,
                    FinanceAdvanced = false,
                    TaxReports = false,

                    // Marketing
                    EmailMarketing = false,
                    MaxEmailsPerMonth = 0,
                    SMSMarketing = false,
                    MarketingAutomation = false,

                    // Integration
                    WhatsappIntegration = false,
                    FacebookMarketplace = false,
                    WebhooksAPI = false,
                    PublicAPI = false,

                    // Support
                    PrioritySupport = false,
                    DedicatedAccountManager = false
                },

                DealerPlan.Basic => new DealerPlanFeatures
                {
                    // Marketplace
                    MaxListings = 50,
                    MaxImages = 10,
                    FeaturedListings = 2,
                    BulkUpload = true,
                    CustomBranding = false,

                    // Analytics
                    AnalyticsAccess = true,         // ✅ Incluido
                    MarketPriceAnalysis = false,
                    AdvancedReports = false,        // ❌ Add-on $29/mo
                    CustomDashboards = false,

                    // CRM
                    CRMBasic = true,                // ✅ Incluido
                    CRMAdvanced = false,            // ❌ Add-on $29/mo
                    MaxLeads = 100,
                    LeadAutomation = false,

                    // Invoicing
                    InvoicingBasic = true,          // ✅ Incluido
                    InvoicingCFDI = false,          // ❌ Add-on $39/mo
                    MaxInvoicesPerMonth = 50,

                    // Finance
                    FinanceBasic = false,
                    FinanceAdvanced = false,        // ❌ Add-on $49/mo
                    TaxReports = false,

                    // Marketing
                    EmailMarketing = true,          // ✅ Incluido
                    MaxEmailsPerMonth = 500,
                    SMSMarketing = false,
                    MarketingAutomation = false,    // ❌ Add-on $59/mo

                    // Integration
                    WhatsappIntegration = false,    // ❌ Add-on $19/mo
                    FacebookMarketplace = false,
                    WebhooksAPI = false,
                    PublicAPI = false,              // ❌ Add-on $99/mo

                    // Support
                    PrioritySupport = false,
                    DedicatedAccountManager = false
                },

                DealerPlan.Pro => new DealerPlanFeatures
                {
                    // Marketplace
                    MaxListings = 200,
                    MaxImages = 20,
                    FeaturedListings = 10,
                    BulkUpload = true,
                    CustomBranding = true,

                    // Analytics
                    AnalyticsAccess = true,
                    MarketPriceAnalysis = true,     // ✅ Incluido
                    AdvancedReports = false,        // ❌ Add-on $29/mo
                    CustomDashboards = false,

                    // CRM
                    CRMBasic = true,
                    CRMAdvanced = false,            // ❌ Add-on $29/mo
                    MaxLeads = 500,
                    LeadAutomation = true,          // ✅ Incluido

                    // Invoicing
                    InvoicingBasic = true,
                    InvoicingCFDI = true,           // ✅ Incluido
                    MaxInvoicesPerMonth = 200,

                    // Finance
                    FinanceBasic = true,            // ✅ Incluido
                    FinanceAdvanced = false,        // ❌ Add-on $49/mo
                    TaxReports = false,

                    // Marketing
                    EmailMarketing = true,
                    MaxEmailsPerMonth = 2000,
                    SMSMarketing = false,
                    MarketingAutomation = false,    // ❌ Add-on $59/mo

                    // Integration
                    WhatsappIntegration = true,     // ✅ Incluido
                    FacebookMarketplace = true,     // ✅ Incluido
                    WebhooksAPI = false,
                    PublicAPI = false,              // ❌ Add-on $99/mo

                    // Support
                    PrioritySupport = true,         // ✅ Incluido
                    DedicatedAccountManager = false
                },

                DealerPlan.Enterprise => new DealerPlanFeatures
                {
                    // Marketplace
                    MaxListings = 999999,           // Ilimitado
                    MaxImages = 50,
                    FeaturedListings = 50,
                    BulkUpload = true,
                    CustomBranding = true,

                    // Analytics
                    AnalyticsAccess = true,
                    MarketPriceAnalysis = true,
                    AdvancedReports = true,         // ✅ Incluido
                    CustomDashboards = true,        // ✅ Incluido

                    // CRM
                    CRMBasic = true,
                    CRMAdvanced = true,             // ✅ Incluido
                    MaxLeads = 999999,              // Ilimitado
                    LeadAutomation = true,

                    // Invoicing
                    InvoicingBasic = true,
                    InvoicingCFDI = true,
                    MaxInvoicesPerMonth = 999999,   // Ilimitado

                    // Finance
                    FinanceBasic = true,
                    FinanceAdvanced = true,         // ✅ Incluido
                    TaxReports = true,              // ✅ Incluido

                    // Marketing
                    EmailMarketing = true,
                    MaxEmailsPerMonth = 999999,     // Ilimitado
                    SMSMarketing = true,
                    MarketingAutomation = true,     // ✅ Incluido

                    // Integration
                    WhatsappIntegration = true,
                    FacebookMarketplace = true,
                    WebhooksAPI = true,             // ✅ Incluido
                    PublicAPI = true,               // ✅ Incluido

                    // Support
                    PrioritySupport = true,
                    DedicatedAccountManager = true  // ✅ Incluido
                },

                _ => throw new ArgumentException($"Unknown plan: {plan}")
            };
        }
    }

    /// <summary>
    /// Features disponibles por plan
    /// NOTA: Algunos features son add-ons vendibles. Los booleanos indican si están incluidos en el plan base.
    /// </summary>
    public class DealerPlanFeatures
    {
        // ========================================
        // MARKETPLACE FEATURES (Core)
        // ========================================
        public int MaxListings { get; set; }
        public int MaxImages { get; set; }
        public int FeaturedListings { get; set; }
        public bool BulkUpload { get; set; }
        public bool CustomBranding { get; set; }

        // ========================================
        // ANALYTICS FEATURES
        // ========================================
        public bool AnalyticsAccess { get; set; }           // Básico: tráfico, vistas
        public bool MarketPriceAnalysis { get; set; }       // Comparativa de precios de mercado
        public bool AdvancedReports { get; set; }           // Reportes avanzados (o add-on $29/mo)
        public bool CustomDashboards { get; set; }          // Dashboards personalizados (Enterprise)

        // ========================================
        // CRM FEATURES
        // ========================================
        public bool CRMBasic { get; set; }                  // Leads básico (BASIC+)
        public bool CRMAdvanced { get; set; }               // Pipeline, automatización (PRO+ o add-on $29/mo)
        public int MaxLeads { get; set; }                   // FREE=10, BASIC=100, PRO=500, ENT=unlimited
        public bool LeadAutomation { get; set; }            // Follow-ups automáticos

        // ========================================
        // INVOICING FEATURES
        // ========================================
        public bool InvoicingBasic { get; set; }            // Cotizaciones simples (BASIC+)
        public bool InvoicingCFDI { get; set; }             // Facturación electrónica México (PRO+ o add-on $39/mo)
        public int MaxInvoicesPerMonth { get; set; }        // FREE=5, BASIC=50, PRO=200, ENT=unlimited

        // ========================================
        // FINANCE FEATURES
        // ========================================
        public bool FinanceBasic { get; set; }              // Gastos/ingresos básicos (PRO+)
        public bool FinanceAdvanced { get; set; }           // Contabilidad completa (Enterprise o add-on $49/mo)
        public bool TaxReports { get; set; }                // Reportes fiscales (Enterprise)

        // ========================================
        // MARKETING FEATURES
        // ========================================
        public bool EmailMarketing { get; set; }            // Email campaigns básicas (BASIC+)
        public int MaxEmailsPerMonth { get; set; }          // BASIC=500, PRO=2000, ENT=unlimited
        public bool SMSMarketing { get; set; }              // SMS campaigns (PRO+ o incluido en automation add-on)
        public bool MarketingAutomation { get; set; }       // Workflows, segmentación (Enterprise o add-on $59/mo)

        // ========================================
        // INTEGRATION FEATURES
        // ========================================
        public bool WhatsappIntegration { get; set; }       // WhatsApp Business API (PRO+ o add-on $19/mo)
        public bool FacebookMarketplace { get; set; }       // Publicación automática FB (PRO+)
        public bool WebhooksAPI { get; set; }               // Webhooks + API básica (Enterprise)
        public bool PublicAPI { get; set; }                 // API pública completa (Enterprise o add-on $99/mo)

        // ========================================
        // SUPPORT FEATURES
        // ========================================
        public bool PrioritySupport { get; set; }           // Support prioritario (PRO+)
        public bool DedicatedAccountManager { get; set; }   // Account manager dedicado (Enterprise)
    }
}
