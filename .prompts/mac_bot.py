#!/usr/bin/env python3
"""
mac_bot.py — Control total de macOS (mouse, teclado, apps, Chrome, screenshots)
Uso: python3 .prompts/mac_bot.py [COMANDO] [OPCIONES]
"""

import argparse
import subprocess
import time
import sys
import os
from pathlib import Path

# ─────────────────────────────────────────────────────────────────────────────
# SETUP PYAUTOGUI
# ─────────────────────────────────────────────────────────────────────────────
try:
    import pyautogui
    pyautogui.FAILSAFE = True       # Mover mouse a esquina superior-izquierda = STOP
    pyautogui.PAUSE = 0.15          # Pausa entre acciones (más humano)
    PYAUTOGUI_OK = True
except ImportError:
    PYAUTOGUI_OK = False
    print("[WARN] PyAutoGUI no instalado. Corre: pip install pyautogui pillow")

SCREENSHOTS_DIR = Path(__file__).parent.parent / ".github" / "screenshots"
SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)


# ─────────────────────────────────────────────────────────────────────────────
# APPLESCRIPT — Control nativo macOS
# ─────────────────────────────────────────────────────────────────────────────
def run_applescript(script: str) -> str:
    """Ejecuta AppleScript y devuelve el resultado."""
    result = subprocess.run(
        ["osascript", "-e", script],
        capture_output=True, text=True
    )
    if result.returncode != 0:
        print(f"[AppleScript ERROR] {result.stderr.strip()}")
        return ""
    return result.stdout.strip()


def applescript_file(path: str) -> str:
    """Ejecuta un archivo .applescript."""
    result = subprocess.run(
        ["osascript", path],
        capture_output=True, text=True
    )
    return result.stdout.strip()


# ─────────────────────────────────────────────────────────────────────────────
# CONTROL DE APLICACIONES (AppleScript)
# ─────────────────────────────────────────────────────────────────────────────
def app_open(app_name: str):
    """Abre una aplicación."""
    run_applescript(f'tell application "{app_name}" to activate')
    print(f"[APP] Abriendo: {app_name}")


def app_quit(app_name: str):
    """Cierra una aplicación."""
    run_applescript(f'tell application "{app_name}" to quit')
    print(f"[APP] Cerrando: {app_name}")


def app_frontmost() -> str:
    """Devuelve la app en primer plano."""
    return run_applescript('tell application "System Events" to return name of first application process whose frontmost is true')


def list_running_apps() -> list[str]:
    """Lista todas las apps corriendo."""
    raw = run_applescript(
        'tell application "System Events" to return name of every application process whose background only is false'
    )
    return [a.strip() for a in raw.split(",") if a.strip()]


# ─────────────────────────────────────────────────────────────────────────────
# CONTROL DE CHROME (AppleScript)
# ─────────────────────────────────────────────────────────────────────────────
def chrome_open_url(url: str):
    """Abre URL en Chrome (nueva pestaña)."""
    script = f'''
    tell application "Google Chrome"
        activate
        if (count of windows) = 0 then
            make new window
        end if
        set theTab to make new tab at end of tabs of front window with properties {{URL:"{url}"}}
        activate
    end tell
    '''
    run_applescript(script)
    print(f"[Chrome] → {url}")


def chrome_execute_js(js_code: str) -> str:
    """Ejecuta JavaScript en la pestaña activa de Chrome."""
    escaped = js_code.replace('"', '\\"').replace('\n', ' ')
    script = f'''
    tell application "Google Chrome"
        execute front window's active tab javascript "{escaped}"
    end tell
    '''
    result = run_applescript(script)
    print(f"[Chrome JS] resultado: {result}")
    return result


def chrome_get_url() -> str:
    """Obtiene la URL de la pestaña activa."""
    return run_applescript('tell application "Google Chrome" to return URL of active tab of front window')


def chrome_get_title() -> str:
    """Obtiene el título de la pestaña activa."""
    return run_applescript('tell application "Google Chrome" to return title of active tab of front window')


def chrome_reload():
    """Recarga la pestaña activa."""
    run_applescript('tell application "Google Chrome" to reload active tab of front window')
    print("[Chrome] Recargando pestaña...")


def chrome_close_tab():
    """Cierra la pestaña activa."""
    run_applescript('tell application "Google Chrome" to close active tab of front window')
    print("[Chrome] Pestaña cerrada")


def chrome_new_window(url: str = ""):
    """Abre una nueva ventana de Chrome."""
    script = f'tell application "Google Chrome" to make new window'
    if url:
        script = f'''
        tell application "Google Chrome"
            make new window
            set URL of active tab of front window to "{url}"
        end tell
        '''
    run_applescript(script)


# ─────────────────────────────────────────────────────────────────────────────
# PYAUTOGUI — Mouse y Teclado
# ─────────────────────────────────────────────────────────────────────────────
def _check_pyautogui():
    if not PYAUTOGUI_OK:
        print("[ERROR] PyAutoGUI no está instalado.")
        sys.exit(1)


def mouse_move(x: int, y: int, duration: float = 0.3):
    """Mueve el mouse a coordenadas X,Y."""
    _check_pyautogui()
    pyautogui.moveTo(x, y, duration=duration)
    print(f"[Mouse] Movido a ({x}, {y})")


def mouse_click(x: int = None, y: int = None, button: str = "left", clicks: int = 1):
    """Click en posición (o en el lugar actual si no se dan coords)."""
    _check_pyautogui()
    if x is not None and y is not None:
        pyautogui.click(x, y, button=button, clicks=clicks)
        print(f"[Mouse] Click {button} en ({x}, {y}) x{clicks}")
    else:
        pyautogui.click(button=button, clicks=clicks)
        print(f"[Mouse] Click {button} en posición actual x{clicks}")


def mouse_drag(x1: int, y1: int, x2: int, y2: int, duration: float = 0.5):
    """Arrastra desde (x1,y1) hasta (x2,y2)."""
    _check_pyautogui()
    pyautogui.moveTo(x1, y1)
    pyautogui.dragTo(x2, y2, duration=duration, button="left")
    print(f"[Mouse] Arrastrado de ({x1},{y1}) a ({x2},{y2})")


def mouse_scroll(x: int, y: int, amount: int):
    """Scroll en posición. Positivo = arriba, Negativo = abajo."""
    _check_pyautogui()
    pyautogui.scroll(amount, x=x, y=y)
    print(f"[Mouse] Scroll {amount} en ({x}, {y})")


def keyboard_type(text: str, interval: float = 0.05):
    """Escribe texto con el teclado."""
    _check_pyautogui()
    pyautogui.typewrite(text, interval=interval)
    print(f"[Teclado] Escribió: {text[:50]}{'...' if len(text) > 50 else ''}")


def keyboard_hotkey(*keys: str):
    """Ejecuta atajo de teclado. Ej: 'cmd', 'c' para copiar."""
    _check_pyautogui()
    pyautogui.hotkey(*keys)
    print(f"[Teclado] Hotkey: {'+'.join(keys)}")


def keyboard_press(key: str, presses: int = 1):
    """Presiona una tecla N veces."""
    _check_pyautogui()
    pyautogui.press(key, presses=presses)
    print(f"[Teclado] Tecla '{key}' x{presses}")


def keyboard_write_clipboard(text: str):
    """Pega texto usando el portapapeles (más rápido que typewrite para texto largo)."""
    _check_pyautogui()
    import subprocess
    proc = subprocess.run(
        ["pbcopy"], input=text.encode("utf-8"),
        capture_output=True
    )
    time.sleep(0.1)
    pyautogui.hotkey("cmd", "v")
    print(f"[Teclado] Pegado desde clipboard ({len(text)} chars)")


# ─────────────────────────────────────────────────────────────────────────────
# SCREENSHOTS
# ─────────────────────────────────────────────────────────────────────────────
def take_screenshot(name: str = None) -> Path:
    """Captura pantalla completa. Devuelve la ruta del archivo."""
    _check_pyautogui()
    fname = name or f"screenshot_{int(time.time())}.png"
    if not fname.endswith(".png"):
        fname += ".png"
    path = SCREENSHOTS_DIR / fname
    img = pyautogui.screenshot()
    img.save(str(path))
    print(f"[Screenshot] Guardado: {path}")
    return path


def screenshot_region(x: int, y: int, w: int, h: int, name: str = None) -> Path:
    """Captura una región específica de la pantalla."""
    _check_pyautogui()
    fname = name or f"region_{int(time.time())}.png"
    if not fname.endswith(".png"):
        fname += ".png"
    path = SCREENSHOTS_DIR / fname
    img = pyautogui.screenshot(region=(x, y, w, h))
    img.save(str(path))
    print(f"[Screenshot] Región guardada: {path}")
    return path


def get_screen_size() -> tuple[int, int]:
    """Devuelve el tamaño de la pantalla (ancho, alto)."""
    _check_pyautogui()
    size = pyautogui.size()
    print(f"[Pantalla] Tamaño: {size.width}x{size.height}")
    return size.width, size.height


def get_mouse_position() -> tuple[int, int]:
    """Devuelve la posición actual del mouse."""
    _check_pyautogui()
    pos = pyautogui.position()
    print(f"[Mouse] Posición actual: ({pos.x}, {pos.y})")
    return pos.x, pos.y


# ─────────────────────────────────────────────────────────────────────────────
# SISTEMA — Notificaciones, Portapapeles, Dialogs
# ─────────────────────────────────────────────────────────────────────────────
def notify(title: str, message: str, sound: bool = True):
    """Muestra notificación nativa de macOS."""
    sound_script = 'sound name "Glass"' if sound else ""
    script = f'display notification "{message}" with title "{title}" {sound_script}'
    run_applescript(script)
    print(f"[Notify] {title}: {message}")


def dialog_ask(prompt: str, default: str = "") -> str:
    """Muestra diálogo de input y devuelve la respuesta."""
    script = f'text returned of (display dialog "{prompt}" default answer "{default}")'
    return run_applescript(script)


def dialog_confirm(prompt: str) -> bool:
    """Muestra diálogo de confirmación. Devuelve True si el usuario presionó OK."""
    script = f'''
    try
        display dialog "{prompt}" buttons {{"Cancelar", "OK"}} default button "OK"
        return "true"
    on error
        return "false"
    end try
    '''
    return run_applescript(script) == "true"


def clipboard_get() -> str:
    """Obtiene el contenido del portapapeles."""
    result = subprocess.run(["pbpaste"], capture_output=True, text=True)
    return result.stdout


def clipboard_set(text: str):
    """Establece el contenido del portapapeles."""
    subprocess.run(["pbcopy"], input=text.encode("utf-8"))
    print(f"[Clipboard] Seteado: {text[:80]}{'...' if len(text) > 80 else ''}")


# ─────────────────────────────────────────────────────────────────────────────
# SECUENCIAS COMPUESTAS (workflows)
# ─────────────────────────────────────────────────────────────────────────────
def workflow_open_okla_local():
    """Abre https://okla.local en Chrome."""
    app_open("Google Chrome")
    time.sleep(0.5)
    chrome_open_url("https://okla.local")
    time.sleep(2)
    title = chrome_get_title()
    print(f"[Workflow] OKLA Local abierto — Título: {title}")
    take_screenshot("okla_local_open")


def workflow_fill_form(fields: dict[str, str], submit_selector: str = None):
    """
    Rellena un formulario Chrome con JS.
    fields = {"#email": "test@okla.com", "#password": "pass123"}
    """
    for selector, value in fields.items():
        js = f'''
        (function() {{
            var el = document.querySelector('{selector}');
            if (!el) return 'NOT_FOUND:{selector}';
            el.focus();
            var nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype, 'value').set;
            nativeInputValueSetter.call(el, '{value}');
            el.dispatchEvent(new Event('input', {{bubbles: true}}));
            el.dispatchEvent(new Event('change', {{bubbles: true}}));
            return 'OK:{selector}';
        }})()
        '''
        result = chrome_execute_js(js)
        print(f"[Form] {selector} = '{value}' → {result}")

    if submit_selector:
        js_submit = f"document.querySelector('{submit_selector}').click()"
        chrome_execute_js(js_submit)
        print(f"[Form] Submit: {submit_selector}")


def workflow_screenshot_all_breakpoints(url: str):
    """Toma screenshots en distintos tamaños de ventana (responsive testing)."""
    breakpoints = [
        ("mobile", 390, 844),
        ("tablet", 768, 1024),
        ("desktop", 1440, 900),
    ]
    chrome_open_url(url)
    time.sleep(2)

    for name, w, h in breakpoints:
        js = f"window.resizeTo({w}, {h})"
        chrome_execute_js(js)
        time.sleep(0.5)
        take_screenshot(f"responsive_{name}_{w}x{h}")

    print("[Workflow] Screenshots responsive completados")


def workflow_monitor_mouse():
    """Modo interactivo: muestra coordenadas del mouse en tiempo real hasta Ctrl+C."""
    _check_pyautogui()
    print("[Monitor] Modo posición de mouse activo. Presiona Ctrl+C para salir.")
    print("  Mueve el mouse a la esquina superior-izquierda para FAILSAFE.")
    try:
        while True:
            pos = pyautogui.position()
            print(f"\r  Mouse: x={pos.x:4d}  y={pos.y:4d}   ", end="", flush=True)
            time.sleep(0.1)
    except KeyboardInterrupt:
        print("\n[Monitor] Detenido.")


# ─────────────────────────────────────────────────────────────────────────────
# CLI
# ─────────────────────────────────────────────────────────────────────────────
def main():
    parser = argparse.ArgumentParser(
        description="mac_bot.py — Control total de macOS",
        formatter_class=argparse.RawTextHelpFormatter
    )
    subs = parser.add_subparsers(dest="cmd", metavar="COMANDO")

    # ── Mouse ────────────────────────────────────────────────────────────────
    p = subs.add_parser("move", help="Mover mouse a coordenadas")
    p.add_argument("x", type=int); p.add_argument("y", type=int)
    p.add_argument("--duration", type=float, default=0.3)

    p = subs.add_parser("click", help="Click del mouse")
    p.add_argument("x", type=int, nargs="?"); p.add_argument("y", type=int, nargs="?")
    p.add_argument("--button", default="left", choices=["left", "right", "middle"])
    p.add_argument("--clicks", type=int, default=1)

    p = subs.add_parser("drag", help="Arrastrar mouse")
    p.add_argument("x1", type=int); p.add_argument("y1", type=int)
    p.add_argument("x2", type=int); p.add_argument("y2", type=int)

    p = subs.add_parser("scroll", help="Scroll del mouse")
    p.add_argument("x", type=int); p.add_argument("y", type=int)
    p.add_argument("amount", type=int, help="Positivo=arriba, Negativo=abajo")

    subs.add_parser("pos", help="Posición actual del mouse")
    subs.add_parser("size", help="Tamaño de la pantalla")
    subs.add_parser("monitor", help="Monitor de posición del mouse en tiempo real")

    # ── Teclado ──────────────────────────────────────────────────────────────
    p = subs.add_parser("type", help="Escribir texto")
    p.add_argument("text"); p.add_argument("--interval", type=float, default=0.05)

    p = subs.add_parser("paste", help="Pegar texto (vía portapapeles, más rápido)")
    p.add_argument("text")

    p = subs.add_parser("hotkey", help="Atajo de teclado. Ej: cmd c")
    p.add_argument("keys", nargs="+")

    p = subs.add_parser("press", help="Presionar tecla")
    p.add_argument("key"); p.add_argument("--times", type=int, default=1)

    # ── Screenshot ───────────────────────────────────────────────────────────
    p = subs.add_parser("screenshot", help="Capturar pantalla completa")
    p.add_argument("--name", default=None)

    p = subs.add_parser("screenshot-region", help="Capturar región de pantalla")
    p.add_argument("x", type=int); p.add_argument("y", type=int)
    p.add_argument("w", type=int); p.add_argument("h", type=int)
    p.add_argument("--name", default=None)

    # ── Apps ─────────────────────────────────────────────────────────────────
    p = subs.add_parser("open", help="Abrir aplicación")
    p.add_argument("app")

    p = subs.add_parser("quit", help="Cerrar aplicación")
    p.add_argument("app")

    subs.add_parser("frontmost", help="App en primer plano")
    subs.add_parser("apps", help="Listar apps corriendo")

    # ── Chrome ───────────────────────────────────────────────────────────────
    p = subs.add_parser("chrome-url", help="Abrir URL en Chrome")
    p.add_argument("url")

    subs.add_parser("chrome-reload", help="Recargar pestaña Chrome")
    subs.add_parser("chrome-close-tab", help="Cerrar pestaña Chrome")
    subs.add_parser("chrome-current-url", help="URL de la pestaña activa")
    subs.add_parser("chrome-title", help="Título de la pestaña activa")

    p = subs.add_parser("chrome-js", help="Ejecutar JavaScript en Chrome")
    p.add_argument("code")

    p = subs.add_parser("chrome-fill", help="Rellenar campo en Chrome (selector value)")
    p.add_argument("selector"); p.add_argument("value")

    # ── Sistema ──────────────────────────────────────────────────────────────
    p = subs.add_parser("notify", help="Notificación macOS")
    p.add_argument("title"); p.add_argument("message")

    p = subs.add_parser("ask", help="Diálogo de input")
    p.add_argument("prompt"); p.add_argument("--default", default="")

    p = subs.add_parser("confirm", help="Diálogo de confirmación")
    p.add_argument("prompt")

    subs.add_parser("clipboard-get", help="Obtener portapapeles")
    p = subs.add_parser("clipboard-set", help="Setear portapapeles")
    p.add_argument("text")

    p = subs.add_parser("applescript", help="Ejecutar AppleScript directamente")
    p.add_argument("script")

    # ── Workflows ────────────────────────────────────────────────────────────
    subs.add_parser("okla-open", help="Abrir https://okla.local en Chrome")

    p = subs.add_parser("responsive", help="Screenshots responsive de una URL")
    p.add_argument("url")

    # ── Ayuda extra ──────────────────────────────────────────────────────────
    if len(sys.argv) == 1:
        parser.print_help()
        print("""
EJEMPLOS RÁPIDOS:
  python3 .prompts/mac_bot.py pos                          # Posición del mouse
  python3 .prompts/mac_bot.py monitor                      # Rastrear mouse en vivo
  python3 .prompts/mac_bot.py screenshot                   # Captura de pantalla
  python3 .prompts/mac_bot.py move 500 300                 # Mover mouse
  python3 .prompts/mac_bot.py click 500 300                # Click en coords
  python3 .prompts/mac_bot.py click 500 300 --button right # Click derecho
  python3 .prompts/mac_bot.py click 500 300 --clicks 2     # Doble click
  python3 .prompts/mac_bot.py hotkey cmd c                 # Copiar
  python3 .prompts/mac_bot.py hotkey cmd v                 # Pegar
  python3 .prompts/mac_bot.py hotkey cmd shift 3           # Screenshot nativo
  python3 .prompts/mac_bot.py type "Hola mundo"            # Escribir texto
  python3 .prompts/mac_bot.py paste "Texto largo..."       # Pegar rápido
  python3 .prompts/mac_bot.py open "Finder"                # Abrir Finder
  python3 .prompts/mac_bot.py apps                         # Apps corriendo
  python3 .prompts/mac_bot.py chrome-url https://okla.local
  python3 .prompts/mac_bot.py chrome-js "document.title"
  python3 .prompts/mac_bot.py chrome-fill "#email" "test@okla.com"
  python3 .prompts/mac_bot.py notify "OKLA" "Deploy completado"
  python3 .prompts/mac_bot.py applescript 'say "Hola"'
  python3 .prompts/mac_bot.py okla-open                   # Workflow: abrir OKLA local
""")
        return

    args = parser.parse_args()

    # ── Dispatch ─────────────────────────────────────────────────────────────
    match args.cmd:
        case "move":            mouse_move(args.x, args.y, args.duration)
        case "click":           mouse_click(args.x, args.y, args.button, args.clicks)
        case "drag":            mouse_drag(args.x1, args.y1, args.x2, args.y2)
        case "scroll":          mouse_scroll(args.x, args.y, args.amount)
        case "pos":             get_mouse_position()
        case "size":            get_screen_size()
        case "monitor":         workflow_monitor_mouse()
        case "type":            keyboard_type(args.text, args.interval)
        case "paste":           keyboard_write_clipboard(args.text)
        case "hotkey":          keyboard_hotkey(*args.keys)
        case "press":           keyboard_press(args.key, args.times)
        case "screenshot":      take_screenshot(args.name)
        case "screenshot-region": screenshot_region(args.x, args.y, args.w, args.h, args.name)
        case "open":            app_open(args.app)
        case "quit":            app_quit(args.app)
        case "frontmost":       print(app_frontmost())
        case "apps":            [print(f"  · {a}") for a in list_running_apps()]
        case "chrome-url":      chrome_open_url(args.url)
        case "chrome-reload":   chrome_reload()
        case "chrome-close-tab": chrome_close_tab()
        case "chrome-current-url": print(chrome_get_url())
        case "chrome-title":    print(chrome_get_title())
        case "chrome-js":       chrome_execute_js(args.code)
        case "chrome-fill":     workflow_fill_form({args.selector: args.value})
        case "notify":          notify(args.title, args.message)
        case "ask":             print(dialog_ask(args.prompt, args.default))
        case "confirm":         print("OK" if dialog_confirm(args.prompt) else "CANCELADO")
        case "clipboard-get":   print(clipboard_get())
        case "clipboard-set":   clipboard_set(args.text)
        case "applescript":     print(run_applescript(args.script))
        case "okla-open":       workflow_open_okla_local()
        case "responsive":      workflow_screenshot_all_breakpoints(args.url)
        case _:                 parser.print_help()


if __name__ == "__main__":
    main()
