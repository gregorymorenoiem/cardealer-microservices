#!/bin/bash
# =============================================================================
# Setup Script para Auto-Learning System
# =============================================================================
# Este script instala todas las dependencias necesarias y configura Ollama
#
# Uso: chmod +x setup_autolearn.sh && ./setup_autolearn.sh
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "🚀 Configurando Auto-Learning System para Background Removal"
echo "=============================================================="
echo ""

# =============================================================================
# 1. Verificar Python
# =============================================================================
echo "📦 Verificando Python..."
if command -v python3 &> /dev/null; then
    PYTHON_VERSION=$(python3 --version 2>&1)
    echo "   ✅ Python encontrado: $PYTHON_VERSION"
else
    echo "   ❌ Python 3 no encontrado. Por favor instálalo primero."
    exit 1
fi

# =============================================================================
# 2. Crear/Activar entorno virtual
# =============================================================================
VENV_DIR="$SCRIPT_DIR/venv_autolearn"

if [ ! -d "$VENV_DIR" ]; then
    echo ""
    echo "📦 Creando entorno virtual en $VENV_DIR..."
    python3 -m venv "$VENV_DIR"
    echo "   ✅ Entorno virtual creado"
fi

echo ""
echo "📦 Activando entorno virtual..."
source "$VENV_DIR/bin/activate"
echo "   ✅ Activado: $(which python)"

# =============================================================================
# 3. Instalar dependencias Python
# =============================================================================
echo ""
echo "📦 Instalando dependencias Python..."

pip install --upgrade pip wheel setuptools > /dev/null

# Core dependencies
pip install torch torchvision --index-url https://download.pytorch.org/whl/cpu
pip install numpy scipy pillow opencv-python-headless

# Ollama
pip install ollama

# Segment Anything
pip install segment-anything

# YOLO
pip install ultralytics

# Transformers (opcional, para CLIP)
pip install transformers

echo "   ✅ Dependencias Python instaladas"

# =============================================================================
# 4. Verificar/Instalar Ollama
# =============================================================================
echo ""
echo "🦙 Verificando Ollama..."

if command -v ollama &> /dev/null; then
    echo "   ✅ Ollama instalado"
else
    echo "   ⚠️ Ollama no encontrado. Instalando..."
    
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        if command -v brew &> /dev/null; then
            brew install ollama
        else
            echo "   Descargando Ollama para macOS..."
            curl -fsSL https://ollama.com/install.sh | sh
        fi
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        # Linux
        curl -fsSL https://ollama.com/install.sh | sh
    else
        echo "   ❌ Sistema no soportado. Instala Ollama manualmente desde: https://ollama.com"
        exit 1
    fi
fi

# =============================================================================
# 5. Descargar modelo LLaVA para Ollama
# =============================================================================
echo ""
echo "🧠 Verificando modelo LLaVA..."

# Iniciar Ollama en background si no está corriendo
if ! pgrep -x "ollama" > /dev/null; then
    echo "   Iniciando Ollama..."
    ollama serve &
    sleep 3
fi

# Verificar si LLaVA está instalado
if ollama list 2>/dev/null | grep -q "llava:7b"; then
    echo "   ✅ LLaVA:7b ya instalado"
else
    echo "   📥 Descargando LLaVA:7b (esto puede tardar varios minutos)..."
    ollama pull llava:7b
    echo "   ✅ LLaVA:7b instalado"
fi

# =============================================================================
# 6. Descargar modelo SAM
# =============================================================================
echo ""
echo "🎯 Verificando modelo SAM..."

SAM_FILE="$SCRIPT_DIR/sam_vit_h_4b8939.pth"
if [ -f "$SAM_FILE" ]; then
    echo "   ✅ SAM checkpoint existe"
else
    echo "   📥 Descargando SAM checkpoint (~2.5GB)..."
    curl -L -o "$SAM_FILE" \
        "https://dl.fbaipublicfiles.com/segment_anything/sam_vit_h_4b8939.pth"
    echo "   ✅ SAM descargado"
fi

# =============================================================================
# 7. Crear directorios necesarios
# =============================================================================
echo ""
echo "📁 Creando directorios..."

mkdir -p "$SCRIPT_DIR/input"
mkdir -p "$SCRIPT_DIR/output_autolearn/transparent"
mkdir -p "$SCRIPT_DIR/output_autolearn/shadow"
mkdir -p "$SCRIPT_DIR/output_autolearn/debug"
mkdir -p "$SCRIPT_DIR/checkpoints"
mkdir -p "$SCRIPT_DIR/learning_logs"

echo "   ✅ Directorios creados"

# =============================================================================
# 8. Verificación final
# =============================================================================
echo ""
echo "🔍 Verificación final..."

# Test Python imports
python3 -c "import torch; print(f'   ✅ PyTorch {torch.__version__}')"
python3 -c "import cv2; print(f'   ✅ OpenCV {cv2.__version__}')"
python3 -c "import ollama; print('   ✅ Ollama Python client')"

# Test Ollama connection
if ollama list &> /dev/null; then
    echo "   ✅ Ollama servidor respondiendo"
else
    echo "   ⚠️ Ollama no responde. Ejecuta: ollama serve"
fi

# =============================================================================
# 9. Instrucciones finales
# =============================================================================
echo ""
echo "=============================================================="
echo "✅ SETUP COMPLETADO"
echo "=============================================================="
echo ""
echo "📋 Próximos pasos:"
echo ""
echo "1. Asegurar que Ollama esté corriendo:"
echo "   ollama serve"
echo ""
echo "2. Colocar imágenes en ./input/"
echo ""
echo "3. Ejecutar el sistema:"
echo ""
echo "   # Activar entorno virtual"
echo "   source $VENV_DIR/bin/activate"
echo ""
echo "   # Procesar una imagen"
echo "   python auto_learning_system.py --mode single --input ./input/car.jpg"
echo ""
echo "   # Procesar todas las imágenes"
echo "   python auto_learning_system.py --mode single --input ./input"
echo ""
echo "   # Entrenamiento batch con múltiples épocas"
echo "   python auto_learning_system.py --mode batch --input ./input --epochs 5"
echo ""
echo "   # Ver estadísticas"
echo "   python auto_learning_system.py --mode stats"
echo ""
echo "=============================================================="
