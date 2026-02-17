# 🔄 Fygaro Payment - Pagos Recurrentes con Fygaro - Matriz de Procesos

> **Proveedor:** Fygaro (Agregador de Pagos)  
> **Tipo:** Aggregator  
> **Puerto:** 15105 (PaymentService)  
> **Última actualización:** Enero 28, 2026  
> **Estado:** ✅ IMPLEMENTADO - **⭐ RECOMENDADO PARA SUSCRIPCIONES**

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 28, 2026)

| Proceso       | Backend | UI Access | Observación                        |
| ------------- | ------- | --------- | ---------------------------------- |
| FYGARO-SUB-\* | ✅ 100% | ✅ 100%   | `/checkout` (selector de pasarela) |

### Rutas UI Existentes ✅

- ✅ `/checkout` - CheckoutPage (**Fygaro optimizado para recurrentes**)

---

## 1. Descripción General

**Fygaro** es un agregador de pagos dominicano con módulo especializado en **suscripciones recurrentes**. Ideal para dealers que quieren automatizar cobros mensuales sin preocuparse por reintentos, dunning o gestión de tarjetas expiradas.

### Características Principales

- **Tipo:** Aggregator (agrega múltiples pasarelas)
- **Comisión:** Varía según volumen
- **Costo fijo:** Varía
- **Mensualidad:** US$15+/mes (según plan)
- **Tokenización:** Módulo de suscripciones nativo
- **Monedas:** DOP, USD
- **Cobertura:** 🇩🇴 República Dominicana
- **Depósito:** 48-72 horas
- **Reintentos automáticos:** ✅ Incluido
- **Dunning management:** ✅ Incluido

### ⭐ Ventajas para Suscripciones

1. **Reintentos automáticos** - 3 intentos en 5 días
2. **Dunning inteligente** - Emails automáticos antes de cancelar
3. **Tarjetas actualizables** - Dealer puede actualizar método sin nueva auth
4. **Dashboard de churn** - Métricas de cancelaciones
5. **Webhooks robustos** - Notificaciones de todos los eventos
6. **Gestión de trials** - Soporte nativo para periodos de prueba

### Casos de Uso

1. **Suscripciones mensuales** - Dealers pagan cada mes automáticamente
2. **Reducción de churn** - Reintentos + dunning disminuyen cancelaciones
3. **Gestión compleja** - Upgrades, downgrades, pausas
4. **Compliance** - Cumple con regulaciones de pagos recurrentes

---

## 1.1 Arquitectura Fygaro

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ARQUITECTURA FYGARO PAYMENT                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Frontend (React)                                                           │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  SubscriptionCheckoutPage                                  │            │
│  │  ├─ PlanSelector (Starter/Pro/Enterprise)                 │            │
│  │  ├─ BillingCycleSelector (Monthly/Annual)                 │            │
│  │  ├─ PaymentMethodSelector                                 │            │
│  │  │   └─ ⭐ Fygaro (Recomendado para recurrentes)           │            │
│  │  ├─ FygaroPaymentForm                                     │            │
│  │  │   ├─ CardNumber                                        │            │
│  │  │   ├─ CVV                                               │            │
│  │  │   ├─ ExpiryDate                                        │            │
│  │  │   ├─ CardholderName                                    │            │
│  │  │   └─ AcceptRecurringTerms ✅                            │            │
│  │  └─ SubmitButton → POST /api/subscriptions/create         │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  API Gateway (:18443)                                                       │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  /api/subscriptions/* → BillingService:15106               │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  BillingService (:15106)                                                    │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  SubscriptionsController                                   │            │
│  │  ├─ POST /api/subscriptions/create                         │            │
│  │  │   └─ MediatR → CreateSubscriptionCommand                │            │
│  │  └─ CreateSubscriptionHandler                              │            │
│  │      ├─ PaymentGatewayFactory.GetProvider(Fygaro)          │            │
│  │      ├─ FygaroPaymentProvider.CreateSubscriptionAsync()    │            │
│  │      └─ Save Subscription (gateway=Fygaro)                 │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  FygaroPaymentProvider                                                      │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  ├─ ValidateConfiguration() (ApiKey, MerchantId)           │            │
│  │  ├─ CreateFygaroCustomer()                                 │            │
│  │  │   • Email, Name, Phone                                  │            │
│  │  ├─ CreatePaymentMethod()                                  │            │
│  │  │   • Tokenizar tarjeta con Fygaro Vault                  │            │
│  │  ├─ CreateFygaroSubscription()                             │            │
│  │  │   • customer_id                                         │            │
│  │  │   • plan_id (Starter/Pro/Enterprise)                    │            │
│  │  │   • payment_method_id                                   │            │
│  │  │   • billing_cycle (monthly)                             │            │
│  │  │   • trial_days (90 si Early Bird)                       │            │
│  │  │   • webhook_url                                         │            │
│  │  ├─ HTTP POST → Fygaro API                                 │            │
│  │  ├─ ParseFygaroResponse()                                  │            │
│  │  └─ MapToSubscriptionResult()                              │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  🔄 Fygaro API (External)                                                   │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  Módulo de Suscripciones Fygaro                            │            │
│  │  ├─ Crear customer en vault                                │            │
│  │  ├─ Tokenizar tarjeta segura                               │            │
│  │  ├─ Crear subscription                                     │            │
│  │  ├─ Programar cobros mensuales                             │            │
│  │  ├─ Configurar reintentos (3x en 5 días)                   │            │
│  │  ├─ Configurar dunning (emails automáticos)                │            │
│  │  └─ Response:                                               │            │
│  │     {                                                       │            │
│  │       "subscription_id": "sub_fygaro_abc123",               │            │
│  │       "customer_id": "cus_xyz789",                          │            │
│  │       "status": "trialing", // o "active"                   │            │
│  │       "next_charge_date": "2026-04-28"  // +3 meses trial  │            │
│  │     }                                                       │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  POST-PROCESAMIENTO                                                         │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  1. Guardar Subscription en DB                             │            │
│  │     • FygaroSubscriptionId = "sub_fygaro_abc123"           │            │
│  │     • FygaroCustomerId = "cus_xyz789"                      │            │
│  │     • Status = Trialing (si trial) o Active                │            │
│  │     • NextChargeDate = "2026-04-28"                        │            │
│  │  2. Actualizar DealerSubscription                          │            │
│  │  3. Publicar evento: subscription.created (RabbitMQ)       │            │
│  │  4. NotificationService → Email bienvenida                 │            │
│  │  5. Programar webhooks listener                            │            │
│  │  6. Dashboard → Mostrar próxima fecha de cobro             │            │
│  └────────────────────────────────────────────────────────────┘            │
│                                                                             │
│  COBROS RECURRENTES AUTOMÁTICOS (Fygaro se encarga)                        │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  Cada mes, Fygaro automáticamente:                         │            │
│  │  1. Intenta cobrar con payment_method guardado             │            │
│  │  2. Si éxito → Webhook: subscription.charged               │            │
│  │  3. Si falla:                                               │            │
│  │     • Reintento 1 (día +2)                                 │            │
│  │     • Reintento 2 (día +4)                                 │            │
│  │     • Reintento 3 (día +5)                                 │            │
│  │     • Email dunning al dealer                              │            │
│  │     • Webhook: subscription.past_due                       │            │
│  │     • Si todos fallan: subscription.canceled               │            │
│  │  4. Backend recibe webhooks y actualiza estado             │            │
│  └────────────────────────────────────────────────────────────┘            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1.2 Flujo de Suscripción Fygaro (14 pasos + recurrencia)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                FLUJO DE SUSCRIPCIÓN FYGARO (Recurrente)                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣  DEALER: Registrarse y seleccionar plan                                 │
│      ├─ Elegir plan: Starter ($49), Pro ($129), Enterprise ($299)          │
│      ├─ Elegir ciclo: Mensual o Anual                                      │
│      ├─ Ver si aplica Early Bird (3 meses gratis)                          │
│      └─ Click "Suscribirse"                                                 │
│                                                                             │
│  2️⃣  CHECKOUT PAGE: Seleccionar método de pago                              │
│      ├─ AZUL (pago único, renovar manual)                                  │
│      ├─ PixelPay (pago único)                                              │
│      └─ ⭐ Fygaro (RECURRENTE - cobro automático cada mes)                  │
│                                                                             │
│  3️⃣  FRONTEND: Construir request de suscripción                             │
│      POST /api/subscriptions/create                                         │
│      {                                                                      │
│        "dealerId": "guid",                                                  │
│        "plan": "Pro",                                                       │
│        "billingCycle": "Monthly",                                           │
│        "gateway": "Fygaro",                                                 │
│        "cardNumber": "4111111111111111",                                    │
│        "cvv": "123",                                                        │
│        "expiryMonth": "12",                                                 │
│        "expiryYear": "2027",                                                │
│        "acceptRecurringTerms": true                                         │
│      }                                                                      │
│                                                                             │
│  4️⃣  BACKEND: SubscriptionsController recibe                                │
│      ├─ Validar JWT token                                                  │
│      ├─ Validar que dealer no tiene suscripción activa                     │
│      ├─ FluentValidation checks                                            │
│      ├─ MediatR → CreateSubscriptionCommand                                │
│      └─ CreateSubscriptionHandler ejecuta                                  │
│                                                                             │
│  5️⃣  HANDLER: Obtener proveedor Fygaro                                      │
│      var provider = _factory.GetProvider(PaymentGateway.Fygaro);           │
│      if (!provider.IsAvailable()) throw ProviderUnavailableException;      │
│                                                                             │
│  6️⃣  FYGARO PROVIDER: Crear customer en Fygaro Vault                        │
│      POST https://api.fygaro.com/v1/customers                               │
│      {                                                                      │
│        "email": "dealer@example.com",                                       │
│        "name": "Auto Dealer XYZ",                                           │
│        "phone": "+1809-555-1234",                                           │
│        "metadata": { "dealerId": "guid" }                                   │
│      }                                                                      │
│      Response: { "customer_id": "cus_fygaro_xyz789" }                       │
│                                                                             │
│  7️⃣  FYGARO PROVIDER: Tokenizar tarjeta                                     │
│      POST https://api.fygaro.com/v1/payment_methods                         │
│      {                                                                      │
│        "customer_id": "cus_fygaro_xyz789",                                  │
│        "card_number": "4111111111111111",                                   │
│        "cvv": "123",                                                        │
│        "exp_month": "12",                                                   │
│        "exp_year": "2027"                                                   │
│      }                                                                      │
│      Response: { "payment_method_id": "pm_card_abc123" }                    │
│                                                                             │
│  8️⃣  FYGARO PROVIDER: Crear suscripción                                     │
│      POST https://api.fygaro.com/v1/subscriptions                           │
│      {                                                                      │
│        "customer_id": "cus_fygaro_xyz789",                                  │
│        "plan_id": "plan_pro_monthly",                                       │
│        "payment_method_id": "pm_card_abc123",                               │
│        "trial_days": 90,  // Si Early Bird                                  │
│        "webhook_url": "https://api.okla.com.do/webhooks/fygaro",            │
│        "metadata": {                                                        │
│          "dealerId": "guid",                                                │
│          "platform": "OKLA"                                                 │
│        }                                                                    │
│      }                                                                      │
│                                                                             │
│  9️⃣  FYGARO API: Response                                                   │
│      {                                                                      │
│        "subscription_id": "sub_fygaro_abc123",                              │
│        "customer_id": "cus_fygaro_xyz789",                                  │
│        "status": "trialing",  // o "active"                                 │
│        "current_period_start": "2026-01-28",                                │
│        "current_period_end": "2026-02-28",                                  │
│        "trial_end": "2026-04-28",  // +90 días                              │
│        "next_charge_date": "2026-04-28",                                    │
│        "amount": 129.00,                                                    │
│        "currency": "USD"                                                    │
│      }                                                                      │
│                                                                             │
│  🔟  FYGARO PROVIDER: Parsear y retornar                                    │
│      return SubscriptionResult.Success(                                     │
│        subscriptionId: response.SubscriptionId,                             │
│        customerId: response.CustomerId,                                     │
│        status: response.Status,                                             │
│        nextChargeDate: response.NextChargeDate                              │
│      );                                                                     │
│                                                                             │
│  1️⃣1️⃣  HANDLER: Guardar en base de datos                                    │
│      var subscription = new Subscription {                                  │
│        Id = Guid.NewGuid(),                                                 │
│        DealerId = request.DealerId,                                         │
│        FygaroSubscriptionId = "sub_fygaro_abc123",                          │
│        FygaroCustomerId = "cus_fygaro_xyz789",                              │
│        Plan = SubscriptionPlan.Pro,                                         │
│        Status = SubscriptionStatus.Trialing,                                │
│        CurrentPeriodStart = DateTime.UtcNow,                                │
│        CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),                     │
│        TrialEnd = DateTime.UtcNow.AddDays(90),                              │
│        NextChargeDate = DateTime.UtcNow.AddDays(90),                        │
│        BasePrice = 129.00m,                                                 │
│        Currency = "USD",                                                    │
│        Gateway = PaymentGateway.Fygaro                                      │
│      };                                                                     │
│      await _subscriptionRepo.AddAsync(subscription);                        │
│                                                                             │
│  1️⃣2️⃣  HANDLER: Post-procesamiento                                          │
│      ├─ Actualizar DealerManagement:                                        │
│      │   • Dealer.CurrentPlan = Pro                                        │
│      │   • Dealer.MaxActiveListings = 50                                   │
│      │   • Dealer.IsSubscriptionActive = true                              │
│      ├─ Publicar RabbitMQ: subscription.created                            │
│      ├─ NotificationService → Email bienvenida con detalles:               │
│      │   "Tu suscripción está activa. Primer cobro: 28 Abril 2026"        │
│      └─ Retornar 201 Created a frontend                                    │
│                                                                             │
│  1️⃣3️⃣  FRONTEND: Redirigir a dashboard                                      │
│      /dealer/dashboard                                                      │
│      ├─ Mostrar: "✅ Suscripción activa"                                    │
│      ├─ Mostrar: "Próximo cobro: 28 Abril 2026 - $129"                     │
│      ├─ Mostrar: "Método de pago: •••• 1111 (Visa)"                        │
│      └─ Botón: "Actualizar método de pago"                                 │
│                                                                             │
│  1️⃣4️⃣  FYGARO: Gestión automática (cada mes)                                │
│      ┌──────────────────────────────────────────────┐                      │
│      │ DÍA DEL COBRO (28 de cada mes)              │                      │
│      ├──────────────────────────────────────────────┤                      │
│      │ 1. Fygaro intenta cobrar $129 a tarjeta     │                      │
│      │ 2. Si éxito:                                 │                      │
│      │    • Webhook: subscription.charged           │                      │
│      │    • Backend actualiza: LastPaymentDate      │                      │
│      │    • Email: "Cobro exitoso - $129"           │                      │
│      │ 3. Si falla:                                 │                      │
│      │    • Reintento automático día +2             │                      │
│      │    • Email: "Problema con tu pago"           │                      │
│      │    • Webhook: payment.failed                 │                      │
│      │ 4. Si falla reintento 1:                     │                      │
│      │    • Reintento automático día +4             │                      │
│      │    • Email: "Urgente: Actualiza tu tarjeta"  │                      │
│      │ 5. Si falla reintento 2:                     │                      │
│      │    • Reintento automático día +5             │                      │
│      │    • Email: "Última oportunidad"             │                      │
│      │ 6. Si falla reintento 3:                     │                      │
│      │    • Webhook: subscription.canceled          │                      │
│      │    • Backend: Status = Canceled              │                      │
│      │    • Email: "Suscripción cancelada"          │                      │
│      │    • Dealer.MaxActiveListings = 0            │                      │
│      └──────────────────────────────────────────────┘                      │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Webhooks Fygaro

### Eventos Soportados

| Evento                    | Descripción                 | Acción en Backend                   |
| ------------------------- | --------------------------- | ----------------------------------- |
| `subscription.created`    | Suscripción creada          | Log evento, email confirmación      |
| `subscription.trialing`   | En periodo de prueba        | Recordar fecha de primer cobro      |
| `subscription.active`     | Suscripción activa          | Habilitar funcionalidades completas |
| `subscription.charged`    | Cobro exitoso               | Generar factura, email confirmación |
| `subscription.past_due`   | Pago pendiente              | Notificar dealer, mostrar alerta    |
| `subscription.canceled`   | Suscripción cancelada       | Deshabilitar funcionalidades        |
| `subscription.upgraded`   | Plan mejorado               | Actualizar límites                  |
| `subscription.downgraded` | Plan reducido               | Actualizar límites                  |
| `payment.failed`          | Intento de pago falló       | Email de advertencia                |
| `payment_method.updated`  | Tarjeta actualizada         | Log cambio, email confirmación      |
| `customer.updated`        | Info de cliente actualizada | Sincronizar datos                   |

---

## 3. Configuración

### appsettings.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",
    "Fygaro": {
      "Enabled": true,
      "ApiKey": "sk_live_fygaro_xxxxx",
      "MerchantId": "merchant_xxxxx",
      "WebhookSecret": "whsec_fygaro_xxxxx",
      "ApiUrl": "https://api.fygaro.com/v1",
      "WebhookUrl": "https://api.okla.com.do/webhooks/fygaro",
      "Environment": "Production",
      "TimeoutSeconds": 30,
      "Currency": "USD",
      "EnableDunning": true,
      "MaxRetries": 3,
      "RetrySchedule": [2, 4, 5] // Días después del fallo
    }
  }
}
```

---

## 4. Ventajas de Fygaro para Suscripciones

| Feature                     | Fygaro      | AZUL Manual | PixelPay Manual |
| --------------------------- | ----------- | ----------- | --------------- |
| Cobros automáticos          | ✅ Incluido | ❌ Manual   | ❌ Manual       |
| Reintentos automáticos      | ✅ 3x en 5d | ❌ N/A      | ❌ N/A          |
| Dunning management          | ✅ Emails   | ❌ N/A      | ❌ N/A          |
| Actualizar tarjeta fácil    | ✅ Portal   | ❌ Re-auth  | ❌ Re-auth      |
| Dashboard de churn          | ✅ Completo | ❌ N/A      | ❌ N/A          |
| Webhooks de todo            | ✅ 11 tipos | ❌ Básicos  | ⚠️ Limitados    |
| Comisión                    | 3.0%        | 3.5%        | 1.5%            |
| **Reducción de churn**      | **-40%**    | 0%          | 0%              |
| **Automatización completa** | **✅**      | ❌          | ❌              |

---

## 5. Reducción de Churn con Fygaro

### Ejemplo: 100 dealers con suscripción mensual

**Sin Fygaro (manual):**

- 10% de tarjetas fallan cada mes (expiradas, sin fondos)
- 100% de estos cancelan (no hay reintentos)
- Churn mensual: **10 dealers perdidos**

**Con Fygaro:**

- 10% de tarjetas fallan inicialmente
- Reintentos recuperan 60% (3 intentos + dunning)
- Churn mensual: **4 dealers perdidos**
- **Reducción de churn: 60%** 💰

**Impacto anual:**

- Sin Fygaro: 120 dealers perdidos/año
- Con Fygaro: 48 dealers perdidos/año
- **72 dealers retenidos = $9,288 MRR extra** (asumiendo $129/mes)

---

## 📚 Referencias

- [Fygaro Developer Portal](https://developers.fygaro.com/)
- [Fygaro Subscription Module](https://developers.fygaro.com/subscriptions)
- [01-billing-service.md](01-billing-service.md) - Servicio principal
- [06-subscriptions.md](06-subscriptions.md) - Gestión de suscripciones
- [PaymentService README](../../backend/PaymentService/README.md) - Arquitectura

---

**✅ Fygaro implementado - ⭐ RECOMENDADO PARA SUSCRIPCIONES RECURRENTES**  
_Reintentos automáticos, dunning inteligente, reducción de churn del 60%._
