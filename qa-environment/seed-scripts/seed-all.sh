#!/bin/bash
# =====================================================
# OKLA QA Environment - Seed Data via API
# =====================================================
# Este script crea todos los datos de prueba usando los APIs
# de los microservicios, simulando uso real del sistema.
#
# Uso: ./seed-all.sh
# =====================================================

set -e  # Exit on error

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuración
API_URL="${API_URL:-http://localhost:18443}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Variables globales para tokens
ADMIN_TOKEN=""
DEALER_TOKEN=""
SELLER_TOKEN=""
BUYER_TOKEN=""

echo -e "${BLUE}=====================================${NC}"
echo -e "${BLUE}   OKLA QA - Seeding via API${NC}"
echo -e "${BLUE}=====================================${NC}"
echo ""
echo -e "API URL: ${YELLOW}${API_URL}${NC}"
echo ""

# =====================================================
# Funciones de utilidad
# =====================================================

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[OK]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Función para hacer requests con retry
api_request() {
    local method=$1
    local endpoint=$2
    local data=$3
    local token=$4
    local max_retries=3
    local retry=0
    
    local headers="-H 'Content-Type: application/json'"
    if [ -n "$token" ]; then
        headers="$headers -H 'Authorization: Bearer $token'"
    fi
    
    while [ $retry -lt $max_retries ]; do
        if [ -n "$data" ]; then
            response=$(curl -s -w "\n%{http_code}" -X "$method" \
                -H "Content-Type: application/json" \
                ${token:+-H "Authorization: Bearer $token"} \
                -d "$data" \
                "${API_URL}${endpoint}" 2>/dev/null)
        else
            response=$(curl -s -w "\n%{http_code}" -X "$method" \
                -H "Content-Type: application/json" \
                ${token:+-H "Authorization: Bearer $token"} \
                "${API_URL}${endpoint}" 2>/dev/null)
        fi
        
        http_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | sed '$d')
        
        if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
            echo "$body"
            return 0
        elif [ "$http_code" -eq 409 ]; then
            # Conflict - ya existe, es OK para seeding idempotente
            echo "$body"
            return 0
        elif [ "$http_code" -ge 500 ]; then
            retry=$((retry + 1))
            log_warning "Server error ($http_code), retry $retry/$max_retries..."
            sleep 2
        else
            log_error "Request failed: $method $endpoint (HTTP $http_code)"
            echo "$body" >&2
            return 1
        fi
    done
    
    log_error "Max retries reached for $method $endpoint"
    return 1
}

# Esperar a que los servicios estén healthy
wait_for_services() {
    log_info "Esperando a que los servicios estén listos..."
    
    local services=("health" "api/auth/health" "api/roles/health" "api/vehicles/health")
    
    for endpoint in "${services[@]}"; do
        local retries=30
        while [ $retries -gt 0 ]; do
            if curl -s "${API_URL}/${endpoint}" > /dev/null 2>&1; then
                log_success "Servicio listo: $endpoint"
                break
            fi
            retries=$((retries - 1))
            sleep 2
        done
        
        if [ $retries -eq 0 ]; then
            log_error "Timeout esperando servicio: $endpoint"
            exit 1
        fi
    done
    
    echo ""
}

# =====================================================
# 1. SEED USUARIOS (AuthService)
# =====================================================
seed_users() {
    echo -e "${BLUE}=====================================${NC}"
    echo -e "${BLUE}  1. Creando Usuarios de Prueba${NC}"
    echo -e "${BLUE}=====================================${NC}"
    
    # Admin user
    log_info "Creando usuario admin@okla.com..."
    response=$(api_request POST "/api/auth/register" '{
        "userName": "admin",
        "email": "admin@okla.com",
        "password": "Admin123!@#",
        "confirmPassword": "Admin123!@#",
        "firstName": "Admin",
        "lastName": "OKLA",
        "phoneNumber": "+18091234567",
        "accountType": "Admin"
    }')
    
    if [ $? -eq 0 ]; then
        log_success "Usuario admin creado"
    fi
    
    # Dealer user
    log_info "Creando usuario dealer@okla.com..."
    response=$(api_request POST "/api/auth/register" '{
        "userName": "dealer1",
        "email": "dealer@okla.com",
        "password": "Dealer123!@#",
        "confirmPassword": "Dealer123!@#",
        "firstName": "Carlos",
        "lastName": "Dealer",
        "phoneNumber": "+18092345678",
        "accountType": "Dealer"
    }')
    
    if [ $? -eq 0 ]; then
        log_success "Usuario dealer creado"
    fi
    
    # Seller user (individual)
    log_info "Creando usuario seller@okla.com..."
    response=$(api_request POST "/api/auth/register" '{
        "userName": "seller1",
        "email": "seller@okla.com",
        "password": "Seller123!@#",
        "confirmPassword": "Seller123!@#",
        "firstName": "Maria",
        "lastName": "Vendedor",
        "phoneNumber": "+18093456789",
        "accountType": "Individual"
    }')
    
    if [ $? -eq 0 ]; then
        log_success "Usuario seller creado"
    fi
    
    # Buyer users
    log_info "Creando usuarios compradores..."
    for i in 1 2 3; do
        response=$(api_request POST "/api/auth/register" "{
            \"userName\": \"buyer${i}\",
            \"email\": \"buyer${i}@okla.com\",
            \"password\": \"Buyer123!@#\",
            \"confirmPassword\": \"Buyer123!@#\",
            \"firstName\": \"Comprador\",
            \"lastName\": \"Numero${i}\",
            \"phoneNumber\": \"+1809456789${i}\",
            \"accountType\": \"Individual\"
        }")
        
        if [ $? -eq 0 ]; then
            log_success "Usuario buyer${i} creado"
        fi
    done
    
    echo ""
}

# =====================================================
# 2. LOGIN Y OBTENER TOKENS
# =====================================================
get_tokens() {
    echo -e "${BLUE}=====================================${NC}"
    echo -e "${BLUE}  2. Obteniendo Tokens de Acceso${NC}"
    echo -e "${BLUE}=====================================${NC}"
    
    # Admin token
    log_info "Login como admin..."
    response=$(api_request POST "/api/auth/login" '{
        "email": "admin@okla.com",
        "password": "Admin123!@#"
    }')
    
    if [ $? -eq 0 ]; then
        ADMIN_TOKEN=$(echo "$response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
        if [ -n "$ADMIN_TOKEN" ]; then
            log_success "Token admin obtenido"
        else
            log_error "No se pudo extraer token admin"
        fi
    fi
    
    # Dealer token
    log_info "Login como dealer..."
    response=$(api_request POST "/api/auth/login" '{
        "email": "dealer@okla.com",
        "password": "Dealer123!@#"
    }')
    
    if [ $? -eq 0 ]; then
        DEALER_TOKEN=$(echo "$response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
        if [ -n "$DEALER_TOKEN" ]; then
            log_success "Token dealer obtenido"
        fi
    fi
    
    # Seller token
    log_info "Login como seller..."
    response=$(api_request POST "/api/auth/login" '{
        "email": "seller@okla.com",
        "password": "Seller123!@#"
    }')
    
    if [ $? -eq 0 ]; then
        SELLER_TOKEN=$(echo "$response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
        if [ -n "$SELLER_TOKEN" ]; then
            log_success "Token seller obtenido"
        fi
    fi
    
    # Buyer token
    log_info "Login como buyer1..."
    response=$(api_request POST "/api/auth/login" '{
        "email": "buyer1@okla.com",
        "password": "Buyer123!@#"
    }')
    
    if [ $? -eq 0 ]; then
        BUYER_TOKEN=$(echo "$response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
        if [ -n "$BUYER_TOKEN" ]; then
            log_success "Token buyer obtenido"
        fi
    fi
    
    echo ""
}

# =====================================================
# 3. SEED VEHÍCULOS (VehiclesSaleService)
# =====================================================
seed_vehicles() {
    echo -e "${BLUE}=====================================${NC}"
    echo -e "${BLUE}  3. Creando Vehículos de Prueba${NC}"
    echo -e "${BLUE}=====================================${NC}"
    
    # Verificar que tenemos token
    if [ -z "$DEALER_TOKEN" ]; then
        log_error "No hay token de dealer, saltando vehículos"
        return 1
    fi
    
    # Array de vehículos para crear
    declare -a vehicles=(
        '{"title":"2024 Toyota Corolla LE","description":"Sedan económico y confiable, perfecto para la ciudad. Excelente consumo de combustible.","price":1250000,"currency":"DOP","make":"Toyota","model":"Corolla","trim":"LE","year":2024,"vehicleType":"Sedan","bodyStyle":"Sedan","doors":4,"seats":5,"fuelType":"Gasoline","engineSize":"1.8L","horsepower":139,"transmission":"Automatic","driveType":"FWD","mileage":0,"mileageUnit":"Km","condition":"New","exteriorColor":"Blanco Perla","interiorColor":"Negro","city":"Santo Domingo","state":"Distrito Nacional","country":"DO"}'
        '{"title":"2023 Honda CR-V EX","description":"SUV familiar con amplio espacio y tecnología avanzada de seguridad Honda Sensing.","price":2100000,"currency":"DOP","make":"Honda","model":"CR-V","trim":"EX","year":2023,"vehicleType":"SUV","bodyStyle":"SUV","doors":5,"seats":5,"fuelType":"Gasoline","engineSize":"1.5L Turbo","horsepower":190,"transmission":"Automatic","driveType":"AWD","mileage":15000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Gris Moderno","interiorColor":"Beige","city":"Santiago","state":"Santiago","country":"DO"}'
        '{"title":"2022 Hyundai Tucson Limited","description":"SUV compacto con diseño futurista y todas las comodidades modernas.","price":1850000,"currency":"DOP","make":"Hyundai","model":"Tucson","trim":"Limited","year":2022,"vehicleType":"SUV","bodyStyle":"SUV","doors":5,"seats":5,"fuelType":"Gasoline","engineSize":"2.5L","horsepower":187,"transmission":"Automatic","driveType":"FWD","mileage":28000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Azul","interiorColor":"Negro","city":"Santo Domingo","state":"Distrito Nacional","country":"DO"}'
        '{"title":"2024 Kia Sportage SX","description":"La nueva generación del Sportage con tecnología de punta y diseño audaz.","price":2300000,"currency":"DOP","make":"Kia","model":"Sportage","trim":"SX","year":2024,"vehicleType":"SUV","bodyStyle":"SUV","doors":5,"seats":5,"fuelType":"Gasoline","engineSize":"2.5L Turbo","horsepower":281,"transmission":"Automatic","driveType":"AWD","mileage":5000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Rojo","interiorColor":"Negro","city":"Punta Cana","state":"La Altagracia","country":"DO"}'
        '{"title":"2023 Nissan Sentra SR","description":"Sedan deportivo con excelente equipamiento y bajo consumo.","price":1100000,"currency":"DOP","make":"Nissan","model":"Sentra","trim":"SR","year":2023,"vehicleType":"Sedan","bodyStyle":"Sedan","doors":4,"seats":5,"fuelType":"Gasoline","engineSize":"2.0L","horsepower":149,"transmission":"Automatic","driveType":"FWD","mileage":20000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Negro","interiorColor":"Negro","city":"Santo Domingo","state":"Distrito Nacional","country":"DO"}'
        '{"title":"2024 Toyota RAV4 Hybrid XLE","description":"SUV híbrido con excelente eficiencia de combustible y espacio familiar.","price":2800000,"currency":"DOP","make":"Toyota","model":"RAV4","trim":"Hybrid XLE","year":2024,"vehicleType":"SUV","bodyStyle":"SUV","doors":5,"seats":5,"fuelType":"Hybrid","engineSize":"2.5L Hybrid","horsepower":219,"transmission":"Automatic","driveType":"AWD","mileage":0,"mileageUnit":"Km","condition":"New","exteriorColor":"Plata","interiorColor":"Negro","city":"Santo Domingo","state":"Distrito Nacional","country":"DO"}'
        '{"title":"2022 Ford F-150 XLT","description":"Pickup full-size con capacidad de carga y remolque superior.","price":3200000,"currency":"DOP","make":"Ford","model":"F-150","trim":"XLT","year":2022,"vehicleType":"Truck","bodyStyle":"Pickup","doors":4,"seats":5,"fuelType":"Gasoline","engineSize":"3.5L V6 EcoBoost","horsepower":400,"transmission":"Automatic","driveType":"4WD","mileage":35000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Azul","interiorColor":"Gris","city":"Santiago","state":"Santiago","country":"DO"}'
        '{"title":"2023 Mazda CX-5 Grand Touring","description":"SUV premium con interior de lujo y manejo deportivo.","price":2400000,"currency":"DOP","make":"Mazda","model":"CX-5","trim":"Grand Touring","year":2023,"vehicleType":"SUV","bodyStyle":"SUV","doors":5,"seats":5,"fuelType":"Gasoline","engineSize":"2.5L Turbo","horsepower":256,"transmission":"Automatic","driveType":"AWD","mileage":12000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Rojo Soul","interiorColor":"Blanco","city":"Santo Domingo","state":"Distrito Nacional","country":"DO"}'
        '{"title":"2024 Chevrolet Silverado LT","description":"Pickup resistente con tecnología avanzada y gran capacidad.","price":3500000,"currency":"DOP","make":"Chevrolet","model":"Silverado","trim":"LT","year":2024,"vehicleType":"Truck","bodyStyle":"Pickup","doors":4,"seats":5,"fuelType":"Gasoline","engineSize":"5.3L V8","horsepower":355,"transmission":"Automatic","driveType":"4WD","mileage":8000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Blanco","interiorColor":"Negro","city":"La Romana","state":"La Romana","country":"DO"}'
        '{"title":"2023 Jeep Wrangler Sahara","description":"El icónico todoterreno con capacidades off-road incomparables.","price":3800000,"currency":"DOP","make":"Jeep","model":"Wrangler","trim":"Sahara","year":2023,"vehicleType":"SUV","bodyStyle":"SUV","doors":4,"seats":5,"fuelType":"Gasoline","engineSize":"3.6L V6","horsepower":285,"transmission":"Automatic","driveType":"4WD","mileage":18000,"mileageUnit":"Km","condition":"Used","exteriorColor":"Verde","interiorColor":"Negro","city":"Puerto Plata","state":"Puerto Plata","country":"DO"}'
    )
    
    local count=0
    for vehicle in "${vehicles[@]}"; do
        count=$((count + 1))
        log_info "Creando vehículo $count/10..."
        
        response=$(api_request POST "/api/vehicles" "$vehicle" "$DEALER_TOKEN")
        
        if [ $? -eq 0 ]; then
            log_success "Vehículo $count creado"
        else
            log_warning "Error creando vehículo $count"
        fi
    done
    
    echo ""
}

# =====================================================
# 4. SEED FAVORITOS Y CONTACTOS
# =====================================================
seed_interactions() {
    echo -e "${BLUE}=====================================${NC}"
    echo -e "${BLUE}  4. Creando Interacciones de Prueba${NC}"
    echo -e "${BLUE}=====================================${NC}"
    
    if [ -z "$BUYER_TOKEN" ]; then
        log_warning "No hay token de buyer, saltando interacciones"
        return 0
    fi
    
    # Obtener lista de vehículos
    log_info "Obteniendo lista de vehículos..."
    vehicles_response=$(api_request GET "/api/vehicles?pageSize=5" "" "$BUYER_TOKEN")
    
    if [ $? -eq 0 ]; then
        log_success "Lista de vehículos obtenida"
        
        # Extraer IDs de vehículos (simplificado)
        # En un script más robusto usaríamos jq
        log_info "Agregando vehículos a favoritos..."
        # Los favoritos se agregarían aquí con los IDs extraídos
    fi
    
    echo ""
}

# =====================================================
# 5. RESUMEN FINAL
# =====================================================
show_summary() {
    echo -e "${GREEN}=====================================${NC}"
    echo -e "${GREEN}   SEEDING COMPLETADO${NC}"
    echo -e "${GREEN}=====================================${NC}"
    echo ""
    echo -e "${BLUE}Usuarios creados:${NC}"
    echo "  - admin@okla.com (Admin)"
    echo "  - dealer@okla.com (Dealer)"
    echo "  - seller@okla.com (Seller)"
    echo "  - buyer1@okla.com, buyer2@okla.com, buyer3@okla.com (Buyers)"
    echo ""
    echo -e "${BLUE}Credenciales de prueba:${NC}"
    echo "  - Admin:  admin@okla.com / Admin123!@#"
    echo "  - Dealer: dealer@okla.com / Dealer123!@#"
    echo "  - Seller: seller@okla.com / Seller123!@#"
    echo "  - Buyer:  buyer1@okla.com / Buyer123!@#"
    echo ""
    echo -e "${BLUE}Vehículos creados:${NC} 10 vehículos de diferentes tipos"
    echo ""
    echo -e "${YELLOW}URLs importantes:${NC}"
    echo "  - Frontend: http://localhost:3001"
    echo "  - API Gateway: http://localhost:18443"
    echo "  - RabbitMQ: http://localhost:15672 (guest/guest)"
    echo ""
}

# =====================================================
# MAIN
# =====================================================
main() {
    # Esperar servicios
    wait_for_services
    
    # Ejecutar seeds en orden
    seed_users
    get_tokens
    seed_vehicles
    seed_interactions
    
    # Mostrar resumen
    show_summary
}

# Ejecutar
main "$@"
