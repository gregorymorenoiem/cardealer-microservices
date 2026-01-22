#!/bin/bash

# ════════════════════════════════════════════════════════════════════════════
# OKLA Frontend-Only Compose Manager
# ════════════════════════════════════════════════════════════════════════════
#
# Script para gestionar el compose.frontend-only.yaml de forma fácil
#
# Uso:
#   ./compose-frontend.sh up          # Levantar servicios
#   ./compose-frontend.sh down        # Detener servicios
#   ./compose-frontend.sh logs        # Ver logs
#   ./compose-frontend.sh status      # Ver estado
#   ./compose-frontend.sh clean       # Limpiar todo
#
# ════════════════════════════════════════════════════════════════════════════

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
COMPOSE_FILE="compose.frontend-only.yaml"
SERVICES=("postgres_db" "rabbitmq" "redis" "consul" "authservice" "vehiclessaleservice" "mediaservice" "userservice" "contactservice" "notificationservice" "adminservice" "gateway")

# ════════════════════════════════════════════════════════════════════════════
# Functions
# ════════════════════════════════════════════════════════════════════════════

print_header() {
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${CYAN}ℹ $1${NC}"
}

# Check if compose file exists
check_compose_file() {
    if [ ! -f "$COMPOSE_FILE" ]; then
        print_error "Archivo $COMPOSE_FILE no encontrado en el directorio actual"
        exit 1
    fi
}

# Check if Docker is running
check_docker() {
    if ! docker ps > /dev/null 2>&1; then
        print_error "Docker no está corriendo. Inicia Docker Desktop."
        exit 1
    fi
    print_success "Docker está corriendo"
}

# Up - Start services
cmd_up() {
    print_header "🚀 INICIANDO SERVICIOS FRONTEND-ONLY"
    
    check_docker
    check_compose_file
    
    print_info "Levantando ${#SERVICES[@]} servicios..."
    docker-compose -f "$COMPOSE_FILE" up -d
    
    print_success "Containers iniciados"
    
    print_info "Esperando health checks (esto puede tomar ~30 segundos)..."
    sleep 3
    
    # Wait for healthy status
    local retry=0
    local max_retries=20
    while [ $retry -lt $max_retries ]; do
        local healthy_count=$(docker-compose -f "$COMPOSE_FILE" ps | grep -c "healthy" || true)
        print_info "Servicios healthy: $healthy_count/${#SERVICES[@]}"
        
        if [ "$healthy_count" -ge 8 ]; then
            break
        fi
        
        sleep 2
        retry=$((retry + 1))
    done
    
    # Final status
    echo ""
    cmd_status
    
    echo ""
    print_success "🎉 Servicios listos!"
    echo ""
    print_info "API Gateway: http://localhost:18443"
    print_info "RabbitMQ UI: http://localhost:15672 (guest/guest)"
    print_info "Consul: http://localhost:8500/ui/"
    print_info "Frontend: npm run dev"
}

# Down - Stop services
cmd_down() {
    print_header "🛑 DETENIENDO SERVICIOS"
    
    check_compose_file
    
    docker-compose -f "$COMPOSE_FILE" down
    
    print_success "Servicios detenidos"
}

# Status - Show service status
cmd_status() {
    print_header "📊 ESTADO DE SERVICIOS"
    
    check_compose_file
    
    docker-compose -f "$COMPOSE_FILE" ps
}

# Logs - Show logs
cmd_logs() {
    print_header "📋 LOGS"
    
    check_compose_file
    
    local service=$1
    
    if [ -z "$service" ]; then
        print_info "Mostrando logs de todos los servicios (Ctrl+C para salir)"
        docker-compose -f "$COMPOSE_FILE" logs -f --tail=50
    else
        if [[ ! " ${SERVICES[@]} " =~ " ${service} " ]]; then
            print_error "Servicio '$service' no existe"
            print_info "Servicios disponibles: ${SERVICES[@]}"
            return 1
        fi
        print_info "Mostrando logs de $service (Ctrl+C para salir)"
        docker-compose -f "$COMPOSE_FILE" logs -f "$service"
    fi
}

# Health - Check health status
cmd_health() {
    print_header "🏥 HEALTH CHECKS"
    
    check_compose_file
    
    echo ""
    echo "PostgreSQL:"
    docker-compose -f "$COMPOSE_FILE" exec -T postgres_db pg_isready -U postgres 2>/dev/null && print_success "Healthy" || print_error "Down"
    
    echo ""
    echo "RabbitMQ:"
    docker-compose -f "$COMPOSE_FILE" exec -T rabbitmq rabbitmq-diagnostics -q ping 2>/dev/null && print_success "Healthy" || print_error "Down"
    
    echo ""
    echo "Redis:"
    docker-compose -f "$COMPOSE_FILE" exec -T redis redis-cli ping 2>/dev/null && print_success "Healthy" || print_error "Down"
    
    echo ""
    echo "Gateway:"
    curl -s http://localhost:18443/health > /dev/null && print_success "Healthy" || print_error "Down"
    
    echo ""
    echo "AuthService:"
    curl -s http://localhost:15001/health > /dev/null && print_success "Healthy" || print_error "Down"
}

# Restart - Restart services
cmd_restart() {
    print_header "🔄 REINICIANDO SERVICIOS"
    
    check_compose_file
    
    local service=$1
    
    if [ -z "$service" ]; then
        print_info "Reiniciando todos los servicios..."
        docker-compose -f "$COMPOSE_FILE" restart
    else
        if [[ ! " ${SERVICES[@]} " =~ " ${service} " ]]; then
            print_error "Servicio '$service' no existe"
            print_info "Servicios disponibles: ${SERVICES[@]}"
            return 1
        fi
        print_info "Reiniciando $service..."
        docker-compose -f "$COMPOSE_FILE" restart "$service"
    fi
    
    print_success "Reinicio completado"
}

# Stop - Stop a service
cmd_stop() {
    print_header "⏸️  DETENIENDO SERVICIO"
    
    check_compose_file
    
    local service=$1
    
    if [ -z "$service" ]; then
        print_error "Especifica un servicio"
        print_info "Servicios disponibles: ${SERVICES[@]}"
        return 1
    fi
    
    if [[ ! " ${SERVICES[@]} " =~ " ${service} " ]]; then
        print_error "Servicio '$service' no existe"
        print_info "Servicios disponibles: ${SERVICES[@]}"
        return 1
    fi
    
    docker-compose -f "$COMPOSE_FILE" stop "$service"
    print_success "$service detenido"
}

# Clean - Remove all containers and volumes
cmd_clean() {
    print_header "🧹 LIMPIANDO EVERYTHING"
    print_warning "Esto eliminará TODOS los containers y volúmenes (DATOS SE PIERDEN)"
    
    read -p "¿Continuar? (y/n): " -n 1 -r
    echo
    
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        check_compose_file
        docker-compose -f "$COMPOSE_FILE" down -v
        print_success "Limpieza completada"
    else
        print_info "Operación cancelada"
    fi
}

# Build - Rebuild images
cmd_build() {
    print_header "🔨 RECOMPILANDO IMÁGENES"
    
    check_docker
    check_compose_file
    
    print_warning "Esto puede tomar varios minutos..."
    
    docker-compose -f "$COMPOSE_FILE" build --no-cache
    
    print_success "Recompilación completada"
}

# Shell - Enter container shell
cmd_shell() {
    print_header "🐚 ENTRANDO A SHELL"
    
    check_compose_file
    
    local service=$1
    
    if [ -z "$service" ]; then
        print_error "Especifica un servicio"
        print_info "Servicios disponibles: ${SERVICES[@]}"
        return 1
    fi
    
    if [[ ! " ${SERVICES[@]} " =~ " ${service} " ]]; then
        print_error "Servicio '$service' no existe"
        print_info "Servicios disponibles: ${SERVICES[@]}"
        return 1
    fi
    
    docker-compose -f "$COMPOSE_FILE" exec "$service" /bin/bash
}

# Ports - Show port mappings
cmd_ports() {
    print_header "🔌 MAPEO DE PUERTOS"
    
    echo ""
    echo "Infraestructura:"
    echo "  PostgreSQL:        localhost:5433"
    echo "  RabbitMQ:          localhost:5672  (AMQP)"
    echo "  RabbitMQ UI:       localhost:15672 (Management)"
    echo "  Redis:             localhost:6379"
    echo "  Consul:            localhost:8500"
    echo ""
    echo "Servicios:"
    echo "  AuthService:       localhost:15001 (Swagger: /swagger)"
    echo "  VehiclesSale:      localhost:15010 (Swagger: /swagger)"
    echo "  MediaService:      localhost:15020 (Swagger: /swagger)"
    echo "  UserService:       localhost:15002 (Swagger: /swagger)"
    echo "  ContactService:    localhost:15003 (Swagger: /swagger)"
    echo "  NotificationSvc:   localhost:15005 (Swagger: /swagger)"
    echo "  AdminService:      localhost:15007 (Swagger: /swagger)"
    echo ""
    echo "Gateway:"
    echo "  API Gateway:       localhost:18443 (Ocelot router)"
}

# Help - Show help
cmd_help() {
    cat << EOF

${BLUE}════════════════════════════════════════════════════════════${NC}
${BLUE}OKLA Frontend-Only Compose Manager${NC}
${BLUE}════════════════════════════════════════════════════════════${NC}

${CYAN}Comandos:${NC}
  ${GREEN}up${NC}              Levantar todos los servicios
  ${GREEN}down${NC}            Detener todos los servicios
  ${GREEN}status${NC}          Ver estado de los servicios
  ${GREEN}logs${NC} [service]  Ver logs (Ctrl+C para salir)
  ${GREEN}health${NC}          Verificar health checks
  ${GREEN}restart${NC} [srv]   Reiniciar servicios
  ${GREEN}stop${NC} [service]  Detener un servicio específico
  ${GREEN}build${NC}           Recompilar imágenes Docker
  ${GREEN}shell${NC} [service] Entrar a shell del container
  ${GREEN}ports${NC}           Ver mapeo de puertos
  ${GREEN}clean${NC}           Eliminar containers y volúmenes
  ${GREEN}help${NC}            Mostrar esta ayuda

${CYAN}Ejemplos:${NC}
  ./compose-frontend.sh up
  ./compose-frontend.sh logs gateway
  ./compose-frontend.sh restart authservice
  ./compose-frontend.sh shell postgres_db
  ./compose-frontend.sh clean

${CYAN}Servicios disponibles:${NC}
  ${SERVICES[@]}

${BLUE}════════════════════════════════════════════════════════════${NC}

EOF
}

# ════════════════════════════════════════════════════════════════════════════
# Main
# ════════════════════════════════════════════════════════════════════════════

if [ $# -eq 0 ]; then
    cmd_help
    exit 0
fi

case "$1" in
    up)
        cmd_up
        ;;
    down)
        cmd_down
        ;;
    status)
        cmd_status
        ;;
    logs)
        cmd_logs "$2"
        ;;
    health)
        cmd_health
        ;;
    restart)
        cmd_restart "$2"
        ;;
    stop)
        cmd_stop "$2"
        ;;
    build)
        cmd_build
        ;;
    shell)
        cmd_shell "$2"
        ;;
    ports)
        cmd_ports
        ;;
    clean)
        cmd_clean
        ;;
    help)
        cmd_help
        ;;
    *)
        print_error "Comando desconocido: $1"
        cmd_help
        exit 1
        ;;
esac
