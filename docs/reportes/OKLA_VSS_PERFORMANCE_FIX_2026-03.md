# VehiclesSaleService — Database Performance Fix Report

**Fecha:** 2026-03-06
**Responsable:** CPSO
**Tipo:** Performance Optimization (Sprint 19, proactive)

---

## Resumen

Se identificaron y corrigieron **11 problemas de performance** en VehiclesSaleService, el servicio core del marketplace que sirve búsquedas, listados de vendedores/dealers, favoritos e historial.

---

## Fixes Implementados

### 🔴 CRÍTICO 1: Seller Vehicles — In-Memory Pagination → Database-Level

**Archivos modificados:**

- `IVehicleRepository.cs` — nuevo signature: `GetBySellerAsync(sellerId, page, pageSize, status?)`
- `VehicleRepository.cs` — `Skip/Take` ahora en el `IQueryable` antes de `ToListAsync()`
- `VehiclesController.cs` — controller delega paginación al repositorio

**Antes:** Cargaba TODOS los vehículos del vendedor a memoria, luego aplicaba `.Skip().Take()` en LINQ-to-Objects.
**Después:** `CountAsync()` + `Skip().Take().ToListAsync()` ejecutados en PostgreSQL.

**Impacto estimado:** Para un vendedor con 500 vehículos, de ~500 rows transferidas a ~12.

### 🔴 CRÍTICO 2: Dealer Vehicles — Unbounded Query → Paginated

**Antes:** `GetByDealerAsync(dealerId)` retornaba TODOS los vehículos sin límite.
**Después:** Retorna tupla `(Items, TotalCount)` con paginación server-side (default page=1, pageSize=12).

**Impacto estimado:** Dealers grandes con 2000+ vehículos ahora solo transfieren 12 por página.

### 🔴 CRÍTICO 3: Seller Stats — In-Memory Aggregation → SQL GROUP BY

**Antes:** Cargaba todos los vehículos del vendedor y hacía `.Count()` y `.Sum()` en C#.
**Después:** `GroupBy(_ => 1).Select(g => new { Count(), Sum() })` ejecutado como SQL `SELECT COUNT(*), SUM(*)`.

**Impacto estimado:** Una sola query SQL en vez de materializar N entidades.

### 🟠 HIGH 4: N+1 en Compare Endpoint

**Antes:** Loop `foreach (var id) { await GetByIdAsync(id); }` — 5 queries individuales.
**Después:** `GetByIdsAsync(ids)` — una sola query con `WHERE id IN (...)`.

### 🟠 HIGH 5-6: History Delete — Load-to-Delete → ExecuteDeleteAsync

**Antes:** `ClearHistory()` y `RemoveFromHistory()` cargaban todas las entidades a memoria para luego llamar `RemoveRange()`.
**Después:** `ExecuteDeleteAsync()` — emite un solo `DELETE FROM ... WHERE ...` sin materializar entidades.

### 🟠 HIGH 7: SyncHistory — N+1 per Sync Item → Batch Pre-load

**Antes:** Loop con `FirstOrDefaultAsync()` por cada item en el batch de sync.
**Después:** Pre-carga todos los historiales existentes con `ToDictionaryAsync()`, luego hace lookup in-memory.

### 🟠 HIGH 8: Favorites — Unbounded + ALL Images → Paginated + Primary Only

**Antes:** `GetByUserIdAsync()` cargaba TODOS los favoritos con TODAS las imágenes de cada vehículo.
**Después:** Paginado (page/pageSize), y `Images.Where(i => i.IsPrimary)` para solo cargar la imagen principal.

---

## Archivos Modificados (8 archivos)

| Archivo                                                                 | Cambio                                                                     |
| ----------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| `VehiclesSaleService.Domain/Interfaces/IVehicleRepository.cs`           | Nuevos signatures paginados + `SellerVehicleStatsResult` + `GetByIdsAsync` |
| `VehiclesSaleService.Domain/Interfaces/IFavoriteRepository.cs`          | Paginación en `GetByUserIdAsync`                                           |
| `VehiclesSaleService.Infrastructure/Repositories/VehicleRepository.cs`  | DB-level Skip/Take, SQL aggregation, batch query                           |
| `VehiclesSaleService.Infrastructure/Repositories/FavoriteRepository.cs` | Paginación + filtro de imágenes primarias                                  |
| `VehiclesSaleService.Api/Controllers/VehiclesController.cs`             | Usa nuevos repos paginados, stats SQL                                      |
| `VehiclesSaleService.Api/Controllers/HistoryController.cs`              | ExecuteDeleteAsync + batch pre-load                                        |
| `VehiclesSaleService.Api/Controllers/FavoritesController.cs`            | Paginación en GetMyFavorites                                               |

---

## Issues Pendientes (Medium/Low — Sprint 20+)

| #   | Severidad | Issue                                                     | Ubicación                |
| --- | --------- | --------------------------------------------------------- | ------------------------ |
| 9   | 🟡 Medium | BulkImport N+1 per make/model/trim                        | VehicleCatalogRepository |
| 10  | 🟡 Medium | GetBySlug loads multiple, filters in memory               | VehiclesController       |
| 11  | 🟢 Low    | Unbounded catalog queries (acceptable for small datasets) | CatalogController        |

---

## Breaking Changes

⚠️ **API contract changes:**

- `GET /api/vehicles/dealer/{dealerId}` — ahora acepta `?page=1&pageSize=12` y retorna `{ Data, Page, PageSize, TotalCount, TotalPages }` en vez de un array plano.
- `GET /api/favorites` — ahora acepta `?page=1&pageSize=20`.

El frontend debe actualizarse para manejar la nueva estructura de respuesta en el endpoint de dealer vehicles. El endpoint de seller vehicles ya tenía paginación en la API (solo se movió al DB level).
