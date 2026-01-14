#!/bin/bash

set -e

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo "                   🚀 STARTUP GRADUAL DE SERVICIOS"
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# Colores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# ════════════════════════════════════════════════════════════════════════════════
# FASE 1: INFRAESTRUCTURA BASE (Bases de datos, caché, mensaje broker)
# ════════════════════════════════════════════════════════════════════════════════

echo -e "${BLUE}📌 FASE 1: INFRAESTRUCTURA BASE${NC}"
echo "   Levantando: PostgreSQL, Redis, RabbitMQ, Consul"
echo ""

docker compose up -d \
  postgres_db \
  redis \
  rabbitmq \
  consul \
  2>&1 | grep -E "(Creating|Created|Starting|Started|Network)" || true

echo -e "${GREEN}✓ Esperando 15 segundos a que PostgreSQL esté listo...${NC}"
sleep 15

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# ════════════════════════════════════════════════════════════════════════════════
# FASE 2: SERVICIOS CORE (Auth, Users, Roles)
# ════════════════════════════════════════════════════════════════════════════════

echo -e "${BLUE}📌 FASE 2: SERVICIOS CORE${NC}"
echo "   Levantando: AuthService, UserService, RoleService"
echo ""

docker compose up -d \
  authservice \
  userservice \
  roleservice \
  2>&1 | grep -E "(Creating|Created|Starting|Started|Network)" || true

echo -e "${GREEN}✓ Esperando 10 segundos...${NC}"
sleep 10

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# ════════════════════════════════════════════════════════════════════════════════
# FASE 3: SERVICIOS PRINCIPALES (Vehículos, Media, Notificaciones)
# ════════════════════════════════════════════════════════════════════════════════

echo -e "${BLUE}📌 FASE 3: SERVICIOS PRINCIPALES${NC}"
echo "   Levantando: VehiclesSaleService, MediaService, NotificationService, ErrorService"
echo ""

docker compose up -d \
  vehiclessaleservice \
  mediaservice \
  notificationservice \
  errorservice \
  2>&1 | grep -E "(Creating|Created|Starting|Started|Network)" || true

echo -e "${GREEN}✓ Esperando 10 segundos...${NC}"
sleep 10

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# ════════════════════════════════════════════════════════════════════════════════
# FASE 4: GATEWAY Y SERVICIOS RESTANTES
# ════════════════════════════════════════════════════════════════════════════════

echo -e "${BLUE}📌 FASE 4: GATEWAY Y SERVICIOS RESTANTES${NC}"
echo "   Levantando: Gateway (API), AdminService, BillingService, y otros"
echo ""

docker compose up -d \
  gateway \
  adminservice \
  billingservice \
  dealermanagementservice \
  maintenanceservice \
  2>&1 | grep -E "(Creating|Created|Starting|Started|Network)" || true

echo -e "${GREEN}✓ Esperando 10 segundos...${NC}"
sleep 10

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo "                       📊 ESTADO FINAL DE SERVICIOS"
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# Verificar estado
echo "Contenedores corriendo:"
docker compose ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "(Up|running|Exited|Error)" | head -20

echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""

# Contar servicios
TOTAL=$(docker compose ps --format "json" 2>/dev/null | grep -c '"Name"' || echo "0")
RUNNING=$(docker compose ps --format "json" 2>/dev/null | grep -c '"State":"running"' || echo "0")

echo -e "${GREEN}✅ STARTUP COMPLETO${NC}"
echo "   Total de servicios: $TOTAL"
echo "   Servicios corriendo: $RUNNING"
echo ""
echo "🌐 API Gateway: http://localhost:18443"
echo "📊 PostgreSQL: localhost:5432"
echo "🔴 Redis: localhost:6379"
echo "🐰 RabbitMQ: http://localhost:15672"
echo "🏛️  Consul: http://localhost:8500"
echo ""
echo "════════════════════════════════════════════════════════════════════════════════"
echo ""
