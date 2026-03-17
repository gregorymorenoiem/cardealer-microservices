using BillingService.Domain.Entities;

namespace BillingService.Application.DTOs;

// ========================================
// STRIPE REQUEST DTOs
// ========================================

/// <summary>
/// DTO para crear un cliente en Stripe
/// </summary>
public record CreateStripeCustomerRequest(
    Guid DealerId,
    string Email,
    string Name,
    string? Phone = null,
    Dictionary<string, string>? Metadata = null
);

/// <summary>
/// DTO para crear una suscripción con Stripe
/// </summary>
public record CreateStripeSubscriptionRequest(
    Guid DealerId,
    SubscriptionPlan Plan,
    BillingCycle Cycle,
    string? PaymentMethodId = null,
    bool EnableTrial = true,
    int TrialDays = 14,
    string? PromotionCode = null
);

/// <summary>
/// DTO para actualizar una suscripción (upgrade/downgrade)
/// </summary>
public record UpdateStripeSubscriptionRequest(
    Guid SubscriptionId,
    SubscriptionPlan NewPlan,
    BillingCycle? NewCycle = null,
    bool ProrationEnabled = true
);

/// <summary>
/// DTO para cancelar una suscripción
/// </summary>
public record CancelStripeSubscriptionRequest(
    Guid SubscriptionId,
    string? Reason = null,
    bool CancelImmediately = false
);

/// <summary>
/// DTO para adjuntar un método de pago
/// </summary>
public record AttachPaymentMethodRequest(
    Guid DealerId,
    string PaymentMethodId,
    bool SetAsDefault = true
);

/// <summary>
/// DTO para crear un PaymentIntent
/// </summary>
public record CreatePaymentIntentRequest(
    Guid DealerId,
    decimal Amount,
    string Currency = "usd",
    string? Description = null,
    Dictionary<string, string>? Metadata = null
);

/// <summary>
/// DTO para crear una sesión de Checkout
/// </summary>
public record CreateCheckoutSessionRequest(
    Guid DealerId,
    SubscriptionPlan Plan,
    BillingCycle Cycle,
    string SuccessUrl,
    string CancelUrl,
    bool EnableTrial = true,
    int TrialDays = 14
);

/// <summary>
/// DTO para crear un portal de billing
/// </summary>
public record CreateBillingPortalSessionRequest(
    Guid DealerId,
    string ReturnUrl
);

// ========================================
// RESPONSE DTOs
// ========================================

/// <summary>
/// DTO de respuesta para cliente de Stripe
/// </summary>
public record StripeCustomerResponse(
    Guid Id,
    Guid DealerId,
    string StripeCustomerId,
    string Email,
    string Name,
    string? Phone,
    string? DefaultPaymentMethodId,
    List<PaymentMethodInfo> PaymentMethods,
    bool IsActive,
    DateTime CreatedAt
);

/// <summary>
/// DTO de información de método de pago
/// </summary>
public record PaymentMethodInfo(
    string Id,
    string Type,
    string? Brand,
    string? Last4,
    int? ExpMonth,
    int? ExpYear,
    bool IsDefault
);

/// <summary>
/// DTO de respuesta para suscripción
/// </summary>
public record SubscriptionResponse(
    Guid Id,
    Guid DealerId,
    SubscriptionPlan Plan,
    SubscriptionStatus Status,
    BillingCycle Cycle,
    decimal PricePerCycle,
    string Currency,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime? TrialEndDate,
    DateTime? NextBillingDate,
    string? StripeSubscriptionId,
    int MaxUsers,
    int MaxVehicles,
    DateTime CreatedAt
);

/// <summary>
/// DTO de respuesta para checkout session
/// </summary>
public record CheckoutSessionResponse(
    string SessionId,
    string CheckoutUrl
);

/// <summary>
/// DTO de respuesta para billing portal
/// </summary>
public record BillingPortalSessionResponse(
    string SessionId,
    string PortalUrl
);

/// <summary>
/// DTO de respuesta para PaymentIntent
/// </summary>
public record PaymentIntentResponse(
    string PaymentIntentId,
    string? ClientSecret,
    string Status,
    decimal Amount,
    string Currency
);

/// <summary>
/// DTO para información de precios de un plan
/// </summary>
public record PlanPricingInfo(
    SubscriptionPlan Plan,
    string PlanName,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    decimal YearlyDiscount,
    int MaxUsers,
    int MaxVehicles,
    List<string> Features,
    bool RecommendedForUpgrade
);

/// <summary>
/// DTO de resumen de billing para un dealer
/// </summary>
public record BillingSummaryResponse(
    SubscriptionResponse? CurrentSubscription,
    StripeCustomerResponse? StripeCustomer,
    List<InvoiceResponse> RecentInvoices,
    List<PaymentResponse> RecentPayments,
    decimal TotalPaidAmount,
    decimal OutstandingBalance,
    DateTime? NextPaymentDate,
    decimal? NextPaymentAmount
);

/// <summary>
/// DTO de resumen de uso vs límites
/// </summary>
public record UsageSummaryResponse(
    Guid DealerId,
    SubscriptionPlan Plan,
    int UsedUsers,
    int MaxUsers,
    int UsedVehicles,
    int MaxVehicles,
    decimal UsersPercentage,
    decimal VehiclesPercentage,
    bool IsNearLimit,
    bool ShouldUpgrade
);

// ========================================
// STRIPE PRICE MAPPING
// ========================================

/// <summary>
/// Mapeo de planes a Price IDs de Stripe
/// </summary>
public static class StripePriceMapping
{
    // En producción, estos valores vendrían de configuración
    private static readonly Dictionary<(SubscriptionPlan, BillingCycle), string> PriceIds = new()
    {
        { (SubscriptionPlan.Free, BillingCycle.Monthly), "" },
        { (SubscriptionPlan.Free, BillingCycle.Yearly), "" },
        { (SubscriptionPlan.Basic, BillingCycle.Monthly), "price_basic_monthly" },
        { (SubscriptionPlan.Basic, BillingCycle.Yearly), "price_basic_yearly" },
        { (SubscriptionPlan.Professional, BillingCycle.Monthly), "price_professional_monthly" },
        { (SubscriptionPlan.Professional, BillingCycle.Yearly), "price_professional_yearly" },
        { (SubscriptionPlan.Enterprise, BillingCycle.Monthly), "price_enterprise_monthly" },
        { (SubscriptionPlan.Enterprise, BillingCycle.Yearly), "price_enterprise_yearly" },
        { (SubscriptionPlan.Starter, BillingCycle.Monthly), "price_starter_monthly" },
        { (SubscriptionPlan.Starter, BillingCycle.Yearly), "price_starter_yearly" },
        { (SubscriptionPlan.Corporate, BillingCycle.Monthly), "price_corporate_monthly" },
        { (SubscriptionPlan.Corporate, BillingCycle.Yearly), "price_corporate_yearly" }
    };

    private static readonly Dictionary<SubscriptionPlan, (decimal Monthly, decimal Yearly)> Prices = new()
    {
        { SubscriptionPlan.Free, (0m, 0m) },
        { SubscriptionPlan.Basic, (29m, 290m) },         // ~17% descuento anual
        { SubscriptionPlan.Starter, (59m, 590m) },       // NEW: STARTER plan
        { SubscriptionPlan.Professional, (99m, 990m) },  // ~17% descuento anual
        { SubscriptionPlan.Enterprise, (349m, 3490m) },  // ÉLITE — was $249, updated to $349
        { SubscriptionPlan.Corporate, (599m, 5990m) }    // NEW: ENTERPRISE tier
    };

    private static readonly Dictionary<SubscriptionPlan, (int Users, int Vehicles)> Limits = new()
    {
        // OKLA Plan Table: ALL plans have ∞ UNLIMITED vehicle listings
        // Differentiation is via premium features (photos, search priority, ChatAgent, etc.)
        { SubscriptionPlan.Free, (1, -1) },          // ∞ listings, 1 user
        { SubscriptionPlan.Basic, (5, -1) },          // ∞ listings, 5 users
        { SubscriptionPlan.Starter, (10, -1) },       // ∞ listings, 10 users — NEW
        { SubscriptionPlan.Professional, (20, -1) },  // ∞ listings, 20 users
        { SubscriptionPlan.Enterprise, (-1, -1) },    // ∞ listings, ∞ users (ÉLITE)
        { SubscriptionPlan.Corporate, (-1, -1) }      // ∞ listings, ∞ users — NEW ENTERPRISE
    };

    private static readonly Dictionary<SubscriptionPlan, List<string>> Features = new()
    {
        { SubscriptionPlan.Free, new List<string>
            {
                "Publicaciones ilimitadas de vehículos",
                "Hasta 10 fotos por vehículo",
                "Posición estándar en búsquedas",
                "1 valoración IA gratis",
                "Perfil público básico",
                "Soporte FAQ"
            }
        },
        { SubscriptionPlan.Basic, new List<string>
            {
                "Publicaciones ilimitadas de vehículos",
                "Hasta 10 fotos por vehículo",
                "Prioridad media en búsquedas",
                "3 vehículos destacados/mes",
                "$15 créditos publicitarios/mes",
                "Badge 'Dealer Verificado OKLA'",
                "Dashboard Analytics básico",
                "5 valoraciones IA/mes",
                "Perfil público mejorado",
                "5 usuarios",
                "Soporte email 48h"
            }
        },
        { SubscriptionPlan.Starter, new List<string>
            {
                "Publicaciones ilimitadas de vehículos",
                "Hasta 12 fotos por vehículo",
                "Alta prioridad en búsquedas",
                "5 vehículos destacados/mes",
                "$30 créditos publicitarios/mes",
                "Badge Verificado+",
                "ChatAgent Web 100 conv/mes",
                "ChatAgent WhatsApp 100 conv/mes",
                "Overage $0.10/conv adicional",
                "Dashboard Analytics básico",
                "10 usuarios",
                "Soporte email prioritario"
            }
        },
        { SubscriptionPlan.Professional, new List<string>
            {
                "Publicaciones ilimitadas de vehículos",
                "Hasta 15 fotos por vehículo",
                "Alta prioridad en búsquedas",
                "10 vehículos destacados/mes",
                "$45 créditos publicitarios/mes",
                "Badge 'Dealer Verificado OKLA'",
                "ChatAgent Web 300 conv/mes",
                "ChatAgent WhatsApp 300 conv/mes",
                "Agendamiento de citas automático",
                "Human handoff email alert",
                "Valoración IA ilimitada",
                "Dashboard Analytics avanzado",
                "Perfil público premium",
                "20 usuarios",
                "Soporte chat 12h"
            }
        },
        { SubscriptionPlan.Enterprise, new List<string>
            {
                "Publicaciones ilimitadas de vehículos",
                "Hasta 20 fotos + video tour",
                "Top prioridad en búsquedas",
                "25 vehículos destacados/mes",
                "$120 créditos publicitarios/mes",
                "Badge Verificado Premium",
                "ChatAgent Web 5,000 conv/mes",
                "ChatAgent WhatsApp 5,000 conv/mes",
                "Agendamiento + recordatorios WA",
                "Live chat + CRM handoff",
                "Valoración IA ilimitada + informe PDF",
                "Dashboard Analytics completo + exportar",
                "Perfil premium + showcase homepage",
                "Usuarios ilimitados",
                "Soporte dedicado 4h"
            }
        },
        { SubscriptionPlan.Corporate, new List<string>
            {
                "Publicaciones ilimitadas de vehículos",
                "Hasta 20 fotos + video tour",
                "#1 GARANTIZADO en búsquedas",
                "50 vehículos destacados/mes",
                "$300 créditos publicitarios/mes",
                "Badge Enterprise",
                "ChatAgent SIN LÍMITE",
                "Agendamiento + CRM completo + recordatorios WA",
                "Acceso completo a API OKLA",
                "Dashboard + API + reportes custom",
                "Empleados ilimitados",
                "Manager dedicado + SLA garantizado",
                "Soporte 24/7"
            }
        }
    };

    public static string GetPriceId(SubscriptionPlan plan, BillingCycle cycle)
    {
        return PriceIds.TryGetValue((plan, cycle), out var priceId) ? priceId : "";
    }

    public static decimal GetPrice(SubscriptionPlan plan, BillingCycle cycle)
    {
        if (!Prices.TryGetValue(plan, out var prices)) return 0;
        return cycle == BillingCycle.Yearly ? prices.Yearly : prices.Monthly;
    }

    public static (int MaxUsers, int MaxVehicles) GetLimits(SubscriptionPlan plan)
    {
        return Limits.TryGetValue(plan, out var limits) ? limits : (1, 3);
    }

    public static List<string> GetFeatures(SubscriptionPlan plan)
    {
        return Features.TryGetValue(plan, out var features) ? features : new List<string>();
    }

    public static List<PlanPricingInfo> GetAllPricing(SubscriptionPlan? currentPlan = null)
    {
        return Enum.GetValues<SubscriptionPlan>()
            .Where(p => p != SubscriptionPlan.Custom)
            .Select(plan =>
            {
                var (maxUsers, maxVehicles) = GetLimits(plan);
                var monthlyPrice = GetPrice(plan, BillingCycle.Monthly);
                var yearlyPrice = GetPrice(plan, BillingCycle.Yearly);
                var yearlyDiscount = monthlyPrice > 0
                    ? (1 - (yearlyPrice / (monthlyPrice * 12))) * 100
                    : 0;

                return new PlanPricingInfo(
                    plan,
                    plan.ToString(),
                    monthlyPrice,
                    yearlyPrice,
                    Math.Round(yearlyDiscount, 1),
                    maxUsers,
                    maxVehicles,
                    GetFeatures(plan),
                    currentPlan.HasValue && plan > currentPlan.Value
                );
            })
            .ToList();
    }
}

// ========================================
// BILLING RESPONSE DTOs
// ========================================

/// <summary>
/// DTO de respuesta para factura
/// </summary>
public record InvoiceResponse(
    Guid Id,
    Guid DealerId,
    string InvoiceNumber,
    string Status,
    decimal Subtotal,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    DateTime? IssueDate,
    DateTime DueDate,
    DateTime? PaidDate,
    string? PdfUrl
);

/// <summary>
/// DTO de respuesta para pago
/// </summary>
public record PaymentResponse(
    Guid Id,
    Guid DealerId,
    decimal Amount,
    string Status,
    string PaymentMethod,
    string? Description,
    string? ReceiptUrl,
    DateTime? ProcessedAt,
    DateTime CreatedAt
);
