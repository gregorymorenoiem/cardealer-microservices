# 💸 PixelPay Payment - Pagos con PixelPay - Matriz de Procesos

> **Proveedor:** PixelPay (Fintech)  
> **Tipo:** Fintech  
> **Puerto:** 15105 (PaymentService)  
> **Última actualización:** Enero 28, 2026  
> **Estado:** ✅ IMPLEMENTADO - **⭐ RECOMENDADO PARA VOLUMEN ALTO**

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 28, 2026)

| Proceso      | Backend | UI Access | Observación                        |
| ------------ | ------- | --------- | ---------------------------------- |
| PIXEL-PAY-\* | ✅ 100% | ✅ 100%   | `/checkout` (selector de pasarela) |

### Rutas UI Existentes ✅

- ✅ `/checkout` - CheckoutPage (**PixelPay recomendado para volumen alto**)

---

## 1. Descripción General

**PixelPay** es una fintech moderna con **comisiones más bajas** (1.0%-3.5%) que las bancarias tradicionales. Es la **opción recomendada** para dealers con alto volumen de transacciones.

### Características Principales

- **Tipo:** Fintech (tecnología financiera moderna)
- **Comisión:** **1.0% - 3.5%** 💰 (MÁS BAJA)
- **Costo fijo:** US$0.15 - 0.25
- **Mensualidad:** Varía (planes escalables)
- **Tokenización:** Nativa (API fácil de integrar)
- **Monedas:** DOP, USD, EUR
- **Cobertura:** 🇩🇴 RD + 🌎 LAT (Latinoamérica)
- **Depósito:** 48-72 horas
- **API:** REST moderna, bien documentada

### ⭐ Ventajas Clave

1. **Comisiones más bajas** - Ahorra hasta 2.5% vs bancarias
2. **API moderna** - Integración rápida y fácil
3. **Dashboard analytics** - Métricas en tiempo real
4. **Soporte LAT** - No solo RD, útil para expansión
5. **EUR soporte** - Clientes europeos pueden pagar
6. **Tokenización nativa** - Sin setup adicional

### Casos de Uso

1. **Volumen alto** - Dealers con 50+ transacciones/mes
2. **Ahorro de costos** - Reducir comisiones significativamente
3. **Expansión LAT** - Preparado para otros países
4. **Pagos en EUR** - Clientes europeos/expatriados

---

## 1.1 Arquitectura PixelPay

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ARQUITECTURA PIXELPAY PAYMENT                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Frontend (React)                                                           │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  CheckoutPage                                              │            │
│  │  ├─ PaymentMethodSelector                                 │            │
│  │  │   ├─ AZUL (3.5%)                                       │            │
│  │  │   ├─ CardNET (3.0%)                                    │            │
│  │  │   └─ ⭐ PixelPay (1.5%) ← RECOMENDADO                   │            │
│  │  ├─ PixelPayPaymentForm                                   │            │
│  │  │   ├─ CardNumber (con Luhn validation)                 │            │
│  │  │   ├─ CVV (3-4 dígitos)                                │            │
│  │  │   ├─ ExpiryDate (MM/YY)                               │            │
│  │  │   ├─ CardholderName                                   │            │
│  │  │   └─ CurrencySelector (DOP/USD/EUR)                   │            │
│  │  └─ SubmitButton → POST /api/payments/charge             │            │
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
│  │  ├─ POST /api/payments/charge                              │            │
│  │  │   └─ MediatR → ChargeCommand                            │            │
│  │  └─ ChargeHandler                                          │            │
│  │      ├─ PaymentGatewayFactory.GetProvider(PixelPay)        │            │
│  │      ├─ PixelPayPaymentProvider.ChargeAsync()              │            │
│  │      └─ Save PaymentTransaction (gateway=PixelPay)         │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  PixelPayPaymentProvider                                                    │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  ├─ ValidateConfiguration() (ApiKey, PublicKey)            │            │
│  │  ├─ ValidateRequest(Card, Amount, Currency)                │            │
│  │  ├─ EncryptCardData() (tokenización nativa)                │            │
│  │  ├─ BuildPixelPayRequest()                                 │            │
│  │  │   • api_key                                             │            │
│  │  │   • amount (centavos)                                   │            │
│  │  │   • currency (DOP|USD|EUR)                              │            │
│  │  │   • card_token (encrypted)                              │            │
│  │  │   • customer_email                                      │            │
│  │  │   • order_id                                            │            │
│  │  │   • webhook_url                                         │            │
│  │  ├─ HTTP POST → PixelPay API                               │            │
│  │  ├─ ParsePixelPayResponse() (JSON moderno)                 │            │
│  │  └─ MapToPaymentResult()                                   │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  💳 PixelPay API (External)                                                 │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  • Tokenización automática de tarjeta                      │            │
│  │  • Validación de fondos                                    │            │
│  │  • Procesamiento de cargo                                  │            │
│  │  • Anti-fraude con ML                                      │            │
│  │  • Webhook notification (async)                            │            │
│  │  • Dashboard analytics en tiempo real                      │            │
│  │  • Response: { "status": "success", "txn_id": "..." }     │            │
│  └────────────────────────────────────────────────────────────┘            │
│                              │                                              │
│                              ▼                                              │
│  POST-PROCESAMIENTO                                                         │
│  ┌────────────────────────────────────────────────────────────┐            │
│  │  1. Guardar PaymentTransaction en DB                       │            │
│  │  2. Actualizar DealerSubscription                          │            │
│  │  3. Publicar evento: payment.completed (RabbitMQ)          │            │
│  │  4. Trigger InvoicingService (NCF + DGII)                  │            │
│  │  5. NotificationService → Email confirmación               │            │
│  │  6. Programar depósito (48-72h)                            │            │
│  │  7. Webhook listener para estado final                     │            │
│  └────────────────────────────────────────────────────────────┘            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1.2 Flujo de Pago PixelPay (12 pasos con webhook)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     FLUJO DE PAGO PIXELPAY                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣  CLIENTE: Seleccionar PixelPay (⭐ comisión más baja)                   │
│      ├─ Ingresar datos de tarjeta                                          │
│      ├─ Seleccionar moneda (DOP/USD/EUR)                                   │
│      ├─ Frontend valida con Luhn                                           │
│      └─ Submit → POST /api/payments/charge                                  │
│                                                                             │
│  2️⃣  FRONTEND: Construir request                                            │
│      {                                                                      │
│        "gateway": "PixelPay",                                               │
│        "amount": 49.00,                                                     │
│        "currency": "USD",                                                   │
│        "cardNumber": "4111111111111111",                                    │
│        "cvv": "123",                                                        │
│        "expiryMonth": "12",                                                 │
│        "expiryYear": "2027",                                                │
│        "customerEmail": "dealer@okla.com.do"                                │
│      }                                                                      │
│                                                                             │
│  3️⃣  BACKEND: PaymentController recibe request                              │
│      ├─ Validar JWT token                                                  │
│      ├─ FluentValidation checks                                            │
│      ├─ MediatR dispatch: ChargeCommand                                    │
│      └─ ChargeHandler ejecuta                                              │
│                                                                             │
│  4️⃣  HANDLER: Obtener proveedor PixelPay                                    │
│      var provider = _factory.GetProvider(PaymentGateway.PixelPay);         │
│      if (!provider.IsAvailable()) throw ProviderUnavailableException;      │
│                                                                             │
│  5️⃣  PIXELPAY PROVIDER: Validar configuración                               │
│      ├─ Verificar ApiKey existe                                            │
│      ├─ Verificar PublicKey existe                                         │
│      ├─ Verificar WebhookSecret existe                                     │
│      └─ Si falta config → throw ConfigurationException                     │
│                                                                             │
│  6️⃣  PIXELPAY PROVIDER: Tokenizar tarjeta                                   │
│      // Tokenización nativa de PixelPay (automática)                       │
│      var cardToken = await _pixelPayClient.TokenizeCardAsync(              │
│        cardNumber, cvv, expiryMonth, expiryYear                            │
│      );                                                                     │
│      // Retorna: "tok_xxxxxxxxxxxx"                                        │
│                                                                             │
│  7️⃣  PIXELPAY PROVIDER: Construir request                                   │
│      {                                                                      │
│        "api_key": "pk_live_xxxxx",                                          │
│        "amount": 4900, // centavos                                          │
│        "currency": "USD",                                                   │
│        "card_token": "tok_xxxxxxxxxxxx",                                    │
│        "customer_email": "dealer@okla.com.do",                              │
│        "order_id": "ORD-20260128-001",                                      │
│        "description": "Suscripción Starter - 1 mes",                        │
│        "webhook_url": "https://api.okla.com.do/webhooks/pixelpay",          │
│        "metadata": {                                                        │
│          "dealerId": "guid",                                                │
│          "subscriptionId": "guid"                                           │
│        }                                                                    │
│      }                                                                      │
│                                                                             │
│  8️⃣  PIXELPAY API: Procesar pago                                            │
│      ├─ Validar token de tarjeta                                           │
│      ├─ Anti-fraude con ML (score 0-100)                                   │
│      ├─ Verificar fondos con banco                                         │
│      ├─ Autorizar cargo                                                    │
│      └─ Response inmediata:                                                 │
│         {                                                                   │
│           "status": "success",                                              │
│           "txn_id": "txn_pixel_abc123",                                     │
│           "auth_code": "AUTH789",                                           │
│           "commission": 0.74, // 1.5% de $49                                │
│           "net_amount": 48.26,                                              │
│           "currency": "USD",                                                │
│           "timestamp": "2026-01-28T10:30:00Z"                               │
│         }                                                                   │
│                                                                             │
│  9️⃣  PIXELPAY PROVIDER: Parsear response                                    │
│      if (response.Status == "success") {                                    │
│        return PaymentResult.Success(                                        │
│          transactionId: response.TxnId,                                     │
│          authCode: response.AuthCode,                                       │
│          netAmount: response.NetAmount  // Importante para reconciliación   │
│        );                                                                   │
│      } else {                                                               │
│        return PaymentResult.Failed(                                         │
│          errorCode: response.ErrorCode,                                     │
│          message: response.ErrorMessage                                     │
│        );                                                                   │
│      }                                                                      │
│                                                                             │
│  🔟  HANDLER: Post-procesamiento                                            │
│      ├─ Guardar PaymentTransaction en DB                                   │
│      │   • TransactionId = "txn_pixel_abc123"                              │
│      │   • Gateway = PixelPay                                              │
│      │   • GrossAmount = 49.00                                             │
│      │   • Commission = 0.74                                               │
│      │   • NetAmount = 48.26                                               │
│      │   • Status = Pending (esperar webhook)                              │
│      ├─ Publicar RabbitMQ: payment.initiated                               │
│      └─ Retornar 200 OK a frontend                                         │
│                                                                             │
│  1️⃣1️⃣  WEBHOOK: PixelPay notifica estado final (async)                      │
│      // 5-30 segundos después                                              │
│      POST https://api.okla.com.do/webhooks/pixelpay                         │
│      {                                                                      │
│        "event": "payment.completed",                                        │
│        "txn_id": "txn_pixel_abc123",                                        │
│        "status": "completed",                                               │
│        "deposited_at": "2026-01-30T08:00:00Z"  // +48h                      │
│      }                                                                      │
│                                                                             │
│      Backend:                                                               │
│      ├─ Validar webhook signature                                          │
│      ├─ Actualizar PaymentTransaction.Status = Completed                   │
│      ├─ Actualizar DealerSubscription.IsActive = true                      │
│      ├─ Trigger InvoicingService → NCF + DGII                              │
│      ├─ NotificationService → Email confirmación                           │
│      └─ Publicar RabbitMQ: payment.completed                               │
│                                                                             │
│  1️⃣2️⃣  FRONTEND: Mostrar resultado                                          │
│      ├─ Polling cada 2s: GET /api/payments/txn_pixel_abc123                │
│      ├─ Cuando status=Completed → Redirect /billing/success                │
│      └─ Mostrar: "Pago procesado - Ahorraste $0.98 vs AZUL" 💰             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Response Codes PixelPay

| Código            | Descripción                        | Acción                  |
| ----------------- | ---------------------------------- | ----------------------- |
| `success`         | Pago aprobado                      | ✅ Continuar            |
| `pending`         | Procesando (esperar webhook)       | ⏳ Polling status       |
| `declined`        | Tarjeta rechazada                  | ❌ Mostrar error        |
| `insufficient`    | Fondos insuficientes               | ❌ Sin fondos           |
| `invalid_card`    | Tarjeta inválida                   | ❌ Número incorrecto    |
| `expired_card`    | Tarjeta expirada                   | ❌ Expirada             |
| `fraud_detected`  | Anti-fraude detectó actividad rara | 🚨 Bloqueado, contactar |
| `network_error`   | Error de red                       | 🔧 Reintentar           |
| `invalid_amount`  | Monto inválido                     | ⚠️ Verificar monto      |
| `currency_error`  | Moneda no soportada                | ⚠️ Usar DOP/USD/EUR     |
| `rate_limit`      | Too many requests                  | ⏱️ Esperar 1 minuto     |
| `api_key_invalid` | API Key incorrecta                 | 🔧 Error de config      |

---

## 3. Configuración

### appsettings.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",
    "PixelPay": {
      "Enabled": true,
      "ApiKey": "pk_live_xxxxx",
      "PublicKey": "pub_xxxxx",
      "WebhookSecret": "whsec_xxxxx",
      "ApiUrl": "https://api.pixelpay.com.do/v2/payments",
      "WebhookUrl": "https://api.okla.com.do/webhooks/pixelpay",
      "Environment": "Production", // Production | Sandbox
      "TimeoutSeconds": 30,
      "SupportedCurrencies": ["DOP", "USD", "EUR"],
      "MaxRetries": 3,
      "EnableAntifraud": true,
      "MinimumAmount": 1.0,
      "MaximumAmount": 50000.0
    }
  }
}
```

---

## 4. Comparación de Costos (Ejemplo Real)

### Dealer con 100 transacciones/mes de $49 c/u

| Proveedor    | Comisión  | Costo/Transacción | Costo Mensual | Ahorro Anual   |
| ------------ | --------- | ----------------- | ------------- | -------------- |
| **AZUL**     | 3.5%      | $1.72             | $172          | -              |
| **CardNET**  | 3.0%      | $1.47             | $147          | $300/año       |
| **PixelPay** | **1.5%**  | **$0.74**         | **$74**       | **$1,176/año** |
| **Ahorro**   | **-2.0%** | **-$0.98**        | **-$98/mes**  | 💰             |

**Conclusión:** PixelPay ahorra hasta **$1,176/año** para dealers con volumen alto.

---

## 5. Webhook Handling

### Endpoint en PaymentService

```csharp
[HttpPost("webhooks/pixelpay")]
[AllowAnonymous]
public async Task<IActionResult> PixelPayWebhook()
{
    // 1. Leer body raw
    var json = await new StreamReader(Request.Body).ReadToEndAsync();

    // 2. Validar signature
    var signature = Request.Headers["X-PixelPay-Signature"];
    if (!_pixelPayService.ValidateSignature(json, signature))
        return Unauthorized();

    // 3. Parsear evento
    var webhookEvent = JsonSerializer.Deserialize<PixelPayWebhookEvent>(json);

    // 4. Procesar según tipo
    switch (webhookEvent.Event)
    {
        case "payment.completed":
            await HandlePaymentCompletedAsync(webhookEvent);
            break;
        case "payment.failed":
            await HandlePaymentFailedAsync(webhookEvent);
            break;
        case "refund.completed":
            await HandleRefundCompletedAsync(webhookEvent);
            break;
    }

    return Ok();
}
```

### Validación de Signature

```csharp
public bool ValidateSignature(string payload, string signature)
{
    var secret = _configuration["PaymentGateway:PixelPay:WebhookSecret"];
    var computedSignature = HMACSHA256(payload, secret);
    return computedSignature == signature;
}
```

---

## 6. Testing

### Request de Ejemplo

```json
POST /api/payments/charge
{
  "gateway": "PixelPay",
  "amount": 129.00,
  "currency": "USD",
  "cardNumber": "4111111111111111",
  "cvv": "123",
  "expiryMonth": "12",
  "expiryYear": "2027",
  "cardholderName": "Ana García",
  "customerEmail": "ana@dealer.com.do",
  "dealerId": "guid",
  "subscriptionId": "guid"
}
```

### Response Success

```json
{
  "success": true,
  "transactionId": "txn_pixel_abc123",
  "authorizationCode": "AUTH789",
  "gateway": "PixelPay",
  "amount": 129.0,
  "commission": 1.94, // 1.5% de $129
  "netAmount": 127.06,
  "currency": "USD",
  "status": "Pending", // Esperar webhook
  "message": "Pago en proceso, recibirás confirmación en segundos",
  "timestamp": "2026-01-28T10:30:00Z",
  "savings": 2.58 // vs AZUL (3.5%)
}
```

---

## 7. Dashboard Analytics

PixelPay ofrece dashboard en tiempo real:

- **Transacciones por día/semana/mes**
- **Tasa de éxito vs rechazo**
- **Gráfico de comisiones pagadas**
- **Top 10 dealers por volumen**
- **Detección de fraude (score)**
- **Velocidad de depósito promedio**

Acceso: https://dashboard.pixelpay.com.do

---

## 📚 Referencias

- [PixelPay Developer Portal](https://developers.pixelpay.com.do/)
- [PixelPay API Docs](https://developers.pixelpay.com.do/api/v2)
- [01-billing-service.md](01-billing-service.md) - Servicio principal
- [03-azul-payment.md](03-azul-payment.md) - AZUL (principal)
- [PaymentService README](../../backend/PaymentService/README.md) - Arquitectura

---

**✅ PixelPay implementado - ⭐ RECOMENDADO PARA VOLUMEN ALTO**  
_Comisiones más bajas (1.5% vs 3.5%), API moderna, ahorro de hasta $1,176/año._
