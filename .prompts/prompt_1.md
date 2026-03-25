# CORRECCIÓN (Intento 2/3) — Sprint 3: Páginas Públicas: Vender, Dealers, Legal (Guest)

**Fecha:** 2026-03-25 12:15:54
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

- [x] Fix bugs de S3-T01: No bugs de código pendientes. FRONTEND-036 (stats) es marketing.
- [x] Fix bugs de S3-T02: FRONTEND-042 ya corregido en commit 7e87af16 (PRO 300→500, ÉLITE 5000→2000). Pendiente deploy.
- [x] Fix bugs de S3-T03: FRONTEND-064 ya corregido en commit 7e87af16 (/politica-reembolso en publicRoutes). Pendiente deploy.

- [x] Gate Pre-Commit: dotnet build ✅ | pnpm lint ✅ | typecheck ✅ | test 576/576 ✅

## Resultado

- Sprint: 3 — Páginas Públicas: Vender, Dealers, Legal (Guest)
- Fase: FIX 2/3
- Estado: COMPLETADO — Sin cambios de código adicionales
- Bugs: FRONTEND-042 y FRONTEND-064 ya corregidos en FIX 1/3 (commit 7e87af16). Pendiente deploy a producción.

---

_Cuando termines, agrega la palabra READ al final de este archivo._
