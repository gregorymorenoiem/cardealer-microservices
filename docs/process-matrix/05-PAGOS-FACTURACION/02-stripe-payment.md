# 💳 Stripe Payment Service - Matriz de Procesos

> **Servicio:** StripePaymentService (BillingService)  
> **Puerto:** 5008  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente       | Total | Implementado | Pendiente | Estado |
| ---------------- | ----- | ------------ | --------- | ------ |
| Controllers      | 1     | 0            | 1         | 🔴     |
| STRIPE-PAY-\*    | 8     | 0            | 8         | 🔴     |
| STRIPE-SUB-\*    | 6     | 0            | 6         | 🔴     |
| STRIPE-WH-\*     | 5     | 0            | 5         | 🔴     |
| STRIPE-WALLET-\* | 4     | 0            | 4         | 🔴     |
| Tests            | 0     | 0            | 15        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Integración con Stripe para procesar pagos internacionales (tarjetas no dominicanas), Apple Pay, Google Pay y gestión de suscripciones de dealers. Stripe es la pasarela secundaria, mientras AZUL es la primaria para tarjetas locales.

### 1.2 Características

| Feature                  | Soportado | Notas                  |
| ------------------------ | --------- | ---------------------- |
| Tarjetas Internacionales | ✅        | Visa, MC, Amex         |
| Apple Pay                | ✅        | Web + iOS              |
| Google Pay               | ✅        | Web + Android          |
| Suscripciones            | ✅        | Dealers                |
| Webhooks                 | ✅        | Eventos automáticos    |
| Disputes                 | ✅        | Gestión de chargebacks |
| 3D Secure                | ✅        | SCA compliance         |

### 1.3 Dependencias

| Servicio            | Propósito                |
| ------------------- | ------------------------ |
| UserService         | Datos del customer       |
| DealerService       | Suscripciones de dealers |
| NotificationService | Confirmaciones de pago   |
| InvoiceService      | Generación de facturas   |

### 1.4 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Stripe Payment Architecture                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Frontend                           BillingService                          │
│   ┌────────────────┐                ┌────────────────────────────────┐      │
│   │ Checkout Page  │──┐             │      StripePaymentsController    │      │
│   │ (Card Form)    │  │             │  ┌──────────────────────────┐   │      │
│   └────────────────┘  │             │  │ Endpoints                │   │      │
│   ┌────────────────┐  │             │  │ • POST /payment-intent   │   │      │
│   │ Apple Pay      │──┼────────────▶│  │ • POST /confirm          │   │      │
│   │ Google Pay     │  │             │  │ • GET /payment/{id}      │   │      │
│   └────────────────┘  │             │  │ • POST /refund           │   │      │
│   ┌────────────────┐  │             │  └──────────────────────────┘   │      │
│   │ Dealer Sub     │──┘             │  ┌──────────────────────────┐   │      │
│   │ (Monthly)      │               │  │ StripeWebhooksController │   │      │
│   └────────────────┘               │  │ • payment_intent.success │   │      │
│                                    │  │ • invoice.paid           │   │      │
│   Stripe API                       │  │ • charge.dispute.created │   │      │
│   ┌────────────────┐               │  └──────────────────────────┘   │      │
│   │ Payment Intent │◀─────────────└────────────────────────────────┘      │
│   │ Subscriptions  │                           │                        │
│   │ Webhooks       │               ┌───────────┼───────────┐                │
│   └────────────────┘               ▼           ▼           ▼                │
│                            ┌────────────┐ ┌────────────┐ ┌────────────┐   │
│                            │ PostgreSQL │ │   Redis    │ │  RabbitMQ  │   │
│                            │ (Payments, │ │ (Idempot., │ │ (Payment   │   │
│                            │  Customers)│ │  Sessions) │ │  Events)   │   │
│                            └────────────┘ └────────────┘ └────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints API

### 2.1 StripePaymentsController

| Método   | Endpoint                                  | Descripción          | Auth | Roles |
| -------- | ----------------------------------------- | -------------------- | ---- | ----- |
| `POST`   | `/api/stripe/payment-intent`              | Crear Payment Intent | ✅   | User  |
| `POST`   | `/api/stripe/payment-intent/{id}/confirm` | Confirmar pago       | ✅   | User  |
| `GET`    | `/api/stripe/payment-intent/{id}`         | Estado del pago      | ✅   | User  |
| `POST`   | `/api/stripe/refund`                      | Procesar reembolso   | ✅   | Admin |
| `GET`    | `/api/stripe/payment-methods`             | Métodos guardados    | ✅   | User  |
| `POST`   | `/api/stripe/payment-methods`             | Guardar método       | ✅   | User  |
| `DELETE` | `/api/stripe/payment-methods/{id}`        | Eliminar método      | ✅   | User  |

### 2.2 StripeSubscriptionsController

| Método | Endpoint                                  | Descripción         | Auth | Roles  |
| ------ | ----------------------------------------- | ------------------- | ---- | ------ |
| `POST` | `/api/stripe/subscriptions`               | Crear suscripción   | ✅   | Dealer |
| `GET`  | `/api/stripe/subscriptions/{id}`          | Obtener suscripción | ✅   | Dealer |
| `PUT`  | `/api/stripe/subscriptions/{id}`          | Actualizar plan     | ✅   | Dealer |
| `POST` | `/api/stripe/subscriptions/{id}/cancel`   | Cancelar            | ✅   | Dealer |
| `POST` | `/api/stripe/subscriptions/{id}/pause`    | Pausar              | ✅   | Dealer |
| `POST` | `/api/stripe/subscriptions/{id}/resume`   | Reanudar            | ✅   | Dealer |
| `GET`  | `/api/stripe/subscriptions/{id}/invoices` | Historial facturas  | ✅   | Dealer |

### 2.3 StripeWebhooksController

| Método | Endpoint               | Descripción     | Auth | Roles  |
| ------ | ---------------------- | --------------- | ---- | ------ |
| `POST` | `/api/stripe/webhooks` | Recibir eventos | ❌\* | Stripe |

\*Validación via Stripe-Signature header

---

## 3. Entidades y Enums

### 3.1 StripePaymentStatus (Enum)

```csharp
public enum StripePaymentStatus
{
    Created = 0,              // Intent creado
    RequiresPaymentMethod = 1, // Esperando método
    RequiresConfirmation = 2, // Esperando confirmación
    RequiresAction = 3,       // 3D Secure requerido
    Processing = 4,           // Procesando
    Succeeded = 5,            // Exitoso
    Canceled = 6,             // Cancelado
    Failed = 7                // Fallido
}
```

### 3.2 SubscriptionStatus (Enum)

```csharp
public enum SubscriptionStatus
{
    Incomplete = 0,           // Pago inicial pendiente
    IncompleteExpired = 1,    // Expiró sin pago
    Trialing = 2,             // En período de prueba
    Active = 3,               // Activa
    PastDue = 4,              // Pago vencido
    Canceled = 5,             // Cancelada
    Unpaid = 6,               // Impaga
    Paused = 7                // Pausada
}
```

### 3.3 StripePayment (Entidad)

```csharp
public class StripePayment
{
    public Guid Id { get; set; }
    public string StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public Guid UserId { get; set; }
    public Guid? DealerId { get; set; }

    // Monto
    public decimal Amount { get; set; }
    public string Currency { get; set; }         // usd, dop
    public decimal? AmountRefunded { get; set; }

    // Estado
    public StripePaymentStatus Status { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }

    // Método de pago
    public string PaymentMethodType { get; set; } // card, apple_pay, google_pay
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }

    // Metadata
    public string? Description { get; set; }
    public string? Metadata { get; set; }         // JSON

    // 3D Secure
    public bool Requires3DSecure { get; set; }
    public string? ClientSecret { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}
```

### 3.4 StripeSubscription (Entidad)

```csharp
public class StripeSubscription
{
    public Guid Id { get; set; }
    public string StripeSubscriptionId { get; set; }
    public string StripeCustomerId { get; set; }
    public Guid DealerId { get; set; }

    // Plan
    public DealerPlan Plan { get; set; }
    public string StripePriceId { get; set; }
    public decimal MonthlyAmount { get; set; }

    // Estado
    public SubscriptionStatus Status { get; set; }

    // Período
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? TrialEnd { get; set; }

    // Early Bird
    public bool IsEarlyBird { get; set; }
    public decimal DiscountPercent { get; set; }

    // Cancelación
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

## 4. Procesos Detallados

### 4.1 STRIPE-PAY-001: Crear Payment Intent

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | STRIPE-PAY-001                  |
| **Nombre**  | Crear Payment Intent            |
| **Actor**   | Usuario/Sistema                 |
| **Trigger** | POST /api/stripe/payment-intent |

#### Flujo del Proceso

| Paso | Acción                  | Sistema        | Validación               |
| ---- | ----------------------- | -------------- | ------------------------ |
| 1    | Usuario inicia checkout | Frontend       | Monto calculado          |
| 2    | Seleccionar Stripe      | Frontend       | Si tarjeta internacional |
| 3    | Request Payment Intent  | BillingService | Con monto y metadata     |
| 4    | Obtener/crear Customer  | BillingService | StripeCustomerId         |
| 5    | Llamar Stripe API       | Stripe SDK     | PaymentIntents.Create    |
| 6    | Guardar en DB           | Database       | Status = Created         |
| 7    | Retornar client_secret  | Response       | Para frontend            |

#### Request

```json
{
  "amount": 129.0,
  "currency": "usd",
  "description": "Suscripción Plan Pro - OKLA",
  "metadata": {
    "dealerId": "uuid",
    "plan": "Pro",
    "type": "subscription"
  }
}
```

#### Response

```json
{
  "paymentIntentId": "pi_3abc123...",
  "clientSecret": "pi_3abc123_secret_xyz...",
  "amount": 129.0,
  "currency": "usd",
  "status": "requires_payment_method"
}
```

---

### 4.2 STRIPE-PAY-002: Confirmar Pago con 3D Secure

| Campo       | Valor                            |
| ----------- | -------------------------------- |
| **ID**      | STRIPE-PAY-002                   |
| **Nombre**  | Confirmar Pago con Autenticación |
| **Actor**   | Usuario                          |
| **Trigger** | Frontend Stripe.js               |

#### Flujo del Proceso (Frontend)

| Paso | Acción                    | Sistema   | Validación       |
| ---- | ------------------------- | --------- | ---------------- |
| 1    | Mostrar Stripe Elements   | Frontend  | Card element     |
| 2    | Usuario ingresa tarjeta   | Stripe.js | Validación       |
| 3    | stripe.confirmCardPayment | Stripe.js | Con clientSecret |
| 4    | Si requires_action        | Stripe    | Abrir 3DS modal  |
| 5    | Usuario completa 3DS      | Banco     | Autenticación    |
| 6    | Resultado al frontend     | Stripe.js | succeeded/failed |

#### Flujo del Proceso (Webhook)

| Paso | Acción              | Sistema             | Validación                |
| ---- | ------------------- | ------------------- | ------------------------- |
| 1    | Stripe envía evento | Webhook             | payment_intent.succeeded  |
| 2    | Validar firma       | BillingService      | Stripe-Signature          |
| 3    | Obtener pago en DB  | Database            | Por StripePaymentIntentId |
| 4    | Actualizar status   | Database            | Succeeded                 |
| 5    | Procesar según tipo | BillingService      | Subscription/OneTime      |
| 6    | Enviar confirmación | NotificationService | Email/Push                |
| 7    | Publicar evento     | RabbitMQ            | payment.completed         |

---

### 4.3 STRIPE-SUB-001: Crear Suscripción de Dealer

| Campo       | Valor                          |
| ----------- | ------------------------------ |
| **ID**      | STRIPE-SUB-001                 |
| **Nombre**  | Crear Suscripción              |
| **Actor**   | Dealer                         |
| **Trigger** | POST /api/stripe/subscriptions |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación              |
| ---- | --------------------------- | ------------------- | ----------------------- |
| 1    | Dealer completa onboarding  | Frontend            | Status = Approved       |
| 2    | Seleccionar plan            | Frontend            | Starter/Pro/Enterprise  |
| 3    | Ingresar método de pago     | Stripe Elements     | Tarjeta válida          |
| 4    | Crear/obtener Customer      | BillingService      | En Stripe               |
| 5    | Adjuntar PaymentMethod      | Stripe API          | Al customer             |
| 6    | Determinar si Early Bird    | BillingService      | Fecha < 31/01/2026      |
| 7    | Aplicar cupón si Early Bird | Stripe API          | 20% off forever         |
| 8    | Crear suscripción           | Stripe API          | Con trial si Early Bird |
| 9    | Guardar en DB               | Database            | Con detalles            |
| 10   | Activar dealer              | DealerService       | Status = Active         |
| 11   | Enviar bienvenida           | NotificationService | Email                   |
| 12   | Publicar evento             | RabbitMQ            | subscription.created    |

#### Request

```json
{
  "dealerId": "uuid",
  "plan": "Pro",
  "paymentMethodId": "pm_1abc..."
}
```

#### Stripe API Call

```csharp
var subscription = await _stripeClient.Subscriptions.CreateAsync(new SubscriptionCreateOptions
{
    Customer = stripeCustomerId,
    Items = new List<SubscriptionItemOptions>
    {
        new() { Price = GetPriceId(plan) }
    },
    DefaultPaymentMethod = paymentMethodId,
    TrialEnd = isEarlyBird ? DateTime.UtcNow.AddDays(90) : null,
    Coupon = isEarlyBird ? "EARLYBIRD_20" : null,
    Metadata = new Dictionary<string, string>
    {
        { "dealerId", dealerId.ToString() },
        { "plan", plan.ToString() }
    }
});
```

---

### 4.4 STRIPE-SUB-002: Upgrade/Downgrade de Plan

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | STRIPE-SUB-002                     |
| **Nombre**  | Cambiar Plan de Suscripción        |
| **Actor**   | Dealer                             |
| **Trigger** | PUT /api/stripe/subscriptions/{id} |

#### Flujo del Proceso

| Paso | Acción                    | Sistema             | Validación           |
| ---- | ------------------------- | ------------------- | -------------------- |
| 1    | Dealer accede a billing   | Dashboard           | Autenticado          |
| 2    | Seleccionar nuevo plan    | Frontend            | Diferente al actual  |
| 3    | Mostrar prorrateo         | Frontend            | Calcular diferencia  |
| 4    | Confirmar cambio          | Frontend            | Aceptar              |
| 5    | Actualizar suscripción    | Stripe API          | Con proration        |
| 6    | Actualizar en DB          | Database            | Nuevo plan           |
| 7    | Actualizar límites dealer | DealerService       | MaxVehicles          |
| 8    | Si upgrade inmediato      | Check               | Cobrar diferencia    |
| 9    | Enviar confirmación       | NotificationService | Email                |
| 10   | Publicar evento           | RabbitMQ            | subscription.updated |

#### Stripe API Call (Upgrade)

```csharp
var subscription = await _stripeClient.Subscriptions.UpdateAsync(subscriptionId, new SubscriptionUpdateOptions
{
    Items = new List<SubscriptionItemUpdateOptions>
    {
        new()
        {
            Id = currentItemId,
            Price = GetPriceId(newPlan)
        }
    },
    ProrationBehavior = "create_prorations",
    BillingCycleAnchor = "now"  // O "unchanged" para esperar
});
```

---

### 4.5 STRIPE-SUB-003: Cancelar Suscripción

| Campo       | Valor                                      |
| ----------- | ------------------------------------------ |
| **ID**      | STRIPE-SUB-003                             |
| **Nombre**  | Cancelar Suscripción                       |
| **Actor**   | Dealer                                     |
| **Trigger** | POST /api/stripe/subscriptions/{id}/cancel |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación                  |
| ---- | ---------------------------- | ------------------- | --------------------------- |
| 1    | Dealer accede a billing      | Dashboard           | Autenticado                 |
| 2    | Click "Cancelar suscripción" | Frontend            | Confirmación                |
| 3    | Seleccionar razón            | Frontend            | Dropdown                    |
| 4    | Tipo de cancelación          | Frontend            | Inmediata/FinPeriodo        |
| 5    | Confirmar cancelación        | Frontend            | Checkbox                    |
| 6    | Llamar Stripe API            | BillingService      | Cancel                      |
| 7    | Actualizar en DB             | Database            | Status + CancellationReason |
| 8    | Si inmediata                 | Check               | Desactivar dealer           |
| 9    | Si fin de período            | Check               | CancelAtPeriodEnd = true    |
| 10   | Enviar confirmación          | NotificationService | Email                       |
| 11   | Publicar evento              | RabbitMQ            | subscription.cancelled      |

---

### 4.6 STRIPE-WH-001: Procesar Webhook

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | STRIPE-WH-001             |
| **Nombre**  | Procesar Evento de Stripe |
| **Actor**   | Stripe                    |
| **Trigger** | POST /api/stripe/webhooks |

#### Eventos Procesados

| Evento                          | Acción                   |
| ------------------------------- | ------------------------ |
| `payment_intent.succeeded`      | Marcar pago exitoso      |
| `payment_intent.payment_failed` | Marcar pago fallido      |
| `customer.subscription.created` | Crear suscripción        |
| `customer.subscription.updated` | Actualizar suscripción   |
| `customer.subscription.deleted` | Cancelar suscripción     |
| `invoice.paid`                  | Registrar factura pagada |
| `invoice.payment_failed`        | Notificar fallo de pago  |
| `charge.refunded`               | Registrar reembolso      |
| `charge.dispute.created`        | Crear disputa            |

#### Flujo del Proceso

| Paso | Acción                   | Sistema        | Validación          |
| ---- | ------------------------ | -------------- | ------------------- |
| 1    | Recibir request          | BillingService | POST /webhooks      |
| 2    | Leer body raw            | BillingService | No parsear aún      |
| 3    | Obtener Stripe-Signature | Headers        | Obligatorio         |
| 4    | Validar firma            | Stripe SDK     | Con webhook secret  |
| 5    | Si inválida              | Response       | 400 Bad Request     |
| 6    | Parsear evento           | Stripe SDK     | Event object        |
| 7    | Buscar handler           | BillingService | Por event.Type      |
| 8    | Ejecutar handler         | BillingService | Procesar evento     |
| 9    | Responder 200            | Response       | Éxito               |
| 10   | Publicar evento interno  | RabbitMQ       | stripe.{event_type} |

#### Código de Validación

```csharp
[HttpPost("webhooks")]
public async Task<IActionResult> HandleWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
    var signature = Request.Headers["Stripe-Signature"];

    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            signature,
            _webhookSecret
        );

        await ProcessEvent(stripeEvent);
        return Ok();
    }
    catch (StripeException ex)
    {
        _logger.LogWarning("Webhook signature validation failed: {Message}", ex.Message);
        return BadRequest();
    }
}
```

---

## 5. Planes y Precios

### 5.1 Productos en Stripe

| Plan       | Price ID (prod)          | Monto   | Intervalo |
| ---------- | ------------------------ | ------- | --------- |
| Starter    | price_starter_monthly    | $49.00  | monthly   |
| Pro        | price_pro_monthly        | $129.00 | monthly   |
| Enterprise | price_enterprise_monthly | $299.00 | monthly   |

### 5.2 Cupón Early Bird

```json
{
  "id": "EARLYBIRD_20",
  "percent_off": 20,
  "duration": "forever",
  "max_redemptions": null,
  "redeem_by": 1738281599 // 2026-01-31T23:59:59Z
}
```

---

## 6. Manejo de Errores

### 6.1 Códigos de Error Stripe

| Código                    | Descripción            | Acción                  |
| ------------------------- | ---------------------- | ----------------------- |
| `card_declined`           | Tarjeta rechazada      | Pedir otro método       |
| `expired_card`            | Tarjeta expirada       | Actualizar tarjeta      |
| `insufficient_funds`      | Fondos insuficientes   | Intentar otro método    |
| `incorrect_cvc`           | CVC incorrecto         | Reingresar datos        |
| `processing_error`        | Error de procesamiento | Reintentar              |
| `authentication_required` | 3DS requerido          | Completar autenticación |

### 6.2 Reintentos Automáticos

```json
{
  "subscription_settings": {
    "default_payment_method": "pm_xxx",
    "payment_behavior": "error_if_incomplete",
    "collection_method": "charge_automatically"
  },
  "invoice_settings": {
    "auto_advance": true,
    "days_until_due": null
  }
}
```

Stripe reintenta automáticamente:

- Día 0: Intento inicial
- Día 3: Primer reintento
- Día 5: Segundo reintento
- Día 7: Tercer reintento (marca como unpaid)

---

## 7. Eventos RabbitMQ

| Evento                          | Exchange         | Payload                                |
| ------------------------------- | ---------------- | -------------------------------------- |
| `stripe.payment.succeeded`      | `billing.events` | `{ paymentId, amount }`                |
| `stripe.payment.failed`         | `billing.events` | `{ paymentId, error }`                 |
| `stripe.subscription.created`   | `billing.events` | `{ subscriptionId, dealerId, plan }`   |
| `stripe.subscription.updated`   | `billing.events` | `{ subscriptionId, oldPlan, newPlan }` |
| `stripe.subscription.cancelled` | `billing.events` | `{ subscriptionId, reason }`           |
| `stripe.refund.created`         | `billing.events` | `{ refundId, amount }`                 |
| `stripe.dispute.created`        | `billing.events` | `{ disputeId, chargeId }`              |

---

## 8. Configuración

### 8.1 appsettings.json

```json
{
  "Stripe": {
    "SecretKey": "${STRIPE_SECRET_KEY}",
    "PublishableKey": "${STRIPE_PUBLISHABLE_KEY}",
    "WebhookSecret": "${STRIPE_WEBHOOK_SECRET}",
    "PriceIds": {
      "Starter": "price_starter_monthly",
      "Pro": "price_pro_monthly",
      "Enterprise": "price_enterprise_monthly"
    },
    "EarlyBirdCoupon": "EARLYBIRD_20",
    "EarlyBirdTrialDays": 90
  }
}
```

### 8.2 Webhook Endpoint (Stripe Dashboard)

URL: `https://api.okla.com.do/api/stripe/webhooks`

Eventos habilitados:

- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `customer.subscription.*`
- `invoice.*`
- `charge.refunded`
- `charge.dispute.*`

---

## 9. Métricas

### 9.1 Prometheus

```
# Pagos
stripe_payments_total{status="succeeded|failed"}
stripe_payments_amount_total{currency="usd|dop"}
stripe_payments_3ds_required_total

# Suscripciones
stripe_subscriptions_total{plan="starter|pro|enterprise", status="..."}
stripe_subscriptions_mrr_total{currency="usd"}
stripe_subscription_upgrades_total{from="...", to="..."}
stripe_subscription_churns_total

# Webhooks
stripe_webhooks_received_total{event="..."}
stripe_webhooks_processed_total
stripe_webhooks_failed_total
stripe_webhook_latency_seconds
```

---

## 10. Seguridad

### 10.1 PCI Compliance

| Práctica             | Implementación                           |
| -------------------- | ---------------------------------------- |
| Tokenización         | Stripe.js (nunca tocar datos de tarjeta) |
| Webhook verification | Stripe-Signature validation              |
| HTTPS                | Obligatorio en producción                |
| API keys             | Variables de entorno                     |
| Idempotency          | Idempotency-Key en requests              |

### 10.2 Idempotencia

```csharp
var options = new PaymentIntentCreateOptions
{
    Amount = 12900,
    Currency = "usd"
};

var requestOptions = new RequestOptions
{
    IdempotencyKey = $"order_{orderId}"
};

var paymentIntent = await _stripeClient.PaymentIntents.CreateAsync(options, requestOptions);
```

---

## 📚 Referencias

- [03-azul-payment.md](03-azul-payment.md) - Pagos locales RD
- [04-invoicing-service.md](04-invoicing-service.md) - Facturación
- [06-subscriptions.md](06-subscriptions.md) - Gestión de suscripciones
- [Stripe API Reference](https://stripe.com/docs/api)
- [Stripe.js Reference](https://stripe.com/docs/js)
