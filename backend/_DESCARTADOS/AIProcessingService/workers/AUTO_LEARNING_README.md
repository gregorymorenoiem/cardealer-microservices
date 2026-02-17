# 🧠 Auto-Learning System para Background Removal

Sistema de aprendizaje automático que perfecciona automáticamente el proceso de eliminación de fondo y generación de sombras para vehículos.

## 🌟 Características Principales

- **🦙 Ollama como Evaluador**: Usa LLaVA (modelo de visión multimodal) para evaluar la calidad de los resultados - 100% LOCAL, GRATIS, SIN LÍMITES
- **🧬 Red Neuronal Predictora**: Predice los parámetros óptimos para cada imagen
- **🎯 Aprendizaje por Refuerzo**: Mejora continuamente basándose en el feedback
- **💾 Base de Datos SQLite**: Almacena todo el conocimiento aprendido
- **🔄 Auto-mejora**: El sistema mejora con cada imagen procesada

## 📋 Requisitos

- Python 3.9+
- Ollama instalado y corriendo
- ~4GB RAM mínimo (8GB recomendado)
- ~3GB espacio en disco para modelos

## 🚀 Instalación Rápida

```bash
# 1. Ejecutar script de setup
chmod +x setup_autolearn.sh
./setup_autolearn.sh

# 2. Activar entorno virtual
source venv_autolearn/bin/activate

# 3. Verificar que Ollama está corriendo
ollama serve  # En otra terminal si no está corriendo
```

## 📖 Uso

### Procesar una sola imagen

```bash
python auto_learning_system.py --mode single --input ./input/car.jpg
```

### Procesar todas las imágenes en un directorio

```bash
python auto_learning_system.py --mode single --input ./input
```

### Entrenamiento batch con múltiples épocas

```bash
python auto_learning_system.py --mode batch --input ./input --epochs 5
```

### Modo continuo (monitorea directorio)

```bash
python auto_learning_system.py --mode continuous --input ./input
```

### Ver estadísticas

```bash
python auto_learning_system.py --mode stats
```

## ⚙️ Opciones de Línea de Comandos

| Opción             | Descripción                            | Default            |
| ------------------ | -------------------------------------- | ------------------ |
| `--mode`           | Modo: single, batch, continuous, stats | single             |
| `--input`          | Imagen o directorio de entrada         | ./input            |
| `--output`         | Directorio de salida                   | ./output_autolearn |
| `--target-score`   | Puntuación objetivo (0-100)            | 90                 |
| `--max-iterations` | Máximo de iteraciones por imagen       | 5                  |
| `--epochs`         | Épocas para modo batch                 | 1                  |
| `--ollama-model`   | Modelo Ollama a usar                   | llava:7b           |

## 🏗️ Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         AUTO-LEARNING SYSTEM                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐                   │
│  │   INPUT     │ ──► │  PREDICTOR  │ ──► │  PIPELINE   │                   │
│  │   Image     │     │  (Neural)   │     │  V7 Process │                   │
│  └─────────────┘     └─────────────┘     └─────────────┘                   │
│                            ▲                    │                           │
│                            │                    ▼                           │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐                   │
│  │  DATABASE   │ ◄── │  RL AGENT   │ ◄── │  OLLAMA     │                   │
│  │  SQLite     │     │  Update     │     │  LLaVA Eval │                   │
│  └─────────────┘     └─────────────┘     └─────────────┘                   │
│                                                                             │
│  Flujo:                                                                     │
│  1. Imagen entra al sistema                                                 │
│  2. Predictor predice parámetros óptimos (o usa caché)                     │
│  3. Pipeline procesa la imagen                                              │
│  4. Ollama evalúa el resultado y da feedback                               │
│  5. RL Agent ajusta parámetros basado en feedback                          │
│  6. Database almacena conocimiento                                          │
│  7. Sistema mejora con cada imagen                                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 📊 Componentes

### 1. OllamaEvaluator

Evalúa la calidad del resultado usando LLaVA multimodal:

- Analiza imagen original vs resultado
- Detecta problemas específicos (ruedas cortadas, sombras, bordes, etc.)
- Da puntuación 0-100 y clasificación
- Proporciona sugerencias de mejora

### 2. ParamPredictor

Red neuronal que predice parámetros óptimos:

- CNN para extraer características de imagen
- MLP para predecir 20 parámetros
- Aprende qué funciona para diferentes tipos de imágenes

### 3. ReinforcementAgent

Agente de aprendizaje por refuerzo:

- Almacena experiencias (estado, acción, recompensa)
- Actualiza predictor basado en recompensas
- Mejora continuamente

### 4. ProcessingPipeline

Pipeline de procesamiento (basado en V7):

- YOLO para detección de vehículos
- SAM para segmentación precisa
- Post-procesamiento con parámetros ajustables
- Generación de sombras profesionales

### 5. LearningDatabase

Base de datos SQLite para conocimiento:

- Historial de procesamiento
- Parámetros óptimos por imagen
- Estadísticas de mejora

## 📈 Parámetros Optimizables

El sistema aprende automáticamente estos 20 parámetros:

| Categoría        | Parámetro             | Rango       |
| ---------------- | --------------------- | ----------- |
| **Detección**    | detection_confidence  | 0.1 - 0.5   |
| **Segmentación** | sam_points_per_side   | 16 - 64     |
| **Máscara**      | dilation_iterations   | 0 - 5       |
|                  | erosion_iterations    | 0 - 5       |
|                  | fill_holes            | true/false  |
| **Bordes**       | edge_softness         | 1.0 - 5.0   |
|                  | edge_feather          | 0.1 - 2.0   |
| **Sombra**       | shadow_enabled        | true/false  |
|                  | shadow_intensity      | 0.2 - 0.7   |
|                  | shadow_blur           | 10 - 40     |
|                  | shadow_bottom_offset  | 0.05 - 0.25 |
|                  | shadow_side_offset    | 0.02 - 0.15 |
|                  | contact_shadow_height | 0.01 - 0.05 |
|                  | ambient_shadow_height | 0.05 - 0.2  |
|                  | wheel_shadow_boost    | 0.2 - 0.5   |
| **Colores**      | shadow_color_r/g/b    | 0 - 50      |
| **Post**         | alpha_threshold       | 0.05 - 0.2  |
|                  | final_denoise         | true/false  |

## 🎯 Sistema de Puntuación

| Rango  | Clasificación | Descripción                   |
| ------ | ------------- | ----------------------------- |
| 90-100 | 🌟 EXCELLENT  | Calidad profesional, perfecto |
| 75-89  | ✅ GOOD       | Pequeños detalles menores     |
| 60-74  | ⚠️ ACCEPTABLE | Aceptable pero mejorable      |
| 40-59  | 🔧 NEEDS_WORK | Problemas significativos      |
| 0-39   | ❌ REJECTED   | Inaceptable, rehacer          |

## 📁 Estructura de Directorios

```
workers/
├── auto_learning_system.py    # Sistema principal
├── setup_autolearn.sh         # Script de instalación
├── autolearn_viewer.html      # Visualizador de resultados
├── input/                     # Imágenes de entrada
├── output_autolearn/          # Resultados
│   ├── transparent/           # Sin fondo
│   ├── shadow/                # Con sombra
│   └── debug/                 # Debugging
├── checkpoints/               # Modelos guardados
├── learning_logs/             # Logs de entrenamiento
├── auto_learning.db           # Base de datos SQLite
└── sam_vit_h_4b8939.pth      # Modelo SAM
```

## 🔧 Troubleshooting

### Ollama no responde

```bash
# Verificar que está corriendo
ollama list

# Si no responde, iniciar
ollama serve

# Descargar modelo si no existe
ollama pull llava:7b
```

### Memoria insuficiente

```bash
# Usar modelo más pequeño
python auto_learning_system.py --ollama-model llava:7b

# O reducir iterations
python auto_learning_system.py --max-iterations 3
```

### SAM no carga

```bash
# Descargar modelo
curl -L -o sam_vit_h_4b8939.pth \
  "https://dl.fbaipublicfiles.com/segment_anything/sam_vit_h_4b8939.pth"
```

## 📊 Visualizar Resultados

1. Abrir `autolearn_viewer.html` en un navegador
2. Click "Load JSON" y seleccionar archivo de logs
3. Explorar resultados, scores y parámetros

## 🤖 Modelos de Ollama Compatibles

| Modelo      | Tamaño | Velocidad | Calidad   |
| ----------- | ------ | --------- | --------- |
| `llava:7b`  | 4.5GB  | ⚡ Rápido | Buena     |
| `llava:13b` | 8GB    | Medio     | Muy buena |
| `llava:34b` | 20GB   | Lento     | Excelente |
| `bakllava`  | 4.5GB  | ⚡ Rápido | Buena     |

## 📈 Tips para Mejores Resultados

1. **Imágenes de alta calidad**: El sistema funciona mejor con fotos de buena resolución
2. **Vehículo centrado**: Asegúrate de que el vehículo esté centrado y visible
3. **Iluminación uniforme**: Evita sombras fuertes o contraluces
4. **Múltiples épocas**: Para entrenamiento, usa más épocas (5-10)
5. **Target score realista**: 85-90 es un buen objetivo inicial

## 🔄 Proceso de Aprendizaje

```
Iteración 1: Usa parámetros predichos o defaults
     ↓
Evaluación: Ollama analiza resultado
     ↓
Feedback: "Ruedas cortadas, bordes ásperos"
     ↓
Iteración 2: Ajusta parámetros (más dilatación, más suavizado)
     ↓
Evaluación: Ollama analiza nuevo resultado
     ↓
Feedback: "Mucho mejor, sombra muy oscura"
     ↓
Iteración 3: Ajusta sombra
     ↓
Evaluación: Score 92/100 - EXCELLENT
     ↓
Guarda parámetros óptimos para esta imagen
     ↓
RL Agent aprende de esta experiencia
     ↓
Siguiente imagen se beneficia del aprendizaje
```

## 🌟 Ventajas sobre el Approach Manual

1. **Auto-mejora**: No necesitas ajustar parámetros manualmente
2. **Consistencia**: Aprende qué funciona y lo aplica
3. **Escalabilidad**: Procesa cientos de imágenes con calidad consistente
4. **Transferencia**: Lo aprendido con una imagen ayuda con otras similares
5. **Sin costos de API**: Ollama es 100% local y gratuito

## 📝 Licencia

MIT License - Libre para uso comercial y personal.

## 👤 Autor

Gregory Moreno - Enero 2026
