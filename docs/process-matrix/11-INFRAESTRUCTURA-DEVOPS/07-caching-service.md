# 🗄️ Caching Service - Sistema de Caché Distribuido - Matriz de Procesos

> **Tecnología:** Redis 7+  
> **Librería:** StackExchange.Redis  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 1. Información General

### 1.1 Descripción

Sistema de caché distribuido basado en Redis que proporciona almacenamiento en memoria de alta velocidad para datos frecuentemente accedidos, reduciendo la carga en las bases de datos y mejorando los tiempos de respuesta.

### 1.2 Casos de Uso

| Caso                 | Descripción                     | TTL      |
| -------------------- | ------------------------------- | -------- |
| **Session Store**    | Sesiones de usuario, tokens     | 24h      |
| **API Cache**        | Respuestas de endpoints         | 5-60min  |
| **Rate Limiting**    | Contadores de rate limit        | 1-60min  |
| **Distributed Lock** | Locks para operaciones críticas | 30s      |
| **Pub/Sub**          | Eventos en tiempo real          | N/A      |
| **Queue**            | Cola de trabajos pendientes     | Variable |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Distributed Cache Architecture                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Microservices                                                         │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐                  │
│   │  Auth    │ │ Vehicles │ │ Billing  │ │   ...    │                  │
│   │ Service  │ │ Service  │ │ Service  │ │          │                  │
│   └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘                  │
│        │            │            │            │                         │
│        └────────────┴────────────┴────────────┘                        │
│                           │                                             │
│                    IDistributedCache                                   │
│                           │                                             │
│                           ▼                                             │
│              ┌─────────────────────────┐                               │
│              │      Redis Cluster      │                               │
│              │                         │                               │
│              │  ┌──────┐  ┌──────┐    │                               │
│              │  │Master│──│Replica│   │                               │
│              │  └──────┘  └──────┘    │                               │
│              │                         │                               │
│              │  Port: 6379            │                               │
│              └─────────────────────────┘                               │
│                           │                                             │
│                           ▼                                             │
│                    Redis Sentinel                                       │
│                   (High Availability)                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Configuración

### 2.1 Connection String

```json
{
  "Redis": {
    "ConnectionString": "redis-master.okla.svc.cluster.local:6379,password=${REDIS_PASSWORD},ssl=False,abortConnect=False,connectTimeout=5000,syncTimeout=5000",
    "InstanceName": "okla:",
    "DefaultDatabase": 0
  }
}
```

### 2.2 Configuración por Servicio

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

// Connection multiplexer singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(configuration);
});
```

### 2.3 Database Segregation

| Database | Uso           | Servicio            |
| -------- | ------------- | ------------------- |
| 0        | General cache | Todos               |
| 1        | Sessions      | AuthService         |
| 2        | Rate Limiting | Gateway             |
| 3        | Search Cache  | VehiclesSaleService |
| 4        | Media Queue   | MediaService        |
| 5        | Notifications | NotificationService |

---

## 3. Patrones de Caché

### 3.1 Cache-Aside Pattern

```csharp
public class VehicleCacheService : IVehicleCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IVehicleRepository _repository;
    private readonly ILogger<VehicleCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(15);

    public async Task<Vehicle?> GetVehicleAsync(Guid vehicleId)
    {
        var cacheKey = $"vehicle:{vehicleId}";

        // 1. Try cache first
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            _logger.LogDebug("Cache HIT for {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<Vehicle>(cached);
        }

        _logger.LogDebug("Cache MISS for {CacheKey}", cacheKey);

        // 2. Get from database
        var vehicle = await _repository.GetByIdAsync(vehicleId);

        if (vehicle != null)
        {
            // 3. Store in cache
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(vehicle),
                options);
        }

        return vehicle;
    }

    public async Task InvalidateVehicleAsync(Guid vehicleId)
    {
        var cacheKey = $"vehicle:{vehicleId}";
        await _cache.RemoveAsync(cacheKey);
        _logger.LogInformation("Cache invalidated for {CacheKey}", cacheKey);
    }
}
```

### 3.2 Write-Through Pattern

```csharp
public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
{
    // 1. Update database first
    var updated = await _repository.UpdateAsync(vehicle);

    // 2. Update cache immediately
    var cacheKey = $"vehicle:{vehicle.Id}";
    await _cache.SetStringAsync(
        cacheKey,
        JsonSerializer.Serialize(updated),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        });

    // 3. Invalidate related caches
    await InvalidateRelatedCachesAsync(vehicle);

    return updated;
}

private async Task InvalidateRelatedCachesAsync(Vehicle vehicle)
{
    var keysToInvalidate = new[]
    {
        $"dealer:{vehicle.DealerId}:vehicles",
        $"search:make:{vehicle.MakeId}",
        $"homepage:featured",
        $"homepage:latest"
    };

    foreach (var key in keysToInvalidate)
    {
        await _cache.RemoveAsync(key);
    }
}
```

### 3.3 Cache Stampede Prevention

```csharp
public class StampedeProtectedCache<T>
{
    private readonly IDistributedCache _cache;
    private readonly IDistributedLockFactory _lockFactory;
    private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public async Task<T?> GetOrCreateAsync(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration)
    {
        // Try cache first
        var cached = await _cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<T>(cached);
        }

        // Acquire distributed lock to prevent stampede
        var lockKey = $"lock:{key}";
        await using var lockHandle = await _lockFactory.AcquireAsync(lockKey, _lockTimeout);

        if (lockHandle == null)
        {
            // Could not acquire lock, wait and retry
            await Task.Delay(100);
            cached = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonSerializer.Deserialize<T>(cached);
            }
            throw new TimeoutException($"Could not acquire cache lock for {key}");
        }

        // Double-check cache after acquiring lock
        cached = await _cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<T>(cached);
        }

        // Generate value
        var value = await factory();

        // Store in cache
        await _cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            });

        return value;
    }
}
```

---

## 4. Cache Keys Convention

### 4.1 Key Structure

```
{service}:{entity}:{id}:{subkey}
```

### 4.2 Key Examples

| Key Pattern                 | Ejemplo                               | Descripción                  |
| --------------------------- | ------------------------------------- | ---------------------------- |
| `vehicle:{id}`              | `vehicle:abc-123`                     | Vehículo individual          |
| `dealer:{id}:vehicles`      | `dealer:dlr-456:vehicles`             | Lista de vehículos de dealer |
| `user:{id}:favorites`       | `user:usr-789:favorites`              | Favoritos de usuario         |
| `search:hash:{hash}`        | `search:hash:a1b2c3`                  | Resultado de búsqueda        |
| `session:{token}`           | `session:jwt-xyz`                     | Sesión de usuario            |
| `ratelimit:{ip}:{endpoint}` | `ratelimit:192.168.1.1:/api/vehicles` | Rate limit counter           |
| `homepage:{section}`        | `homepage:featured`                   | Sección del homepage         |

### 4.3 Key Prefixes por Servicio

| Servicio            | Prefijo                            | Database |
| ------------------- | ---------------------------------- | -------- |
| AuthService         | `auth:`                            | 1        |
| UserService         | `user:`                            | 0        |
| VehiclesSaleService | `vehicle:`, `search:`, `homepage:` | 0, 3     |
| BillingService      | `billing:`, `subscription:`        | 0        |
| MediaService        | `media:`, `upload:`                | 4        |
| Gateway             | `ratelimit:`                       | 2        |

---

## 5. TTL Strategy

### 5.1 TTL por Tipo de Dato

| Tipo                   | TTL      | Sliding | Justificación        |
| ---------------------- | -------- | ------- | -------------------- |
| **Vehicle details**    | 15 min   | 5 min   | Cambia con ediciones |
| **Search results**     | 5 min    | No      | Datos volátiles      |
| **Homepage sections**  | 10 min   | No      | Semi-estático        |
| **User profile**       | 1 hora   | 15 min  | Cambia poco          |
| **Session token**      | 24 horas | 1 hora  | Seguridad            |
| **Rate limit counter** | 1-60 min | No      | Según regla          |
| **Media metadata**     | 1 hora   | 15 min  | Cambia poco          |

### 5.2 Implementación de TTL

```csharp
public static class CacheTtl
{
    public static readonly TimeSpan VehicleDetails = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan SearchResults = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan HomepageSections = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan UserProfile = TimeSpan.FromHours(1);
    public static readonly TimeSpan Session = TimeSpan.FromHours(24);
    public static readonly TimeSpan MediaMetadata = TimeSpan.FromHours(1);

    public static DistributedCacheEntryOptions GetOptions(TimeSpan absoluteExpiration,
        TimeSpan? slidingExpiration = null)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };
    }
}
```

---

## 6. Cache para Búsquedas

### 6.1 Search Cache Key Generator

```csharp
public class SearchCacheKeyGenerator
{
    public string GenerateKey(SearchVehiclesQuery query)
    {
        var normalizedQuery = new
        {
            query.MakeId,
            query.ModelId,
            query.YearFrom,
            query.YearTo,
            query.PriceFrom,
            query.PriceTo,
            query.FuelType,
            query.Transmission,
            query.BodyType,
            query.Province,
            query.Page,
            query.PageSize,
            Sort = query.SortBy?.ToLowerInvariant()
        };

        var json = JsonSerializer.Serialize(normalizedQuery);
        var hash = ComputeSha256Hash(json);

        return $"search:vehicles:{hash}";
    }

    private string ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }
}
```

### 6.2 Cached Search Handler

```csharp
public class SearchVehiclesHandler : IRequestHandler<SearchVehiclesQuery, PagedResult<VehicleSummaryDto>>
{
    private readonly IDistributedCache _cache;
    private readonly IVehicleSearchService _searchService;
    private readonly SearchCacheKeyGenerator _keyGenerator;

    public async Task<PagedResult<VehicleSummaryDto>> Handle(
        SearchVehiclesQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = _keyGenerator.GenerateKey(query);

        // Try cache
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<PagedResult<VehicleSummaryDto>>(cached)!;
        }

        // Execute search
        var result = await _searchService.SearchAsync(query, cancellationToken);

        // Cache results
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken);

        return result;
    }
}
```

---

## 7. Session Management

### 7.1 Session Store

```csharp
public class RedisSessionStore : ISessionStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeSpan _sessionExpiry = TimeSpan.FromHours(24);

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var db = _redis.GetDatabase(1); // Session database
        var key = $"session:{sessionId}";

        var data = await db.StringGetAsync(key);
        if (data.IsNullOrEmpty)
            return null;

        // Refresh sliding expiration
        await db.KeyExpireAsync(key, _sessionExpiry);

        return JsonSerializer.Deserialize<UserSession>(data!);
    }

    public async Task SetSessionAsync(string sessionId, UserSession session)
    {
        var db = _redis.GetDatabase(1);
        var key = $"session:{sessionId}";

        await db.StringSetAsync(
            key,
            JsonSerializer.Serialize(session),
            _sessionExpiry);

        // Also add to user's session set (for multi-session management)
        await db.SetAddAsync($"user:{session.UserId}:sessions", sessionId);
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var db = _redis.GetDatabase(1);
        var key = $"session:{sessionId}";

        // Get session to find user
        var data = await db.StringGetAsync(key);
        if (!data.IsNullOrEmpty)
        {
            var session = JsonSerializer.Deserialize<UserSession>(data!);
            await db.SetRemoveAsync($"user:{session!.UserId}:sessions", sessionId);
        }

        await db.KeyDeleteAsync(key);
    }

    public async Task DeleteAllUserSessionsAsync(Guid userId)
    {
        var db = _redis.GetDatabase(1);
        var sessionsKey = $"user:{userId}:sessions";

        var sessionIds = await db.SetMembersAsync(sessionsKey);

        foreach (var sessionId in sessionIds)
        {
            await db.KeyDeleteAsync($"session:{sessionId}");
        }

        await db.KeyDeleteAsync(sessionsKey);
    }
}
```

---

## 8. Distributed Locking

### 8.1 RedLock Implementation

```csharp
public class DistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<DistributedLockService> _logger;

    public async Task<IAsyncDisposable?> AcquireLockAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan wait,
        TimeSpan retry)
    {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:{resource}";
        var lockValue = Guid.NewGuid().ToString();

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < wait)
        {
            // Try to acquire lock using SET NX EX
            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                expiry,
                When.NotExists);

            if (acquired)
            {
                _logger.LogDebug("Lock acquired for {Resource}", resource);
                return new LockHandle(db, lockKey, lockValue, _logger);
            }

            await Task.Delay(retry);
        }

        _logger.LogWarning("Failed to acquire lock for {Resource} after {Wait}", resource, wait);
        return null;
    }

    private class LockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _value;
        private readonly ILogger _logger;
        private bool _disposed;

        public LockHandle(IDatabase db, string key, string value, ILogger logger)
        {
            _db = db;
            _key = key;
            _value = value;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            // Only release if we still own the lock
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            await _db.ScriptEvaluateAsync(script, new RedisKey[] { _key }, new RedisValue[] { _value });
            _logger.LogDebug("Lock released for {Key}", _key);
        }
    }
}
```

### 8.2 Uso del Lock

```csharp
// Para operaciones que deben ser atómicas entre instancias
await using var lockHandle = await _lockService.AcquireLockAsync(
    $"vehicle:{vehicleId}:update",
    expiry: TimeSpan.FromSeconds(30),
    wait: TimeSpan.FromSeconds(10),
    retry: TimeSpan.FromMilliseconds(100));

if (lockHandle == null)
{
    throw new ConcurrencyException("Could not acquire lock for vehicle update");
}

// Operación protegida
await UpdateVehicleAsync(vehicle);
```

---

## 9. Pub/Sub para Invalidación

### 9.1 Cache Invalidation Publisher

```csharp
public class CacheInvalidationPublisher : ICacheInvalidationPublisher
{
    private readonly IConnectionMultiplexer _redis;
    private const string Channel = "cache:invalidation";

    public async Task PublishInvalidationAsync(string[] keys)
    {
        var subscriber = _redis.GetSubscriber();
        var message = JsonSerializer.Serialize(new CacheInvalidationMessage
        {
            Keys = keys,
            Timestamp = DateTime.UtcNow,
            SourceInstance = Environment.MachineName
        });

        await subscriber.PublishAsync(Channel, message);
    }
}
```

### 9.2 Cache Invalidation Subscriber

```csharp
public class CacheInvalidationSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheInvalidationSubscriber> _logger;
    private const string Channel = "cache:invalidation";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(Channel, async (channel, message) =>
        {
            try
            {
                var invalidation = JsonSerializer.Deserialize<CacheInvalidationMessage>(message!);

                // Skip if from same instance (already invalidated locally)
                if (invalidation!.SourceInstance == Environment.MachineName)
                    return;

                foreach (var key in invalidation.Keys)
                {
                    await _cache.RemoveAsync(key);
                }

                _logger.LogDebug("Invalidated {Count} cache keys from {Source}",
                    invalidation.Keys.Length, invalidation.SourceInstance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cache invalidation");
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
```

---

## 10. Métricas y Monitoreo

### 10.1 Prometheus Metrics

```
# Cache hit/miss ratio
cache_hits_total{service="...", cache_key_prefix="..."}
cache_misses_total{service="...", cache_key_prefix="..."}

# Cache operations latency
cache_operation_duration_seconds{service="...", operation="get|set|delete"}

# Redis connections
redis_connected_clients
redis_used_memory_bytes
redis_keyspace_hits_total
redis_keyspace_misses_total
```

### 10.2 Cache Metrics Middleware

```csharp
public class CacheMetricsDecorator : IDistributedCache
{
    private readonly IDistributedCache _inner;
    private readonly Counter _hits;
    private readonly Counter _misses;
    private readonly Histogram _duration;

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        using var timer = _duration.Labels("get").NewTimer();

        var result = await _inner.GetAsync(key, token);

        if (result != null)
            _hits.Labels(GetPrefix(key)).Inc();
        else
            _misses.Labels(GetPrefix(key)).Inc();

        return result;
    }

    private string GetPrefix(string key)
    {
        var colonIndex = key.IndexOf(':');
        return colonIndex > 0 ? key[..colonIndex] : "unknown";
    }
}
```

---

## 11. Configuración Kubernetes

### 11.1 Redis StatefulSet

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: redis
  namespace: okla
spec:
  serviceName: redis
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
        - name: redis
          image: redis:7-alpine
          ports:
            - containerPort: 6379
          command:
            - redis-server
            - "--appendonly"
            - "yes"
            - "--maxmemory"
            - "512mb"
            - "--maxmemory-policy"
            - "allkeys-lru"
          resources:
            requests:
              memory: "256Mi"
              cpu: "100m"
            limits:
              memory: "512Mi"
              cpu: "500m"
          volumeMounts:
            - name: redis-data
              mountPath: /data
  volumeClaimTemplates:
    - metadata:
        name: redis-data
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 5Gi
```

---

## 📚 Referencias

- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/) - Documentación
- [Redis Documentation](https://redis.io/documentation) - Documentación oficial
- [06-rate-limiting.md](06-rate-limiting.md) - Rate limiting
- [04-health-checks.md](04-health-checks.md) - Health checks
