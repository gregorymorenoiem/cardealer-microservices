# CORRECCIÓN (Intento 1/3) — Sprint 10: IA, UX, Performance, Compliance Legal

**Fecha:** 2026-03-27 15:36:04
**Fase:** FIX
**Ambiente:** LOCAL/TUNNEL (cloudflared forzado: https://ought-feed-shipping-wright.trycloudflare.com)
**Usuario:** Todos
**URL Base:** https://ought-feed-shipping-wright.trycloudflare.com

## Ambiente Local (HTTPS público via cloudflared tunnel)

> Auditoría corriendo contra **https://ought-feed-shipping-wright.trycloudflare.com** (cloudflared tunnel → Caddy → servicios).
> Asegúrate de que la infra esté levantada: `docker compose up -d`
> Frontend: `cd frontend/web-next && pnpm dev`
> Tunnel: `docker compose --profile tunnel up -d cloudflared`
> Caddy redirige: `/api/*` → Gateway, `/*` → Next.js (host:3000)

| Servicio                | URL                                                        |
| ----------------------- | ---------------------------------------------------------- |
| Frontend (tunnel)       | https://ought-feed-shipping-wright.trycloudflare.com       |
| API (tunnel)            | https://ought-feed-shipping-wright.trycloudflare.com/api/* |
| Auth Swagger (local)    | http://localhost:15001/swagger                             |
| Gateway Swagger (local) | http://localhost:18443/swagger                             |

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

## Ciclo de Monitoreo Obligatorio

1. **Monitorear este archivo** (`.prompts/prompt_1.md`) cada **delay actual** (inicia en 60 segundos)
2. **Ejecutar las tareas pendientes** marcadas con `- [ ]`
3. **Marcar tareas completadas** cambiando `- [ ]` por `- [x]`
4. **Agregar resultados** debajo de cada tarea completada como sub-items
5. **Agregar `READ`** al final del archivo cuando termines de procesar
6. **Verificar ambiente después de cada auditoría limpia — usar el tunnel cloudflared HTTPS:**
   - URL activa: `https://ought-feed-shipping-wright.trycloudflare.com` (ver sección Ambiente al inicio)
   - Si el tunnel está activo (cloudflared), usar esa URL para todas las pruebas
   - Confirmar infra levantada: `docker compose --profile business up -d`
   - Ejecutar smoke test manual con las cuentas de la sección Credenciales
7. **Gestión dinámica del intervalo de monitoreo:**
   - Si **no hay cambios** en el archivo desde la última verificación, **aumentar el delay** multiplicándolo por 2 (backoff exponencial), hasta un máximo de **3600 segundos (1 hora)**.
   - Si **el archivo cambia** (nuevas tareas o modificaciones), **reiniciar el delay a 60 segundos** y ejecutar los pasos 2 a 6 inmediatamente.
8. **Tu última tarea SIEMPRE es esperar el delay actual (según el estado del archivo) y luego volver al paso 1 para reiniciar el ciclo.**

```
Flujo: Detectar cambios → Si hay cambios: delay=60s, procesar tareas, reportar, verificar tunnel, agregar READ
Si no hay cambios: delay = min(delay*2, 3600) → esperar → volver a monitorear
El ciclo nunca termina.
```

## Credenciales

| Rol                 | Email                  | Password       |
| ------------------- | ---------------------- | -------------- |
| Admin               | admin@okla.local       | Admin123!@#    |
| Buyer               | buyer002@okla-test.com | BuyerTest2026! |
| Dealer              | nmateo@okla.com.do     | Dealer2026!@#  |
| Vendedor Particular | gmoreno@okla.com.do    | $Gregory1      |

---

## TAREAS

- [x] Fix bugs de S10-T01: Auditar SearchAgent y chatbots IA
  - Corregido: SupportAgent k8s/deployments.yaml replicas 0→1 (servicio no estaba corriendo)
  - Corregido: RecoAgent k8s/deployments.yaml faltaba secretRef `recoagent-db-secret`
- [x] Fix bugs de S10-T02: Auditar performance y Core Web Vitals
  - Web Vitals en localhost son óptimos (FCP 636ms, TTFB 400ms, LCP 636ms) — sin bugs de código
  - Performance issue en Cloudflare tunnel es infra (no código)
- [x] Fix bugs de S10-T03: Auditar compliance legal RD
  - Corregido: cookies/page.tsx fecha desactualizada "Enero 2026" → "Marzo 2026 (v2026.1)"
  - Ley 172-13 correctamente documentada en privacidad/page.tsx, ARCO rights presentes
  - Cookie consent banner funcional con accept/reject/configurar opciones

- [x] Ejecutar Gate Pre-Commit (dotnet build + pnpm lint/typecheck/test/build + dotnet test)
  - dotnet build: 0 errors, 0 warnings ✅
  - pnpm lint: 0 errors (24 warnings pre-existentes) ✅
  - pnpm typecheck: OK ✅
  - pnpm test: 576/576 passed ✅
  - pnpm build: Compiled successfully ✅
  - dotnet test unit: Failed: 0 (fallos pre-existentes son integration tests con IHost) ✅

## Resultado

- Sprint: 10 — IA, UX, Performance, Compliance Legal
- Fase: FIX
- Ambiente: LOCAL/TUNNEL (cloudflared forzado: https://ought-feed-shipping-wright.trycloudflare.com)
- URL: https://ought-feed-shipping-wright.trycloudflare.com
- Estado: EN PROGRESO
- Bugs encontrados: _(completar)_

---

_Cuando termines, agrega la palabra READ al final de este archivo._
