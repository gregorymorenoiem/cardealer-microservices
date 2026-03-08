import json
import random
import time
import os
from pathlib import Path

# ------------------------------------------------------------
# 1. Construcción de listas grandes de frases (≈50 cada una)
# ------------------------------------------------------------

# Verbos y frases para iniciar la orden
verbos_inicio = [
    "Ejecuta", "Corre", "Inicia", "Lanza", "Procesa", "Realiza", "Lleva a cabo",
    "Arranca", "Pon en marcha", "Comienza con", "Haz", "Ejecuta ahora", "Ejecuta por favor",
    "Ejecuta inmediatamente", "Ejecuta ya", "Ejecuta sin demora", "Procede a ejecutar",
    "Vamos a ejecutar", "Quiero que ejecutes", "Necesito que ejecutes", "Debes ejecutar",
    "Te pido que ejecutes", "Sería bueno que ejecutes", "Ejecuta rápido", "Ejecuta de una vez",
    "Ejecuta cuanto antes", "Ejecuta el día de hoy", "Ejecuta en este momento"
]

# Modificadores para dar variedad
modificadores_inicio = [
    "", "ahora", "de inmediato", "ya mismo", "sin demora", "rápidamente",
    "por favor", "urgentemente", "en este instante", "a continuación"
]

# Construimos INICIOS combinando verbos y modificadores
INICIOS = []
for verbo in verbos_inicio:
    for mod in modificadores_inicio:
        if mod:
            frase = f"{verbo} {mod} el prompt"
        else:
            frase = f"{verbo} el prompt"
        INICIOS.append(frase)
# Eliminar duplicados y limitar a 50
INICIOS = list(set(INICIOS))[:50]

# ------------------------------------------------
PRIMERAS_VECES = [
    "Como si fuera la primera vez",
    "Como si nunca lo hubieras hecho",
    "Como si fuera la primera ocasión",
    "Hazlo como si fuera la primera ejecución",
    "Simula que es la primera vez",
    "Como si no tuvieras experiencia previa",
    "Imaginando que es la primera vez",
    "Como si partieras de cero",
    "Como si jamás lo hubieras realizado",
    "Como si fuese la primera vez",
    "Como si no hubieras ejecutado nada antes",
    "Como si empezaras de nuevo",
    "Como si fuera un sistema nuevo",
    "Como si no hubiera historial",
    "Como si nunca antes hubieras visto este prompt",
    "Hazlo con mentalidad de principiante",
    "Hazlo como si no supieras nada",
    "Hazlo desde cero",
    "Hazlo como si fuera el primer día",
    "Hazlo sin asumir nada previo",
    "Como si no hubiera contexto anterior",
    "Como si no hubieras leído los otros prompts",
    "Como si fuera la primera interacción",
    "Como si no hubieras procesado nada aún",
    "Como si todo empezara ahora",
    "Como si nunca hubieras visto esto",
    "Como si fuera la primera ejecución en la historia",
]

mas_primera = [
    "Como si fuese la primera ocasión",
    "Como si fuera un debut",
    "Como si fuera un estreno",
    "Como si no tuvieras registro",
    "Como si no hubiera pasado nada antes",
    "Como si el prompt fuera nuevo para ti",
    "Como si nunca hubieras trabajado con esto",
    "Como si fuera la primera vez que ves este tipo de instrucción",
]
PRIMERAS_VECES.extend(mas_primera)
PRIMERAS_VECES = list(set(PRIMERAS_VECES))[:50]

# ------------------------------------------------
# Lista original de frases de auditoría (para generación aleatoria en JSON)
TIPOS_AUDITORIA = [
    "y haz otro tipo de auditoría",
    "y realiza una auditoría diferente",
    "y efectúa una revisión distinta",
    "y lleva a cabo una auditoría alternativa",
    "y ejecuta una auditoría con otro enfoque",
    "y cambia el tipo de auditoría",
    "y prueba una auditoría novedosa",
    "y aplica una metodología de auditoría distinta",
    "y haz una auditoría con un criterio diferente",
    "y realiza una inspección distinta",
    "y efectúa un análisis diferente",
    "y lleva a cabo un examen alternativo",
    "y ejecuta una revisión con otro método",
    "y cambia el enfoque de la auditoría",
    "y prueba una nueva forma de auditar",
    "y aplica una técnica de auditoría diferente",
    "y haz una auditoría desde otra perspectiva",
    "y realiza una auditoría con parámetros distintos",
    "y efectúa una auditoría con nuevas reglas",
    "y lleva a cabo una auditoría modificada",
    "y ejecuta una auditoría con variantes",
    "y prueba un estilo de auditoría diferente",
    "y aplica un modelo de auditoría novedoso",
    "y haz una auditoría no convencional",
    "y realiza una auditoría con otro alcance",
    "y efectúa una auditoría con otras métricas",
]
TIPOS_AUDITORIA = list(set(TIPOS_AUDITORIA))[:50]

# ------------------------------------------------
# CICLO MAESTRO OKLA MARKETPLACE — Rotación completa integrada
# Cubre los 16 ciclos: Modelo de Negocio, Dealers, OKLA Score,
# Agentes IA, Prompt Engineering, Base de Conocimiento, RAG,
# Aprendizaje Continuo, Seguridad, Infraestructura, UX/UI,
# Testing, Analytics, Arquitectura y Estrategia de Mercado RD.
TIPOS_AUDITORIA_SECUENCIAL = [
    # ── MODELO DE NEGOCIO
    "Modelo Freemium: Validar listings gratuitos ilimitados y revenue por visibilidad + publicidad y ejecuta correcciones",
    "Planes Libre/Visible/Pro/Élite: Auditar diferenciación, márgenes y tasa de upgrade entre planes y ejecuta correcciones",
    "OKLA Coins: Auditar sistema de créditos, bonificaciones por volumen y conversión publicitaria y ejecuta correcciones",
    "Break-even 45 Dealers: Verificar que el mix actual de planes cubre el OPEX mensual completo y ejecuta correcciones",
    # ── DEALERS
    "Adquisición de Dealers: Auditar funnel desde contacto inicial hasta primer listing publicado y ejecuta correcciones",
    "Importación desde Facebook: Auditar ListingAgent procesando URLs de Facebook Marketplace automáticamente y ejecuta correcciones",
    "Retención de Dealers: Auditar tasa de churn mensual y acciones de retención activas por plan y ejecuta correcciones",
    # ── OKLA SCORE
    "D1 Historial VIN (25%): Auditar integración CARFAX/VinAudit/NHTSA y precisión del score de historial y ejecuta correcciones",
    "D3 Odómetro (18%): Auditar detección de rollback y conversión correcta millas/kilómetros y ejecuta correcciones",
    "D4 Precio vs Mercado (17%): Auditar Algoritmo Precio OKLA y scraping de portales RD y ejecuta correcciones",
    "D2 Condición Mecánica (20%): Auditar scoring de equipamiento, recalls activos y quejas NHTSA y ejecuta correcciones",
    "OKLA Score Completo: Auditar suma ponderada de 7 dimensiones y rango 0-1,000 con tipo de cambio BCRD y ejecuta correcciones",
    # ── AGENTES IA
    "SearchAgent (Haiku 4.5): Auditar precisión de búsqueda, filtros y latencia <2s y ejecuta correcciones",
    "RecoAgent (Sonnet 4.5): Auditar personalización, diversificación y calidad de explicaciones en español dominicano y ejecuta correcciones",
    "DealerChatAgent: Auditar respuestas 24/7, límite de conversaciones y overage $0.08 y ejecuta correcciones",
    "PricingAgent: Auditar cálculo de precio justo e identificación de sobrevaluaciones >20% y ejecuta correcciones",
    "ListingAgent: Auditar procesamiento automático de fotos y creación de listings optimizados y ejecuta correcciones",
    # ── PROMPT ENGINEERING
    "System Prompts: Auditar estructura, rol, tono dominicano y formato de salida de cada agente y ejecuta correcciones",
    "Constitutional AI: Auditar que los principios de comportamiento previenen respuestas fuera de política y ejecuta correcciones",
    "Few-Shot Examples: Auditar cobertura de escenarios reales del mercado automotriz dominicano y ejecuta correcciones",
    "Prompt Caching: Auditar reducción de costos en conversaciones repetitivas del DealerChatAgent y ejecuta correcciones",
    # ── BASE DE CONOCIMIENTO
    "Base de Conocimiento: Auditar actualización de listings, precios, FAQs y políticas vigentes y ejecuta correcciones",
    "APIs Externas: Auditar estado de NHTSA vPIC, VinAudit, MarketCheck y BCRD tipo de cambio y ejecuta correcciones",
    "RAG Pipeline: Auditar chunking, embeddings, relevancia y latencia de recuperación <500ms y ejecuta correcciones",
    # ── APRENDIZAJE CONTINUO
    "Feedback Loop: Auditar captura de señales explícitas e implícitas y ciclo de mejora semanal y ejecuta correcciones",
    "LLM-as-a-Judge: Auditar evaluación automática de calidad de respuestas y tasa de alucinaciones y ejecuta correcciones",
    "A/B Testing Prompts: Auditar experimentos activos y métricas de conversión por variante y ejecuta correcciones",
    # ── SEGURIDAD
    "OWASP + Prompt Injection: Auditar vulnerabilidades en backend, frontend y manipulación de agentes IA y ejecuta correcciones",
    "KYC Dealers + Ley 172-13: Auditar verificación de identidad y cumplimiento de protección de datos RD y ejecuta correcciones",
    "Detección de Fraude: Auditar identificación de VINs clonados, rollback y listings fraudulentos y ejecuta correcciones",
    # ── INFRAESTRUCTURA
    "DOKS Kubernetes: Auditar estado de pods, deployments y costos de DigitalOcean vs proyectado y ejecuta correcciones",
    "CI/CD GitHub Actions: Auditar pipelines, tiempos de deploy y estrategia blue-green y ejecuta correcciones",
    "Costos Claude API: Auditar gasto por agente y optimizar modelo Haiku vs Sonnet por caso de uso y ejecuta correcciones",
    # ── UX/UI
    "Core Web Vitals Mobile: Auditar LCP/CLS/INP en 4G dominicana y flujo comprador sin fricciones y ejecuta correcciones",
    "OKLA Score UX: Auditar que el semáforo es comprensible para el comprador promedio dominicano y ejecuta correcciones",
    # ── TESTING
    "Cobertura de Tests: Auditar unit, integration, E2E y tests de agentes IA (objetivo >80% coverage) y ejecuta correcciones",
    "Red Teaming IA: Auditar intentos de jailbreak y respuestas fuera de política en todos los agentes y ejecuta correcciones",
    # ── ANALYTICS
    "KPIs Core: Auditar MRR, churn, CAC, LTV, conversión y tráfico vs proyecciones del modelo Freemium v3 y ejecuta correcciones",
    "SEO Técnico: Auditar posicionamiento para keywords automotrices dominicanas y ventaja vs SuperCarros y ejecuta correcciones",
    # ── ESTRATEGIA
    "Mercado RD: Auditar tendencias DGII/Aduanas, competidores y oportunidades de nuevos features premium y ejecuta correcciones",
    "Expansión Caribe: Auditar preparación técnica y regulatoria para primer mercado fuera de RD y ejecuta correcciones",
]

# ------------------------------------------------
MONITOREOS = [
    "Y sigue monitoriando el archivo .prompts/prompt_6.md.",
    "Continúa monitoreando el archivo .prompts/prompt_6.md.",
    "No olvides monitorear .prompts/prompt_6.md.",
    "Sigue vigilando el archivo .prompts/prompt_6.md.",
    "Mantén el monitoreo de .prompts/prompt_6.md.",
    "Y continúa con la monitorización de .prompts/prompt_6.md.",
    "Además, sigue monitoreando .prompts/prompt_6.md.",
    "Y no dejes de monitorear .prompts/prompt_6.md.",
    "Sigue con el monitoreo de .prompts/prompt_6.md.",
    "Y permanece atento a .prompts/prompt_6.md.",
    "No detengas la monitorización de .prompts/prompt_6.md.",
    "Y prosigue con la vigilancia de .prompts/prompt_6.md.",
    "Y continúa observando .prompts/prompt_6.md.",
    "Sigue revisando .prompts/prompt_6.md.",
    "Mantén la vigilancia sobre .prompts/prompt_6.md.",
    "No pares de monitorear .prompts/prompt_6.md.",
    "Y sigue pendiente de .prompts/prompt_6.md.",
    "Continúa atento a .prompts/prompt_6.md.",
    "Y no descuides el monitoreo de .prompts/prompt_6.md.",
    "Sigue supervisando .prompts/prompt_6.md.",
    "Mantén el control sobre .prompts/prompt_6.md.",
    "Y sigue chequeando .prompts/prompt_6.md.",
    "Continúa con la supervisión de .prompts/prompt_6.md.",
    "No interrumpas la monitorización de .prompts/prompt_6.md.",
]
MONITOREOS = list(set(MONITOREOS))[:50]

print(f"Tamaños: INICIOS={len(INICIOS)}, PRIMERAS_VECES={len(PRIMERAS_VECES)}, TIPOS_AUDITORIA={len(TIPOS_AUDITORIA)}, MONITOREOS={len(MONITOREOS)}")

# ------------------------------------------------------------
# 2. Funciones de generación de mensajes
# ------------------------------------------------------------

def generar_mensaje(prompt_num, tipo_auditoria=None):
    """
    Genera un mensaje combinando aleatoriamente una frase de cada lista.
    Si se proporciona tipo_auditoria (string), se usa ese tipo en lugar de elegir
    uno aleatorio de TIPOS_AUDITORIA, y se añade la orden de ejecutar recomendaciones.
    """
    inicio = random.choice(INICIOS)
    primera_vez = random.choice(PRIMERAS_VECES)
    monitoreo = random.choice(MONITOREOS)

    if tipo_auditoria is not None:
        # Modo secuencial: usamos el tipo proporcionado y ordenamos ejecutar recomendaciones
        parte_auditoria = f"y realiza una auditoría de {tipo_auditoria} y ejecuta inmediatamente sus recomendaciones."
    else:
        # Modo aleatorio (para generar JSON): elegimos una frase de la lista antigua
        parte_auditoria = random.choice(TIPOS_AUDITORIA)

    return f"{inicio} {prompt_num}. {primera_vez} {parte_auditoria} {monitoreo}"

def generar_lista_mensajes(cantidad):
    """Genera una lista de mensajes con prompts aleatorios sin repetición consecutiva."""
    mensajes = []
    ultimo_prompt = None
    for _ in range(cantidad):
        opciones = [p for p in range(1, 6) if p != ultimo_prompt]
        prompt = random.choice(opciones)
        mensajes.append(generar_mensaje(prompt))  # Sin tipo, usa aleatorio
        ultimo_prompt = prompt
    return mensajes

# ------------------------------------------------------------
# 3. Manejo de archivos
# ------------------------------------------------------------

def guardar_json(mensajes, archivo):
    with open(archivo, "w", encoding="utf-8") as f:
        json.dump(mensajes, f, indent=2, ensure_ascii=False)

def cargar_json(archivo):
    with open(archivo, "r", encoding="utf-8") as f:
        return json.load(f)

def asegurar_directorio(ruta):
    Path(ruta).parent.mkdir(parents=True, exist_ok=True)

def escribir_en_archivo(archivo, mensaje):
    asegurar_directorio(archivo)
    with open(archivo, "w", encoding="utf-8") as f:
        f.write(mensaje)

# ------------------------------------------------------------
# 4. Lectura segura del archivo de destino
# ------------------------------------------------------------

def leer_archivo(archivo):
    """Lee el contenido del archivo; retorna cadena vacía si no existe."""
    try:
        with open(archivo, "r", encoding="utf-8") as f:
            return f.read()
    except FileNotFoundError:
        return ""

# ------------------------------------------------------------
# 5. Programa principal
# ------------------------------------------------------------

def main():
    import argparse
    parser = argparse.ArgumentParser(
        description="Actualiza .prompts/prompt_6.md solo cuando detecta la palabra READ en el archivo."
    )
    parser.add_argument("--json", type=str, default="mensajes_100.json",
                        help="Archivo JSON donde guardar 100 mensajes de ejemplo")
    parser.add_argument("--intervalo", type=int, default=2,
                        help="Intervalo en segundos entre comprobaciones del archivo (default: 2)")
    parser.add_argument("--generar-json", action="store_true",
                        help="Solo genera el JSON con 100 mensajes y termina")
    args = parser.parse_args()

    archivo_destino = ".prompts/prompt_6.md"

    if args.generar_json:
        print("Generando 100 mensajes de ejemplo...")
        mensajes_ejemplo = generar_lista_mensajes(100)
        guardar_json(mensajes_ejemplo, args.json)
        print(f"JSON guardado en '{args.json}'")
        return

    print(f"Monitoreando '{archivo_destino}' cada {args.intervalo} segundos.")
    print("Se actualizará el contenido cada vez que se detecte la palabra READ (en mayúsculas).")
    ultimo_prompt = None
    indice_auditoria = 0  # Índice para la lista secuencial

    try:
        while True:
            contenido = leer_archivo(archivo_destino)
            if "READ" in contenido:
                # Calcular el tipo actual (circular)
                tipo_actual = TIPOS_AUDITORIA_SECUENCIAL[indice_auditoria % len(TIPOS_AUDITORIA_SECUENCIAL)]

                # Si es el inicio de un nuevo ciclo, mostramos un mensaje informativo
                if indice_auditoria % len(TIPOS_AUDITORIA_SECUENCIAL) == 0:
                    print("\n--- 🔄 Ciclo completo de auditorías finalizado. Comenzando nuevo ciclo. ---\n")

                # Elegir número de prompt sin repetir el último
                opciones = [p for p in range(1, 6) if p != ultimo_prompt]
                prompt = random.choice(opciones)
                mensaje = generar_mensaje(prompt, tipo_auditoria=tipo_actual)
                escribir_en_archivo(archivo_destino, mensaje)
                print(f"[{time.strftime('%Y-%m-%d %H:%M:%S')}] READ detectado → Prompt {prompt} | Auditoría: {tipo_actual}")
                print(f"Mensaje: {mensaje[:120]}...\n")
                ultimo_prompt = prompt

                # Incrementar el índice para la próxima auditoría
                indice_auditoria += 1

            time.sleep(args.intervalo)
    except KeyboardInterrupt:
        print("\nProceso detenido por el usuario.")

if __name__ == "__main__":
    main()