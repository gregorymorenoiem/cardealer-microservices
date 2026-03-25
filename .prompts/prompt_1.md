# CORRECCIÓN (Intento 2/3) — Sprint 1: Homepage & Navegación Pública (Guest)
**Fecha:** 2026-03-25 07:41:19
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
| Rol | Email | Password |
|-----|-------|----------|
| Admin | admin@okla.local | Admin123!@# |
| Buyer | buyer002@okla-test.com | BuyerTest2026! |
| Dealer | nmateo@okla.com.do | Dealer2026!@# |
| Vendedor Particular | gmoreno@okla.com.do | $Gregory1 |

---

## TAREAS

- [x] Fix bugs de S1-T01: Auditar Homepage completa
- [x] Fix bugs de S1-T02: Auditar Navbar y Footer
- [x] Fix bugs de S1-T03: Auditar sección de Concesionarios y Carruseles
- [x] Fix bugs de S1-T04: Auditar responsive mobile

- [x] Ejecutar Gate Pre-Commit (dotnet build + pnpm lint/typecheck/test/build + dotnet test)

## Resultado
- Sprint: 1 — Homepage & Navegación Pública (Guest)
- Fase: FIX (Intento 2/3)
- Estado: COMPLETADO
- Bugs corregidos: 3

### Fixes aplicados:

| Bug | Fix | Archivo |
|-----|-----|---------|
| P0-005 | Frontend: filtro E2E en VehicleTypeSection y FeaturedVehicles (excluye títulos con "E2E" o "DO NOT BUY") | vehicle-type-section.tsx, featured-vehicles.tsx |
| P0-007 | Frontend: dedup por make+model+year en VehicleTypeSection y FeaturedVehicles (no solo por ID) | vehicle-type-section.tsx, featured-vehicles.tsx |
| P0-008 | Frontend: normalizeLocationName ahora normaliza `state` también + regex fallback para "DomingoNorte/Este/Oeste" | utils.ts, vehicles.ts, vehicle-type-section.tsx, featured-vehicles.tsx |

### Gate Pre-Commit:
- dotnet build: 0 errors, 0 warnings ✅
- pnpm lint: 0 errors ✅
- pnpm typecheck: pass ✅
- pnpm test: 576/576 ✅
- pnpm build: compiled ✅
- dotnet test: unit tests pass ✅

---

_Cuando termines, agrega la palabra READ al final de este archivo._

READ
