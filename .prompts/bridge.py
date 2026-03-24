#!/usr/bin/env python3
"""
bridge.py — Utilidad para comunicación VS Code ↔ OpenClaw via .prompts/prompt_1.md

Uso desde terminal o scripts:

  # Enviar tarea a OpenClaw
  python3 .prompts/bridge.py send "Auditar SearchAgent en producción"
  
  # Enviar tarea y esperar respuesta (con delay)
  python3 .prompts/bridge.py send --wait "Ejecutar CI/CD y verificar producción"
  
  # Verificar si OpenClaw respondió (busca READ)
  python3 .prompts/bridge.py status
  
  # Leer resultados de la última ejecución
  python3 .prompts/bridge.py results
  
  # Esperar a que OpenClaw procese (polling con delay)
  python3 .prompts/bridge.py wait --timeout 180

Delay strategy:
  - OpenClaw cron cada 60s → latencia máxima ~60s para detectar cambio
  - Procesamiento de tarea: 10-300s dependiendo de complejidad
  - Default wait: 120s con polling cada 15s
  - VS Code debe esperar ≥30s antes de leer respuesta
"""

import argparse
import hashlib
import sys
import time
from datetime import datetime
from pathlib import Path

PROMPT_FILE = Path(__file__).parent / "prompt_1.md"
DEFAULT_WAIT_TIMEOUT = 180  # 3 min
POLL_INTERVAL = 15  # check every 15s


def get_hash(filepath: Path) -> str:
    try:
        return hashlib.md5(filepath.read_text("utf-8").encode()).hexdigest()
    except Exception:
        return ""


def send_task(task: str, wait: bool = False, timeout: int = DEFAULT_WAIT_TIMEOUT) -> None:
    """Escribe una tarea en prompt_1.md para que OpenClaw la ejecute."""
    content = PROMPT_FILE.read_text("utf-8")
    ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    # Reemplazar placeholder si existe
    if "_Sin tareas pendientes" in content:
        content = content.replace(
            "_Sin tareas pendientes. Esperando instrucciones de VS Code._",
            f"- [ ] {task}"
        )
    else:
        # Insertar después de "## TAREAS PENDIENTES" + comment
        marker = "<!-- VS Code (Copilot) escribe tareas aquí. OpenClaw las ejecuta. -->"
        if marker in content:
            content = content.replace(marker, f"{marker}\n\n- [ ] {task}")
        else:
            # Fallback: insertar antes de ## RESULTADOS
            content = content.replace(
                "## RESULTADOS",
                f"- [ ] {task}\n\n---\n\n## RESULTADOS"
            )

    # Quitar READ anterior para que OpenClaw detecte cambio
    if content.rstrip().endswith("READ"):
        content = content.rsplit("READ", 1)[0].rstrip() + "\n"

    # Agregar log entry
    log_row = f"| {ts} | VS Code | Tarea enviada: {task[:60]} |"
    lines = content.splitlines()
    for i in range(len(lines) - 1, -1, -1):
        if lines[i].strip().startswith("|") and "Timestamp" not in lines[i] and "---" not in lines[i]:
            lines.insert(i + 1, log_row)
            break
    content = "\n".join(lines) + "\n"

    PROMPT_FILE.write_text(content, "utf-8")
    print(f"[{ts}] ✅ Tarea enviada a OpenClaw: {task}")
    print(f"[{ts}] ⏳ OpenClaw la detectará en ~60s (cron interval)")

    if wait:
        wait_for_response(timeout)


def wait_for_response(timeout: int = DEFAULT_WAIT_TIMEOUT) -> bool:
    """Espera a que OpenClaw procese y agregue READ."""
    print(f"\n⏳ Esperando respuesta de OpenClaw (timeout: {timeout}s, poll: {POLL_INTERVAL}s)...")
    
    initial_hash = get_hash(PROMPT_FILE)
    elapsed = 0

    while elapsed < timeout:
        time.sleep(POLL_INTERVAL)
        elapsed += POLL_INTERVAL

        content = PROMPT_FILE.read_text("utf-8")
        current_hash = get_hash(PROMPT_FILE)

        # Verificar si hay READ nuevo y tareas completadas
        has_read = content.rstrip().endswith("READ")
        has_completed = "- [x]" in content

        if has_read and current_hash != initial_hash and has_completed:
            print(f"\n✅ OpenClaw respondió después de {elapsed}s")
            show_results()
            return True

        dots = "." * (elapsed // POLL_INTERVAL)
        print(f"  [{elapsed}s/{timeout}s] Esperando{dots}", end="\r")

    print(f"\n⚠️  Timeout después de {timeout}s. OpenClaw puede estar procesando aún.")
    return False


def show_status() -> None:
    """Muestra estado actual del bridge."""
    content = PROMPT_FILE.read_text("utf-8")
    
    pending = content.count("- [ ]")
    completed = content.count("- [x]")
    has_read = content.rstrip().endswith("READ")

    print(f"📋 Estado del bridge prompt_1.md:")
    print(f"   Tareas pendientes:  {pending}")
    print(f"   Tareas completadas: {completed}")
    print(f"   Último READ:        {'✅ Sí' if has_read else '⏳ No (OpenClaw procesando)'}")


def show_results() -> None:
    """Muestra resultados de tareas completadas."""
    content = PROMPT_FILE.read_text("utf-8")
    
    in_results = False
    for line in content.splitlines():
        if "## RESULTADOS" in line:
            in_results = True
            continue
        if in_results and line.startswith("## "):
            break
        if in_results and line.strip():
            print(line)

    # También mostrar tareas completadas
    print("\n📋 Tareas completadas:")
    for line in content.splitlines():
        if "- [x]" in line or (line.strip().startswith("- ✅") and "  -" in line):
            print(f"  {line.strip()}")


def main() -> None:
    parser = argparse.ArgumentParser(description="Bridge VS Code ↔ OpenClaw")
    sub = parser.add_subparsers(dest="command")

    # send
    p_send = sub.add_parser("send", help="Enviar tarea a OpenClaw")
    p_send.add_argument("task", help="Descripción de la tarea")
    p_send.add_argument("--wait", action="store_true", help="Esperar respuesta")
    p_send.add_argument("--timeout", type=int, default=DEFAULT_WAIT_TIMEOUT)

    # status
    sub.add_parser("status", help="Ver estado del bridge")

    # results
    sub.add_parser("results", help="Ver resultados de tareas")

    # wait
    p_wait = sub.add_parser("wait", help="Esperar respuesta de OpenClaw")
    p_wait.add_argument("--timeout", type=int, default=DEFAULT_WAIT_TIMEOUT)

    args = parser.parse_args()

    if args.command == "send":
        send_task(args.task, args.wait, args.timeout)
    elif args.command == "status":
        show_status()
    elif args.command == "results":
        show_results()
    elif args.command == "wait":
        wait_for_response(args.timeout)
    else:
        parser.print_help()


if __name__ == "__main__":
    main()
