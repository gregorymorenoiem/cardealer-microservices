# 🏗️ Reporte de Auditoría de Arquitectura — OKLA Platform

**Sprint:** 16  
**Fecha:** 2026-03-06  
**Autor:** CPSO (Copilot)  
**Alcance:** Backend (25 servicios), Frontend (Next.js 16), Kubernetes (DOKS), CI/CD

---

## 📋 Resumen Ejecutivo

La plataforma OKLA opera con **25 microservicios .NET 8**, un frontend **Next.js 16**, y se despliega en **DigitalOcean Kubernetes Service (DOKS)**. La auditoría revela un ecosistema funcional con adopción progresiva de las librerías compartidas (`_Shared/`), pero con **brechas significativas en estandarización** — especialmente en los servicios más nuevos.

### Métricas Clave

| Métrica                                  | Valor                   |
| ---------------------------------------- | ----------------------- |
| Servicios totales                        | 25                      |
| Totalmente conformes                     | 7 (28%)                 |
| Con problemas críticos de seguridad      | 1 (AIProcessingService) |
| Health checks completos (3 endpoints)    | 12 de 25 (48%)          |
| Usando extensiones compartidas completas | 10 de 25 (40%)          |
| Deployments activos en K8s               | 15 de 47 (32%)          |
| Rutas frontend                           | 43 públicas + 32 admin  |

---

## 1. 🔴 Hallazgos Críticos de Seguridad

### 1.1 Secreto JWT Hardcodeado — AIProcessingService

**Severidad:** 🔴 CRÍTICA (CWE-798)

```csharp
// AIProcessingService.Api/Program.cs ~línea 80
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? "OKLA-SuperSecretKey-2026-CarDealer-Microservices-256bit";
```

**Impacto:** El secreto JWT está en el código fuente, committed en git. Cualquier persona con acceso al repositorio puede forjar tokens válidos.

**Remediación:** Asignado en Tarea 16.1 (Desarrollador Senior). Reemplazar con `throw new InvalidOperationException`.

### 1.2 Fallbacks JWT Hardcodeados en Múltiples Servicios

**Severidad:** 🟠 ALTA

| Servicio                   | Variable afectada            | Valor hardcodeado                       |
| -------------------------- | ---------------------------- | --------------------------------------- |
| VehiclesSaleService        | `Jwt:Issuer`, `Jwt:Audience` | `"CarDealerPlatform"`, `"CarDealerAPI"` |
| BillingService             | `Jwt:Issuer`, `Jwt:Audience` | Valor genérico, `"OKLA-Dev"`            |
| DealerAnalyticsService     | `Jwt:Issuer`, `Jwt:Audience` | Valor genérico, `"OKLA-Dev"`            |
| VehicleIntelligenceService | `Jwt:Issuer`, `Jwt:Audience` | `"VehicleIntelligenceServiceUsers"`     |
| CRMService                 | `Jwt:Issuer`, `Jwt:Audience` | Valor genérico, `"OKLA-Dev"`            |
| RecommendationService      | `JwtSettings:SecretKey`      | Fallback con valor por defecto          |

### 1.3 Inconsistencia en Paths de Configuración JWT

**Severidad:** 🟡 MEDIA

Se detectaron **5 variantes distintas** de paths de configuración JWT:

| Path                                  | Servicios                                   |
| ------------------------------------- | ------------------------------------------- |
| `AddMicroserviceSecrets()` + estándar | 15 servicios ✅                             |
| `Jwt:Secret` con fallback             | AIProcessingService, DealerAnalyticsService |
| `JWT:SecretKey`                       | ReviewService                               |
| `JwtSettings:SecretKey`               | RecommendationService                       |
| Sin autenticación JWT                 | AuditService, ReportsService                |

---

## 2. 📊 Matriz de Conformidad — Backend

### 2.1 Adopción de Extensiones Compartidas

| Servicio                   | Serilog | Observability | ErrorHandling | Audit | MediatR | FluentVal | Tier |
| -------------------------- | ------- | ------------- | ------------- | ----- | ------- | --------- | ---- |
| AdminService               | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| AuthService                | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| ContactService             | ✅      | ✅            | ✅            | ✅    | ❌      | ❌        | 1\*  |
| MediaService               | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| NotificationService        | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| RecoAgent                  | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| SearchAgent                | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| SupportAgent               | ✅      | ✅            | ✅            | ✅    | ✅      | ✅        | 1    |
| ErrorService               | ⚠️      | ⚠️            | ⚠️            | ✅    | ✅      | ✅        | 2    |
| Gateway                    | ✅      | ✅            | ✅            | ✅    | ❌      | ❌        | 2    |
| BillingService             | ✅      | ✅            | ✅            | ✅    | ❌      | ✅        | 2    |
| UserService                | ✅      | ✅            | ✅            | ❌    | ✅      | ✅        | 3    |
| RoleService                | ⚠️      | ⚠️            | ⚠️            | ✅    | ✅      | ✅        | 3    |
| VehiclesSaleService        | ✅      | ✅            | ✅            | ❌    | ❌      | ❌        | 3    |
| CRMService                 | ⚠️      | ❌            | ❌            | ✅    | ✅      | ✅        | 3    |
| KYCService                 | ❌      | ❌            | ❌            | ❌    | ✅      | ✅        | 4    |
| AuditService               | ⚠️      | ⚠️            | ❌            | N/A   | ❌      | ❌        | 4    |
| ReviewService              | ⚠️      | ❌            | ❌            | ❌    | ✅      | ✅        | 4    |
| ChatbotService             | ⚠️      | ❌            | ❌            | ❌    | ✅      | ✅        | 4    |
| DealerAnalyticsService     | ❌      | ❌            | ❌            | ❌    | ✅      | ❌        | 5    |
| ComparisonService          | ❌      | ❌            | ❌            | ❌    | ❌      | ❌        | 5    |
| AIProcessingService        | ❌      | ❌            | ❌            | ❌    | ✅      | ❌        | 5    |
| VehicleIntelligenceService | ❌      | ❌            | ❌            | ❌    | ✅      | ❌        | 5    |
| RecommendationService      | ❌      | ❌            | ❌            | ❌    | ✅      | ❌        | 5    |
| ReportsService             | ⚠️      | ❌            | ❌            | ❌    | ❌      | ❌        | 5    |

**Leyenda:** ✅ = Usa extensión compartida | ⚠️ = Implementación manual/parcial | ❌ = Ausente

### 2.2 Tiers de Conformidad

| Tier                          | Descripción                                                      | Servicios | %   |
| ----------------------------- | ---------------------------------------------------------------- | --------- | --- |
| **1** — Totalmente Conforme   | Todas las extensiones compartidas + health checks + JWT estándar | 8         | 32% |
| **2** — Mayormente Conforme   | Extensiones presentes, brechas menores                           | 3         | 12% |
| **3** — Parcialmente Conforme | Algunas extensiones, brechas significativas                      | 4         | 16% |
| **4** — Baja Conformidad      | Pocas extensiones compartidas                                    | 4         | 16% |
| **5** — Mínima/Nula           | Setup bare-bones                                                 | 6         | 24% |

---

## 3. 🏥 Health Checks

### 3.1 Conformidad de Endpoints

El estándar OKLA requiere 3 endpoints:

- `/health` — excluye tag `"external"` (para K8s liveness probes)
- `/health/ready` — verifica dependencias externas (para K8s readiness probes)
- `/health/live` — predicate `_ => false` (siempre healthy si el proceso responde)

| Estado                          | Servicios                                                                                                                                                                                                                                     |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ✅ 3 endpoints conformes        | AdminService, AuthService, ContactService, ErrorService, MediaService, NotificationService, ReviewService, RecoAgent, SearchAgent, SupportAgent (10)                                                                                          |
| ⚠️ 3 endpoints con problemas    | AuditService (predicate incorrecto en `/health`), Gateway (custom middleware)                                                                                                                                                                 |
| 🔴 Solo `/health` sin predicate | KYCService, VehiclesSaleService, BillingService, UserService, RoleService, ComparisonService, ChatbotService, AIProcessingService, DealerAnalyticsService, VehicleIntelligenceService, RecommendationService, CRMService, ReportsService (13) |

**Riesgo:** Los 13 servicios sin predicate en `/health` ejecutan ALL health checks incluyendo DB/Redis/RabbitMQ. Si una dependencia externa cae, K8s reiniciará los pods innecesariamente.

---

## 4. 🖥️ Frontend — Next.js 16

### 4.1 Estructura y Escala

| Componente              | Cantidad |
| ----------------------- | -------- |
| Rutas públicas `(main)` | 43       |
| Rutas admin `(admin)`   | 32       |
| Rutas auth `(auth)`     | 10       |
| BFF endpoints `api/`    | 16       |
| Grupos de componentes   | 27       |
| Archivos de servicios   | 46       |
| Custom hooks            | 37       |
| Tests de servicios      | 8        |

### 4.2 Stack Tecnológico

| Tecnología        | Versión   | Estado           |
| ----------------- | --------- | ---------------- |
| Next.js           | 16.1.6    | ✅ App Router    |
| React             | 19.2.3    | ✅               |
| TypeScript        | —         | ✅               |
| TanStack Query    | 5.90.20   | ✅ Server state  |
| react-hook-form   | 7.71.1    | ✅               |
| zod               | 4.3.6     | ✅               |
| Tailwind CSS      | v4        | ✅               |
| shadcn/ui (Radix) | Múltiples | ✅               |
| Vitest            | —         | ✅ (no Jest)     |
| Playwright        | 1.58.1    | ✅ E2E           |
| pnpm              | 10.28.2   | ✅ (no npm/yarn) |

### 4.3 Observaciones

- ✅ **Seguridad implementada**: CSRF (`csrf.tsx`), sanitización XSS (`sanitize.ts`), rate limiting (`rate-limit.ts`), device fingerprint
- ✅ **SEO**: `robots.ts`, `sitemap.ts`, `og/` para Open Graph
- ✅ **Performance**: Bundle analyzer configurado, image optimization con `sharp`
- ✅ **PWA**: Componentes PWA en `src/components/pwa/`
- ⚠️ **Zustand eliminado**: Se removió como dependencia muerta (Sprint 16, Tarea 16.6)
- ⚠️ **Test coverage limitada**: Solo 8 archivos de test para 46 servicios (17% cobertura de archivos de servicio)

---

## 5. ☸️ Kubernetes (DOKS)

### 5.1 Infraestructura

| Recurso                                  | Detalle                             |
| ---------------------------------------- | ----------------------------------- |
| Nodos                                    | 2× `s-4vcpu-8gb` (autoscale a 3)    |
| Namespace                                | `okla`                              |
| Deployments definidos                    | 47                                  |
| Deployments activos (replicas ≥ 1)       | 15                                  |
| Deployments deshabilitados (replicas: 0) | 32                                  |
| Ingress                                  | Solo `frontend-web` → `okla.com.do` |
| TLS                                      | cert-manager + Let's Encrypt        |

### 5.2 Conformidad K8s

| Regla                                                 | Estado                                |
| ----------------------------------------------------- | ------------------------------------- |
| Todos los servicios en puerto 8080                    | ✅ Conforme                           |
| Gateway NO expuesto externamente                      | ✅ Conforme                           |
| Security contexts (runAsNonRoot, readOnly, drop caps) | ✅ Conforme                           |
| Topology spread constraints                           | ✅ Conforme                           |
| Registry credentials                                  | ✅ GHCR configurado                   |
| Tags de imagen inmutables (SHA)                       | 🔴 No conforme — todos usan `:latest` |
| HPA configurado                                       | ✅ Para servicios principales         |
| PDB configurado                                       | ✅ Para servicios críticos            |
| Network Policies                                      | ✅ Segmentación definida              |

### 5.3 Observaciones de Cluster (2026-03-06)

| Observación                    | Severidad | Detalle                                                     |
| ------------------------------ | --------- | ----------------------------------------------------------- |
| Pod RabbitMQ duplicado         | ⚠️ Media  | `rabbitmq-5c69876567-j64lp` atascado en `ContainerCreating` |
| Pod `psql-test` activo 13 días | ⚠️ Baja   | Pod de debug consumiendo recursos                           |
| Metrics API no disponible      | ⚠️ Media  | `kubectl top` falla — metrics-server no instalado           |
| BillingService con 2 restarts  | ℹ️ Info   | Posible OOM o startup lento                                 |

---

## 6. 🔄 CI/CD (GitHub Actions)

### 6.1 Workflows

| Workflow              | Propósito                                                | Estado    |
| --------------------- | -------------------------------------------------------- | --------- |
| `ci-cd-pipeline.yml`  | Orquestador principal — detecta cambios, build selectivo | ✅ Activo |
| `dotnet-service.yml`  | Template reutilizable para .NET                          | ✅        |
| `nextjs-frontend.yml` | Template para frontend (pnpm)                            | ✅        |
| `pr-validation.yml`   | Validación rápida de PRs                                 | ✅        |
| `security-scan.yml`   | Gitleaks + NuGet vulns (semanal + PRs)                   | ✅        |

### 6.2 Observaciones CI/CD

- ✅ **Monorepo-aware**: Solo construye servicios modificados
- ✅ **Security scanning**: Gitleaks para detección de secretos
- ⚠️ **Self-hosted macOS ARM64 runner**: Riesgo de incompatibilidad de plataforma con DOKS (amd64). El workflow debe usar `--platform linux/amd64` explícitamente.
- 🔴 **Tags `:latest` en producción**: Todos los deployments usan `:latest` en lugar de SHA de commit inmutable

---

## 7. 📚 Librerías Compartidas (`_Shared/`)

Se identificaron **14 proyectos compartidos**:

| Librería                                           | Adopción      | Observaciones      |
| -------------------------------------------------- | ------------- | ------------------ |
| `CarDealer.Shared` (`Result<T>`, `ApiResponse<T>`) | ~18 servicios | Base               |
| `CarDealer.Contracts` (`EventBase`, DTOs)          | ~15 servicios | Eventos            |
| `CarDealer.Shared.Logging` (`UseStandardSerilog`)  | 10 servicios  | 15 usan manual     |
| `CarDealer.Shared.Observability`                   | 10 servicios  | Alta deuda técnica |
| `CarDealer.Shared.ErrorHandling`                   | 10 servicios  | Alta deuda técnica |
| `CarDealer.Shared.Audit`                           | ~12 servicios |                    |
| `CarDealer.Shared.HealthChecks`                    | ~10 servicios | 13 sin adoptar     |
| `CarDealer.Shared.RateLimiting`                    | Pocos         |                    |
| `CarDealer.Shared.Resilience`                      | Pocos         |                    |
| `CarDealer.Shared.FeatureFlags`                    | Pocos         |                    |
| `CarDealer.Shared.Idempotency`                     | Mínimo        |                    |
| `CarDealer.Shared.Sagas`                           | Mínimo        |                    |
| `CarDealer.Shared.ApiVersioning`                   | Mínimo        |                    |

---

## 8. 📈 Recomendaciones Priorizadas

### Prioridad 1 — Seguridad (inmediato)

| #   | Acción                                 | Servicios Afectados                                                                                                        |
| --- | -------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| S1  | Eliminar secreto JWT hardcodeado       | AIProcessingService                                                                                                        |
| S2  | Eliminar fallbacks JWT hardcodeados    | VehiclesSaleService, BillingService, DealerAnalyticsService, VehicleIntelligenceService, CRMService, RecommendationService |
| S3  | Estandarizar path de configuración JWT | ReviewService (`JWT:SecretKey`), RecommendationService (`JwtSettings:SecretKey`)                                           |

### Prioridad 2 — Estabilidad (Sprint 16-17)

| #   | Acción                                                    | Servicios Afectados   |
| --- | --------------------------------------------------------- | --------------------- |
| E1  | Agregar health checks conformes (3 endpoints + predicate) | 13 servicios Tier 3-5 |
| E2  | Instalar metrics-server en DOKS                           | Cluster               |
| E3  | Limpiar pod RabbitMQ duplicado                            | Cluster               |
| E4  | Limpiar pod `psql-test`                                   | Cluster               |
| E5  | Migrar a tags de imagen inmutables (SHA)                  | Todos los deployments |

### Prioridad 3 — Estandarización (Sprint 17-19)

| #   | Acción                                           | Servicios Afectados                             |
| --- | ------------------------------------------------ | ----------------------------------------------- |
| D1  | Migrar a `UseStandardSerilog()`                  | 15 servicios con Serilog manual/ausente         |
| D2  | Agregar `AddStandardObservability()`             | 15 servicios                                    |
| D3  | Agregar `AddStandardErrorHandling()`             | 15 servicios                                    |
| D4  | Agregar `AddAuditPublisher()`                    | 11 servicios                                    |
| D5  | Registrar MediatR + FluentValidation donde falta | 6 servicios sin MediatR, 9 sin FluentValidation |

### Prioridad 4 — Calidad (Sprint 19-20)

| #   | Acción                                      | Detalle                                                         |
| --- | ------------------------------------------- | --------------------------------------------------------------- |
| Q1  | Aumentar cobertura de tests frontend        | 8/46 servicios testeados (17%)                                  |
| Q2  | Agregar startup tests a servicios sin tests | Identificados en Tarea 16.4                                     |
| Q3  | Eliminar rutas stale en Ocelot              | Validar que todos los downstream hosts tengan deployment activo |

---

## 9. 🎯 Plan de Estandarización Estimado

```
Sprint 16 (actual):
  ├── S1: Eliminar hardcoded secret (AIProcessingService) ← Tarea 16.1
  ├── D1-D3: Estandarizar ReviewService ← Tarea 16.2
  ├── D1-D3 + E1: Estandarizar DealerAnalyticsService ← Tarea 16.3
  └── Q2: Tests unitarios para 5 servicios ← Tarea 16.4

Sprint 17:
  ├── S2: Eliminar todos los fallbacks JWT restantes
  ├── E1: Health checks conformes en 13 servicios
  ├── E5: Migrar a tags de imagen SHA
  └── D1-D3: Estandarizar KYCService, ChatbotService, AuditService

Sprint 18:
  ├── D1-D4: Estandarizar ComparisonService, CRMService, ReportsService
  ├── D5: Registrar MediatR/FluentValidation donde falta
  └── E2: Instalar metrics-server

Sprint 19-20:
  ├── Q1: Aumentar test coverage frontend
  └── Q3: Limpieza de rutas Ocelot
```

---

## 10. Apéndice — Gateway Ocelot

### Servicios Enrutados (24)

`adminservice`, `aiprocessingservice`, `auditservice`, `authservice`, `azulpaymentservice`, `billingservice`, `chatbotservice`, `comparisonservice`, `contactservice`, `crmservice`, `errorservice`, `kycservice`, `mediaservice`, `notificationservice`, `recoagent`, `recommendationservice`, `reportsservice`, `reviewservice`, `roleservice`, `searchagent`, `supportagent`, `userservice`, `vehicleintelligenceservice`, `vehiclessaleservice`

### Posibles Rutas Stale

Servicios con rutas en Ocelot pero **sin deployment activo** (replicas: 0):

- `aiprocessingservice`, `comparisonservice`, `crmservice`, `recommendationservice`, `reportsservice`, `searchagent`, `supportagent`, `vehicleintelligenceservice`

> Nota: Estos servicios pueden estar intencionalmente deshabilitados. Verificar si las rutas devuelven 503 cuando se acceden.

---

_Fin del reporte. Próxima auditoría programada: Sprint 18._
