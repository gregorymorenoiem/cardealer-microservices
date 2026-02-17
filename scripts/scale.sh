#!/bin/bash
# ============================================================================
# scale.sh — Script de Escalado para OKLA Microservices
# ============================================================================
#
# Uso:
#   ./scripts/scale.sh local up          # Escalar local con réplicas HA
#   ./scripts/scale.sh local down        # Volver a single instance
#   ./scripts/scale.sh local scale 3     # Escalar servicios críticos a 3
#   ./scripts/scale.sh k8s status        # Ver estado de HPAs en K8s
#   ./scripts/scale.sh k8s scale-up      # Escalar servicios críticos en K8s
#   ./scripts/scale.sh k8s scale-down    # Reducir a mínimos en K8s
#
# ============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Servicios por tier
CRITICAL_SERVICES=("gateway" "authservice" "vehiclessaleservice" "mediaservice" "billingservice")
IMPORTANT_SERVICES=("userservice" "roleservice" "notificationservice" "contactservice" "adminservice" "kycservice")
INTERNAL_SERVICES=("errorservice" "auditservice" "idempotencyservice")

log() { echo -e "${GREEN}[SCALE]${NC} $1"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
error() { echo -e "${RED}[ERROR]${NC} $1"; }

# ──────────────────────────────────────────────────────────────────────
# LOCAL (Docker Compose)
# ──────────────────────────────────────────────────────────────────────

local_up() {
    log "🚀 Escalando servicios críticos a 2 réplicas..."
    cd "$ROOT_DIR"
    
    local scale_args=""
    for svc in "${CRITICAL_SERVICES[@]}"; do
        scale_args="$scale_args --scale $svc=2"
    done
    
    docker compose -f compose.yaml -f compose.scaling.yaml up -d $scale_args
    
    log "✅ Servicios críticos escalados a 2 réplicas"
    docker compose -f compose.yaml -f compose.scaling.yaml ps
}

local_down() {
    log "📉 Reduciendo todos los servicios a 1 réplica..."
    cd "$ROOT_DIR"
    
    local scale_args=""
    for svc in "${CRITICAL_SERVICES[@]}" "${IMPORTANT_SERVICES[@]}" "${INTERNAL_SERVICES[@]}"; do
        scale_args="$scale_args --scale $svc=1"
    done
    
    docker compose -f compose.yaml -f compose.scaling.yaml up -d $scale_args
    
    log "✅ Todos los servicios reducidos a 1 réplica"
}

local_scale() {
    local count="${1:-2}"
    log "⚡ Escalando servicios críticos a $count réplicas..."
    cd "$ROOT_DIR"
    
    local scale_args=""
    for svc in "${CRITICAL_SERVICES[@]}"; do
        scale_args="$scale_args --scale $svc=$count"
    done
    
    docker compose -f compose.yaml -f compose.scaling.yaml up -d $scale_args
    
    log "✅ Servicios críticos escalados a $count réplicas"
    docker compose -f compose.yaml -f compose.scaling.yaml ps
}

local_status() {
    cd "$ROOT_DIR"
    echo ""
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  ESTADO DE RÉPLICAS (Docker Compose)${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""
    
    docker compose -f compose.yaml ps --format "table {{.Name}}\t{{.State}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || \
    docker compose -f compose.yaml ps
}

# ──────────────────────────────────────────────────────────────────────
# KUBERNETES (DOKS)
# ──────────────────────────────────────────────────────────────────────

k8s_status() {
    echo ""
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  ESTADO DE AUTO-SCALING (Kubernetes)${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""
    
    echo -e "${GREEN}📊 HPAs:${NC}"
    kubectl get hpa -n okla -o wide 2>/dev/null || warn "No se puede conectar al cluster"
    
    echo ""
    echo -e "${GREEN}📊 Pods por servicio:${NC}"
    kubectl get pods -n okla -o wide --sort-by=.metadata.labels.app 2>/dev/null || true
    
    echo ""
    echo -e "${GREEN}📊 PDBs:${NC}"
    kubectl get pdb -n okla 2>/dev/null || true
    
    echo ""
    echo -e "${GREEN}📊 Resource Quota:${NC}"
    kubectl get resourcequota -n okla -o yaml 2>/dev/null | grep -A 20 "status:" || true
}

k8s_scale_up() {
    log "🚀 Escalando servicios críticos en Kubernetes..."
    
    for svc in "${CRITICAL_SERVICES[@]}"; do
        log "  → $svc: scaling to 3 replicas"
        kubectl scale deployment "$svc" --replicas=3 -n okla 2>/dev/null || warn "No se encontró $svc"
    done
    
    for svc in "${IMPORTANT_SERVICES[@]}"; do
        log "  → $svc: scaling to 2 replicas"
        kubectl scale deployment "$svc" --replicas=2 -n okla 2>/dev/null || warn "No se encontró $svc"
    done
    
    log "✅ Escalado completado. Los HPAs ajustarán automáticamente."
    kubectl get hpa -n okla 2>/dev/null
}

k8s_scale_down() {
    log "📉 Reduciendo servicios en Kubernetes a mínimos..."
    
    for svc in "${CRITICAL_SERVICES[@]}"; do
        log "  → $svc: scaling to 2 replicas (minimum HA)"
        kubectl scale deployment "$svc" --replicas=2 -n okla 2>/dev/null || true
    done
    
    for svc in "${IMPORTANT_SERVICES[@]}" "${INTERNAL_SERVICES[@]}"; do
        log "  → $svc: scaling to 1 replica"
        kubectl scale deployment "$svc" --replicas=1 -n okla 2>/dev/null || true
    done
    
    log "✅ Reducido a mínimos. Los HPAs mantendrán los minReplicas."
}

k8s_install_keda() {
    log "📦 Instalando KEDA en el cluster..."
    
    helm repo add kedacore https://kedacore.github.io/charts 2>/dev/null || true
    helm repo update
    
    helm upgrade --install keda kedacore/keda \
        --namespace keda \
        --create-namespace \
        --set watchNamespace="okla" \
        --wait
    
    log "✅ KEDA instalado. Aplicando ScaledObjects..."
    kubectl apply -f "$ROOT_DIR/k8s/keda.yaml"
    
    log "✅ KEDA configurado para auto-scaling basado en RabbitMQ"
}

# ──────────────────────────────────────────────────────────────────────
# MAIN
# ──────────────────────────────────────────────────────────────────────

main() {
    local env="${1:-help}"
    local action="${2:-status}"
    local arg="${3:-}"
    
    case "$env" in
        local)
            case "$action" in
                up)       local_up ;;
                down)     local_down ;;
                scale)    local_scale "$arg" ;;
                status)   local_status ;;
                *)        error "Acción desconocida: $action" ;;
            esac
            ;;
        k8s|kubernetes)
            case "$action" in
                status)     k8s_status ;;
                scale-up)   k8s_scale_up ;;
                scale-down) k8s_scale_down ;;
                install-keda) k8s_install_keda ;;
                *)          error "Acción desconocida: $action" ;;
            esac
            ;;
        help|*)
            echo ""
            echo "Uso: $0 <environment> <action> [args]"
            echo ""
            echo "Environments:"
            echo "  local       Docker Compose (desarrollo)"
            echo "  k8s         Kubernetes (DOKS producción)"
            echo ""
            echo "Acciones local:"
            echo "  up          Escalar críticos a 2 réplicas"
            echo "  down        Reducir todo a 1 réplica"
            echo "  scale <n>   Escalar críticos a N réplicas"
            echo "  status      Ver estado actual"
            echo ""
            echo "Acciones k8s:"
            echo "  status       Ver HPAs, pods, PDBs"
            echo "  scale-up     Escalar críticos a 3, importantes a 2"
            echo "  scale-down   Reducir a mínimos HA"
            echo "  install-keda Instalar KEDA para event-driven scaling"
            echo ""
            ;;
    esac
}

main "$@"
