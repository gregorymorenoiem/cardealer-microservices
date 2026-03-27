# macOS Bot Automation — Documentación

> Scripts de automatización para controlar macOS, Chrome y el chat de VS Code Copilot.  
> Ubicación: `.prompts/mac_bot.py` y `.prompts/vscode_copilot_monitor.py`

---

## Índice

1. [Requisitos](#1-requisitos)
2. [Instalación](#2-instalación)
3. [Permisos de macOS](#3-permisos-de-macos)
4. [mac_bot.py — Control de macOS](#4-mac_botpy--control-de-macos)
5. [vscode_copilot_monitor.py — Monitor del Chat](#5-vscode_copilot_monitorpy--monitor-del-chat)
6. [Tareas de VS Code](#6-tareas-de-vs-code)
7. [Referencia completa de comandos](#7-referencia-completa-de-comandos)

---

## 1. Requisitos

| Componente     | Versión mínima               |
| -------------- | ---------------------------- |
| macOS          | 12 Monterey o superior       |
| Python         | 3.11+                        |
| VS Code        | 1.85+ (para el monitor)      |
| GitHub Copilot | Extensión instalada y activa |

---

## 2. Instalación

```bash
# Desde la raíz del proyecto
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Activar el entorno virtual
source .venv/bin/activate

# Instalar dependencias
pip install pyautogui pillow pyobjc-framework-Vision
```

Dependencias que se instalan automáticamente con `pyautogui`:

- `pyobjc-core`, `pyobjc-framework-Cocoa`, `pyobjc-framework-quartz` — bindings nativos macOS
- `pyscreeze`, `pillow` — captura y procesamiento de imágenes
- `pyperclip` — manejo del portapapeles

---

## 3. Permisos de macOS

Los scripts necesitan dos permisos que macOS solicita la primera vez:

### Screen Recording (capturas de pantalla)

Requerido por: `mac_bot.py screenshot`, `vscode_copilot_monitor.py`

```
System Settings → Privacy & Security → Screen Recording
→ Activar: Terminal (o iTerm2, según el terminal que uses)
```

### Accessibility (control de teclado/mouse/apps)

Requerido por: `mac_bot.py` (mouse, teclado, control de apps), `vscode_copilot_monitor.py`

```
System Settings → Privacy & Security → Accessibility
→ Activar: Terminal (o iTerm2)
```

> **Nota:** Después de conceder permisos, reinicia el terminal para que surtan efecto.

---

## 4. mac_bot.py — Control de macOS

Control total del sistema: mouse, teclado, aplicaciones, Chrome y el portapapeles. Funciona desde cualquier terminal, independiente de VS Code.

### Uso base

```bash
source .venv/bin/activate
python3 .prompts/mac_bot.py [COMANDO] [OPCIONES]
```

### 4.1 Mouse

| Comando             | Descripción                      | Ejemplo                                            |
| ------------------- | -------------------------------- | -------------------------------------------------- |
| `pos`               | Posición actual del mouse        | `python3 .prompts/mac_bot.py pos`                  |
| `size`              | Resolución de pantalla           | `python3 .prompts/mac_bot.py size`                 |
| `monitor`           | Rastrear posición en tiempo real | `python3 .prompts/mac_bot.py monitor`              |
| `move X Y`          | Mover mouse a coordenadas        | `python3 .prompts/mac_bot.py move 500 300`         |
| `click [X Y]`       | Click (en posición o coords)     | `python3 .prompts/mac_bot.py click 500 300`        |
| `drag X1 Y1 X2 Y2`  | Arrastrar                        | `python3 .prompts/mac_bot.py drag 100 100 400 400` |
| `scroll X Y AMOUNT` | Scroll (+ arriba, - abajo)       | `python3 .prompts/mac_bot.py scroll 500 300 3`     |

**Opciones de `click`:**

```bash
python3 .prompts/mac_bot.py click 500 300 --button right   # Click derecho
python3 .prompts/mac_bot.py click 500 300 --clicks 2       # Doble click
python3 .prompts/mac_bot.py click 500 300 --button middle  # Click central
```

> **FAILSAFE:** Mover el mouse a la esquina superior-izquierda de la pantalla detiene el script inmediatamente.

### 4.2 Teclado

| Comando               | Descripción                                                | Ejemplo                                              |
| --------------------- | ---------------------------------------------------------- | ---------------------------------------------------- |
| `type "texto"`        | Escribir texto carácter por carácter                       | `python3 .prompts/mac_bot.py type "Hola mundo"`      |
| `paste "texto"`       | Pegar texto (vía clipboard, más rápido para textos largos) | `python3 .prompts/mac_bot.py paste "texto largo..."` |
| `hotkey KEY [KEY...]` | Atajo de teclado                                           | `python3 .prompts/mac_bot.py hotkey cmd c`           |
| `press KEY`           | Presionar tecla                                            | `python3 .prompts/mac_bot.py press enter`            |

**Atajos de teclado comunes:**

```bash
python3 .prompts/mac_bot.py hotkey cmd c          # Copiar
python3 .prompts/mac_bot.py hotkey cmd v          # Pegar
python3 .prompts/mac_bot.py hotkey cmd z          # Deshacer
python3 .prompts/mac_bot.py hotkey cmd shift 3    # Screenshot nativo macOS
python3 .prompts/mac_bot.py hotkey cmd tab        # Cambiar app
python3 .prompts/mac_bot.py press escape          # Escape
python3 .prompts/mac_bot.py press tab --times 3   # Tab x3
```

### 4.3 Screenshots

```bash
# Captura pantalla completa → .github/screenshots/
python3 .prompts/mac_bot.py screenshot
python3 .prompts/mac_bot.py screenshot --name "mi_captura"

# Captura región (x, y, ancho, alto)
python3 .prompts/mac_bot.py screenshot-region 0 0 800 600
python3 .prompts/mac_bot.py screenshot-region 1000 0 512 400 --name "panel_derecho"
```

Las capturas se guardan en `.github/screenshots/`.

### 4.4 Control de Aplicaciones (AppleScript)

```bash
python3 .prompts/mac_bot.py open "Finder"          # Abrir Finder
python3 .prompts/mac_bot.py open "Google Chrome"   # Abrir Chrome
python3 .prompts/mac_bot.py quit "Finder"          # Cerrar Finder
python3 .prompts/mac_bot.py frontmost             # App en primer plano
python3 .prompts/mac_bot.py apps                  # Listar todas las apps corriendo
```

### 4.5 Control de Chrome

```bash
python3 .prompts/mac_bot.py chrome-url https://okla.local        # Abrir URL
python3 .prompts/mac_bot.py chrome-reload                         # Recargar pestaña
python3 .prompts/mac_bot.py chrome-close-tab                      # Cerrar pestaña
python3 .prompts/mac_bot.py chrome-current-url                    # URL activa
python3 .prompts/mac_bot.py chrome-title                          # Título de la pestaña
python3 .prompts/mac_bot.py chrome-js "document.title"            # Ejecutar JS
python3 .prompts/mac_bot.py chrome-fill "#email" "test@okla.com"  # Rellenar input
```

### 4.6 Sistema

```bash
python3 .prompts/mac_bot.py notify "Título" "Mensaje"   # Notificación macOS
python3 .prompts/mac_bot.py ask "¿Tu nombre?"           # Diálogo de input
python3 .prompts/mac_bot.py confirm "¿Continuar?"       # Diálogo OK/Cancelar
python3 .prompts/mac_bot.py clipboard-get               # Leer portapapeles
python3 .prompts/mac_bot.py clipboard-set "texto"       # Escribir portapapeles
python3 .prompts/mac_bot.py applescript 'say "Hola"'    # AppleScript libre
```

### 4.7 Workflows predefinidos

```bash
# Abrir OKLA local en Chrome + screenshot automático
python3 .prompts/mac_bot.py okla-open

# Screenshots en mobile/tablet/desktop de una URL
python3 .prompts/mac_bot.py responsive https://okla.local
```

---

## 5. vscode_copilot_monitor.py — Monitor del Chat

Monitorea el chat de GitHub Copilot en VS Code mediante OCR de pantalla. Detecta dos situaciones y actúa automáticamente:

| Situación detectada                                     | Acción automática                              |
| ------------------------------------------------------- | ---------------------------------------------- |
| Copilot se detuvo por error                             | Escribe `continuar` en el chat activo          |
| Copilot terminó + `prompt_1.md` tiene READ estable 120s | Abre nuevo chat y envía `AGENT_LOOP_PROMPT.md` |

### Arquitectura

```
Ciclo cada N segundos
    │
    ├─ 1. screencapture → PNG
    ├─ 2. OCR panel chat (Vision framework, zona derecha ~32% de pantalla)
    ├─ 3. Clasificar estado:
    │      WORKING  → no intervenir (Copilot trabajando)
    │      ERROR    → enviar "continuar"
    │      DONE     → verificar prompt_1.md
    │      IDLE     → esperar
    ├─ 4. Leer última línea de prompt_1.md
    └─ 5. Actuar según lógica de decisión
```

### Uso

```bash
source .venv/bin/activate

# Loop automático continuo (recomendado)
python3 .prompts/vscode_copilot_monitor.py

# Loop con intervalo personalizado (segundos)
python3 .prompts/vscode_copilot_monitor.py --interval 30

# Un solo ciclo de diagnóstico
python3 .prompts/vscode_copilot_monitor.py --once

# Screenshot + OCR + estado (sin actuar) — útil para calibrar
python3 .prompts/vscode_copilot_monitor.py --screenshot --debug

# Acciones manuales inmediatas
python3 .prompts/vscode_copilot_monitor.py --action-continue    # Enviar "continuar"
python3 .prompts/vscode_copilot_monitor.py --action-new-chat    # Nuevo chat + prompt
```

### Patrones de detección OCR

**ERROR** — cualquiera de estas frases dispara `continuar`:

```
an error occurred · something went wrong · request failed
rate limit · context length exceeded · token limit
network error · timed out · try again · reintent
límite de contexto · error al procesar
```

**WORKING** — estas frases indican que Copilot está activo (no intervenir):

```
running · ejecutando · building · compilando
testing · probando · step N · paso N
thinking · procesando · analizando · evaluating
```

**DONE** — estas frases combinadas con `prompt_1.md=READ` por 120s disparan nuevo chat:

```
¿hay algo más? · is there anything else
task complete · tarea completada · all done
```

### Cooldowns (anti-spam)

| Acción                 | Cooldown               |
| ---------------------- | ---------------------- |
| Enviar "continuar"     | 90 segundos            |
| Abrir nuevo chat       | 180 segundos           |
| READ estable requerido | 120 segundos continuos |

### Archivos generados

| Archivo                        | Contenido                 |
| ------------------------------ | ------------------------- |
| `.github/screenshots/monitor/` | Screenshots de cada ciclo |
| `.github/copilot-monitor.log`  | Log completo del monitor  |

---

## 6. Tareas de VS Code

Abre con `Cmd+Shift+P` → **Tasks: Run Task**

### Monitor del Chat

| Tarea                                                      | Descripción                |
| ---------------------------------------------------------- | -------------------------- |
| `🧠 Copilot Monitor: Loop Continuo (cada 20s)`             | Loop automático completo   |
| `🧠 Copilot Monitor: Screenshot + OCR (diagnóstico)`       | Ver estado actual del chat |
| `🧠 Copilot Monitor: Enviar 'continuar' ahora`             | Acción manual inmediata    |
| `🧠 Copilot Monitor: Nuevo Chat + AGENT_LOOP_PROMPT ahora` | Reinicio manual del loop   |

### Control macOS

| Tarea                                         | Descripción                      |
| --------------------------------------------- | -------------------------------- |
| `🤖 Bot: Posición del Mouse (tiempo real)`    | Rastrear coordenadas X,Y         |
| `🤖 Bot: Screenshot Pantalla Completa`        | Captura → `.github/screenshots/` |
| `🤖 Bot: Abrir OKLA Local en Chrome`          | Workflow okla-open               |
| `🤖 Bot: Screenshots Responsive (OKLA Local)` | mobile/tablet/desktop            |
| `🤖 Bot: Listar Apps Corriendo`               | Lista de procesos GUI            |
| `🤖 Bot: Tamaño de Pantalla`                  | Resolución actual                |
| `🤖 Bot: URL Activa Chrome`                   | URL de la pestaña Chrome         |
| `🤖 Bot: Notificación macOS`                  | Notificación de prueba           |
| `🤖 Bot: Ayuda (todos los comandos)`          | Help completo                    |

---

## 7. Referencia completa de comandos

```
mac_bot.py COMANDOS
───────────────────────────────────────────────
Mouse
  pos                          Posición actual del mouse
  size                         Resolución de pantalla
  monitor                      Rastrear X,Y en tiempo real (Ctrl+C para parar)
  move X Y [--duration SEG]    Mover mouse
  click [X Y] [--button left|right|middle] [--clicks N]
  drag X1 Y1 X2 Y2             Arrastrar
  scroll X Y AMOUNT            Scroll

Teclado
  type "texto" [--interval SEG]  Escribir texto
  paste "texto"                  Pegar vía clipboard
  hotkey KEY [KEY...]            Atajo: cmd c, cmd v, etc.
  press KEY [--times N]          Presionar tecla

Screenshot
  screenshot [--name NOMBRE]
  screenshot-region X Y W H [--name NOMBRE]

Apps (AppleScript)
  open APP                     Abrir aplicación
  quit APP                     Cerrar aplicación
  frontmost                    App en primer plano
  apps                         Listar apps corriendo

Chrome (AppleScript)
  chrome-url URL               Abrir URL en nueva pestaña
  chrome-reload                Recargar pestaña activa
  chrome-close-tab             Cerrar pestaña activa
  chrome-current-url           URL de la pestaña activa
  chrome-title                 Título de la pestaña activa
  chrome-js "código"           Ejecutar JavaScript
  chrome-fill SELECTOR VALOR   Rellenar campo de formulario

Sistema
  notify TITLE MSG             Notificación nativa macOS
  ask "prompt" [--default X]   Diálogo de input
  confirm "prompt"             Diálogo OK/Cancelar
  clipboard-get                Leer portapapeles
  clipboard-set "texto"        Escribir portapapeles
  applescript "script"         Ejecutar AppleScript libre

Workflows
  okla-open                    Abrir https://okla.local + screenshot
  responsive URL               Screenshots mobile/tablet/desktop

vscode_copilot_monitor.py OPCIONES
───────────────────────────────────────────────
  (sin args)                   Loop continuo cada 20s
  --interval N                 Loop cada N segundos
  --once                       Un solo ciclo
  --debug                      Mostrar OCR raw en consola
  --screenshot                 Screenshot + OCR + estado (sin actuar)
  --action-continue            Enviar "continuar" ahora
  --action-new-chat            Nuevo chat + AGENT_LOOP_PROMPT ahora
```
