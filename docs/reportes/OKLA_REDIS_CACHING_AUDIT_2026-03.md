# 🔴 OKLA — Redis & Caching Audit Report

**Date:** 2026-03-06  
**Auditor:** CPSO (GitHub Copilot)  
**Scope:** All 27 microservices in `/backend/`

---

## 1. Redis Infrastructure Configuration

### Docker Compose (`compose.yaml`)

- **Image:** `redis:7-alpine`
- **Max Memory:** 256 MB (`--maxmemory 256mb`)
- **Eviction Policy:** `allkeys-lru` (good for caching)
- **Persistence:** RDB snapshots every 60s with 1000+ changes (`--save 60 1000`)
- **Resources:** 0.5 CPU, 512 MB RAM (256 MB reserved)
- **Health Check:** `redis-cli ping` every 10s

### Docker Compose Production (`compose.docker.yaml`)

- Password-protected via Docker secrets (`redis_password.txt`)
- Data persisted in `redis_data` volume

### ⚠️ Finding: `compose.yaml` (dev) has **no password** — acceptable for local dev but inconsistent with production.

---

## 2. Where Redis/Caching IS Used (Current State)

### ✅ 2.1 AuthService — **MOST MATURE** Redis Consumer

| Component                 | Type                                            | Details                                                                                            |
| ------------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `TwoFactorService`        | `IDistributedCache`                             | Dual persistence (Redis + PostgreSQL) for 2FA recovery codes; 365-day TTL with PostgreSQL fallback |
| `TokenService`            | `IDistributedCache`                             | Token caching with 5-min expiration                                                                |
| `RevokedDeviceService`    | `IDistributedCache`                             | Revoked device session tracking                                                                    |
| `IpApiGeoLocationService` | `IMemoryCache`                                  | GeoIP lookup results cached in-memory                                                              |
| `SecurityConfigProvider`  | `IMemoryCache`                                  | Security configuration cached in-memory                                                            |
| `RedisHealthCheck`        | `IConnectionMultiplexer`                        | Direct StackExchange.Redis PING health check                                                       |
| DI Setup                  | `AddStackExchangeRedisCache` + `AddMemoryCache` | Full setup with fallback to `DistributedMemoryCache`                                               |

**Config:**

```json
"Cache": {
    "RedisConnectionString": "redis:6379",
    "UserCacheExpirationMinutes": 15,
    "TokenCacheExpirationMinutes": 5,
    "EnableDistributedCache": true
},
"ConnectionStrings": { "Redis": "redis:6379" }
```

---

### ✅ 2.2 Gateway — Redis for Rate Limiting

| Component     | Type                                  | Details                                                                    |
| ------------- | ------------------------------------- | -------------------------------------------------------------------------- |
| Rate Limiting | `RateLimitingOptions.RedisConnection` | Redis-backed distributed rate limiting via `CarDealer.Shared.RateLimiting` |

**Config:**

```json
"Redis": { "Connection": "redis:6379,abortConnect=false" },
"RateLimiting": { "Enabled": true, "Mode": "redis" }
```

**⚠️ Finding:** Gateway uses Redis **only for rate limiting**. There is **NO response caching** or output caching at the gateway level. This is a significant missed opportunity.

---

### ✅ 2.3 ChatbotService — Redis for LLM Response Cache

| Component                 | Type                                                   | Details                                                                                                |
| ------------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------ |
| `LlmResponseCacheService` | `IDistributedCache` (via `AddStackExchangeRedisCache`) | SHA256-keyed cache for LLM responses; 30-min default TTL; skips dynamic intents (vehicle_search, etc.) |

**Config:** `Redis:ConnectionString` → `redis:6379`, fallback to `DistributedMemoryCache`.

**⚠️ Finding:** Good implementation with smart exclusions for dynamic content. However, the `try/catch` around `AddStackExchangeRedisCache` at startup doesn't actually work — `AddStackExchangeRedisCache` registers services (doesn't connect yet). The catch is never hit.

---

### ✅ 2.4 SearchAgent — Redis for AI Search Cache

| Component            | Type                | Details                                                                            |
| -------------------- | ------------------- | ---------------------------------------------------------------------------------- |
| `SearchCacheService` | `IDistributedCache` | Caches AI-powered search responses by query hash; configurable TTL (default 3600s) |
| `SearchAgentConfig`  | Domain entity       | `EnableCache`, `CacheTtlSeconds`, `SemanticCacheThreshold` (0.95) stored in DB     |

**Config:** `ConnectionStrings:Redis` → `AddStackExchangeRedisCache`, fallback to `DistributedMemoryCache`.

---

### ✅ 2.5 Shared Libraries — Redis Usage

| Library                         | Component                    | Details                                                                                     |
| ------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------- |
| `CarDealer.Shared.Idempotency`  | `RedisIdempotencyClient`     | Redis via `IConnectionMultiplexer` for idempotent request deduplication; TTL-based expiry   |
| `CarDealer.Shared.HealthChecks` | `HealthCheckExtensions`      | Standard Redis health check configuration (`HealthChecks.Redis`)                            |
| `CarDealer.Shared.FeatureFlags` | `FeatureFlagClient`          | `IMemoryCache` for feature flag caching (NOT Redis)                                         |
| `CarDealer.Shared.Sagas`        | `SagaOptions`                | Supports `Redis` repository type for saga state; `RedisKeyPrefix`, `RedisExpirationMinutes` |
| `CarDealer.Shared`              | `ConfigurationServiceClient` | `IMemoryCache` for config values (NOT Redis)                                                |
| `CarDealer.Shared`              | `ModuleAccessExtensions`     | `AddDistributedMemoryCache()` for module access tokens                                      |

---

### ✅ 2.6 NotificationService — In-Memory Only

| Component                    | Type           | Details                             |
| ---------------------------- | -------------- | ----------------------------------- |
| `ConfigurationServiceClient` | `IMemoryCache` | Config values cached in-memory      |
| `TemplateEngine`             | `IMemoryCache` | Compiled templates cached in-memory |
| `TemplateService`            | `IMemoryCache` | Template lookups cached in-memory   |

**⚠️ Finding:** No Redis — only `AddMemoryCache()`. Acceptable for templates since they're local.

---

### ✅ 2.7 UserService — In-Memory Only

| Component                   | Type           | Details                                 |
| --------------------------- | -------------- | --------------------------------------- |
| `CheckPermissionQuery`      | `IMemoryCache` | Permission check results cached locally |
| `ReviewServiceClient`       | `IMemoryCache` | External service responses cached       |
| `VehiclesSaleServiceClient` | `IMemoryCache` | External service responses cached       |
| `RoleServiceClient`         | `IMemoryCache` | Role lookups cached                     |

**⚠️ Finding:** All caching is in-memory. For a multi-instance deployment, permission caches will be **inconsistent across pods**.

---

### ✅ 2.8 SupportAgent — In-Memory FAQ Cache

| Component                  | Type                         | Details                                              |
| -------------------------- | ---------------------------- | ---------------------------------------------------- |
| `InMemoryFaqResponseCache` | `IMemoryCache` (50 MB limit) | FAQ responses cached; implements `IFaqResponseCache` |

---

### ✅ 2.9 RoleService — In-Memory Only

| Component    | Type                          | Details                                                 |
| ------------ | ----------------------------- | ------------------------------------------------------- |
| `Program.cs` | `AddDistributedMemoryCache()` | In-memory distributed cache (not actually distributed!) |

---

### ✅ 2.10 AuditService — CacheSettings Defined but NOT Wired

| Component          | Type                | Details                                                                                    |
| ------------------ | ------------------- | ------------------------------------------------------------------------------------------ |
| `CacheSettings.cs` | Settings class      | Full `CacheSettings` defined (RedisConnectionString, AuditLogCacheExpirationMinutes, etc.) |
| `RedisHealthCheck` | `IDistributedCache` | Health check exists using distributed cache                                                |
| `Program.cs`       | **MISSING**         | ⛔ No `AddStackExchangeRedisCache()` or `AddDistributedMemoryCache()` in Program.cs        |

**🔴 Critical:** `CacheSettings` and `RedisHealthCheck` exist but Redis is NOT registered in DI. The health check will fail at runtime with a missing service exception if enabled.

---

### ✅ 2.11 MediaService — CacheSettings Defined but NOT Wired

| Component          | Type           | Details                                                                                                          |
| ------------------ | -------------- | ---------------------------------------------------------------------------------------------------------------- |
| `CacheSettings.cs` | Settings class | Full `CacheSettings` (RedisConnectionString, MediaCacheExpirationMinutes, UploadUrlCacheExpirationMinutes, etc.) |
| `Program.cs`       | **MISSING**    | ⛔ No Redis or distributed cache registration found                                                              |

**🔴 Critical:** Same issue as AuditService — settings exist but no actual cache implementation.

---

### ✅ 2.12 BillingService — Redis for Rate Limiting Only

| Component     | Type                                  | Details                                                            |
| ------------- | ------------------------------------- | ------------------------------------------------------------------ |
| Rate Limiting | `RateLimitingOptions.RedisConnection` | Redis connection configured (`Redis:Connection`) for rate limiting |

---

## 3. Services with ZERO Caching

| Service                        | Status                                                | Risk                                                |
| ------------------------------ | ----------------------------------------------------- | --------------------------------------------------- |
| **VehiclesSaleService**        | ⛔ NO caching at all                                  | 🔴 **CRITICAL** — Highest-traffic service           |
| **RecommendationService**      | ⛔ NO caching                                         | 🟡 Medium — recommendations are compute-heavy       |
| **DealerAnalyticsService**     | ⛔ NO caching                                         | 🟡 Medium — aggregations are expensive              |
| **CRMService**                 | ⛔ NO caching                                         | 🟢 Low — mostly write operations                    |
| **ComparisonService**          | ⛔ NO caching                                         | 🟡 Medium — vehicle comparisons are read-heavy      |
| **ReportsService**             | ⛔ NO caching                                         | 🟡 Medium — report generation is expensive          |
| **VehicleIntelligenceService** | ⛔ NO caching                                         | 🟡 Medium — AI processing results                   |
| **AIProcessingService**        | ⛔ NO caching                                         | 🟢 Low — async processing                           |
| **ContactService**             | ⛔ NO caching                                         | 🟢 Low — mostly writes                              |
| **ErrorService**               | ⛔ NO caching (only AddMemoryCache for rate limiting) | 🟢 Low                                              |
| **AdminService**               | ⛔ NO caching                                         | 🟢 Low — admin-only, low traffic                    |
| **KYCService**                 | ⛔ NO caching (has TODO comment)                      | 🟡 Medium — JCE external API calls should be cached |

---

## 4. High-Impact Recommendations (Ranked by Performance Impact)

### 🥇 P0 — VehiclesSaleService: Add Redis Caching (CRITICAL)

**Why:** This is the **core revenue-generating service** (vehicle search, listings, catalog). Every homepage load, search, and vehicle detail hits PostgreSQL directly with zero caching.

**Uncached hot paths identified:**

- `GET /api/vehicles/search` — Full-text search with 15+ filter params → hits DB every time
- `GET /api/vehicles/{id}` — Vehicle detail page (every view = DB query)
- `GET /api/vehicles/slug/{slug}` — SEO-friendly detail page
- `GET /api/catalog/makes` — Vehicle makes catalog (static data, changes rarely)
- `GET /api/catalog/makes/popular` — Popular makes (semi-static)
- `GET /api/catalog/models/{makeSlug}` — Models by make (static data)
- `GET /api/catalog/body-types`, `/fuel-types`, `/transmissions`, `/drive-types`, `/conditions` — Enum-like static data
- `GET /api/homepage-sections` — Homepage curated sections with vehicles

**Recommended TTLs:**
| Endpoint | TTL | Reason |
|----------|-----|--------|
| Catalog (makes, models, trims) | 24 hours | Changes only via admin |
| Body types / fuel types / enums | 7 days | Effectively static |
| Vehicle search results | 2–5 minutes | Balance freshness vs load |
| Individual vehicle detail | 5–10 minutes | Invalidate on update event |
| Homepage sections | 5–10 minutes | Curated, low churn |
| Vehicle count/stats | 15 minutes | Aggregation query |

**Estimated impact:** 60–80% reduction in PostgreSQL load for read operations.

---

### 🥈 P1 — Gateway: Add Response Caching Layer

**Why:** The Gateway proxies ALL traffic but performs zero response caching. Adding output/response caching at this layer would protect ALL downstream services.

**Recommendation:** Add `OutputCache` middleware or Ocelot's built-in `FileCacheOptions` in route configuration:

```json
"FileCacheOptions": {
    "TtlSeconds": 30,
    "Region": "vehiclesearch"
}
```

**Best candidates for gateway-level caching:**

- `GET /api/vehicles/search` (anonymous/common searches)
- `GET /api/catalog/*` (fully cacheable)
- `GET /api/homepage-sections` (public)

**Estimated impact:** 30–50% reduction in downstream service calls.

---

### 🥉 P2 — Fix AuditService & MediaService: Wire Up Existing CacheSettings

**Why:** Both services have full `CacheSettings` classes and even `RedisHealthCheck` implementations, but **Redis is not registered in DI**. This is dead code that suggests caching was planned but never completed.

**For AuditService:** Add to `Program.cs`:

```csharp
var cacheSettings = builder.Configuration.GetSection("Cache").Get<CacheSettings>();
if (cacheSettings?.EnableDistributedCache == true)
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = cacheSettings.RedisConnectionString);
else
    builder.Services.AddDistributedMemoryCache();
```

**For MediaService:** Same pattern. Media metadata and upload URLs are excellent cache candidates.

---

### P3 — KYCService: Implement Redis for JCE API Calls

**Why:** The JCE (Junta Central Electoral) citizen verification API is an external service. There's already a TODO comment at `JCEService.cs:293`:

```csharp
// TODO: Implementar cache con Redis
// var cached = await _cache.GetAsync<JCECitizenData>($"jce:cedula:{cleanCedula}");
```

**Recommendation:** Cache JCE responses with 60-minute TTL (already defined in `CacheMinutes`). Citizen data doesn't change frequently.

---

### P4 — UserService: Migrate Permission Cache to Redis

**Why:** `CheckPermissionQuery` caches permissions in `IMemoryCache`. In a multi-pod K8s deployment, if User A's role changes, pod 2's cache still has the old permissions → **authorization inconsistency**.

**Recommendation:** Replace `IMemoryCache` with `IDistributedCache` (Redis) for permission checks, role lookups.

---

### P5 — Create a Shared Caching Library

**Why:** Three services (AuthService, MediaService, AuditService) independently defined identical `CacheSettings` classes. There's no `CarDealer.Shared.Caching` library.

**Recommendation:** Create `CarDealer.Shared.Caching` with:

- Unified `CacheSettings` class
- `AddStandardCache(IConfiguration)` extension method (mirrors `AddStandardDatabase`, `AddStandardRabbitMq`)
- `ICacheService<T>` with get/set/invalidate + auto-serialization
- Built-in metrics (cache hit/miss counters)
- Cache key builder utility

---

### P6 — ComparisonService & RecommendationService: Add Caching

**Why:** Both are read-heavy, compute-intensive services:

- Vehicle comparisons require loading multiple vehicles + computing diffs
- Recommendations require scoring algorithms across many vehicles

**Recommendation:** Cache comparison results by vehicle-pair hash (1-hour TTL), recommendation results by user profile hash (15-min TTL).

---

## 5. Shared Caching Pattern Assessment

| Pattern                            | Status             | Services Using                                                                   |
| ---------------------------------- | ------------------ | -------------------------------------------------------------------------------- |
| `AddStackExchangeRedisCache`       | ✅ Used            | AuthService, ChatbotService, SearchAgent                                         |
| `IDistributedCache`                | ✅ Used            | AuthService, ChatbotService, SearchAgent, AuditService (broken)                  |
| `IConnectionMultiplexer` (direct)  | ✅ Used            | AuthService, Shared.Idempotency                                                  |
| `IMemoryCache`                     | ✅ Used            | AuthService, NotificationService, UserService, SupportAgent, Shared.FeatureFlags |
| `AddDistributedMemoryCache` (fake) | ⚠️ Used            | RoleService, SearchAgent (fallback), Shared.ModuleAccess, RecoAgent              |
| `OutputCache` / `ResponseCache`    | ⛔ Not used        | None                                                                             |
| Shared caching library             | ⛔ Does not exist  | N/A                                                                              |
| Cache invalidation via events      | ⛔ Not implemented | N/A                                                                              |

---

## 6. Summary Scorecard

| Metric                               | Value                                                                             |
| ------------------------------------ | --------------------------------------------------------------------------------- |
| Services with Redis (real)           | **3** (AuthService, ChatbotService, SearchAgent)                                  |
| Services with IMemoryCache only      | **5** (NotificationService, UserService, SupportAgent, ErrorService, RoleService) |
| Services with broken/dead cache code | **2** (AuditService, MediaService)                                                |
| Services with ZERO caching           | **12**                                                                            |
| Gateway response caching             | ⛔ **None**                                                                       |
| Shared caching library               | ⛔ **None**                                                                       |
| Cache invalidation strategy          | ⛔ **None** (TTL-only)                                                            |
| **Total services audited**           | **22+**                                                                           |

---

## 7. Quick Win Implementation Order

1. **VehiclesSaleService catalog endpoints** — static data, easiest to cache, highest traffic
2. **Gateway FileCacheOptions** for public GET routes — single config change, protects all services
3. **Wire up AuditService & MediaService** — code already exists, just needs DI registration
4. **KYCService JCE caching** — commented-out code ready to activate
5. **Shared `CarDealer.Shared.Caching`** — reduce duplication, standardize patterns
6. **Cache invalidation via RabbitMQ events** — when vehicle updated → invalidate `vehicle:{id}` cache key

---

_End of audit._
