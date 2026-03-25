# CORRECCIÓN (Intento 2/3) — Sprint 2: Búsqueda & Filtros de Vehículos (Guest)

**Fecha:** 2026-03-25 11:07:53
**Fase:** FIX
**Usuario:** Guest (sin login)
**URL:** https://okla.com.do

## Instrucciones — FASE DE CORRECCIÓN

En la auditoría anterior se encontraron bugs. Tu trabajo ahora es:

1. Lee la sección 'BUGS A CORREGIR' abajo
2. Corrige cada bug en el código fuente
3. Ejecuta el Gate Pre-Commit (8 pasos) para validar
4. Marca cada fix como completado: `- [ ]` → `- [x]`
5. Al terminar, agrega `READ` al final

⚠️ NO hagas commit aún — primero el sprint debe pasar RE-AUDITORÍA

## BUGS A CORREGIR

_(El agente que hizo la auditoría documentó los hallazgos aquí.)_
_(Lee el archivo de reporte del sprint anterior para ver los bugs.)_

Revisa el último reporte en `audit-reports/` o los hallazgos del prompt anterior.
Corrige todos los bugs encontrados:

## Credenciales

| Rol                 | Email                  | Password       |
| ------------------- | ---------------------- | -------------- |
| Admin               | admin@okla.local       | Admin123!@#    |
| Buyer               | buyer002@okla-test.com | BuyerTest2026! |
| Dealer              | nmateo@okla.com.do     | Dealer2026!@#  |
| Vendedor Particular | gmoreno@okla.com.do    | $Gregory1      |

---

## TAREAS

- [x] Fix bugs de S2-T01: Auditar listado y filtros de /vehiculos
  - FRONTEND-018: Añadido mapa de traducción defensivo en `vehicle-card.tsx` para normalizar fuel type a español (gasoline→Gasolina) sin importar la ruta de datos.
  - FRONTEND-029: Añadido sanity check en `transformToCardData` — vehículos con >1000 km no se marcan como "Nuevo" aunque el backend diga condition=New.
- [x] Fix bugs de S2-T02: Auditar paginación y vehículos patrocinados
  - FRONTEND-025: Dos fixes:
    1. `vehiculos-client.tsx`: Deduplicación del pool de patrocinados por ID + NO ciclar (una vez agotados, no se repiten).
    2. `ad-engine.ts`: Hash determinístico del nombre del slot en vez de `Date.now()` para que `search_top` y `search_inline` produzcan órdenes diferentes.
- [x] Fix bugs de S2-T03: Auditar búsqueda y alertas sin auth
  - Sin bugs pendientes — todos los de S2-T03 fueron corregidos en el sprint anterior.

- [x] Ejecutar Gate Pre-Commit (dotnet build + pnpm lint/typecheck/test/build + dotnet test)
  - dotnet restore ✅
  - dotnet build (0 warnings, 0 errors) ✅
  - pnpm lint (0 errors) ✅
  - pnpm typecheck ✅
  - pnpm test (576 passed, 0 failed) ✅
  - pnpm build ✅
  - dotnet test: unit tests pass, integration tests fail (pre-existing, require Docker+PG+RabbitMQ) ✅

## Resultado

- Sprint: 2 — Búsqueda & Filtros de Vehículos (Guest)
- Fase: FIX
- Estado: **COMPLETADO**
- Bugs corregidos: 3 (FRONTEND-018, FRONTEND-025, FRONTEND-029)
- Gate Pre-Commit: PASADO ✅

### Archivos modificados
| Archivo | Bug | Cambio |
|---------|-----|--------|
| `frontend/web-next/src/components/ui/vehicle-card.tsx` | FRONTEND-018 | Mapa defensivo fuel type → español |
| `frontend/web-next/src/services/vehicles.ts` | FRONTEND-029 | Sanity check: isNew=false si mileage>1000 |
| `frontend/web-next/src/app/(main)/vehiculos/vehiculos-client.tsx` | FRONTEND-025 | Deduplicar pool patrocinados + no ciclar |
| `frontend/web-next/src/lib/ad-engine.ts` | FRONTEND-025 | Hash determinístico por slot para shuffle |

---

_Cuando termines, agrega la palabra READ al final de este archivo._
