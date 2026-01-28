# 🚀 PaymentService - Roadmap de Implementación

**Estado:** Arquitectura completada ✅  
**Fase Actual:** Integración e Implementación de Proveedores  
**Versión:** 2.0.0  
**Última actualización:** Enero 28, 2026

---

## 📋 Descripción General

Este documento proporciona un roadmap claro para completar la implementación del PaymentService multi-proveedor. La arquitectura está 100% completa, pero requiere implementación de detalles específicos de cada proveedor y integración con otros servicios.

---

## 🎯 FASE 1: Configuración de DI e Integración (PRÓXIMA - 2-3 días)

### 1.1 Actualizar Program.cs

**Archivo:** `PaymentService.Api/Program.cs`

**Cambios requeridos:**

```csharp
// Agregar después de línea ~50 (después de Services.AddDbContext)

// ==================== Payment Gateway Infrastructure ====================

// Register Core Services
builder.Services.AddScoped<IPaymentGatewayRegistry, PaymentGatewayRegistry>();
builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

// Register Individual Providers
builder.Services.AddScoped<AzulPaymentProvider>();
builder.Services.AddScoped<CardNETPaymentProvider>();
builder.Services.AddScoped<PixelPayPaymentProvider>();
builder.Services.AddScoped<FygaroPaymentProvider>();

// Initialize and Register Providers with Registry
var sp = builder.Services.BuildServiceProvider();
var registry = sp.GetRequiredService<IPaymentGatewayRegistry>();

try
{
    registry.RegisterProvider(sp.GetRequiredService<AzulPaymentProvider>());
    registry.RegisterProvider(sp.GetRequiredService<CardNETPaymentProvider>());
    registry.RegisterProvider(sp.GetRequiredService<PixelPayPaymentProvider>());
    registry.RegisterProvider(sp.GetRequiredService<FygaroPaymentProvider>());

    logger.LogInformation("✅ All payment providers registered successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Error registering payment providers");
    throw;
}

// ==================== End Payment Gateway Infrastructure ====================
```

**Tiempo estimado:** 30 minutos

---

### 1.2 Actualizar appsettings.json y appsettings.Development.json

**Archivo:** `PaymentService.Api/appsettings.json`

**Agregar sección completa:**

```json
{
  "PaymentGateway": {
    "Default": "Azul",
    "Azul": {
      "IsEnabled": true,
      "ApiKey": "${AZUL_API_KEY}",
      "MerchantId": "${AZUL_MERCHANT_ID}",
      "BaseUrl": "https://api.azul.com.do",
      "Timeout": 30,
      "RetryCount": 3,
      "SupportedPaymentMethods": ["CreditCard", "DebitCard", "TokenizedCard"],
      "CommissionPercentage": 3.5,
      "FixedCommission": 0,
      "CurrencySupport": ["DOP", "USD"]
    },
    "CardNET": {
      "IsEnabled": false,
      "ApiKey": "${CARDNET_API_KEY}",
      "TerminalId": "${CARDNET_TERMINAL_ID}",
      "BaseUrl": "https://api.cardnet.com.do",
      "Timeout": 30,
      "RetryCount": 3,
      "SupportedPaymentMethods": [
        "CreditCard",
        "DebitCard",
        "ACH",
        "TokenizedCard"
      ],
      "CommissionPercentage": 3.0,
      "FixedCommission": 0,
      "CurrencySupport": ["DOP", "USD"]
    },
    "PixelPay": {
      "IsEnabled": true,
      "PublicKey": "${PIXELPAY_PUBLIC_KEY}",
      "SecretKey": "${PIXELPAY_SECRET_KEY}",
      "BaseUrl": "https://api.pixelpay.com",
      "Timeout": 30,
      "RetryCount": 3,
      "SupportedPaymentMethods": [
        "CreditCard",
        "DebitCard",
        "MobilePayment",
        "EWallet"
      ],
      "CommissionPercentage": 2.5,
      "FixedCommission": 0.15,
      "CurrencySupport": ["DOP", "USD", "EUR"]
    },
    "Fygaro": {
      "IsEnabled": false,
      "ApiKey": "${FYGARO_API_KEY}",
      "MerchantId": "${FYGARO_MERCHANT_ID}",
      "BaseUrl": "https://api.fygaro.com",
      "Timeout": 30,
      "RetryCount": 3,
      "SupportedPaymentMethods": [
        "CreditCard",
        "DebitCard",
        "TokenizedCard",
        "Subscription"
      ],
      "CommissionPercentage": 3.0,
      "FixedCommission": 0,
      "CurrencySupport": ["DOP", "USD"]
    }
  }
}
```

**Tiempo estimado:** 20 minutos

---

### 1.3 Crear Clases de Configuración (Settings)

**Archivos a crear:**

- `PaymentService.Infrastructure/Services/Settings/AzulSettings.cs`
- `PaymentService.Infrastructure/Services/Settings/CardNETSettings.cs`
- `PaymentService.Infrastructure/Services/Settings/PixelPaySettings.cs`
- `PaymentService.Infrastructure/Services/Settings/FygaroSettings.cs`

**Ejemplo (AzulSettings.cs):**

```csharp
namespace PaymentService.Infrastructure.Services.Settings;

public class AzulSettings
{
    public bool IsEnabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.azul.com.do";
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public List<string> SupportedPaymentMethods { get; set; } = new();
    public decimal CommissionPercentage { get; set; }
    public decimal FixedCommission { get; set; }
    public List<string> CurrencySupport { get; set; } = new();

    public bool IsConfigured =>
        !string.IsNullOrEmpty(ApiKey) &&
        !string.IsNullOrEmpty(MerchantId) &&
        IsEnabled;
}
```

**Tiempo estimado:** 45 minutos (4 clases)

---

### 1.4 Actualizar Inyección de Dependencias

**Archivo:** `PaymentService.Api/Program.cs`

**Agregar después de las configuraciones:**

```csharp
// Load Payment Gateway Settings
builder.Services.Configure<AzulSettings>(builder.Configuration.GetSection("PaymentGateway:Azul"));
builder.Services.Configure<CardNETSettings>(builder.Configuration.GetSection("PaymentGateway:CardNET"));
builder.Services.Configure<PixelPaySettings>(builder.Configuration.GetSection("PaymentGateway:PixelPay"));
builder.Services.Configure<FygaroSettings>(builder.Configuration.GetSection("PaymentGateway:Fygaro"));
```

**Tiempo estimado:** 10 minutos

---

**Subtotal FASE 1:** ~2.5 horas

---

## 🔌 FASE 2: Actualizar Controllers (3-4 días)

### 2.1 Modificar ChargeRequestDto

**Archivo:** `PaymentService.Application/DTOs/ChargeRequestDto.cs`

**Agregar propiedad:**

```csharp
public PaymentGateway SelectedGateway { get; set; } = PaymentGateway.Azul;
```

**Tiempo estimado:** 10 minutos

---

### 2.2 Actualizar ChargeRequestValidator

**Archivo:** `PaymentService.Application/Validators/ChargeRequestValidator.cs`

**Agregar validación:**

```csharp
RuleFor(x => x.SelectedGateway)
    .IsInEnum()
    .WithMessage("Invalid payment gateway");
```

**Tiempo estimado:** 10 minutos

---

### 2.3 Actualizar PaymentsController

**Archivo:** `PaymentService.Api/Controllers/PaymentsController.cs`

**Cambiar método Charge:**

```csharp
[HttpPost("charge")]
[Idempotent]
public async Task<ActionResult<ChargeResponseDto>> Charge(
    [FromBody] ChargeRequestDto request,
    CancellationToken cancellationToken)
{
    var command = new ChargeCommand(
        request.UserId,
        request.Amount,
        request.Currency,
        request.Description,
        request.PaymentMethod,
        request.CardNumber,
        request.CardToken,
        request.SelectedGateway  // ← NUEVO
    );

    var result = await _mediator.Send(command, cancellationToken);

    return result.IsSuccess
        ? Ok(result.Value)
        : BadRequest(result.Error);
}
```

**Tiempo estimado:** 30 minutos

---

### 2.4 Actualizar Swagger/OpenAPI Documentation

**Actualizar ejemplos:**

```csharp
// En PaymentsController
[SwaggerParameter("The payment gateway to process the payment through")]
public PaymentGateway SelectedGateway { get; set; }
```

**Tiempo estimado:** 20 minutos

---

**Subtotal FASE 2:** ~1.5 horas

---

## 🎯 FASE 3: Implementar Lógica de Handlers (4-5 días)

### 3.1 Actualizar ChargeCommandHandler

**Archivo:** `PaymentService.Application/Features/Charge/Commands/ChargeCommandHandler.cs`

**Implementar routing a proveedores:**

```csharp
public class ChargeCommandHandler : IRequestHandler<ChargeCommand, Result<ChargeResponseDto>>
{
    private readonly IPaymentGatewayFactory _factory;
    private readonly ILogger<ChargeCommandHandler> _logger;
    private readonly IAzulTransactionRepository _repository;

    public async Task<Result<ChargeResponseDto>> Handle(
        ChargeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Obtener proveedor
            var provider = _factory.CreateProvider(request.Gateway);

            if (!provider.IsConfigured)
            {
                _logger.LogError($"Provider {request.Gateway} is not configured");
                return Result.Failure<ChargeResponseDto>(
                    $"Payment gateway {request.Gateway} is not available");
            }

            // 2. Preparar request para el proveedor
            var paymentRequest = new PaymentGatewayRequest
            {
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                PaymentMethod = request.PaymentMethod,
                CardNumber = request.CardNumber,
                CardToken = request.CardToken,
                UserId = request.UserId
            };

            // 3. Procesar pago
            var paymentResult = await provider.ProcessPaymentAsync(
                paymentRequest,
                cancellationToken);

            // 4. Guardar transacción
            var transaction = new PaymentTransaction
            {
                ProviderTransactionId = paymentResult.TransactionId,
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = paymentResult.IsSuccess ? TransactionStatus.Completed : TransactionStatus.Failed,
                Provider = request.Gateway,
                ResponseCode = paymentResult.ResponseCode,
                ResponseMessage = paymentResult.ResponseMessage
            };

            await _repository.AddAsync(transaction, cancellationToken);

            return Result.Success(new ChargeResponseDto
            {
                TransactionId = transaction.Id,
                Status = transaction.Status,
                Amount = transaction.Amount,
                AuthorizationCode = paymentResult.AuthorizationCode,
                ProviderResponse = paymentResult.ResponseMessage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing charge");
            return Result.Failure<ChargeResponseDto>($"Payment processing failed: {ex.Message}");
        }
    }
}
```

**Tiempo estimado:** 1.5 horas

---

### 3.2 Actualizar RefundCommandHandler

**Similar a ChargeCommandHandler pero llamando a `provider.RefundAsync()`**

**Tiempo estimado:** 1 hora

---

### 3.3 Implementar Lógica de Subscripción

**Archivo:** `PaymentService.Application/Features/Subscription/Commands/CreateSubscriptionCommandHandler.cs`

**Tiempo estimado:** 1.5 horas

---

**Subtotal FASE 3:** ~4 horas

---

## 💾 FASE 4: Migraciones de Base de Datos (2-3 días)

### 4.1 Crear Migración para PaymentTransaction

```bash
cd PaymentService.Api
dotnet ef migrations add AddPaymentTransactionTable -o Infrastructure/Persistence/Migrations
```

**Archivo generado:** `Infrastructure/Persistence/Migrations/xxxxx_AddPaymentTransactionTable.cs`

**Contenido de la migración:**

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "payment_transactions",
        columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            ProviderTransactionId = table.Column<string>(type: "text", nullable: false),
            UserId = table.Column<Guid>(type: "uuid", nullable: false),
            Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
            Currency = table.Column<string>(type: "character varying(3)", nullable: false),
            Status = table.Column<int>(type: "integer", nullable: false),
            Provider = table.Column<int>(type: "integer", nullable: false),
            ResponseCode = table.Column<string>(type: "text", nullable: true),
            ResponseMessage = table.Column<string>(type: "text", nullable: true),
            CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_payment_transactions", x => x.Id);
            table.ForeignKey(
                name: "FK_payment_transactions_AspNetUsers_UserId",
                column: x => x.UserId,
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_payment_transactions_CreatedAt",
        table: "payment_transactions",
        column: "CreatedAt");

    migrationBuilder.CreateIndex(
        name: "IX_payment_transactions_Provider",
        table: "payment_transactions",
        column: "Provider");

    migrationBuilder.CreateIndex(
        name: "IX_payment_transactions_UserId",
        table: "payment_transactions",
        column: "UserId");
}
```

**Ejecutar migración:**

```bash
dotnet ef database update
```

**Tiempo estimado:** 1 hora

---

### 4.2 Crear Índices de Rendimiento

```sql
CREATE INDEX idx_transactions_provider_date
ON payment_transactions(provider, created_at DESC);

CREATE INDEX idx_transactions_user_status
ON payment_transactions(user_id, status);

CREATE UNIQUE INDEX idx_transactions_provider_txn
ON payment_transactions(provider, provider_transaction_id);
```

**Tiempo estimado:** 30 minutos

---

**Subtotal FASE 4:** ~1.5 horas

---

## 🧪 FASE 5: Testing e Integración (5-7 días)

### 5.1 Crear Tests de Providers

**Ubicación:** `PaymentService.Tests/Providers/`

**Archivos a crear:**

- `AzulPaymentProviderTests.cs` - 10-15 tests
- `CardNETPaymentProviderTests.cs` - 10-15 tests
- `PixelPayPaymentProviderTests.cs` - 10-15 tests
- `FygaroPaymentProviderTests.cs` - 10-15 tests

**Ejemplo:**

```csharp
[Fact]
public async Task ProcessPaymentAsync_WithValidRequest_ReturnsSuccessResult()
{
    // Arrange
    var provider = new AzulPaymentProvider(_logger, _options);
    var request = new PaymentGatewayRequest { Amount = 1000 };

    // Act
    var result = await provider.ProcessPaymentAsync(request, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotEmpty(result.TransactionId);
}
```

**Tiempo estimado:** 3 horas

---

### 5.2 Crear Tests de Factory & Registry

**Archivo:** `PaymentService.Tests/PaymentGatewayFactoryTests.cs`

**Tests:**

- Factory crea proveedor correcto
- Factory lanza excepción si proveedor no disponible
- Registry registra y obtiene proveedores correctamente

**Tiempo estimado:** 1.5 horas

---

### 5.3 Crear Tests E2E

**Archivo:** `PaymentService.Tests/E2E/PaymentServiceE2ETests.cs`

**Scenarios:**

- Complete charge flow: Request → Provider → Response → Database
- Refund flow
- Multi-provider fallover

**Tiempo estimado:** 2 horas

---

### 5.4 Ejecutar Suite Completa de Tests

```bash
dotnet test
dotnet test /p:CollectCoverage=true /p:CoverageThreshold=80
```

**Target:** ≥80% code coverage

**Tiempo estimado:** 1 hora

---

**Subtotal FASE 5:** ~7.5 horas

---

## 🔗 FASE 6: Integración con BillingService (3-4 días)

### 6.1 Actualizar BillingService para usar PaymentService

**En BillingService:**

```csharp
// Hacer llamada HTTP a PaymentService
var chargeRequest = new
{
    userId = customerId,
    amount = invoiceAmount,
    currency = "DOP",
    selectedGateway = PaymentGateway.PixelPay
};

var response = await _httpClient.PostAsJsonAsync(
    "https://api.okla.com.do/api/payments/charge",
    chargeRequest);
```

**Tiempo estimado:** 1.5 horas

---

### 6.2 Integración con NotificationService

**Enviar notificaciones sobre resultado de pago:**

```csharp
// Después de pago exitoso
await _notificationService.SendAsync(new PaymentNotification
{
    UserId = request.UserId,
    Type = "PaymentSuccess",
    Amount = request.Amount,
    Gateway = request.Gateway.ToString()
});
```

**Tiempo estimado:** 1 hora

---

### 6.3 Integración con ErrorService

**Registrar errores de pago:**

```csharp
if (!paymentResult.IsSuccess)
{
    await _errorService.LogAsync(new PaymentError
    {
        UserId = request.UserId,
        Gateway = request.Gateway,
        ErrorCode = paymentResult.ResponseCode,
        ErrorMessage = paymentResult.ResponseMessage
    });
}
```

**Tiempo estimado:** 1 hora

---

**Subtotal FASE 6:** ~3.5 horas

---

## 🚀 FASE 7: Deployment a DOKS (2 días)

### 7.1 Actualizar Docker Image

**Archivo:** `PaymentService/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PaymentService/PaymentService.Api/PaymentService.Api.csproj", "PaymentService/PaymentService.Api/"]
# ... rest of COPY commands
RUN dotnet restore "PaymentService/PaymentService.Api/PaymentService.Api.csproj"
COPY . .
RUN dotnet build "PaymentService/PaymentService.Api/PaymentService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService/PaymentService.Api/PaymentService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]
```

**Tiempo estimado:** 30 minutos

---

### 7.2 Crear Kubernetes Manifests

**Archivo:** `k8s/paymentservice-deployment.yaml`

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: paymentservice
  namespace: okla
spec:
  replicas: 3
  selector:
    matchLabels:
      app: paymentservice
  template:
    metadata:
      labels:
        app: paymentservice
    spec:
      containers:
        - name: paymentservice
          image: ghcr.io/gregorymorenoiem/cardealer-paymentservice:latest
          ports:
            - containerPort: 8080
          env:
            - name: AZUL_API_KEY
              valueFrom:
                secretKeyRef:
                  name: payment-secrets
                  key: azul-api-key
          # ... more env vars
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 30
            periodSeconds: 10
```

**Tiempo estimado:** 1 hora

---

### 7.3 Crear Secrets en Kubernetes

```bash
kubectl create secret generic payment-secrets \
  --from-literal=azul-api-key=$AZUL_API_KEY \
  --from-literal=cardnet-api-key=$CARDNET_API_KEY \
  --from-literal=pixelpay-public-key=$PIXELPAY_PUBLIC_KEY \
  --from-literal=pixelpay-secret-key=$PIXELPAY_SECRET_KEY \
  --from-literal=fygaro-api-key=$FYGARO_API_KEY \
  -n okla
```

**Tiempo estimado:** 20 minutos

---

### 7.4 Actualizar Gateway Routing

**Archivo:** `Gateway.Api/ocelot.prod.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "paymentservice", "Port": 8080 }],
      "UpstreamPathTemplate": "/api/payments/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
    }
  ]
}
```

**Tiempo estimado:** 20 minutos

---

**Subtotal FASE 7:** ~2 horas

---

## 📊 TIMELINE TOTAL

| Fase      | Descripción    | Tiempo          | Inicio | Fin   |
| --------- | -------------- | --------------- | ------ | ----- |
| 1         | DI + Config    | 2.5h            | Día 1  | Día 1 |
| 2         | Controllers    | 1.5h            | Día 1  | Día 2 |
| 3         | Handlers       | 4h              | Día 2  | Día 3 |
| 4         | Migraciones BD | 1.5h            | Día 3  | Día 3 |
| 5         | Testing        | 7.5h            | Día 3  | Día 5 |
| 6         | Integración    | 3.5h            | Día 5  | Día 6 |
| 7         | Deployment     | 2h              | Día 6  | Día 6 |
| **TOTAL** |                | **~22.5 horas** |        |       |

**Estimación:** ~3 semanas de trabajo con 1 dev full-time (contando pausas, reuniones, etc.)

---

## ✅ Checklist de Completado

### Antes de cambiar a Producción:

- [ ] Todas las 7 fases completadas
- [ ] Coverage de tests ≥80%
- [ ] Todos los tests pasan (unit, integration, e2e)
- [ ] Documentación actualizada (README, CONFIGURATION, etc.)
- [ ] Secrets configurados en DOKS
- [ ] Gateway routing actualizado y verificado
- [ ] Load testing realizado (mínimo 1000 TPS)
- [ ] Failover testing entre proveedores realizado
- [ ] Security review completado
- [ ] Code review y aprobación del equipo
- [ ] Rollback plan documentado

---

## 🔄 Rollback Plan

**Si algo sale mal en producción:**

```bash
# 1. Revertir imagen Docker
kubectl set image deployment/paymentservice \
  paymentservice=ghcr.io/gregorymorenoiem/cardealer-paymentservice:v1.0.0 \
  -n okla

# 2. Revertir Gateway config
kubectl apply -f k8s/gateway-config-v1.yaml

# 3. Verificar estado
kubectl rollout status deployment/paymentservice -n okla
```

**Tiempo de recuperación:** <5 minutos

---

## 📞 Contacto & Soporte

Para preguntas durante la implementación:

- **Documentación Técnica:** Ver los 6 archivos .md en este directorio
- **Código Existente:** Ver estructuras en PaymentService.Domain/Entities/
- **Ejemplos:** Ver BasePaymentGatewayProvider.cs para patrón de implementación

---

**Estado:** 🟡 EN PROGRESO  
**Próximo paso:** Iniciar FASE 1 - Configuración de DI
