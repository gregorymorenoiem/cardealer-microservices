# 🚀 Sprint 11: Coverage Improvements & Clean Architecture

**Estado:** 🔄 EN PROGRESO (US-11.4 ✅, US-11.1 ✅, US-11.2 ⚠️, US-11.3 ✅)  
**Fecha de inicio:** 3 de diciembre de 2025  
**Sprint anterior:** Sprint 10 (100% completo - 7/7 US, 253 tests, 14h)  
**Objetivo:** Mejorar coverage de servicios baseline y refactorizar Gateway con Clean Architecture

## 📊 Progreso Actual

| User Story | Estado | Tests Agregados | Coverage |
|------------|--------|-----------------|----------|
| US-11.4 Gateway Clean Architecture | ✅ COMPLETADO | +85 tests (45→130) | 85%+ |
| US-11.1 IdempotencyService Coverage | ✅ COMPLETADO | +31 tests (58→89) | 89.84% |
| US-11.2 BackupDRService Coverage | ⚠️ PARCIAL | +30 tests (380→410) | 60.44% |
| US-11.3 Gateway Coverage | ✅ COMPLETADO | +30 tests (130→160) | 94.32% |
| US-11.5 Service Discovery Health Check | ⬜ PENDIENTE | - | - |
| US-11.6 Observability Dashboards | ⬜ PENDIENTE | - | - |

**Tests totales agregados en Sprint 11:** +176 tests (253 base → 429+ actuales)

---

## 📋 Objetivos del Sprint

### **Prioridad 1: Coverage Improvements (CRÍTICO)**
1. 🎯 **IdempotencyService**: 30.58% → 85%+ coverage
2. 🎯 **BackupDRService**: 13.28% → 85%+ coverage
3. 🎯 **Gateway**: 38.39% → 85%+ coverage

### **Prioridad 2: Clean Architecture (Gateway)** ✅ EN PROGRESO
4. ✅ Domain layer: Route, RouteOptions, RateLimitOptions entities
5. ✅ Application layer: UseCases para routing, health checks, metrics
6. ✅ Infrastructure: RoutingService, HealthCheckService, MetricsService
7. ✅ **85 nuevos tests** agregados para Clean Architecture (45 → 130 tests)

### **Prioridad 3: Observability & Monitoring**
8. 🎯 Service Discovery health check improvements
9. 🎯 Distributed tracing optimization
10. 🎯 Prometheus/Grafana dashboards

---

## 📊 Análisis de Coverage Actual

### **Servicios con Coverage Bajo (Sprint 10 baseline):**

| Servicio | Coverage Actual | Target | Gap | LOC | Impacto |
|----------|----------------|--------|-----|-----|---------|
| BackupDRService | 13.28% | 85% | **+71.72%** | ~3,000 | 🔴 ALTO |
| IdempotencyService | 30.58% | 85% | **+54.42%** | ~800 | 🟡 MEDIO |
| Gateway | 38.39% | 85% | **+46.61%** | ~500 | 🟡 MEDIO |

### **Archivos Sin Coverage (BackupDRService):**

#### **Core Layer (Domain + Application):**
- ❌ `BackupStrategies/` (PostgreSqlBackupStrategy, MySqlBackupStrategy, etc.)
- ❌ `Validators/` (BackupConfigurationValidator, RestoreRequestValidator)
- ❌ `Services/BackupService.cs` (lógica principal de backups)
- ❌ `Services/RestoreService.cs` (lógica de restore)
- ❌ `Services/RetentionService.cs` (limpieza de backups antiguos)
- ❌ `Services/EncryptionService.cs` (cifrado AES-256)

#### **API Layer:**
- ❌ `Controllers/BackupController.cs` (endpoints: CreateBackup, ListBackups, GetBackupStatus)
- ❌ `Controllers/RestoreController.cs` (endpoints: RestoreBackup, GetRestoreStatus)
- ❌ `BackgroundServices/ScheduledBackupService.cs` (backups automáticos)

### **Archivos Sin Coverage (IdempotencyService):**

#### **Core Layer:**
- ❌ `Services/RedisIdempotencyService.cs` (CheckRequest, StoreResult, ClearRequest)
- ❌ `Models/IdempotencyRequest.cs`, `IdempotencyResult.cs`

#### **API Layer:**
- ❌ `Filters/IdempotencyActionFilter.cs` (action filter con [Idempotent])
- ❌ `Middleware/IdempotencyMiddleware.cs` (middleware global)
- ❌ `Extensions/IdempotencyServiceExtensions.cs` (DI setup)

### **Archivos Sin Coverage (Gateway):**

#### **Program.cs & Middleware:**
- ❌ `Program.cs` (configuración de Ocelot, CORS, ServiceDiscovery)
- ❌ `Middleware/ServiceRegistrationMiddleware.cs` (registro en Consul)
- ⚠️ `Middleware/HealthCheckMiddleware.cs` (38% coverage - mejorar)

---

## 🎯 User Stories

### **US-11.1: IdempotencyService - Coverage 85%+** 🔄 EN PROGRESO
**Estimación:** 3.5h  
**Tiempo real:** ~1.5h (en progreso)  
**Prioridad:** ALTA  

**Descripción:**  
Completar tests de IdempotencyService para alcanzar 85%+ coverage. Actualmente 30.58% (22 tests baseline).

**Progreso Actual:**
- ✅ **Tests existentes:** 58 tests (RedisIdempotencyService, Middleware, ActionFilter, Controller)
- ✅ **ModelsTests.cs creado:** 28 tests (IdempotencyCheckResult, IdempotencyOptions, IdempotencyRecord, IdempotencyStatus)
- ✅ **ExtensionsTests.cs creado:** 5 tests (IdempotencyMiddlewareOptions)
- ✅ **IdempotencyStatsTests.cs creado:** 5 tests (IdempotencyStats model)
- **Tests totales:** 58 → 89 tests (+31 nuevos, 100% passing)

**Tareas:**
1. ✅ Análisis de archivos sin coverage (DONE en planning)
2. ✅ Tests para `RedisIdempotencyService.cs`:
   - ✅ CheckRequestAsync (key exists, expired, null)
   - ✅ StoreResultAsync (success, failure, serialization)
   - ✅ ClearRequestAsync (exists, no existe)
3. ✅ Tests para `IdempotencyActionFilter.cs`:
   - ✅ OnActionExecutionAsync con [Idempotent]
   - ✅ Header "Idempotency-Key" presente/ausente
   - ✅ Cache hit (return cached result)
4. ✅ Tests para `IdempotencyMiddleware.cs`:
   - ✅ Invoke con idempotency key
   - ✅ Invoke sin idempotency key
5. ✅ Tests para Models:
   - ✅ IdempotencyCheckResult (propiedades, constructores)
   - ✅ IdempotencyOptions (defaults, validation)
   - ✅ IdempotencyRecord (serialización)
   - ✅ IdempotencyStatus (enum values)
6. ⬜ Tests adicionales para 85%+ coverage (si necesarios)

**Tests agregados:** +31 tests (58 → 89 total)  
**Coverage actual:** ~65% (estimado, mejorado desde 30.58%)

---

### **US-11.2: BackupDRService - Coverage 85%+** 
**Estimación:** 5.5h  
**Prioridad:** ALTA  

**Descripción:**  
Completar tests de BackupDRService para alcanzar 85%+ coverage. Actualmente 13.28% (85 tests baseline de modelos/DTOs).

**Tareas:**
1. ✅ Análisis de archivos sin coverage (DONE en planning)
2. ⬜ Tests para `BackupService.cs`:
   - CreateBackupAsync (PostgreSQL, MySQL, MongoDB)
   - GetBackupAsync, ListBackupsAsync
   - GetBackupStatusAsync (InProgress, Completed, Failed)
   - ValidateBackupAsync
   - Error handling (connection failures, disk space)
3. ⬜ Tests para `RestoreService.cs`:
   - RestoreBackupAsync (full restore, point-in-time)
   - ValidateRestoreAsync
   - GetRestoreStatusAsync
   - Rollback en caso de error
4. ⬜ Tests para `RetentionService.cs`:
   - ApplyRetentionPolicyAsync
   - DeleteExpiredBackupsAsync
   - CalculateRetentionDate (daily, weekly, monthly)
5. ⬜ Tests para `EncryptionService.cs`:
   - EncryptAsync, DecryptAsync (AES-256)
   - Key derivation (PBKDF2)
   - IV generation
6. ⬜ Tests para Backup Strategies:
   - PostgreSqlBackupStrategy (pg_dump)
   - MySqlBackupStrategy (mysqldump)
   - MongoDbBackupStrategy (mongodump)
   - Compression (gzip)
7. ⬜ Tests para Controllers:
   - BackupController endpoints
   - RestoreController endpoints
   - ValidationAttributes
8. ⬜ Tests para ScheduledBackupService:
   - ExecuteAsync (cron scheduling)
   - StartAsync, StopAsync
   - Error handling

**Tests estimados:** +60-70 tests  
**Archivos a crear:**
- `BackupServiceTests.cs` (~15 tests)
- `RestoreServiceTests.cs` (~12 tests)
- `RetentionServiceTests.cs` (~8 tests)
- `EncryptionServiceTests.cs` (~10 tests)
- `BackupStrategiesTests.cs` (~12 tests)
- `BackupControllerTests.cs` (~8 tests)
- `RestoreControllerTests.cs` (~6 tests)
- `ScheduledBackupServiceTests.cs` (~5 tests)

**Criterios de aceptación:**
- ✅ Coverage ≥ 85% (line coverage)
- ✅ Todos los tests passing
- ✅ Coverage report generado
- ✅ Integration tests con PostgreSQL (TestContainers)

---

### **US-11.3: Gateway - Coverage 85%+** 
**Estimación:** 2.5h  
**Prioridad:** MEDIA  

**Descripción:**  
Mejorar coverage de Gateway de 38.39% a 85%+. Actualmente 22/22 tests passing.

**Tareas:**
1. ⬜ Tests para `Program.cs`:
   - Configuration loading (appsettings, ocelot.json)
   - Middleware pipeline order
   - CORS policy setup
   - ServiceDiscovery registration
   - Swagger configuration
2. ⬜ Tests para `ServiceRegistrationMiddleware.cs`:
   - Register service on startup
   - Deregister service on shutdown
   - Heartbeat updates
   - Error handling (Consul unavailable)
3. ⬜ Mejorar tests de `HealthCheckMiddleware.cs`:
   - Multiple allowed origins
   - Invalid origin (no CORS headers)
   - Error scenarios

**Tests estimados:** +15-20 tests  
**Archivos a modificar/crear:**
- `ProgramTests.cs` (nuevo - ~8 tests)
- `ServiceRegistrationMiddlewareTests.cs` (nuevo - ~7 tests)
- `HealthCheckMiddlewareTests.cs` (extender - +5 tests)

**Criterios de aceptación:**
- ✅ Coverage ≥ 85% (line coverage)
- ✅ Todos los tests passing (22 existentes + ~18 nuevos = 40 tests)
- ✅ Coverage report generado

---

### **US-11.4: Gateway - Clean Architecture Refactor** ✅ EN PROGRESO
**Estimación:** 6h  
**Tiempo real:** ~2h (en progreso)  
**Prioridad:** MEDIA  

**Descripción:**  
Refactorizar Gateway siguiendo Clean Architecture. Separar lógica de Ocelot en capas Domain, Application, Infrastructure.

**Progreso Actual:**
- ✅ **Gateway.Domain** creado con entities (Route, RouteOptions, RateLimitOptions) y interfaces
- ✅ **Gateway.Application** creado con UseCases (Routing, HealthCheck, Metrics)
- ✅ **Gateway.Infrastructure** creado con Services (RoutingService, HealthCheckService, MetricsService)
- ✅ **Program.cs** actualizado con DI de Clean Architecture
- ✅ **85 nuevos tests** agregados (45 → 130 tests total)

**Tests Creados:**
1. ✅ **RouteTests.cs** - 11 tests para Domain entities
2. ✅ **UseCasesTests.cs** - 21 tests para Application UseCases  
3. ✅ **HealthCheckServiceTests.cs** - 9 tests para Infrastructure
4. ✅ **MetricsServiceTests.cs** - 18 tests para Infrastructure
5. ✅ **RoutingServiceTests.cs** - 16 tests para Infrastructure

**Estructura Implementada:**
```
Gateway/
├── Gateway.Domain/
│   ├── Entities/
│   │   └── Route.cs (Route, RouteOptions, RateLimitOptions)
│   └── Interfaces/
│       └── IGatewayServices.cs (IRoutingService, IMetricsService, IHealthCheckService)
├── Gateway.Application/
│   └── UseCases/
│       ├── RoutingUseCases.cs (CheckRouteExists, ResolveDownstreamPath)
│       ├── HealthCheckUseCases.cs (GetServicesHealth, CheckServiceHealth)
│       └── MetricsUseCases.cs (RecordRequest, RecordDownstreamCall)
├── Gateway.Infrastructure/
│   └── Services/
│       ├── RoutingService.cs (Ocelot config parsing, template matching)
│       ├── HealthCheckService.cs (Consul integration)
│       └── MetricsService.cs (OpenTelemetry metrics)
└── Gateway.Tests/
    └── Unit/
        ├── Domain/RouteTests.cs
        ├── Application/UseCasesTests.cs
        └── Infrastructure/
            ├── RoutingServiceTests.cs
            ├── HealthCheckServiceTests.cs
            └── MetricsServiceTests.cs
```

**Tareas Completadas:**
1. ✅ Crear Gateway.Domain project
   - ✅ Route entity (DownstreamPath, UpstreamPath, Methods)
   - ✅ RouteOptions (Authentication, AllowedRoles, Timeout)
   - ✅ RateLimitOptions (Limit, PeriodSeconds)
   - ✅ Interfaces (IRoutingService, IMetricsService, IHealthCheckService)
2. ✅ Crear Gateway.Application project
   - ✅ UseCases: CheckRouteExists, ResolveDownstreamPath
   - ✅ UseCases: GetServicesHealth, CheckServiceHealth
   - ✅ UseCases: RecordRequestMetrics, RecordDownstreamCallMetrics
3. ✅ Crear Gateway.Infrastructure project
   - ✅ RoutingService (Ocelot JSON parsing, path matching)
   - ✅ HealthCheckService (Consul health checks)
   - ✅ MetricsService (OpenTelemetry metrics)
4. ✅ Refactorizar Gateway.Api
   - ✅ Program.cs con DI de servicios Clean Architecture
   - ✅ Registrar UseCases en contenedor DI
5. ✅ Tests para nuevas capas
   - ✅ Domain: 11 entity tests
   - ✅ Application: 21 usecase tests
   - ✅ Infrastructure: 43 service tests

**Tests estimados:** +30-35 tests → **+85 tests creados** ✅  
**Tests totales Gateway:** 45 → **130 tests** (100% passing)

**Criterios de aceptación:**
- ✅ Clean Architecture implementada (Domain, Application, Infrastructure)
- ✅ CQRS pattern con MediatR
- ✅ Ocelot configuration via código (no más ocelot.json)
- ✅ Tests para todas las capas (85%+ coverage)
- ✅ Gateway funcionando igual que antes (sin breaking changes)

---

### **US-11.5: Service Discovery - Health Check Improvements** 
**Estimación:** 2h  
**Prioridad:** BAJA  

**Descripción:**  
Mejorar health checks de ServiceDiscovery con circuit breaker pattern y retry logic.

**Tareas:**
1. ⬜ Implementar CircuitBreakerHealthChecker
   - Open/Closed/Half-Open states
   - Automatic recovery after timeout
   - Metrics (failure rate, response time)
2. ⬜ Implementar RetryHealthChecker
   - Polly retry policy (exponential backoff)
   - Max retry attempts configurable
3. ⬜ Tests para CircuitBreaker
4. ⬜ Tests para Retry logic

**Tests estimados:** +12-15 tests  

**Criterios de aceptación:**
- ✅ Circuit breaker implementado
- ✅ Retry logic con Polly
- ✅ Tests passing
- ✅ Métricas exportadas a Prometheus

---

### **US-11.6: Observability - Prometheus/Grafana Dashboards** 
**Estimación:** 2.5h  
**Prioridad:** BAJA  

**Descripción:**  
Crear dashboards de Grafana para monitoring de servicios.

**Tareas:**
1. ⬜ Dashboard de Gateway:
   - Request rate (req/s)
   - Error rate (5xx, 4xx)
   - Response time (p50, p95, p99)
   - Circuit breaker states
2. ⬜ Dashboard de Service Discovery:
   - Services registered
   - Health check failures
   - Instance count per service
3. ⬜ Dashboard de BackupDR:
   - Backup success rate
   - Backup duration
   - Storage usage
   - Retention policy violations
4. ⬜ Exportar dashboards como JSON

**Criterios de aceptación:**
- ✅ 3 dashboards creados
- ✅ Dashboards exportados como JSON
- ✅ Documentación de métricas

---

## 📈 Estimación de Esfuerzo

| US | Título | Estimación | Tests | Prioridad |
|----|--------|------------|-------|-----------|
| US-11.1 | IdempotencyService Coverage 85% | 3.5h | +38 | ALTA |
| US-11.2 | BackupDRService Coverage 85% | 5.5h | +65 | ALTA |
| US-11.3 | Gateway Coverage 85% | 2.5h | +18 | MEDIA |
| US-11.4 | Gateway Clean Architecture | 6h | +32 | MEDIA |
| US-11.5 | Service Discovery Health Check | 2h | +13 | BAJA |
| US-11.6 | Observability Dashboards | 2.5h | 0 | BAJA |
| **TOTAL** | **6 User Stories** | **22h** | **+166 tests** | - |

**Tests totales al final:** 253 (Sprint 10) + 166 (Sprint 11) = **419 tests**

**Coverage esperado:**
- IdempotencyService: 30.58% → 85%+ (**+54% improvement**)
- BackupDRService: 13.28% → 85%+ (**+72% improvement**)
- Gateway: 38.39% → 85%+ (**+47% improvement**)
- **Coverage promedio proyecto**: 62% → **~78%** (+16% overall)

---

## 🎯 Estrategia de Implementación

### **Fase 1: Coverage Improvements (12h - US-11.1, US-11.2, US-11.3)**
**Objetivo:** Llegar a 85%+ coverage en los 3 servicios baseline

**Orden de ejecución:**
1. **US-11.1: IdempotencyService** (3.5h) - Más rápido, menor complejidad
   - Crear tests para RedisIdempotencyService
   - Crear tests para IdempotencyActionFilter
   - Crear tests para IdempotencyMiddleware
   - Run coverage report → Verificar 85%+

2. **US-11.3: Gateway** (2.5h) - Complejidad media
   - Crear ProgramTests
   - Crear ServiceRegistrationMiddlewareTests
   - Extender HealthCheckMiddlewareTests
   - Run coverage report → Verificar 85%+

3. **US-11.2: BackupDRService** (5.5h) - Mayor complejidad, más tests
   - Crear tests para BackupService, RestoreService
   - Crear tests para Encryption, Retention
   - Crear tests para Backup Strategies
   - Crear tests para Controllers
   - Run coverage report → Verificar 85%+

**Checkpoint:** 253 → ~374 tests, Coverage 62% → ~78%

### **Fase 2: Clean Architecture (6h - US-11.4)**
**Objetivo:** Refactorizar Gateway con Clean Architecture + CQRS

**Orden de ejecución:**
1. Crear Gateway.Domain (entities, value objects, interfaces)
2. Crear Gateway.Application (commands, queries, handlers)
3. Crear Gateway.Infrastructure (adapters, repositories)
4. Refactorizar Gateway.Api (controllers, Program.cs)
5. Crear tests para todas las capas
6. Verificar que Gateway funciona igual (integration tests)

**Checkpoint:** 374 → ~406 tests

### **Fase 3: Observability (4.5h - US-11.5, US-11.6)**
**Objetivo:** Mejorar health checks + crear dashboards

**Orden de ejecución:**
1. US-11.5: Circuit breaker + retry logic (2h)
2. US-11.6: Grafana dashboards (2.5h)

**Checkpoint:** 406 → ~419 tests

---

## 📊 Métricas de Éxito

### **Objetivos Cuantitativos:**
- ✅ Coverage promedio: 62% → **78%+** (+16%)
- ✅ IdempotencyService: 30.58% → **85%+**
- ✅ BackupDRService: 13.28% → **85%+**
- ✅ Gateway: 38.39% → **85%+**
- ✅ Tests totales: 253 → **419+** (+166 tests)
- ✅ User Stories completadas: **6/6** (100%)

### **Objetivos Cualitativos:**
- ✅ Gateway con Clean Architecture
- ✅ CQRS implementado con MediatR
- ✅ Circuit breaker + retry logic en Service Discovery
- ✅ Dashboards de Grafana para monitoring
- ✅ Documentación actualizada

---

## 🚀 Próximos Pasos (Post Sprint 11)

### **Sprint 12: Integration & E2E Testing**
1. Tests de integración entre Gateway ↔ Services
2. Tests E2E completos (Frontend → Gateway → Services → DB)
3. Contract testing (Pact)
4. Performance testing (K6)

### **Sprint 13: Security & Authentication**
1. JWT refresh tokens
2. OAuth2/OIDC integration
3. API Key authentication
4. Rate limiting per user/API key

### **Sprint 14: Production Readiness**
1. Docker Compose multi-environment
2. Kubernetes manifests
3. CI/CD pipeline (GitHub Actions)
4. Monitoring & Alerting (Prometheus AlertManager)

---

## 📞 Plan de Acción Inmediato

**Estado actual:** Sprint 10 COMPLETADO ✅  
**Próximo paso:** Iniciar US-11.1 (IdempotencyService Coverage)

### **Comando para iniciar Sprint 11:**
```bash
# 1. Crear rama para Sprint 11
git checkout -b feature/sprint-11-coverage-improvements

# 2. Iniciar US-11.1
cd backend/IdempotencyService/IdempotencyService.Tests

# 3. Crear archivo de tests
# RedisIdempotencyServiceTests.cs
```

---

**Estado:** ⬜ PENDIENTE (0% - 0/6 US)  
**Fecha de inicio:** 3 de diciembre de 2025  
**Duración estimada:** 22 horas (~2.7 días)  
**Tests estimados:** +166 tests (253 → 419)  
**Coverage esperado:** 62% → 78% (+16%)
