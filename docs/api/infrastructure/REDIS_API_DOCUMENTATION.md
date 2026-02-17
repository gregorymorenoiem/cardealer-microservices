# 🔴 API Redis - Cache & Session Store

**Proveedor:** Redis (Open Source)  
**Documentación oficial:** https://redis.io/docs/  
**Versión:** 7+  
**Actualizado:** Enero 2026

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Configuración](#configuración)
3. [Casos de uso](#casos-de-uso)
4. [Comandos básicos](#comandos-básicos)
5. [Conexión desde .NET](#conexión-desde-net)
6. [Patrones de uso](#patrones-de-uso)
7. [Performance](#performance)
8. [Persistence](#persistence)

---

## 🎯 Introducción

Redis es el cache distribuido y session store de OKLA, usado para:

- **Cache:** Reducir carga en PostgreSQL
- **Sessions:** JWT tokens, user sessions
- **Rate limiting:** Controlar requests por IP/user
- **Pub/Sub:** Comunicación real-time entre servicios
- **Locks distribuidos:** Prevenir race conditions

###¿Por qué Redis?

- ⚡ **Ultra rápido:** Operaciones en memoria (<1ms)
- 🔄 **Versatilidad:** Strings, Lists, Sets, Hashes, Sorted Sets
- 📡 **Pub/Sub:** Mensajería en tiempo real
- 🌐 **Distribuido:** Cluster mode para alta disponibilidad
- 💾 **Persistence:** RDB y AOF para durabilidad

---

## 🔧 Configuración

### Docker Compose

```yaml
redis:
  image: redis:7-alpine
  container_name: redis
  ports:
    - "6379:6379"
  command: redis-server --requirepass ${REDIS_PASSWORD:-password}
  volumes:
    - redis_data:/data
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
    interval: 10s
    timeout: 5s
    retries: 5
```

### Connection String

```
localhost:6379,password=password,abortConnect=false,connectTimeout=5000
```

**Producción (DOKS):**

```
redis.okla.svc.cluster.local:6379,password=${REDIS_PASSWORD},ssl=true,abortConnect=false
```

---

## 💼 Casos de Uso en OKLA

| Servicio                   | Usa Redis Para                                  |
| -------------------------- | ----------------------------------------------- |
| **AuthService**            | JWT refresh tokens, session tokens              |
| **VehiclesSaleService**    | Cache de vehículos populares, homepage sections |
| **BillingService**         | Idempotency keys de pagos, rate limiting        |
| **Gateway**                | Rate limiting global, circuit breaker state     |
| **NotificationService**    | Throttling de emails/SMS                        |
| **DealerAnalyticsService** | Cache de stats, rolling windows                 |
| **ComparisonService**      | Cache de comparaciones recientes                |

---

## 📝 Comandos Básicos

### Strings

```redis
# Set/Get
SET user:123:name "John Doe"
GET user:123:name

# Set con expiración (5 minutos)
SETEX session:abc123 300 "user_data"

# Set solo si no existe
SETNX lock:vehicle:456 "locked"

# Increment
INCR page:views:123
INCRBY page:views:123 10
```

### Hashes

```redis
# Set hash fields
HSET vehicle:123 make "Toyota" model "Camry" year "2024"

# Get single field
HGET vehicle:123 make

# Get all fields
HGETALL vehicle:123

# Increment hash field
HINCRBY vehicle:123 views 1
```

### Lists

```redis
# Push (agregar al final)
RPUSH notifications:user:123 "New message"

# Pop (obtener y remover del inicio)
LPOP notifications:user:123

# Get rango
LRANGE notifications:user:123 0 9  # Primeros 10

# Tamaño
LLEN notifications:user:123
```

### Sets

```redis
# Add members
SADD favorites:user:123 vehicle:456 vehicle:789

# Check membership
SISMEMBER favorites:user:123 vehicle:456

# Get all members
SMEMBERS favorites:user:123

# Remove member
SREM favorites:user:123 vehicle:456

# Count
SCARD favorites:user:123
```

### Sorted Sets

```redis
# Add with score
ZADD leaderboard:views 1500 vehicle:123 2300 vehicle:456

# Get top 10
ZREVRANGE leaderboard:views 0 9 WITHSCORES

# Get rank
ZREVRANK leaderboard:views vehicle:123

# Increment score
ZINCRBY leaderboard:views 1 vehicle:123
```

---

## 🔌 Conexión desde .NET

### Instalar StackExchange.Redis

```bash
dotnet add package StackExchange.Redis --version 2.7.10
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 8.0.0
```

### Program.cs

```csharp
using StackExchange.Redis;

// Opción 1: IDistributedCache (abstracto, fácil)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "okla:";
});

// Opción 2: IConnectionMultiplexer (avanzado, más control)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration["Redis:ConnectionString"],
        true
    );

    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    configuration.ReconnectRetryPolicy = new ExponentialRetry(1000);

    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<ICacheService, RedisCacheService>();
```

---

## 🎯 Patrones de Uso

### 1. Cache-Aside Pattern

```csharp
public class VehicleService
{
    private readonly IDistributedCache _cache;
    private readonly IVehicleRepository _repository;

    public async Task<Vehicle?> GetVehicleAsync(Guid id)
    {
        // 1. Intentar obtener del cache
        var cacheKey = $"vehicle:{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<Vehicle>(cachedData);
        }

        // 2. Si no está en cache, obtener de DB
        var vehicle = await _repository.GetByIdAsync(id);

        if (vehicle == null)
            return null;

        // 3. Guardar en cache (5 minutos)
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(vehicle),
            options
        );

        return vehicle;
    }
}
```

### 2. Rate Limiting

```csharp
public class RateLimitingMiddleware
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<bool> IsAllowedAsync(string clientId, int maxRequests, TimeSpan window)
    {
        var db = _redis.GetDatabase();
        var key = $"ratelimit:{clientId}";

        // Increment y get con transacción
        var tran = db.CreateTransaction();
        var incrTask = tran.StringIncrementAsync(key);
        var expireTask = tran.KeyExpireAsync(key, window);

        await tran.ExecuteAsync();

        var count = await incrTask;

        // Si es primera request, set expiration
        if (count == 1)
        {
            await db.KeyExpireAsync(key, window);
        }

        return count <= maxRequests;
    }
}

// Uso: Middleware en Program.cs
app.Use(async (context, next) =>
{
    var rateLimiter = context.RequestServices.GetRequiredService<RateLimitingMiddleware>();
    var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    if (!await rateLimiter.IsAllowedAsync(clientId, maxRequests: 100, TimeSpan.FromMinutes(1)))
    {
        context.Response.StatusCode = 429; // Too Many Requests
        await context.Response.WriteAsync("Rate limit exceeded");
        return;
    }

    await next();
});
```

### 3. Distributed Lock

```csharp
public class InventoryService
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<bool> ProcessInventoryUpdateAsync(Guid dealerId)
    {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:inventory:{dealerId}";
        var lockValue = Guid.NewGuid().ToString();

        try
        {
            // Adquirir lock (5 segundos)
            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                TimeSpan.FromSeconds(5),
                When.NotExists
            );

            if (!acquired)
            {
                // Otro proceso tiene el lock
                return false;
            }

            // Procesar update
            await UpdateInventoryAsync(dealerId);

            return true;
        }
        finally
        {
            // Liberar lock (solo si somos los dueños)
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end
            ";

            await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
        }
    }
}
```

### 4. Session Storage

```csharp
public class SessionService
{
    private readonly IDistributedCache _cache;

    public async Task CreateSessionAsync(string sessionId, UserSession session)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(24) // Renovar si activo
        };

        await _cache.SetStringAsync(
            $"session:{sessionId}",
            JsonSerializer.Serialize(session),
            options
        );
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        var data = await _cache.GetStringAsync($"session:{sessionId}");

        if (string.IsNullOrEmpty(data))
            return null;

        return JsonSerializer.Deserialize<UserSession>(data);
    }

    public async Task ExtendSessionAsync(string sessionId)
    {
        await _cache.RefreshAsync($"session:{sessionId}");
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        await _cache.RemoveAsync($"session:{sessionId}");
    }
}
```

### 5. Pub/Sub

```csharp
// Publisher
public class VehicleEventPublisher
{
    private readonly IConnectionMultiplexer _redis;

    public async Task PublishVehicleCreatedAsync(Guid vehicleId)
    {
        var sub = _redis.GetSubscriber();
        await sub.PublishAsync(
            "vehicle:events",
            JsonSerializer.Serialize(new { VehicleId = vehicleId, Event = "Created" })
        );
    }
}

// Subscriber
public class VehicleEventSubscriber : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = _redis.GetSubscriber();

        await sub.SubscribeAsync("vehicle:events", (channel, message) =>
        {
            var data = JsonSerializer.Deserialize<VehicleEvent>(message!);
            // Procesar evento
            Console.WriteLine($"Vehicle {data.VehicleId} was {data.Event}");
        });
    }
}
```

---

## ⚡ Performance

### 1. Pipelining

Enviar múltiples comandos sin esperar respuestas (reduce round-trips).

```csharp
var db = _redis.GetDatabase();

var batch = db.CreateBatch();
var tasks = new List<Task>();

for (int i = 0; i < 1000; i++)
{
    tasks.Add(batch.StringSetAsync($"key:{i}", $"value:{i}"));
}

batch.Execute();
await Task.WhenAll(tasks);
```

### 2. Batch Operations

```csharp
var db = _redis.GetDatabase();

// Get múltiples keys
var keys = Enumerable.Range(1, 100).Select(i => (RedisKey)$"vehicle:{i}").ToArray();
var values = await db.StringGetAsync(keys);

// Set múltiples keys
var entries = Enumerable.Range(1, 100)
    .Select(i => new KeyValuePair<RedisKey, RedisValue>($"vehicle:{i}", $"data{i}"))
    .ToArray();

await db.StringSetAsync(entries);
```

### 3. Compression

```csharp
using System.IO.Compression;

public class CompressedCacheService
{
    private readonly IDistributedCache _cache;

    public async Task SetCompressedAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Fastest))
        {
            await gzip.WriteAsync(bytes);
        }

        await _cache.SetAsync(key, output.ToArray(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });
    }

    public async Task<T?> GetCompressedAsync<T>(string key)
    {
        var compressed = await _cache.GetAsync(key);

        if (compressed == null)
            return default;

        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);

        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

---

## 💾 Persistence

### RDB (Snapshot)

Guardar snapshot del dataset completo periódicamente.

**redis.conf:**

```
save 900 1      # Después de 900 seg (15 min) si al menos 1 key cambió
save 300 10     # Después de 300 seg (5 min) si al menos 10 keys cambiaron
save 60 10000   # Después de 60 seg (1 min) si al menos 10000 keys cambiaron
```

### AOF (Append-Only File)

Log de todas las operaciones de escritura.

**redis.conf:**

```
appendonly yes
appendfsync everysec   # Fsync cada segundo (balance performance/durabilidad)
```

### Recomendación para OKLA

- **Desarrollo:** Sin persistence (más rápido)
- **Producción:** RDB + AOF (máxima durabilidad)

---

## 🔐 Seguridad

### 1. Password

```bash
# Set password
redis-cli CONFIG SET requirepass "strong_password"

# Auth
redis-cli -a strong_password
```

### 2. Rename Dangerous Commands

```
rename-command FLUSHDB ""
rename-command FLUSHALL ""
rename-command CONFIG "CONFIG_abc123"
```

### 3. ACL (Redis 6+)

```
# Crear usuario read-only
ACL SETUSER readonly on >password ~cached:* +get +mget

# Crear usuario para rate limiting
ACL SETUSER ratelimiter on >password ~ratelimit:* +get +set +incr +expire
```

---

## 📊 Monitoring

### Redis CLI

```bash
# Info general
redis-cli INFO

# Memoria
redis-cli INFO memory

# Stats
redis-cli INFO stats

# Clientes conectados
redis-cli CLIENT LIST

# Slow log (queries >10ms)
redis-cli SLOWLOG GET 10

# Monitor en tiempo real (¡cuidado, muy verbose!)
redis-cli MONITOR
```

### Métricas Importantes

| Métrica                             | Descripción         | Threshold          |
| ----------------------------------- | ------------------- | ------------------ |
| `used_memory`                       | Memoria usada       | <80% del maxmemory |
| `connected_clients`                 | Clientes conectados | <10,000            |
| `keyspace_hits` / `keyspace_misses` | Cache hit ratio     | >90%               |
| `evicted_keys`                      | Keys evictadas      | <100/seg           |
| `expired_keys`                      | Keys expiradas      | Normal             |

---

## 🧪 Testing

### Flush All (SOLO en desarrollo)

```bash
redis-cli FLUSHALL
```

### Test de Performance

```bash
# Benchmark
redis-benchmark -h localhost -p 6379 -c 50 -n 10000

# Test específico
redis-benchmark -t set,get -n 100000 -q
```

---

## 📚 Referencias

- [Redis Documentation](https://redis.io/docs/)
- [StackExchange.Redis Docs](https://stackexchange.github.io/StackExchange.Redis/)
- [Redis Best Practices](https://redis.io/docs/manual/patterns/)
- [Microsoft IDistributedCache](https://docs.microsoft.com/aspnet/core/performance/caching/distributed)

---

**Implementado en:** AuthService, VehiclesSaleService, BillingService, Gateway  
**Versión:** 7  
**Última actualización:** Enero 15, 2026
