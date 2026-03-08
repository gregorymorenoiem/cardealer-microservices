# 📚 OKLA — Auditoría de Shared Libraries

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Scope:** 15 librerías compartidas en `backend/_Shared/` (~15,528 LoC)

---

## 📊 Resumen Ejecutivo

| Métrica             | Valor                        |
| ------------------- | ---------------------------- |
| Total de librerías  | 15                           |
| Líneas de código    | ~15,528                      |
| Con tests unitarios | **1/15** (solo Contracts) ❌ |
| Con extensión DI    | 13/15 ✅                     |
| Calidad promedio    | ⭐⭐⭐⭐ (4/5)               |
| Issues críticos     | 5                            |

---

## 1. Inventario de Librerías

| #   | Librería                       | Propósito                                             | LoC   | Tests | DI Extension | Calidad    |
| --- | ------------------------------ | ----------------------------------------------------- | ----- | ----- | ------------ | ---------- |
| 1   | CarDealer.Shared               | Core: DB, messaging, persistence, secrets, middleware | 4,257 | ❌    | ✅ (5)       | ⭐⭐⭐⭐   |
| 2   | CarDealer.Contracts            | Event contracts, DTOs, EventBase                      | 1,308 | ✅    | N/A          | ⭐⭐⭐⭐⭐ |
| 3   | CarDealer.Shared.Logging       | Serilog: Console, Seq, File, enrichers                | 568   | ❌    | ✅ (3)       | ⭐⭐⭐⭐   |
| 4   | CarDealer.Shared.Observability | OpenTelemetry tracing + metrics + Prometheus          | 512   | ❌    | ✅           | ⭐⭐⭐⭐   |
| 5   | CarDealer.Shared.ErrorHandling | Global exception middleware → ProblemDetails          | 1,154 | ❌    | ✅ (2)       | ⭐⭐⭐⭐⭐ |
| 6   | CarDealer.Shared.HealthChecks  | Liveness/readiness/health: PG, Redis, RabbitMQ        | 436   | ❌    | ✅ (2)       | ⭐⭐⭐⭐⭐ |
| 7   | CarDealer.Shared.Audit         | Audit middleware → RabbitMQ publish                   | 713   | ❌    | ✅ (2)       | ⭐⭐⭐⭐   |
| 8   | CarDealer.Shared.RateLimiting  | Redis/HTTP rate limiting por endpoint                 | 895   | ❌    | ✅ (2)       | ⭐⭐⭐⭐   |
| 9   | CarDealer.Shared.Idempotency   | Redis-based idempotency con atributos                 | 974   | ❌    | ✅ (2)       | ⭐⭐⭐⭐⭐ |
| 10  | CarDealer.Shared.Resilience    | Polly 8: Retry, Circuit Breaker, Bulkhead             | 993   | ❌    | ✅ (2)       | ⭐⭐⭐⭐⭐ |
| 11  | CarDealer.Shared.FeatureFlags  | HTTP client a FeatureToggleService                    | 462   | ❌    | ✅           | ⭐⭐⭐     |
| 12  | CarDealer.Shared.Sagas         | MassTransit saga (VehiclePurchase)                    | 735   | ❌    | ✅ (2)       | ⭐⭐⭐     |
| 13  | CarDealer.Shared.ApiVersioning | Asp.Versioning + Swagger multi-version                | 540   | ❌    | ✅ (2)       | ⭐⭐⭐⭐   |
| 14  | ServiceDiscovery               | Consul-based service discovery                        | 1,483 | ❌    | ❌           | ⭐⭐       |
| 15  | CarDealer.Contracts.Tests      | xUnit tests para Contracts                            | 498   | ✅    | N/A          | ⭐⭐⭐     |

---

## 2. Hallazgos Críticos 🔴

### C1. 13/14 librerías sin tests unitarios

Solo `CarDealer.Contracts.Tests` existe. Librerías con lógica compleja (ErrorHandling, Resilience, Idempotency) no tienen tests. Un bug en una shared library afecta **todos** los servicios.

### C2. Proliferación de conexiones RabbitMQ (3x por servicio)

Tres librerías crean sus propias conexiones RabbitMQ independientes:

- `CarDealer.Shared` → `RabbitMqConnectionManager` (singleton correcto)
- `CarDealer.Shared.ErrorHandling` → Propia ConnectionFactory
- `CarDealer.Shared.Audit` → Propia ConnectionFactory

Con 15+ servicios × 3 conexiones = **45+ conexiones TCP mínimo** a RabbitMQ.

### C3. CarDealer.Shared es un monolito

El paquete core bundlea EF Core, Redis, RabbitMQ, NpgSql, HealthChecks como dependencias directas. Un servicio que solo necesita `Result<T>` arrastra ~11 paquetes NuGet.

### C4. `/health` no excluye tag "external" por default

`UseStandardHealthChecks()` mapea `/health` para incluir TODOS los checks. **Contradice la regla crítica** del proyecto: `/health` debe excluir checks con tag `"external"`.

### C5. Inconsistencia de versiones Polly (v7 vs v8)

- `CarDealer.Shared.FeatureFlags` usa Polly v7 API (`AddPolicyHandler`)
- `CarDealer.Shared.Resilience` usa Polly v8 API (`AddResilienceHandler`)

---

## 3. Warnings ⚠️

### W1. Inconsistencias de versiones NuGet

| Paquete                                  | Versiones encontradas |
| ---------------------------------------- | --------------------- |
| Microsoft.Extensions.DependencyInjection | 8.0.0 y 8.0.1         |
| Microsoft.Extensions.Configuration       | 8.0.0 y 8.0.2         |
| Microsoft.Extensions.Options             | 8.0.0 y 8.0.2         |
| RabbitMQ.Client                          | 6.8.1 y 6.6.0         |
| Polly                                    | v7 API y v8 API       |

**Solución:** Implementar `Directory.Packages.props` para centralización.

### W2. `ServiceName` enum desactualizado

Lista 9 servicios, pero existen 20+. Faltan: BillingService, KYCService, VehiclesSaleService, ReportsService, ReviewService, CRMService, etc.

### W3. `CreateBootstrapLogger()` expuesto sin guardrail

`SerilogExtensions` tiene el método disponible, pero copilot-instructions dice "nunca usarlo con UseStandardSerilog()". Sin verificación runtime.

### W4. Código duplicado en Observability

Los dos overloads de `AddStandardObservability()` tienen ~130 líneas duplicadas que deberían extraerse a un método privado.

### W5. ServiceDiscovery arquitecturalmente diferente

Es el único proyecto con Clean Architecture (3 proyectos). No tiene extensión DI. Consul puede ser innecesario dado que DOKS tiene service discovery nativo por DNS.

---

## 4. Funcionalidad Faltante (Duplicada en Servicios)

| Librería Faltante           | Qué se Duplica                                  | Dónde                            |
| --------------------------- | ----------------------------------------------- | -------------------------------- |
| CarDealer.Shared.Validation | `SecurityValidators.cs` (NoSqlInjection, NoXss) | Copiado en 23+ servicios         |
| CarDealer.Shared.CQRS       | `ValidationBehavior<T>` para MediatR            | Duplicado en Application layers  |
| CarDealer.Shared.Pagination | `PagedResult<T>`, `IPagedQuery`                 | Duplicado en cada servicio       |
| CarDealer.Shared.Caching    | Abstracciones de Redis cache                    | Cada servicio implementa el suyo |
| CarDealer.Shared.JWT        | Validación JWT, extracción de claims            | Duplicado en Auth y Gateway      |

---

## 5. Top 5 Recomendaciones

| #   | Recomendación                                                                                   | Impacto                                | Esfuerzo |
| --- | ----------------------------------------------------------------------------------------------- | -------------------------------------- | -------- |
| 1   | **Tests para shared libs** (ErrorHandling, Resilience, Idempotency min.)                        | Un bug aquí afecta todos los servicios | Alto     |
| 2   | **Unificar conexiones RabbitMQ** — Audit y ErrorHandling deben usar `RabbitMqConnectionManager` | Reduce conexiones de 45+ a 15          | Medio    |
| 3   | **Crear CarDealer.Shared.Validation** con SecurityValidators                                    | Elimina copy-paste en 23+ archivos     | Bajo     |
| 4   | **`Directory.Packages.props`** para centralizar versiones NuGet                                 | Consistencia, facilita upgrades        | Bajo     |
| 5   | **Dividir CarDealer.Shared** en paquetes más pequeños (Core, Database, Messaging)               | Reduce dependencias innecesarias       | Alto     |

---

## 6. Integración con Sprint 18/19

**Sprint 18 CPSO:**

- Crear `CarDealer.Shared.Validation` con SecurityValidators extraídos
- Actualizar `ServiceName` enum en Contracts
- Fix `/health` default para excluir "external"

**Sprint 19 Developer:**

- Unificar conexiones RabbitMQ en Audit y ErrorHandling
- Implementar `Directory.Packages.props`
- Tests para ErrorHandling, Resilience, Idempotency
