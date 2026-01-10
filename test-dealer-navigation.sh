#!/bin/bash

# 🧪 Testing Script - Navegación de Dealers
# Verifica que la solución de navegación funcione correctamente para cada tipo de usuario

echo "🚀 INICIANDO TESTS DE NAVEGACIÓN PARA DEALERS"
echo "=============================================="
echo ""

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# URL base
BASE_URL="http://localhost:8080"

# Función para hacer requests y verificar respuestas
test_endpoint() {
    local description="$1"
    local url="$2"
    local expected_status="$3"
    local cookie="$4"
    
    echo -n "  Testing: $description... "
    
    if [ -n "$cookie" ]; then
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" -H "Cookie: $cookie" "$url")
    else
        response=$(curl -s -w "HTTPSTATUS:%{http_code}" "$url")
    fi
    
    http_code=$(echo $response | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    
    if [ "$http_code" -eq "$expected_status" ]; then
        echo -e "${GREEN}✅ PASS${NC} (HTTP $http_code)"
    else
        echo -e "${RED}❌ FAIL${NC} (Expected $expected_status, got $http_code)"
    fi
}

# Función para simular login y obtener token/cookie
login_user() {
    local email="$1"
    local password="$2"
    
    echo -n "  🔑 Logging in $email... "
    
    # Simulamos login (esto puede necesitar ajuste según tu implementación)
    login_response=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -d '{"email":"'$email'","password":"'$password'"}' \
        "$BASE_URL/api/auth/login")
    
    # Extraer token (esto puede necesitar ajuste)
    token=$(echo "$login_response" | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)
    
    if [ -n "$token" ]; then
        echo -e "${GREEN}✅ SUCCESS${NC}"
        echo "auth_token=Bearer $token; Path=/; HttpOnly"
    else
        echo -e "${RED}❌ FAILED${NC}"
        echo ""
    fi
}

echo "🧪 TESTING CADA TIPO DE USUARIO:"
echo "================================"

# Array de usuarios para testing
declare -a users=(
    "individual@cardealer.com:Password123!:individual:Para Dealers"
    "dealer.free@cardealer.com:Password123!:dealer:Mi Dashboard"
    "dealer.basic@cardealer.com:Password123!:dealer:Mi Dashboard"
    "dealer.pro@cardealer.com:Password123!:dealer:Mi Dashboard"
    "dealer.enterprise@cardealer.com:Password123!:dealer:Mi Dashboard"
    "seller@cardealer.com:Password123!:unknown:Para Dealers o Mi Dashboard"
)

for user in "${users[@]}"; do
    IFS=':' read -r email password expected_type expected_navbar <<< "$user"
    
    echo ""
    echo -e "${BLUE}👤 TESTING USER: $email${NC}"
    echo "  Expected Type: $expected_type"
    echo "  Expected Navbar: $expected_navbar"
    echo "  ----------------------------------------"
    
    # Login del usuario
    cookie=$(login_user "$email" "$password")
    
    if [ -n "$cookie" ] && [[ "$cookie" != *"FAILED"* ]]; then
        
        # Test 1: Homepage debe cargar
        test_endpoint "Homepage access" "$BASE_URL/" 200 "$cookie"
        
        # Test 2: Acceso a dealer dashboard
        if [ "$expected_type" = "dealer" ]; then
            test_endpoint "Dealer Dashboard access (SHOULD WORK)" "$BASE_URL/dealer/dashboard" 200 "$cookie"
            test_endpoint "Dealer Analytics access (SHOULD WORK)" "$BASE_URL/dealer/analytics" 200 "$cookie"
            test_endpoint "Dealer Leads access (SHOULD WORK)" "$BASE_URL/dealer/leads" 200 "$cookie"
        else
            test_endpoint "Dealer Dashboard access (SHOULD REDIRECT)" "$BASE_URL/dealer/dashboard" 302 "$cookie"
        fi
        
        # Test 3: Acceso a páginas públicas debe funcionar siempre
        test_endpoint "Dealer Landing (marketing) access" "$BASE_URL/dealer/landing" 200 "$cookie"
        test_endpoint "Vehicles page access" "$BASE_URL/vehicles" 200 "$cookie"
        
    else
        echo -e "  ${RED}❌ SKIPPING TESTS - Login failed${NC}"
    fi
done

echo ""
echo "🔍 TESTING RUTAS PROTEGIDAS SIN AUTENTICACIÓN:"
echo "=============================================="

test_endpoint "Dealer Dashboard (NO AUTH - should redirect to login)" "$BASE_URL/dealer/dashboard" 302
test_endpoint "Dealer Analytics (NO AUTH - should redirect to login)" "$BASE_URL/dealer/analytics" 302
test_endpoint "Dealer Leads (NO AUTH - should redirect to login)" "$BASE_URL/dealer/leads" 302

echo ""
echo "✅ TESTING RUTAS PÚBLICAS:"
echo "=========================="

test_endpoint "Homepage (PUBLIC)" "$BASE_URL/" 200
test_endpoint "Dealer Landing (PUBLIC)" "$BASE_URL/dealer/landing" 200
test_endpoint "Vehicles (PUBLIC)" "$BASE_URL/vehicles" 200
test_endpoint "Login page (PUBLIC)" "$BASE_URL/login" 200

echo ""
echo "🎯 MANUAL VERIFICATION NEEDED:"
echo "=============================="
echo "Para cada usuario dealer, verificar manualmente en el browser:"
echo "  1. ✅ Navbar muestra 'Mi Dashboard' en lugar de 'Para Dealers'"
echo "  2. ✅ Click en 'Mi Dashboard' va a /dealer/dashboard"
echo "  3. ✅ Dropdown del usuario muestra badge 'Dealer'"
echo "  4. ✅ Acceso completo a todas las secciones del dashboard"
echo ""
echo "Para usuario individual, verificar:"
echo "  1. ✅ Navbar muestra 'Para Dealers'"
echo "  2. ✅ Click en 'Para Dealers' va a /dealer/landing"
echo "  3. ✅ Intentar acceder a /dealer/dashboard redirige a /"
echo ""
echo "🚀 TESTING COMPLETADO"
echo "====================="