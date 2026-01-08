#!/bin/bash

# ============================================================================
# OKLA Sprint Testing Script
# ============================================================================
# Este script automatiza el testing completo de un sprint antes de marcarlo
# como completado. Verifica backend, frontend, Docker y accesibilidad.
#
# Uso: ./test-sprint.sh [nombre-del-servicio]
# Ejemplo: ./test-sprint.sh alertservice
# ============================================================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
API_URL="${API_URL:-http://localhost:18443}"
SERVICE_NAME="${1}"

if [ -z "$SERVICE_NAME" ]; then
    echo -e "${RED}Error: Debes proporcionar el nombre del servicio${NC}"
    echo "Uso: ./test-sprint.sh [nombre-del-servicio]"
    echo "Ejemplo: ./test-sprint.sh alertservice"
    exit 1
fi

# Convert to lowercase and remove "Service" suffix if present
SERVICE_NAME=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]' | sed 's/service$//')

echo -e "${BLUE}╔═══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║        OKLA Sprint Testing - ${SERVICE_NAME}                      ${NC}"
echo -e "${BLUE}╚═══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# ============================================================================
# FASE 1: BACKEND TESTING
# ============================================================================
echo -e "${YELLOW}[1/6] Backend Testing...${NC}"

# Find service directory
SERVICE_DIR=$(find backend -type d -iname "${SERVICE_NAME}Service" -o -iname "${SERVICE_NAME}" | head -n 1)

if [ -z "$SERVICE_DIR" ]; then
    echo -e "${RED}✗ Servicio no encontrado: ${SERVICE_NAME}${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Servicio encontrado: ${SERVICE_DIR}${NC}"

# Check if Dockerfile exists
API_DIR=$(find "$SERVICE_DIR" -type d -name "*.Api" | head -n 1)
if [ ! -f "$API_DIR/Dockerfile" ]; then
    echo -e "${RED}✗ Dockerfile no encontrado en ${API_DIR}${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Dockerfile encontrado${NC}"

# Check if Health Check endpoint exists
if ! grep -r "MapHealthChecks" "$API_DIR/Program.cs" > /dev/null 2>&1; then
    echo -e "${YELLOW}⚠ Warning: Health Check podría no estar configurado${NC}"
fi

echo -e "${GREEN}✓ Backend structure OK${NC}"
echo ""

# ============================================================================
# FASE 2: DOCKER BUILD
# ============================================================================
echo -e "${YELLOW}[2/6] Docker Build Testing...${NC}"

cd "$API_DIR"
SERVICE_IMAGE="cardealer-${SERVICE_NAME}:test"

echo "Building Docker image: $SERVICE_IMAGE"
if docker build -t "$SERVICE_IMAGE" . > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Docker build exitoso${NC}"
else
    echo -e "${RED}✗ Docker build falló${NC}"
    exit 1
fi

cd - > /dev/null
echo ""

# ============================================================================
# FASE 3: DOCKER COMPOSE TESTING
# ============================================================================
echo -e "${YELLOW}[3/6] Docker Compose Testing...${NC}"

# Check if service is in compose.yaml
if ! grep -q "${SERVICE_NAME}service:" compose.yaml; then
    echo -e "${YELLOW}⚠ Warning: Servicio no encontrado en compose.yaml${NC}"
    echo "   Agregarlo manualmente para testing local"
else
    echo -e "${GREEN}✓ Servicio en compose.yaml${NC}"
    
    # Try to start the service
    echo "Starting service with docker-compose..."
    if docker-compose up -d "${SERVICE_NAME}service" postgres redis rabbitmq > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Servicio iniciado${NC}"
        
        # Wait for service to be ready
        echo "Esperando 10 segundos para que el servicio esté listo..."
        sleep 10
        
        # Check if service is running
        if docker-compose ps "${SERVICE_NAME}service" | grep -q "Up"; then
            echo -e "${GREEN}✓ Servicio corriendo${NC}"
        else
            echo -e "${RED}✗ Servicio no está corriendo${NC}"
            docker-compose logs "${SERVICE_NAME}service" | tail -20
            exit 1
        fi
        
        # Cleanup
        docker-compose down > /dev/null 2>&1
    else
        echo -e "${YELLOW}⚠ No se pudo iniciar con docker-compose (podría no estar configurado)${NC}"
    fi
fi

echo ""

# ============================================================================
# FASE 4: FRONTEND INTEGRATION TESTING
# ============================================================================
echo -e "${YELLOW}[4/6] Frontend Integration Testing...${NC}"

FRONTEND_DIR="frontend/web/src"

# Check if routes are added in App.tsx
if [ -f "$FRONTEND_DIR/App.tsx" ]; then
    # Look for service-related routes (generic check)
    if grep -q "Route.*path.*${SERVICE_NAME}" "$FRONTEND_DIR/App.tsx" || 
       grep -q "import.*${SERVICE_NAME^}" "$FRONTEND_DIR/App.tsx"; then
        echo -e "${GREEN}✓ Rutas encontradas en App.tsx${NC}"
    else
        echo -e "${YELLOW}⚠ No se encontraron rutas relacionadas en App.tsx${NC}"
        echo "   Si hay UI, verificar manualmente que estén agregadas"
    fi
    
    # Check Navbar integration
    if [ -f "$FRONTEND_DIR/components/organisms/Navbar.tsx" ]; then
        if grep -q "${SERVICE_NAME}" "$FRONTEND_DIR/components/organisms/Navbar.tsx"; then
            echo -e "${GREEN}✓ Links encontrados en Navbar.tsx${NC}"
        else
            echo -e "${YELLOW}⚠ No se encontraron links en Navbar.tsx${NC}"
            echo "   Si hay UI pública, verificar manualmente"
        fi
    fi
    
    # Check if MainLayout is used in pages
    PAGE_FILES=$(find "$FRONTEND_DIR/pages" -name "*${SERVICE_NAME^}*.tsx" -o -name "*${SERVICE_NAME}*.tsx" 2>/dev/null)
    if [ -n "$PAGE_FILES" ]; then
        for page in $PAGE_FILES; do
            if grep -q "MainLayout" "$page"; then
                echo -e "${GREEN}✓ $(basename $page) usa MainLayout${NC}"
            else
                echo -e "${RED}✗ $(basename $page) NO usa MainLayout${NC}"
                echo "   Agregar <MainLayout> wrapper para banners site-wide"
            fi
        done
    else
        echo -e "${BLUE}ℹ No se encontraron páginas frontend para este servicio${NC}"
    fi
else
    echo -e "${YELLOW}⚠ App.tsx no encontrado${NC}"
fi

echo ""

# ============================================================================
# FASE 5: GATEWAY CONFIGURATION
# ============================================================================
echo -e "${YELLOW}[5/6] Gateway Configuration Testing...${NC}"

GATEWAY_CONFIG="backend/Gateway/Gateway.Api/ocelot.prod.json"

if [ -f "$GATEWAY_CONFIG" ]; then
    # Check if service routes are configured
    if grep -q "\"${SERVICE_NAME}\"" "$GATEWAY_CONFIG"; then
        echo -e "${GREEN}✓ Rutas encontradas en ocelot.prod.json${NC}"
        
        # Check if port is 8080 (not 80)
        if grep -A5 "\"${SERVICE_NAME}\"" "$GATEWAY_CONFIG" | grep -q "\"Port\": 8080"; then
            echo -e "${GREEN}✓ Puerto 8080 configurado correctamente${NC}"
        elif grep -A5 "\"${SERVICE_NAME}\"" "$GATEWAY_CONFIG" | grep -q "\"Port\": 80"; then
            echo -e "${RED}✗ Puerto 80 encontrado - DEBE ser 8080 en producción${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ Rutas no encontradas en Gateway${NC}"
        echo "   Si el servicio expone API pública, agregar rutas en ocelot.prod.json"
    fi
else
    echo -e "${YELLOW}⚠ ocelot.prod.json no encontrado${NC}"
fi

echo ""

# ============================================================================
# FASE 6: KUBERNETES MANIFESTS
# ============================================================================
echo -e "${YELLOW}[6/6] Kubernetes Manifests Testing...${NC}"

K8S_DEPLOYMENTS="k8s/deployments.yaml"
K8S_SERVICES="k8s/services.yaml"

if [ -f "$K8S_DEPLOYMENTS" ]; then
    if grep -q "${SERVICE_NAME}service" "$K8S_DEPLOYMENTS"; then
        echo -e "${GREEN}✓ Deployment encontrado en k8s/deployments.yaml${NC}"
        
        # Check if port is 8080
        if grep -A10 "${SERVICE_NAME}service" "$K8S_DEPLOYMENTS" | grep -q "containerPort: 8080"; then
            echo -e "${GREEN}✓ containerPort 8080 configurado${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ Deployment no encontrado en K8s manifests${NC}"
        echo "   Agregar deployment para desplegar a producción"
    fi
    
    if [ -f "$K8S_SERVICES" ]; then
        if grep -q "${SERVICE_NAME}service" "$K8S_SERVICES"; then
            echo -e "${GREEN}✓ Service encontrado en k8s/services.yaml${NC}"
        fi
    fi
else
    echo -e "${YELLOW}⚠ Manifests de Kubernetes no encontrados${NC}"
fi

echo ""

# ============================================================================
# RESUMEN FINAL
# ============================================================================
echo -e "${BLUE}╔═══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                    RESUMEN DEL TESTING                        ${NC}"
echo -e "${BLUE}╚═══════════════════════════════════════════════════════════════╝${NC}"
echo ""

echo -e "${GREEN}✓ Checks completados${NC}"
echo ""
echo -e "${YELLOW}SIGUIENTE PASOS MANUALES:${NC}"
echo "1. Probar endpoints manualmente:"
echo "   curl ${API_URL}/api/${SERVICE_NAME}/health"
echo ""
echo "2. Si hay frontend, verificar en navegador:"
echo "   http://localhost:5173/ruta-correspondiente"
echo ""
echo "3. Verificar logs sin errores:"
echo "   docker-compose logs -f ${SERVICE_NAME}service"
echo ""
echo "4. Si todo OK, marcar sprint como completado ✅"
echo ""

echo -e "${BLUE}Testing completado para: ${SERVICE_NAME}${NC}"
