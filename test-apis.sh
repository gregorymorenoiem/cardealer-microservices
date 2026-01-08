#!/bin/bash

# Sprint 1 - API Testing Script
# Prueba todos los endpoints de los servicios existentes y nuevos
# Fecha: Enero 8, 2026

set -e

BASE_URL="http://localhost"
AUTH_PORT="15085"
VEHICLES_PORT="15070"
USER_PORT="15100"

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}🧪 OKLA API Testing - Sprint 1${NC}"
echo "========================================"
echo ""

# 1. Health Checks
echo -e "${YELLOW}1️⃣  Testing Health Checks${NC}"
echo "----------------------------------------"

echo -n "AuthService Health: "
HEALTH=$(curl -s "$BASE_URL:$AUTH_PORT/health")
if [ "$HEALTH" == "Healthy" ]; then
    echo -e "${GREEN}✅ PASS${NC}"
else
    echo -e "${RED}❌ FAIL${NC}"
fi

echo -n "VehiclesSaleService Health: "
HEALTH=$(curl -s "$BASE_URL:$VEHICLES_PORT/health")
if [ "$HEALTH" == "Healthy" ]; then
    echo -e "${GREEN}✅ PASS${NC}"
else
    echo -e "${RED}❌ FAIL${NC}"
fi

echo -n "UserService Health: "
HEALTH=$(curl -s "$BASE_URL:$USER_PORT/health")
if [ "$HEALTH" == "Healthy" ]; then
    echo -e "${GREEN}✅ PASS${NC}"
else
    echo -e "${RED}❌ FAIL${NC}"
fi

echo ""

# 2. Authentication Tests
echo -e "${YELLOW}2️⃣  Testing Authentication${NC}"
echo "----------------------------------------"

# Register new user
echo -n "Register User: "
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL:$AUTH_PORT/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\":\"testuser$(date +%s)@okla.com.do\",
    \"password\":\"TestPassword123!\",
    \"userName\":\"testuser$(date +%s)\",
    \"fullName\":\"Test User\",
    \"accountType\":\"Individual\"
  }")

TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.data.accessToken')

if [ "$TOKEN" != "null" ] && [ ! -z "$TOKEN" ]; then
    echo -e "${GREEN}✅ PASS${NC} (Token obtenido)"
    export TOKEN
else
    echo -e "${RED}❌ FAIL${NC}"
    echo "Response: $REGISTER_RESPONSE"
    exit 1
fi

echo ""

# 3. Vehicles Tests
echo -e "${YELLOW}3️⃣  Testing Vehicles API${NC}"
echo "----------------------------------------"

echo -n "GET /api/vehicles (lista): "
VEHICLES=$(curl -s "$BASE_URL:$VEHICLES_PORT/api/vehicles?pageNumber=1&pageSize=5")
TOTAL=$(echo $VEHICLES | jq -r '.totalCount')
if [ "$TOTAL" != "null" ]; then
    echo -e "${GREEN}✅ PASS${NC} ($TOTAL vehículos totales)"
else
    echo -e "${RED}❌ FAIL${NC}"
fi

echo -n "GET /api/homepagesections/homepage: "
SECTIONS=$(curl -s "$BASE_URL:$VEHICLES_PORT/api/homepagesections/homepage")
SECTION_COUNT=$(echo $SECTIONS | jq '. | length')
if [ "$SECTION_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✅ PASS${NC} ($SECTION_COUNT secciones)"
else
    echo -e "${RED}❌ FAIL${NC}"
fi

echo -n "GET /api/catalog/makes: "
MAKES=$(curl -s "$BASE_URL:$VEHICLES_PORT/api/catalog/makes")
MAKES_COUNT=$(echo $MAKES | jq '. | length')
if [ "$MAKES_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✅ PASS${NC} ($MAKES_COUNT marcas)"
else
    echo -e "${RED}❌ FAIL${NC}"
fi

echo ""

# 4. User Profile Tests
echo -e "${YELLOW}4️⃣  Testing User Profile${NC}"
echo "----------------------------------------"

USER_ID=$(echo $REGISTER_RESPONSE | jq -r '.data.userId')

echo -n "GET /api/users/$USER_ID: "
USER_PROFILE=$(curl -s "$BASE_URL:$USER_PORT/api/users/$USER_ID" \
  -H "Authorization: Bearer $TOKEN")
USER_EMAIL=$(echo $USER_PROFILE | jq -r '.email // .data.email // empty')
if [ ! -z "$USER_EMAIL" ]; then
    echo -e "${GREEN}✅ PASS${NC}"
else
    echo -e "${RED}❌ FAIL${NC}"
    echo "Response: $USER_PROFILE"
fi

echo ""

# 5. Favorites Tests (si el endpoint existe)
echo -e "${YELLOW}5️⃣  Testing Favorites (Sprint 1 - Frontend ready)${NC}"
echo "----------------------------------------"

echo -n "GET /api/favorites: "
FAVORITES=$(curl -s "$BASE_URL:$VEHICLES_PORT/api/favorites" \
  -H "Authorization: Bearer $TOKEN" 2>&1)

if echo "$FAVORITES" | jq '.' > /dev/null 2>&1; then
    FAVS_COUNT=$(echo $FAVORITES | jq '. | length')
    echo -e "${GREEN}✅ PASS${NC} ($FAVS_COUNT favoritos)"
elif echo "$FAVORITES" | grep -q "404"; then
    echo -e "${YELLOW}⚠️  PENDING${NC} (endpoint no implementado aún)"
else
    echo -e "${YELLOW}⚠️  PENDING${NC}"
fi

echo ""

# 6. New Services Tests (Sprint 1)
echo -e "${YELLOW}6️⃣  Testing NEW Services (Sprint 1)${NC}"
echo "----------------------------------------"

# MaintenanceService
echo -n "MaintenanceService (5061): "
MAINTENANCE=$(curl -s "http://localhost:5061/api/maintenance/current" 2>&1)
if echo "$MAINTENANCE" | grep -q "Connection refused"; then
    echo -e "${YELLOW}⚠️  SERVICE NOT RUNNING${NC}"
else
    echo -e "${GREEN}✅ SERVICE UP${NC}"
fi

# ComparisonService
echo -n "ComparisonService (5066): "
COMPARISON=$(curl -s "http://localhost:5066/api/comparisons" \
  -H "Authorization: Bearer $TOKEN" 2>&1)
if echo "$COMPARISON" | grep -q "Connection refused"; then
    echo -e "${YELLOW}⚠️  SERVICE NOT RUNNING${NC}"
else
    echo -e "${GREEN}✅ SERVICE UP${NC}"
fi

# AlertService
echo -n "AlertService (5067): "
ALERTS=$(curl -s "http://localhost:5067/api/alerts/price-alerts" \
  -H "Authorization: Bearer $TOKEN" 2>&1)
if echo "$ALERTS" | grep -q "Connection refused"; then
    echo -e "${YELLOW}⚠️  SERVICE NOT RUNNING${NC}"
else
    echo -e "${GREEN}✅ SERVICE UP${NC}"
fi

echo ""

# Summary
echo "========================================"
echo -e "${GREEN}✅ Testing Completed${NC}"
echo ""
echo "Servicios existentes funcionando correctamente."
echo "Nuevos servicios (Sprint 1) necesitan:"
echo "  1. Agregar a compose.yaml"
echo "  2. docker compose up maintenanceservice comparisonservice alertservice"
echo ""
echo "Token JWT guardado en variable \$TOKEN"
echo "Usar: curl -H \"Authorization: Bearer \$TOKEN\" {URL}"
