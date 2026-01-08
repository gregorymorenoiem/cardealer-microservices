# BillingService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** BillingService
- **Puerto en Kubernetes:** 8080
- **Puerto en Desarrollo:** 5007
- **Estado:** ✅ **EN PRODUCCIÓN**
- **Base de Datos:** PostgreSQL (`billingservice`)
- **Imagen Docker:** ghcr.io/gregorymorenoiem/cardealer-billingservice:latest

### Propósito
Servicio de facturación y procesamiento de pagos. Integración con Stripe para pagos con tarjeta, gestión de suscripciones, planes de publicación de vehículos, facturación y reportes de ingresos.

---

## 🏗️ ARQUITECTURA

```
BillingService/
├── BillingService.Api/
│   ├── Controllers/
│   │   ├── PaymentsController.cs
│   │   ├── SubscriptionsController.cs
│   │   ├── InvoicesController.cs
│   │   └── WebhooksController.cs          # Stripe webhooks
│   └── Program.cs
├── BillingService.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   │   ├── CreatePaymentIntentCommand.cs
│   │   │   ├── ConfirmPaymentCommand.cs
│   │   │   ├── RefundPaymentCommand.cs
│   │   │   └── CreateSubscriptionCommand.cs
│   │   └── Queries/
│   │       ├── GetInvoiceQuery.cs
│   │       └── GetPaymentHistoryQuery.cs
│   └── DTOs/
├── BillingService.Domain/
│   ├── Entities/
│   │   ├── Payment.cs
│   │   ├── Invoice.cs
│   │   ├── Subscription.cs
│   │   ├── Plan.cs
│   │   └── PaymentMethod.cs
│   ├── Enums/
│   │   ├── PaymentStatus.cs
│   │   ├── PaymentType.cs
│   │   └── SubscriptionStatus.cs
│   └── Interfaces/
│       ├── IPaymentRepository.cs
│       └── IPaymentGateway.cs
└── BillingService.Infrastructure/
    ├── Services/
    │   ├── StripePaymentGateway.cs
    │   ├── InvoiceGenerator.cs
    │   └── SubscriptionManager.cs
    └── BackgroundServices/
        └── SubscriptionRenewalWorker.cs
```

---

## 📦 ENTIDADES

### Payment
```csharp
public class Payment
{
    public Guid Id { get; set; }
    
    // Cliente
    public Guid UserId { get; set; }
    public string UserEmail { get; set; }
    
    // Monto
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? Fee { get; set; }               // Comisión Stripe
    public decimal NetAmount { get; set; }          // Amount - Fee
    
    // Tipo de pago
    public PaymentType Type { get; set; }           // VehicleListing, Subscription, Featured
    public string? Description { get; set; }
    public Guid? EntityId { get; set; }             // ID del vehículo, suscripción, etc.
    
    // Estado
    public PaymentStatus Status { get; set; }       // Pending, Succeeded, Failed, Refunded
    
    // Stripe
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public string? StripeCustomerId { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? FailureReason { get; set; }
    
    // Factura
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
}
```

### Invoice
```csharp
public class Invoice
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }       // INV-2026-001234
    
    // Cliente
    public Guid UserId { get; set; }
    public string BillingName { get; set; }
    public string BillingEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? TaxId { get; set; }              // RNC en República Dominicana
    
    // Monto
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Estado
    public InvoiceStatus Status { get; set; }       // Draft, Sent, Paid, Void
    
    // Fechas
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Items
    public ICollection<InvoiceItem> Items { get; set; }
    
    // PDF
    public string? PdfUrl { get; set; }
}
```

### Subscription
```csharp
public class Subscription
{
    public Guid Id { get; set; }
    
    // Usuario
    public Guid UserId { get; set; }
    
    // Plan
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; }
    
    // Estado
    public SubscriptionStatus Status { get; set; }  // Active, Canceled, Expired, PastDue
    
    // Fechas
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
    
    // Stripe
    public string? StripeSubscriptionId { get; set; }
    
    // Facturación
    public bool AutoRenew { get; set; } = true;
}
```

### Plan
```csharp
public class Plan
{
    public Guid Id { get; set; }
    public string Name { get; set; }                // "Básico", "Premium", "Dealer"
    public string Description { get; set; }
    
    // Precio
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingInterval Interval { get; set; }   // Monthly, Yearly
    
    // Límites
    public int MaxVehicleListings { get; set; }
    public int MaxPhotosPerVehicle { get; set; }
    public bool CanUseFeaturedListings { get; set; }
    public bool HasAnalytics { get; set; }
    
    // Estado
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    
    // Stripe
    public string? StripePriceId { get; set; }
}
```

---

## 📡 ENDPOINTS API

### Pagos

#### POST `/api/payments/create-intent`
Crear Payment Intent de Stripe.

**Request:**
```json
{
  "amount": 50.00,
  "currency": "USD",
  "type": "VehicleListing",
  "description": "Publicación de vehículo premium",
  "entityId": "..."
}
```

**Response (201 Created):**
```json
{
  "paymentId": "...",
  "clientSecret": "pi_xxx_secret_xxx",
  "amount": 50.00,
  "currency": "USD",
  "status": "Pending"
}
```

#### POST `/api/payments/{id}/confirm`
Confirmar pago (después de procesar con Stripe en frontend).

**Response (200 OK):**
```json
{
  "paymentId": "...",
  "status": "Succeeded",
  "paidAt": "2026-01-07T10:30:00Z",
  "invoiceId": "..."
}
```

#### POST `/api/payments/{id}/refund`
Procesar reembolso (admin only).

**Request:**
```json
{
  "amount": 50.00,
  "reason": "Customer request"
}
```

#### GET `/api/payments/user/{userId}`
Historial de pagos del usuario.

**Response (200 OK):**
```json
{
  "payments": [
    {
      "id": "...",
      "amount": 50.00,
      "currency": "USD",
      "type": "VehicleListing",
      "status": "Succeeded",
      "paidAt": "2026-01-07T10:30:00Z"
    }
  ],
  "totalSpent": 250.00,
  "totalTransactions": 5
}
```

### Suscripciones

#### POST `/api/subscriptions`
Crear suscripción.

**Request:**
```json
{
  "planId": "...",
  "paymentMethodId": "pm_card_visa"
}
```

**Response (201 Created):**
```json
{
  "subscriptionId": "...",
  "status": "Active",
  "currentPeriodEnd": "2026-02-07T00:00:00Z",
  "plan": {
    "name": "Premium",
    "price": 29.99,
    "interval": "Monthly"
  }
}
```

#### GET `/api/subscriptions/user/{userId}`
Obtener suscripción activa del usuario.

#### POST `/api/subscriptions/{id}/cancel`
Cancelar suscripción.

### Facturas

#### GET `/api/invoices/{id}`
Obtener factura.

**Response (200 OK):**
```json
{
  "id": "...",
  "invoiceNumber": "INV-2026-001234",
  "billingName": "Juan Pérez",
  "subtotal": 47.62,
  "tax": 2.38,
  "total": 50.00,
  "status": "Paid",
  "issueDate": "2026-01-07T00:00:00Z",
  "paidAt": "2026-01-07T10:30:00Z",
  "items": [
    {
      "description": "Publicación de vehículo premium",
      "quantity": 1,
      "unitPrice": 50.00,
      "amount": 50.00
    }
  ],
  "pdfUrl": "https://okla-invoices.s3.amazonaws.com/..."
}
```

#### GET `/api/invoices/user/{userId}`
Historial de facturas del usuario.

### Webhooks de Stripe

#### POST `/api/webhooks/stripe`
Recibir eventos de Stripe.

**Eventos manejados:**
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `charge.refunded`
- `customer.subscription.created`
- `customer.subscription.deleted`
- `invoice.payment_succeeded`

---

## 💰 PLANES DE PUBLICACIÓN

### Planes Disponibles

| Plan | Precio | Vehículos | Fotos | Featured | Duración |
|------|--------|-----------|-------|----------|----------|
| **Gratis** | $0 | 3 | 5/vehículo | ❌ | 30 días |
| **Básico** | $9.99/mes | 10 | 10/vehículo | ❌ | Ilimitado |
| **Premium** | $29.99/mes | 50 | 20/vehículo | ✅ 5 featured | Ilimitado |
| **Dealer** | $99.99/mes | Ilimitado | 30/vehículo | ✅ 20 featured | Ilimitado |

### Pagos por Publicación (Pay-per-listing)

| Tipo | Precio | Descripción |
|------|--------|-------------|
| **Estándar** | $5 | Publicación por 30 días |
| **Premium** | $15 | Publicación por 60 días + Featured |
| **Destacado Extra** | $10 | Agregar vehículo a sección destacada (7 días) |

---

## 🔧 TECNOLOGÍAS

```xml
<PackageReference Include="Stripe.net" Version="43.10.0" />
<PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.4.0" />
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
```

### Servicios Externos
- **Stripe**: Procesamiento de pagos
- **PostgreSQL**: Datos de facturación
- **AWS S3**: Almacenamiento de PDFs de facturas

---

## ⚙️ CONFIGURACIÓN

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=billingservice;..."
  },
  "Stripe": {
    "SecretKey": "${STRIPE_SECRET_KEY}",
    "PublishableKey": "${STRIPE_PUBLISHABLE_KEY}",
    "WebhookSecret": "${STRIPE_WEBHOOK_SECRET}"
  },
  "Billing": {
    "TaxRate": 0.05,
    "Currency": "USD",
    "InvoicePrefix": "INV"
  }
}
```

---

## 🔄 EVENTOS PUBLICADOS

### PaymentSucceededEvent
```csharp
public record PaymentSucceededEvent(
    Guid PaymentId,
    Guid UserId,
    decimal Amount,
    PaymentType Type,
    Guid? EntityId,
    DateTime PaidAt
);
```

**Exchange:** `billing.events`  
**Routing Key:** `payment.succeeded`  
**Consumidores:**
- **VehiclesSaleService**: Activar publicación de vehículo
- **UserService**: Actualizar suscripción
- **NotificationService**: Enviar recibo por email

### SubscriptionCreatedEvent
Cuando se crea una suscripción nueva.

### PaymentFailedEvent
Cuando un pago falla.

---

## 📝 REGLAS DE NEGOCIO

### Procesamiento de Pagos
1. **Confirmación en 2 pasos**: Create Intent → Confirm
2. **Webhook validation**: Verificar firma de Stripe
3. **Idempotencia**: Evitar procesar mismo pago dos veces

### Facturación
1. **Numeración secuencial**: INV-{YEAR}-{SEQUENCE}
2. **Tax incluido**: 5% ITBIS (impuesto RD)
3. **PDF generado automáticamente** después de pago exitoso

### Suscripciones
1. **Auto-renovación por defecto**
2. **Gracia de 3 días** después de fallo de pago
3. **Cancelación inmediata**: No reembolso pro-rata

---

## 🔗 RELACIONES

### Publica Eventos A:
- **VehiclesSaleService**: Activación de publicaciones
- **UserService**: Actualización de planes
- **NotificationService**: Recibos y alertas

### Consultado Por:
- **VehiclesSaleService**: Verificar plan activo
- **Frontend**: Procesar pagos con Stripe Elements

---

## 📅 ÚLTIMA ACTUALIZACIÓN

**Fecha:** Enero 7, 2026  
**Versión:** 1.0.0  
**Estado:** Producción en DOKS
