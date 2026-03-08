# 📊 OKLA — Reporte de Estado Técnico Q1 2026

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Período:** Q1 2026 (Enero - Marzo)
**Versión:** 1.0

---

## 1. Resumen Ejecutivo

OKLA es un marketplace de vehículos para República Dominicana con **24+ microservicios .NET 8**, un frontend **Next.js 16**, y una app mobile en **Flutter/Dart** (~15-20% paridad). La arquitectura es sólida con Clean Architecture, CQRS parcial, y una suite de 15 shared libraries. Sin embargo, existen **5 vulnerabilidades de seguridad P0**, **3 problemas de performance P0**, y **deuda técnica significativa** en testing (solo 1/15 shared libs tiene tests).

### Scorecard General

| Área                    | Score            | Tendencia                                  |
| ----------------------- | ---------------- | ------------------------------------------ |
| 🏗️ Arquitectura Backend | ⭐⭐⭐⭐ (4/5)   | ↑ Mejorando (CRM, Reports estandarizados)  |
| 🎨 Frontend Web         | ⭐⭐⭐⭐⭐ (5/5) | → Estable (excelente postura de seguridad) |
| 📱 Mobile App           | ⭐⭐ (2/5)       | → Estancada (~15% paridad)                 |
| 🗄️ Base de Datos        | ⭐⭐⭐ (3/5)     | ↓ Riesgo (paginación en memoria, N+1)      |
| 🔄 CI/CD                | ⭐⭐⭐⭐ (4/5)   | → Estable (smart CI, pero SPOF en runner)  |
| 🐳 Docker               | ⭐⭐⭐⭐ (4/5)   | → Estable (LlmServer es outlier)           |
| ☸️ Kubernetes           | ⭐⭐⭐⭐ (4/5)   | → Estable (3 workloads sin hardening)      |
| 📚 Shared Libraries     | ⭐⭐⭐⭐ (4/5)   | ↓ Riesgo (0 tests, conexiones RabbitMQ)    |
| 🔍 SEO                  | ⭐⭐⭐ (3/5)     | ↑ Mejorando (structured data parcial)      |
| 🔐 Seguridad            | ⭐⭐⭐ (3/5)     | ↓ Riesgo (5 vulnerabilidades P0)           |

---

## 2. Hallazgos Críticos (P0)

### 🔴 Seguridad

| #   | Issue                                        | Servicio            | Estado                |
| --- | -------------------------------------------- | ------------------- | --------------------- |
| S1  | JWT secret hardcodeado                       | AIProcessingService | Pendiente (Sprint 16) |
| S2  | Credenciales RabbitMQ "guest"                | Gateway             | Pendiente (Sprint 17) |
| S3  | Connection string en logs                    | VehiclesSaleService | Pendiente (Sprint 17) |
| S4  | AuditService sin autenticación               | AuditService        | Pendiente (Sprint 17) |
| S5  | LlmServer Docker corre como root             | LlmServer           | Propuesto Sprint 18   |
| S6  | frontend-web/gateway sin runAsNonRoot en K8s | K8s deployments     | Propuesto Sprint 18   |
| S7  | Trivy scan no bloquea CRITICAL               | CI/CD               | Propuesto Sprint 18   |
| S8  | Sin rate limiting en Ingress K8s             | K8s ingress         | Propuesto Sprint 18   |

### 🔴 Performance

| #   | Issue                             | Servicio            | Impacto                                     |
| --- | --------------------------------- | ------------------- | ------------------------------------------- |
| P1  | Paginación en memoria             | VehiclesSaleService | 100MB+ RAM por request para dealers grandes |
| P2  | N+1 en Compare endpoint           | ComparisonService   | 5 queries secuenciales por comparación      |
| P3  | BillingService queries sin límite | BillingService      | Table scans a medida que data crece         |

---

## 3. Inventario de Servicios

### 3.1 Backend (24 servicios)

| Servicio            | Clean Arch | Serilog Std    | Observability  | Error Handling | JWT          | Health Checks   | Score             |
| ------------------- | ---------- | -------------- | -------------- | -------------- | ------------ | --------------- | ----------------- |
| AuthService         | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐⭐        |
| ContactService      | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐⭐ (Gold) |
| SearchAgent         | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐⭐        |
| SupportAgent        | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐⭐        |
| RecoAgent           | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐⭐        |
| AdminService        | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐⭐        |
| Gateway             | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐          |
| NotificationService | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐          |
| MediaService        | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐          |
| ErrorService        | ✅         | ✅             | ✅             | ✅             | ✅           | ✅ Triple       | ⭐⭐⭐⭐          |
| CRMService          | ✅         | ✅ (Sprint 17) | ✅ (Sprint 17) | ✅ (Sprint 17) | ✅ (Fix S17) | ✅ Triple (S17) | ⭐⭐⭐⭐          |
| ReportsService      | ✅         | ✅ (Sprint 17) | ✅ (Sprint 17) | ✅ (Sprint 17) | ✅ (Add S17) | ✅ Triple (S17) | ⭐⭐⭐⭐          |
| VehiclesSaleService | ✅         | Manual         | ❌             | ❌             | ⚠️ Fallback  | Básico          | ⭐⭐⭐            |
| BillingService      | ✅         | Manual         | ❌             | ❌             | ✅           | Básico          | ⭐⭐⭐            |
| UserService         | ✅         | Manual         | ❌             | ❌             | ✅           | Básico          | ⭐⭐⭐            |
| KYCService          | ✅         | Manual         | ❌             | ❌             | ✅           | Básico          | ⭐⭐              |
| ChatbotService      | ✅         | Manual         | ❌             | ❌             | ⚠️           | Básico          | ⭐⭐              |
| AuditService        | ✅         | Manual         | ❌             | ❌             | ❌ Missing   | Básico          | ⭐⭐              |
| ReviewService       | ✅         | Manual         | ❌             | ❌             | ⚠️           | Básico          | ⭐⭐              |
| AIProcessingService | ✅         | Manual         | ❌             | ❌             | 🔴 Hardcoded | Básico          | ⭐                |

### 3.2 Frontend Web

| Componente       | Estado         | Notas                             |
| ---------------- | -------------- | --------------------------------- |
| Next.js 16.1.6   | ✅ Actualizado | App Router                        |
| React 19.2.3     | ✅ Actualizado |                                   |
| TypeScript       | ✅ Strict mode |                                   |
| Tailwind CSS v4  | ✅             |                                   |
| TanStack Query   | ✅             | Server state management           |
| shadcn/ui        | ✅             | Component library                 |
| CSRF Protection  | ✅             | csrfFetch(), CsrfInput            |
| XSS Prevention   | ✅             | escapeHtml(), sanitizeText()      |
| URL Sanitization | ✅             | sanitizeUrl()                     |
| BFF Pattern      | ✅             | /api/\* → internal gateway        |
| SEO (JSON-LD)    | ⚠️ Parcial     | Falta per-vehicle structured data |
| Sitemap          | ⚠️             | Monolítico, necesita splitting    |
| OG Images        | ⚠️             | Estáticas, necesita dinámicas     |

### 3.3 Mobile App (Flutter/Dart)

| Componente      | Estado             | Notas                                       |
| --------------- | ------------------ | ------------------------------------------- |
| Framework       | Flutter/Dart       | ⚠️ Prompt dice React Native pero es Flutter |
| Arquitectura    | Clean Arch + BLoC  | Bien estructurado                           |
| Feature Parity  | ~15-20%            | Mayormente scaffolding                      |
| Auth Flow       | ✅ Implementado    | Login, registro                             |
| Vehicle Listing | ⚠️ Parcial         | Lista básica, sin filtros avanzados         |
| KYC             | ❌ No implementado |                                             |
| Chat/Messaging  | ❌ No implementado |                                             |
| Payments        | ❌ No implementado |                                             |

---

## 4. Infraestructura

### 4.1 Kubernetes (DOKS)

| Métrica           | Valor                               |
| ----------------- | ----------------------------------- |
| Total deployments | ~42 (17 activos, 25 en replicas: 0) |
| Namespace         | okla                                |
| Cluster           | DO DOKS (2-6 nodos)                 |
| Service type      | 100% ClusterIP                      |
| TLS               | cert-manager + Let's Encrypt        |
| Network Policies  | ✅ Zero-trust default-deny          |
| Resource Limits   | ✅ En todos los deployments         |
| Probes            | ✅ Startup + Liveness + Readiness   |
| HPA               | ✅ Tiered (pero con duplicados)     |
| PDB               | ⚠️ 3 servicios activos sin PDB      |

### 4.2 CI/CD (GitHub Actions)

| Métrica           | Valor                                            |
| ----------------- | ------------------------------------------------ |
| Workflows         | 8 (smart-cicd, security, frontend, deploy, etc.) |
| Runner            | Self-hosted macOS ARM64 (SPOF)                   |
| Smart CI          | ✅ dorny/paths-filter (solo construye cambios)   |
| Image tags        | ⚠️ :latest (no inmutable)                        |
| Security scanning | ✅ Trivy + Gitleaks + NuGet vuln                 |
| Trivy blocking    | ❌ continue-on-error: true                       |
| Image signing     | ❌ No Cosign/Notary                              |

### 4.3 Docker

| Métrica            | Valor                       |
| ------------------ | --------------------------- |
| .NET Dockerfiles   | 12 (11 Good, 1 Poor)        |
| Multi-stage builds | 11/12                       |
| Alpine runtime     | 11/12                       |
| Non-root user      | 11/12 (LlmServer exception) |
| HEALTHCHECK        | 12/12                       |
| Port 8080          | 11/12 (LlmServer usa 8000)  |

---

## 5. Shared Libraries

| Librería                       | LoC   | Tests | Calidad    |
| ------------------------------ | ----- | ----- | ---------- |
| CarDealer.Shared (Core)        | 4,257 | ❌    | ⭐⭐⭐⭐   |
| CarDealer.Contracts            | 1,308 | ✅    | ⭐⭐⭐⭐⭐ |
| CarDealer.Shared.ErrorHandling | 1,154 | ❌    | ⭐⭐⭐⭐⭐ |
| CarDealer.Shared.Resilience    | 993   | ❌    | ⭐⭐⭐⭐⭐ |
| CarDealer.Shared.Idempotency   | 974   | ❌    | ⭐⭐⭐⭐⭐ |
| CarDealer.Shared.RateLimiting  | 895   | ❌    | ⭐⭐⭐⭐   |
| CarDealer.Shared.Sagas         | 735   | ❌    | ⭐⭐⭐     |
| CarDealer.Shared.Audit         | 713   | ❌    | ⭐⭐⭐⭐   |
| CarDealer.Shared.Logging       | 568   | ❌    | ⭐⭐⭐⭐   |
| CarDealer.Shared.ApiVersioning | 540   | ❌    | ⭐⭐⭐⭐   |
| CarDealer.Shared.Observability | 512   | ❌    | ⭐⭐⭐⭐   |
| CarDealer.Shared.HealthChecks  | 436   | ❌    | ⭐⭐⭐⭐⭐ |
| CarDealer.Shared.FeatureFlags  | 462   | ❌    | ⭐⭐⭐     |
| ServiceDiscovery               | 1,483 | ❌    | ⭐⭐       |

**Issue principal:** 13/14 librerías sin tests. Un bug en ErrorHandling o Resilience afecta TODOS los servicios.

---

## 6. Top 20 Issues Priorizados

| #   | Categoría      | Issue                                  | Impacto | Esfuerzo | Sprint |
| --- | -------------- | -------------------------------------- | ------- | -------- | ------ |
| 1   | 🔴 Seguridad   | JWT hardcodeado en AIProcessingService | Crítico | Bajo     | 16     |
| 2   | 🔴 Seguridad   | RabbitMQ "guest" en Gateway            | Crítico | Bajo     | 17     |
| 3   | 🔴 Seguridad   | Connection string en logs VSS          | Crítico | Bajo     | 17     |
| 4   | 🔴 Seguridad   | AuditService sin auth                  | Crítico | Medio    | 17     |
| 5   | 🔴 Seguridad   | LlmServer Docker root                  | Alto    | Bajo     | 18     |
| 6   | 🔴 Seguridad   | K8s frontend/gateway sin runAsNonRoot  | Alto    | Bajo     | 18     |
| 7   | 🔴 Performance | Paginación en memoria VSS              | Alto    | Medio    | 18     |
| 8   | 🔴 CI/CD       | Trivy no blocking                      | Alto    | Bajo     | 18     |
| 9   | 🟠 Performance | N+1 en ComparisonService               | Medio   | Bajo     | 19     |
| 10  | 🟠 Performance | BillingService sin paginación          | Medio   | Medio    | 19     |
| 11  | 🟠 Operacional | HPAs duplicados K8s                    | Medio   | Bajo     | 18     |
| 12  | 🟠 Calidad     | 0 tests en shared libs                 | Alto    | Alto     | 19-20  |
| 13  | 🟠 Infra       | 3x conexiones RabbitMQ                 | Medio   | Medio    | 19     |
| 14  | 🟠 Correctness | /health incluye "external"             | Medio   | Bajo     | 18     |
| 15  | 🟡 Deuda       | ServiceName enum desactualizado        | Bajo    | Bajo     | 18     |
| 16  | 🟡 Deuda       | SecurityValidators copy-paste          | Medio   | Bajo     | 19     |
| 17  | 🟡 Deuda       | NuGet versions inconsistentes          | Bajo    | Bajo     | 19     |
| 18  | 🟡 SEO         | Sitemap monolítico                     | Medio   | Medio    | 19     |
| 19  | 🟡 Mobile      | ~15% paridad                           | Alto    | Alto     | 20+    |
| 20  | 🟡 Operacional | CI runner SPOF (macOS)                 | Alto    | Alto     | 20+    |

---

## 7. Roadmap Técnico Q2 2026

### Abril 2026 (Sprints 18-19)

- Cerrar todas las vulnerabilidades de seguridad P0
- Fix de performance en VehiclesSaleService y BillingService
- Consolidar K8s resources (HPAs, NetworkPolicies)
- Tests para shared libraries críticas

### Mayo 2026 (Sprints 20-21)

- Migrar todos los servicios a shared libraries (estandarización 100%)
- Implementar Directory.Packages.props
- Crear CarDealer.Shared.Validation (SecurityValidators centralizado)
- MVP de calculadora DGII (datos estáticos)

### Junio 2026 (Sprints 22-23)

- Mobile app: alcanzar 50% paridad
- SEO: sitemap splitting, dynamic OG images
- Convenio con proveedor de datos gubernamentales
- Implementar premium features Tier 1 (Plan Básico dealers)

---

## 8. Proyección de Deuda Técnica

### Si no se abordan los P0 en Sprint 18:

- **Seguridad:** Riesgo de breach por JWT hardcodeado o Docker root. Costo potencial: incalculable (reputación, legal)
- **Performance:** VehiclesSaleService se degradará significativamente cuando un dealer tenga 500+ vehículos. Downtime potencial
- **CI/CD:** Imágenes con CVEs críticos podrían llegar a producción sin detección

### Si no se abordan los P1 en Sprints 19-20:

- **Testing:** Un bug en shared libraries propaga a 20+ servicios sin detección temprana
- **Operacional:** HPAs conflictivos causan scaling impredecible bajo carga
- **Mobile:** Pérdida de usuarios mobile-first (>60% del tráfico en RD es mobile)

---

## 9. Reportes Detallados (Referencias)

| Reporte                    | Archivo                                                   |
| -------------------------- | --------------------------------------------------------- |
| Auditoría Backend          | docs/reportes/sprints/SPRINT_17_AUDIT_2026-03-06.md       |
| Auditoría CI/CD & Docker   | docs/reportes/OKLA_CICD_DOCKER_AUDIT_2026-03.md           |
| Auditoría Kubernetes       | docs/reportes/OKLA_K8S_AUDIT_2026-03.md                   |
| Auditoría Shared Libraries | docs/reportes/OKLA_SHARED_LIBS_AUDIT_2026-03.md           |
| Auditoría DB Performance   | docs/reportes/OKLA_DB_PERFORMANCE_AUDIT_2026-03.md        |
| Features Premium           | docs/reportes/OKLA_PREMIUM_FEATURES_RESEARCH_2026-03.md   |
| Integración DGII           | docs/reportes/OKLA_DGII_INTEGRATION_RESEARCH_2026-03.md   |
| Análisis Competitivo       | docs/reportes/OKLA_COMPETITIVE_ANALYSIS_Q1_2026.md        |
| Sprint 17 CPSO Report      | docs/reportes/sprints/SPRINT_17_CPSO_REPORT_2026-03-06.md |
| Sprint 18 Propuesta        | docs/reportes/sprints/SPRINT_18_PROPOSAL_2026-03-06.md    |
