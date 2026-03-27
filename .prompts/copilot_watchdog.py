#!/usr/bin/env python3
"""
copilot_watchdog.py — Guardian de GitHub Copilot en VS Code
via OpenClaw + Groq llama-3.1-8b-instant

Arquitectura:
  OpenClaw (copilot-watchdog) = cerebro → analiza estado → decide accion
  Python                      = manos   → recoge estado → ejecuta accion

Capas de recuperacion:
  CAPA 1 — CONTINUAR  : Escribe "continuar" en el chat activo de VS Code
  CAPA 2 — NUEVO_CHAT : Abre nuevo chat de Copilot y carga AGENT_LOOP_PROMPT.md
  CAPA 3 — REINICIAR  : Mata VS Code, lo reabre con el workspace y carga el prompt

Uso:
  python3 .prompts/copilot_watchdog.py              # loop continuo (cada 20s)
  python3 .prompts/copilot_watchdog.py --once       # un ciclo y salir
  python3 .prompts/copilot_watchdog.py --interval 30
  python3 .prompts/copilot_watchdog.py --dry-run    # ver decision sin actuar
  python3 .prompts/copilot_watchdog.py --status     # estado actual y salir
  python3 .prompts/copilot_watchdog.py --force CONTINUAR  # forzar accion
"""

import argparse
import json
import os
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

try:
    from groq import Groq
except ImportError:
    print("ERROR: groq SDK no instalado. Ejecuta: pip install groq")
    sys.exit(1)

# ─── Rutas ────────────────────────────────────────────────────────────────────
REPO          = Path(__file__).parent.parent
PROMPT_FILE   = REPO / ".prompts" / "prompt_1.md"
LOOP_PROMPT   = REPO / ".prompts" / "AGENT_LOOP_PROMPT.md"
WORKSPACE     = REPO / "cardealer.code-workspace"
LOG_FILE      = REPO / ".github" / "copilot-monitor.log"
STATE_FILE    = REPO / ".prompts" / ".watchdog_state.json"

# ─── Config ───────────────────────────────────────────────────────────────────
POLL               = 20    # segundos entre checks
STALL_MINUTES      = 5     # minutos sin cambio → sospecha de stall
STALL_MINUTES_HARD = 10    # minutos sin cambio después de CONTINUAR → NUEVO_CHAT
MAX_CONTINUE       = 3     # intentos de CONTINUAR antes de NUEVO_CHAT
MAX_NEW_CHAT       = 2     # intentos de NUEVO_CHAT antes de REINICIAR
VSCODE_BOOT_WAIT   = 20    # segundos para que VS Code arranque tras reinicio

# ─── Logging ──────────────────────────────────────────────────────────────────
def log(msg: str, level: str = "INFO") -> None:
    ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    line = f"[{ts}] [{level}] {msg}"
    print(line, flush=True)
    try:
        LOG_FILE.parent.mkdir(parents=True, exist_ok=True)
        with open(LOG_FILE, "a") as f:
            f.write(line + "\n")
    except Exception:
        pass


# ─── Estado persistente ───────────────────────────────────────────────────────
def load_state() -> dict:
    try:
        return json.loads(STATE_FILE.read_text())
    except Exception:
        return {
            "continue_attempts": 0,
            "new_chat_attempts": 0,
            "last_action": "OK",
            "last_action_ts": 0,
            "last_prompt_mtime": 0,
        }


def save_state(state: dict) -> None:
    STATE_FILE.write_text(json.dumps(state, indent=2))


# ─── Detección de estado ──────────────────────────────────────────────────────
def vscode_running() -> bool:
    """True si VS Code (Electron) está corriendo."""
    r = subprocess.run(
        ["pgrep", "-f", "Visual Studio Code"],
        capture_output=True, text=True
    )
    return r.returncode == 0


def minutes_since_modified(path: Path) -> float:
    """Minutos desde la última modificación del archivo. 999 si no existe."""
    if not path.exists():
        return 999.0
    return (time.time() - path.stat().st_mtime) / 60


def collect_state(st: dict) -> dict:
    """Recoge las métricas actuales para enviárselas al agente."""
    vsc_ok = vscode_running()
    prompt_mins  = minutes_since_modified(PROMPT_FILE)
    log_mins     = minutes_since_modified(LOG_FILE)

    # Detectar cambio en prompt_1.md respecto al estado previo
    current_mtime = PROMPT_FILE.stat().st_mtime if PROMPT_FILE.exists() else 0
    prompt_changed = current_mtime != st.get("last_prompt_mtime", 0)

    return {
        "vscode_running": vsc_ok,
        "prompt_mins_ago": round(prompt_mins, 1),
        "log_mins_ago": round(log_mins, 1),
        "prompt_changed_since_last_check": prompt_changed,
        "continue_attempts": st.get("continue_attempts", 0),
        "new_chat_attempts": st.get("new_chat_attempts", 0),
        "last_action": st.get("last_action", "OK"),
        "last_action_secs_ago": round(time.time() - st.get("last_action_ts", 0)),
    }


def build_message(metrics: dict) -> str:
    """Construye el mensaje para el agente copilot-watchdog."""
    return (
        f"Estado actual de GitHub Copilot en VS Code:\n"
        f"- VS Code corriendo: {'SI' if metrics['vscode_running'] else 'NO'}\n"
        f"- prompt_1.md ultimo cambio: hace {metrics['prompt_mins_ago']} min\n"
        f"- copilot-audit.log ultimo cambio: hace {metrics['log_mins_ago']} min\n"
        f"- prompt_1.md cambio desde el ultimo check: {'SI' if metrics['prompt_changed_since_last_check'] else 'NO'}\n"
        f"- Intentos CONTINUAR recientes: {metrics['continue_attempts']}\n"
        f"- Intentos NUEVO_CHAT recientes: {metrics['new_chat_attempts']}\n"
        f"- Ultima accion: {metrics['last_action']} (hace {metrics['last_action_secs_ago']}s)\n"
        f"\n"
        f"Responde SOLO con una palabra: OK | CONTINUAR | NUEVO_CHAT | REINICIAR"
    )


# Workspace del agente copilot-watchdog (OpenClaw define el SOUL/identidad)
WATCHDOG_SOUL = Path.home() / ".openclaw/agents/copilot-watchdog/workspace/SOUL.md"
OPENCLAW_CONFIG = Path.home() / ".openclaw/openclaw.json"


def _load_groq_key() -> str:
    """Carga GROQ_API_KEY desde el env, o desde openclaw.json si no está en el env."""
    key = os.getenv("GROQ_API_KEY", "")
    if key:
        return key
    try:
        cfg = json.loads(OPENCLAW_CONFIG.read_text())
        return cfg.get("env", {}).get("GROQ_API_KEY", "")
    except Exception:
        return ""


# ─── Consulta al agente (Groq SDK directo — sistema prompt del workspace OpenClaw) ────
def ask_watchdog(message: str, dry_run: bool = False) -> str:
    """
    Usa Groq SDK directamente con el SOUL.md del agente copilot-watchdog de OpenClaw
    como system prompt. Esto es más fiable que pasar por el gateway (que cuelga).
    El workspace OpenClaw sigue siendo la fuente de verdad del comportamiento del agente.
    Devuelve: OK | CONTINUAR | NUEVO_CHAT | REINICIAR
    """
    if dry_run:
        log(f"[DRY-RUN] Mensaje para copilot-watchdog:\n{message}")
        return "DRY_RUN"

    api_key = _load_groq_key()
    if not api_key:
        log("ERROR: GROQ_API_KEY no encontrada en env ni en ~/.openclaw/openclaw.json", "ERROR")
        return "OK"

    # System prompt = SOUL.md del agente OpenClaw
    system_prompt = WATCHDOG_SOUL.read_text() if WATCHDOG_SOUL.exists() else (
        "Eres el guardian de GitHub Copilot. "
        "Responde SOLO con: OK | CONTINUAR | NUEVO_CHAT | REINICIAR"
    )

    log("Consultando copilot-watchdog (Groq SDK directo, SOUL.md de OpenClaw)...")
    try:
        client = Groq(api_key=api_key)
        resp = client.chat.completions.create(
            model="llama-3.1-8b-instant",
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user",   "content": message},
            ],
            max_tokens=10,    # Solo necesita una palabra
            temperature=0.0,  # Determinístico
        )
        result = resp.choices[0].message.content.strip().upper()
        log(f"Respuesta raw del LLM: '{result}' (tokens: {resp.usage.total_tokens})")
        for decision in ("REINICIAR", "NUEVO_CHAT", "CONTINUAR", "OK"):
            if decision in result:
                return decision
        log(f"No se reconoció la decisión: '{result}' → usando OK", "WARN")
        return "OK"
    except Exception as exc:
        log(f"Error consultando Groq: {exc}", "WARN")
        return "OK"


# ─── Acciones de VS Code ──────────────────────────────────────────────────────
def osascript(script: str) -> str:
    """Ejecuta AppleScript."""
    r = subprocess.run(["osascript", "-e", script], capture_output=True, text=True)
    return r.stdout.strip()


def action_continuar(dry_run: bool = False) -> None:
    """
    CAPA 1: Escribe "continuar" en el chat activo de VS Code.
    Estrategia:
      1. Activar VS Code
      2. Usar clipboard para pegar "continuar" en el input del chat
      3. Enter
    """
    log("CAPA 1: Enviando 'continuar' al chat de VS Code...")
    if dry_run:
        log("[DRY-RUN] action_continuar()")
        return

    # 1. Activar VS Code
    osascript('tell application "Visual Studio Code" to activate')
    time.sleep(0.8)

    # 2. Copiar "continuar" al clipboard
    subprocess.run(["pbcopy"], input=b"continuar")
    time.sleep(0.2)

    # 3. Hacer click en el area de chat y pegar
    # Primero: Cmd+Shift+I para asegurar que el panel de chat está abierto
    osascript('''
        tell application "System Events"
            tell process "Code"
                keystroke "i" using {command down, shift down}
            end tell
        end tell
    ''')
    time.sleep(1.0)

    # Cmd+V para pegar y Enter para enviar
    osascript('''
        tell application "System Events"
            tell process "Code"
                keystroke "v" using {command down}
                delay 0.3
                key code 36
            end tell
        end tell
    ''')
    log("'continuar' enviado al chat de VS Code")


def action_nuevo_chat(dry_run: bool = False) -> None:
    """
    CAPA 2: Abre un nuevo chat de Copilot y carga AGENT_LOOP_PROMPT.md.
    """
    log("CAPA 2: Abriendo nuevo chat de Copilot...")
    if dry_run:
        log("[DRY-RUN] action_nuevo_chat()")
        return

    if not LOOP_PROMPT.exists():
        log(f"AGENT_LOOP_PROMPT.md no encontrado en {LOOP_PROMPT}", "WARN")
        prompt_text = "Continúa con las tareas pendientes en .prompts/prompt_1.md"
    else:
        prompt_text = LOOP_PROMPT.read_text(encoding="utf-8")[:4000]  # Límite seguro

    # Activar VS Code y abrir nuevo chat
    osascript('tell application "Visual Studio Code" to activate')
    time.sleep(0.8)

    # Cmd+Shift+I = abrir/enfocar Copilot Chat
    osascript('''
        tell application "System Events"
            tell process "Code"
                keystroke "i" using {command down, shift down}
            end tell
        end tell
    ''')
    time.sleep(1.5)

    # Abrir NUEVO chat: el botón "New Chat" o Cmd+N dentro del panel
    # Intentar con el comando de VS Code: workbench.action.chat.newChat
    subprocess.run([
        "code", "--command", "workbench.action.chat.newChat"
    ], capture_output=True, timeout=5)
    time.sleep(1.0)

    # Pegar el prompt completo via clipboard
    subprocess.run(["pbcopy"], input=prompt_text.encode("utf-8"))
    time.sleep(0.2)

    osascript('''
        tell application "System Events"
            tell process "Code"
                keystroke "v" using {command down}
                delay 0.5
                key code 36
            end tell
        end tell
    ''')
    log("Nuevo chat abierto con AGENT_LOOP_PROMPT")


def action_reiniciar(dry_run: bool = False) -> None:
    """
    CAPA 3: Reinicia VS Code con el workspace.
    """
    log("CAPA 3: Reiniciando VS Code...")
    if dry_run:
        log("[DRY-RUN] action_reiniciar()")
        return

    # Cerrar VS Code limpiamente
    osascript('tell application "Visual Studio Code" to quit')
    time.sleep(3)

    # Forzar cierre si sigue corriendo
    subprocess.run(["pkill", "-f", "Visual Studio Code"], capture_output=True)
    time.sleep(2)

    # Reabrir con el workspace
    workspace_path = str(WORKSPACE) if WORKSPACE.exists() else str(REPO)
    subprocess.Popen(["code", workspace_path])
    log(f"VS Code reabriendo con: {workspace_path}")

    # Esperar que cargue
    log(f"Esperando {VSCODE_BOOT_WAIT}s para que VS Code arranque...")
    time.sleep(VSCODE_BOOT_WAIT)

    # Abrir Copilot chat y cargar el prompt
    action_nuevo_chat(dry_run=False)


# ─── Ciclo principal ──────────────────────────────────────────────────────────
def run_loop(poll: int, once: bool, dry_run: bool, force: str = None) -> None:
    log(f"=== copilot_watchdog v1 | OpenClaw+Groq | poll={poll}s ===")
    log(f"Agente: copilot-watchdog (groq/llama-3.1-8b-instant, tools=minimal)")

    state = load_state()

    while True:
        state = load_state()
        metrics = collect_state(state)

        log(
            f"VS Code={'OK' if metrics['vscode_running'] else 'DOWN'} | "
            f"prompt_1.md={metrics['prompt_mins_ago']}min | "
            f"log={metrics['log_mins_ago']}min | "
            f"continuar_intentos={metrics['continue_attempts']}"
        )

        # Acción forzada (para testing)
        if force:
            decision = force.upper()
            log(f"Acción forzada: {decision}")
        else:
            message  = build_message(metrics)
            decision = ask_watchdog(message, dry_run=dry_run)

        # Ejecutar acción según decisión
        now = time.time()

        if decision == "OK" or decision == "DRY_RUN":
            log("Estado: OK — Copilot trabajando normalmente.")
            # Reset parcial si hubo cambios
            if metrics["prompt_changed_since_last_check"]:
                state["continue_attempts"] = 0
                state["new_chat_attempts"] = 0

        elif decision == "CONTINUAR":
            if state["continue_attempts"] >= MAX_CONTINUE:
                log(f"Maximo de intentos CONTINUAR ({MAX_CONTINUE}) alcanzado → escalando a NUEVO_CHAT")
                decision = "NUEVO_CHAT"
            else:
                action_continuar(dry_run=dry_run)
                state["continue_attempts"] = state.get("continue_attempts", 0) + 1
                state["last_action"] = "CONTINUAR"
                state["last_action_ts"] = now

        if decision == "NUEVO_CHAT":
            if state["new_chat_attempts"] >= MAX_NEW_CHAT:
                log(f"Maximo de intentos NUEVO_CHAT ({MAX_NEW_CHAT}) alcanzado → escalando a REINICIAR")
                decision = "REINICIAR"
            else:
                action_nuevo_chat(dry_run=dry_run)
                state["new_chat_attempts"] = state.get("new_chat_attempts", 0) + 1
                state["continue_attempts"] = 0
                state["last_action"] = "NUEVO_CHAT"
                state["last_action_ts"] = now

        if decision == "REINICIAR":
            action_reiniciar(dry_run=dry_run)
            state["continue_attempts"] = 0
            state["new_chat_attempts"] = 0
            state["last_action"] = "REINICIAR"
            state["last_action_ts"] = now

        # Actualizar mtime del prompt para detectar cambios en el próximo ciclo
        state["last_prompt_mtime"] = PROMPT_FILE.stat().st_mtime if PROMPT_FILE.exists() else 0
        save_state(state)

        if once or force:
            break

        log(f"Esperando {poll}s...")
        time.sleep(poll)

    log("=== copilot_watchdog finalizado ===")


# ─── Comando --status ─────────────────────────────────────────────────────────
def show_status() -> None:
    state = load_state()
    metrics = collect_state(state)
    vsc = "✅ corriendo" if metrics["vscode_running"] else "❌ detenido"
    prompt_ago = f"{metrics['prompt_mins_ago']} min"
    log_ago = f"{metrics['log_mins_ago']} min"

    print(f"\n{'─'*54}")
    print(f"  VS Code            : {vsc}")
    print(f"  prompt_1.md        : ultimo cambio hace {prompt_ago}")
    print(f"  copilot-audit.log  : ultimo cambio hace {log_ago}")
    print(f"  Intentos CONTINUAR : {state.get('continue_attempts', 0)}/{MAX_CONTINUE}")
    print(f"  Intentos NUEVO_CHAT: {state.get('new_chat_attempts', 0)}/{MAX_NEW_CHAT}")
    print(f"  Ultima accion      : {state.get('last_action', 'OK')}")

    stall = metrics["prompt_mins_ago"] >= STALL_MINUTES and metrics["vscode_running"]
    banner = "⚠️  POSIBLE STALL" if stall else "✅ NORMAL"
    print(f"  Diagnostico        : {banner}")
    print(f"{'─'*54}\n")


# ─── Entry point ─────────────────────────────────────────────────────────────
def main() -> None:
    parser = argparse.ArgumentParser(
        description="Guardian de GitHub Copilot — OpenClaw + Groq llama-3.1-8b-instant"
    )
    parser.add_argument("--interval",  type=int, default=POLL,  help=f"Segundos entre checks (default: {POLL})")
    parser.add_argument("--once",      action="store_true",      help="Un ciclo y salir")
    parser.add_argument("--dry-run",   action="store_true",      help="Ver decisión sin ejecutar acciones")
    parser.add_argument("--status",    action="store_true",      help="Estado actual y salir")
    parser.add_argument("--force",     metavar="ACTION",         help="Forzar acción: OK|CONTINUAR|NUEVO_CHAT|REINICIAR")
    args = parser.parse_args()

    if args.status:
        show_status()
        return

    run_loop(
        poll=args.interval,
        once=args.once,
        dry_run=args.dry_run,
        force=args.force,
    )


if __name__ == "__main__":
    main()
