#!/usr/bin/env python3
"""
okla_loop.py v3 — Loop inteligente OKLA con Groq llama-3.1-8b-instant + tool calling nativo.

Arquitectura:
  - Groq (llama-3.1-8b-instant) = cerebro: lee prompt_1.md, decide qué hacer, llama tools
  - Python = loop liviano: monitorea el archivo y dispara el ciclo agente
  - Sin openclaw en la ejecución → control total de tokens, sin overhead de 10k+ chars

Herramientas disponibles para el LLM:
  correr_comando(cmd, timeout?)  → shell (git, dotnet, pnpm, docker, curl, etc.)
  leer_archivo(ruta)             → lee cualquier archivo del repo
  escribir_archivo(ruta, cont)   → crea/sobreescribe archivo
  agregar_a_archivo(ruta, texto) → append (usar para escribir READ en prompt_1.md)

Uso:
  python3 .prompts/openclaw_loop.py                    # loop continuo
  python3 .prompts/openclaw_loop.py --once             # un ciclo y salir
  python3 .prompts/openclaw_loop.py --interval 30      # poll cada 30s
  python3 .prompts/openclaw_loop.py --dry-run          # ver tool calls sin ejecutar
  python3 .prompts/openclaw_loop.py --status           # estado actual de prompt_1.md
  python3 .prompts/openclaw_loop.py --max-tools 50     # más tool calls por ciclo

Variables de entorno:
  GROQ_API_KEY   → requerida (gsk_...)
  LOOP_INTERVAL  → segundos entre checks (default: 20)
  MAX_TOOL_CALLS → tope de herramientas por ciclo (default: 40)
"""

import argparse
import json
import os
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

from groq import Groq

# ─── Config ───────────────────────────────────────────────────────────────────
REPO          = Path(__file__).parent.parent
PROMPT_FILE   = REPO / ".prompts" / "prompt_1.md"
AUDIT_LOG     = REPO / ".github" / "copilot-monitor.log"
MODEL         = "llama-3.1-8b-instant"
POLL          = int(os.getenv("LOOP_INTERVAL", "20"))
MAX_TOOLS     = int(os.getenv("MAX_TOOL_CALLS", "40"))

SYSTEM_PROMPT = """\
Eres el agente de desarrollo senior de OKLA (marketplace automotriz, República Dominicana).
Stack: .NET 8 · Next.js 16 (App Router) · PostgreSQL · RabbitMQ · Redis · Docker · Kubernetes.
Repo raíz: /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

Tu misión en cada ciclo:
1. Detecta la FASE del archivo (AUDIT / FIX / REAUDIT / UNKNOWN).
2. Ejecuta SOLO las tareas con "- [ ]" (ignora las "- [x]" ya completadas).
3. Usa las herramientas disponibles: corre comandos, lee y escribe archivos.
4. Cuando TODAS las tareas pendientes estén completadas, llama agregar_a_archivo
   con ruta=".prompts/prompt_1.md" y texto="\\nREAD".

Reglas críticas:
- Respuestas cortas. Sin relleno ni explicaciones largas.
- Gate pre-commit OBLIGATORIO antes de git commit:
    dotnet build /p:TreatWarningsAsErrors=true
    cd frontend/web-next && pnpm lint && pnpm typecheck && CI=true pnpm test -- --run && pnpm build
    dotnet test --no-build --blame-hang --blame-hang-timeout 2min 2>&1 | grep -E "(Passed|Failed).*\\.dll"
- Rama: NUNCA hacer push directo a main → usar feature/* → staging → main.
- Si una tarea falla, documenta el error y continúa con la siguiente.
"""

# ─── Definición de herramientas (tool calling) ────────────────────────────────
TOOLS = [
    {
        "type": "function",
        "function": {
            "name": "correr_comando",
            "description": (
                "Ejecuta un comando shell en el directorio raíz del proyecto. "
                "Devuelve stdout + stderr. Úsalo para git, dotnet, pnpm, docker, curl, etc."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "cmd": {
                        "type": "string",
                        "description": "Comando shell a ejecutar (se corre en la raíz del repo)",
                    },
                    "timeout": {
                        "type": "integer",
                        "description": "Timeout en segundos (default 120). Usar 300+ para dotnet build/test.",
                        "default": 120,
                    },
                },
                "required": ["cmd"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "leer_archivo",
            "description": "Lee el contenido de un archivo del proyecto (ruta relativa desde la raíz).",
            "parameters": {
                "type": "object",
                "properties": {
                    "ruta": {
                        "type": "string",
                        "description": "Ruta relativa desde la raíz del repo, ej: 'backend/AuthService/AuthService.Api/Program.cs'",
                    }
                },
                "required": ["ruta"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "escribir_archivo",
            "description": "Crea o sobreescribe un archivo del proyecto con el contenido dado.",
            "parameters": {
                "type": "object",
                "properties": {
                    "ruta": {"type": "string", "description": "Ruta relativa desde la raíz del repo"},
                    "contenido": {"type": "string", "description": "Contenido completo del archivo"},
                },
                "required": ["ruta", "contenido"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "agregar_a_archivo",
            "description": (
                "Agrega texto al FINAL de un archivo (append). "
                "USAR para marcar tareas completadas con READ: "
                "ruta='.prompts/prompt_1.md', texto='\\nREAD'."
            ),
            "parameters": {
                "type": "object",
                "properties": {
                    "ruta": {"type": "string", "description": "Ruta relativa desde la raíz del repo"},
                    "texto": {"type": "string", "description": "Texto a agregar al final del archivo"},
                },
                "required": ["ruta", "texto"],
            },
        },
    },
]

# ─── Logging ──────────────────────────────────────────────────────────────────
def log(msg: str, level: str = "INFO") -> None:
    ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    line = f"[{ts}] [{level}] {msg}"
    print(line, flush=True)
    try:
        AUDIT_LOG.parent.mkdir(parents=True, exist_ok=True)
        with open(AUDIT_LOG, "a") as f:
            f.write(line + "\n")
    except Exception:
        pass


# ─── Ejecución de herramientas ────────────────────────────────────────────────
def exec_tool(name: str, args: dict, dry_run: bool = False) -> str:
    if dry_run:
        return f"[dry-run] {name}({args})"

    try:
        if name == "correr_comando":
            cmd = args["cmd"]
            timeout = int(args.get("timeout", 120))
            log(f"$ {cmd}", "TOOL")
            r = subprocess.run(
                cmd,
                shell=True,
                capture_output=True,
                text=True,
                timeout=timeout,
                cwd=str(REPO),
            )
            output = (r.stdout + r.stderr).strip()
            # Cap en 4000 chars para no saturar el contexto del LLM
            if len(output) > 4000:
                output = output[-4000:]
                output = f"[...truncado, mostrando últimos 4000 chars]\n{output}"
            log(f"rc={r.returncode} | {output[:100].replace(chr(10), ' ')}", "TOOL")
            return f"rc={r.returncode}\n{output}"

        elif name == "leer_archivo":
            path = REPO / args["ruta"]
            if not path.exists():
                return f"ERROR: archivo no encontrado: {args['ruta']}"
            content = path.read_text(encoding="utf-8")
            if len(content) > 6000:
                content = content[:6000] + "\n[...truncado a 6000 chars]"
            log(f"leer {args['ruta']} ({len(content)} chars)", "TOOL")
            return content

        elif name == "escribir_archivo":
            path = REPO / args["ruta"]
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(args["contenido"], encoding="utf-8")
            log(f"escribir {args['ruta']} ({len(args['contenido'])} chars)", "TOOL")
            return f"OK: {args['ruta']} escrito ({len(args['contenido'])} chars)"

        elif name == "agregar_a_archivo":
            path = REPO / args["ruta"]
            with open(path, "a", encoding="utf-8") as f:
                f.write(args["texto"])
            log(f"append {args['ruta']}: {repr(args['texto'][:50])}", "TOOL")
            return f"OK: texto agregado a {args['ruta']}"

        else:
            return f"ERROR: herramienta desconocida: {name}"

    except subprocess.TimeoutExpired:
        return f"ERROR: timeout ({args.get('timeout', 120)}s) al correr: {args.get('cmd', '')}"
    except Exception as exc:
        return f"ERROR: {exc}"


# ─── Ciclo agente Groq ────────────────────────────────────────────────────────
def run_agent_cycle(client: Groq, content: str, dry_run: bool = False, max_tools: int = MAX_TOOLS) -> bool:
    """
    Ciclo completo de tool calling con Groq.
    El LLM lee prompt_1.md, detecta tareas pendientes, las ejecuta con tools,
    y al final escribe READ en el archivo.
    Devuelve True si prompt_1.md termina en READ al finalizar.
    """
    messages: list[dict] = [
        {"role": "system", "content": SYSTEM_PROMPT},
        {
            "role": "user",
            "content": (
                f"Aquí está el archivo de tareas .prompts/prompt_1.md:\n\n{content}\n\n"
                "Ejecuta todas las tareas pendientes (- [ ]) usando las herramientas disponibles. "
                "Al terminar TODO, llama agregar_a_archivo con texto='\\nREAD'."
            ),
        },
    ]

    tool_calls_used = 0

    while tool_calls_used < max_tools:
        resp = client.chat.completions.create(
            model=MODEL,
            messages=messages,
            tools=TOOLS,
            tool_choice="auto",
            max_tokens=1024,
            temperature=0.1,
        )

        choice = resp.choices[0]
        assistant_msg = choice.message

        # Serializar el mensaje del asistente correctamente
        msg_dict: dict = {"role": "assistant"}
        if assistant_msg.content:
            msg_dict["content"] = assistant_msg.content
        if assistant_msg.tool_calls:
            msg_dict["tool_calls"] = [
                {
                    "id": tc.id,
                    "type": "function",
                    "function": {"name": tc.function.name, "arguments": tc.function.arguments},
                }
                for tc in assistant_msg.tool_calls
            ]
        messages.append(msg_dict)

        if choice.finish_reason == "tool_calls" and assistant_msg.tool_calls:
            for tc in assistant_msg.tool_calls:
                tool_calls_used += 1
                fn_name = tc.function.name
                try:
                    fn_args = json.loads(tc.function.arguments)
                except json.JSONDecodeError:
                    fn_args = {}

                if dry_run:
                    log(f"[DRY-RUN] {fn_name}({json.dumps(fn_args, ensure_ascii=False)[:120]})", "TOOL")
                    result = "(dry-run: no ejecutado)"
                else:
                    result = exec_tool(fn_name, fn_args, dry_run=False)

                messages.append({
                    "role": "tool",
                    "tool_call_id": tc.id,
                    "content": result,
                })
        else:
            # El modelo terminó (stop / sin tool calls pendientes)
            if assistant_msg.content:
                log(f"Agente: {assistant_msg.content[:300]}")
            break

    if tool_calls_used >= max_tools:
        log(f"Cap de {max_tools} tool calls alcanzado en este ciclo.", "WARN")

    return _last_line() == "READ"


# ─── Helpers de estado ────────────────────────────────────────────────────────
def _last_line() -> str:
    if not PROMPT_FILE.exists():
        return ""
    lines = PROMPT_FILE.read_text(encoding="utf-8").rstrip().splitlines()
    return lines[-1].strip() if lines else ""


def _has_tasks() -> bool:
    ll = _last_line()
    return ll not in ("READ", "STOP") and bool(ll)


# ─── Comando --status ─────────────────────────────────────────────────────────
def show_status() -> None:
    ll = _last_line()
    has_t = _has_tasks()
    stopped = ll == "STOP"
    size = PROMPT_FILE.stat().st_size if PROMPT_FILE.exists() else 0
    mtime = (
        datetime.fromtimestamp(PROMPT_FILE.stat().st_mtime).strftime("%Y-%m-%d %H:%M:%S")
        if PROMPT_FILE.exists()
        else "N/A"
    )
    estado = "🔴 STOP" if stopped else ("✅ TAREAS PENDIENTES" if has_t else "⏳ READ (esperando cambios)")

    print(f"\n{'─'*52}")
    print(f"  Archivo   : {PROMPT_FILE}")
    print(f"  Tamaño    : {size} bytes  |  Modificado: {mtime}")
    print(f"  Última línea: '{ll}'")
    print(f"  Estado    : {estado}")

    if has_t and PROMPT_FILE.exists():
        content = PROMPT_FILE.read_text(encoding="utf-8")
        pending = content.count("- [ ]")
        done = len([l for l in content.splitlines() if "- [x]" in l.lower()])
        print(f"  Pendientes: {pending} | Completadas: {done} | Total: {pending + done}")

    print(f"{'─'*52}\n")


# ─── Loop principal ───────────────────────────────────────────────────────────
def run_loop(poll: int, once: bool, dry_run: bool, max_tools: int) -> None:
    api_key = os.getenv("GROQ_API_KEY")
    if not api_key:
        log("ERROR: GROQ_API_KEY no definida. Ejecuta: export GROQ_API_KEY=gsk_...", "ERROR")
        sys.exit(1)

    client = Groq(api_key=api_key)

    log(f"=== okla_loop v3 | modelo=groq/{MODEL} | poll={poll}s | max_tools={max_tools} ===")
    log(f"Monitoreando: {PROMPT_FILE}")

    while True:
        ll = _last_line()

        if ll == "STOP":
            log("STOP detectado en prompt_1.md. Loop terminado.")
            break

        if _has_tasks():
            content = PROMPT_FILE.read_text(encoding="utf-8")
            pending = content.count("- [ ]")
            done = len([line for line in content.splitlines() if "- [x]" in line.lower()])
            log(f"Tareas: {pending} pendientes / {done} hechas — iniciando ciclo Groq...")

            ok = run_agent_cycle(client, content, dry_run=dry_run, max_tools=max_tools)

            if ok:
                log("✅ Ciclo completado — READ detectado en prompt_1.md.")
            else:
                log("⚠️  Ciclo terminó sin READ. Revisar prompt_1.md manualmente.", "WARN")

            if once or dry_run:
                break
        else:
            log(f"Sin tareas nuevas ('{ll}'). Esperando {poll}s...")
            if once:
                break
            time.sleep(poll)

    log("=== okla_loop finalizado ===")


# ─── Entry point ─────────────────────────────────────────────────────────────
def main() -> None:
    parser = argparse.ArgumentParser(
        description="Loop inteligente OKLA — Groq llama-3.1-8b-instant con tool calling"
    )
    parser.add_argument("--interval",  type=int, default=POLL,       help=f"Segundos entre polls (default: {POLL})")
    parser.add_argument("--once",      action="store_true",           help="Ejecutar un solo ciclo y salir")
    parser.add_argument("--dry-run",   action="store_true",           help="Ver tool calls sin ejecutar nada")
    parser.add_argument("--status",    action="store_true",           help="Estado actual de prompt_1.md y salir")
    parser.add_argument("--max-tools", type=int, default=MAX_TOOLS,   help=f"Tool calls máx por ciclo (default: {MAX_TOOLS})")
    args = parser.parse_args()

    if args.status:
        show_status()
        return

    run_loop(
        poll=args.interval,
        once=args.once,
        dry_run=args.dry_run,
        max_tools=args.max_tools,
    )


if __name__ == "__main__":
    main()
