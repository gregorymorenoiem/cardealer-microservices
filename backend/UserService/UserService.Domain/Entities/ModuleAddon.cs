using System;
using System.Collections.Generic;

namespace UserService.Domain.Entities
{
    /// <summary>
    /// Categorías de módulos add-on
    /// </summary>
    public enum ModuleCategory
    {
        Core,           // Incluido en plan base (Marketplace, Inventario básico)
        Sales,          // CRM, Leads, Pipeline
        Finance,        // Facturación, Contabilidad, Impuestos
        Marketing,      // Email, SMS, Campañas
        Integration,    // WhatsApp, Facebook, APIs
        Analytics,      // Reportes avanzados, Dashboards
        Automation,     // Workflows, Reglas de negocio
        Support         // Chat, Tickets, Help desk
    }

    /// <summary>
    /// Módulo add-on vendible (SaaS style)
    /// Ejemplo: "CRM Avanzado", "Facturación CFDI", "WhatsApp Business"
    /// </summary>
    public class ModuleAddon
    {
        public Guid Id { get; set; }

        // Identificación
        public string Name { get; set; } = string.Empty;           // "CRM Avanzado", "Facturación Electrónica"
        public string Code { get; set; } = string.Empty;           // "crm-advanced", "invoicing-cfdi"
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty; // Markdown para página del módulo
        public ModuleCategory Category { get; set; }

        // Pricing
        public decimal MonthlyPrice { get; set; }                  // Precio mensual
        public decimal YearlyPrice { get; set; }                   // Precio anual (con descuento)
        public bool HasFreeTrial { get; set; }                     // ¿Tiene trial de 14 días?
        public int TrialDays { get; set; } = 14;

        // Features (JSON array de strings)
        // Ejemplo: ["Pipeline de ventas", "Automatización de seguimiento", "500 leads/mes"]
        public string Features { get; set; } = "[]";

        // Dependencias (algunos módulos requieren otros)
        // Ejemplo: "CRM Avanzado" requiere "CRM Básico" (incluido en BASIC+)
        public string RequiredModules { get; set; } = "[]";        // JSON array de codes

        // Planes que incluyen este módulo GRATIS
        // Ejemplo: "WhatsApp" incluido en PRO y ENTERPRISE
        public string IncludedInPlans { get; set; } = "[]";        // JSON array: ["Pro", "Enterprise"]

        // Visibilidad
        public bool IsActive { get; set; } = true;                 // ¿Se puede comprar?
        public bool IsVisible { get; set; } = true;                // ¿Se muestra en marketplace?
        public int SortOrder { get; set; }                         // Orden en UI

        // Metadata
        public string? IconUrl { get; set; }                       // URL del icono
        public string? ThumbnailUrl { get; set; }                  // Thumbnail para cards
        public string? DocumentationUrl { get; set; }              // Link a docs

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // Navigation
        public ICollection<DealerModuleSubscription> Subscriptions { get; set; } = new List<DealerModuleSubscription>();
    }

    /// <summary>
    /// Suscripción de un dealer a un módulo add-on
    /// </summary>
    public class DealerModuleSubscription
    {
        public Guid Id { get; set; }

        // Relaciones
        public Guid DealerId { get; set; }
        public Guid ModuleAddonId { get; set; }

        // Estado
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

        // Fechas
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }                     // null si es activa
        public DateTime? TrialEndDate { get; set; }                // Fin del trial

        // Pricing (guardamos precio histórico por si cambian precios)
        public decimal MonthlyPrice { get; set; }                  // Precio cuando se suscribió
        public bool IsYearlyBilling { get; set; }                  // ¿Facturado anualmente?

        // Billing (Stripe)
        public string? StripeSubscriptionItemId { get; set; }      // ID del subscription item en Stripe

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        // Navigation
        public User Dealer { get; set; } = null!;
        public ModuleAddon ModuleAddon { get; set; } = null!;

        // Métodos helper
        public bool IsActive() => Status == SubscriptionStatus.Active &&
                                  (EndDate == null || EndDate > DateTime.UtcNow);

        public bool IsInTrial() => Status == SubscriptionStatus.Trial &&
                                   TrialEndDate.HasValue &&
                                   TrialEndDate.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Historial de cambios en módulos (activaciones, desactivaciones)
    /// </summary>
    public class ModuleSubscriptionHistory
    {
        public Guid Id { get; set; }
        public Guid DealerModuleSubscriptionId { get; set; }
        public SubscriptionStatus OldStatus { get; set; }
        public SubscriptionStatus NewStatus { get; set; }
        public string Action { get; set; } = string.Empty;         // "subscribed", "trial_started", "trial_ended", "canceled", "reactivated"
        public string? Reason { get; set; }                        // Razón del cambio
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Guid? ChangedBy { get; set; }

        // Navigation
        public DealerModuleSubscription? DealerModuleSubscription { get; set; }
    }

    /// <summary>
    /// Configuración de módulos por defecto (seed data)
    /// </summary>
    public static class DefaultModules
    {
        public static List<ModuleAddon> GetDefaultModules()
        {
            return new List<ModuleAddon>
            {
                // === SALES ===
                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "CRM Avanzado",
                    Code = "crm-advanced",
                    Description = "Pipeline de ventas, automatización de seguimiento, gestión de leads completa",
                    Category = ModuleCategory.Sales,
                    MonthlyPrice = 29m,
                    YearlyPrice = 290m, // 2 meses gratis
                    HasFreeTrial = true,
                    Features = "[\"Pipeline de ventas\", \"Automatización de seguimiento\", \"500 leads/mes\", \"Reportes de conversión\", \"Tareas y recordatorios\", \"Historial completo de interacciones\"]",
                    IncludedInPlans = "[\"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 1
                },
                
                // === FINANCE ===
                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "Facturación Electrónica (CFDI)",
                    Code = "invoicing-cfdi",
                    Description = "Facturas con validación SAT, timbrado fiscal, complementos de pago",
                    Category = ModuleCategory.Finance,
                    MonthlyPrice = 39m,
                    YearlyPrice = 390m,
                    HasFreeTrial = true,
                    Features = "[\"Timbrado fiscal ilimitado\", \"CFDI 4.0\", \"Complementos de pago\", \"Cancelaciones SAT\", \"Carta porte\", \"Export XML/PDF\", \"Envío automático por email\"]",
                    IncludedInPlans = "[\"Pro\", \"Enterprise\"]",
                    RequiredModules = "[\"invoicing-basic\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 2
                },

                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "Contabilidad",
                    Code = "finance-accounting",
                    Description = "Catálogo de cuentas, balance general, estado de resultados, conciliaciones",
                    Category = ModuleCategory.Finance,
                    MonthlyPrice = 49m,
                    YearlyPrice = 490m,
                    HasFreeTrial = true,
                    Features = "[\"Catálogo de cuentas\", \"Balance general\", \"Estado de resultados\", \"Flujo de efectivo\", \"Conciliación bancaria\", \"Export a Excel\", \"Reportes fiscales\"]",
                    IncludedInPlans = "[\"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 3
                },
                
                // === MARKETING ===
                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "Marketing Automation",
                    Code = "marketing-automation",
                    Description = "Campañas de email/SMS, workflows automáticos, segmentación avanzada",
                    Category = ModuleCategory.Marketing,
                    MonthlyPrice = 59m,
                    YearlyPrice = 590m,
                    HasFreeTrial = true,
                    Features = "[\"Campañas de email ilimitadas\", \"SMS marketing (500/mes)\", \"Workflows automáticos\", \"Segmentación avanzada\", \"A/B testing\", \"Landing pages\", \"Lead magnets\"]",
                    IncludedInPlans = "[\"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 4
                },
                
                // === INTEGRATION ===
                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "WhatsApp Business",
                    Code = "integration-whatsapp",
                    Description = "Integración con WhatsApp Business API, mensajes automáticos, chatbot",
                    Category = ModuleCategory.Integration,
                    MonthlyPrice = 19m,
                    YearlyPrice = 190m,
                    HasFreeTrial = true,
                    Features = "[\"WhatsApp Business API\", \"Mensajes automáticos\", \"Templates aprobados\", \"Chatbot básico\", \"1000 conversaciones/mes\", \"Integración con CRM\"]",
                    IncludedInPlans = "[\"Pro\", \"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 5
                },

                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "API Pública",
                    Code = "integration-public-api",
                    Description = "API REST completa para integraciones personalizadas, webhooks, documentación Swagger",
                    Category = ModuleCategory.Integration,
                    MonthlyPrice = 99m,
                    YearlyPrice = 990m,
                    HasFreeTrial = false,
                    Features = "[\"API REST completa\", \"10,000 requests/hora\", \"Webhooks ilimitados\", \"Documentación Swagger\", \"API keys\", \"Logs de requests\", \"Support prioritario\"]",
                    IncludedInPlans = "[\"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 6
                },
                
                // === ANALYTICS ===
                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "Reportes Avanzados",
                    Code = "analytics-reports",
                    Description = "Dashboards personalizados, reportes automáticos, export avanzado",
                    Category = ModuleCategory.Analytics,
                    MonthlyPrice = 29m,
                    YearlyPrice = 290m,
                    HasFreeTrial = true,
                    Features = "[\"Dashboards personalizados\", \"Reportes automáticos por email\", \"Export Excel/PDF avanzado\", \"Gráficas interactivas\", \"Comparativas mes/año\", \"KPIs personalizados\"]",
                    IncludedInPlans = "[\"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 7
                },
                
                // === AUTOMATION ===
                new ModuleAddon
                {
                    Id = Guid.NewGuid(),
                    Name = "Workflows Personalizados",
                    Code = "automation-workflows",
                    Description = "Automatizaciones visuales sin código, triggers, acciones, condiciones",
                    Category = ModuleCategory.Automation,
                    MonthlyPrice = 39m,
                    YearlyPrice = 390m,
                    HasFreeTrial = true,
                    Features = "[\"Workflow builder visual\", \"50 workflows activos\", \"Triggers personalizados\", \"Acciones multi-servicio\", \"Condiciones complejas\", \"Logs de ejecución\"]",
                    IncludedInPlans = "[\"Enterprise\"]",
                    IsActive = true,
                    IsVisible = true,
                    SortOrder = 8
                }
            };
        }
    }
}
