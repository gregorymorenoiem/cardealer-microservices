# Sprint 23 Report — 2026-03-06

## 🎯 Tema: Estandarización BillingService + Auth Hardening VehiclesSaleService

## ✅ Tareas Completadas

### 1. BillingService Program.cs Estandarización (P0/P1/Medium)

**Archivos:** `BillingService.Api/Program.cs`

Fixes aplicados:

- **P0**: Health check triple pattern (`/health` con exclusión external, `/health/ready`, `/health/live`)
- **P0**: try/catch/finally wrapper con `Log.Fatal` + `Log.CloseAndFlush()`
- **P1**: CORS restringido — eliminado `SetIsOriginAllowed(_ => true)` y `AllowAnyMethod/AllowAnyHeader`, reemplazado con `WithOrigins` + `WithMethods` + `WithHeaders` explícitos
- **Medium**: JWT cambiado de raw config con fallbacks (`"AuthService-Dev"`, `"OKLA-Dev"`) a `MicroserviceSecretsConfiguration.GetJwtConfig()` — throws si no está configurado
- **Medium**: `ClockSkew` reducido de `FromMinutes(5)` a `TimeSpan.Zero`
- **Medium**: `UseHttpsRedirection()` ahora condicional — skip en producción (TLS en Ingress)
- **Medium**: Migration envuelta en try/catch para prevenir crash de startup
- **Low**: Constantes `ServiceName` y `ServiceVersion` añadidas

Lo que YA estaba bien (preservado): UseStandardSerilog, AddStandardObservability, AddStandardErrorHandling, AddAuditPublisher, UseGlobalErrorHandling, UseRequestLogging, UseAuditMiddleware, UseApiSecurityHeaders, Idempotency, FluentValidation, Stripe/Azul config, RabbitMQ publisher, HTTP clients.

### 2. VehiclesSaleService Moderation Auth — 4 P0 Fixes

**Archivo:** `VehiclesSaleService.Api/Controllers/VehiclesController.cs`

| Endpoint                | Antes                          | Después                                  | Severidad |
| ----------------------- | ------------------------------ | ---------------------------------------- | --------- |
| `POST /{id}/approve`    | `[Authorize]` (class, no role) | `[Authorize(Roles = "Admin,Moderator")]` | 🔴 P0     |
| `POST /{id}/reject`     | `[Authorize]` (class, no role) | `[Authorize(Roles = "Admin,Moderator")]` | 🔴 P0     |
| `GET /moderation/queue` | `[AllowAnonymous]`             | `[Authorize(Roles = "Admin,Moderator")]` | 🔴 P0     |
| `GET /admin/search`     | `[AllowAnonymous]`             | `[Authorize(Roles = "Admin,Moderator")]` | 🔴 P0     |
| `POST /{id}/feature`    | `[Authorize]` (class, no role) | `[Authorize(Roles = "Admin")]`           | 🟡 P1     |
| `POST /bulk-images`     | `[Authorize]` (class, no role) | `[Authorize(Roles = "Admin")]`           | 🟡 P1     |

**Impacto**: Antes, CUALQUIER usuario autenticado podía aprobar/rechazar vehículos. La cola de moderación y búsqueda admin eran completamente públicas (sin autenticación).

### 3. HomepageSectionsController Auth — 6 P1 Fixes

**Archivo:** `VehiclesSaleService.Api/Controllers/HomepageSectionsController.cs`

Todos los endpoints mutables (POST, PUT, DELETE) ahora requieren `[Authorize(Roles = "Admin")]`:

- `POST {slug}/vehicles` — Asignar vehículo a sección
- `DELETE {slug}/vehicles/{vehicleId}` — Remover vehículo de sección
- `POST` — Crear sección
- `PUT {slug}` — Actualizar sección
- `DELETE {slug}` — Eliminar sección
- `POST {slug}/vehicles/bulk` — Asignación masiva

Los endpoints de lectura (`[AllowAnonymous]`) permanecen públicos (correcto para el frontend).

## 📊 Métricas del Sprint

| Métrica                            | Valor              |
| ---------------------------------- | ------------------ |
| Archivos modificados               | 3                  |
| Vulnerabilidades P0 corregidas     | 4                  |
| Vulnerabilidades P1 corregidas     | 8                  |
| Vulnerabilidades Medium corregidas | 4                  |
| Servicios estandarizados           | 1 (BillingService) |

## 🔍 Servicios Estandarizados (Acumulado Sprints 21-23)

| Servicio               | Sprint | Estado           |
| ---------------------- | ------ | ---------------- |
| ReviewService          | 21     | ✅ Gold Standard |
| ChatbotService         | 21     | ✅ Gold Standard |
| DealerAnalyticsService | 22     | ✅ Gold Standard |
| BillingService         | 23     | ✅ Gold Standard |

## ⚠️ Deuda Técnica Remanente

| Severidad | Issue                                            | Servicio                        |
| --------- | ------------------------------------------------ | ------------------------------- |
| P1        | X-Dealer-Id header trusted sin validación JWT    | BillingService (5+ controllers) |
| P1        | StripePaymentController DealerId from body       | BillingService                  |
| P2        | POST /{id}/views sin dedup/rate limiting         | VehiclesSaleService             |
| P2        | Create/Update/Delete vehicle sin ownership check | VehiclesSaleService             |

## 🏗️ Próximo Sprint

- Auditoría completa de ownership checks en VehiclesSaleService
- BillingService DealerId JWT extraction (refactoring profundo)
- CI/CD SHA immutable tags
- Frontend dynamic OG images
