# 🌎 PayPal Payment - Pagos Internacionales con PayPal - Matriz de Procesos

> **Proveedor:** PayPal (Fintech Global)  
> **Tipo:** Fintech Internacional  
> **Puerto:** 15105 (PaymentService)  
> **Última actualización:** Enero 28, 2026  
> **Estado:** ✅ IMPLEMENTADO - **⭐ RECOMENDADO PARA CLIENTES INTERNACIONALES**

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 28, 2026)

| Proceso           | Backend | UI Access | Observación                        |
| ----------------- | ------- | --------- | ---------------------------------- |
| PAYPAL-INT-PAY-\* | ✅ 100% | ✅ 100%   | `/checkout` (selector de pasarela) |

### Rutas UI Existentes ✅

- ✅ `/checkout` - CheckoutPage (**PayPal para clientes internacionales**)

---

## 1. Descripción General

**PayPal** es el proveedor global de pagos más reconocido, aceptado en **200+ países**. Es ideal para dealers/compradores internacionales que quieren pagar desde fuera de República Dominicana o preferir pagar con balance de PayPal.

### Características Principales

- **Tipo:** Fintech Internacional
- **Comisión:** 2.9% + US$0.30
- **Costo fijo:** US$0.30 por transacción
- **Mensualidad:** Gratis (con límites, planes premium opcionales)
- **Tokenización:** Nativa (PayPal Vault)
- **Monedas:** **USD, EUR, DOP** + 25 más
- **Cobertura:** 🌎 **200+ países**
- **Depósito:** 24-48 horas
- **Reconocimiento de marca:** ⭐⭐⭐⭐⭐ (universal)
- **Protección al comprador:** ✅ Incluida

### ⭐ Ventajas Clave

1. **Alcance global** - Aceptado en todo el mundo
2. **Confianza del usuario** - Marca reconocida universalmente
3. **Balance PayPal** - Usuarios pueden pagar sin tarjeta
4. **Multi-moneda** - Soporte 25+ monedas
5. **Protección comprador** - Disputa/reembolso management
6. **Sin setup fees** - Comienza gratis

### Casos de Uso

1. **Dealers internacionales** - Expatriados dominicanos
2. **Compradores desde USA/EUR** - Turistas comprando vehículos
3. **Pagos en EUR** - Clientes europeos
4. **Sin tarjeta dominicana** - Alternativa para no residentes
5. **Preferred by expats** - Dominicanos en el exterior

---

## 1.1 Arquitectura PayPal

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ARQUITECTURA PAYPAL PAYMENT                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Frontend (React)                                                           │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  CheckoutPage                                              │            │
│  │  ├─ PaymentMethodSelector                                 │            │
│  │  │   ├─ AZUL (dominicanos)                                │            │
│  │  │   ├─ PixelPay (volumen alto)                           │            │
│  │  │   └─ ⭐ PayPal (internacional) 🌎                       │            │
│  │  ├─ PayPalButtonContainer                                 │            │
│  │  │   • Botón oficial de PayPal (SDK)                     │            │
│  │  │   • Popup PayPal login                                 │            │
│  │  │   • Seleccionar fuente de pago:                        │            │
│  │  │     - Balance PayPal                                   │            │
│  │  │     - Tarjeta vinculada                                │            │
│  │  │     - Cuenta bancaria                                  │            │
│  │  └─ onApprove → POST /api/payments/paypal/capture         │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  API Gateway (:18443)                                                       │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  /api/payments/* → PaymentService:15105                    │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  PaymentService (:15105)                                                    │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  PaymentController                                         │            │
│  │  ├─ POST /api/payments/paypal/create-order                 │            │
│  │  │   └─ Crear order en PayPal, retornar order_id          │            │
│  │  ├─ POST /api/payments/paypal/capture                      │            │
│  │  │   └─ Capturar pago después de aprobación               │            │
│  │  └─ POST /api/payments/paypal/refund                       │            │
│  │      └─ Procesar reembolso                                 │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  PayPalPaymentProvider                                                      │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  ├─ ValidateConfiguration() (ClientId, Secret)             │            │
│  │  ├─ GetAccessToken() (OAuth 2.0)                           │            │
│  │  ├─ CreateOrder()                                           │            │
│  │  │   POST /v2/checkout/orders                              │            │
│  │  │   {                                                      │            │
│  │  │     "intent": "CAPTURE",                                │            │
│  │  │     "purchase_units": [{                                │            │
│  │  │       "amount": {                                       │            │
│  │  │         "currency_code": "USD",                         │            │
│  │  │         "value": "49.00"                                │            │
│  │  │       },                                                │            │
│  │  │       "description": "Suscripción Starter - 1 mes"     │            │
│  │  │     }]                                                  │            │
│  │  │   }                                                      │            │
│  │  ├─ CaptureOrder(orderId)                                  │            │
│  │  │   POST /v2/checkout/orders/{order_id}/capture           │            │
│  │  ├─ RefundPayment(captureId, amount)                       │            │
│  │  │   POST /v2/payments/captures/{capture_id}/refund        │            │
│  │  └─ ParsePayPalResponse()                                  │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  💰 PayPal API (External)                                                   │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  PayPal Checkout API                                       │            │
│  │  ├─ OAuth 2.0 Authentication                               │            │
│  │  ├─ Create Order (retorna order_id)                        │            │
│  │  ├─ Usuario aprueba en popup PayPal                        │            │
│  │  ├─ Capture Order (procesar pago)                          │            │
│  │  ├─ Conversión de moneda automática                        │            │
│  │  ├─ Protección al comprador activada                       │            │
│  │  └─ Response:                                               │            │
│  │     {                                                       │            │
│  │       "id": "PAYID-XXXXXX",                                 │            │
│  │       "status": "COMPLETED",                                │            │
│  │       "purchase_units": [{                                  │            │
│  │         "payments": {                                       │            │
│  │           "captures": [{                                    │            │
│  │             "id": "CAPTURE-ID",                             │            │
│  │             "status": "COMPLETED",                          │            │
│  │             "amount": {                                     │            │
│  │               "currency_code": "USD",                       │            │
│  │               "value": "49.00"                              │            │
│  │             },                                              │            │
│  │             "seller_receivable_breakdown": {                │            │
│  │               "net_amount": { "value": "47.28" } // Fee deducted │      │
│  │             }                                                │            │
│  │           }]                                                │            │
│  │         }                                                   │            │
│  │       }]                                                    │            │
│  │     }                                                       │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  POST-PROCESAMIENTO                                                         │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  1. Guardar PaymentTransaction en DB                       │            │
│  │     • TransactionId = "PAYID-XXXXXX"                       │            │
│  │     • Gateway = PayPal                                     │            │
│  │     • Amount = 49.00 USD                                   │            │
│  │     • Commission = 1.72 (2.9% + $0.30)                     │            │
│  │     • NetAmount = 47.28                                    │            │
│  │     • Status = Completed                                   │            │
│  │  2. Actualizar DealerSubscription                          │            │
│  │  3. Publicar evento: payment.completed (RabbitMQ)          │            │
│  │  4. Trigger InvoicingService (NCF + DGII)                  │            │
│  │  5. NotificationService → Email confirmación               │            │
│  │  6. Programar depósito (24-48h)                            │            │
│  └────────────────────────────────────────────────────────────┘            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1.2 Flujo de Pago PayPal (2-Phase: Create + Capture)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     FLUJO DE PAGO PAYPAL (2 Fases)                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  FASE 1: CREATE ORDER (Pre-autorización)                                   │
│  ═══════════════════════════════════════                                   │
│                                                                             │
│  1️⃣  CLIENTE: Seleccionar PayPal                                            │
│      ├─ Click "Pagar con PayPal" (botón oficial)                           │
│      └─ Frontend: POST /api/payments/paypal/create-order                    │
│                                                                             │
│  2️⃣  BACKEND: Crear orden en PayPal                                         │
│      POST https://api.paypal.com/v2/checkout/orders                         │
│      {                                                                      │
│        "intent": "CAPTURE",                                                 │
│        "purchase_units": [{                                                 │
│          "reference_id": "OKLA-SUB-001",                                    │
│          "description": "Suscripción Pro - 1 mes",                          │
│          "amount": {                                                        │
│            "currency_code": "USD",                                          │
│            "value": "129.00"                                                │
│          }                                                                  │
│        }],                                                                  │
│        "application_context": {                                             │
│          "return_url": "https://okla.com.do/billing/paypal/success",        │
│          "cancel_url": "https://okla.com.do/billing/paypal/cancel"          │
│        }                                                                    │
│      }                                                                      │
│                                                                             │
│  3️⃣  PAYPAL API: Response con Order ID                                      │
│      {                                                                      │
│        "id": "ORDER-12345ABCDE",                                            │
│        "status": "CREATED",                                                 │
│        "links": [{                                                          │
│          "rel": "approve",                                                  │
│          "href": "https://www.paypal.com/checkoutnow?token=ORDER-12345..."  │
│        }]                                                                   │
│      }                                                                      │
│                                                                             │
│  4️⃣  FRONTEND: Abrir popup PayPal                                           │
│      window.open(approveUrl)                                                │
│      ├─ Usuario ve login PayPal                                            │
│      ├─ Ingresa email/password de PayPal                                   │
│      ├─ Selecciona fuente de pago:                                         │
│      │   • Balance PayPal                                                  │
│      │   • Tarjeta Visa **** 1234                                          │
│      │   • Cuenta bancaria Wells Fargo                                     │
│      ├─ Revisa resumen: $129.00 USD a OKLA                                 │
│      └─ Click "Aprobar"                                                     │
│                                                                             │
│  5️⃣  PAYPAL: Redirigir a return_url                                         │
│      https://okla.com.do/billing/paypal/success?token=ORDER-12345&PayerID=XXX │
│                                                                             │
│  ═════════════════════════════════════════════════════════════════════════ │
│                                                                             │
│  FASE 2: CAPTURE ORDER (Capturar el pago)                                  │
│  ═══════════════════════════════════════                                   │
│                                                                             │
│  6️⃣  FRONTEND: Extraer token y PayerID                                      │
│      const orderId = new URLParams(location.search).get('token');          │
│      POST /api/payments/paypal/capture                                      │
│      { "orderId": "ORDER-12345ABCDE" }                                      │
│                                                                             │
│  7️⃣  BACKEND: Capturar orden en PayPal                                      │
│      POST https://api.paypal.com/v2/checkout/orders/ORDER-12345ABCDE/capture │
│      Headers: {                                                             │
│        "Authorization": "Bearer {access_token}",                            │
│        "Content-Type": "application/json"                                   │
│      }                                                                      │
│                                                                             │
│  8️⃣  PAYPAL API: Procesar pago                                              │
│      ├─ Verificar fondos en balance/tarjeta/banco                          │
│      ├─ Aplicar comisión (2.9% + $0.30)                                    │
│      ├─ Transferir $129 - fee = $125.26 a OKLA                             │
│      └─ Response:                                                           │
│         {                                                                   │
│           "id": "ORDER-12345ABCDE",                                         │
│           "status": "COMPLETED",                                            │
│           "purchase_units": [{                                              │
│             "payments": {                                                   │
│               "captures": [{                                                │
│                 "id": "CAPTURE-67890FGHIJ",                                 │
│                 "status": "COMPLETED",                                      │
│                 "amount": {                                                 │
│                   "currency_code": "USD",                                   │
│                   "value": "129.00"                                         │
│                 },                                                          │
│                 "seller_receivable_breakdown": {                            │
│                   "gross_amount": { "value": "129.00" },                    │
│                   "paypal_fee": { "value": "3.74" }, // 2.9% + $0.30       │
│                   "net_amount": { "value": "125.26" }                       │
│                 },                                                          │
│                 "create_time": "2026-01-28T10:30:00Z"                       │
│               }]                                                            │
│             }                                                               │
│           }]                                                                │
│         }                                                                   │
│                                                                             │
│  9️⃣  BACKEND: Parsear response                                              │
│      if (response.Status == "COMPLETED") {                                  │
│        var capture = response.PurchaseUnits[0].Payments.Captures[0];        │
│        return PaymentResult.Success(                                        │
│          transactionId: capture.Id,                                         │
│          orderId: response.Id,                                              │
│          amount: decimal.Parse(capture.Amount.Value),                       │
│          commission: decimal.Parse(capture.SellerReceivableBreakdown.PayPalFee.Value), │
│          netAmount: decimal.Parse(capture.SellerReceivableBreakdown.NetAmount.Value)   │
│        );                                                                   │
│      }                                                                      │
│                                                                             │
│  🔟  BACKEND: Post-procesamiento                                            │
│      ├─ Guardar PaymentTransaction:                                        │
│      │   • TransactionId = "CAPTURE-67890FGHIJ"                            │
│      │   • OrderId = "ORDER-12345ABCDE"                                    │
│      │   • Gateway = PayPal                                                │
│      │   • Amount = 129.00                                                 │
│      │   • Commission = 3.74                                               │
│      │   • NetAmount = 125.26                                              │
│      │   • Status = Completed                                              │
│      ├─ Actualizar DealerSubscription.IsActive = true                      │
│      ├─ Publicar RabbitMQ: payment.completed                               │
│      ├─ Trigger InvoicingService → NCF + DGII                              │
│      ├─ NotificationService → Email confirmación                           │
│      └─ Programar depósito (24-48h a cuenta OKLA)                          │
│                                                                             │
│  1️⃣1️⃣  FRONTEND: Mostrar resultado                                          │
│      /billing/success                                                       │
│      ├─ ✅ "Pago procesado exitosamente con PayPal"                         │
│      ├─ Detalles: $129.00 USD pagados                                      │
│      ├─ Transacción: CAPTURE-67890FGHIJ                                    │
│      └─ Botón: "Ir a Dashboard"                                             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Códigos de Estado PayPal

| Status                  | Descripción                  | Acción                   |
| ----------------------- | ---------------------------- | ------------------------ |
| `CREATED`               | Order creado, esperando      | ⏳ Usuario debe aprobar  |
| `APPROVED`              | Usuario aprobó, no capturado | ▶️ Capturar orden        |
| `COMPLETED`             | Pago completado exitosamente | ✅ Continuar             |
| `VOIDED`                | Orden cancelada              | ❌ Reintentar            |
| `PAYER_ACTION_REQUIRED` | Acción adicional requerida   | ⚠️ Seguir link de PayPal |

---

## 3. Configuración

### appsettings.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",
    "PayPal": {
      "Enabled": true,
      "ClientId": "AbcdEfgh1234567890_CLIENT_ID",
      "ClientSecret": "AbcdEfgh1234567890_CLIENT_SECRET",
      "Mode": "Live", // Live | Sandbox
      "ApiUrl": "https://api.paypal.com",
      "WebhookId": "WH-xxxxx",
      "TimeoutSeconds": 30,
      "Currency": "USD",
      "ReturnUrl": "https://okla.com.do/billing/paypal/success",
      "CancelUrl": "https://okla.com.do/billing/paypal/cancel"
    }
  }
}
```

---

## 4. Comparación: PayPal vs Otros Proveedores

### Para Cliente Dominicano (en RD)

| Característica       | AZUL    | PixelPay | PayPal     |
| -------------------- | ------- | -------- | ---------- |
| Comisión             | 3.5%    | 1.5%     | 2.9%       |
| Costo fijo           | $0      | $0.15    | $0.30      |
| Cobertura            | 🇩🇴 Solo | 🇩🇴 LAT   | 🌎 200+    |
| Reconocimiento marca | ⭐⭐⭐  | ⭐⭐     | ⭐⭐⭐⭐⭐ |
| **Recomendación**    | ✅      | ✅       | ⚠️         |

### Para Cliente Internacional (USA/EUR)

| Característica     | AZUL       | PixelPay | PayPal     |
| ------------------ | ---------- | -------- | ---------- |
| Tarjeta extranjera | ⚠️ Varía   | ⚠️ Varía | ✅         |
| Balance propio     | ❌         | ❌       | ✅         |
| Multi-moneda       | ❌ DOP/USD | ✅ 3     | ✅ 25+     |
| Confianza global   | ⭐⭐       | ⭐⭐     | ⭐⭐⭐⭐⭐ |
| **Recomendación**  | ❌         | ⚠️       | ✅ ⭐      |

---

## 5. PayPal vs Stripe (Eliminado)

**Por qué PayPal es mejor alternativa que Stripe para OKLA:**

| Feature              | PayPal            | Stripe (Removido) |
| -------------------- | ----------------- | ----------------- |
| Setup                | ✅ Fácil          | ⚠️ Complejo       |
| Sin tarjeta          | ✅ Balance PayPal | ❌ Solo tarjetas  |
| Reconocimiento RD    | ⭐⭐⭐⭐⭐        | ⭐⭐⭐            |
| Comisión             | 2.9% + $0.30      | 2.9% + $0.30      |
| Protección comprador | ✅ Robusta        | ✅ Básica         |
| Popularidad global   | #1                | #2                |
| **Decisión OKLA**    | ✅ IMPLEMENTADO   | ❌ REMOVIDO       |

---

## 6. Testing

### Request: Create Order

```json
POST /api/payments/paypal/create-order
{
  "amount": 299.00,
  "currency": "USD",
  "description": "Suscripción Enterprise - 1 mes",
  "dealerId": "guid"
}
```

### Response: Order Created

```json
{
  "orderId": "ORDER-12345ABCDE",
  "status": "CREATED",
  "approveUrl": "https://www.paypal.com/checkoutnow?token=ORDER-12345ABCDE",
  "amount": 299.0,
  "currency": "USD"
}
```

### Request: Capture Order

```json
POST /api/payments/paypal/capture
{
  "orderId": "ORDER-12345ABCDE"
}
```

### Response: Payment Completed

```json
{
  "success": true,
  "transactionId": "CAPTURE-67890FGHIJ",
  "orderId": "ORDER-12345ABCDE",
  "gateway": "PayPal",
  "amount": 299.0,
  "commission": 8.97, // 2.9% + $0.30
  "netAmount": 290.03,
  "currency": "USD",
  "status": "Completed",
  "timestamp": "2026-01-28T10:30:00Z"
}
```

---

## 7. Caso de Uso Real: Expatriado Dominicano

### Perfil

- **Nombre:** Carlos Pérez
- **Ubicación:** New York, USA
- **Vehículo:** Quiere comprar Honda Civic 2020 en RD
- **Situación:** No tiene tarjeta dominicana activa

### Flujo con PayPal

1. **Navega OKLA desde USA**
2. **Encuentra vehículo perfecto**
3. **Click "Contactar Dealer"** → Requiere cuenta
4. **Registrarse:** Crea cuenta como comprador
5. **Quiere suscripción Premium** (ver más listings)
6. **Checkout:** Selecciona PayPal (única opción que tiene)
7. **Login PayPal:** Usa su cuenta USA
8. **Paga con balance** de PayPal (tiene $500 disponibles)
9. **Pago exitoso:** Acceso inmediato a OKLA Premium
10. **Contacta 5 dealers** para negociar precio

**Sin PayPal:**

- ❌ No podría pagar (sin tarjeta dominicana)
- ❌ Perdería interés
- ❌ OKLA pierde cliente internacional

**Con PayPal:**

- ✅ Pago exitoso en 2 minutos
- ✅ OKLA gana $47.28 neto
- ✅ Cliente satisfecho, puede comprar vehículo

---

## 📚 Referencias

- [PayPal Developer Portal](https://developer.paypal.com/)
- [PayPal Checkout Integration](https://developer.paypal.com/docs/checkout/)
- [PayPal REST API Reference](https://developer.paypal.com/api/rest/)
- [01-billing-service.md](01-billing-service.md) - Servicio principal
- [PaymentService README](../../backend/PaymentService/README.md) - Arquitectura

---

**✅ PayPal implementado - ⭐ RECOMENDADO PARA CLIENTES INTERNACIONALES**  
_Alcance global 200+ países, confianza universal, ideal para expatriados dominicanos._
