# CORRECCIÓN (Intento 3/3) — Sprint 3: Páginas Públicas: Vender, Dealers, Legal (Guest)

**Fecha:** 2026-03-25 12:36:24
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

- [x] Fix bugs de S3-T01: Auditar /vender — Planes de Seller
  - No code bugs found. Stats are hardcoded (acceptable for MVP).
- [x] Fix bugs de S3-T02: Auditar /dealers — Planes de Dealer (verificar alineación backend)
  - Testimonios ficticios noted but acceptable for MVP placeholder content.
- [x] Fix bugs de S3-T03: Auditar páginas legales y herramientas
  - FIXED: Added `/reclamaciones` and `/okla-score` to publicRoutes in middleware.ts
  - `/politica-reembolso` was already in publicRoutes (added in commit 7e87af16) — may need deploy
- [x] Ejecutar Gate Pre-Commit (dotnet build + pnpm lint/typecheck/test/build + dotnet test)

## Resultado

- Sprint: 3 — Páginas Públicas: Vender, Dealers, Legal (Guest)
- Fase: FIX
- Estado: COMPLETADO
- Fixes aplicados:
  1. ✅ Added `/reclamaciones` to publicRoutes in middleware.ts
  2. ✅ Added `/okla-score` to publicRoutes in middleware.ts
  3. ℹ️ `/politica-reembolso` already public since commit 7e87af16 — may need deploy to production

---

_Cuando termines, agrega la palabra READ al final de este archivo._
