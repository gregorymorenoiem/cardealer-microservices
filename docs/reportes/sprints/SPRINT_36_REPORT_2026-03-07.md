# 📊 Sprint 36 Report — Zustand, Build Fixes, Tests 317+, Real Persistence, Cron DoS Prevention

**Fecha**: 7 marzo 2026
**Sprint**: 36
**Branch**: main

---

## Objetivo del Sprint

Completar 5 tareas técnicas: setup de Zustand en el frontend, corrección de build errors en servicios backend, aumentar cobertura de tests compartidos a 275+, implementar persistencia real para PriceAlerts y SavedSearches, y prevención de DoS en validación de CronExpression.

---

## Tareas Ejecutadas

### Tarea 36.1 — Frontend Zustand Store + Search State Management

**Archivos creados**:

- `frontend/web-next/src/stores/search-store.ts` (364 líneas)
- `frontend/web-next/src/stores/index.ts` (barrel export)

**Funcionalidad**:

- Store centralizado de estado de búsqueda con `zustand 5.0.11` + `persist` middleware
- Tipos: `SearchFilters` (25+ campos DR-market), `ViewMode`, `RecentSearch`, `SearchDraft`
- State: filtros activos, previousFilters (undo), UI (sidebar, viewMode), historia (max 15), drafts (max 10)
- Actions: setFilter/setFilters (auto-reset page), clearFilter/clearAllFilters, undoFilterChange
- Drafts: saveDraft/loadDraft/removeDraft para combinaciones de filtros sin guardar
- Persistencia: localStorage key `okla-search-state`, partialize para excluir state transitorio
- Selector hooks optimizados: useSearchFilters, useSearchViewMode, useRecentSearches, useActiveFilterCount, useFilterSidebarOpen
- TypeScript clean: 0 errores

### Tarea 36.2 — Fix Build Errors

**CRMService** (`CRMService.Api.csproj`):

- 3 project references rotas: `CarDealer.Logging` → `CarDealer.Shared.Logging`, `CarDealer.Observability` → `CarDealer.Shared.Observability`, `CarDealer.ErrorHandling` → `CarDealer.Shared.ErrorHandling`
- Build: 0 errors, 0 warnings

**AuditService** (`AuditService.Api/Program.cs`):

- `/health` predicate: `check.Tags.Contains("liveness")` → `!check.Tags.Contains("external")` (estándar del proyecto)
- Build: 0 errors

### Tarea 36.3 — Shared Lib Test Coverage: 317 tests (+108)

**Target**: 275+ tests | **Resultado**: 317 tests ✅

**12 archivos de test creados** en `CarDealer.Shared.Tests`:

| Archivo                                 | Tests   | Cobertura                                        |
| --------------------------------------- | ------- | ------------------------------------------------ |
| Contracts/EventBaseTests.cs             | 12      | EventBase defaults, IEvent interface, uniqueness |
| Contracts/ApiResponseTests.cs           | 7       | SuccessResponse, ErrorResponse, complex types    |
| Contracts/PaginationDtoTests.cs         | 10      | TotalPages, HasPrevious/NextPage, edge cases     |
| Contracts/ServiceNamesTests.cs          | 27      | 27 enum values, no duplicates (Theory)           |
| Contracts/ErrorDetailsDtoTests.cs       | 4       | Defaults, timestamp, validation errors           |
| Audit/AuditOptionsTests.cs              | 7       | SectionName, defaults, RabbitMq, AutoAudit       |
| Audit/AuditAttributeTests.cs            | 8       | Constructors, AttributeUsage, severity           |
| Idempotency/IdempotencyOptionsTests.cs  | 5       | SectionName, defaults, excluded paths/methods    |
| Idempotency/IdempotencyRecordTests.cs   | 7       | Defaults, status transitions, headers, metadata  |
| Idempotency/IdempotentAttributeTests.cs | 5       | Defaults, AttributeUsage, SkipIdempotency        |
| RateLimiting/RateLimitOptionsTests.cs   | 7       | SectionName, policies, EndpointRateLimitPolicy   |
| HealthChecks/HealthCheckOptionsTests.cs | 8       | 5 health check types defaults                    |
| **Total**                               | **108** |                                                  |

**Dependencia agregada**: `CarDealer.Contracts` project reference al test project.

### Tarea 36.4 — PriceAlerts + SavedSearches Real Persistence

Migración de stubs in-memory a persistencia real EF Core en NotificationService.

**Domain Layer**:

- `PriceAlert.cs`: Entity con factory `Create()`, métodos `Deactivate()`, `MarkTriggered()`, `UpdatePrice()`
- `SavedSearch.cs`: Entity con factory `Create()`, métodos `IncrementMatchCount()`, `MarkNotified()`, `Deactivate()`
- `IPriceAlertRepository.cs`: 9 métodos async (GetById, GetByUser, paginated, filtered)
- `ISavedSearchRepository.cs`: 8 métodos async (GetById, GetByUser, paginated, active searches)

**Application Layer**:

- `AlertDtos.cs`: 6 record DTOs (Create/Update/Response para ambos)
- `AlertValidators.cs`: 4 FluentValidation validators con `NoSqlInjection()` + `NoXss()` en todos los strings

**Infrastructure Layer**:

- `EfPriceAlertRepository.cs`: Implementación completa con paginación Skip/Take
- `EfSavedSearchRepository.cs`: Implementación completa con paginación
- `PriceAlertConfiguration.cs`: Tabla "PriceAlerts", precision(18,2), indexes compuestos
- `SavedSearchConfiguration.cs`: Tabla "SavedSearches", CriteriaJson como `jsonb`, indexes

**API Layer**:

- `PriceAlertsController.cs`: Reescrito con IPriceAlertRepository, async, ownership checks, MapToResponse
- `SavedSearchesController.cs`: Reescrito con ISavedSearchRepository, async, JsonSerializer para criteria

**DI**: Registrados `IPriceAlertRepository` → `EfPriceAlertRepository` y `ISavedSearchRepository` → `EfSavedSearchRepository` en `ServiceCollectionExtensions.cs`

**Build**: 0 errors, 1 warning pre-existente (ServiceVersion unused)

### Tarea 36.5 — CronExpression Validation (DoS Prevention)

**Archivo**: `NotificationService.Application/Validators/ScheduleNotificationRequestValidator.cs`

**Cambios**:

- Agregado NuGet `Cronos 0.8.4` para parsing de expresiones cron
- Validación de sintaxis cron con Cronos parser (SecondFormat)
- **DoS prevention**: Enforcement de intervalo mínimo de 5 minutos entre ejecuciones (previene cron sub-minuto)
- Validación de TimeZone via `TimeZoneInfo.FindSystemTimeZoneById`
- Validación de regla de negocio: si `IsRecurring == true`, cron expression es obligatoria
- Build: 0 errors

---

## Métricas

| Métrica                  | Antes | Después    |
| ------------------------ | ----- | ---------- |
| Tests compartidos        | 209   | 317 (+108) |
| Build errors (CRM/Audit) | 4+    | 0          |
| Zustand stores           | 0     | 1          |
| Stub controllers         | 2     | 0          |
| Archivos creados         | —     | 18+        |
| Archivos modificados     | —     | 8+         |

---

## Próximo Sprint (37)

1. EF migration para tablas PriceAlert + SavedSearch
2. Integration tests para PriceAlerts/SavedSearches controllers
3. Integración frontend search page con Zustand store
4. Auditoría de health check triple en todos los servicios
5. Shared lib test coverage target 375+
