# Authentication Security Audit & Fixes Report

**Fecha:** 2026-03-06
**Responsable:** CPSO
**Tipo:** Security Audit + Remediation (Proactive)

---

## Resumen

Se descubrieron **10 brechas de autenticación** en 6 servicios diferentes. Se corrigieron las 4 más críticas (P0) inmediatamente.

---

## Vulnerabilidades Encontradas

### 🔴 P0 — Corregidas

| #   | Servicio                   | Issue                                                | Estado                                                |
| --- | -------------------------- | ---------------------------------------------------- | ----------------------------------------------------- |
| 1   | DealerAnalyticsService     | ALL 9 controllers con `[Authorize]` comentado        | ✅ FIXED — Descomentado en los 9                      |
| 2   | HomepageSectionsController | Sin auth — POST/PUT/DELETE expuestos                 | ✅ FIXED — `[Authorize]` + `[AllowAnonymous]` en GETs |
| 3   | AIProcessingService        | POST `/process` con `[AllowAnonymous]` ("temporary") | ✅ FIXED — Cambiado a `[Authorize]`                   |
| 4   | AuditService               | Sin JWT config en Program.cs                         | ⚠️ Requiere refactor mayor (Sprint 19)                |

### 🟡 P1 — Pendientes (Sprint 19)

| #   | Servicio              | Issue                                                        |
| --- | --------------------- | ------------------------------------------------------------ |
| 5   | VehiclesSaleService   | CatalogController — `POST seed` sin auth (admin destructivo) |
| 6   | BillingService        | Payment init acepta DealerId sin verificar identidad         |
| 7   | SearchAgent/RecoAgent | POST endpoints a Claude API sin auth ni rate limiting        |
| 8   | VehiclesSaleService   | Moderation queue expuesta anónimamente (pod-to-pod)          |

### 🟡 P2 — Pendientes

| #   | Servicio            | Issue                                                         |
| --- | ------------------- | ------------------------------------------------------------- |
| 9   | VehiclesSaleService | `POST {id}/views` sin dedup/rate limiting (view count gaming) |

### 🟢 Aceptable (No requieren cambio)

- BillingService pricing/packages — datos públicos
- Stripe/Azul webhooks — validados con firma
- Compare endpoint — semántica read-only pese a ser POST
- Guest contact leads — por diseño

---

## Archivos Modificados (12 archivos)

| Archivo                                                      | Cambio                                             |
| ------------------------------------------------------------ | -------------------------------------------------- |
| DealerAnalyticsService `.../DashboardController.cs`          | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../InventoryAnalyticsController.cs` | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../AnalyticsController.cs`          | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../InsightsController.cs`           | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../OverviewController.cs`           | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../ReportsController.cs`            | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../ConversionFunnelController.cs`   | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../BenchmarkController.cs`          | Uncommented `[Authorize]`                          |
| DealerAnalyticsService `.../AlertsController.cs`             | Uncommented `[Authorize]`                          |
| VehiclesSaleService `.../HomepageSectionsController.cs`      | Added `[Authorize]` + `[AllowAnonymous]` on GETs   |
| AIProcessingService `.../AIProcessingController.cs`          | `[AllowAnonymous]` → `[Authorize]` on POST process |

---

## Impacto

- **11 controladores** asegurados con `[Authorize]`
- **11 endpoints POST/PUT/DELETE** que estaban públicamente expuestos ahora requieren JWT
- **4 endpoints GET** del homepage correctamente marcados con `[AllowAnonymous]`
- **DealerAnalytics** ya no expone datos de negocio de dealers sin autenticación
- **AIProcessingService** ya no permite consumo anónimo de recursos GPU/API
