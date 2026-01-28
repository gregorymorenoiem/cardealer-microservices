# 💳 PaymentService - Servicio de Pagos Genérico Multi-Proveedor

**Última actualización:** Enero 28, 2026

## 🎯 Descripción General

**PaymentService** es un microservicio genérico que unifica la integración de múltiples pasarelas de pago para OKLA. Proporciona una abstracción única que permite cambiar, agregar o usar múltiples proveedores sin modificar el código de los clientes.

### Antes (Acoplado)

```
Servicio A → AZUL
Servicio B → AZUL
BillingService → AZUL (hardcoded)
```

### Ahora (Desacoplado)

```
Servicio A ──┐
Servicio B ──┼─→ PaymentService (Factory) ──→ [Azul|CardNET|PixelPay|Fygaro]
Billing ─────┘
```

---

## 📊 Resumen de Proveedores (5 total)

| Proveedor    | Tipo       | Comisión     | Monedas       | Cobertura      |
| ------------ | ---------- | ------------ | ------------- | -------------- |
| **AZUL**     | Banking    | 3.5% + $0    | DOP, USD      | 🇩🇴 RD          |
| **CardNET**  | Banking    | 3.0% + $0    | DOP, USD      | 🇩🇴 RD          |
| **PixelPay** | Fintech    | 2.5% + $0.15 | DOP, USD, EUR | 🇩🇴 RD/LAT      |
| **Fygaro**   | Aggregator | 3.0% + $0    | DOP, USD      | 🇩🇴 RD          |
| **PayPal**   | Fintech    | 2.9% + $0.30 | USD, EUR, DOP | 🌎 200+ países |

---

## 📊 Tabla Comparativa de Pasarelas Soportadas

| Pasarela     | Tipo       | Comisión        | Costo Fijo         | Mensualidad | Tokenización  | Caso de Uso                       |
| ------------ | ---------- | --------------- | ------------------ | ----------- | ------------- | --------------------------------- |
| **AZUL**     | Banking    | 2.9% - 4.5%     | RD$5 - 10          | US$30 - 50  | Cybersource   | Primario (Doméstico)              |
| **CardNET**  | Banking    | 2.5% - 4.5%     | RD$5 - 10          | US$30 - 50  | Solicitar     | Backup/Alternativa                |
| **PixelPay** | Fintech    | **1.0% - 3.5%** | **US$0.15 - 0.25** | Varía       | Nativa (API)  | **Volumen alto (✅ RECOMENDADA)** |
| **Fygaro**   | Aggregator | Varía           | Varía              | US$15+      | Suscripciones | Recurrentes/SaaS                  |

### Recomendación Estratégica

- **Volumen bajo/medio:** AZUL (bancaria confiable)
- **Volumen alto:** PixelPay (comisiones más bajas: 1.0%-3.5%)
- **Suscripciones:** Fygaro (módulo optimizado para recurrentes)
- **Redundancia:** CardNET (backup si AZUL falla)

---

## 🏗️ Arquitectura

### Clean Architecture con Múltiples Capas

```
PaymentService/
├── PaymentService.Domain/
│   ├── Entities/
│   │   ├── PaymentTransaction.cs      # Transacción genérica (múltiples proveedores)
│   │   └── ... (otras entidades)
│   ├── Enums/
│   │   ├── PaymentGateway.cs          # Azul, CardNET, PixelPay, Fygaro
│   │   ├── PaymentGatewayType.cs      # Banking, Fintech, Aggregator
│   │   ├── PaymentMethod.cs           # CreditCard, DebitCard, etc.
│   │   └── TransactionStatus.cs
│   └── Interfaces/
│       ├── IPaymentGatewayProvider.cs  # ⭐ Interfaz base de proveedores
│       ├── IPaymentGatewayFactory.cs   # Factory para crear proveedores
│       └── IPaymentGatewayRegistry.cs  # Registry de proveedores
│
├── PaymentService.Application/
│   ├── DTOs/
│   │   ├── ChargeRequestDto.cs
│   │   ├── ChargeResponseDto.cs
│   │   └── ...
│   └── Features/
│       ├── Charge/
│       ├── Refund/
│       └── ...
│
├── PaymentService.Infrastructure/
│   ├── Services/
│   │   ├── PaymentGatewayFactory.cs     # Factory implementation
│   │   ├── PaymentGatewayRegistry.cs    # Registry implementation
│   │   └── Providers/
│   │       ├── BasePaymentGatewayProvider.cs    # Clase base abstracta
│   │       ├── AzulPaymentProvider.cs           # Implementación AZUL
│   │       ├── CardNETPaymentProvider.cs        # Implementación CardNET
│   │       ├── PixelPayPaymentProvider.cs       # Implementación PixelPay
│   │       └── FygaroPaymentProvider.cs         # Implementación Fygaro
│   ├── Persistence/
│   │   ├── DbContext.cs
│   │   └── Repositories/
│   └── ...
│
└── PaymentService.Api/
    ├── Controllers/
    │   ├── PaymentsController.cs
    │   ├── SubscriptionsController.cs
    │   └── WebhooksController.cs
    └── Program.cs
```

### Patrón Factory + Registry + Strategy

```
┌─────────────────────────────────────────────────────────────┐
│                   PaymentGatewayFactory                      │
│  (Crea instancias de proveedores automáticamente)           │
└─────────────────────────────────────────────────────────────┘
              │
              ├──→ GetProvider(PaymentGateway.Azul)
              ├──→ GetProvider(PaymentGateway.CardNET)
              ├──→ GetProvider(PaymentGateway.PixelPay)
              └──→ GetProvider(PaymentGateway.Fygaro)
              │
              ▼
┌─────────────────────────────────────────────────────────────┐
│              PaymentGatewayRegistry                          │
│  (Almacena y gestiona los proveedores registrados)         │
│                                                             │
│  _providers: Dictionary<PaymentGateway, Provider>           │
└─────────────────────────────────────────────────────────────┘
              │
              ├──→ AzulPaymentProvider (implementa IPaymentGatewayProvider)
              ├──→ CardNETPaymentProvider
              ├──→ PixelPayPaymentProvider
              └──→ FygaroPaymentProvider
              │
              ▼
┌─────────────────────────────────────────────────────────────┐
│            IPaymentGatewayProvider (Interfaz)               │
│                                                             │
│  + IsAvailableAsync()                                       │
│  + ValidateConfiguration()                                  │
│  + ChargeAsync(ChargeRequest)                               │
│  + AuthorizeAsync(ChargeRequest)                            │
│  + CaptureAsync(authCode, amount)                           │
│  + RefundAsync(transactionId, amount)                       │
│  + TokenizeCardAsync(cardData)                              │
│  + ChargeTokenAsync(token)                                  │
│  + ValidateWebhook(body, signature)                         │
│  + ProcessWebhookAsync(data)                                │
└─────────────────────────────────────────────────────────────┘
```

---

## 🚀 Cómo Usar

### 1️⃣ Inyectar en Program.cs

```csharp
// En Program.cs
builder.Services.AddScoped<IPaymentGatewayRegistry, PaymentGatewayRegistry>();
builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

// Registrar proveedores
var registry = builder.Services.BuildServiceProvider()
    .GetRequiredService<IPaymentGatewayRegistry>();

registry.Register(new AzulPaymentProvider(logger, configuration, httpClient));
registry.Register(new CardNETPaymentProvider(logger, configuration, httpClient));
registry.Register(new PixelPayPaymentProvider(logger, configuration, httpClient));
registry.Register(new FygaroPaymentProvider(logger, configuration, httpClient));
```

### 2️⃣ Usar en Controllers/Services

```csharp
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGatewayFactory _factory;

    [HttpPost("charge")]
    public async Task<IActionResult> ChargeAsync(
        [FromBody] ChargeRequest request,
        CancellationToken cancellationToken)
    {
        // Opción A: Usar proveedor específico
        var provider = _factory.GetProvider(PaymentGateway.PixelPay);
        var result = await provider.ChargeAsync(request, cancellationToken);

        // Opción B: Usar proveedor por defecto
        var defaultProvider = _factory.GetDefaultProvider();
        var result = await defaultProvider.ChargeAsync(request, cancellationToken);

        // Opción C: Cambiar dinámicamente
        var gateway = DetermineGateway(request.Amount);
        var selectedProvider = _factory.GetProvider(gateway);
        var result = await selectedProvider.ChargeAsync(request, cancellationToken);

        return Ok(result);
    }
}
```

### 3️⃣ Configurar en appsettings.json

```json
{
  "PaymentGateway": {
    "Default": "Azul",

    "Azul": {
      "MerchantId": "your-merchant-id",
      "AuthKey": "your-auth-key",
      "CyberSourceSecretKey": "your-secret-key",
      "Endpoint": "https://api.azul.com/v1"
    },

    "CardNET": {
      "TerminalId": "your-terminal-id",
      "APIKey": "your-api-key",
      "Endpoint": "https://api.cardnet.com/v1"
    },

    "PixelPay": {
      "PublicKey": "pk_live_xxx",
      "SecretKey": "sk_live_xxx",
      "Endpoint": "https://api.pixelpay.com/v1",
      "WebhookSecret": "whsec_xxx"
    },

    "Fygaro": {
      "ApiKey": "api_key_xxx",
      "Endpoint": "https://api.fygaro.com/v1",
      "SubscriptionModuleKey": "sub_key_xxx"
    }
  }
}
```

---

## 📡 API Endpoints

### Proveedores (Multi-Gateway)

| Método | Endpoint                                   | Descripción                              |
| ------ | ------------------------------------------ | ---------------------------------------- |
| `GET`  | `/api/payments/providers`                  | Listar todos los proveedores registrados |
| `GET`  | `/api/payments/providers/{gateway}`        | Información de un proveedor específico   |
| `GET`  | `/api/payments/providers/{gateway}/health` | Estado de salud de un proveedor          |

### Pagos

| Método | Endpoint                        | Descripción                                                    |
| ------ | ------------------------------- | -------------------------------------------------------------- |
| `POST` | `/api/payments/charge`          | Procesar pago/cobro (selecciona gateway con `Gateway` en body) |
| `POST` | `/api/payments/authorize`       | Autorizar sin capturar                                         |
| `POST` | `/api/payments/capture`         | Capturar autorización                                          |
| `POST` | `/api/payments/refund`          | Reembolso (detecta gateway de transacción original)            |
| `GET`  | `/api/payments/{transactionId}` | Obtener detalles de transacción                                |
| `GET`  | `/api/payments/health`          | Health check de todos los proveedores                          |

### Ejemplo de Request con Selección de Gateway

```json
POST /api/payments/charge
{
  "userId": "uuid",
  "amount": 5000.00,
  "currency": "DOP",
  "gateway": "PixelPay",  // ← NUEVO: Selecciona proveedor (opcional, default: Azul)
  "paymentMethod": "CreditCard",
  "cardNumber": "4111111111111111",
  "cardExpiryMonth": "12",
  "cardExpiryYear": "25",
  "cardCVV": "123",
  "cardholderName": "Juan Perez"
}
```

### Respuesta con Información del Proveedor

```json
{
  "transactionId": "uuid",
  "azulTransactionId": "PX-123456", // Prefijo indica proveedor
  "status": "Approved",
  "gateway": "PixelPay", // ← NUEVO
  "providerName": "PixelPay - Fintech", // ← NUEVO
  "commission": 12.65, // ← NUEVO: Comisión calculada
  "commissionPercentage": 2.5, // ← NUEVO
  "netAmount": 4987.35, // ← NUEVO: Monto neto
  "amount": 5000.0,
  "currency": "DOP",
  "isSuccessful": true
}
```

### Webhooks (Multi-Provider)

| Método | Endpoint                 | Descripción                       |
| ------ | ------------------------ | --------------------------------- |
| `POST` | `/api/webhooks/azul`     | Recibir eventos de AZUL           |
| `POST` | `/api/webhooks/cardnet`  | Recibir eventos de CardNET        |
| `POST` | `/api/webhooks/pixelpay` | Recibir eventos de PixelPay       |
| `POST` | `/api/webhooks/fygaro`   | Recibir eventos de Fygaro         |
| `POST` | `/api/webhooks/paypal`   | Recibir eventos de PayPal         |
| `POST` | `/api/webhooks/event`    | Legacy endpoint (redirige a AZUL) |
| `GET`  | `/api/webhooks/health`   | Health check de webhooks          |

### Tokenización

- `POST /api/payments/tokenize` - Tokenizar tarjeta
- `POST /api/payments/charge-token` - Cobrar con token
- `DELETE /api/payments/tokens/{token}` - Remover token

### Suscripciones

- `POST /api/subscriptions` - Crear suscripción
- `PUT /api/subscriptions/{subscriptionId}` - Actualizar
- `DELETE /api/subscriptions/{subscriptionId}` - Cancelar
- `GET /api/subscriptions/{subscriptionId}` - Detalles

---

## 🧪 Testing

### Implementar Test para Nuevo Proveedor

```csharp
[TestClass]
public class NuevoProviderTests
{
    private NuevoPaymentProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        var logger = new Mock<ILogger<BasePaymentGatewayProvider>>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "PaymentGateway:NuevoProvider:ApiKey", "test-key" }
            })
            .Build();
        var httpClient = new HttpClient();

        _provider = new NuevoPaymentProvider(logger.Object, config, httpClient);
    }

    [TestMethod]
    public async Task ChargeAsync_ShouldReturnSuccess()
    {
        var request = new ChargeRequest
        {
            UserId = Guid.NewGuid(),
            Amount = 100m,
            Currency = "DOP"
        };

        var result = await _provider.ChargeAsync(request, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.TransactionId);
    }
}
```

---

## 🔄 Estrategia de Migración

### Fase 1: Implementación (Actual)

- ✅ PaymentService creado con 4 proveedores
- ✅ Interfaces genéricas definidas
- ✅ Factory y Registry implementados

### Fase 2: Integración (Próxima)

- [ ] Actualizar BillingService para usar PaymentService
- [ ] Migrar lógica de AZUL a PaymentService.Azul
- [ ] Tests end-to-end

### Fase 3: Optimización

- [ ] Soporte de fallover automático (si un proveedor falla, intenta otro)
- [ ] Load balancing entre proveedores
- [ ] Dashboard de estadísticas por proveedor

---

## 📈 Monitoreo y Observabilidad

### Métricas Clave

```
# Pagos procesados por proveedor
payment_service_charges_total{gateway="Azul", status="success"} 1250
payment_service_charges_total{gateway="PixelPay", status="success"} 890
payment_service_charges_total{gateway="CardNET", status="failed"} 5

# Tiempo de respuesta por proveedor
payment_service_charge_duration_seconds{gateway="Azul"} 1.2
payment_service_charge_duration_seconds{gateway="PixelPay"} 0.8

# Errores por proveedor
payment_service_errors_total{gateway="Azul", error="timeout"} 2
payment_service_errors_total{gateway="CardNET", error="invalid_config"} 1

# Comisiones totales
payment_service_commissions_total{gateway="PixelPay", currency="DOP"} 15000.00
payment_service_commissions_total{gateway="Azul", currency="DOP"} 12500.00
```

### Logging

```csharp
_logger.LogInformation("Procesando cargo {Gateway} para usuario {UserId}",
    request.Gateway, request.UserId);

_logger.LogError(ex, "Error procesando cargo {Gateway}",
    request.Gateway);
```

---

## 🔐 Seguridad

- ✅ Configuraciones en Secrets de K8s (no en código)
- ✅ Validación de webhooks con firmas
- ✅ Encriptación de tokens de tarjeta
- ✅ PCI DSS compliance (solo números últimos 4 dígitos almacenados)
- ✅ Idempotency keys para prevenir duplicados
- ✅ CORS configurado
- ✅ JWT authentication requerida

---

## 📚 Referencias

- [AZUL API Documentation](https://azul.com/api)
- [CardNET API Documentation](https://cardnet.com/api)
- [PixelPay API Documentation](https://pixelpay.com/api)
- [Fygaro API Documentation](https://fygaro.com/api)
- [PaymentService Source Code](/backend/PaymentService)

---

## 🤝 Contribuir

Para agregar un nuevo proveedor de pago:

1. Crear clase que herede de `BasePaymentGatewayProvider`
2. Implementar todos los métodos abstractos
3. Registrar en `PaymentGatewayRegistry` durante bootstrap
4. Agregar tests unitarios
5. Actualizar esta documentación

### Template para Nuevo Proveedor

```csharp
public class NuevoPaymentProvider : BasePaymentGatewayProvider
{
    public override PaymentGateway Gateway => PaymentGateway.Nuevo;
    public override string Name => "Nuevo Proveedor";
    public override PaymentGatewayType Type => PaymentGatewayType.Fintech;

    public override List<string> ValidateConfiguration() => ValidateBasicConfig("ApiKey");

    public override async Task<PaymentResult> ChargeAsync(
        ChargeRequest request, CancellationToken cancellationToken)
    {
        // Implementación específica
    }

    // ... Otros métodos
}
```

---

**Desarrollado por:** OKLA Team  
**Última actualización:** Enero 28, 2026  
**Versión:** 2.0.0 (Multi-Proveedor)
