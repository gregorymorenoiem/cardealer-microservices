# 🔑 IdempotencyService

Servicio de idempotencia para prevenir operaciones duplicadas en requests críticos utilizando Redis como almacenamiento distribuido.

## 📋 Descripción

IdempotencyService proporciona una capa de protección contra requests duplicados en operaciones críticas como:
- Creación de órdenes
- Procesamiento de pagos
- Registro de usuarios
- Cualquier operación POST/PUT/PATCH que no deba ejecutarse múltiples veces

## 🏗️ Arquitectura

```
IdempotencyService/
├── IdempotencyService.Core/         # Lógica de negocio
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
│   │   └── IdempotencyController.cs # Endpoints de gestión
│   ├── Middleware/
│   │   └── IdempotencyMiddleware.cs # Middleware automático
│   └── Program.cs                   # Configuración
└── IdempotencyService.Tests/        # Tests unitarios
```

## 🚀 Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/idempotency/{key}` | Obtener registro por clave |
| POST | `/api/idempotency/check` | Verificar estado de clave |
| POST | `/api/idempotency` | Crear registro manualmente |
| DELETE | `/api/idempotency/{key}` | Eliminar registro |
| GET | `/api/idempotency/stats` | Estadísticas de uso |
| POST | `/api/idempotency/cleanup` | Limpieza manual |
| GET | `/health` | Health check |

## 🔧 Uso

### 1. Como Cliente (Header)

Envía el header `X-Idempotency-Key` en tus requests POST/PUT/PATCH:

```bash
curl -X POST http://localhost:15096/api/orders \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: unique-request-id-123" \
  -d '{"product": "Car", "quantity": 1}'
```

### 2. Respuestas Posibles

**Primera ejecución:**
```json
{
  "orderId": "abc123",
  "status": "created"
}
```

**Ejecución duplicada (respuesta cacheada):**
```json
{
  "orderId": "abc123",
  "status": "created"
}
// Header: X-Idempotency-Replayed: true
```

**Conflicto (diferente body, misma key):**
```json
{
  "error": "Idempotency key conflict",
  "message": "Request body differs from the original request"
}
// Status: 409 Conflict
```

**Request en proceso:**
```json
{
  "error": "Request in progress",
  "message": "A request with this idempotency key is currently being processed"
}
// Status: 409 Conflict
```

### 3. Integrar el Middleware en Otros Servicios

```csharp
// Program.cs de otro servicio
builder.Services.Configure<IdempotencyOptions>(
    builder.Configuration.GetSection("Idempotency"));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});
builder.Services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

// ...

app.UseIdempotency(); // Agregar middleware
```

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "redis:6379"
  },
  "Idempotency": {
    "DefaultTtlSeconds": 86400,      // 24 horas
    "MinTtlSeconds": 60,              // 1 minuto
    "MaxTtlSeconds": 604800,          // 7 días
    "HeaderName": "X-Idempotency-Key",
    "RequireIdempotencyKey": false,   // true para forzar
    "ExcludedPaths": [
      "/health",
      "/swagger",
      "/api/idempotency"
    ],
    "IdempotentMethods": ["POST", "PUT", "PATCH"],
    "KeyPrefix": "idempotency:",
    "ValidateRequestHash": true,      // Detectar conflictos
    "ProcessingTimeoutSeconds": 30
  }
}
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
    public string RequestHash { get; set; }   // Hash del body
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

## 🔒 Mejores Prácticas

1. **Generar claves únicas**: Usa UUIDs o combina user_id + timestamp
2. **No reutilizar claves**: Cada operación debe tener una clave única
3. **TTL apropiado**: Configura el TTL según el caso de uso
4. **Manejo de errores**: Si falla el cache, permitir la operación
5. **Logging**: Loguea duplicados para debugging

## 📈 Métricas

El servicio expone estadísticas de uso:

```bash
GET /api/idempotency/stats
```

Respuesta:
```json
{
  "duplicateRequestsBlocked": 150,
  "totalRecords": 1000,
  "processingRecords": 5,
  "completedRecords": 980,
  "failedRecords": 15
}
```

## 🔗 Integración con Otros Servicios

### AuthService
```bash
POST /api/auth/register
X-Idempotency-Key: register-user@email.com-1234567890
```

### NotificationService
```bash
POST /api/notifications/send
X-Idempotency-Key: notification-email-abc123
```

### PaymentService (futuro)
```bash
POST /api/payments/process
X-Idempotency-Key: payment-order-789-attempt-1
```

## 📝 Notas

- Redis maneja automáticamente la expiración de claves
- El middleware es thread-safe y async
- Soporta múltiples instancias del servicio (escalabilidad horizontal)
- Compatible con cualquier cliente HTTP que pueda enviar headers custom

---

**Puerto:** 15096  
**Stack:** ASP.NET Core 8.0, Redis, StackExchange.Redis  
**Tests:** 23 unit tests
