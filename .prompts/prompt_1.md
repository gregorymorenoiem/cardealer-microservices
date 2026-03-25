# CORRECCIÓN (Intento 1/3) — Sprint 4: Login & Registro (Todos los Usuarios)

**Fecha:** 2026-03-25 13:17:25
**Fase:** FIX
**Usuario:** Guest → Buyer, Seller, Dealer
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

- [x] Fix bugs de S4-T01: Auditar página de Login
  - BUG-S4-001: Mensajes de error en inglés traducidos a español en LoginCommandHandler.cs
- [x] Fix bugs de S4-T02: Auditar Login como BUYER — Sin bugs específicos
- [x] Fix bugs de S4-T03: Auditar Login como SELLER — Sin bugs específicos
- [x] Fix bugs de S4-T04: Auditar Login como DEALER
  - BUG-S4-002: Botón cookies movido de left-4 a right-4 en cookie-consent.tsx
- [x] Fix bugs de S4-T05: Auditar página de Registro — Sin bugs de registro

- [x] Ejecutar Gate Pre-Commit (dotnet build + pnpm lint/typecheck/test/build + dotnet test)

## Resultado

- Sprint: 4 — Login & Registro (Todos los Usuarios)
- Fase: FIX (Intento 1/3)
- Estado: COMPLETADO
- Bugs corregidos: 2 (BUG-S4-001 mensajes inglés→español, BUG-S4-002 cookie overlap)

---

_Cuando termines, agrega la palabra READ al final de este archivo._

READ
