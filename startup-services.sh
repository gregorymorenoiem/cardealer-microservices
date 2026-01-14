#!/bin/bash

# 🚀 Script para levantar servicios OKLA gradualmente según Sprint Plan
# Uso: chmod +x startup-services.sh && ./startup-services.sh

set -e

cd "$(dirname "$0")"
WORKSPACE=$(pwd)

echo "════════════════════════════════════════════════════════════════════════════"
echo "🚀 INICIANDO PLATAFORMA OKLA - STARTUP GRADUAL"
echo "════════════════════════════════════════════════════════════════════════════"

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para esperar a que un servicio esté healthy
wait_for_service() {
    local service=$1
    local max_attempts=30
    local attempt=0
    
    echo -e "${BLUE}⏳ Esperando a que $service esté listo...${NC}"
    
    while [ $attempt -lt $max_attempts ]; do
        if docker compose ps $service 2>/dev/null | grep -q "healthy\|Up"; then
            echo -e "${GREEN}✅ $service está listo${NC}"
            return 0
        fi
        attempt=$((attempt + 1))
        echo -n "."
        sleep 2
    done
    
    echo -e "${YELLOW}⚠️ $service tardó más de lo esperado${NC}"
    return 0
}

# FASE 1: Infraestructura Base
echo ""
echo -e "${BLUE}════ FASE 1: INFRAESTRUCTURA BASE ════${NC}"
echo ""

echo -e "${YELLOW}1️⃣ Levantando postgres_db...${NC}"
docker compose up -d postgres_db
wait_for_service "postgres_db"

echo -e "${YELLOW}2️⃣ Levantando redis...${NC}"
docker compose up -d redis
wait_for_service "redis"

echo -e "${YELLOW}3️⃣ Levantando rabbitmq...${NC}"
docker compose up -d rabbitmq
wait_for_service "rabbitmq"

sleep 3
echo -e "${GREEN}✅ FASE 1 COMPLETADA: Infraestructura base lista${NC}\n"

# FASE 2: Servicios de Autenticación y Autorización
echo ""
echo -e "${BLUE}════ FASE 2: AUTH & ROLES (Sprint 1) ════${NC}"
echo ""

echo -e "${YELLOW}4️⃣ Levantando authservice...${NC}"
docker compose up -d authservice
wait_for_service "authservice"

echo -e "${YELLOW}5️⃣ Levantando roleservice...${NC}"
docker compose up -d roleservice
wait_for_service "roleservice"

sleep 2
echo -e "${GREEN}✅ FASE 2 COMPLETADA: Auth & Roles listos${NC}\n"

# FASE 3: Servicios Principales MVP (Sprint 1)
echo ""
echo -e "${BLUE}════ FASE 3: MVP MARKETPLACE (Sprint 1) ════${NC}"
echo ""

echo -e "${YELLOW}6️⃣ Levantando vehiclessaleservice...${NC}"
docker compose up -d vehiclessaleservice
wait_for_service "vehiclessaleservice"

echo -e "${YELLOW}7️⃣ Levantando mediaservice...${NC}"
docker compose up -d mediaservice
wait_for_service "mediaservice"

echo -e "${YELLOW}8️⃣ Levantando notificationservice...${NC}"
docker compose up -d notificationservice
wait_for_service "notificationservice"

echo -e "${YELLOW}9️⃣ Levantando errorservice...${NC}"
docker compose up -d errorservice
wait_for_service "errorservice"

sleep 2
echo -e "${GREEN}✅ FASE 3 COMPLETADA: MVP Services listos${NC}\n"

# FASE 4: API Gateway
echo ""
echo -e "${BLUE}════ FASE 4: API GATEWAY ════${NC}"
echo ""

echo -e "${YELLOW}🔟 Levantando gateway (Ocelot)...${NC}"
docker compose up -d gateway
wait_for_service "gateway"

sleep 3
echo -e "${GREEN}✅ FASE 4 COMPLETADA: Gateway listo${NC}\n"

# FASE 5: Servicios Adicionales Sprint 1
echo ""
echo -e "${BLUE}════ FASE 5: SERVICIOS SPRINT 1 ════${NC}"
echo ""

echo -e "${YELLOW}1️⃣1️⃣ Levantando maintenanceservice...${NC}"
docker compose up -d maintenanceservice 2>/dev/null || echo "⚠️ maintenanceservice no encontrado, continuando..."

echo -e "${YELLOW}1️⃣2️⃣ Levantando userservice...${NC}"
docker compose up -d userservice 2>/dev/null || echo "⚠️ userservice no encontrado, continuando..."

echo -e "${YELLOW}1️⃣3️⃣ Levantando billingservice...${NC}"
docker compose up -d billingservice 2>/dev/null || echo "⚠️ billingservice no encontrado, continuando..."

sleep 2
echo -e "${GREEN}✅ FASE 5 COMPLETADA${NC}\n"

# FASE 6: Servicios Sprint 2 (Opcional)
echo ""
echo -e "${BLUE}════ FASE 6: SERVICIOS SPRINT 2 (Contacto) ════${NC}"
echo ""

echo -e "${YELLOW}Levantando contactservice...${NC}"
docker compose up -d contactservice 2>/dev/null || echo "⚠️ contactservice no encontrado"

echo -e "${YELLOW}Levantando comparisonservice...${NC}"
docker compose up -d comparisonservice 2>/dev/null || echo "⚠️ comparisonservice no encontrado"

echo -e "${YELLOW}Levantando alertservice...${NC}"
docker compose up -d alertservice 2>/dev/null || echo "⚠️ alertservice no encontrado"

sleep 2

# RESUMEN FINAL
echo ""
echo "════════════════════════════════════════════════════════════════════════════"
echo -e "${GREEN}🎉 STARTUP COMPLETADO${NC}"
echo "════════════════════════════════════════════════════════════════════════════"
echo ""

echo -e "${BLUE}📊 ESTADO DE SERVICIOS:${NC}\n"
docker compose ps --format "table {{.Names}}\t{{.Status}}" | grep -E "CONTAINER|Up|postgres|redis|rabbitmq|auth|role|vehicle|media|notification|error|gateway|maintenance|user|billing|contact|comparison|alert"

echo ""
echo -e "${BLUE}🌐 URLs DE ACCESO:${NC}"
echo "  Gateway:       http://localhost:18443"
echo "  RabbitMQ:      http://localhost:15672 (guest/guest)"
echo "  Redis:         localhost:6379"
echo "  PostgreSQL:    localhost:5432"
echo ""

echo -e "${YELLOW}💡 Próximos pasos:${NC}"
echo "  1. Verificar logs: docker compose logs -f gateway"
echo "  2. Probar API:     curl http://localhost:18443/health"
echo "  3. Iniciar tests:  dotnet test"
echo ""

echo -e "${GREEN}✅ ¡Plataforma lista para desarrollo!${NC}\n"
