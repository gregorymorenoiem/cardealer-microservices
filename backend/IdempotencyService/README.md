# 🔑 IdempotencyService

Servicio de idempotencia para prevenir operaciones duplicadas en requests críticos utilizando Redis como almacenamiento distribuido. Ahora con **middleware automático basado en atributos**.

## 📋 Descripción

IdempotencyService proporciona una capa de protección contra requests duplicados en operaciones críticas como:
- Creación de órdenes
- Procesamiento de pagos
- Registro de usuarios
- Cualquier operación POST/PUT/PATCH que no deba ejecutarse múltiples veces

## ✨ Características Nuevas

### Middleware Automático Basado en Atributos

Ahora puedes controlar la idempotencia de forma **declarativa** usando atributos:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // ✅ Idempotente - Requiere clave, 1 hora de cache
    [HttpPost]
    [Idempotent(RequireKey = true, TtlSeconds = 3600)]
    public ActionResult<Order> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Tu lógica de negocio
    }

    // ✅ No idempotente - Operación de lectura
    [HttpGet("{id}")]
    [SkipIdempotency]
    public ActionResult<Order> GetOrder(string id)
    {
        // GET no necesita idempotencia
    }

    // ✅ Idempotente con prefijo personalizado
    [HttpPut("{id}")]
    [Idempotent(RequireKey = true, KeyPrefix = "order-update")]
    public ActionResult<Order> UpdateOrder(string id, [FromBody] UpdateOrderRequest request)
    {
        // Actualización idempotente
    }
}
```

### Características del Sistema de Atributos

1. **`[Idempotent]`** - Marca endpoints como idempotentes
   - `RequireKey` - Si es obligatorio el header de idempotencia
   - `TtlSeconds` - Duración del cache (override del default)
   - `HeaderName` - Header personalizado (default: X-Idempotency-Key)
   - `IncludeBodyInHash` - Incluir body en hash de validación
   - `IncludeQueryInHash` - Incluir query params en hash
   - `KeyPrefix` - Prefijo para namespace de keys

2. **`[SkipIdempotency]`** - Excluye endpoints específicos
   - Útil para GET, HEAD, OPTIONS
   - Previene overhead innecesario

3. **Swagger Integration** - Documentación automática
   - Headers documentados en OpenAPI
   - Ejemplos de uso
   - Indicadores visuales (🔒 requerido, 🔓 opcional)

## 🏗️ Arquitectura

```
IdempotencyService/
├── IdempotencyService.Core/         # Lógica de negocio
│   ├── Attributes/                  # ✨ NUEVO
│   │   ├── IdempotentAttribute.cs   # Atributo para marcar endpoints
│   │   └── SkipIdempotencyAttribute.cs
│   ├── Models/                      # Modelos de dominio
│   │   ├── IdempotencyRecord.cs     # Registro de idempotencia
│   │   ├── IdempotencyCheckResult.cs # Resultado de verificación
│   │   └── IdempotencyOptions.cs    # Configuración
│   ├── Interfaces/                  # Contratos
│   │   └── IIdempotencyService.cs   # Servicio principal
│   └── Services/                    # Implementaciones
│       └── RedisIdempotencyService.cs
├── IdempotencyService.Api/          # API REST
│   ├── Controllers/
│   │   ├── IdempotencyController.cs # Endpoints de gestión
│   │   └── OrdersController.cs      # ✨ NUEVO - Ejemplo de uso
│   ├── Extensions/                  # ✨ NUEVO
│   │   └── IdempotencyServiceExtensions.cs # Setup fluido
│   ├── Filters/                     # ✨ NUEVO
│   │   ├── IdempotencyActionFilter.cs # Action filter automático
│   │   └── IdempotencyHeaderOperationFilter.cs # Swagger docs
│   ├── Middleware/
│   │   └── IdempotencyMiddleware.cs # Middleware original (legacy)
│   └── Program.cs                   # Configuración
└── IdempotencyService.Tests/        # Tests unitarios
```

## 🚀 Inicio Rápido

### 1. Configurar el Servicio (Setup de Una Línea)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// ✨ Agregar idempotencia completa con una línea
builder.Services.AddIdempotency(builder.Configuration);

// Configurar Swagger
builder.Services.AddSwaggerGen(options =>
{
    // ✨ Agregar documentación automática de headers
    options.OperationFilter<IdempotencyHeaderOperationFilter>();
});

var app = builder.Build();

// Opcional: Activar middleware legacy
// app.UseIdempotencyMiddleware(options => options.UseMiddleware = true);

app.MapControllers();
app.Run();
```

### 2. Usar en Controladores

```csharp
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    [HttpPost]
    [Idempotent(
        RequireKey = true,           // ❌ Error si falta header
        TtlSeconds = 7200,           // Cache 2 horas
        IncludeQueryInHash = true,   // Validar query params
        KeyPrefix = "payment"        // Namespace: payment:key
    )]
    public async Task<ActionResult<PaymentResult>> ProcessPayment(
        [FromQuery] string orderId,
        [FromBody] PaymentRequest request)
    {
        // Lógica de pago
        return Ok(new PaymentResult { TransactionId = Guid.NewGuid() });
    }
}
```

### 3. Cliente HTTP

```bash
# Primera solicitud
curl -X POST http://localhost:15096/api/payments \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: payment-abc-123" \
  -d '{"amount": 100, "currency": "USD"}'

# ✅ Respuesta: {"transactionId": "xyz789"}

# Solicitud duplicada (misma key)
curl -X POST http://localhost:15096/api/payments \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: payment-abc-123" \
  -d '{"amount": 100, "currency": "USD"}'

# ✅ Respuesta: {"transactionId": "xyz789"}
# ✅ Header: X-Idempotency-Replayed: true
```

## 🔧 Uso Avanzado

### Configuración Granular

```csharp
[HttpPost("process")]
[Idempotent(
    RequireKey = true,              // Header obligatorio
    HeaderName = "X-Request-ID",    // Header personalizado
    TtlSeconds = 3600,              // 1 hora
    IncludeBodyInHash = true,       // Validar body completo
    IncludeQueryInHash = true,      // Validar query string
    KeyPrefix = "payment-process"   // Prefix: payment-process:key
)]
public ActionResult ProcessComplexPayment(...)
```

### Skip Idempotencia

```csharp
[HttpGet]
[SkipIdempotency]  // No aplicar idempotencia a GET
public ActionResult<List<Order>> GetOrders()
{
    // Operaciones de lectura no necesitan idempotencia
}
```

### Cliente .NET (Biblioteca Helper)

```csharp
// Crear cliente idempotente
var httpClient = new HttpClient();
var idempotentClient = httpClient.AsIdempotent();

// POST con clave generada automáticamente
var response = await idempotentClient.PostAsync(
    "/api/orders",
    JsonContent.Create(order),
    idempotencyKey: "order-123"
);

// Verificar si es respuesta cacheada
if (response.IsReplayed())
{
    Console.WriteLine("Esta es una respuesta duplicada");
}
```

## 📊 Endpoints de Gestión

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/idempotency/{key}` | Obtener registro por clave |
| POST | `/api/idempotency/check` | Verificar estado de clave |
| POST | `/api/idempotency` | Crear registro manualmente |
| DELETE | `/api/idempotency/{key}` | Eliminar registro |
| GET | `/api/idempotency/stats` | Estadísticas de uso |
| POST | `/api/idempotency/cleanup` | Limpieza manual |
| GET | `/health` | Health check |

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "redis:6379"
  },
  "Idempotency": {
    "DefaultTtlSeconds": 86400,      // 24 horas (default)
    "MinTtlSeconds": 60,              // 1 minuto mínimo
    "MaxTtlSeconds": 604800,          // 7 días máximo
    "HeaderName": "X-Idempotency-Key",
    "RequireIdempotencyKey": false,   // false = opcional, true = obligatorio
    "ExcludedPaths": [                // Paths excluidos del middleware
      "/health",
      "/swagger",
      "/api/idempotency"
    ],
    "IdempotentMethods": ["POST", "PUT", "PATCH"],
    "KeyPrefix": "idempotency:",
    "ValidateRequestHash": true,      // Detectar conflictos de body
    "ProcessingTimeoutSeconds": 30
  }
}
```

## 🎯 Casos de Uso

### 1. Pagos (Critical)

```csharp
[HttpPost("charge")]
[Idempotent(RequireKey = true, TtlSeconds = 86400, KeyPrefix = "payment")]
public ActionResult ChargeCard(PaymentRequest request) { }
```

### 2. Registro de Usuarios

```csharp
[HttpPost("register")]
[Idempotent(RequireKey = true, TtlSeconds = 3600, IncludeBodyInHash = true)]
public ActionResult RegisterUser(RegisterRequest request) { }
```

### 3. Creación de Órdenes

```csharp
[HttpPost]
[Idempotent(RequireKey = false, TtlSeconds = 7200, KeyPrefix = "order")]
public ActionResult CreateOrder(OrderRequest request) { }
```

### 4. Notificaciones (Evitar duplicados)

```csharp
[HttpPost("send")]
[Idempotent(RequireKey = true, TtlSeconds = 300, IncludeBodyInHash = false)]
public ActionResult SendNotification(NotificationRequest request) { }
```

## 🐳 Docker

```bash
# Build
docker build -t idempotencyservice .

# Run
docker run -p 15096:80 \
  -e ConnectionStrings__Redis=redis:6379 \
  idempotencyservice
```

### Docker Compose

```yaml
idempotencyservice:
  build:
    context: ./IdempotencyService
    dockerfile: Dockerfile
  container_name: idempotencyservice
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ASPNETCORE_URLS: http://+:80
    ConnectionStrings__Redis: "redis:6379"
  ports:
    - "15096:80"
  depends_on:
    - redis
```

## 📊 Modelo de Datos

### IdempotencyRecord

```csharp
public class IdempotencyRecord
{
    public string Key { get; set; }           // Clave única
    public string HttpMethod { get; set; }    // POST, PUT, etc.
    public string Path { get; set; }          // /api/orders
    public string RequestHash { get; set; }   // SHA-256 del body
    public int ResponseStatusCode { get; set; }
    public string ResponseBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public IdempotencyStatus Status { get; set; }
    public string? ClientId { get; set; }     // Multi-tenant
}
```

### Estados

| Estado | Descripción |
|--------|-------------|
| `Processing` | Request en proceso |
| `Completed` | Request completado con éxito |
| `Failed` | Request falló |

## 🧪 Tests

```bash
cd IdempotencyService.Tests
dotnet test
```

**Cobertura:**
- `RedisIdempotencyServiceTests` - 12 tests
- `IdempotencyControllerTests` - 11 tests
- `IdempotencyActionFilterTests` - (TODO)

## 🔒 Mejores Prácticas

1. **Generar claves únicas**: Usa UUIDs o combina user_id + timestamp
   ```
   payment-{userId}-{timestamp}
   order-{orderId}-{attemptNumber}
   ```

2. **No reutilizar claves**: Cada operación debe tener una clave única

3. **TTL apropiado**:
   - Pagos: 24-48 horas
   - Registros: 1-2 horas
   - Notificaciones: 5-30 minutos

4. **RequireKey en operaciones críticas**:
   ```csharp
   [Idempotent(RequireKey = true)]  // ❌ Error si falta header
   ```

5. **IncludeBodyInHash para validación estricta**:
   ```csharp
   [Idempotent(IncludeBodyInHash = true)]  // Detecta cambios en body
   ```

6. **KeyPrefix para namespacing**:
   ```csharp
   [Idempotent(KeyPrefix = "payment")]  // payment:abc-123
   [Idempotent(KeyPrefix = "order")]    // order:abc-123
   ```

## 📈 Métricas y Monitoreo

```bash
GET /api/idempotency/stats
```

Respuesta:
```json
{
  "totalRecords": 1000,
  "processingRecords": 5,
  "completedRecords": 980,
  "failedRecords": 15,
  "duplicateRequestsBlocked": 150,
  "lastUpdated": "2024-01-15T10:30:00Z"
}
```

## 🔗 Integración con Otros Servicios

### Como Biblioteca

```bash
# Instalar desde NuGet (cuando se publique)
dotnet add package IdempotencyService.Core
```

```csharp
// Program.cs del servicio consumidor
builder.Services.AddIdempotency(builder.Configuration);

// Usar en controladores
[Idempotent(RequireKey = true)]
public ActionResult CreateResource(...) { }
```

### Como Servicio HTTP

```bash
# Verificar estado de key
POST http://idempotencyservice:15096/api/idempotency/check
{
  "key": "payment-abc-123",
  "requestHash": "sha256hash..."
}
```

## 🆚 Middleware vs Atributos

| Feature | Middleware Legacy | Atributos (Nuevo) |
|---------|------------------|-------------------|
| Control granular | ❌ | ✅ |
| Configuración por endpoint | ❌ | ✅ |
| Skip específico | ❌ ExcludedPaths | ✅ [SkipIdempotency] |
| TTL personalizado | ❌ | ✅ |
| Documentación Swagger | ❌ | ✅ |
| Performance | ⚠️ Procesa todos | ✅ Solo marcados |
| Uso recomendado | Legacy | ✅ **Recomendado** |

## 📝 Ejemplos Completos

Ver `OrdersController.cs` para ejemplos de:
- ✅ POST con idempotencia requerida
- ✅ PUT con prefijo personalizado
- ✅ PATCH con query hash
- ✅ GET con skip
- ✅ DELETE con validación de body desactivada

---

**Puerto:** 15096  
**Stack:** ASP.NET Core 8.0, Redis, StackExchange.Redis  
**Tests:** 23+ unit tests

## 🎉 Changelog

### v2.0 - Middleware Automático
- ✨ Sistema de atributos declarativo
- ✨ `[Idempotent]` y `[SkipIdempotency]`
- ✨ Action filter automático
- ✨ Integración con Swagger
- ✨ Extension methods para setup fluido
- ✨ Biblioteca cliente HTTP
- ✨ Documentación completa

### v1.0 - Versión Inicial
- ✅ Middleware manual
- ✅ Redis storage
- ✅ REST API
- ✅ Tests unitarios
