namespace BillingService.Shared.DTOs;

// ========================================
// STRIPE CUSTOMER DTOs
// ========================================

/// <summary>
/// Request para crear un cliente en Stripe
/// </summary>
public record CreateStripeCustomerRequest(
    Guid DealerId,
    string Email,
    string Name,
    string? Phone = null,
    Dictionary<string, string>? Metadata = null
);

/// <summary>
/// Respuesta de la creación de cliente en Stripe
/// </summary>
public record StripeCustomerResponse(
    Guid Id,
    Guid DealerId,
    string StripeCustomerId,
    string Email,
    string Name,
    string? DefaultPaymentMethodId,
    bool IsTestMode,
    DateTime CreatedAt
);

// ========================================
// SUBSCRIPTION DTOs
// ========================================

/// <summary>
/// Request para crear una suscripción
/// </summary>
public record CreateSubscriptionRequest(
    Guid DealerId,
    string PlanCode,  // "free", "basic", "professional", "enterprise"
    string BillingCycle,  // "monthly", "yearly"
    string? PaymentMethodId = null,
    bool EnableTrial = true,
    int TrialDays = 14,
    string? PromotionCode = null
);

/// <summary>
/// Request para actualizar suscripción (upgrade/downgrade)
/// </summary>
public record UpdateSubscriptionRequest(
    string NewPlanCode,
    string? NewBillingCycle = null,
    bool ProrationEnabled = true
);

/// <summary>
/// Request para cancelar suscripción
/// </summary>
public record CancelSubscriptionRequest(
    string? Reason = null,
    bool CancelImmediately = false
);

/// <summary>
/// Respuesta de suscripción
/// </summary>
public record SubscriptionResponse(
    Guid Id,
    Guid DealerId,
    string Plan,
    string Status,
    string BillingCycle,
    decimal Price,
    string? StripeSubscriptionId,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    DateTime? TrialEndDate,
    DateTime CreatedAt
);

// ========================================
// CHECKOUT & PORTAL DTOs
// ========================================

/// <summary>
/// Request para crear sesión de checkout
/// </summary>
public record CreateCheckoutSessionRequest(
    Guid DealerId,
    string PlanCode,
    string BillingCycle,
    string SuccessUrl,
    string CancelUrl
);

/// <summary>
/// Respuesta de sesión de checkout
/// </summary>
public record CheckoutSessionResponse(
    string SessionId,
    string Url
);

/// <summary>
/// Request para crear sesión del portal de billing
/// </summary>
public record CreateBillingPortalRequest(
    Guid DealerId,
    string ReturnUrl
);

/// <summary>
/// Respuesta del portal de billing
/// </summary>
public record BillingPortalResponse(
    string Url
);

// ========================================
// PRICING DTOs
// ========================================

/// <summary>
/// Información de un plan de precios
/// </summary>
public record PlanPricingInfo(
    string PlanCode,
    string PlanName,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxUsers,
    int MaxVehicles,
    int MaxProperties,
    List<string> IncludedFeatures,
    bool IsPopular = false
);

/// <summary>
/// Respuesta con todos los planes disponibles
/// </summary>
public record PricingResponse(
    List<PlanPricingInfo> Plans,
    string Currency,
    int DefaultTrialDays
);

// ========================================
// INVOICE & PAYMENT DTOs
// ========================================

/// <summary>
/// Respuesta de factura
/// </summary>
public record InvoiceResponse(
    Guid Id,
    Guid DealerId,
    string InvoiceNumber,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    string Status,
    string? StripeInvoiceId,
    string? PdfUrl,
    DateTime DueDate,
    DateTime? PaidAt,
    DateTime CreatedAt
);

/// <summary>
/// Respuesta de pago
/// </summary>
public record PaymentResponse(
    Guid Id,
    Guid DealerId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string? StripePaymentIntentId,
    string? ReceiptUrl,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

// ========================================
// EVENTS (para integración entre servicios)
// ========================================

/// <summary>
/// Evento cuando se crea un customer en Stripe
/// </summary>
public record StripeCustomerCreatedEvent(
    Guid DealerId,
    string StripeCustomerId,
    string Email,
    bool IsTestMode,
    DateTime CreatedAt
);

/// <summary>
/// Evento cuando cambia el estado de una suscripción
/// </summary>
public record SubscriptionStatusChangedEvent(
    Guid DealerId,
    Guid SubscriptionId,
    string StripeSubscriptionId,
    string Plan,
    string OldStatus,
    string NewStatus,
    DateTime? TrialEndDate,
    DateTime? CurrentPeriodEnd,
    DateTime ChangedAt
);

/// <summary>
/// Evento cuando se procesa un pago exitosamente
/// </summary>
public record PaymentSucceededEvent(
    Guid DealerId,
    Guid PaymentId,
    string StripePaymentIntentId,
    decimal Amount,
    string Currency,
    DateTime PaidAt
);

/// <summary>
/// Evento cuando falla un pago
/// </summary>
public record PaymentFailedEvent(
    Guid DealerId,
    string StripePaymentIntentId,
    decimal Amount,
    string FailureReason,
    DateTime FailedAt
);
