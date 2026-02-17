# RateLimitingService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** RateLimitingService
- **Puerto en Desarrollo:** 5014
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Backend:** Redis
- **Base de Datos:** N/A (usa Redis para contadores)
- **Imagen Docker:** Local only

### Propósito
Servicio centralizado de rate limiting para proteger APIs de abuso. Implementa algoritmos de sliding window y token bucket. En producción, el Gateway (Ocelot) maneja rate limiting directamente.

---

## 🏗️ ARQUITECTURA

```
RateLimitingService/
├── RateLimitingService.Api/
│   ├── Controllers/
│   │   ├── RateLimitController.cs
│   │   └── QuotasController.cs
│   └── Program.cs
├── RateLimitingService.Application/
│   └── Services/
│       ├── RateLimitService.cs
│       └── QuotaManager.cs
├── RateLimitingService.Domain/
│   ├── Entities/
│   │   ├── RateLimitRule.cs
│   │   └── ClientQuota.cs
│   └── Enums/
│       └── LimitAlgorithm.cs
└── RateLimitingService.Infrastructure/
    └── Redis/
        └── RedisRateLimiter.cs
```

---

## 📦 ENTIDADES

### RateLimitRule
```csharp
public class RateLimitRule
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Endpoint { get; set; }           // "/api/vehicles/*"
    public string? IpAddress { get; set; }
    public string? UserId { get; set; }
    public int Limit { get; set; }                  // 100 requests
    public TimeSpan Window { get; set; }            // per 1 minute
    public LimitAlgorithm Algorithm { get; set; }   // SlidingWindow, TokenBucket
    public bool IsActive { get; set; }
}
```

---

## 📡 ENDPOINTS API

#### POST `/api/ratelimit/check`
Verificar si un request está permitido.

**Request:**
```json
{
  "clientId": "user123",
  "endpoint": "/api/vehicles",
  "ipAddress": "192.168.1.1"
}
```

**Response (200 OK):**
```json
{
  "allowed": true,
  "limit": 100,
  "remaining": 85,
  "resetAt": "2026-01-07T11:00:00Z"
}
```

**Response (429 Too Many Requests):**
```json
{
  "allowed": false,
  "limit": 100,
  "remaining": 0,
  "resetAt": "2026-01-07T11:00:00Z",
  "retryAfter": 120
}
```

#### POST `/api/ratelimit/increment`
Incrementar contador de requests.

#### GET `/api/quotas/{clientId}`
Obtener cuotas de un cliente.

---

## 🔧 ALGORITMOS

### Sliding Window
```
Ventana deslizante de tiempo:
- Cuenta requests en últimos N minutos
- Más preciso pero más costoso
```

### Token Bucket
```
Bucket se llena con tokens a rate constante:
- Cada request consume 1 token
- Si bucket vacío, request bloqueado
- Permite bursts cortos
```

### Fixed Window
```
Ventana fija de tiempo:
- Resetea contador al inicio de cada ventana
- Más simple pero menos preciso
```

---

## 📝 REGLAS PREDEFINIDAS

| Endpoint | Límite | Ventana | Tipo |
|----------|--------|---------|------|
| `/api/auth/login` | 5 | 5 min | Por IP |
| `/api/auth/register` | 3 | 1 hora | Por IP |
| `/api/vehicles` (GET) | 100 | 1 min | Por IP |
| `/api/vehicles` (POST) | 10 | 1 min | Por UserId |
| `/api/media/upload` | 20 | 1 hora | Por UserId |

---

## 🚀 EN PRODUCCIÓN

El Gateway (Ocelot) tiene rate limiting built-in:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 20;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});
```

O usando middleware ASP.NET Core:

```csharp
app.UseRateLimiter();
```

---

**Estado:** Solo desarrollo - Gateway maneja rate limiting en prod  
**Versión:** 1.0.0
