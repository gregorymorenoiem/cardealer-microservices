# Sprint 31 Report — 2026-03-06

## Resumen Ejecutivo

Sprint enfocado en **caching estratégico** para resolver el hallazgo más crítico del Sprint 30: 12+ microservicios sin caching, VehiclesSaleService (mayor tráfico) con ZERO caching.

**Resultado**: biblioteca de caching estandarizada + integración en VehiclesSaleService + Gateway-level caching en 5 rutas públicas.

---

## Tareas Completadas (6/6)

### 1. ✅ CarDealer.Shared.Caching Library (P0)

**Archivos creados:** 5

- `CarDealer.Shared.Caching.csproj` — net8.0, StackExchange.Redis 2.8.22, Microsoft.Extensions.Caching.StackExchangeRedis 8.0.11
- `Models/CacheOptions.cs` — Configuración: RedisConnectionString, InstanceName, DefaultTtlSeconds=300, MaxTtlSeconds=86400, SlidingExpiration, Metrics
- `Interfaces/ICacheService.cs` — GetAsync, SetAsync, RemoveAsync, ExistsAsync, GetOrSetAsync, InvalidateByPatternAsync
- `Services/RedisCacheService.cs` — Implementación completa: JSON serialization, fail-safe (nunca throw), hit/miss metrics, SCAN para pattern invalidation
- `Extensions/CachingExtensions.cs` — `AddStandardCaching(config, serviceName)` con Redis/in-memory fallback, env var fallback, TryAddSingleton

**Características clave:**

- Fail-safe: todas las operaciones Redis son try/catch, nunca lanzan excepciones
- Key prefixing: `okla:{servicename}:` para aislamiento multi-servicio en Redis compartido
- Max TTL guard: previene TTL excesivamente largos
- Pattern invalidation: SCAN + DELETE para invalidación de wildcards (vehicle:\*)
- Fallback: si Redis no está disponible, usa DistributedMemoryCache automáticamente

### 2. ✅ Caching Library Tests (27 tests)

- `CacheOptionsTests.cs` (5 tests): defaults, SectionName, property overrides
- `RedisCacheServiceTests.cs` (16 tests): get/set/remove/exists, GetOrSetAsync cache miss/hit/factory failure, TTL clamping, pattern invalidation no-op, serialization edge cases
- `CachingExtensionsTests.cs` (7 tests): DI registration, instance naming, config binding, string overload, idempotency, end-to-end in-memory roundtrip

**Total tests**: 83 (56 Sprint 30 + 27 caching)

### 3. ✅ VehiclesSaleService Redis Caching Integration (P0)

**Archivos modificados:** 5

- `VehiclesSaleService.Api.csproj` — referencia a CarDealer.Shared.Caching
- `Program.cs` — `AddStandardCaching(config, "VehiclesSaleService")`
- `VehiclesController.cs`:
  - ICacheService inyectado
  - `GetById()` — cache 5 min TTL (clave: `vehicle:detail:{id}`)
  - `GetFeatured()` — cache 10 min TTL (clave: `vehicle:featured:{take}`)
  - Cache invalidation en Update y Delete via `InvalidateVehicleCacheAsync()`
- `CatalogController.cs`:
  - ICacheService inyectado
  - `GetAllMakes()` — cache 24h TTL (clave: `catalog:makes:all`)
  - `GetPopularMakes()` — cache 24h TTL (clave: `catalog:makes:popular:{take}`)
- `appsettings.json` / `appsettings.Docker.json` — sección "Caching" agregada

### 4. ✅ Pre-existing Build Errors Fixed

- `VehiclesController.cs:1203` — `vehicle.SellerType ?? "Individual"` → `vehicle.SellerType.ToString()` (SellerType es enum, no nullable string)
- `VehiclesController.cs:1209` — `vehicle.Vin` → `vehicle.VIN` (property name es VIN, no Vin)

### 5. ✅ AuditService/MediaService Dead Redis Audit

**Hallazgo:** Código muerto (configuración scaffolded pero nunca conectada), NO riesgo runtime.

- AuditService: tiene `RedisHealthCheck.cs` que inyecta `IDistributedCache`, pero el check NUNCA se registra en Program.cs. Sin crash risk.
- MediaService: solo `CacheSettings.cs` y `CacheKeys.cs` sin ningún NuGet Redis ni uso real.
- **Decisión:** no tocar por ahora — riesgo cero, priorizar caching activo sobre limpiar dead code.

### 6. ✅ Gateway Ocelot FileCacheOptions (5 rutas)

**Archivo:** `Gateway.Api/ocelot.prod.json`

| Ruta                             | TTL  | Region           |
| -------------------------------- | ---- | ---------------- |
| `GET /api/vehicles` (search)     | 30s  | vehiclesSearch   |
| `GET /api/vehicles/featured`     | 60s  | vehiclesFeatured |
| `GET /api/vehicles/{id}`         | 30s  | vehicleDetail    |
| `GET /api/vehicles/{id}/similar` | 60s  | vehiclesSimilar  |
| `GET /api/catalog`               | 300s | catalogListing   |

---

## Métricas

| Métrica                        | Valor                                                           |
| ------------------------------ | --------------------------------------------------------------- |
| Archivos creados               | 8                                                               |
| Archivos modificados           | 8                                                               |
| Tests totales                  | 83 (27 nuevos)                                                  |
| Tests pasando                  | 83/83 ✅                                                        |
| Builds exitosos                | VehiclesSaleService ✅, Gateway ✅, CarDealer.Shared.Caching ✅ |
| Bugs pre-existentes corregidos | 2 (SellerType enum, VIN property name)                          |

---

## Impacto de Performance Esperado

### Antes (Sprint 30)

- VehiclesSaleService: 0 cache, TODA request → PostgreSQL
- Gateway: 0 response cache, TODA request → downstream microservicio
- Catalog (makes/models): consultado en cada page load, mismos datos siempre

### Después (Sprint 31)

- **Vehicle detail**: ~90% cache hit ratio esperado (5 min TTL, same vehicle viewed repeatedly)
- **Featured vehicles**: ~95% cache hit (10 min TTL, homepage component)
- **Catalog makes**: ~99% cache hit (24h TTL, data changes monthly)
- **Gateway search**: reduce downstream calls en 30-50% (30s TTL, absorbe burst traffic)
- **In-memory fallback**: si Redis falla, el servicio sigue funcionando con DistributedMemoryCache

---

## Deuda Técnica Identificada

1. **No hay cache invalidation via eventos RabbitMQ** — cuando un vehículo se modifica en otro pod, los demás pods no lo saben
2. **6 shared libraries sin tests**: ApiVersioning, FeatureFlags, Logging, Observability, Sagas, ServiceDiscovery
3. **12+ servicios sin caching**: solo VehiclesSaleService tiene caching ahora
4. **VehiclesController fat controller**: 1,860 líneas, debería migrar a CQRS/MediatR
5. **ContactService vacío**: solo tiene Class1.cs, servicio no implementado

---

## Prioridades Sprint 32

1. Cache invalidation via RabbitMQ (vehicle.updated → invalidate cache across pods)
2. Shared lib tests restantes (6 bibliotecas)
3. ContactService implementation
4. Frontend performance audit (TanStack Query stale times)
5. Response compression middleware
6. NotificationService security audit
