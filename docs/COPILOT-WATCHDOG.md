# Copilot Watchdog — Documentación

Servicio de supervisión automática del agente GitHub Copilot en VS Code.  
Corre en background y reacciona cuando Copilot se detiene, se traba o VS Code deja de responder.

---

## Cómo funciona

```
┌─────────────────────────────────────────────────────────────┐
│                   copilot-watchdog.mjs                      │
│                  (loop cada 30 segundos)                    │
└──────────────────────────┬──────────────────────────────────┘
                           │
           ┌───────────────▼───────────────┐
           │   ¿VS Code está corriendo?    │
           └───────┬───────────────┬───────┘
                 NO │               │ SÍ
                   ▼               ▼
           ┌──────────────┐  ┌─────────────────────────────┐
           │ REINICIAR    │  │ ¿Hubo cambios en archivos?  │
           │ VS Code      │  └──────┬──────────────┬────────┘
           └──────────────┘       SÍ │              │ NO
                                     ▼              ▼
                               ┌──────────┐  ┌───────────────────────┐
                               │  OK      │  │ ¿Cuánto tiempo lleva  │
                               │ No hacer │  │ sin actividad?        │
                               │ nada     │  └──────┬────────┬────────┘
                               └──────────┘      <5min │      │ >5min
                                                       ▼      ▼
                                               ┌───────────┐ ┌─────────────────┐
                                               │ Esperar   │ │ Enviar          │
                                               │           │ │ "continuar"     │
                                               └───────────┘ │ al chat         │
                                                             └────────┬────────┘
                                                                      │ Si pasan
                                                                      │ 10min más
                                                                      ▼
                                                             ┌─────────────────┐
                                                             │ Abrir NUEVO     │
                                                             │ CHAT + enviar   │
                                                             │ prompt completo │
                                                             └─────────────────┘
```

### Reglas de decisión

| Condición | Tiempo | Acción |
|-----------|--------|--------|
| VS Code no está corriendo | — | Reinicia VS Code |
| Archivos del proyecto cambiaron | — | OK, no hace nada |
| Sin cambios en archivos | > 5 min | Envía `"continuar"` al chat de Copilot |
| Sin cambios tras enviar "continuar" | > 10 min total | Abre nuevo chat + envía prompt completo |

### Detección de actividad

El watchdog **no lee el chat de Copilot directamente** — monitorea cambios de archivos en el workspace del proyecto (`mtime`). Si Copilot está trabajando activamente, los archivos del proyecto van a cambiar. Si no hay cambios, asume que el agente se detuvo.

Directorios ignorados: `.git`, `node_modules`, `bin`, `obj`, `.next`, `dist`, `__pycache__`, `.turbo`

### Interacción con VS Code

Usa AppleScript para:
1. Traer VS Code al frente (`activate`)
2. Abrir el panel de Copilot Chat (`Cmd+Ctrl+I`)
3. Pegar texto vía portapapeles y presionar Enter

**Requisito:** Terminal (o `node`) debe tener permiso de **Accesibilidad** en `Configuración del Sistema → Privacidad y Seguridad → Accesibilidad`.

---

## Archivos del sistema

| Archivo | Descripción |
|---------|-------------|
| `~/.openclaw/workspace/copilot-watchdog.mjs` | Script principal |
| `~/Library/LaunchAgents/com.openclaw.copilot-watchdog.plist` | Servicio launchd (arranca con el login) |
| `~/.openclaw/logs/copilot-watchdog.log` | Log de salida estándar |
| `~/.openclaw/logs/copilot-watchdog.err.log` | Log de errores |
| `/tmp/copilot-watchdog.pid` | PID del proceso activo |
| `/tmp/copilot-watchdog-state.json` | Estado interno (último cambio, contadores) |
| `/tmp/copilot-watchdog.hibernate` | **Archivo de hibernación** — si existe, el watchdog no actúa |

---

## Comandos

### Activar / despertar

```bash
watchdog start
```

O con opciones manuales:
```bash
node ~/.openclaw/workspace/copilot-watchdog.mjs \
  --workspace /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices \
  --prompt "Continúa la auditoría QA desde el Proceso 2"
```

### Hibernar (no interactúa con VS Code, sigue corriendo)

```bash
watchdog hibernate
```

Esto crea el archivo `/tmp/copilot-watchdog.hibernate`. El proceso sigue vivo pero no envía mensajes ni reinicia VS Code. Útil cuando quieres trabajar manualmente en VS Code sin interferencia.

### Despertar de hibernación

```bash
watchdog wake
```

### Detener completamente

```bash
watchdog stop
```

### Ver estado

```bash
watchdog status
```

### Ver logs en vivo

```bash
watchdog logs
```

### Cambiar el prompt de recuperación

```bash
watchdog set-prompt "Continúa la auditoría QA, revisa prompt_1.md y retoma desde el Proceso 3"
```

---

## Servicio launchd

El watchdog se registra como servicio del sistema macOS (`launchd`), lo que significa:

- **Arranca automáticamente** al iniciar sesión
- **Se reinicia solo** si el proceso crashea (ThrottleInterval: 5s)
- **No requiere terminal abierta** para funcionar

### Gestión del servicio

```bash
# Detener el servicio
launchctl stop com.openclaw.copilot-watchdog

# Iniciar el servicio
launchctl start com.openclaw.copilot-watchdog

# Deshabilitar (no arranca en el próximo login)
launchctl unload ~/Library/LaunchAgents/com.openclaw.copilot-watchdog.plist

# Volver a habilitar
launchctl load ~/Library/LaunchAgents/com.openclaw.copilot-watchdog.plist
```

---

## Integración con OpenClaw

El watchdog es independiente de OpenClaw pero complementario:

- **OpenClaw** gestiona los modelos de IA (`gpt-4.1` → `gpt-4o` → `gpt-4o-mini`) y ejecuta los agentes
- **Watchdog** supervisa que VS Code/Copilot siga activo y reacciona si se detiene
- **Modelo activo:** `github-copilot/gpt-4.1` (primario), con fallback automático a `gpt-4o` y `gpt-4o-mini`

### Cadena de fallback de modelos

```
github-copilot/gpt-4.1  ──falla──▶  github-copilot/gpt-4o  ──falla──▶  github-copilot/gpt-4o-mini
```

Config en: `~/.openclaw/openclaw.json`

---

## Solución de problemas

### El watchdog no envía mensajes a VS Code

**Causa:** Falta el permiso de Accesibilidad.  
**Solución:**
```bash
open "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility"
```
Agrega **Terminal** (o **iTerm**) a la lista.

### El watchdog está hibernando pero no recuerdo haberlo puesto así

```bash
watchdog status   # muestra si está hibernando
watchdog wake     # lo despierta
```

### Ver qué está haciendo en tiempo real

```bash
watchdog logs
```

### Cambiar los tiempos de espera

Edita las constantes en `~/.openclaw/workspace/copilot-watchdog.mjs`:
```js
checkIntervalMs:  30_000,   // Revisar cada N ms (default: 30s)
continuarAfterMs: 5 * 60_000,  // Enviar "continuar" después de N ms (default: 5min)
newChatAfterMs:  10 * 60_000,  // Abrir nuevo chat después de N ms (default: 10min)
```

Luego reinicia:
```bash
watchdog stop && watchdog start
```
