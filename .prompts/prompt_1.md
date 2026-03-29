# CORRECCIÓN (Intento 3/3) — Sprint 6: Seller — Publicar Mi Primer Vehículo
**Fecha:** 2026-03-29 14:22:10
**Fase:** FIX
**Ambiente:** LOCAL/TUNNEL (cloudflared forzado: https://approximately-sterling-disable-alias.trycloudflare.com)
**Usuario:** Seller (gmoreno@okla.com.do / $Gregory1)
**URL Base:** https://approximately-sterling-disable-alias.trycloudflare.com

## Ambiente Local (HTTPS público via cloudflared tunnel)
> Auditoría corriendo contra **https://approximately-sterling-disable-alias.trycloudflare.com** (cloudflared tunnel → Caddy → servicios).
> Asegúrate de que la infra esté levantada: `docker compose up -d`
> Frontend: `cd frontend/web-next && pnpm dev`
> Tunnel: `docker compose --profile tunnel up -d cloudflared`
> Caddy redirige: `/api/*` → Gateway, `/*` → Next.js (host:3000)

| Servicio | URL |
|----------|-----|
| Frontend (tunnel) | https://approximately-sterling-disable-alias.trycloudflare.com |
| API (tunnel) | https://approximately-sterling-disable-alias.trycloudflare.com/api/* |
| Auth Swagger (local) | http://localhost:15001/swagger |
| Gateway Swagger (local) | http://localhost:18443/swagger |

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

- [ ] Fix bugs de S6-T01: Wizard de publicación paso a paso
- [ ] Fix bugs de S6-T02: Dashboard del vendedor

- [ ] Ejecutar Gate Pre-Commit (dotnet build + pnpm lint/typecheck/test/build + dotnet test)

## Resultado
- Sprint: 6 — Seller — Publicar Mi Primer Vehículo
- Fase: FIX
- Ambiente: LOCAL/TUNNEL (cloudflared forzado: https://approximately-sterling-disable-alias.trycloudflare.com)
- URL: https://approximately-sterling-disable-alias.trycloudflare.com
- Estado: EN PROGRESO
- Bugs encontrados: _(completar)_

---

_Cuando termines, agrega la palabra READ al final de este archivo._
