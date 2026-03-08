# Sprint 29 Report — 2026-03-06

## 📋 Resumen

**Período:** 2026-03-06
**CPSO:** Gregory Moreno (100% implementación)
**Tema:** Auditoría Auth (ContactService + AdminService) + Frontend OG + CI/CD SHA Tags

---

## ✅ Tareas Completadas

### 29.1 — Auditoría Auth: ContactService (5 controllers)

| Controller                   | Endpoints | Auth Class-Level | Ownership Checks      | Hallazgos          |
| ---------------------------- | --------- | ---------------- | --------------------- | ------------------ |
| ContactController.cs         | 0 (empty) | N/A              | N/A                   | Vacío — sin riesgo |
| ContactHistoryController.cs  | 0 (empty) | N/A              | N/A                   | Vacío — sin riesgo |
| ContactRequestsController.cs | 8         | ✅ `[Authorize]` | ✅ Buyer/Seller check | Bien implementado  |
| ContactMessagesController.cs | 2         | ✅ `[Authorize]` | ✅ Party check        | Bien implementado  |
| AppointmentsController.cs    | 6         | ✅ `[Authorize]` | **🔴 3 P1 missing**   | Fixed ↓            |

**P1 Issues Found & Fixed in AppointmentsController:**

- `GetById` — NO ownership check → **Fixed**: added user verification + Forbid pattern
- `Update` — NO ownership check → **Fixed**: added user verification + input validation
- `Delete` — NO ownership check → **Fixed**: added user verification
- `Create` — NO XSS/SQL validation on string inputs → **Fixed**: added `ContainsDangerousPatterns()`
- `GetCurrentUserId()` returned `Guid.Empty` on failure → **Fixed**: now throws `UnauthorizedAccessException`

### 29.2 — Auditoría Auth: AdminService (16 controllers)

| Controller                  | Auth Class-Level                                        | Hallazgos          |
| --------------------------- | ------------------------------------------------------- | ------------------ |
| AdminController.cs          | ✅ `Admin,SuperAdmin` + SuperAdmin-only                 | Bien implementado  |
| **DealersController.cs**    | **🔴 Missing** (per-endpoint only)                      | **Fixed** ↓        |
| PlatformUsersController.cs  | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| VehiclesController.cs       | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| ModerationController.cs     | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| StaffController.cs          | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| ConfigurationsController.cs | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| ContentController.cs        | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| PublicContentController.cs  | ✅ `[AllowAnonymous]` (intentional)                     | Correcto — público |
| AdvertisingController.cs    | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| ReviewsController.cs        | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| ReportsController.cs        | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| AnalyticsController.cs      | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| DashboardController.cs      | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| AdminMessagesController.cs  | ✅ `Admin,SuperAdmin`                                   | Bien implementado  |
| MaintenanceController.cs    | ✅ `Admin,SuperAdmin` + `[AllowAnonymous]` on GetStatus | Correcto           |

**P1 Issue Found & Fixed in DealersController:**

- Missing class-level `[Authorize(Roles = "Admin,SuperAdmin")]` — each endpoint had it individually but pattern was fragile
- **Fixed**: Added class-level authorize attribute

### 29.3 — Frontend: Dynamic OG Images per Vehicle

**Created:**

- `src/app/(main)/vehiculos/[slug]/opengraph-image.tsx` — Branded 1200×630 OG image with:
  - Vehicle photo as background with dark gradient overlay
  - OKLA logo + brand in top-left
  - Condition badge (Nuevo/Usado) in top-right
  - Vehicle title (Year Make Model) at bottom
  - Price in OKLA green (#00A870)
  - Mileage and location pills
  - okla.com.do watermark
- `src/app/(main)/vehiculos/[slug]/twitter-image.tsx` — Re-exports OG image for Twitter cards

**Updated:**

- `vehiculos/[slug]/page.tsx` — Removed manual `openGraph.images` / `twitter.images` from `generateMetadata` to let file-based convention take precedence

**SEO Impact:** Social shares now show branded vehicle images with price/specs overlay instead of raw S3 photos.

### 29.4 — CI/CD: SHA Immutable Image Tags

**Problem:** All 49 K8s deployments used `:latest` tags. Rollbacks unreliable, no image provenance.

**Changes:**

1. **`_reusable-dotnet-service.yml`**: Added `sha-${{ github.sha }}` tag alongside version + latest
2. **`_reusable-frontend.yml`**: Added `sha-${{ github.sha }}` tag alongside version + latest
3. **`deploy-digitalocean.yml`**: Updated deploy step to:
   - `kubectl set image` pins each deployment to `sha-<commit>` tag
   - Removed `rollout restart` (image change triggers rollout automatically)
   - Added SHA to version output for traceability

**Result:** Every deployment is now pinned to an immutable commit SHA tag. Rollbacks use `kubectl rollout undo` which reverts to the previous pinned SHA.

---

## 📊 Métricas

| Metric                                   | Value                                            |
| ---------------------------------------- | ------------------------------------------------ |
| P1 vulnerabilities found                 | 4 (3 IDOR + 1 fragile auth)                      |
| P1 vulnerabilities fixed                 | 4/4 (100%)                                       |
| Controllers audited                      | 21 (5 Contact + 16 Admin)                        |
| Files created                            | 2 (OG image + Twitter image)                     |
| Files modified                           | 6 (3 CI/CD + 2 controllers + 1 page)             |
| `:latest` tags remaining in K8s template | 49 (kept as template, overridden at deploy time) |

---

## 🔮 Sprint 30 — Próximo

- Shared libraries testing (13/14 without unit tests)
- VehiclesSaleService in-memory pagination fix
- Audit remaining services: ErrorService, MediaService controllers
- Frontend: Dealer detail OG images (same pattern as vehicle)
- Performance: Redis caching audit

---

## 📝 Deuda Técnica Identificada

1. **AppointmentsController** — All endpoints return stub data (TODO: implement MediatR handlers)
2. **ConfigurationsController** — Uses static in-memory Dictionary (TODO: persist to database)
3. **MaintenanceController** — Static maintenance status (TODO: persist to Redis/DB)
4. **DTOs in controller files** — Multiple controllers have DTOs defined inline instead of in Application/DTOs
5. **49 `:latest` tags in deployments.yaml** — Template file still uses :latest (OK since deploy pins with SHA), but could be confusing
