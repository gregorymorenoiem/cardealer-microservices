#!/bin/bash
# =====================================================
# OKLA QA - Seed Users via AuthService API
# =====================================================

set -e

API_URL="${API_URL:-http://localhost:18443}"

# Colores
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}=== Creando Usuarios via AuthService API ===${NC}"

# Contador para rate limiting (API permite 5 requests/minuto)
REQUEST_COUNT=0

# Función para manejar rate limiting
check_rate_limit() {
    REQUEST_COUNT=$((REQUEST_COUNT + 1))
    if [ $REQUEST_COUNT -ge 4 ]; then
        echo -e "${YELLOW}⏳ Pausa de 65 segundos (rate limiting: 5 req/min)...${NC}"
        sleep 65
        REQUEST_COUNT=0
    fi
}

# Función para registrar usuario
register_user() {
    local email=$1
    local password=$2
    local username=$3
    local firstName=$4
    local lastName=$5
    local phone=$6
    local accountType=$7
    
    echo -e "${YELLOW}Registrando: $email${NC}"
    
    response=$(curl -s -w "\n%{http_code}" -X POST \
        -H "Content-Type: application/json" \
        -d "{
            \"userName\": \"$username\",
            \"email\": \"$email\",
            \"password\": \"$password\",
            \"confirmPassword\": \"$password\",
            \"firstName\": \"$firstName\",
            \"lastName\": \"$lastName\",
            \"phoneNumber\": \"$phone\",
            \"accountType\": \"$accountType\"
        }" \
        "${API_URL}/api/auth/register")
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -eq 200 ] || [ "$http_code" -eq 201 ]; then
        echo -e "${GREEN}✓ Usuario creado: $email${NC}"
        check_rate_limit
        return 0
    elif [ "$http_code" -eq 409 ]; then
        echo -e "${YELLOW}⚠ Usuario ya existe: $email${NC}"
        return 0
    else
        echo -e "${RED}✗ Error ($http_code): $body${NC}"
        check_rate_limit
        return 1
    fi
}

# =====================================================
# USUARIOS DE PRUEBA
# =====================================================

echo ""
echo -e "${BLUE}=== Usuario Principal de QA (CORREO REAL) ===${NC}"
register_user "gregorymoreno.iem@gmail.com" "Gregory123!@#" "gregorymoreno" "Gregory" "Moreno" "+18095551234" "Admin"

echo ""
echo "=== Usuarios Administrativos ==="
register_user "superadmin@okla.com" "SuperAdmin123!@#" "superadmin" "Super" "Admin" "+18091000001" "Admin"
register_user "admin@okla.com" "Admin123!@#" "admin" "Admin" "OKLA" "+18091000002" "Admin"

echo ""
echo "=== Dealers (Concesionarios) ==="
register_user "dealer1@okla.com" "Dealer123!@#" "dealer1" "Auto Plaza" "RD" "+18092000001" "Dealer"
register_user "dealer2@okla.com" "Dealer123!@#" "dealer2" "Carros Premium" "Santo Domingo" "+18092000002" "Dealer"
register_user "dealer3@okla.com" "Dealer123!@#" "dealer3" "Vehículos del Cibao" "Santiago" "+18092000003" "Dealer"

echo ""
echo "=== Vendedores Individuales ==="
register_user "seller1@okla.com" "Seller123!@#" "seller1" "Juan" "Pérez" "+18093000001" "Individual"
register_user "seller2@okla.com" "Seller123!@#" "seller2" "María" "García" "+18093000002" "Individual"
register_user "seller3@okla.com" "Seller123!@#" "seller3" "Pedro" "Martínez" "+18093000003" "Individual"

echo ""
echo "=== Compradores ==="
register_user "buyer1@okla.com" "Buyer123!@#" "buyer1" "Ana" "Rodríguez" "+18094000001" "Individual"
register_user "buyer2@okla.com" "Buyer123!@#" "buyer2" "Luis" "Fernández" "+18094000002" "Individual"
register_user "buyer3@okla.com" "Buyer123!@#" "buyer3" "Carmen" "López" "+18094000003" "Individual"
register_user "buyer4@okla.com" "Buyer123!@#" "buyer4" "Roberto" "Sánchez" "+18094000004" "Individual"
register_user "buyer5@okla.com" "Buyer123!@#" "buyer5" "Elena" "Torres" "+18094000005" "Individual"

echo ""
echo -e "${GREEN}=== Usuarios Creados ===${NC}"
echo ""

# Confirmar emails para todos los usuarios creados (necesario para login)
echo -e "${YELLOW}Confirmando emails de usuarios...${NC}"
docker exec postgres_db psql -U postgres -d authservice -c "UPDATE \"Users\" SET \"EmailConfirmed\" = true WHERE \"EmailConfirmed\" = false;" > /dev/null 2>&1
echo -e "${GREEN}✓ Emails confirmados${NC}"
echo ""

echo "=============================================="
echo -e "${BLUE}🔑 USUARIO PRINCIPAL QA (CORREO REAL):${NC}"
echo "=============================================="
echo -e "${GREEN}Email:    gregorymoreno.iem@gmail.com${NC}"
echo -e "${GREEN}Password: Gregory123!@#${NC}"
echo -e "${GREEN}Rol:      Admin (todos los privilegios)${NC}"
echo "=============================================="
echo ""
echo "Otros usuarios de prueba:"
echo "========================"
echo "Super Admin: superadmin@okla.com / SuperAdmin123!@#"
echo "Admin:       admin@okla.com / Admin123!@#"
echo "Dealer 1:    dealer1@okla.com / Dealer123!@#"
echo "Dealer 2:    dealer2@okla.com / Dealer123!@#"
echo "Dealer 3:    dealer3@okla.com / Dealer123!@#"
echo "Seller 1:    seller1@okla.com / Seller123!@#"
echo "Buyer 1:     buyer1@okla.com / Buyer123!@#"
echo ""
