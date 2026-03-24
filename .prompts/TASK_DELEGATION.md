# OKLA — Delegación de Tareas: VS Code (Copilot) vs OpenClaw (Terminal)

> **Fecha:** 2026-03-24  
> **Modelos:** VS Code → Claude Sonnet 4.6 · OpenClaw Terminal → Claude Haiku 4.5  
> **Proyecto:** OKLA Marketplace — cardealer-microservices

---

## 🦞 ¿Qué es OpenClaw?

[OpenClaw](https://docs.openclaw.ai) es un agente de IA de terminal que corre localmente en macOS. Características clave:

| Feature               | Descripción                                                                                |
| --------------------- | ------------------------------------------------------------------------------------------ |
| **Agentes aislados**  | Múltiples agentes con modelos diferentes (`main` con Sonnet 4, `dev-senior` con Haiku 4.5) |
| **Browser integrado** | Chrome headless controlado por IA (`openclaw browser navigate/click/fill/screenshot`)      |
| **Cron scheduler**    | Tareas recurrentes via gateway (`openclaw cron add --every 1m --agent dev-senior`)         |
| **Multi-canal**       | Telegram, Discord, WhatsApp, Slack, Signal, iMessage                                       |
| **Gateway local**     | WebSocket en `ws://127.0.0.1:18789` con auth HMAC SHA256                                   |
| **Sesiones**          | Memoria persistente por sesión, contexto de conversación                                   |
| **Tool use**          | File system, shell, browser, HTTP, y extensiones custom                                    |
| **ACP**               | Agent Control Protocol para orquestación de agentes                                        |

### Agentes Configurados

| Agent ID     | Modelo                            | Rol                                            |
| ------------ | --------------------------------- | ---------------------------------------------- |
| `main`       | `github-copilot/claude-sonnet-4`  | Agente principal (tareas complejas)            |
| `dev-senior` | `github-copilot/claude-haiku-4.5` | Developer senior (monitoreo, auditoría, CI/CD) |

---

## 🧠 Filosofía de Delegación

| Dimensión       | VS Code (Copilot Sonnet 4.6)                              | OpenClaw (Haiku 4.5)                                   |
| --------------- | --------------------------------------------------------- | ------------------------------------------------------ |
| **Fortaleza**   | Razonamiento profundo, refactoring complejo, arquitectura | Velocidad, tareas repetitivas, automatización, browser |
| **Contexto**    | Workspace completo (multi-root, LSP, tipos)               | Archivos individuales, CLI, browser headless           |
| **Costo/token** | ~6x más caro que Haiku                                    | Ultra-económico, alto throughput                       |
| **Latencia**    | 2-5s por respuesta                                        | ~0.5-1s por respuesta                                  |
| **Ideal para**  | Pensar, diseñar, corregir bugs complejos                  | Ejecutar, verificar, monitorear, auditar               |

---

## ✅ Tareas para VS Code (GitHub Copilot — Claude Sonnet 4.6)

### 🏗️ Arquitectura y Diseño

- [ ] Diseñar nuevos microservicios (DDD, Clean Architecture, CQRS)
- [ ] Refactorizar código existente con análisis de impacto
- [ ] Crear interfaces, abstracciones y contratos entre servicios
- [ ] Diseñar esquemas de base de datos (migraciones EF Core)
- [ ] Definir eventos de dominio y handlers (RabbitMQ)
- [ ] Diseñar API contracts (REST/gRPC/GraphQL)

### 🐛 Debugging Complejo

- [ ] Diagnosticar bugs multi-servicio (seguir flujo de datos completo)
- [ ] Resolver errores TypeScript complejos (genéricos, type narrowing)
- [ ] Fixing race conditions, memory leaks, deadlocks
- [ ] Debugging de queries EF Core con N+1, lazy loading issues
- [ ] Resolver conflictos de merge complejos

### 🔧 Implementación de Features

- [ ] Implementar nuevos endpoints en backend (.NET 8)
- [ ] Crear componentes React/Next.js complejos (con estado, hooks custom)
- [ ] Implementar lógica de negocio en Application layer (MediatR handlers)
- [ ] Crear/modificar system prompts para agentes IA (ChatbotService)
- [ ] Implementar middleware, filtros, validaciones
- [ ] Configurar autenticación/autorización (JWT, RBAC, policies)

### 🧪 Testing y Quality

- [ ] Escribir tests unitarios (xUnit, Vitest)
- [ ] Diseñar tests de integración
- [ ] Corregir tests que fallan (entender el "por qué")
- [ ] Code review de PRs con análisis de seguridad
- [ ] Ejecutar y corregir el Gate Pre-Commit completo (8 pasos)

### 📐 Frontend Avanzado

- [ ] Crear páginas con App Router (layouts, loading, error boundaries)
- [ ] Implementar formularios complejos (react-hook-form, Zod validation)
- [ ] Optimizar performance (React.memo, useMemo, code splitting)
- [ ] Implementar TanStack Query mutations con optimistic updates
- [ ] Configurar Tailwind v4, shadcn/ui components custom

### 🔐 Seguridad

- [ ] Auditoría OWASP Top 10 del código
- [ ] Implementar rate limiting, CORS, CSP headers
- [ ] Revisar prompt injection protection en chatbots
- [ ] Configurar secretos y variables de entorno
- [ ] Implementar KYC flows y validaciones

### 📊 Estrategia y Producto

- [ ] Analizar métricas y KPIs de la plataforma
- [ ] Diseñar pricing models y unit economics
- [ ] Planificar sprints con OKRs
- [ ] Documentar decisiones arquitectónicas (ADRs)

---

## ⚡ Tareas para OpenClaw (Terminal — Claude Haiku 4.5)

### 🔍 Auditoría Automatizada de Producción

- [ ] Auditar agentes IA en producción (SearchAgent, DealerChat, Pricing, etc.)
- [ ] Navegar con Chrome headless y capturar errores de consola
- [ ] Verificar respuestas HTTP (status codes, tiempos de respuesta)
- [ ] Probar flujos de usuario end-to-end en el browser
- [ ] Capturar screenshots de evidencia
- [ ] Verificar deployments post-CI/CD en producción

### 🤖 Monitoreo Continuo

- [ ] Monitorear `.prompts/prompt_6.md` cada 60s para tareas nuevas
- [ ] Monitorear `.prompts/prompt_1.md` como bridge de tareas VS Code → OpenClaw
- [ ] Health checks periódicos a `https://okla.com.do`
- [ ] Verificar estado de pods en DigitalOcean (via API)
- [ ] Monitorear métricas LLM Gateway (`/api/admin/llm-gateway/health`)

### 🔄 CI/CD y DevOps

- [ ] Ejecutar `gh workflow run smart-cicd.yml --ref main`
- [ ] Monitorear estado de pipelines en GitHub Actions
- [ ] Verificar que `deploy-digitalocean.yml` se ejecuta post-CI
- [ ] Validar que los pods están healthy post-deploy
- [ ] Ejecutar rollback si detecta errores en producción

### 🧹 Tareas Repetitivas y Batch

- [ ] Formatear y lint de archivos en batch
- [ ] Ejecutar suites de tests y reportar resultados
- [ ] Generar reportes de auditoría en Markdown
- [ ] Actualizar logs de auditoría (`.github/copilot-audit.log`)
- [ ] Limpiar y organizar archivos temporales
- [ ] Sincronizar lockfiles (`pnpm install`)

### 🌐 Browser Testing (OpenClaw Chrome)

- [ ] Probar login/logout con diferentes roles
- [ ] Verificar UI de búsqueda de vehículos
- [ ] Probar chat con dealers (enviar mensajes)
- [ ] Verificar página de pricing y planes
- [ ] Probar flujo de publicación de vehículos
- [ ] Capturar console errors en cada página

### 📡 Comunicación y Alertas

- [ ] Enviar notificaciones via Telegram/Discord/WhatsApp
- [ ] Reportar status de auditorías al equipo
- [ ] Alertar sobre errores críticos en producción
- [ ] Enviar resúmenes diarios de actividad

### ⏰ Tareas Programadas (Cron)

- [ ] Auditoría de salud cada 30 minutos
- [ ] Verificación de SSL/certificados diaria
- [ ] Reporte de métricas LLM cada hora
- [ ] Limpieza de logs de auditoría semanal

---

## 🔗 Comunicación VS Code ↔ OpenClaw (Bridge Pattern)

```
┌─────────────────────┐     .prompts/prompt_1.md      ┌──────────────────────┐
│                     │  ──────────────────────────►   │                      │
│   VS Code           │    (escribe tareas nuevas)     │   OpenClaw           │
│   Copilot           │                                │   Haiku 4.5          │
│   Sonnet 4.6        │  ◄──────────────────────────   │                      │
│                     │    (agrega "READ" + resultados)│                      │
└─────────────────────┘                                └──────────────────────┘
         │                                                       │
         │  .prompts/prompt_6.md                                 │
         │  (OpenClaw monitorea cada 60s)                        │
         │                                                       │
         ▼                                                       ▼
   Implementa fixes                                     Ejecuta auditorías
   Corre gate pre-commit                               Corre CI/CD
   Diseña features                                     Verifica producción
   Escribe tests                                       Genera reportes
```

### Flujo de Trabajo Coordinado

1. **VS Code** diseña e implementa cambios
2. **VS Code** ejecuta el Gate Pre-Commit (8 pasos)
3. **VS Code** hace commit + push
4. **VS Code** escribe tarea en `.prompts/prompt_1.md`: `"Ejecutar CI/CD y verificar producción"`
5. **OpenClaw** detecta el cambio (monitoreo cada 60s)
6. **OpenClaw** ejecuta `gh workflow run smart-cicd.yml`
7. **OpenClaw** espera deploy y verifica producción con Chrome
8. **OpenClaw** escribe resultado en `.prompts/prompt_1.md` + `"READ"`
9. **VS Code** lee el resultado y decide siguiente acción

---

## 📋 Reglas de Oro

| #   | Regla                                                                                       | Aplica a |
| --- | ------------------------------------------------------------------------------------------- | -------- |
| 1   | **Nunca duplicar trabajo** — si Copilot está editando un archivo, OpenClaw no lo toca       | Ambos    |
| 2   | **El archivo bridge es sagrado** — `.prompts/prompt_1.md` es el único canal de comunicación | Ambos    |
| 3   | **Copilot piensa, OpenClaw ejecuta** — las decisiones arquitectónicas son de Sonnet         | VS Code  |
| 4   | **OpenClaw reporta, no decide** — si encuentra un bug, lo reporta; Copilot lo arregla       | OpenClaw |
| 5   | **Gate Pre-Commit siempre en VS Code** — los 8 pasos requieren contexto del workspace       | VS Code  |
| 6   | **CI/CD siempre via OpenClaw** — `gh workflow run` es una tarea CLI perfecta para Haiku     | OpenClaw |
| 7   | **Auditorías de producción = OpenClaw** — browser testing es su dominio                     | OpenClaw |
| 8   | **`READ` es el acknowledgment** — sin `READ`, la tarea no fue procesada                     | OpenClaw |
| 9   | **Delay de 30s en Copilot** — esperar antes de leer respuesta de OpenClaw                   | VS Code  |
| 10  | **Última tarea = monitorear** — OpenClaw SIEMPRE termina monitoreando prompt_1.md           | OpenClaw |

---

## ⚡ Comandos Rápidos de Referencia

### VS Code → OpenClaw (escribir tarea)

```markdown
<!-- En .prompts/prompt_1.md -->

- [ ] Auditar SearchAgent en producción y reportar errores
- [ ] Ejecutar gh workflow run smart-cicd.yml --ref main
- [ ] Verificar pods post-deploy en https://okla.com.do
```

### OpenClaw → Terminal

```bash
# Monitorear prompt_1.md continuamente
openclaw cron add --name "monitor-prompt1" --every 1m \
  --agent dev-senior --message "Lee .prompts/prompt_1.md, ejecuta tareas pendientes y agrega READ"

# Auditar agentes IA
openclaw agent --agent dev-senior --message "Audita SearchAgent en https://okla.com.do/vehiculos"

# Ejecutar CI/CD
gh workflow run smart-cicd.yml --ref main
```

---

## 🏗️ Setup Completo del Sistema

### 1. Cron Job Activo (ya configurado)

```bash
# Verificar que el cron está activo
openclaw cron list --json 2>&1 | grep -v bedrock

# Output esperado:
# "name": "monitor-prompt1", "enabled": true, "agentId": "dev-senior", "everyMs": 60000
```

El cron `monitor-prompt1` ejecuta al agente `dev-senior` cada 60 segundos. Lee `.prompts/prompt_1.md`, ejecuta tareas pendientes, y agrega `READ`.

### 2. Scripts Disponibles

| Script               | Propósito                          | Ejecutar desde                  |
| -------------------- | ---------------------------------- | ------------------------------- |
| `bridge.py`          | Enviar tareas y esperar respuestas | VS Code terminal                |
| `monitor_prompt1.py` | Monitoreo continuo con polling     | Terminal (backup si cron falla) |
| `monitor_prompt6.py` | Auditoría de agentes IA            | OpenClaw terminal               |

### 3. Bridge: Enviar Tarea desde VS Code a OpenClaw

```bash
# Enviar tarea simple
python3 .prompts/bridge.py send "Auditar SearchAgent en producción"

# Enviar tarea y esperar respuesta (delay automático)
python3 .prompts/bridge.py send --wait "Ejecutar CI/CD y verificar producción"

# Verificar estado
python3 .prompts/bridge.py status

# Leer resultados
python3 .prompts/bridge.py results

# Solo esperar (si ya enviaste la tarea)
python3 .prompts/bridge.py wait --timeout 180
```

### 4. Delay Strategy (Timing)

```
VS Code escribe tarea ──► prompt_1.md actualizado (0s)
                              │
                              ▼
OpenClaw cron detecta ───► máx 60s (cron interval)
                              │
                              ▼
OpenClaw procesa ─────────► 10-300s (depende de tarea)
                              │
                              ▼
OpenClaw agrega READ ────► prompt_1.md actualizado
                              │
                              ▼
VS Code lee resultado ───► poll cada 15s, timeout 180s
```

- **Latencia mínima:** ~70s (60s cron + 10s tarea simple)
- **Latencia típica:** ~120s (60s cron + 60s tarea media)
- **Latencia máxima:** ~360s (60s cron + 300s tarea compleja)
- **VS Code delay mínimo recomendado:** 30s antes de leer respuesta

### 5. Manejo de Cron

```bash
# Ver todos los cron jobs
openclaw cron list

# Pausar monitoreo
openclaw cron disable monitor-prompt1

# Reanudar monitoreo
openclaw cron enable monitor-prompt1

# Ejecutar manualmente (debug)
openclaw cron run monitor-prompt1

# Eliminar cron job
openclaw cron rm monitor-prompt1

# Ver historial de ejecuciones
openclaw cron runs monitor-prompt1
```

### 6. Administrar Agente dev-senior

```bash
# Enviar mensaje directo al agente
openclaw agent --agent dev-senior --message "Tu tarea aquí" --thinking off --timeout 300

# Ver configuración
cat ~/.openclaw/agents/dev-senior/agent/models.json

# Ver sesiones activas
openclaw sessions --json

# Gestionar agentes
openclaw agents list
```
