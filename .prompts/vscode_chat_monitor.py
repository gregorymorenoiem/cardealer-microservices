#!/usr/bin/env python3
"""
vscode_chat_monitor.py — Monitor del chat de VS Code / GitHub Copilot
======================================================================
Lógica:
  1. Monitorea el chat de VS Code tomando screenshots periódicos
  2. Usa Claude Vision para analizar el estado del chat
  3. Si el chat se DETUVO POR ERROR → escribe "continuar" en el chat
  4. Si Copilot TERMINÓ SU TAREA (prompt_1.md termina con READ y chat está idle)
     → abre un NUEVO CHAT en VS Code y envía el AGENT_LOOP_PROMPT

Requisitos:
  pip install pyautogui anthropic opencv-python pytesseract pillow
  brew install tesseract
  ANTHROPIC_API_KEY real en .env

Uso:
  python3 .prompts/vscode_chat_monitor.py              # correr en foreground
  python3 .prompts/vscode_chat_monitor.py --interval 30 # check cada 30s
  python3 .prompts/vscode_chat_monitor.py --debug       # screenshots + análisis sin actuar
  python3 .prompts/vscode_chat_monitor.py --once        # un solo check y salir
"""

import argparse
import base64
import os
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

# ─── Imports opcionales (con fallback graceful) ───────────────────────────────
try:
    import pyautogui
    pyautogui.FAILSAFE = True
    pyautogui.PAUSE = 0.2
    PYAUTOGUI_OK = True
except ImportError:
    PYAUTOGUI_OK = False

try:
    import anthropic as _anthropic_mod
    ANTHROPIC_OK = True
except ImportError:
    ANTHROPIC_OK = False

try:
    import cv2
    import numpy as np
    CV2_OK = True
except ImportError:
    CV2_OK = False

try:
    import pytesseract
    TESSERACT_OK = True
except ImportError:
    TESSERACT_OK = False

# ─── Paths ────────────────────────────────────────────────────────────────────
ROOT = Path(__file__).parent.parent
PROMPT_1_FILE = ROOT / ".prompts" / "prompt_1.md"
AGENT_LOOP_FILE = ROOT / ".prompts" / "AGENT_LOOP_PROMPT.md"
SCREENSHOTS_DIR = ROOT / ".github" / "screenshots" / "monitor"
LOG_FILE = ROOT / ".github" / "copilot-audit.log"
SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)

# ─── Cargar .env ──────────────────────────────────────────────────────────────
def load_env():
    env_file = ROOT / ".env"
    if env_file.exists():
        for line in env_file.read_text().splitlines():
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, _, v = line.partition("=")
                os.environ.setdefault(k.strip(), v.strip())

load_env()
ANTHROPIC_API_KEY = os.environ.get("ANTHROPIC_API_KEY", "")

# ─── Estados posibles del chat ────────────────────────────────────────────────
class ChatState:
    WORKING       = "WORKING"        # Copilot generando respuesta (stop button visible)
    ERROR         = "ERROR"          # Chat detenido por error
    IDLE_FINISHED = "IDLE_FINISHED"  # Ciclo completado (READ en prompt_1.md)
    IDLE_WAITING  = "IDLE_WAITING"   # Chat idle, esperando (no hay READ, no hay error)
    VSCODE_CLOSED = "VSCODE_CLOSED"  # VS Code no está abierto
    UNKNOWN       = "UNKNOWN"        # No se pudo determinar


# ─── LOG ──────────────────────────────────────────────────────────────────────
def log(msg: str, level: str = "INFO"):
    ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    line = f"[{ts}] [MONITOR/{level}] {msg}"
    print(line, flush=True)
    try:
        with LOG_FILE.open("a") as f:
            f.write(line + "\n")
    except Exception:
        pass


# ─── AppleScript helpers ──────────────────────────────────────────────────────
def run_applescript(script: str, timeout: int = 10) -> str:
    try:
        result = subprocess.run(
            ["osascript", "-e", script],
            capture_output=True, text=True, timeout=timeout
        )
        return result.stdout.strip()
    except subprocess.TimeoutExpired:
        return ""
    except Exception as e:
        log(f"AppleScript error: {e}", "WARN")
        return ""


def is_vscode_running() -> bool:
    result = subprocess.run(
        ["pgrep", "-x", "Electron"],
        capture_output=True, text=True
    )
    # VS Code usa Electron. También intentamos por nombre del proceso
    r2 = run_applescript(
        'tell application "System Events" to return exists process "Code"'
    )
    return r2 == "true" or result.returncode == 0


def get_vscode_window_bounds() -> dict | None:
    """Obtiene posición y tamaño de la ventana de VS Code."""
    script = '''
    tell application "System Events"
        tell process "Code"
            set w to front window
            set {px, py} to position of w
            set {sw, sh} to size of w
            return (px as string) & "," & (py as string) & "," & (sw as string) & "," & (sh as string)
        end tell
    end tell
    '''
    result = run_applescript(script)
    if not result:
        return None
    try:
        x, y, w, h = [int(v.strip()) for v in result.split(",")]
        return {"x": x, "y": y, "w": w, "h": h}
    except Exception:
        return None


def focus_vscode():
    run_applescript('tell application "Code" to activate')
    time.sleep(0.4)


def vscode_keystroke(key: str, modifiers: list[str] = None):
    """Envía keystroke a VS Code vía AppleScript."""
    if modifiers:
        mod_str = "{" + ", ".join(f"{m} down" for m in modifiers) + "}"
        script = f'''
        tell application "System Events"
            tell process "Code"
                keystroke "{key}" using {mod_str}
            end tell
        end tell
        '''
    else:
        script = f'''
        tell application "System Events"
            tell process "Code"
                keystroke "{key}"
            end tell
        end tell
        '''
    run_applescript(script)


def vscode_key_code(code: int, modifiers: list[str] = None):
    """Envía key code a VS Code (para teclas especiales como Enter, Escape)."""
    if modifiers:
        mod_str = "{" + ", ".join(f"{m} down" for m in modifiers) + "}"
        script = f'''
        tell application "System Events"
            tell process "Code"
                key code {code} using {mod_str}
            end tell
        end tell
        '''
    else:
        script = f'''
        tell application "System Events"
            tell process "Code"
                key code {code}
            end tell
        end tell
        '''
    run_applescript(script)


# ─── Screenshot ───────────────────────────────────────────────────────────────
def _screencapture(path: Path, region: tuple = None) -> bool:
    """
    Usa el comando nativo 'screencapture' de macOS.
    Más confiable que PyAutoGUI — funciona sin permisos extra si se otorga
    Screen Recording a Terminal en System Settings → Privacy & Security.
    region = (x, y, w, h)
    """
    cmd = ["screencapture", "-x", "-t", "png"]  # -x = sin sonido
    if region:
        x, y, w, h = region
        cmd += ["-R", f"{x},{y},{w},{h}"]
    cmd.append(str(path))
    result = subprocess.run(cmd, capture_output=True, timeout=10)
    return result.returncode == 0 and path.exists() and path.stat().st_size > 0


def take_vscode_screenshot(name: str = None) -> Path | None:
    """Toma screenshot de la ventana de VS Code (usando screencapture nativo)."""
    focus_vscode()
    time.sleep(0.3)

    fname = name or f"vscode_{int(time.time())}.png"
    if not fname.endswith(".png"):
        fname += ".png"
    path = SCREENSHOTS_DIR / fname

    bounds = get_vscode_window_bounds()
    if bounds:
        ok = _screencapture(path, (bounds["x"], bounds["y"], bounds["w"], bounds["h"]))
    else:
        ok = _screencapture(path)

    if not ok:
        log("screencapture falló — verifica Screen Recording en System Settings → Privacy", "ERROR")
        return None
    return path


def take_chat_panel_screenshot(name: str = None) -> Path | None:
    """Toma screenshot solo del panel del chat (tercio derecho de VS Code)."""
    focus_vscode()
    time.sleep(0.3)

    fname = name or f"chat_{int(time.time())}.png"
    if not fname.endswith(".png"):
        fname += ".png"
    path = SCREENSHOTS_DIR / fname

    bounds = get_vscode_window_bounds()
    if bounds:
        # El panel de chat de Copilot es el tercio derecho
        chat_x = bounds["x"] + int(bounds["w"] * 0.60)
        chat_y = bounds["y"]
        chat_w = int(bounds["w"] * 0.40)
        chat_h = bounds["h"]
        ok = _screencapture(path, (chat_x, chat_y, chat_w, chat_h))
    else:
        ok = _screencapture(path)

    if not ok:
        log("screencapture falló — verifica Screen Recording en System Settings → Privacy", "ERROR")
        return None
    return path


# ─── Claude Vision — análisis del chat ────────────────────────────────────────
def analyze_screenshot_with_claude(screenshot_path: Path) -> str:
    """
    Envía screenshot a Claude Vision y obtiene el estado del chat.
    Retorna: WORKING | ERROR | IDLE_FINISHED | IDLE_WAITING | UNKNOWN
    """
    if not ANTHROPIC_OK or not ANTHROPIC_API_KEY or "test" in ANTHROPIC_API_KEY.lower():
        log("Claude Vision no disponible (API key inválida o no configurada). Usando OCR fallback.", "WARN")
        return analyze_screenshot_with_ocr(screenshot_path)

    try:
        with open(screenshot_path, "rb") as f:
            image_data = base64.standard_b64encode(f.read()).decode("utf-8")

        client = _anthropic_mod.Anthropic(api_key=ANTHROPIC_API_KEY)
        message = client.messages.create(
            model="claude-haiku-4-5",
            max_tokens=50,
            messages=[{
                "role": "user",
                "content": [
                    {
                        "type": "image",
                        "source": {
                            "type": "base64",
                            "media_type": "image/png",
                            "data": image_data,
                        },
                    },
                    {
                        "type": "text",
                        "text": (
                            "This is a screenshot of VS Code with GitHub Copilot chat open. "
                            "Analyze the chat panel and respond with EXACTLY ONE of these words:\n"
                            "- WORKING: Copilot is currently generating a response (stop/square button visible, spinner active)\n"
                            "- ERROR: Chat shows an error message, 'Sorry', rate limit, connection error, or was interrupted unexpectedly\n"
                            "- IDLE: Chat is idle/finished (last message is complete, no spinner, no error)\n"
                            "- UNKNOWN: Cannot determine the state or VS Code chat is not visible\n\n"
                            "Respond with ONLY the single word, nothing else."
                        ),
                    },
                ],
            }]
        )

        raw = message.content[0].text.strip().upper()
        log(f"Claude Vision → {raw}")

        if "WORKING" in raw:
            return ChatState.WORKING
        elif "ERROR" in raw:
            return ChatState.ERROR
        elif "IDLE" in raw:
            return ChatState.IDLE_WAITING
        else:
            return ChatState.UNKNOWN

    except Exception as e:
        log(f"Claude Vision error: {e}", "WARN")
        return analyze_screenshot_with_ocr(screenshot_path)


def analyze_screenshot_with_ocr(screenshot_path: Path) -> str:
    """Fallback: OCR para detectar estado del chat."""
    if not TESSERACT_OK or not CV2_OK:
        log("OCR no disponible", "WARN")
        return ChatState.UNKNOWN

    try:
        img = cv2.imread(str(screenshot_path))
        text = pytesseract.image_to_string(img).lower()

        error_keywords = [
            "sorry", "error", "something went wrong", "couldn't complete",
            "rate limit", "connection", "failed", "unable to", "lo siento",
            "ocurrió un error", "algo salió mal"
        ]
        working_keywords = ["generating", "thinking", "loading", "working"]

        for kw in error_keywords:
            if kw in text:
                log(f"OCR detectó error keyword: '{kw}'")
                return ChatState.ERROR

        for kw in working_keywords:
            if kw in text:
                return ChatState.WORKING

        return ChatState.IDLE_WAITING

    except Exception as e:
        log(f"OCR error: {e}", "WARN")
        return ChatState.UNKNOWN


# ─── Leer estado de prompt_1.md ───────────────────────────────────────────────
def get_prompt1_last_line() -> str:
    """Obtiene la última línea no vacía de prompt_1.md."""
    if not PROMPT_1_FILE.exists():
        return ""
    try:
        lines = PROMPT_1_FILE.read_text(encoding="utf-8").splitlines()
        for line in reversed(lines):
            if line.strip():
                return line.strip()
    except Exception:
        pass
    return ""


def prompt1_is_read() -> bool:
    return get_prompt1_last_line() == "READ"


def prompt1_is_stop() -> bool:
    return get_prompt1_last_line() == "STOP"


# ─── Acciones sobre VS Code ───────────────────────────────────────────────────
def click_vscode_chat_input():
    """Hace click en el área de input del chat de VS Code."""
    if not PYAUTOGUI_OK:
        return

    bounds = get_vscode_window_bounds()
    if not bounds:
        log("No se pudo obtener bounds de VS Code", "WARN")
        return

    # El input del chat de Copilot está en la parte inferior del panel derecho
    # Aproximadamente: 70% del ancho, 90% del alto
    chat_input_x = bounds["x"] + int(bounds["w"] * 0.78)
    chat_input_y = bounds["y"] + int(bounds["h"] * 0.93)

    pyautogui.click(chat_input_x, chat_input_y)
    time.sleep(0.3)
    log(f"Click en chat input: ({chat_input_x}, {chat_input_y})")


def type_in_chat(text: str):
    """Escribe en el chat de VS Code Copilot."""
    focus_vscode()
    time.sleep(0.3)

    # Método 1: Click en input del chat
    click_vscode_chat_input()
    time.sleep(0.3)

    # Pegar vía clipboard para evitar problemas con caracteres especiales
    proc = subprocess.run(["pbcopy"], input=text.encode("utf-8"), capture_output=True)
    if PYAUTOGUI_OK:
        pyautogui.hotkey("cmd", "a")    # seleccionar todo en el input
        time.sleep(0.1)
        pyautogui.hotkey("cmd", "v")    # pegar
        time.sleep(0.3)
        # Enviar con Shift+Enter NO, con Enter directo
        # En VS Code chat, Enter envía el mensaje
        pyautogui.press("enter")
        time.sleep(0.5)
    log(f"Mensaje enviado en chat: {text[:80]}{'...' if len(text) > 80 else ''}")


def open_copilot_new_chat():
    """
    Abre un NUEVO chat de GitHub Copilot en VS Code.
    Intenta varios métodos en orden de confiabilidad.
    """
    focus_vscode()
    time.sleep(0.5)

    log("Abriendo nuevo chat de Copilot...")

    # Método 1: Command Palette → "GitHub Copilot: New Chat"
    vscode_keystroke("p", ["command", "shift"])
    time.sleep(0.6)

    # Escribir el comando
    if PYAUTOGUI_OK:
        pyautogui.typewrite("GitHub Copilot: New Chat", interval=0.04)
        time.sleep(0.5)
        pyautogui.press("enter")
        time.sleep(1.0)
        log("Nuevo chat abierto vía Command Palette")
    else:
        # Fallback: solo teclado vía AppleScript
        run_applescript('''
        tell application "System Events"
            tell process "Code"
                keystroke "GitHub Copilot: New Chat"
            end tell
        end tell
        ''')
        time.sleep(0.5)
        vscode_key_code(36)  # Enter
        time.sleep(1.0)


def send_agent_loop_prompt():
    """Envía el AGENT_LOOP_PROMPT al chat activo de Copilot."""
    # Leer el prompt desde el archivo
    if AGENT_LOOP_FILE.exists():
        prompt_text = AGENT_LOOP_FILE.read_text(encoding="utf-8")
        # Extraer solo el contenido del prompt (sin el encabezado del archivo)
        lines = prompt_text.splitlines()
        # Saltar las primeras líneas de metadata (título + descripción)
        start_idx = 0
        for i, line in enumerate(lines):
            if line.strip().startswith("Ejecuta el siguiente loop"):
                start_idx = i
                break
        prompt_content = "\n".join(lines[start_idx:]).strip()
    else:
        log(f"AGENT_LOOP_PROMPT.md no encontrado en {AGENT_LOOP_FILE}", "ERROR")
        return

    focus_vscode()
    time.sleep(0.5)

    # Click en el input del chat
    click_vscode_chat_input()
    time.sleep(0.4)

    # Copiar al clipboard y pegar
    proc = subprocess.run(["pbcopy"], input=prompt_content.encode("utf-8"), capture_output=True)
    time.sleep(0.2)

    if PYAUTOGUI_OK:
        pyautogui.hotkey("cmd", "v")
        time.sleep(0.5)
        # En VS Code chat, Shift+Enter = nueva línea, Enter = enviar
        pyautogui.press("enter")
        time.sleep(0.5)

    log("AGENT_LOOP_PROMPT enviado al chat de Copilot")


def type_continuar():
    """Escribe 'continuar' en el chat para que Copilot retome."""
    focus_vscode()
    time.sleep(0.3)
    click_vscode_chat_input()
    time.sleep(0.3)

    if PYAUTOGUI_OK:
        proc = subprocess.run(["pbcopy"], input="continuar".encode("utf-8"), capture_output=True)
        time.sleep(0.1)
        pyautogui.hotkey("cmd", "v")
        time.sleep(0.2)
        pyautogui.press("enter")
        time.sleep(0.3)

    log("Enviado: 'continuar' al chat de Copilot")


# ─── Loop principal de monitoreo ──────────────────────────────────────────────
def run_monitor(interval: int = 45, debug: bool = False, once: bool = False):
    """
    Loop principal del monitor.

    Estados y acciones:
    - prompt_1.md termina en STOP  → Detener el monitor
    - prompt_1.md termina en READ + chat IDLE → Nuevo chat + AGENT_LOOP_PROMPT
    - Chat en ERROR → type "continuar"
    - Chat WORKING  → no hacer nada
    """
    log(f"Monitor iniciado. Interval: {interval}s | Debug: {debug}")
    log(f"Pantalla: {pyautogui.size() if PYAUTOGUI_OK else 'N/A'}")
    log(f"Claude Vision: {'✓' if (ANTHROPIC_OK and ANTHROPIC_API_KEY and 'test' not in ANTHROPIC_API_KEY.lower()) else '✗ (fallback OCR)'}")
    log(f"Monitoreando prompt_1.md: {PROMPT_1_FILE}")

    cycle = 0
    last_action = None
    consecutive_errors = 0
    MAX_CONSECUTIVE_ERRORS = 3

    while True:
        cycle += 1
        log(f"─── Ciclo {cycle} ─────────────────────────────")

        # 1. Verificar STOP en prompt_1.md
        if prompt1_is_stop():
            log("STOP detectado en prompt_1.md. Monitor detenido.")
            break

        # 2. Verificar si VS Code está corriendo
        if not is_vscode_running():
            log("VS Code no está corriendo. Esperando...", "WARN")
            if not once:
                time.sleep(interval)
                continue
            break

        # 3. Tomar screenshot del chat
        ss_path = take_chat_panel_screenshot(f"cycle_{cycle:04d}")
        if not ss_path:
            log("No se pudo tomar screenshot", "WARN")
            if not once:
                time.sleep(interval)
                continue
            break

        log(f"Screenshot: {ss_path.name}")

        # 4. Detectar estado del prompt_1.md
        last_line = get_prompt1_last_line()
        log(f"prompt_1.md última línea: '{last_line}'")

        # 5. Analizar estado del chat con Claude Vision / OCR
        chat_state = analyze_screenshot_with_claude(ss_path)
        log(f"Estado del chat: {chat_state}")

        if debug:
            log(f"[DEBUG] Solo análisis, sin actuar. Estado={chat_state}, last_line='{last_line}'")
            if once:
                break
            time.sleep(interval)
            continue

        # 6. Tomar decisión y actuar
        # ─────────────────────────────────────────────────────────────────────
        action = None

        if last_line == "READ" and chat_state in (ChatState.IDLE_WAITING, ChatState.IDLE_FINISHED, ChatState.UNKNOWN):
            # Copilot terminó un ciclo y está idle → nuevo chat + AGENT_LOOP_PROMPT
            action = "NEW_CHAT"

        elif chat_state == ChatState.ERROR:
            # Error en el chat → escribir "continuar"
            consecutive_errors += 1
            if consecutive_errors <= MAX_CONSECUTIVE_ERRORS:
                action = "CONTINUAR"
            else:
                log(f"Demasiados errores consecutivos ({consecutive_errors}). Esperando más tiempo.", "WARN")

        elif chat_state == ChatState.WORKING:
            # Copilot trabajando → no hacer nada
            consecutive_errors = 0
            log("Copilot trabajando, sin acción.")

        elif chat_state == ChatState.IDLE_WAITING:
            # Idle pero sin READ → puede estar esperando tarea o entre ciclos
            consecutive_errors = 0
            log("Chat idle, esperando tarea.")

        # ─────────────────────────────────────────────────────────────────────
        # Ejecutar acción
        if action == "NEW_CHAT":
            log("ACCIÓN: Abrir nuevo chat + enviar AGENT_LOOP_PROMPT")
            consecutive_errors = 0
            open_copilot_new_chat()
            time.sleep(1.5)
            send_agent_loop_prompt()
            last_action = "NEW_CHAT"
            log("✓ Nuevo chat iniciado con AGENT_LOOP_PROMPT")
            take_chat_panel_screenshot(f"after_new_chat_{cycle:04d}")

        elif action == "CONTINUAR":
            log(f"ACCIÓN: Escribir 'continuar' en el chat (intento {consecutive_errors})")
            type_continuar()
            last_action = "CONTINUAR"
            take_chat_panel_screenshot(f"after_continuar_{cycle:04d}")
            time.sleep(3)  # Dar tiempo a que Copilot procese

        if once:
            break

        log(f"Esperando {interval}s...")
        time.sleep(interval)

    log("Monitor finalizado.")


# ─── CLI ──────────────────────────────────────────────────────────────────────
def main():
    parser = argparse.ArgumentParser(
        description="vscode_chat_monitor.py — Monitor del chat de VS Code Copilot",
        formatter_class=argparse.RawTextHelpFormatter
    )
    parser.add_argument(
        "--interval", type=int, default=45,
        help="Segundos entre checks (default: 45)"
    )
    parser.add_argument(
        "--debug", action="store_true",
        help="Solo analiza y loguea, sin tomar acciones"
    )
    parser.add_argument(
        "--once", action="store_true",
        help="Ejecutar un solo check y salir"
    )
    parser.add_argument(
        "--screenshot", action="store_true",
        help="Tomar screenshot ahora y mostrar análisis"
    )
    parser.add_argument(
        "--test-new-chat", action="store_true",
        help="Probar: abrir nuevo chat + enviar AGENT_LOOP_PROMPT"
    )
    parser.add_argument(
        "--test-continuar", action="store_true",
        help="Probar: enviar 'continuar' al chat"
    )
    parser.add_argument(
        "--status", action="store_true",
        help="Mostrar estado actual sin actuar"
    )

    args = parser.parse_args()

    # Checks de dependencias
    if not PYAUTOGUI_OK:
        print("[ERROR] PyAutoGUI no instalado: pip install pyautogui")
        sys.exit(1)

    # Comandos individuales de prueba
    if args.screenshot:
        path = take_vscode_screenshot("test_screenshot")
        if path:
            state = analyze_screenshot_with_claude(path)
            print(f"Screenshot: {path}")
            print(f"Estado detectado: {state}")
        return

    if args.test_new_chat:
        log("TEST: Abriendo nuevo chat + enviando AGENT_LOOP_PROMPT")
        open_copilot_new_chat()
        time.sleep(1.5)
        send_agent_loop_prompt()
        return

    if args.test_continuar:
        log("TEST: Enviando 'continuar' al chat")
        type_continuar()
        return

    if args.status:
        print(f"VS Code corriendo: {is_vscode_running()}")
        print(f"prompt_1.md última línea: '{get_prompt1_last_line()}'")
        print(f"Es READ: {prompt1_is_read()}")
        print(f"Es STOP: {prompt1_is_stop()}")
        bounds = get_vscode_window_bounds()
        print(f"VS Code window bounds: {bounds}")
        ss = take_chat_panel_screenshot("status_check")
        if ss:
            state = analyze_screenshot_with_claude(ss)
            print(f"Estado del chat: {state}")
            print(f"Screenshot: {ss}")
        return

    # Loop principal
    run_monitor(
        interval=args.interval,
        debug=args.debug,
        once=args.once
    )


if __name__ == "__main__":
    main()
