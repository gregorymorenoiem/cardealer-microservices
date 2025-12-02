# Rate Limiting Service

Servicio de rate limiting distribuido para la plataforma CarDealer. Implementa el algoritmo de ventana deslizante (Sliding Window) utilizando Redis para garantizar límites de tasa consistentes en un entorno distribuido.

## Características

- **Algoritmo Sliding Window**: Implementación precisa de límites de tasa por ventana de tiempo
- **Rate Limiting Distribuido**: Almacenamiento en Redis para consistencia entre instancias
- **Tiers de Usuario**: Límites diferenciados por tipo de usuario (Free, Basic, Premium, Enterprise)
- **Políticas Configurables**: Creación y gestión de políticas personalizadas por endpoint
- **Headers HTTP Estándar**: Respuestas con headers X-RateLimit-* y Retry-After
- **Middleware Reutilizable**: Middleware para integración en otros servicios
- **Estadísticas en Tiempo Real**: Métricas de uso y bloqueos

## Arquitectura

```
RateLimitingService/
├── RateLimitingService.Api/        # API REST y Middleware
│   ├── Controllers/
│   │   └── RateLimitController.cs
│   ├── Middleware/
│   │   └── RateLimitingMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
├── RateLimitingService.Core/       # Lógica de negocio
│   ├── Interfaces/
│   │   └── IRateLimitingService.cs
│   ├── Models/
│   │   ├── RateLimitPolicy.cs
│   │   ├── RateLimitResult.cs
│   │   ├── RateLimitOptions.cs
│   │   └── RateLimitStatistics.cs
│   └── Services/
│       └── RedisRateLimitingService.cs
├── RateLimitingService.Tests/      # Tests unitarios
├── Dockerfile
└── README.md
```

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "RateLimiting": {
    "Enabled": true,
    "DefaultLimit": 100,
    "DefaultWindowSeconds": 60,
    "KeyPrefix": "ratelimit:",
    "ClientIdHeader": "X-API-Key",
    "UserTierHeader": "X-User-Tier",
    "UseIpAsFallback": true,
    "IncludeHeaders": true,
    "ExcludedPaths": ["/health", "/swagger"],
    "Policies": [
      {
        "Name": "API Default",
        "WindowSeconds": 60,
        "MaxRequests": 100,
        "Endpoints": ["/api/*"]
      }
    ]
  }
}
```

## Tiers de Usuario

| Tier       | Requests/min | Descripción                    |
|------------|-------------|--------------------------------|
| Free       | 100         | Usuarios anónimos o gratuitos  |
| Basic      | 500         | Plan básico                    |
| Premium    | 2,000       | Plan premium                   |
| Enterprise | 10,000      | Clientes empresariales         |
| Unlimited  | ∞           | Sin límites                    |

## API Endpoints

### Rate Limit Check

```http
GET /api/ratelimit/check?clientId=xxx&endpoint=/api/test&tier=premium
```

**Respuesta:**
```json
{
  "isAllowed": true,
  "remainingRequests": 1999,
  "limit": 2000,
  "resetAt": "2024-01-15T10:01:00Z",
  "retryAfter": "00:00:00",
  "clientIdentifier": "xxx",
  "policyName": "premium"
}
```

### Gestión de Políticas

```http
# Listar políticas
GET /api/ratelimit/policies

# Obtener política
GET /api/ratelimit/policies/{id}

# Crear política
POST /api/ratelimit/policies
{
  "name": "API Heavy",
  "windowSeconds": 60,
  "maxRequests": 50,
  "endpoints": ["/api/heavy/*"],
  "enabled": true
}

# Actualizar política
PUT /api/ratelimit/policies/{id}

# Eliminar política
DELETE /api/ratelimit/policies/{id}
```

### Estadísticas

```http
# Estadísticas globales
GET /api/ratelimit/statistics?from=2024-01-01&to=2024-01-31

# Uso de cliente
GET /api/ratelimit/clients/{clientId}

# Reset de cliente
DELETE /api/ratelimit/clients/{clientId}
```

### Tiers

```http
GET /api/ratelimit/tiers
```

## Headers de Respuesta

Cuando un request es procesado, se incluyen los siguientes headers:

| Header                | Descripción                              |
|-----------------------|------------------------------------------|
| X-RateLimit-Limit     | Límite máximo de requests                |
| X-RateLimit-Remaining | Requests restantes en la ventana         |
| X-RateLimit-Reset     | Timestamp Unix de reset de la ventana    |
| Retry-After           | Segundos para reintentar (cuando 429)    |

## Uso del Middleware

Para integrar el rate limiting en otros servicios:

```csharp
// Program.cs del servicio que consume
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimiting"));
builder.Services.AddSingleton<IConnectionMultiplexer>(...);
builder.Services.AddScoped<IRateLimitingService, RedisRateLimitingService>();

// En el pipeline
app.UseRateLimiting();
```

## Algoritmo Sliding Window

El servicio utiliza un algoritmo de ventana deslizante implementado con Redis Sorted Sets:

1. Cada request se almacena con su timestamp como score
2. Se eliminan entries fuera de la ventana actual
3. Se cuenta el número de entries en la ventana
4. Si el conteo < límite, se permite y agrega el request
5. Si el conteo >= límite, se rechaza con 429

Esta implementación es **atómica** gracias a un script Lua que se ejecuta en Redis.

## Ejecución

### Local

```bash
cd backend/RateLimitingService/RateLimitingService.Api
dotnet run
```

### Docker

```bash
docker build -t ratelimiting-service .
docker run -p 15097:80 -e ConnectionStrings__Redis=redis:6379 ratelimiting-service
```

## Tests

```bash
cd backend/RateLimitingService
dotnet test
```

## Puerto

- **Desarrollo**: 15097
- **Docker**: 80 (interno)

## Dependencias

- .NET 8.0
- StackExchange.Redis
- Serilog
- ASP.NET Core Health Checks

## Métricas y Monitoreo

El servicio expone:
- `/health` - Health check endpoint
- Logs estructurados con Serilog
- Estadísticas de rate limiting vía API
