# 🚦 Rate Limiting - Control de Tráfico - Matriz de Procesos

> **Componente:** Rate Limiting System  
> **Framework:** AspNetCoreRateLimit + Redis  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema de rate limiting distribuido para proteger la plataforma OKLA contra abuso, ataques DDoS y garantizar fair usage entre usuarios. Implementado en el Gateway con soporte para Redis como store distribuido.

### 1.2 Tipos de Rate Limiting

| Tipo                       | Descripción                    | Uso                         |
| -------------------------- | ------------------------------ | --------------------------- |
| **IP Rate Limiting**       | Límite por dirección IP        | Anti-DDoS, anti-scraping    |
| **Client Rate Limiting**   | Límite por API key             | APIs externas               |
| **User Rate Limiting**     | Límite por usuario autenticado | Fair usage                  |
| **Endpoint Rate Limiting** | Límite por endpoint específico | Proteger endpoints costosos |

### 1.3 Dependencias

| Servicio            | Propósito               |
| ------------------- | ----------------------- |
| Redis               | Store distribuido       |
| AspNetCoreRateLimit | Middleware              |
| Ocelot              | Integration con Gateway |

---

## 2. Configuración

### 2.1 IP Rate Limiting

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Forwarded-For",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "QuotaExceededResponse": {
      "Content": "{ \"error\": \"Rate limit exceeded\", \"retryAfter\": \"{0}\" }",
      "ContentType": "application/json",
      "StatusCode": 429
    },
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1s",
        "Limit": 10
      },
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*",
        "Period": "1h",
        "Limit": 1000
      }
    ]
  }
}
```

### 2.2 Endpoint-Specific Rules

```json
{
  "IpRateLimiting": {
    "GeneralRules": [
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "POST:/api/auth/register",
        "Period": "1h",
        "Limit": 3
      },
      {
        "Endpoint": "POST:/api/auth/forgot-password",
        "Period": "1h",
        "Limit": 3
      },
      {
        "Endpoint": "GET:/api/vehicles",
        "Period": "1s",
        "Limit": 20
      },
      {
        "Endpoint": "GET:/api/vehicles/*",
        "Period": "1s",
        "Limit": 30
      },
      {
        "Endpoint": "POST:/api/vehicles",
        "Period": "1m",
        "Limit": 10
      },
      {
        "Endpoint": "POST:/api/leads",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "POST:/api/media/upload",
        "Period": "1m",
        "Limit": 20
      },
      {
        "Endpoint": "GET:/api/search/*",
        "Period": "1s",
        "Limit": 5
      }
    ]
  }
}
```

### 2.3 Client Rate Limiting (API Keys)

```json
{
  "ClientRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "ClientIdHeader": "X-Api-Key",
    "HttpStatusCode": 429,
    "ClientPolicies": [
      {
        "ClientId": "free-tier",
        "Rules": [
          { "Endpoint": "*", "Period": "1h", "Limit": 100 },
          { "Endpoint": "*", "Period": "1d", "Limit": 1000 }
        ]
      },
      {
        "ClientId": "starter-dealer",
        "Rules": [
          { "Endpoint": "*", "Period": "1h", "Limit": 1000 },
          { "Endpoint": "*", "Period": "1d", "Limit": 10000 }
        ]
      },
      {
        "ClientId": "pro-dealer",
        "Rules": [
          { "Endpoint": "*", "Period": "1h", "Limit": 5000 },
          { "Endpoint": "*", "Period": "1d", "Limit": 50000 }
        ]
      },
      {
        "ClientId": "enterprise-dealer",
        "Rules": [
          { "Endpoint": "*", "Period": "1h", "Limit": 20000 },
          { "Endpoint": "*", "Period": "1d", "Limit": 200000 }
        ]
      }
    ]
  }
}
```

### 2.4 IP Whitelist

```json
{
  "IpRateLimiting": {
    "IpWhitelist": [
      "127.0.0.1",
      "::1",
      "10.0.0.0/8",
      "172.16.0.0/12",
      "192.168.0.0/16"
    ],
    "ClientWhitelist": ["internal-service", "load-balancer", "health-check"],
    "EndpointWhitelist": [
      "/health",
      "/health/live",
      "/health/ready",
      "/metrics"
    ]
  }
}
```

---

## 3. Implementación

### 3.1 Configuración en Gateway

```csharp
// Program.cs
builder.Services.AddMemoryCache();

// Redis distributed cache for rate limiting
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "okla-ratelimit:";
});

// IP Rate Limiting
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(
    builder.Configuration.GetSection("IpRateLimitPolicies"));

// Client Rate Limiting
builder.Services.Configure<ClientRateLimitOptions>(
    builder.Configuration.GetSection("ClientRateLimiting"));
builder.Services.Configure<ClientRateLimitPolicies>(
    builder.Configuration.GetSection("ClientRateLimitPolicies"));

// Use Redis as distributed store
builder.Services.AddDistributedRateLimiting<RedisProcessingStrategy>();
builder.Services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// Middleware order matters!
app.UseIpRateLimiting();
app.UseClientRateLimiting();
```

### 3.2 Custom Rate Limit Counter Store (Redis)

```csharp
public class RedisRateLimitCounterStore : IRateLimitCounterStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimitCounterStore> _logger;

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(id);
    }

    public async Task<RateLimitCounter?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(id);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<RateLimitCounter>(value!);
    }

    public async Task SetAsync(string id, RateLimitCounter? entry, TimeSpan? expirationTime = null,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();

        if (entry.HasValue)
        {
            var value = JsonSerializer.Serialize(entry.Value);
            await db.StringSetAsync(id, value, expirationTime);
        }
        else
        {
            await db.KeyDeleteAsync(id);
        }
    }

    public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(id);
    }
}
```

---

## 4. Rate Limit Headers

### 4.1 Response Headers

```http
HTTP/1.1 200 OK
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1705836600
Retry-After: 60
```

| Header                  | Descripción                                   |
| ----------------------- | --------------------------------------------- |
| `X-RateLimit-Limit`     | Límite total del periodo                      |
| `X-RateLimit-Remaining` | Requests restantes                            |
| `X-RateLimit-Reset`     | Unix timestamp del reset                      |
| `Retry-After`           | Segundos hasta poder reintentar (solo en 429) |

### 4.2 Rate Limit Exceeded Response

```json
{
  "error": "Rate limit exceeded",
  "message": "You have exceeded the 100 requests per minute limit",
  "retryAfter": 45,
  "limit": 100,
  "period": "1m",
  "remaining": 0
}
```

---

## 5. Estrategias de Rate Limiting

### 5.1 Fixed Window

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Fixed Window Algorithm                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Limit: 100 requests per minute                                        │
│                                                                          │
│   Window 1 (10:00-10:01)          Window 2 (10:01-10:02)                │
│   ┌─────────────────────┐         ┌─────────────────────┐               │
│   │ ████████████████    │         │ ██████              │               │
│   │     85 requests     │         │    35 requests      │               │
│   │     ✅ ALLOWED      │         │    ✅ ALLOWED       │               │
│   └─────────────────────┘         └─────────────────────┘               │
│                                                                          │
│   ⚠️ Problem: Burst at window edge (85 + 85 = 170 in ~1 second)        │
│                                                                          │
│   10:00:58  10:00:59  │  10:01:00  10:01:01                             │
│   ████████████████████│████████████████████                              │
│       85 requests     │    85 requests                                   │
│                       ▲                                                  │
│               Window boundary                                            │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Sliding Window Log

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Sliding Window Log Algorithm                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Limit: 100 requests per minute                                        │
│                                                                          │
│   Timestamp log for IP 192.168.1.1:                                     │
│   [10:00:15, 10:00:22, 10:00:35, 10:00:45, ... 10:01:10]                │
│                                                                          │
│   At 10:01:15, check window [10:00:15 - 10:01:15]:                      │
│   - Count timestamps in this range                                      │
│   - If count >= 100 → REJECT                                            │
│   - If count < 100 → ALLOW, add new timestamp                           │
│                                                                          │
│   ✅ More accurate than fixed window                                     │
│   ❌ Higher memory usage (stores all timestamps)                         │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.3 Token Bucket

```
┌─────────────────────────────────────────────────────────────────────────┐
│                       Token Bucket Algorithm                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Bucket capacity: 100 tokens                                           │
│   Refill rate: 10 tokens per second                                     │
│                                                                          │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │                        TOKEN BUCKET                              │  │
│   │                                                                  │  │
│   │   t=0s    t=1s    t=2s    t=3s    t=4s                         │  │
│   │   ┌───┐   ┌───┐   ┌───┐   ┌───┐   ┌───┐                        │  │
│   │   │100│   │ 80│   │ 90│   │ 50│   │ 60│                        │  │
│   │   └───┘   └───┘   └───┘   └───┘   └───┘                        │  │
│   │     │       │       │       │       │                           │  │
│   │     │       │       │       │       │                           │  │
│   │    -0     -30     -0      -50     -0    Requests consumed      │  │
│   │   +10     +10     +10     +10     +10   Tokens refilled        │  │
│   │                                                                  │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│   ✅ Allows bursts up to bucket capacity                                 │
│   ✅ Smooth rate limiting                                                │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Rate Limiting por Caso de Uso

### 6.1 Login Attempts

| Límite      | Periodo  | Bloqueo        |
| ----------- | -------- | -------------- |
| 5 intentos  | 1 minuto | Soft (esperar) |
| 10 intentos | 1 hora   | Hard (CAPTCHA) |
| 20 intentos | 24 horas | Account lock   |

### 6.2 API Endpoints

| Endpoint         | Límite | Periodo   | Justificación             |
| ---------------- | ------ | --------- | ------------------------- |
| `GET /vehicles`  | 20/s   | 1 segundo | Alta demanda, cacheado    |
| `POST /vehicles` | 10/m   | 1 minuto  | Crear vehículo es costoso |
| `GET /search`    | 5/s    | 1 segundo | Búsqueda costosa          |
| `POST /leads`    | 5/m    | 1 minuto  | Prevenir spam             |
| `POST /upload`   | 20/m   | 1 minuto  | Upload costoso            |

### 6.3 User Tiers

| Tier           | Requests/hora | Requests/día |
| -------------- | ------------- | ------------ |
| Anonymous      | 100           | 500          |
| Free User      | 500           | 5,000        |
| Starter Dealer | 1,000         | 10,000       |
| Pro Dealer     | 5,000         | 50,000       |
| Enterprise     | 20,000        | 200,000      |

---

## 7. Bypass y Excepciones

### 7.1 Implementación de Bypass

```csharp
public class RateLimitBypassMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public async Task InvokeAsync(HttpContext context)
    {
        // Bypass for internal services
        if (context.Request.Headers.ContainsKey("X-Internal-Service"))
        {
            var serviceKey = context.Request.Headers["X-Internal-Service"].ToString();
            if (IsValidInternalService(serviceKey))
            {
                context.Items["BypassRateLimit"] = true;
            }
        }

        // Bypass for premium users
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tier = context.User.FindFirst("subscription_tier")?.Value;
            if (tier == "enterprise")
            {
                context.Items["RateLimitMultiplier"] = 10; // 10x higher limits
            }
        }

        await _next(context);
    }
}
```

---

## 8. Monitoreo y Alertas

### 8.1 Métricas

```
# Rate limit hits
ratelimit_requests_total{endpoint="...", status="allowed|blocked"}

# Current counters
ratelimit_current_count{ip="...", endpoint="..."}

# Blocked IPs
ratelimit_blocked_ips_total

# Response times affected by rate limiting
ratelimit_response_time_ms
```

### 8.2 Alertas

```yaml
groups:
  - name: rate-limiting-alerts
    rules:
      - alert: HighRateLimitBlocks
        expr: sum(rate(ratelimit_requests_total{status="blocked"}[5m])) > 100
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High rate of blocked requests"
          description: "More than 100 requests/minute are being blocked"

      - alert: PotentialDDoS
        expr: sum(rate(ratelimit_requests_total{status="blocked"}[1m])) > 1000
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Potential DDoS attack detected"
          description: "Over 1000 requests/minute blocked - possible DDoS"

      - alert: SingleIPBlocked
        expr: ratelimit_blocked_by_ip > 500
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Single IP exceeding limits"
          description: "IP {{ $labels.ip }} has been blocked 500+ times"
```

---

## 9. Best Practices

### 9.1 Recommendations

| Práctica                  | Descripción                         |
| ------------------------- | ----------------------------------- |
| **Graceful degradation**  | Degradar features antes de bloquear |
| **Clear error messages**  | Explicar límites y retry-after      |
| **Tier-based limits**     | Límites según plan del usuario      |
| **Distributed store**     | Redis para multi-instance           |
| **Exclude health checks** | No rate-limit endpoints de health   |
| **Log blocked requests**  | Para análisis y debugging           |

### 9.2 Don'ts

| Anti-patrón           | Problema                    |
| --------------------- | --------------------------- |
| Límites muy estrictos | Afecta UX                   |
| Solo IP-based         | Fácil de bypass con proxies |
| Sin whitelist         | Bloquea servicios internos  |
| Sin logging           | No hay visibilidad          |

---

## 10. Configuración Kubernetes

### 10.1 ConfigMap para Rate Limiting

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: ratelimit-config
  namespace: okla
data:
  IpRateLimiting.json: |
    {
      "IpRateLimiting": {
        "EnableEndpointRateLimiting": true,
        "StackBlockedRequests": false,
        "GeneralRules": [
          { "Endpoint": "*", "Period": "1s", "Limit": 10 },
          { "Endpoint": "*", "Period": "1m", "Limit": 100 }
        ]
      }
    }
```

---

## 📚 Referencias

- [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit) - Documentación
- [02-gateway.md](02-gateway.md) - Configuración de Gateway
- [04-health-checks.md](04-health-checks.md) - Health checks
