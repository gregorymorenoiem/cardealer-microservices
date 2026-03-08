# Sprint 32 Report — Shared Library Tests, Cache Invalidation, Compression, Security Audit

**Fecha:** 2026-03-06
**Estado:** En Progreso (4 of 6 tasks complete)

---

## Resumen Ejecutivo

Sprint 32 completa la cobertura de unit tests para las 6 bibliotecas compartidas restantes que no tenían tests: ApiVersioning, FeatureFlags, Logging, Observability, Sagas y ServiceDiscovery.Domain. Se añadieron **88 nuevos tests** llevando el total de la suite a **171 tests (0 failures)**.

---

## Task 1: Shared Library Test Coverage — COMPLETADO ✅

### Archivos Creados (6 test files)

| Test File                                         | Tests | Library Covered                                                                                                                                            |
| ------------------------------------------------- | ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ApiVersioning/ApiVersioningOptionsTests.cs`      | 10    | ApiVersioningOptions, VersionReaderOptions, SwaggerVersionOptions, ContactInfo, LicenseInfo, ApiV1/V2/V3Attribute, ApiVersionDeprecatedAttribute           |
| `FeatureFlags/FeatureFlagOptionsTests.cs`         | 8     | FeatureFlagOptions, FeatureFlagDto, FeatureFlagEvaluationResult, FeatureFlagContext                                                                        |
| `Logging/LoggingOptionsTests.cs`                  | 10    | LoggingOptions, SeqOptions, RabbitMQLogOptions, FileLogOptions, convenience property delegation                                                            |
| `Observability/ObservabilityOptionsTests.cs`      | 12    | ObservabilityOptions, TracingOptions, MetricsOptions, OtlpExporterOptions, convenience property delegation                                                 |
| `Sagas/SagaOptionsTests.cs`                       | 14    | SagaOptions, RabbitMqOptions (computed ConnectionString), SagaRepositoryOptions, SagaRetryOptions, OutboxOptions, SagaContracts records                    |
| `ServiceDiscovery/ServiceDiscoveryDomainTests.cs` | 22    | ServiceInstance (IsValid, IsHealthy, UpdateHealth, Address), HealthCheckResult (factory methods), ServiceCatalog (register/deregister/query/counts), enums |
| **Fix: rewritten 2 broken files**                 | —     | Previous session's ApiVersioning + FeatureFlags tests referenced nonexistent types                                                                         |

### Test Coverage Summary

| Category              | Tests   | Libraries                                                                                                        |
| --------------------- | ------- | ---------------------------------------------------------------------------------------------------------------- |
| Sprint 30 (Core)      | 56      | EntityBase, DeadLetterEvent, ProblemDetails, HealthChecks, Idempotency, RateLimiting, Audit, Secrets, Resilience |
| Sprint 31 (Caching)   | 27      | CacheOptions, RedisCacheService, CachingExtensions                                                               |
| Sprint 32 (Remaining) | 88      | ApiVersioning, FeatureFlags, Logging, Observability, Sagas, ServiceDiscovery.Domain                              |
| **Total**             | **171** | **15 shared libraries**                                                                                          |

### Key Test Patterns

- **Options default validation**: Every config class tested for correct default values
- **Convenience property delegation**: Verified that flat properties (e.g., `SeqEnabled`, `OtlpEndpoint`) correctly delegate to nested objects
- **Computed properties**: `RabbitMqOptions.ConnectionString` tested with default and custom values
- **Domain logic**: `ServiceInstance.IsValid()` tested with 6 invalid boundary cases (empty ID, empty name, port 0/-1/65536), `IsHealthy()` with 5 status combinations
- **Factory methods**: `HealthCheckResult.Healthy/Unhealthy/Degraded` tested for correct field population
- **CRUD operations**: `ServiceCatalog` register/deregister/query/replace/count operations thoroughly tested
- **Record defaults**: Saga contract records (`SagaFaulted`, `SubmitOrder`, etc.) tested for default timestamps and error codes

### Bug Fixes During Testing

- **ApiVersioningOptionsTests.cs**: `ApiVersioningOptions` name collision between `CarDealer.Shared.ApiVersioning.Configuration` and `Asp.Versioning` — resolved with using alias `VersioningOptions`
- **FeatureFlagOptionsTests.cs**: Previous session assumed types (`FeatureFlagDefinition`, `FeatureFlagResult`, `FlagOverride`) that don't exist — rewrote using actual types (`FeatureFlagDto`, `FeatureFlagEvaluationResult`, `FeatureFlagContext`)

---

## Remaining Sprint 32 Tasks

- [ ] ContactService implementation
- [ ] Frontend vehicle search performance audit

---

## Task 2: Cache Invalidation via RabbitMQ — COMPLETADO ✅

### File Created

- `VehiclesSaleService.Infrastructure/Messaging/CacheInvalidationConsumer.cs`

### Implementation Details

- **BackgroundService** pattern matching existing `CampaignEventsConsumer`
- **Queue**: `vehiclessaleservice.cache-invalidation` (durable, non-exclusive, non-autoDelete)
- **Exchange**: `cardealer.events` (topic, durable — shared with other consumers)
- **Routing keys consumed**: `vehicle.vehicle.created`, `vehicle.vehicle.updated`, `vehicle.vehicle.deleted`, `vehicle.vehicle.sold`, `vehicle.vehicle.published`, `vehicles.vehicle.published`
- **Invalidation strategy**: On any event → invalidate `vehicle:featured:*` + `catalog:makes:*` patterns. If vehicleId present → also remove `vehicle:detail:{id}`
- **Error handling**: Nack without requeue — cache entries expire naturally via TTL, so failed invalidation is non-critical
- **Registration**: `builder.Services.AddHostedService<CacheInvalidationConsumer>()` in Program.cs

### Collateral Fixes

- Added `CarDealer.Shared.Caching` reference to `VehiclesSaleService.Infrastructure.csproj`
- Fixed test compilation: added `ICacheService` mock to `VehicleLifecycleControllerTests` and `VinDecodeControllerTests`

---

## Task 3: Response Compression — COMPLETADO ✅

### Changes

- **Services**: `AddResponseCompression` with Brotli + Gzip providers, `CompressionLevel.Fastest`
- **MIME types**: `application/json`, `text/json`, `application/problem+json` (plus defaults)
- **Middleware**: `app.UseResponseCompression()` placed after error handling, before routing
- **HTTPS**: Enabled (`EnableForHttps = true`)

---

## Task 4: NotificationService Security Audit — COMPLETADO ✅

### Findings: 16 total (2 critical, 4 high, 7 medium, 3 low)

### Fixes Applied (5 findings resolved)

| Finding                                                       | Severity    | Fix                                                                                       |
| ------------------------------------------------------------- | ----------- | ----------------------------------------------------------------------------------------- |
| S-03: NotificationPreferencesController missing `[Authorize]` | 🔴 CRITICAL | Added `[Authorize]` to class                                                              |
| S-10: TeamsController missing `[Authorize]`                   | 🔴 CRITICAL | Added `[Authorize(Policy = "NotificationServiceAdmin")]` to class                         |
| S-01: Admin-alert endpoint no admin restriction               | 🟠 HIGH     | Added `[Authorize(Policy = "NotificationServiceAdmin")]`                                  |
| S-11: Template write ops no admin restriction                 | 🟠 HIGH     | Added admin policy to create, update, delete, activate, deactivate, version (6 endpoints) |
| S-14: Exception details leaked in Teams response              | 🔵 LOW      | Removed `ex.Message` from error response                                                  |

### Remaining Findings (for future sprints)

| Finding          | Severity  | Description                                                       |
| ---------------- | --------- | ----------------------------------------------------------------- |
| S-04             | 🟠 HIGH   | InternalNotificationsController needs API key defense-in-depth    |
| S-07             | 🟠 HIGH   | ScheduledNotifications needs admin-only or user scoping           |
| S-06             | 🟡 MEDIUM | Non-MediatR controllers missing FluentValidation (6 controllers)  |
| S-12             | 🟡 MEDIUM | Template content stored XSS risk                                  |
| S-13             | 🟡 MEDIUM | Webhook signature verification degrades instead of failing closed |
| S-02, S-05, S-09 | 🟡 MEDIUM | IDOR issues on notification status, price alerts, saved searches  |
| S-08             | 🟡 MEDIUM | CronExpression input not validated                                |
| S-15, S-16       | 🔵 LOW    | DTOs defined in controller files (architecture cleanup)           |

---

## Remaining Sprint 32 Tasks

- [ ] ContactService implementation
- [ ] Frontend vehicle search performance audit

---

## Métricas

- **Tests añadidos esta sesión:** 88
- **Total tests:** 171
- **Failures:** 0
- **Build warnings:** 1 (NU1903 — Microsoft.Extensions.Caching.Memory 8.0.0 vulnerability in FeatureFlags transitive dep)
- **Duration:** ~32ms full suite
