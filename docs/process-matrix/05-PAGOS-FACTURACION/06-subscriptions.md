# 💳 Subscriptions Service - Suscripciones - Matriz de Procesos

> **Servicio:** SubscriptionService (parte de BillingService)  
> **Puerto:** 5010  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                   | Total | Implementado | Pendiente | Estado         |
| ---------------------------- | ----- | ------------ | --------- | -------------- |
| **Controllers**              | 1     | 0            | 1         | 🔴 Pendiente   |
| **SUB-CREATE-\*** (Crear)    | 4     | 0            | 4         | 🔴 Pendiente   |
| **SUB-UPGRADE-\*** (Upgrade) | 3     | 0            | 3         | 🔴 Pendiente   |
| **SUB-CANCEL-\*** (Cancelar) | 3     | 0            | 3         | 🔴 Pendiente   |
| **SUB-RENEW-\*** (Renovar)   | 3     | 0            | 3         | 🔴 Pendiente   |
| **SUB-TRIAL-\*** (Trial)     | 3     | 0            | 3         | 🔴 Pendiente   |
| **Tests**                    | 0     | 0            | 18        | 🔴 Pendiente   |
| **TOTAL**                    | 17    | 0            | 17        | 🔴 0% Completo |

---

## 1. Información General

### 1.1 Descripción

Sistema de gestión de suscripciones para dealers. Maneja planes mensuales/anuales, cobros recurrentes, upgrades, downgrades, cancelaciones y trial periods.

### 1.2 Planes Disponibles

| Plan           | Precio Mensual | Precio Anual            | Vehículos | Características        |
| -------------- | -------------- | ----------------------- | --------- | ---------------------- |
| **Starter**    | $49            | $490 (2 meses gratis)   | 15        | Básico                 |
| **Pro** ⭐     | $129           | $1,290 (2 meses gratis) | 50        | Destacados + Analytics |
| **Enterprise** | $299           | $2,990 (2 meses gratis) | Ilimitado | API + CRM + Premium    |

### 1.3 Early Bird (Hasta 31/01/2026)

| Beneficio | Valor                         |
| --------- | ----------------------------- |
| Descuento | 20% de por vida               |
| Trial     | 90 días gratis                |
| Badge     | "Miembro Fundador" permanente |

### 1.4 Dependencias

| Servicio            | Propósito         |
| ------------------- | ----------------- |
| BillingService      | Cobro de pagos    |
| DealerService       | Datos del dealer  |
| NotificationService | Recordatorios     |
| InvoicingService    | Facturación       |
| StripeService       | Pagos recurrentes |

---

## 2. Endpoints API

### 2.1 SubscriptionsController

| Método | Endpoint                                 | Descripción           | Auth | Roles  |
| ------ | ---------------------------------------- | --------------------- | ---- | ------ |
| `GET`  | `/api/subscriptions/plans`               | Listar planes         | ❌   | Public |
| `GET`  | `/api/subscriptions/current`             | Mi suscripción actual | ✅   | Dealer |
| `POST` | `/api/subscriptions`                     | Crear suscripción     | ✅   | Dealer |
| `PUT`  | `/api/subscriptions/{id}/upgrade`        | Upgrade de plan       | ✅   | Dealer |
| `PUT`  | `/api/subscriptions/{id}/downgrade`      | Downgrade de plan     | ✅   | Dealer |
| `POST` | `/api/subscriptions/{id}/cancel`         | Cancelar              | ✅   | Dealer |
| `POST` | `/api/subscriptions/{id}/reactivate`     | Reactivar             | ✅   | Dealer |
| `PUT`  | `/api/subscriptions/{id}/payment-method` | Cambiar método pago   | ✅   | Dealer |
| `GET`  | `/api/subscriptions/{id}/invoices`       | Historial facturas    | ✅   | Dealer |

### 2.2 AdminSubscriptionsController

| Método | Endpoint                                       | Descripción             | Auth | Roles |
| ------ | ---------------------------------------------- | ----------------------- | ---- | ----- |
| `GET`  | `/api/admin/subscriptions`                     | Todas las suscripciones | ✅   | Admin |
| `GET`  | `/api/admin/subscriptions/metrics`             | Métricas MRR/Churn      | ✅   | Admin |
| `POST` | `/api/admin/subscriptions/{id}/extend-trial`   | Extender trial          | ✅   | Admin |
| `POST` | `/api/admin/subscriptions/{id}/apply-discount` | Aplicar descuento       | ✅   | Admin |
| `POST` | `/api/admin/subscriptions/{id}/pause`          | Pausar cobros           | ✅   | Admin |

---

## 3. Entidades y Enums

### 3.1 SubscriptionStatus (Enum)

```csharp
public enum SubscriptionStatus
{
    Trialing = 0,           // En periodo de prueba
    Active = 1,             // Activa y al día
    PastDue = 2,            // Pago pendiente
    Canceled = 3,           // Cancelada
    Paused = 4,             // Pausada
    Expired = 5,            // Expirada
    Incomplete = 6          // Pago inicial fallido
}
```

### 3.2 SubscriptionPlan (Enum)

```csharp
public enum SubscriptionPlan
{
    Free = 0,               // Sin suscripción
    Starter = 1,            // $49/mes
    Pro = 2,                // $129/mes
    Enterprise = 3          // $299/mes
}
```

### 3.3 BillingInterval (Enum)

```csharp
public enum BillingInterval
{
    Monthly = 0,
    Annually = 1
}
```

### 3.4 Subscription (Entidad)

```csharp
public class Subscription
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }
    public string StripeSubscriptionId { get; set; }
    public string StripeCustomerId { get; set; }

    // Plan
    public SubscriptionPlan Plan { get; set; }
    public BillingInterval Interval { get; set; }
    public SubscriptionStatus Status { get; set; }

    // Precios
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public string Currency { get; set; }

    // Trial
    public bool IsTrialing { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }

    // Early Bird
    public bool IsEarlyBird { get; set; }
    public bool HasFounderBadge { get; set; }

    // Ciclo de facturación
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? NextBillingDate { get; set; }

    // Límites
    public int MaxActiveListings { get; set; }
    public int CurrentActiveListings { get; set; }

    // Cancelación
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Método de pago
    public string? DefaultPaymentMethodId { get; set; }
    public PaymentMethodType PaymentMethodType { get; set; }
    public string? Last4 { get; set; }
    public string? CardBrand { get; set; }

    // Historial
    public List<SubscriptionChange> ChangeHistory { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 3.5 SubscriptionChange (Entidad)

```csharp
public class SubscriptionChange
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public SubscriptionChangeType Type { get; set; }
    public SubscriptionPlan? OldPlan { get; set; }
    public SubscriptionPlan? NewPlan { get; set; }
    public decimal? ProratedAmount { get; set; }
    public string Reason { get; set; }
    public DateTime ChangedAt { get; set; }
}

public enum SubscriptionChangeType
{
    Created,
    Upgraded,
    Downgraded,
    Canceled,
    Reactivated,
    Paused,
    Resumed,
    TrialEnded,
    PaymentFailed,
    PaymentSucceeded
}
```

---

## 4. Procesos Detallados

### 4.1 SUB-001: Crear Suscripción

| Campo       | Valor                   |
| ----------- | ----------------------- |
| **ID**      | SUB-001                 |
| **Nombre**  | Crear Nueva Suscripción |
| **Actor**   | Dealer                  |
| **Trigger** | POST /api/subscriptions |

#### Flujo del Proceso

| Paso | Acción                   | Sistema             | Validación               |
| ---- | ------------------------ | ------------------- | ------------------------ |
| 1    | Dealer completa registro | Frontend            | DealerRegistration       |
| 2    | Seleccionar plan         | Frontend            | Starter/Pro/Enterprise   |
| 3    | Seleccionar intervalo    | Frontend            | Mensual/Anual            |
| 4    | Ingresar método de pago  | Frontend            | Stripe Elements          |
| 5    | Verificar Early Bird     | SubscriptionService | Fecha < 31/01/2026       |
| 6    | Calcular precio final    | SubscriptionService | Con descuentos           |
| 7    | Crear Customer en Stripe | StripeService       | stripe.customers.create  |
| 8    | Crear Subscription       | StripeService       | Con trial si Early Bird  |
| 9    | Guardar suscripción      | Database            | Status = Trialing/Active |
| 10   | Actualizar dealer        | DealerService       | MaxActiveListings        |
| 11   | Enviar confirmación      | NotificationService | Email                    |
| 12   | Generar factura          | InvoicingService    | Si pago inmediato        |
| 13   | Publicar evento          | RabbitMQ            | subscription.created     |

#### Request

```json
{
  "plan": "Pro",
  "interval": "Monthly",
  "paymentMethodId": "pm_xxxxxxxxxxxxx",
  "promoCode": "EARLYBIRD"
}
```

#### Response

```json
{
  "id": "uuid",
  "plan": "Pro",
  "interval": "Monthly",
  "status": "Trialing",
  "basePrice": 129.0,
  "discount": 20,
  "finalPrice": 103.2,
  "trialEndDate": "2026-04-21",
  "isEarlyBird": true,
  "hasFounderBadge": true,
  "maxActiveListings": 50,
  "features": [
    "50 vehículos activos",
    "Destacados en búsqueda",
    "Analytics básico",
    "Soporte prioritario"
  ]
}
```

---

### 4.2 SUB-002: Upgrade de Plan

| Campo       | Valor                               |
| ----------- | ----------------------------------- |
| **ID**      | SUB-002                             |
| **Nombre**  | Upgrade de Plan                     |
| **Actor**   | Dealer                              |
| **Trigger** | PUT /api/subscriptions/{id}/upgrade |

#### Flujo del Proceso

| Paso | Acción                      | Sistema             | Validación                            |
| ---- | --------------------------- | ------------------- | ------------------------------------- |
| 1    | Dealer quiere más vehículos | Dashboard           | Límite alcanzado                      |
| 2    | Ver planes superiores       | Frontend            | Comparativa                           |
| 3    | Seleccionar nuevo plan      | Frontend            | Pro → Enterprise                      |
| 4    | Calcular prorrateo          | SubscriptionService | Días restantes                        |
| 5    | Mostrar cargo adicional     | Frontend            | Con detalle                           |
| 6    | Confirmar upgrade           | Dealer              | Aceptar                               |
| 7    | Actualizar en Stripe        | StripeService       | proration_behavior: create_prorations |
| 8    | Procesar cargo prorrateado  | StripeService       | Inmediato                             |
| 9    | Actualizar suscripción      | Database            | Nuevo plan                            |
| 10   | Actualizar límites          | DealerService       | MaxActiveListings                     |
| 11   | Enviar confirmación         | NotificationService | Nuevo plan activo                     |
| 12   | Registrar cambio            | Database            | SubscriptionChange                    |
| 13   | Publicar evento             | RabbitMQ            | subscription.upgraded                 |

#### Cálculo de Prorrateo

```csharp
public ProrationResult CalculateProration(
    Subscription current,
    SubscriptionPlan newPlan)
{
    var daysInPeriod = (current.CurrentPeriodEnd - current.CurrentPeriodStart).Days;
    var daysRemaining = (current.CurrentPeriodEnd - DateTime.UtcNow).Days;

    var currentDailyRate = current.FinalPrice / daysInPeriod;
    var newDailyRate = GetPlanPrice(newPlan) / daysInPeriod;

    var creditAmount = currentDailyRate * daysRemaining;
    var chargeAmount = newDailyRate * daysRemaining;
    var prorationAmount = chargeAmount - creditAmount;

    return new ProrationResult
    {
        CreditAmount = creditAmount,
        ChargeAmount = chargeAmount,
        NetCharge = prorationAmount,
        EffectiveImmediately = true
    };
}
```

---

### 4.3 SUB-003: Cancelar Suscripción

| Campo       | Valor                               |
| ----------- | ----------------------------------- |
| **ID**      | SUB-003                             |
| **Nombre**  | Cancelar Suscripción                |
| **Actor**   | Dealer                              |
| **Trigger** | POST /api/subscriptions/{id}/cancel |

#### Flujo del Proceso

| Paso | Acción                     | Sistema             | Validación                 |
| ---- | -------------------------- | ------------------- | -------------------------- |
| 1    | Dealer click "Cancelar"    | Dashboard           | Botón                      |
| 2    | Mostrar encuesta de salida | Frontend            | Razón de cancelación       |
| 3    | Mostrar lo que perderá     | Frontend            | Features del plan          |
| 4    | Ofrecer descuento          | Frontend            | "30% off 3 meses"          |
| 5    | Si acepta oferta           | SubscriptionService | Aplicar descuento          |
| 6    | Si rechaza                 | Continuar           | Cancelar                   |
| 7    | Confirmar cancelación      | Dealer              | "Entiendo"                 |
| 8    | Cancelar en Stripe         | StripeService       | cancel_at_period_end: true |
| 9    | Actualizar suscripción     | Database            | CancelAtPeriodEnd = true   |
| 10   | Programar desactivación    | SchedulerService    | En fecha fin periodo       |
| 11   | Enviar confirmación        | NotificationService | Con fecha de término       |
| 12   | Registrar razón            | Database            | SubscriptionChange         |
| 13   | Publicar evento            | RabbitMQ            | subscription.canceled      |

#### Razones de Cancelación

```csharp
public enum CancellationReason
{
    TooExpensive,
    NotUsingEnough,
    SwitchingToCompetitor,
    ClosingBusiness,
    TechnicalIssues,
    CustomerService,
    MissingFeatures,
    Other
}
```

---

### 4.4 SUB-004: Manejar Pago Fallido

| Campo       | Valor                              |
| ----------- | ---------------------------------- |
| **ID**      | SUB-004                            |
| **Nombre**  | Proceso de Dunning (Cobro Fallido) |
| **Actor**   | Sistema                            |
| **Trigger** | Webhook invoice.payment_failed     |

#### Flujo de Dunning

| Día | Acción               | Comunicación                      |
| --- | -------------------- | --------------------------------- |
| 0   | Pago fallido         | Email + Push: "Pago fallido"      |
| 1   | Primer reintento     | SMS: "Actualiza método de pago"   |
| 3   | Segundo reintento    | Email: "Tu suscripción en riesgo" |
| 5   | Tercer reintento     | Email + SMS: "Último aviso"       |
| 7   | Suspender cuenta     | Email: "Cuenta suspendida"        |
| 14  | Cancelar suscripción | Email: "Suscripción cancelada"    |

#### Flujo del Proceso

| Paso | Acción                  | Sistema             | Validación             |
| ---- | ----------------------- | ------------------- | ---------------------- |
| 1    | Recibir webhook         | StripeService       | invoice.payment_failed |
| 2    | Identificar suscripción | SubscriptionService | Por customer_id        |
| 3    | Actualizar status       | Database            | Status = PastDue       |
| 4    | Programar reintentos    | SchedulerService    | Días 1, 3, 5           |
| 5    | Notificar dealer        | NotificationService | Email + SMS            |
| 6    | Mostrar banner          | Dashboard           | "Actualizar pago"      |
| 7    | Si pago exitoso         | StripeService       | Webhook success        |
| 8    | Restaurar cuenta        | SubscriptionService | Status = Active        |
| 9    | Si todos fallan         | SubscriptionService | Suspender              |
| 10   | Ocultar vehículos       | VehiclesSaleService | Status = Hidden        |
| 11   | Después de 14 días      | SubscriptionService | Cancelar               |
| 12   | Publicar evento         | RabbitMQ            | subscription.churned   |

---

### 4.5 SUB-005: Renovación Automática

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **ID**      | SUB-005                   |
| **Nombre**  | Renovación de Suscripción |
| **Actor**   | Sistema                   |
| **Trigger** | Stripe Billing Cycle      |

#### Flujo del Proceso

| Paso | Acción                 | Sistema             | Validación                  |
| ---- | ---------------------- | ------------------- | --------------------------- |
| 1    | 3 días antes           | NotificationService | Email: "Renovación próxima" |
| 2    | Día de renovación      | Stripe              | Crear invoice               |
| 3    | Cobrar automáticamente | Stripe              | Método default              |
| 4    | Recibir webhook        | StripeService       | invoice.paid                |
| 5    | Actualizar periodo     | SubscriptionService | Nuevas fechas               |
| 6    | Generar factura        | InvoicingService    | Con NCF                     |
| 7    | Enviar recibo          | NotificationService | Email                       |
| 8    | Publicar evento        | RabbitMQ            | subscription.renewed        |

---

## 5. Características por Plan

### 5.1 Plan Starter ($49/mes)

| Característica     | Incluido    |
| ------------------ | ----------- |
| Vehículos activos  | 15          |
| Fotos por vehículo | 10          |
| Destacados         | ❌          |
| Analytics básico   | ✅          |
| Analytics avanzado | ❌          |
| Soporte            | Email (48h) |
| Import CSV         | ❌          |
| API access         | ❌          |
| CRM integración    | ❌          |

### 5.2 Plan Pro ($129/mes)

| Característica     | Incluido    |
| ------------------ | ----------- |
| Vehículos activos  | 50          |
| Fotos por vehículo | 20          |
| Destacados         | 5/mes       |
| Analytics básico   | ✅          |
| Analytics avanzado | ✅          |
| Soporte            | Email (24h) |
| Import CSV         | ✅          |
| API access         | ❌          |
| CRM integración    | ❌          |

### 5.3 Plan Enterprise ($299/mes)

| Característica     | Incluido            |
| ------------------ | ------------------- |
| Vehículos activos  | Ilimitado           |
| Fotos por vehículo | 30                  |
| Destacados         | 20/mes              |
| Analytics básico   | ✅                  |
| Analytics avanzado | ✅                  |
| Soporte            | Prioritario + Phone |
| Import CSV         | ✅                  |
| API access         | ✅                  |
| CRM integración    | ✅                  |
| Account Manager    | ✅                  |

---

## 6. Reglas de Negocio

### 6.1 Cambios de Plan

| Regla                  | Valor                   |
| ---------------------- | ----------------------- |
| Upgrade                | Inmediato con prorrateo |
| Downgrade              | Al final del periodo    |
| Máx downgrades/año     | 2                       |
| Cooldown entre cambios | 30 días                 |

### 6.2 Trials

| Regla                   | Valor          |
| ----------------------- | -------------- |
| Trial normal            | 7 días         |
| Trial Early Bird        | 90 días        |
| Trial extendido (admin) | Hasta 180 días |

### 6.3 Cancelación

| Regla                      | Valor                 |
| -------------------------- | --------------------- |
| Efecto                     | Al final del periodo  |
| Reembolso                  | No (excepto < 48h)    |
| Gracia después de cancelar | 7 días para reactivar |
| Datos después de cancelar  | 30 días               |

---

## 7. Eventos RabbitMQ

| Evento                      | Exchange         | Payload                                |
| --------------------------- | ---------------- | -------------------------------------- |
| `subscription.created`      | `billing.events` | `{ subscriptionId, plan, dealerId }`   |
| `subscription.upgraded`     | `billing.events` | `{ subscriptionId, oldPlan, newPlan }` |
| `subscription.downgraded`   | `billing.events` | `{ subscriptionId, oldPlan, newPlan }` |
| `subscription.renewed`      | `billing.events` | `{ subscriptionId }`                   |
| `subscription.canceled`     | `billing.events` | `{ subscriptionId, reason }`           |
| `subscription.reactivated`  | `billing.events` | `{ subscriptionId }`                   |
| `subscription.past_due`     | `billing.events` | `{ subscriptionId }`                   |
| `subscription.trial_ending` | `billing.events` | `{ subscriptionId, daysLeft }`         |
| `subscription.churned`      | `billing.events` | `{ subscriptionId, dealerId }`         |

---

## 8. Métricas

```
# Suscripciones
subscriptions_active_total{plan="starter|pro|enterprise"}
subscriptions_trialing_total
subscriptions_mrr_total{currency="DOP|USD"}
subscriptions_arr_total

# Cambios
subscriptions_created_total{plan="..."}
subscriptions_upgraded_total
subscriptions_downgraded_total
subscriptions_canceled_total{reason="..."}

# Churn
subscriptions_churn_rate_monthly
subscriptions_churn_rate_voluntary
subscriptions_churn_rate_involuntary

# Revenue
subscription_arpu_monthly
subscription_ltv_average
```

---

## 9. Configuración

```json
{
  "Subscriptions": {
    "Plans": {
      "Starter": { "Price": 49, "MaxVehicles": 15 },
      "Pro": { "Price": 129, "MaxVehicles": 50 },
      "Enterprise": { "Price": 299, "MaxVehicles": -1 }
    },
    "AnnualDiscountPercent": 16.67,
    "EarlyBird": {
      "Enabled": true,
      "Deadline": "2026-01-31T23:59:59Z",
      "DiscountPercent": 20,
      "TrialDays": 90
    },
    "Dunning": {
      "RetryDays": [1, 3, 5],
      "SuspendAfterDays": 7,
      "CancelAfterDays": 14
    }
  }
}
```

---

## 📚 Referencias

- [01-billing-service.md](01-billing-service.md) - Pagos
- [02-stripe-payment.md](02-stripe-payment.md) - Integración Stripe
- [04-invoicing-service.md](04-invoicing-service.md) - Facturación
- [04-dealer-onboarding.md](../02-USUARIOS-DEALERS/04-dealer-onboarding.md) - Onboarding
