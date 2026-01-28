# 💳 CardNET Payment - Pagos con CardNET - Matriz de Procesos

> **Proveedor:** CardNET (Bancaria RD)  
> **Tipo:** Banking  
> **Puerto:** 15105 (PaymentService)  
> **Última actualización:** Enero 28, 2026  
> **Estado:** ✅ IMPLEMENTADO

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 28, 2026)

| Proceso     | Backend | UI Access | Observación                        |
| ----------- | ------- | --------- | ---------------------------------- |
| CARD-PAY-\* | ✅ 100% | ✅ 100%   | `/checkout` (selector de pasarela) |

### Rutas UI Existentes ✅

- ✅ `/checkout` - CheckoutPage (CardNET como opción de backup/alternativa)

---

## 1. Descripción General

**CardNET** es una pasarela bancaria dominicana que actúa como **backup/alternativa** a AZUL. Ofrece comisiones competitivas (2.5%-4.5%) y es ideal para redundancia en caso de fallas en el proveedor principal.

### Características Principales

- **Tipo:** Banking (bancaria tradicional)
- **Comisión:** 2.5% - 4.5%
- **Costo fijo:** RD$5 - 10 por transacción
- **Mensualidad:** US$30 - 50
- **Tokenización:** Sí (solicitar activación)
- **Monedas:** DOP, USD
- **Cobertura:** 🇩🇴 República Dominicana
- **Depósito:** 24-48 horas
- **3D Secure:** Soportado

### Casos de Uso

1. **Backup de AZUL** - Si AZUL falla, automáticamente cambiar a CardNET
2. **A/B Testing** - Comparar tasas de éxito entre bancarias
3. **Diversificación** - No depender de un solo proveedor bancario
4. **Redundancia** - Continuidad del negocio

---

## 1.1 Arquitectura CardNET

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ARQUITECTURA CARDNET PAYMENT                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Frontend (React)                                                           │
│  ┌────────────────────────────────────────────────────────┐                │
│  │  CheckoutPage                                          │                │
│  │  ├─ PaymentMethodSelector (Radio: AZUL/CardNET)       │                │
│  │  ├─ CardNETPaymentForm (si CardNET seleccionado)      │                │
│  │  │   ├─ CardNumber (con validación)                   │                │
│  │  │   ├─ CVV (3-4 dígitos)                             │                │
│  │  │   ├─ ExpiryDate (MM/YY)                            │                │
│  │  │   └─ CardholderName                                │                │
│  │  └─ SubmitButton → POST /api/payments/charge          │                │
│  └────────────────────────────────────────────────────────┘                │
│                              │                                              │
│                              ▼                                              │
│  API Gateway (:18443)                                                       │
│  ┌────────────────────────────────────────────────────────┐                │
│  │  /api/payments/* → PaymentService:15105                │                │
│  └────────────────────────────────────────────────────────┘                │
│                              │                                              │
│                              ▼                                              │
│  PaymentService (:15105)                                                    │
│  ┌────────────────────────────────────────────────────────┐                │
│  │  PaymentController                                     │                │
│  │  ├─ POST /api/payments/charge                          │                │
│  │  │   └─ MediatR → ChargeCommand                        │                │
│  │  └─ ChargeHandler                                      │                │
│  │      ├─ PaymentGatewayFactory.GetProvider(CardNET)     │                │
│  │      ├─ CardNETPaymentProvider.ChargeAsync()           │                │
│  │      └─ Save PaymentTransaction (gateway=CardNET)      │                │
│  └────────────────────────────────────────────────────────┘                │
│                              │                                              │
│                              ▼                                              │
│  CardNETPaymentProvider                                                     │
│  ┌────────────────────────────────────────────────────────┐                │
│  │  ├─ ValidateConfiguration()                            │                │
│  │  ├─ ValidateRequest(CardNumber, CVV, Amount)           │                │
│  │  ├─ BuildCardNETRequest()                              │                │
│  │  │   • MerchantId                                      │                │
│  │  │   • TerminalId                                      │                │
│  │  │   • CardData (encrypted)                            │                │
│  │  │   • Amount                                          │                │
│  │  │   • OrderId                                         │                │
│  │  ├─ HTTP POST → CardNET API                            │                │
│  │  ├─ ParseCardNETResponse()                             │                │
│  │  └─ MapToPaymentResult()                               │                │
│  └────────────────────────────────────────────────────────┘                │
│                              │                                              │
│                              ▼                                              │
│  🏦 CardNET API (External)                                                  │
│  ┌────────────────────────────────────────────────────────┐                │
│  │  • Validación de tarjeta                               │                │
│  │  • Verificación de fondos                              │                │
│  │  • Autorización de cargo                               │                │
│  │  • Tokenización (si habilitado)                        │                │
│  │  • 3D Secure (si requerido)                            │                │
│  │  • Response Code (00=Success, otros=Error)             │                │
│  └────────────────────────────────────────────────────────┘                │
│                              │                                              │
│                              ▼                                              │
│  POST-PROCESAMIENTO                                                         │
│  ┌────────────────────────────────────────────────────────┐                │
│  │  1. Guardar PaymentTransaction en DB                   │                │
│  │  2. Actualizar DealerSubscription (si aplica)          │                │
│  │  3. Publicar evento: payment.completed (RabbitMQ)      │                │
│  │  4. Trigger InvoicingService (NCF + DGII)              │                │
│  │  5. Enviar email confirmación (NotificationService)    │                │
│  │  6. Programar depósito (24-48h)                        │                │
│  └────────────────────────────────────────────────────────┘                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1.2 Flujo de Pago CardNET (10 pasos)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     FLUJO DE PAGO CARDNET                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1️⃣  CLIENTE: Seleccionar CardNET como método de pago                       │
│      ├─ Ingresar datos de tarjeta (CardNumber, CVV, Expiry)                │
│      ├─ Frontend valida formato (Luhn algorithm)                           │
│      └─ Submit → POST /api/payments/charge                                  │
│                                                                             │
│  2️⃣  FRONTEND: Construir request                                            │
│      {                                                                      │
│        "gateway": "CardNET",                                                │
│        "amount": 103.00,                                                    │
│        "currency": "USD",                                                   │
│        "cardNumber": "4111111111111111",                                    │
│        "cvv": "123",                                                        │
│        "expiryMonth": "12",                                                 │
│        "expiryYear": "2027"                                                 │
│      }                                                                      │
│                                                                             │
│  3️⃣  BACKEND: PaymentController recibe request                              │
│      ├─ Validar JWT token                                                  │
│      ├─ Validar DTO con FluentValidation                                   │
│      ├─ MediatR dispatch: ChargeCommand                                    │
│      └─ ChargeHandler ejecuta lógica                                       │
│                                                                             │
│  4️⃣  HANDLER: Obtener proveedor CardNET                                     │
│      var provider = _factory.GetProvider(PaymentGateway.CardNET);          │
│      if (!provider.IsAvailable()) throw ProviderUnavailableException;      │
│                                                                             │
│  5️⃣  CARDNET PROVIDER: Validar configuración                                │
│      ├─ Verificar MerchantId existe                                        │
│      ├─ Verificar TerminalId existe                                        │
│      ├─ Verificar API Key existe                                           │
│      └─ Si falta config → throw ConfigurationException                     │
│                                                                             │
│  6️⃣  CARDNET PROVIDER: Construir request CardNET                            │
│      {                                                                      │
│        "merchantId": "xxxxx",                                               │
│        "terminalId": "12345",                                               │
│        "orderId": "ORD-20260128-001",                                       │
│        "amount": "10300", // en centavos                                    │
│        "currency": "USD",                                                   │
│        "cardData": {                                                        │
│          "number": "4111111111111111",                                      │
│          "cvv": "123",                                                      │
│          "expiryDate": "1227"                                               │
│        }                                                                    │
│      }                                                                      │
│                                                                             │
│  7️⃣  CARDNET API: Procesar pago                                             │
│      ├─ Validar tarjeta (BIN lookup)                                       │
│      ├─ Verificar fondos con banco emisor                                  │
│      ├─ Ejecutar 3D Secure si aplica                                       │
│      ├─ Autorizar cargo                                                    │
│      └─ Response:                                                           │
│         {                                                                   │
│           "responseCode": "00",  // 00=Success                              │
│           "authorizationCode": "AUTH123456",                                │
│           "transactionId": "TXN789012",                                     │
│           "message": "Transacción aprobada"                                 │
│         }                                                                   │
│                                                                             │
│  8️⃣  CARDNET PROVIDER: Parsear response                                     │
│      if (response.ResponseCode == "00") {                                   │
│        return PaymentResult.Success(                                        │
│          transactionId: response.TransactionId,                             │
│          authCode: response.AuthorizationCode                               │
│        );                                                                   │
│      } else {                                                               │
│        return PaymentResult.Failed(                                         │
│          errorCode: response.ResponseCode,                                  │
│          message: GetErrorMessage(response.ResponseCode)                    │
│        );                                                                   │
│      }                                                                      │
│                                                                             │
│  9️⃣  HANDLER: Post-procesamiento                                            │
│      ├─ Guardar PaymentTransaction en DB (status=Completed)                │
│      ├─ Actualizar DealerSubscription.IsActive = true                      │
│      ├─ Publicar RabbitMQ: payment.completed                               │
│      ├─ Trigger InvoicingService → Generar factura con NCF                 │
│      ├─ NotificationService → Email confirmación                           │
│      └─ Programar depósito bancario (24-48h)                               │
│                                                                             │
│  🔟  FRONTEND: Mostrar resultado                                            │
│      ├─ Si Success: Redirect a /billing/success?txn=TXN789012              │
│      ├─ Si Failed: Mostrar error + opción de reintentar                    │
│      └─ Sugerir proveedor alternativo (AZUL) si CardNET falla repetidas    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Response Codes CardNET

| Código | Descripción          | Acción                          |
| ------ | -------------------- | ------------------------------- |
| `00`   | Transacción aprobada | ✅ Continuar                    |
| `01`   | Fondos insuficientes | ❌ Mostrar error, permitir otro |
| `03`   | Comercio inválido    | 🔧 Error de config, contactar   |
| `05`   | Transacción denegada | ❌ Tarjeta rechazada            |
| `12`   | Transacción inválida | ⚠️ Verificar datos              |
| `14`   | Tarjeta inválida     | ❌ Número incorrecto            |
| `30`   | Error de formato     | 🔧 Error de integración         |
| `51`   | Fondos insuficientes | ❌ Sin fondos                   |
| `54`   | Tarjeta expirada     | ❌ Expirada                     |
| `55`   | CVV incorrecto       | ⚠️ Reintentar                   |
| `91`   | Emisor no disponible | ⏳ Reintentar más tarde         |
| `96`   | Error del sistema    | 🔧 Problema técnico             |
| `XX`   | Otros códigos        | 🔍 Consultar documentación      |

---

## 3. Configuración

### appsettings.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",
    "CardNET": {
      "Enabled": true,
      "MerchantId": "xxxxx",
      "TerminalId": "12345",
      "ApiKey": "your-api-key",
      "ApiUrl": "https://api.cardnet.com.do/payment",
      "Environment": "Production", // Production | Sandbox
      "Currency": "USD",
      "TimeoutSeconds": 30,
      "MaxRetries": 3
    }
  }
}
```

### Variables de Entorno (Kubernetes)

```yaml
env:
  - name: PaymentGateway__CardNET__MerchantId
    valueFrom:
      secretKeyRef:
        name: cardnet-credentials
        key: merchant-id
  - name: PaymentGateway__CardNET__ApiKey
    valueFrom:
      secretKeyRef:
        name: cardnet-credentials
        key: api-key
```

---

## 4. Ventajas vs AZUL

| Característica  | CardNET       | AZUL           |
| --------------- | ------------- | -------------- |
| Comisión        | 2.5% - 4.5%   | 2.9% - 4.5%    |
| Depósito        | 24-48h        | 24-48h         |
| Tokenización    | Sí (solicitar | Sí (nativa)    |
| API             | REST          | REST           |
| Documentación   | ⭐⭐⭐        | ⭐⭐⭐⭐       |
| Soporte         | Email/Tel     | Email/Tel/Chat |
| Uptime          | 99.5%         | 99.7%          |
| **Uso en OKLA** | **Backup**    | **Principal**  |

---

## 5. Testing

### Request de Ejemplo (Producción)

```json
POST /api/payments/charge
{
  "gateway": "CardNET",
  "amount": 49.00,
  "currency": "USD",
  "cardNumber": "4111111111111111",
  "cvv": "123",
  "expiryMonth": "12",
  "expiryYear": "2027",
  "cardholderName": "Juan Pérez",
  "dealerId": "guid",
  "subscriptionId": "guid"
}
```

### Response Success

```json
{
  "success": true,
  "transactionId": "TXN-CARD-20260128-001",
  "authorizationCode": "AUTH123456",
  "gateway": "CardNET",
  "amount": 49.0,
  "currency": "USD",
  "status": "Completed",
  "message": "Pago procesado exitosamente",
  "timestamp": "2026-01-28T10:30:00Z"
}
```

---

## 6. Estrategia de Redundancia

**Escenario:** AZUL está caído (timeout o error 500)

```csharp
public async Task<PaymentResult> ChargeWithFallbackAsync(ChargeRequest request)
{
    // 1. Intentar con AZUL (principal)
    var azulProvider = _factory.GetProvider(PaymentGateway.Azul);
    var result = await azulProvider.ChargeAsync(request);

    if (result.Success)
        return result;

    // 2. Si AZUL falla, intentar con CardNET (backup)
    _logger.LogWarning("AZUL falló, intentando con CardNET");
    var cardnetProvider = _factory.GetProvider(PaymentGateway.CardNET);
    result = await cardnetProvider.ChargeAsync(request);

    if (result.Success)
    {
        _logger.LogInformation("CardNET procesó el pago exitosamente");
        return result;
    }

    // 3. Si ambos fallan, error final
    throw new PaymentProcessingException("Todos los proveedores bancarios fallaron");
}
```

---

## 📚 Referencias

- [CardNET Developer Portal](https://www.cardnet.com.do/)
- [01-billing-service.md](01-billing-service.md) - Servicio principal
- [03-azul-payment.md](03-azul-payment.md) - AZUL (principal)
- [PaymentService README](../../backend/PaymentService/README.md) - Arquitectura

---

**✅ CardNET implementado como backup bancario de AZUL**  
_Comisiones competitivas, redundancia garantizada, continuidad del negocio._
