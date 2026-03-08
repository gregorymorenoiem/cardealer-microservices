# Sprint 18 — Closure Report (CPSO Solo Implementation)

**Fecha:** 2026-03-06
**Responsable:** CPSO (100% implementación)
**Estado:** ✅ COMPLETADO

---

## Contexto

A partir de este sprint, el CPSO asumió el 100% de la responsabilidad de implementación técnica. No existe un Desarrollador Senior en el equipo. Todas las tareas fueron implementadas directamente por el CPSO.

---

## Tareas Completadas

### 18.1 ✅ Fix AIProcessingService — Credenciales Hardcoded

**Archivo:** `AIProcessingService.Api/Program.cs`

- **JWT Secret:** Eliminado fallback `"OKLA-SuperSecretKey-2026-CarDealer-Microservices-256bit"` → `throw InvalidOperationException`
- **JWT Issuer:** Eliminado fallback `"OKLA"` → `throw InvalidOperationException`
- **JWT Audience:** Eliminado fallback `"OKLA-Users"` → `throw InvalidOperationException`
- **DB Connection:** Eliminado fallback `"Host=postgres;...Username=postgres;Password=postgres"` → `throw InvalidOperationException`
- **Impacto:** P0 — eliminadas 4 credenciales hardcoded que podían ser explotadas en producción

### 18.2 ✅ Fix Gateway — RabbitMQ "guest" Credentials

**Archivo:** `Gateway.Api/Program.cs`

- **RabbitMQ User:** Eliminado fallback `"guest"` → `throw InvalidOperationException`
- **RabbitMQ Password:** Eliminado fallback `"guest"` → `throw InvalidOperationException`
- **Impacto:** P0 — "guest" es el default de RabbitMQ y tiene acceso completo

### 18.3 ✅ Fix VehiclesSaleService — Connection String Leak

**Archivo:** `VehiclesSaleService.Api/Program.cs`

- **Eliminado:** `Console.WriteLine($"🗄️  Database: {connectionString}")` que exponía credenciales en stdout/logs
- **Impacto:** P0 — connection strings con passwords aparecían en CloudWatch/container logs

### 18.4 ✅ Fix /health — Exclude External Checks

**Archivo:** `_Shared/CarDealer.Shared.HealthChecks/Extensions/HealthCheckExtensions.cs`

- **Agregado:** `Predicate = check => !check.Tags.Contains("external")` al endpoint `/health`
- **Impacto:** P1 — evita que checks de servicios externos (APIs de terceros) causen timeouts en K8s health probes

### 18.5 ✅ Update ServiceNames Enum

**Archivo:** `_Shared/CarDealer.Contracts/Enums/ServiceNames.cs`

- **Antes:** 9 servicios
- **Después:** 27 servicios (todos los del sistema)
- **Agregados:** AIProcessingService, BillingService, ChatbotService, ComparisonService, CRMService, DealerAnalyticsService, KYCService, RecoAgent, RecommendationService, ReportsService, ReviewService, RoleService, SearchAgent, SearchService, SupportAgent, UserService, VehicleIntelligenceService, VehiclesSaleService
- **Impacto:** P2 — necesario para logging estructurado y event routing consistente

### 18.6 ✅ Skip-to-Content + Main Landmark (Ya Existente)

**Archivos verificados:**

- `frontend/web-next/src/app/layout.tsx` — tiene skip-to-content link con patrón `sr-only`
- `frontend/web-next/src/components/layout/main-layout-shell.tsx` — tiene skip-to-content link con patrón `fixed + translate-y` Y `<main id="main-content">`
- **Estado:** Ya implementado en el codebase actual. No requirió cambios.

---

## Resumen de Impacto

| Métrica                        | Valor                                  |
| ------------------------------ | -------------------------------------- |
| Archivos modificados           | 5                                      |
| Vulnerabilidades P0 eliminadas | 3 (7 credenciales hardcoded removidas) |
| Bug P1 corregido               | 1 (health check probe timeout)         |
| Enum actualizado               | +18 servicios agregados                |
| WCAG verificado                | Skip-to-content + main landmark ✅     |

---

## Archivos Modificados

1. `backend/AIProcessingService/AIProcessingService.Api/Program.cs`
2. `backend/Gateway/Gateway.Api/Program.cs`
3. `backend/VehiclesSaleService/VehiclesSaleService.Api/Program.cs`
4. `backend/_Shared/CarDealer.Shared.HealthChecks/Extensions/HealthCheckExtensions.cs`
5. `backend/_Shared/CarDealer.Contracts/Enums/ServiceNames.cs`

---

## Siguiente Sprint

Con las vulnerabilidades P0 resueltas, el Sprint 19 debe enfocarse en:

1. **Consolidación de servicios faltantes** — estandarizar Program.cs de los 5+ servicios restantes
2. **Tests para shared libraries** — 13/14 libs sin tests
3. **CI/CD hardening** — eliminar `:latest` tags, bloquear Trivy findings
4. **Frontend SEO** — dynamic OG images, sitemap splitting
