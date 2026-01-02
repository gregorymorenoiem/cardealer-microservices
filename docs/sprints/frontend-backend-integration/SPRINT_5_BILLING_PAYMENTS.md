# 💳 SPRINT 5 - Billing & Payments con Stripe

**Fecha:** 2 Enero 2026  
**Duración estimada:** 5-6 horas  
**Tokens estimados:** ~26,000  
**Prioridad:** 🔴 Crítica

---

## 🎯 OBJETIVOS

1. Integrar Stripe en BillingService
2. Implementar suscripciones para dealers
3. Crear sistema de featured listings (promociones)
4. Configurar webhooks de Stripe
5. Implementar frontend de pagos
6. Gestionar planes de suscripción
7. Crear dashboard de facturación

---

## 📋 CHECKLIST DE TAREAS

### Fase 1: Stripe Setup (30 min)

- [ ] 1.1. Crear cuenta Stripe (o usar existente)
- [ ] 1.2. Obtener API keys (test y production)
- [ ] 1.3. Crear productos y precios en Stripe
- [ ] 1.4. Configurar webhook endpoint
- [ ] 1.5. Configurar Stripe CLI para testing

### Fase 2: Backend - Subscription Plans (1 hora)

- [ ] 2.1. Definir entidades Subscription y Plan
- [ ] 2.2. Crear endpoints para planes
- [ ] 2.3. Implementar lógica de límites por plan
- [ ] 2.4. Agregar middleware de verificación de suscripción

### Fase 3: Backend - Stripe Integration (2 horas)

- [ ] 3.1. Instalar Stripe.NET SDK
- [ ] 3.2. Crear StripePaymentService
- [ ] 3.3. Implementar creación de checkout session
- [ ] 3.4. Implementar portal de clientes
- [ ] 3.5. Crear PaymentsController

### Fase 4: Backend - Webhooks (1.5 horas)

- [ ] 4.1. Crear endpoint webhook
- [ ] 4.2. Validar firmas de Stripe
- [ ] 4.3. Procesar eventos (payment_succeeded, subscription_updated, etc.)
- [ ] 4.4. Actualizar estados de suscripción

### Fase 5: Frontend - Payment UI (1 hora)

- [ ] 5.1. Instalar Stripe.js
- [ ] 5.2. Crear página de planes
- [ ] 5.3. Crear checkout page
- [ ] 5.4. Implementar success/cancel callbacks
- [ ] 5.5. Crear subscription dashboard

---

## 📝 IMPLEMENTACIÓN DETALLADA

### 1️⃣ Stripe - Crear Productos y Precios

**Pasos en Stripe Dashboard:**

1. Ir a **Dashboard** → https://dashboard.stripe.com/test/products
2. Click **Add product**
3. Crear 3 planes:

#### Plan FREE
- **Name:** CarDealer Free
- **Description:** Plan gratuito con funciones básicas
- **Pricing:**
  - **Type:** Recurring
  - **Price:** $0 USD
  - **Billing period:** Monthly
- **Features:**
  - 5 vehículos activos
  - Imágenes estándar
  - Sin featured listings

#### Plan BASIC ($29/mes)
- **Name:** CarDealer Basic
- **Description:** Plan para dealers pequeños
- **Pricing:**
  - **Type:** Recurring
  - **Price:** $29 USD
  - **Billing period:** Monthly
- **Features:**
  - 25 vehículos activos
  - Imágenes HD
  - 3 featured listings/mes
  - Soporte por email

#### Plan PRO ($99/mes)
- **Name:** CarDealer Pro
- **Description:** Plan para dealers profesionales
- **Pricing:**
  - **Type:** Recurring
  - **Price:** $99 USD
  - **Billing period:** Monthly
- **Features:**
  - Vehículos ilimitados
  - Imágenes 4K
  - 10 featured listings/mes
  - Soporte prioritario
  - Analytics avanzado

4. **Guardar Price IDs:**
   - FREE: `price_xxx`
   - BASIC: `price_yyy`
   - PRO: `price_zzz`

---

### 2️⃣ Backend - Subscription Entity

**Archivo:** `backend/BillingService/BillingService.Domain/Entities/Subscription.cs`

```csharp
using CarDealer.Shared.MultiTenancy;

namespace BillingService.Domain.Entities;

public class Subscription : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }  // Multi-tenant
    
    // Stripe
    public string StripeCustomerId { get; set; } = string.Empty;
    public string? StripeSubscriptionId { get; set; }
    public string? StripePriceId { get; set; }
    
    // Plan
    public PlanType PlanType { get; set; } = PlanType.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    
    // Billing
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? CancelAt { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    
    // Usage Limits (reset monthly)
    public int MaxVehicles { get; set; }
    public int UsedVehicles { get; set; }
    public int MaxFeaturedListings { get; set; }
    public int UsedFeaturedListings { get; set; }
    
    // Payments
    public decimal MonthlyPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum PlanType
{
    Free,
    Basic,
    Pro
}

public enum SubscriptionStatus
{
    Active,
    PastDue,
    Canceled,
    Incomplete,
    Trialing
}

// Plan Configuration
public static class PlanConfiguration
{
    public static readonly Dictionary<PlanType, PlanLimits> Plans = new()
    {
        {
            PlanType.Free,
            new PlanLimits
            {
                MaxVehicles = 5,
                MaxFeaturedListings = 0,
                Price = 0,
                Features = new[] { "5 vehículos", "Imágenes estándar" }
            }
        },
        {
            PlanType.Basic,
            new PlanLimits
            {
                MaxVehicles = 25,
                MaxFeaturedListings = 3,
                Price = 29,
                Features = new[] { "25 vehículos", "Imágenes HD", "3 destacados/mes", "Soporte email" }
            }
        },
        {
            PlanType.Pro,
            new PlanLimits
            {
                MaxVehicles = int.MaxValue,
                MaxFeaturedListings = 10,
                Price = 99,
                Features = new[] { "Vehículos ilimitados", "Imágenes 4K", "10 destacados/mes", "Soporte prioritario", "Analytics" }
            }
        }
    };
}

public class PlanLimits
{
    public int MaxVehicles { get; set; }
    public int MaxFeaturedListings { get; set; }
    public decimal Price { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
}
```

---

### 3️⃣ Backend - Stripe Payment Service

**Archivo:** `backend/BillingService/BillingService.Infrastructure/Services/StripePaymentService.cs`

```csharp
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BillingService.Domain.Entities;

namespace BillingService.Infrastructure.Services;

public interface IStripePaymentService
{
    Task<string> CreateCheckoutSessionAsync(Guid dealerId, PlanType planType, string successUrl, string cancelUrl);
    Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl);
    Task<Customer> CreateCustomerAsync(string email, string name, Dictionary<string, string> metadata);
    Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId);
    Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool immediately = false);
}

public class StripePaymentService : IStripePaymentService
{
    private readonly ILogger<StripePaymentService> _logger;
    private readonly string _secretKey;
    private readonly Dictionary<PlanType, string> _priceIds;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _logger = logger;
        _secretKey = configuration["Stripe:SecretKey"] 
            ?? throw new ArgumentNullException("Stripe:SecretKey not configured");
        
        StripeConfiguration.ApiKey = _secretKey;

        // Load price IDs from configuration
        _priceIds = new Dictionary<PlanType, string>
        {
            { PlanType.Free, "" }, // No price ID for free plan
            { PlanType.Basic, configuration["Stripe:PriceIds:Basic"] ?? "" },
            { PlanType.Pro, configuration["Stripe:PriceIds:Pro"] ?? "" }
        };
    }

    public async Task<string> CreateCheckoutSessionAsync(
        Guid dealerId,
        PlanType planType,
        string successUrl,
        string cancelUrl)
    {
        try
        {
            if (planType == PlanType.Free)
                throw new InvalidOperationException("Cannot create checkout for free plan");

            var priceId = _priceIds[planType];
            if (string.IsNullOrEmpty(priceId))
                throw new InvalidOperationException($"Price ID not configured for {planType}");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    }
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "dealer_id", dealerId.ToString() },
                    { "plan_type", planType.ToString() }
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "dealer_id", dealerId.ToString() },
                        { "plan_type", planType.ToString() }
                    }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation(
                "Created checkout session {SessionId} for dealer {DealerId}",
                session.Id,
                dealerId);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session");
            throw;
        }
    }

    public async Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl)
    {
        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl,
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating portal session");
            throw;
        }
    }

    public async Task<Customer> CreateCustomerAsync(
        string email,
        string name,
        Dictionary<string, string> metadata)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = metadata
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            _logger.LogInformation("Created Stripe customer {CustomerId} for {Email}", customer.Id, email);

            return customer;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating customer");
            throw;
        }
    }

    public async Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId)
    {
        var service = new SubscriptionService();
        return await service.GetAsync(subscriptionId);
    }

    public async Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, bool immediately = false)
    {
        try
        {
            var service = new SubscriptionService();
            
            if (immediately)
            {
                return await service.CancelAsync(subscriptionId);
            }
            else
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };
                return await service.UpdateAsync(subscriptionId, options);
            }
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error canceling subscription");
            throw;
        }
    }
}
```

**Instalar paquete NuGet:**

```xml
<PackageReference Include="Stripe.net" Version="44.13.0" />
```

---

### 4️⃣ Backend - Payments Controller

**Archivo:** `backend/BillingService/BillingService.Api/Controllers/PaymentsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BillingService.Infrastructure.Services;
using BillingService.Domain.Entities;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IStripePaymentService _stripeService;
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IStripePaymentService stripeService,
        IMediator mediator,
        ILogger<PaymentsController> logger)
    {
        _stripeService = stripeService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get available subscription plans
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
    public IActionResult GetPlans()
    {
        var plans = PlanConfiguration.Plans.Select(p => new PlanDto
        {
            Type = p.Key.ToString(),
            Name = p.Key.ToString(),
            Price = p.Value.Price,
            MaxVehicles = p.Value.MaxVehicles,
            MaxFeaturedListings = p.Value.MaxFeaturedListings,
            Features = p.Value.Features
        }).ToList();

        return Ok(plans);
    }

    /// <summary>
    /// Create checkout session
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutRequest request)
    {
        try
        {
            var dealerId = Guid.Parse(User.FindFirst("dealer_id")?.Value ?? "");
            
            var checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(
                dealerId,
                Enum.Parse<PlanType>(request.PlanType),
                request.SuccessUrl,
                request.CancelUrl
            );

            return Ok(new CheckoutResponse { CheckoutUrl = checkoutUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create checkout session");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create customer portal session
    /// </summary>
    [HttpPost("portal")]
    [Authorize]
    [ProducesResponseType(typeof(PortalResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePortal([FromBody] CreatePortalRequest request)
    {
        try
        {
            // Get dealer's subscription
            var dealerId = Guid.Parse(User.FindFirst("dealer_id")?.Value ?? "");
            var query = new GetSubscriptionQuery(dealerId);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess || result.Value == null)
                return NotFound(new { error = "Subscription not found" });

            var portalUrl = await _stripeService.CreatePortalSessionAsync(
                result.Value.StripeCustomerId,
                request.ReturnUrl
            );

            return Ok(new PortalResponse { PortalUrl = portalUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create portal session");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current subscription
    /// </summary>
    [HttpGet("subscription")]
    [Authorize]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscription()
    {
        var dealerId = Guid.Parse(User.FindFirst("dealer_id")?.Value ?? "");
        var query = new GetSubscriptionQuery(dealerId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

public record CreateCheckoutRequest(string PlanType, string SuccessUrl, string CancelUrl);
public record CheckoutResponse(string CheckoutUrl);
public record CreatePortalRequest(string ReturnUrl);
public record PortalResponse(string PortalUrl);

public class PlanDto
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxVehicles { get; set; }
    public int MaxFeaturedListings { get; set; }
    public string[] Features { get; set; } = Array.Empty<string>();
}
```

---

### 5️⃣ Backend - Stripe Webhooks

**Archivo:** `backend/BillingService/BillingService.Api/Controllers/WebhooksController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using MediatR;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhooksController> _logger;
    private readonly string _webhookSecret;

    public WebhooksController(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<WebhooksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? "";
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _webhookSecret
            );

            _logger.LogInformation("Received Stripe webhook: {EventType}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionCompleted:
                    var session = stripeEvent.Data.Object as Session;
                    await HandleCheckoutCompleted(session!);
                    break;

                case Events.CustomerSubscriptionUpdated:
                    var subscription = stripeEvent.Data.Object as Subscription;
                    await HandleSubscriptionUpdated(subscription!);
                    break;

                case Events.CustomerSubscriptionDeleted:
                    var deletedSubscription = stripeEvent.Data.Object as Subscription;
                    await HandleSubscriptionDeleted(deletedSubscription!);
                    break;

                case Events.InvoicePaymentSucceeded:
                    var invoice = stripeEvent.Data.Object as Invoice;
                    await HandlePaymentSucceeded(invoice!);
                    break;

                case Events.InvoicePaymentFailed:
                    var failedInvoice = stripeEvent.Data.Object as Invoice;
                    await HandlePaymentFailed(failedInvoice!);
                    break;

                default:
                    _logger.LogWarning("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
    }

    private async Task HandleCheckoutCompleted(Session session)
    {
        var dealerId = Guid.Parse(session.Metadata["dealer_id"]);
        var planType = Enum.Parse<PlanType>(session.Metadata["plan_type"]);

        var command = new CreateSubscriptionCommand
        {
            DealerId = dealerId,
            StripeCustomerId = session.CustomerId,
            StripeSubscriptionId = session.SubscriptionId,
            PlanType = planType
        };

        await _mediator.Send(command);
    }

    private async Task HandleSubscriptionUpdated(Subscription subscription)
    {
        var dealerId = Guid.Parse(subscription.Metadata["dealer_id"]);

        var command = new UpdateSubscriptionCommand
        {
            DealerId = dealerId,
            Status = subscription.Status,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd
        };

        await _mediator.Send(command);
    }

    private async Task HandleSubscriptionDeleted(Subscription subscription)
    {
        var dealerId = Guid.Parse(subscription.Metadata["dealer_id"]);

        var command = new CancelSubscriptionCommand { DealerId = dealerId };
        await _mediator.Send(command);
    }

    private async Task HandlePaymentSucceeded(Invoice invoice)
    {
        // Update payment date
        _logger.LogInformation("Payment succeeded for customer {CustomerId}", invoice.CustomerId);
    }

    private async Task HandlePaymentFailed(Invoice invoice)
    {
        // Send notification about failed payment
        _logger.LogWarning("Payment failed for customer {CustomerId}", invoice.CustomerId);
    }
}
```

---

### 6️⃣ Frontend - Plans Page

**Archivo:** `frontend/web/original/src/pages/PlansPage.tsx`

```typescript
import { useState, type FC } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Check, Loader2 } from 'lucide-react';
import { api } from '@/services/api';
import { useAuthStore } from '@/stores/authStore';
import toast from 'react-hot-toast';

interface Plan {
  type: string;
  name: string;
  price: number;
  maxVehicles: number;
  maxFeaturedListings: number;
  features: string[];
}

export const PlansPage: FC = () => {
  const { user } = useAuthStore();
  const [loading, setLoading] = useState<string | null>(null);

  const { data: plans, isLoading } = useQuery({
    queryKey: ['plans'],
    queryFn: async () => {
      const response = await api.get<Plan[]>('/payments/plans');
      return response.data;
    },
  });

  const handleSubscribe = async (planType: string) => {
    if (!user) {
      toast.error('Debes iniciar sesión');
      return;
    }

    setLoading(planType);

    try {
      const response = await api.post<{ checkoutUrl: string }>('/payments/checkout', {
        planType,
        successUrl: `${window.location.origin}/subscription/success`,
        cancelUrl: `${window.location.origin}/subscription/cancel`,
      });

      // Redirect to Stripe Checkout
      window.location.href = response.data.checkoutUrl;
    } catch (error: any) {
      toast.error(error.message || 'Error al crear checkout');
      setLoading(null);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-12">
      <div className="text-center mb-12">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          Elige el Plan Perfecto para tu Negocio
        </h1>
        <p className="text-xl text-gray-600">
          Planes diseñados para dealers de todos los tamaños
        </p>
      </div>

      <div className="grid md:grid-cols-3 gap-8">
        {plans?.map((plan) => (
          <div
            key={plan.type}
            className={`
              relative rounded-2xl p-8 border-2
              ${plan.type === 'Pro' 
                ? 'border-blue-600 shadow-xl scale-105' 
                : 'border-gray-200'
              }
            `}
          >
            {plan.type === 'Pro' && (
              <div className="absolute top-0 left-1/2 transform -translate-x-1/2 -translate-y-1/2">
                <span className="bg-blue-600 text-white px-4 py-1 rounded-full text-sm font-medium">
                  Más Popular
                </span>
              </div>
            )}

            <div className="mb-8">
              <h3 className="text-2xl font-bold text-gray-900 mb-2">
                {plan.name}
              </h3>
              <div className="flex items-baseline">
                <span className="text-5xl font-bold text-gray-900">
                  ${plan.price}
                </span>
                {plan.price > 0 && (
                  <span className="text-gray-600 ml-2">/mes</span>
                )}
              </div>
            </div>

            <ul className="space-y-4 mb-8">
              {plan.features.map((feature, index) => (
                <li key={index} className="flex items-start gap-3">
                  <Check className="w-5 h-5 text-green-600 mt-0.5 flex-shrink-0" />
                  <span className="text-gray-700">{feature}</span>
                </li>
              ))}
            </ul>

            <button
              onClick={() => handleSubscribe(plan.type)}
              disabled={loading === plan.type || plan.type === 'Free'}
              className={`
                w-full py-3 px-6 rounded-lg font-medium
                transition-colors disabled:opacity-50
                ${plan.type === 'Pro'
                  ? 'bg-blue-600 text-white hover:bg-blue-700'
                  : 'bg-gray-900 text-white hover:bg-gray-800'
                }
              `}
            >
              {loading === plan.type ? (
                <Loader2 className="w-5 h-5 animate-spin mx-auto" />
              ) : plan.type === 'Free' ? (
                'Plan Actual'
              ) : (
                'Comenzar Ahora'
              )}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
};
```

---

## ✅ CRITERIOS DE ACEPTACIÓN

### Test Backend

```bash
# Get plans
curl http://localhost:15008/api/payments/plans

# Create checkout (con JWT)
curl -X POST http://localhost:15008/api/payments/checkout \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "planType": "Basic",
    "successUrl": "http://localhost:5174/success",
    "cancelUrl": "http://localhost:5174/cancel"
  }'
```

### Test Frontend

1. Ir a http://localhost:5174/plans
2. Ver 3 planes (Free, Basic, Pro)
3. Login como dealer
4. Click "Comenzar Ahora" en plan Basic
5. Redirigir a Stripe Checkout
6. Completar pago con tarjeta de prueba: `4242 4242 4242 4242`
7. Redirigir a success page
8. Verificar suscripción activa

---

## 📊 ESTIMACIÓN DE TOKENS

| Tarea | Tokens |
|-------|--------|
| Stripe setup guide | 3,000 |
| Subscription entities | 3,500 |
| StripePaymentService | 5,000 |
| PaymentsController | 4,000 |
| WebhooksController | 4,500 |
| Frontend Plans page | 4,000 |
| Testing | 2,000 |
| **TOTAL** | **~26,000** |

**Con buffer 15%:** ~30,000 tokens

---

## ➡️ PRÓXIMO SPRINT

**Sprint 6:** Notifications (Email, SMS, Push)

---

**Estado:** ⚪ Pendiente  
**Última actualización:** 2 Enero 2026
