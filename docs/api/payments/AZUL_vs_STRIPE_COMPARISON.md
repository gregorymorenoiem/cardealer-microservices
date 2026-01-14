# 🔄 Comparación: AZUL vs STRIPE

**Análisis Técnico y de Negocio**  
**Actualizado:** Enero 2026

---

## 📊 Comparativa General

| Característica              | AZUL (Banco Popular RD)                                | STRIPE                                                     |
| --------------------------- | ------------------------------------------------------ | ---------------------------------------------------------- |
| **País de origen**          | 🇩🇴 República Dominicana                                | 🌍 Global (San Francisco, USA)                             |
| **Enfoque**                 | Mercado local dominicano                               | Pagos internacionales                                      |
| **Métodos de pago**         | Tarjetas RD, ACH local, móvil                          | Tarjetas globales, Apple Pay, Google Pay, SEPA             |
| **Monedas soportadas**      | DOP, USD                                               | 135+ monedas                                               |
| **Alcance**                 | Dominicana                                             | 190+ países                                                |
| **Documento de referencia** | [AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md) | [STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md) |

---

## 🔑 Autenticación

### AZUL

```csharp
// Basado en Hash SHA-256
AuthHash = SHA256(StoreId + ApiKey + UnixTimestamp)
Header: Authorization: Bearer {AuthHash}
Header: X-Store-Id: {StoreId}
```

✅ **Ventaja:** Hash dinámico por timestamp (más seguro)  
❌ **Desventaja:** Require cálculo adicional

### STRIPE

```csharp
// Basado en Bearer Token
Header: Authorization: Bearer {SecretKey}
// O Basic Auth
Header: Authorization: Basic {base64(key:)}
```

✅ **Ventaja:** Más simple, directo  
❌ **Desventaja:** Token estático

---

## 💳 Métodos de Pago

### AZUL - Métodos Locales

```json
{
  "methods": [
    "CREDIT_CARD", // Visa, Mastercard, Amex
    "DEBIT_CARD", // Tarjetas de débito RD
    "ACH", // Transferencia bancaria local
    "MOBILE_PAYMENT", // Orange Money, Claro Money
    "E_WALLET" // Billeteras electrónicas
  ]
}
```

**Mejor para:** Ventas locales en RD  
**Cobertura:** ~95% del mercado dominicano

### STRIPE - Métodos Globales

```json
{
  "methods": [
    "CARD", // Visa, Mastercard, Amex, Discover
    "APPLE_PAY", // Apple Pay (iOS)
    "GOOGLE_PAY", // Google Pay (Android)
    "LINK", // Stripe Link (pago rápido)
    "ACH_TRANSFER", // ACH USA
    "IDEAL", // iDEAL (Holanda)
    "GIROPAY", // Giropay (Alemania)
    "SEPA_DEBIT", // Débito SEPA (Europa)
    "BITCOIN", // Bitcoin
    "WECHAT_PAY", // WeChat (China)
    "ALIPAY" // Alipay (China)
  ]
}
```

**Mejor para:** Clientes internacionales  
**Cobertura:** Prácticamente global

---

## 💰 Precios y Comisiones

### AZUL

| Tipo            | Comisión | Depósito | Notas                 |
| --------------- | -------- | -------- | --------------------- |
| **Sale**        | ~2.5%    | 24-48h   | En cuenta bancaria    |
| **Refund**      | Gratis   | -48h     | Reversión automática  |
| **Suscripción** | ~2.5%    | 24-48h   | Recurrente automática |
| **Monthly Fee** | Opcional | -        | Desde $0 a $50/mes    |

### STRIPE

| Tipo                     | Comisión            | Depósito  | Notas                  |
| ------------------------ | ------------------- | --------- | ---------------------- |
| **Card charge**          | 2.9% + $0.30        | 1-2 días  | Acumulado a payout     |
| **ACH transfer**         | 1% (max $5)         | 5-7 días  | Más barato que tarjeta |
| **International card**   | 3.9% + $0.30        | 1-2 días  | Tarjetas extranjeras   |
| **Refund**               | Gratis              | Inmediato | A la tarjeta original  |
| **Monthly Subscription** | Variable según plan | -         | Planes: $29, $99, $299 |

---

## 📡 Endpoints y Estructura API

### AZUL - REST Simple

```
POST   /transactions/sale
POST   /transactions/authorize
POST   /transactions/{id}/capture
POST   /transactions/{id}/void
POST   /transactions/{id}/refund
GET    /transactions/{id}
GET    /transactions?filters...
POST   /subscriptions
PUT    /subscriptions/{id}
DELETE /subscriptions/{id}
POST   /tokens/cards
```

**Características:**

- URLs simples y predecibles
- Operaciones directas (sale, auth, capture, void, refund)
- Subscripciones integradas
- Webhooks con validación SHA-256

### STRIPE - REST Moderno

```
POST   /payment_intents
POST   /payment_intents/{id}/confirm
POST   /payment_intents/{id}/cancel
POST   /customers
POST   /customers/{id}
POST   /charges (legacy)
POST   /refunds
POST   /subscriptions
POST   /products
POST   /prices
POST   /invoices
POST   /webhook_endpoints
```

**Características:**

- Filosofía "moderno y RESTful"
- Payment Intents (flujo con múltiples pasos)
- Setup Intents (tokens recurrentes)
- Facturación integrada (invoices)
- Webhooks con validación HMAC-SHA256

---

## 🔄 Flujos de Pago

### AZUL - Tradicional (2 pasos)

```
1. Autorizar
   ↓
2. Capturar
   ↓
✅ Pago completado

O directo (1 paso):
1. Sale (autorizar + capturar en uno)
   ↓
✅ Pago completado
```

### STRIPE - Payment Intents (3 pasos)

```
1. Crear Payment Intent
   ↓
2. Confirmar con método de pago
   ↓
3. Procesar (automático o manual)
   ↓
✅ Pago completado
```

**Ventaja STRIPE:** Soporta flujos complejos (3D Secure, confirmación adicional)

---

## 🔐 Seguridad y Compliance

### AZUL

- ✅ PCI-DSS Level 1
- ✅ Encriptación SSL/TLS
- ✅ Validación de CVV
- ✅ 3D Secure (opcional)
- ✅ Protección contra fraude local
- ❌ No soporta Apple/Google Pay nativamente

### STRIPE

- ✅ PCI-DSS Level 1
- ✅ Encriptación TLS 1.2+
- ✅ Validación de CVV
- ✅ 3D Secure 2.0 (automático)
- ✅ Machine Learning antifraud (líder industria)
- ✅ Apple/Google Pay nativo
- ✅ Radar (detección de fraude avanzada)
- ✅ Compliance global (GDPR, CCPA)

---

## 📈 Suscripciones y Recurrencia

### AZUL

```csharp
POST /subscriptions
{
  "customerId": "CUST-001",
  "amount": 500.00,
  "frequency": "MONTHLY",
  "startDate": "2026-02-01",
  "endDate": "2026-12-31"
}
```

**Características:**

- Frecuencias simples (daily, weekly, monthly, etc.)
- Fecha de inicio y fin
- Cancelación manual
- Webhooks de eventos

### STRIPE

```csharp
// 1. Crear producto
POST /products
{ "name": "Pro Plan" }

// 2. Crear precio recurrente
POST /prices
{
  "product": "prod_xxx",
  "recurring": {
    "interval": "month",
    "interval_count": 1
  },
  "unit_amount": 12900
}

// 3. Crear suscripción
POST /subscriptions
{
  "customer": "cus_xxx",
  "items": [{"price": "price_xxx"}],
  "trial_period_days": 14
}
```

**Características:**

- Modelo de productos + precios (flexible)
- Trial periods
- Metering (uso basado)
- Scheduled subscriptions
- Facturación automática

---

## 🪝 Webhooks

### AZUL

**Eventos:**

```
transaction.approved
transaction.declined
transaction.pending
subscription.created
subscription.charged
subscription.failed
```

**Validación:**

```csharp
var signature = request.Header["X-Azul-Signature"];
var expectedSig = SHA256(payload + apiKey);
// Validar que signature == expectedSig
```

### STRIPE

**Eventos:** (30+ eventos)

```
payment_intent.succeeded
payment_intent.payment_failed
charge.succeeded
invoice.paid
invoice.payment_failed
subscription.created
subscription.updated
subscription.deleted
customer.created
customer.deleted
```

**Validación:**

```csharp
var signatureHeader = request.Header["Stripe-Signature"];
var stripeEvent = EventUtility.ConstructEvent(
    json,
    signatureHeader,
    webhookSecret
);
```

---

## 🌍 Estrategia de Integración

### Escenario 1: Ventas LOCALES (Dominicana)

**Recomendación:** ✅ **AZUL**

```
Razones:
- Comisiones más bajas (~2.5% vs 3.2% promedio)
- Depósitos más rápidos (24-48h vs 1-2 días)
- Métodos de pago locales (Móvil, ACH local)
- Mejor UX para usuarios dominicanos
- Soporte local en RD
```

**Implementación:**

```csharp
// Usar AzulPaymentService para:
- Buyers locales
- Dealers en RD
- Suscripciones mensuales dealers
```

### Escenario 2: Ventas INTERNACIONALES

**Recomendación:** ✅ **STRIPE**

```
Razones:
- Cobertura global (190+ países)
- Apple/Google Pay nativo
- Antifraud machine learning avanzado
- Facturación integrada
- Mejor soporte para casos complejos
```

**Implementación:**

```csharp
// Usar StripePaymentService para:
- Buyers internacionales
- Dealers internacionales
- Métodos de pago globales
- Suscripciones premium internacionales
```

### Escenario 3: HÍBRIDO (RECOMENDADO)

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO HÍBRIDO ÓPTIMO                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  COMPRADOR DOMINICANO                                       │
│  ├─ Detectar país: RD                                       │
│  ├─ Ofrecer AZUL (Móvil, tarjeta local)                    │
│  └─ Fallback: STRIPE (Apple/Google Pay)                    │
│                                                               │
│  COMPRADOR INTERNACIONAL                                    │
│  ├─ Detectar país: USA, MEX, COL, etc.                     │
│  ├─ Ofrecer STRIPE (tarjeta, Apple Pay, Google Pay)        │
│  └─ Fallback: AZUL (si soporta)                            │
│                                                               │
│  DEALER DOMINICANO                                          │
│  ├─ Suscripción mensual → AZUL (comisión menor)           │
│  ├─ Pago único → AZUL o STRIPE (elegir)                    │
│  └─ Método: Seleccionar en checkout                        │
│                                                               │
│  DEALER INTERNACIONAL                                       │
│  ├─ Suscripción mensual → STRIPE (cobertura)              │
│  ├─ Pago único → STRIPE (principal) + AZUL (RD)           │
│  └─ Método: Auto-seleccionar por país                      │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Implementación en OKLA

### AzulPaymentService - Cuándo Usar

```csharp
public async Task<PaymentResult> ProcessPaymentAsync(
    Order order,
    PaymentMethod method)
{
    // Si buyer es dominicano y usa método local
    if (order.Buyer.Country == "DO" &&
        method.Type.IsLocal()) // Mobile, ACH, etc.
    {
        return await _azulService.ChargeAsync(order);
    }

    // Si dealer quiere suscripción mensual en RD
    if (order.Dealer.Country == "DO" &&
        order.IsSubscription)
    {
        return await _azulService.CreateSubscriptionAsync(order);
    }

    // Por defecto, fallback a STRIPE
    return await _stripeService.ChargeAsync(order);
}
```

### StripePaymentService - Cuándo Usar

```csharp
public async Task<PaymentResult> ProcessPaymentAsync(
    Order order,
    PaymentMethod method)
{
    // Si buyer es internacional
    if (order.Buyer.Country != "DO")
    {
        return await _stripeService.ChargeAsync(order);
    }

    // Si buyer quiere usar Apple/Google Pay
    if (method.Type.IsAppleOrGooglePay())
    {
        return await _stripeService.ChargeAsync(order);
    }

    // Si es suscripción de dealer premium
    if (order.Dealer.Plan.IsPremium)
    {
        return await _stripeService.CreateSubscriptionAsync(order);
    }

    return null; // Let default behavior decide
}
```

---

## 📋 Checklist de Integración

### AZUL Payment Service

- [ ] Crear estructura Clean Architecture (Domain, App, Infra, Api)
- [ ] Implementar autenticación (SHA256 hash)
- [ ] Crear controlador `PaymentsController` con endpoints:
  - [ ] POST `/api/azul-payment/charge` (Sale)
  - [ ] POST `/api/azul-payment/authorize` (Pre-auth)
  - [ ] POST `/api/azul-payment/capture/{txnId}` (Capture)
  - [ ] POST `/api/azul-payment/refund/{txnId}` (Refund)
  - [ ] GET `/api/azul-payment/transactions/{txnId}` (Status)
  - [ ] POST `/api/azul-payment/subscriptions` (Suscripción)
- [ ] Implementar manejo de webhooks
- [ ] Tests unitarios (mínimo 10)
- [ ] Documentación de métodos
- [ ] Integración con BillingService

### STRIPE Payment Service

- [ ] Crear estructura Clean Architecture
- [ ] Instalar Stripe.net NuGet (v42.12.0+)
- [ ] Implementar autenticación (Secret Key)
- [ ] Crear controlador `PaymentsController` con endpoints:
  - [ ] POST `/api/stripe-payment/intents` (Crear PI)
  - [ ] POST `/api/stripe-payment/intents/{id}/confirm` (Confirmar)
  - [ ] POST `/api/stripe-payment/refunds` (Reembolso)
  - [ ] GET `/api/stripe-payment/customers/{id}` (Cliente)
  - [ ] POST `/api/stripe-payment/subscriptions` (Suscripción)
- [ ] Implementar manejo de webhooks (signature validation)
- [ ] Tests unitarios (mínimo 10)
- [ ] Documentación de métodos
- [ ] Integración con BillingService

---

## 📚 Documentación Completa

✅ **AZUL:** [AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md) - 400+ líneas  
✅ **STRIPE:** [STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md) - 500+ líneas

**Cada documentación incluye:**

- Métodos de autenticación
- Todos los endpoints
- Ejemplos de requests/responses
- Código C# completo
- Manejo de webhooks
- Códigos de error
- Tarjetas de prueba

---

## 🚀 Recomendación Final

**Para OKLA (República Dominicana + Mercado Global):**

### Fase 1 (MVP) - Q1 2026

Implementar **AMBAS en paralelo:**

- **AZUL:** Para dealers dominicanos (50% del mercado inicial)
- **STRIPE:** Para acceso internacional (primeros 50% de growth)

### Fase 2 (Growth) - Q2 2026

Optimizar routing:

- Detector de país + método de pago
- A/B testing de conversiones
- Análisis de comisiones

### Fase 3 (Scale) - Q3 2026

Agregar métodos regionales:

- PayPal para Latinoamérica
- Mercado Pago (ARG, BRA, MEX)
- Locales por país (iDEAL, Giropay, etc.)

---

**Conclusión:** Implementar ambas APIs desde el inicio da máxima flexibilidad, cobertura y mejor UX para users locales e internacionales.
