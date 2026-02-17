#!/bin/bash
# =====================================================
# OKLA QA - Complete Setup Guide
# =====================================================
#
# Este script inicia todo el ambiente de QA y ejecuta
# el seeding completo de datos via API.
#
# Uso: ./setup-qa.sh [start|seed|stop|status|logs]
# =====================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

# Colores
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Configuración
COMPOSE_FILE="$ROOT_DIR/docker-compose.qa.yml"
API_URL="${API_URL:-http://localhost:18443}"

# =====================================================
# FUNCIONES DE AYUDA
# =====================================================

print_header() {
    echo ""
    echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║${NC}                  ${GREEN}OKLA QA Environment${NC}                        ${BLUE}║${NC}"
    echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

print_usage() {
    echo "Uso: $0 [comando]"
    echo ""
    echo "Comandos disponibles:"
    echo "  start      - Inicia todos los servicios de QA"
    echo "  seed       - Ejecuta el seeding de datos via API"
    echo "  stop       - Detiene todos los servicios"
    echo "  restart    - Reinicia todos los servicios"
    echo "  status     - Muestra el estado de los servicios"
    echo "  logs       - Muestra los logs de los servicios"
    echo "  clean      - Elimina volúmenes y reinicia todo"
    echo "  help       - Muestra esta ayuda"
    echo ""
}

check_docker() {
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}Error: Docker no está instalado${NC}"
        exit 1
    fi
    
    if ! docker info &> /dev/null; then
        echo -e "${RED}Error: Docker no está corriendo${NC}"
        exit 1
    fi
}

wait_for_services() {
    echo -e "${YELLOW}Esperando que los servicios estén listos...${NC}"
    
    local max_attempts=60
    local attempt=0
    
    # Esperar al Gateway
    echo -n "Gateway: "
    while [ $attempt -lt $max_attempts ]; do
        if curl -s "${API_URL}/health" > /dev/null 2>&1; then
            echo -e "${GREEN}✓${NC}"
            break
        fi
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    if [ $attempt -eq $max_attempts ]; then
        echo -e "${RED}✗ Timeout${NC}"
        return 1
    fi
    
    # Esperar servicios individuales
    local services=("auth" "users" "roles" "vehicles" "media" "notifications")
    
    for service in "${services[@]}"; do
        attempt=0
        echo -n "${service^}Service: "
        while [ $attempt -lt 30 ]; do
            if curl -s "${API_URL}/api/${service}/health" > /dev/null 2>&1 || \
               curl -s "http://localhost:18443/api/${service}/health" > /dev/null 2>&1; then
                echo -e "${GREEN}✓${NC}"
                break
            fi
            echo -n "."
            sleep 2
            ((attempt++))
        done
        
        if [ $attempt -eq 30 ]; then
            echo -e "${YELLOW}⚠ No responde (puede continuar)${NC}"
        fi
    done
    
    echo ""
    return 0
}

# =====================================================
# COMANDOS
# =====================================================

cmd_start() {
    print_header
    check_docker
    
    echo -e "${BLUE}=== Iniciando Ambiente QA ===${NC}"
    echo ""
    
    # Verificar si docker-compose existe
    if [ ! -f "$COMPOSE_FILE" ]; then
        echo -e "${RED}Error: No se encuentra $COMPOSE_FILE${NC}"
        echo "Ejecuta este script desde el directorio qa-environment/"
        exit 1
    fi
    
    # Iniciar servicios
    echo -e "${YELLOW}Levantando contenedores...${NC}"
    docker-compose -f "$COMPOSE_FILE" up -d
    
    echo ""
    wait_for_services
    
    echo -e "${GREEN}=== Ambiente QA Iniciado ===${NC}"
    echo ""
    echo "URLs disponibles:"
    echo "  - Frontend:     http://localhost:3000"
    echo "  - API Gateway:  http://localhost:18443"
    echo "  - Swagger:      http://localhost:18443/swagger"
    echo "  - RabbitMQ:     http://localhost:15672 (guest/guest)"
    echo ""
    echo -e "Ejecuta ${YELLOW}'$0 seed'${NC} para poblar la base de datos"
    echo ""
}

cmd_seed() {
    print_header
    
    echo -e "${BLUE}=== Ejecutando Seeding via API ===${NC}"
    echo ""
    
    # Verificar que el Gateway está respondiendo
    if ! curl -s "${API_URL}/health" > /dev/null 2>&1; then
        echo -e "${RED}Error: El Gateway no está respondiendo${NC}"
        echo "Ejecuta primero: $0 start"
        exit 1
    fi
    
    SEED_DIR="$SCRIPT_DIR"
    
    # Ejecutar scripts de seeding en orden
    echo -e "${BLUE}Paso 1/2: Creando usuarios${NC}"
    bash "$SEED_DIR/01-seed-users.sh"
    echo ""
    
    echo -e "${BLUE}Paso 2/2: Creando vehículos (incluye imágenes y publicación)${NC}"
    bash "$SEED_DIR/02-seed-vehicles.sh"
    echo ""
    
    echo -e "${GREEN}=== Seeding Completado ===${NC}"
    echo ""
    echo "Datos creados:"
    echo "  - 1 Usuario QA Principal (gregorymoreno.iem@gmail.com)"
    echo "  - 2 Administradores"
    echo "  - 3 Dealers"
    echo "  - 3 Vendedores"
    echo "  - 5 Compradores"
    echo "  - 15 Vehículos con 3 imágenes cada uno"
    echo ""
    echo "Ver credenciales en: seed-scripts/01-seed-users.sh"
    echo ""
}

cmd_stop() {
    print_header
    check_docker
    
    echo -e "${BLUE}=== Deteniendo Ambiente QA ===${NC}"
    docker-compose -f "$COMPOSE_FILE" down
    echo -e "${GREEN}✓ Servicios detenidos${NC}"
}

cmd_restart() {
    cmd_stop
    cmd_start
}

cmd_status() {
    print_header
    check_docker
    
    echo -e "${BLUE}=== Estado de Servicios ===${NC}"
    echo ""
    docker-compose -f "$COMPOSE_FILE" ps
    echo ""
    
    echo -e "${BLUE}=== Health Checks ===${NC}"
    echo ""
    
    echo -n "Gateway: "
    if curl -s "${API_URL}/health" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Healthy${NC}"
    else
        echo -e "${RED}✗ No responde${NC}"
    fi
    
    echo -n "Frontend: "
    if curl -s "http://localhost:3000" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Healthy${NC}"
    else
        echo -e "${RED}✗ No responde${NC}"
    fi
    
    echo -n "RabbitMQ: "
    if curl -s "http://localhost:15672" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Healthy${NC}"
    else
        echo -e "${RED}✗ No responde${NC}"
    fi
    
    echo ""
}

cmd_logs() {
    check_docker
    
    if [ -n "$2" ]; then
        docker-compose -f "$COMPOSE_FILE" logs -f "$2"
    else
        docker-compose -f "$COMPOSE_FILE" logs -f
    fi
}

cmd_clean() {
    print_header
    check_docker
    
    echo -e "${YELLOW}⚠ ADVERTENCIA: Esto eliminará TODOS los datos${NC}"
    read -p "¿Estás seguro? (y/N) " -n 1 -r
    echo ""
    
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo -e "${BLUE}=== Limpiando Ambiente QA ===${NC}"
        docker-compose -f "$COMPOSE_FILE" down -v
        echo -e "${GREEN}✓ Volúmenes eliminados${NC}"
        
        echo ""
        echo "Ejecuta '$0 start' para reiniciar el ambiente"
    else
        echo "Operación cancelada"
    fi
}

# =====================================================
# MAIN
# =====================================================

case "${1:-}" in
    start)
        cmd_start
        ;;
    seed)
        cmd_seed
        ;;
    stop)
        cmd_stop
        ;;
    restart)
        cmd_restart
        ;;
    status)
        cmd_status
        ;;
    logs)
        cmd_logs "$@"
        ;;
    clean)
        cmd_clean
        ;;
    help|--help|-h)
        print_header
        print_usage
        ;;
    *)
        print_header
        print_usage
        exit 1
        ;;
esac
