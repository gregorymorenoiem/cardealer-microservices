# CacheService

Servicio de caché distribuido basado en Redis para CarDealer Microservices.

## 🎯 Características

- ✅ **Cache distribuido con Redis** - Alta performance y escalabilidad
- ✅ **Multi-tenancy** - Aislamiento de datos por tenant
- ✅ **Distributed Locks** - Coordinación de acceso a recursos compartidos
- ✅ **TTL configurable** - Expiración automática de keys
- ✅ **Statistics tracking** - Métricas de hit/miss ratio
- ✅ **Pattern-based deletion** - Invalidación masiva de cache
- ✅ **Clean Architecture** - Separación en capas Domain/Application/Infrastructure/API

## 🏗️ Arquitectura

```
CacheService
├── CacheService.Domain        # Entidades y lógica de dominio
├── CacheService.Application   # CQRS, MediatR, Interfaces
├── CacheService.Infrastructure # Implementación Redis
└── CacheService.Api           # REST API Controllers
```

## 🚀 Inicio Rápido

### Requisitos Previos

- .NET 8.0 SDK
- Redis 7.x
- Docker (opcional)

### Ejecutar con Docker

```bash
cd backend
docker-compose up cacheservice redis
```

### Ejecutar localmente

```bash
cd backend/CacheService/CacheService.Api
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5095`
- HTTPS: `https://localhost:7095`
- Swagger: `https://localhost:7095/swagger`

## 📋 API Endpoints

### Cache Operations

#### GET /api/cache/{key}
Obtiene un valor del cache.

**Query Parameters:**
- `tenantId` (opcional): ID del tenant para multi-tenancy

**Response:**
```json
{
  "key": "user:123",
  "value": "{\"name\":\"John\"}",
  "tenantId": "tenant-1"
}
```

#### POST /api/cache
Guarda un valor en cache.

**Request Body:**
```json
{
  "key": "user:123",
  "value": "{\"name\":\"John\"}",
  "tenantId": "tenant-1",
  "ttlSeconds": 3600
}
```

#### DELETE /api/cache/{key}
Elimina un valor del cache.

#### DELETE /api/cache/flush
Elimina todos los datos del cache (usar con precaución).

### Distributed Locks

#### POST /api/locks/acquire
Adquiere un lock distribuido.

**Request Body:**
```json
{
  "key": "resource-123",
  "ownerId": "worker-1",
  "ttlSeconds": 30
}
```

**Response:**
```json
{
  "message": "Lock acquired successfully",
  "lock": {
    "key": "resource-123",
    "ownerId": "worker-1",
    "acquiredAt": "2025-12-01T12:00:00Z",
    "expiresAt": "2025-12-01T12:00:30Z",
    "ttl": "00:00:30"
  }
}
```

#### POST /api/locks/release
Libera un lock distribuido.

**Request Body:**
```json
{
  "key": "resource-123",
  "ownerId": "worker-1"
}
```

### Statistics

#### GET /api/statistics
Obtiene estadísticas del cache.

**Response:**
```json
{
  "totalHits": 1500,
  "totalMisses": 300,
  "totalSets": 800,
  "totalDeletes": 50,
  "totalKeys": 750,
  "totalSizeInBytes": 1048576,
  "hitRatio": 0.8333,
  "hitPercentage": 83.33,
  "lastResetAt": "2025-12-01T00:00:00Z"
}
```

## 💡 Ejemplos de Uso

### Cache Simple

```csharp
// Set value
var request = new SetCacheRequest("product:123", "{\"name\":\"Car\"}", ttlSeconds: 3600);
await httpClient.PostAsJsonAsync("/api/cache", request);

// Get value
var response = await httpClient.GetAsync("/api/cache/product:123");
var result = await response.Content.ReadFromJsonAsync<CacheResponse>();
```

### Cache con Multi-Tenancy

```csharp
// Set value for tenant
var request = new SetCacheRequest("user:456", "{\"name\":\"Jane\"}", "tenant-A", 7200);
await httpClient.PostAsJsonAsync("/api/cache", request);

// Get value for tenant
var response = await httpClient.GetAsync("/api/cache/user:456?tenantId=tenant-A");
```

### Distributed Lock

```csharp
// Acquire lock
var acquireRequest = new AcquireLockRequest("order:789", "worker-1", 30);
var acquireResponse = await httpClient.PostAsJsonAsync("/api/locks/acquire", acquireRequest);

if (acquireResponse.IsSuccessStatusCode)
{
    try
    {
        // Critical section: process order
        await ProcessOrder("order:789");
    }
    finally
    {
        // Release lock
        var releaseRequest = new ReleaseLockRequest("order:789", "worker-1");
        await httpClient.PostAsJsonAsync("/api/locks/release", releaseRequest);
    }
}
```

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "CacheConfiguration": {
    "DefaultTtlSeconds": 3600,
    "MaxTtlSeconds": 86400,
    "MaxKeySizeBytes": 1024,
    "MaxValueSizeBytes": 1048576,
    "EnableStatistics": true,
    "EnableCompression": false,
    "ConnectTimeoutMs": 5000,
    "SyncTimeoutMs": 5000,
    "RetryMaxAttempts": 3
  }
}
```

### Variables de Entorno

- `ConnectionStrings__Redis`: Connection string de Redis
- `CacheConfiguration__DefaultTtlSeconds`: TTL por defecto en segundos

## 🧪 Testing

### Ejecutar Tests Unitarios

```bash
cd backend/CacheService
dotnet test
```

**Resultado esperado:** 24/24 tests pasando ✅

### Tests Incluidos

- `CacheEntryTests` - 6 tests
- `CacheLockTests` - 5 tests
- `CacheStatisticsTests` - 8 tests
- `CacheConfigurationTests` - 5 tests

## 📊 Stack Tecnológico

- **Runtime**: .NET 8.0
- **Cache**: Redis 7.x
- **Client**: StackExchange.Redis 2.8.16
- **Patterns**: CQRS con MediatR 12.2.0
- **API**: ASP.NET Core Web API
- **Testing**: xUnit + FluentAssertions + Moq

## 🔒 Seguridad

- ✅ CORS configurado
- ✅ Multi-tenancy con aislamiento de datos
- ✅ Validación de ownership en locks
- ✅ TTL máximo configurable

## 📈 Performance

- **Throughput**: >10,000 ops/sec (depende de Redis)
- **Latency**: <5ms para operaciones GET/SET
- **Distributed Locks**: Lock acquisition <10ms

## 🐛 Troubleshooting

### Error: "Unable to connect to Redis"

1. Verificar que Redis esté corriendo:
   ```bash
   docker ps | grep redis
   ```

2. Verificar connection string en appsettings.json

3. Probar conexión manual:
   ```bash
   redis-cli -h localhost -p 6379 ping
   ```

### Lock no se libera automáticamente

Los locks tienen TTL automático. Si un proceso falla antes de liberar el lock, expirará automáticamente después del TTL configurado.

## 📝 To-Do

- [ ] Implementar integración con Azure Redis Cache
- [ ] Agregar compresión de valores grandes
- [ ] Implementar rate limiting por tenant
- [ ] Dashboard de monitoreo en tiempo real
- [ ] Soporte para Redis Cluster

## 📄 Licencia

MIT License - CarDealer Microservices

## 👥 Contribución

Para contribuir, por favor:
1. Fork el repositorio
2. Crea una rama feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

**Estado**: ✅ Completado - 24/24 tests pasando
**Versión**: 1.0.0
**Última actualización**: Diciembre 1, 2025
