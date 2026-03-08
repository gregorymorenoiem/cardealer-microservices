# Sprint 24 Report — 2026-03-06

## 🎯 Tema: Ownership Checks + Service Standardization Wave 3

## ✅ Tareas Completadas

### 1. VehiclesSaleService Ownership Checks (P2→Fixed)

**Archivo:** `VehiclesSaleService.Api/Controllers/VehiclesController.cs`

Added ownership verification to 5 critical endpoints:

| Endpoint               | Antes                                        | Después                                                      |
| ---------------------- | -------------------------------------------- | ------------------------------------------------------------ |
| `PUT /{id}` (Update)   | No ownership check                           | ✅ `IsOwnerOrAdmin(vehicle)` — 403 if not owner              |
| `DELETE /{id}`         | No ownership check — used `ExistsAsync` only | ✅ Full vehicle load + `IsOwnerOrAdmin()` — 403 if not owner |
| `POST /{id}/publish`   | No ownership check                           | ✅ `IsOwnerOrAdmin()` before KYC gate                        |
| `POST /{id}/unpublish` | No ownership check                           | ✅ `IsOwnerOrAdmin()` before status check                    |
| `POST /{id}/sold`      | No ownership check                           | ✅ `IsOwnerOrAdmin()` before status check                    |

**Helper methods added:**

- `GetCurrentUserId()` — extracts from `ClaimTypes.NameIdentifier`, `sub`, or `userId` claims
- `IsOwnerOrAdmin(vehicle)` — checks `vehicle.SellerId == userId` OR `User.IsInRole("Admin"/"Moderator")`

**Bug fix:** `vehicle.UserId` → `vehicle.SellerId` (2 occurrences in MarkAsSold) — `UserId` doesn't exist on the Vehicle entity.

### 2. AIProcessingService Full Standardization (0/17 → 17/17)

**Archivos:** `AIProcessingService.Api/Program.cs`, `AIProcessingService.Api.csproj`

This was the **least standardized service** — zero shared lib integrations. Full rewrite:

| Pattern                       | Antes                                                                      | Después                                               |
| ----------------------------- | -------------------------------------------------------------------------- | ----------------------------------------------------- |
| UseStandardSerilog            | ❌ Missing                                                                 | ✅ `UseStandardSerilog(ServiceName)`                  |
| AddStandardObservability      | ❌ Missing                                                                 | ✅ 3-param config-driven                              |
| AddStandardErrorHandling      | ❌ Missing                                                                 | ✅ 2-param config-driven                              |
| AddAuditPublisher             | ❌ Missing                                                                 | ✅ Added                                              |
| JWT                           | Raw config (throws OK)                                                     | ✅ `MicroserviceSecretsConfiguration.GetJwtConfig()`  |
| CORS                          | `SetIsOriginAllowed(_ => true)` in dev, `AllowAnyMethod/Header` everywhere | ✅ Restricted                                         |
| Rate limiting                 | ❌ Missing                                                                 | ✅ Per-IP: `fixed` (60/min), `ai-processing` (10/min) |
| Health checks                 | Bare `/health` with DB+RabbitMQ in default                                 | ✅ Triple pattern, tags `ready,external`              |
| try/catch/finally             | ❌ Missing                                                                 | ✅ Added                                              |
| UseGlobalErrorHandling        | ❌ Missing                                                                 | ✅ Added                                              |
| UseRequestLogging             | ❌ Missing                                                                 | ✅ Added                                              |
| UseAuditMiddleware            | ❌ Missing                                                                 | ✅ Added                                              |
| UseHttpsRedirection           | ❌ Missing                                                                 | ✅ Conditional (skip in prod)                         |
| ServiceName/Version constants | ❌ Missing                                                                 | ✅ Added                                              |
| public partial class Program  | ❌ Missing                                                                 | ✅ Added                                              |

4 shared lib project references added to .csproj.

### 3. VehiclesSaleService Program.cs Hardening

**Archivos:** `VehiclesSaleService.Api/Program.cs`, `VehiclesSaleService.Api.csproj`

| Fix                          | Antes                                                   | Después                                              |
| ---------------------------- | ------------------------------------------------------- | ---------------------------------------------------- |
| JWT                          | Raw config with fallback defaults ("CarDealerPlatform") | ✅ `MicroserviceSecretsConfiguration.GetJwtConfig()` |
| Health checks                | Bare `/health`                                          | ✅ Triple pattern with external exclusion            |
| try/catch/finally            | ❌ Missing                                              | ✅ Added with Log.Fatal + Log.CloseAndFlush()        |
| UseAuditMiddleware           | ❌ Missing                                              | ✅ Added                                             |
| public partial class Program | ❌ Missing                                              | ✅ Added                                             |
| Console.WriteLine            | Used for startup messages                               | ✅ Log.Information                                   |
| ServiceName/Version          | ❌ Missing                                              | ✅ Constants added                                   |
| Audit lib reference          | ❌ Missing in .csproj                                   | ✅ CarDealer.Shared.Audit added                      |

## 📊 Métricas del Sprint

| Métrica                        | Valor                                        |
| ------------------------------ | -------------------------------------------- |
| Archivos modificados           | 5                                            |
| Ownership checks añadidos      | 5 endpoints                                  |
| Bugs corregidos                | 1 (vehicle.UserId → vehicle.SellerId)        |
| Servicios estandarizados       | 2 (AIProcessingService, VehiclesSaleService) |
| Shared lib references añadidos | 5                                            |

## 🔍 Servicios Estandarizados (Acumulado Sprints 21-24)

| Servicio               | Sprint | Estado           |
| ---------------------- | ------ | ---------------- |
| ReviewService          | 21     | ✅ Gold Standard |
| ChatbotService         | 21     | ✅ Gold Standard |
| DealerAnalyticsService | 22     | ✅ Gold Standard |
| BillingService         | 23     | ✅ Gold Standard |
| AIProcessingService    | 24     | ✅ Gold Standard |
| VehiclesSaleService    | 24     | ✅ Gold Standard |

## ⚠️ Deuda Técnica Remanente

| Severidad | Issue                                                           | Servicio                            |
| --------- | --------------------------------------------------------------- | ----------------------------------- |
| P1        | X-Dealer-Id header trusted sin validación JWT                   | BillingService                      |
| P2        | POST /{id}/views sin rate limiting per-vehicle                  | VehiclesSaleService                 |
| Medium    | UseStandardSerilog with inline options (functional but verbose) | BillingService, VehiclesSaleService |
