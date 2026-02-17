#!/bin/bash
# ============================================================
# Docker Build & Run Script - Auto-Learning System
# ============================================================

set -e

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🚀 Auto-Learning Background Removal System - Docker${NC}"
echo "============================================================"

# Directorio del script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Crear directorios necesarios
echo -e "${YELLOW}📁 Creando directorios...${NC}"
mkdir -p input output_autolearn models checkpoints data

# Verificar que hay imágenes en input
if [ -z "$(ls -A input 2>/dev/null)" ]; then
    echo -e "${RED}⚠️  No hay imágenes en ./input${NC}"
    echo "   Coloca imágenes de vehículos en la carpeta 'input' antes de ejecutar."
    echo ""
fi

# Verificar modelo SAM
if [ ! -f "sam_vit_h_4b8939.pth" ]; then
    echo -e "${YELLOW}⚠️  No se encontró sam_vit_h_4b8939.pth${NC}"
    echo "   El modelo se descargará automáticamente (~2.5GB)"
fi

# Mostrar uso
usage() {
    echo ""
    echo -e "${GREEN}USO:${NC}"
    echo "  $0 build          - Construir imagen Docker"
    echo "  $0 run            - Procesar imágenes en ./input (batch)"
    echo "  $0 single FILE    - Procesar una imagen específica"
    echo "  $0 continuous     - Modo continuo (watch folder)"
    echo "  $0 stats          - Ver estadísticas de aprendizaje"
    echo "  $0 shell          - Abrir shell en el contenedor"
    echo "  $0 logs           - Ver logs del último run"
    echo "  $0 clean          - Limpiar imágenes y contenedores"
    echo ""
    echo -e "${GREEN}EJEMPLOS:${NC}"
    echo "  $0 build && $0 run"
    echo "  $0 single ./input/car.jpg"
    echo ""
}

case "${1:-help}" in
    build)
        echo -e "${BLUE}🔨 Construyendo imagen Docker...${NC}"
        docker build -t autolearn-system:latest .
        echo -e "${GREEN}✅ Imagen construida exitosamente${NC}"
        ;;
    
    run|batch)
        echo -e "${BLUE}🖼️  Procesando imágenes en ./input (modo batch)...${NC}"
        docker run --rm \
            -v "$SCRIPT_DIR/input:/app/input:ro" \
            -v "$SCRIPT_DIR/output_autolearn:/app/output:rw" \
            -v "$SCRIPT_DIR/models:/app/models:rw" \
            -v "$SCRIPT_DIR/checkpoints:/app/checkpoints:rw" \
            -v "$SCRIPT_DIR/data:/app/data:rw" \
            ${SAM_MOUNT:-} \
            -e "OLLAMA_HOST=${OLLAMA_HOST:-host.docker.internal:11434}" \
            --add-host=host.docker.internal:host-gateway \
            autolearn-system:latest \
            --mode batch --input /app/input --output /app/output
        echo -e "${GREEN}✅ Procesamiento completado. Resultados en ./output_autolearn${NC}"
        ;;
    
    single)
        if [ -z "$2" ]; then
            echo -e "${RED}❌ Especifica la imagen a procesar${NC}"
            echo "   Ejemplo: $0 single ./input/car.jpg"
            exit 1
        fi
        FILENAME=$(basename "$2")
        echo -e "${BLUE}🖼️  Procesando: $FILENAME${NC}"
        docker run --rm \
            -v "$SCRIPT_DIR/input:/app/input:ro" \
            -v "$SCRIPT_DIR/output_autolearn:/app/output:rw" \
            -v "$SCRIPT_DIR/models:/app/models:rw" \
            -v "$SCRIPT_DIR/checkpoints:/app/checkpoints:rw" \
            -v "$SCRIPT_DIR/data:/app/data:rw" \
            -e "OLLAMA_HOST=${OLLAMA_HOST:-host.docker.internal:11434}" \
            --add-host=host.docker.internal:host-gateway \
            autolearn-system:latest \
            --mode single --input "/app/input/$FILENAME" --output /app/output
        ;;
    
    continuous)
        echo -e "${BLUE}👁️  Modo continuo - Monitoreando ./input...${NC}"
        echo "   Presiona Ctrl+C para detener"
        docker run --rm -it \
            -v "$SCRIPT_DIR/input:/app/input:ro" \
            -v "$SCRIPT_DIR/output_autolearn:/app/output:rw" \
            -v "$SCRIPT_DIR/models:/app/models:rw" \
            -v "$SCRIPT_DIR/checkpoints:/app/checkpoints:rw" \
            -v "$SCRIPT_DIR/data:/app/data:rw" \
            -e "OLLAMA_HOST=${OLLAMA_HOST:-host.docker.internal:11434}" \
            --add-host=host.docker.internal:host-gateway \
            autolearn-system:latest \
            --mode continuous --input /app/input --output /app/output
        ;;
    
    stats)
        echo -e "${BLUE}📊 Estadísticas de aprendizaje...${NC}"
        docker run --rm \
            -v "$SCRIPT_DIR/data:/app/data:ro" \
            autolearn-system:latest \
            --mode stats
        ;;
    
    shell)
        echo -e "${BLUE}🐚 Abriendo shell en el contenedor...${NC}"
        docker run --rm -it \
            -v "$SCRIPT_DIR/input:/app/input:ro" \
            -v "$SCRIPT_DIR/output_autolearn:/app/output:rw" \
            -v "$SCRIPT_DIR/models:/app/models:rw" \
            -v "$SCRIPT_DIR/checkpoints:/app/checkpoints:rw" \
            -v "$SCRIPT_DIR/data:/app/data:rw" \
            --entrypoint /bin/bash \
            autolearn-system:latest
        ;;
    
    logs)
        docker logs autolearn-system 2>&1 | tail -100
        ;;
    
    clean)
        echo -e "${YELLOW}🧹 Limpiando...${NC}"
        docker rm -f autolearn-system 2>/dev/null || true
        docker rmi autolearn-system:latest 2>/dev/null || true
        echo -e "${GREEN}✅ Limpieza completada${NC}"
        ;;
    
    help|--help|-h|*)
        usage
        ;;
esac
