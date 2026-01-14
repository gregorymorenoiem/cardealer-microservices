# 🚀 Payment Services Implementation Roadmap

**Para:** AzulPaymentService y StripePaymentService  
**Fecha:** Enero 2026  
**Prioridad:** CRÍTICA (MVP)

---

## 📋 Resumen Ejecutivo

OKLA requiere **dos procesadores de pago** para máxima cobertura:

| Aspecto            | AZUL                                                      | STRIPE                                                        |
| ------------------ | --------------------------------------------------------- | ------------------------------------------------------------- |
| **Prioridad**      | 🔴 ALTA (Mercado local)                                   | 🔴 ALTA (Mercado global)                                      |
| **Timeline**       | Sprint actual                                             | Sprint actual                                                 |
| **Implementación** | Paralela                                                  | Paralela                                                      |
| **Complejidad**    | Media                                                     | Media-Alta                                                    |
| **Story Points**   | 40                                                        | 50                                                            |
| **Documentación**  | ✅ [AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md) | ✅ [STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md) |

---

## 🎯 Fases de Implementación

### Phase 1: Scaffolding (YA COMPLETADO ✅)

**Qué se hizo:**

- ✅ Creada estructura Clean Architecture para ambos servicios
- ✅ `.csproj` files con dependencias necesarias
- ✅ Dockerfiles para producción
- ✅ Agregadas rutas en Gateway (ocelot.json)
- ✅ Agregadas en docker-compose.yaml
- ✅ Documentación completa de APIs

**Archivos creados:**

- ✅ `backend/AzulPaymentService/` (4 capas)
- ✅ `backend/StripePaymentService/` (4 capas)
- ✅ Dockerfile para cada uno
- ✅ `docs/AZUL_API_DOCUMENTATION.md` (620+ líneas)
- ✅ `docs/STRIPE_API_DOCUMENTATION.md` (750+ líneas)
- ✅ `docs/AZUL_vs_STRIPE_COMPARISON.md` (este archivo)

---

### Phase 2: Core Implementation (PRÓXIMA)

#### 2.1 AzulPaymentService

**Deliverables:**

```
AzulPaymentService/
├── AzulPaymentService.Domain/
│   ├── Entities/
│   │   ├── AzulTransaction.cs          ← NEW
│   │   ├── AzulSubscription.cs         ← NEW
│   │   └── AzulWebhookEvent.cs         ← NEW
│   ├── Enums/
│   │   ├── TransactionStatus.cs        ← NEW
│   │   ├── PaymentMethod.cs            ← NEW
│   │   └── SubscriptionFrequency.cs    ← NEW
│   ├── Interfaces/
│   │   ├── IAzulTransactionRepository.cs       ← NEW
│   │   ├── IAzulSubscriptionRepository.cs      ← NEW
│   │   └── IAzulPaymentService.cs              ← NEW
│   └── Exceptions/
│       └── AzulPaymentException.cs     ← NEW
│
├── AzulPaymentService.Application/
│   ├── DTOs/
│   │   ├── ChargeRequestDto.cs         ← NEW
│   │   ├── ChargeResponseDto.cs        ← NEW
│   │   ├── RefundRequestDto.cs         ← NEW
│   │   ├── SubscriptionDto.cs          ← NEW
│   │   └── WebhookEventDto.cs          ← NEW
│   ├── Features/
│   │   ├── Payments/
│   │   │   ├── ChargeCommand.cs        ← NEW
│   │   │   ├── AuthorizeCommand.cs     ← NEW
│   │   │   ├── CaptureCommand.cs       ← NEW
│   │   │   ├── RefundCommand.cs        ← NEW
│   │   │   └── GetTransactionQuery.cs  ← NEW
│   │   └── Subscriptions/
│   │       ├── CreateSubscriptionCommand.cs    ← NEW
│   │       ├── CancelSubscriptionCommand.cs    ← NEW
│   │       └── GetSubscriptionQuery.cs         ← NEW
│   ├── Validators/
│   │   ├── ChargeCommandValidator.cs   ← NEW
│   │   └── RefundCommandValidator.cs   ← NEW
│   └── Services/
│       ├── IAzulAuthenticationService.cs        ← NEW
│       └── IAzulWebhookValidationService.cs     ← NEW
│
├── AzulPaymentService.Infrastructure/
│   ├── Persistence/
│   │   ├── AzulDbContext.cs            ← NEW
│   │   ├── Repositories/
│   │   │   ├── AzulTransactionRepository.cs     ← NEW
│   │   │   └── AzulSubscriptionRepository.cs    ← NEW
│   │   └── Migrations/
│   │       └── 001_InitialCreate.cs    ← NEW
│   ├── External/
│   │   ├── AzulHttpClient.cs           ← NEW (HttpClient wrapper)
│   │   ├── AzulAuthenticationService.cs ← NEW
│   │   └── AzulWebhookValidationService.cs ← NEW
│   └── Configurations/
│       └── AzulSettings.cs             ← NEW
│
└── AzulPaymentService.Api/
    ├── Controllers/
    │   ├── PaymentsController.cs       ← NEW (8 endpoints)
    │   └── SubscriptionsController.cs  ← NEW (4 endpoints)
    ├── Middleware/
    │   └── AzulWebhookMiddleware.cs    ← NEW
    ├── Program.cs                      ← MODIFY
    ├── appsettings.json                ← MODIFY
    ├── appsettings.Development.json    ← NEW
    ├── appsettings.Docker.json         ← NEW
    └── Dockerfile                      ← ALREADY EXISTS
```

**Endpoints a implementar:**

```csharp
// PaymentsController.cs
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    // 1. Crear transacción (Sale)
    [HttpPost("charge")]
    public async Task<IActionResult> ChargeAsync(ChargeRequestDto request)
    // Endpoint: POST /api/payments/charge

    // 2. Pre-autorizar (Authorize)
    [HttpPost("authorize")]
    public async Task<IActionResult> AuthorizeAsync(ChargeRequestDto request)
    // Endpoint: POST /api/payments/authorize

    // 3. Capturar autorización
    [HttpPost("capture/{transactionId}")]
    public async Task<IActionResult> CaptureAsync(string transactionId)
    // Endpoint: POST /api/payments/capture/{id}

    // 4. Anular transacción
    [HttpPost("void/{transactionId}")]
    public async Task<IActionResult> VoidAsync(string transactionId)
    // Endpoint: POST /api/payments/void/{id}

    // 5. Reembolso
    [HttpPost("refund")]
    public async Task<IActionResult> RefundAsync(RefundRequestDto request)
    // Endpoint: POST /api/payments/refund

    // 6. Obtener estado de transacción
    [HttpGet("transactions/{transactionId}")]
    public async Task<IActionResult> GetTransactionAsync(string transactionId)
    // Endpoint: GET /api/payments/transactions/{id}

    // 7. Listar transacciones (admin)
    [HttpGet("transactions")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListTransactionsAsync([FromQuery] FilterDto filter)
    // Endpoint: GET /api/payments/transactions?page=1&pageSize=20

    // 8. Health check
    [HttpGet("health")]
    public IActionResult Health()
    // Endpoint: GET /api/payments/health
}

// SubscriptionsController.cs
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    // 1. Crear suscripción
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(CreateSubscriptionDto request)
    // Endpoint: POST /api/subscriptions

    // 2. Actualizar suscripción
    [HttpPut("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> UpdateAsync(string subscriptionId, UpdateSubscriptionDto request)
    // Endpoint: PUT /api/subscriptions/{id}

    // 3. Cancelar suscripción
    [HttpDelete("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> CancelAsync(string subscriptionId)
    // Endpoint: DELETE /api/subscriptions/{id}

    // 4. Obtener suscripción
    [HttpGet("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync(string subscriptionId)
    // Endpoint: GET /api/subscriptions/{id}
}

// Webhook
[HttpPost("webhooks/azul")]
[AllowAnonymous]
public async Task<IActionResult> HandleWebhookAsync()
// Endpoint: POST /api/webhooks/azul
```

**Rutas en Gateway (ya agregadas):**

```json
{
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "azulpaymentservice", "Port": 8080 }],
  "UpstreamPathTemplate": "/api/azul-payment/{everything}",
  "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
}
```

**Dependencies (.csproj):**

```xml
<!-- Ya incluidas en el .csproj -->
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

<!-- Necesarias agregar -->
<PackageReference Include="Polly" Version="8.2.0" /> <!-- Retry policy -->
<PackageReference Include="Refit" Version="7.0.0" /> <!-- HTTP client gen -->
```

---

#### 2.2 StripePaymentService

**Deliverables:**

```
StripePaymentService/
├── StripePaymentService.Domain/
│   ├── Entities/
│   │   ├── StripePaymentIntent.cs      ← NEW
│   │   ├── StripeCustomer.cs           ← NEW
│   │   ├── StripeSubscription.cs       ← NEW
│   │   └── StripeWebhookEvent.cs       ← NEW
│   ├── Enums/
│   │   ├── PaymentStatus.cs            ← NEW
│   │   ├── SubscriptionStatus.cs       ← NEW
│   │   └── WebhookEventType.cs         ← NEW
│   ├── Interfaces/
│   │   ├── IStripePaymentIntentRepository.cs    ← NEW
│   │   ├── IStripeCustomerRepository.cs         ← NEW
│   │   ├── IStripeSubscriptionRepository.cs     ← NEW
│   │   └── IStripePaymentService.cs             ← NEW
│   └── Exceptions/
│       └── StripePaymentException.cs   ← NEW
│
├── StripePaymentService.Application/
│   ├── DTOs/
│   │   ├── CreatePaymentIntentDto.cs           ← NEW
│   │   ├── ConfirmPaymentIntentDto.cs          ← NEW
│   │   ├── CustomerDto.cs                      ← NEW
│   │   ├── SubscriptionDto.cs                  ← NEW
│   │   ├── RefundRequestDto.cs                 ← NEW
│   │   └── WebhookEventDto.cs                  ← NEW
│   ├── Features/
│   │   ├── PaymentIntents/
│   │   │   ├── CreatePaymentIntentCommand.cs   ← NEW
│   │   │   ├── ConfirmPaymentIntentCommand.cs  ← NEW
│   │   │   ├── CancelPaymentIntentCommand.cs   ← NEW
│   │   │   ├── RefundPaymentCommand.cs         ← NEW
│   │   │   └── GetPaymentIntentQuery.cs        ← NEW
│   │   ├── Customers/
│   │   │   ├── CreateCustomerCommand.cs        ← NEW
│   │   │   ├── UpdateCustomerCommand.cs        ← NEW
│   │   │   └── GetCustomerQuery.cs             ← NEW
│   │   └── Subscriptions/
│   │       ├── CreateSubscriptionCommand.cs    ← NEW
│   │       ├── UpdateSubscriptionCommand.cs    ← NEW
│   │       ├── CancelSubscriptionCommand.cs    ← NEW
│   │       └── GetSubscriptionQuery.cs         ← NEW
│   ├── Validators/
│   │   ├── CreatePaymentIntentValidator.cs     ← NEW
│   │   ├── RefundRequestValidator.cs           ← NEW
│   │   └── CreateSubscriptionValidator.cs      ← NEW
│   └── Services/
│       └── IStripeWebhookValidationService.cs  ← NEW
│
├── StripePaymentService.Infrastructure/
│   ├── Persistence/
│   │   ├── StripeDbContext.cs          ← NEW
│   │   ├── Repositories/
│   │   │   ├── StripePaymentIntentRepository.cs ← NEW
│   │   │   ├── StripeCustomerRepository.cs      ← NEW
│   │   │   └── StripeSubscriptionRepository.cs  ← NEW
│   │   └── Migrations/
│   │       └── 001_InitialCreate.cs    ← NEW
│   ├── External/
│   │   ├── StripeClientService.cs      ← NEW (uses Stripe.net)
│   │   └── StripeWebhookValidationService.cs   ← NEW
│   └── Configurations/
│       └── StripeSettings.cs           ← NEW
│
└── StripePaymentService.Api/
    ├── Controllers/
    │   ├── PaymentIntentsController.cs ← NEW (6 endpoints)
    │   ├── CustomersController.cs      ← NEW (4 endpoints)
    │   ├── SubscriptionsController.cs  ← NEW (5 endpoints)
    │   └── RefundsController.cs        ← NEW (2 endpoints)
    ├── Middleware/
    │   └── StripeWebhookMiddleware.cs  ← NEW
    ├── Program.cs                      ← MODIFY
    ├── appsettings.json                ← MODIFY
    ├── appsettings.Development.json    ← NEW
    ├── appsettings.Docker.json         ← NEW
    └── Dockerfile                      ← ALREADY EXISTS
```

**Endpoints a implementar:**

```csharp
// PaymentIntentsController.cs
[ApiController]
[Route("api/[controller]")]
public class PaymentIntentsController : ControllerBase
{
    // 1. Crear Payment Intent
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreatePaymentIntentDto request)
    // Endpoint: POST /api/paymentintents

    // 2. Confirmar Payment Intent
    [HttpPost("{intentId}/confirm")]
    public async Task<IActionResult> ConfirmAsync(string intentId, ConfirmPaymentIntentDto request)
    // Endpoint: POST /api/paymentintents/{id}/confirm

    // 3. Cancelar Payment Intent
    [HttpPost("{intentId}/cancel")]
    public async Task<IActionResult> CancelAsync(string intentId)
    // Endpoint: POST /api/paymentintents/{id}/cancel

    // 4. Obtener Payment Intent
    [HttpGet("{intentId}")]
    public async Task<IActionResult> GetAsync(string intentId)
    // Endpoint: GET /api/paymentintents/{id}

    // 5. Listar Payment Intents
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListAsync([FromQuery] FilterDto filter)
    // Endpoint: GET /api/paymentintents?page=1

    // 6. Health check
    [HttpGet("health")]
    public IActionResult Health()
    // Endpoint: GET /api/paymentintents/health
}

// CustomersController.cs
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    // 1. Crear cliente
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(CreateCustomerDto request)
    // Endpoint: POST /api/customers

    // 2. Obtener cliente
    [HttpGet("{customerId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync(string customerId)
    // Endpoint: GET /api/customers/{id}

    // 3. Actualizar cliente
    [HttpPut("{customerId}")]
    [Authorize]
    public async Task<IActionResult> UpdateAsync(string customerId, UpdateCustomerDto request)
    // Endpoint: PUT /api/customers/{id}

    // 4. Eliminar cliente
    [HttpDelete("{customerId}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync(string customerId)
    // Endpoint: DELETE /api/customers/{id}
}

// SubscriptionsController.cs
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    // 1. Crear suscripción
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(CreateSubscriptionDto request)
    // Endpoint: POST /api/subscriptions

    // 2. Obtener suscripción
    [HttpGet("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync(string subscriptionId)
    // Endpoint: GET /api/subscriptions/{id}

    // 3. Actualizar suscripción
    [HttpPut("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> UpdateAsync(string subscriptionId, UpdateSubscriptionDto request)
    // Endpoint: PUT /api/subscriptions/{id}

    // 4. Cancelar suscripción
    [HttpDelete("{subscriptionId}")]
    [Authorize]
    public async Task<IActionResult> CancelAsync(string subscriptionId)
    // Endpoint: DELETE /api/subscriptions/{id}

    // 5. Pausar suscripción
    [HttpPost("{subscriptionId}/pause")]
    [Authorize]
    public async Task<IActionResult> PauseAsync(string subscriptionId)
    // Endpoint: POST /api/subscriptions/{id}/pause
}

// RefundsController.cs
[ApiController]
[Route("api/[controller]")]
public class RefundsController : ControllerBase
{
    // 1. Crear reembolso
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(RefundRequestDto request)
    // Endpoint: POST /api/refunds

    // 2. Obtener reembolso
    [HttpGet("{refundId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync(string refundId)
    // Endpoint: GET /api/refunds/{id}
}

// Webhooks
[HttpPost("webhooks/stripe")]
[AllowAnonymous]
public async Task<IActionResult> HandleWebhookAsync()
// Endpoint: POST /api/webhooks/stripe
```

**Rutas en Gateway (ya agregadas):**

```json
{
  "DownstreamPathTemplate": "/api/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{ "Host": "stripepaymentservice", "Port": 8080 }],
  "UpstreamPathTemplate": "/api/stripe-payment/{everything}",
  "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
}
```

**Dependencies (.csproj):**

```xml
<!-- Ya incluidas en el .csproj -->
<PackageReference Include="Stripe.net" Version="42.12.0" />
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="FluentValidation" Version="11.8.0" />
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

<!-- Necesarias agregar -->
<PackageReference Include="Polly" Version="8.2.0" /> <!-- Retry policy -->
```

---

### Phase 3: Testing & Integration (DESPUÉS)

**Para ambos servicios:**

```
✓ Crear proyectos .Tests
✓ Mínimo 10-15 tests por servicio
✓ Test endpoints de pago (success, decline, error)
✓ Test webhooks (validación de signatures)
✓ Test manejo de excepciones
✓ Test integración con DB
✓ Test configuración de DI
✓ Coverage mínimo: 80%
```

**Comando de ejecución:**

```bash
# Test AZUL
dotnet test backend/AzulPaymentService/AzulPaymentService.Tests/AzulPaymentService.Tests.csproj

# Test STRIPE
dotnet test backend/StripePaymentService/StripePaymentService.Tests/StripePaymentService.Tests.csproj

# Con coverage
dotnet test --logger "console;verbosity=detailed" /p:CollectCoverage=true
```

---

### Phase 4: Deployment & Integration (FINAL)

```
✓ Docker build para ambos servicios
✓ docker-compose up funcional
✓ Health checks responden correctamente
✓ Gateway rutea correctamente
✓ Webhooks en producción
✓ Integración con BillingService
✓ Testing E2E en DOKS
```

---

## 🔐 Configuración de Secretos

### Desarrollo (Sandbox)

**Para AZUL:**

```yaml
# appsettings.Development.json
"AzulSettings":
  {
    "ApiBaseUrl": "https://api.azul.com.do/api/1.0",
    "StoreId": "SANDBOX_STORE_ID",
    "ApiKey": "SANDBOX_API_KEY",
    "WebhookSecret": "SANDBOX_WEBHOOK_SECRET",
  }
```

**Para STRIPE:**

```yaml
# appsettings.Development.json
"StripeSettings":
  {
    "ApiKey": "sk_test_xxxx...",
    "PublishableKey": "pk_test_xxxx...",
    "WebhookSecret": "whsec_test_xxxx...",
  }
```

### Producción (Live)

**Docker Secrets o Environment Variables:**

```bash
# AZUL
export AZUL_STORE_ID="prod_store_id"
export AZUL_API_KEY="prod_api_key"
export AZUL_WEBHOOK_SECRET="prod_webhook_secret"

# STRIPE
export STRIPE_API_KEY="sk_live_xxxx..."
export STRIPE_WEBHOOK_SECRET="whsec_live_xxxx..."
```

---

## 📊 Métricas de Éxito

### Por Fase

| Fase    | Métrica                   | Meta           | Status         |
| ------- | ------------------------- | -------------- | -------------- |
| Phase 1 | Scaffolding completo      | 100%           | ✅ DONE        |
| Phase 2 | Controllers implementados | 100%           | 🔄 IN PROGRESS |
| Phase 2 | Tests unitarios           | >80% coverage  | 🔄 IN PROGRESS |
| Phase 3 | Compilation exitosa       | 0 warnings     | ⏳ PENDING     |
| Phase 4 | Docker builds             | 2/2 successful | ⏳ PENDING     |
| Phase 4 | E2E tests                 | All passing    | ⏳ PENDING     |

### Por Servicio

**AzulPaymentService:**

- Controllers: 2 (Payments, Subscriptions)
- Endpoints: 12 total (8 + 4)
- Tests: 15 mínimo
- LOC: ~3,000

**StripePaymentService:**

- Controllers: 4 (PaymentIntents, Customers, Subscriptions, Refunds)
- Endpoints: 17 total (6 + 4 + 5 + 2)
- Tests: 20 mínimo
- LOC: ~3,500

---

## 🎯 Timeline Estimado

```
JANUARY 2026
├─ Week 1 (Jan 1-7)
│  ├─ ✅ Phase 1: Scaffolding (DONE)
│  ├─ ✅ Phase 1: Documentation (DONE)
│  └─ ✅ Phase 1: Gateway routes (DONE)
│
├─ Week 2 (Jan 8-14)
│  ├─ 🔄 Phase 2: AZUL Controllers (IN PROGRESS)
│  ├─ 🔄 Phase 2: STRIPE Controllers (IN PROGRESS)
│  └─ ⏳ Phase 2: Unit tests
│
├─ Week 3 (Jan 15-21)
│  ├─ ⏳ Phase 3: Integration tests
│  ├─ ⏳ Phase 3: Docker testing
│  └─ ⏳ Phase 3: Gateway validation
│
└─ Week 4 (Jan 22-28)
   ├─ ⏳ Phase 4: Production deployment
   ├─ ⏳ Phase 4: Webhook testing (LIVE)
   └─ ⏳ Phase 4: BillingService integration
```

---

## ✅ Checklist Completo

### Before Starting Implementation

- [ ] Entender diferencias AZUL vs STRIPE (leer [AZUL_vs_STRIPE_COMPARISON.md](AZUL_vs_STRIPE_COMPARISON.md))
- [ ] Revisar [AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md) completamente
- [ ] Revisar [STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md) completamente
- [ ] Obtener Sandbox credentials de ambos proveedores
- [ ] Leer ejemplos de código C# en documentaciones
- [ ] Instalar Stripe.net NuGet (StripePaymentService)
- [ ] Preparar appsettings.json para ambos

### AzulPaymentService Implementation

- [ ] Crear entidades en Domain layer
- [ ] Crear DTOs en Application layer
- [ ] Crear Commands/Queries en Application
- [ ] Implementar DbContext en Infrastructure
- [ ] Implementar Repositories en Infrastructure
- [ ] Implementar AzulHttpClient en Infrastructure
- [ ] Implementar Controllers en Api layer
- [ ] Crear Program.cs con DI
- [ ] Implementar webhooks handler
- [ ] Crear 15+ unit tests
- [ ] Compilar sin errores/warnings
- [ ] Docker build exitosa
- [ ] API tests con Postman/curl

### StripePaymentService Implementation

- [ ] Crear entidades en Domain layer
- [ ] Crear DTOs en Application layer
- [ ] Crear Commands/Queries en Application
- [ ] Implementar DbContext en Infrastructure
- [ ] Implementar Repositories en Infrastructure
- [ ] Implementar StripeClientService en Infrastructure
- [ ] Implementar PaymentIntentsController
- [ ] Implementar CustomersController
- [ ] Implementar SubscriptionsController
- [ ] Implementar RefundsController
- [ ] Crear Program.cs con DI
- [ ] Implementar webhooks handler
- [ ] Crear 20+ unit tests
- [ ] Compilar sin errores/warnings
- [ ] Docker build exitosa
- [ ] API tests con Postman/curl

### Integration & Deployment

- [ ] Gateway detecta ambos servicios
- [ ] Health checks funcionan: GET /health
- [ ] CORS configurado correctamente
- [ ] Webhooks reciben eventos del provider
- [ ] JWT auth funciona en endpoints protegidos
- [ ] Base de datos migra correctamente
- [ ] Tests E2E en sandbox
- [ ] Deploy a DOKS exitoso
- [ ] Tests E2E en producción (limited)
- [ ] Documentación actualizada
- [ ] Monitoreo configurado

---

## 📚 Documentación de Referencia

1. **[AZUL_API_DOCUMENTATION.md](AZUL_API_DOCUMENTATION.md)** - 620+ líneas

   - Todos los endpoints de AZUL
   - Ejemplos de requests/responses
   - Código C# completo
   - Webhook validation
   - Error codes

2. **[STRIPE_API_DOCUMENTATION.md](STRIPE_API_DOCUMENTATION.md)** - 750+ líneas

   - Todos los endpoints de Stripe
   - Payment Intents flow
   - Customers y Subscriptions
   - Código C# con Stripe.net
   - Webhook validation
   - Test card numbers

3. **[AZUL_vs_STRIPE_COMPARISON.md](AZUL_vs_STRIPE_COMPARISON.md)** - Este documento
   - Comparativa directa
   - Cuándo usar cada una
   - Pricing analysis
   - Estrategia híbrida

---

## 🚀 Próximo Paso

**Pregunta:** ¿Empiezo la **Phase 2** ahora?

```
Opciones:
A) Implementar AZUL primero (más simple, mercado local)
B) Implementar STRIPE primero (más potente, mercado global)
C) Ambas en paralelo (máximo paralelismo)

Recomendación: C) Ambas en paralelo
- Ambas tienen ~3K LOC
- No hay dependencias entre ellas
- Máximo paralelismo de desarrollo
- Ambas CRÍTICAS para MVP
```

**¿Confirmamos empezar Phase 2?**
