#!/bin/bash
# =====================================================
# OKLA QA - Seed Vehicles via VehiclesSaleService API
# =====================================================
# Este script:
# 1. Crea vehículos con todos los campos requeridos (VIN, contacto)
# 2. Agrega imágenes a cada vehículo
# 3. Publica los vehículos para que aparezcan en el listado

set -e

API_URL="${API_URL:-http://localhost:18443}"

# Colores
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║         OKLA - Seed Vehicles via API                       ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# =====================================================
# FUNCIONES AUXILIARES
# =====================================================

# Contador para generar VINs únicos
VIN_COUNTER=0
VIN_BASE=$(date +%s)

# Generar VIN único de 17 caracteres - incrementa contador global
next_vin() {
    VIN_COUNTER=$((VIN_COUNTER + 1))
    CURRENT_VIN=$(printf "OKLA%010d%03d" $VIN_BASE $VIN_COUNTER)
    CURRENT_VIN="${CURRENT_VIN:0:17}"
}

# Variable global para el ID del último vehículo creado
LAST_VEHICLE_ID=""

# Función para crear vehículo y guardar el ID en LAST_VEHICLE_ID
create_vehicle() {
    local json=$1
    local name=$2
    
    LAST_VEHICLE_ID=""
    echo -e "${YELLOW}📝 Creando: ${name}${NC}"
    
    response=$(curl -s -w "\n%{http_code}" -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $DEALER_TOKEN" \
        -d "$json" \
        "${API_URL}/api/vehicles")
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -eq 200 ] || [ "$http_code" -eq 201 ]; then
        LAST_VEHICLE_ID=$(echo "$body" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
        if [ -n "$LAST_VEHICLE_ID" ]; then
            echo -e "${GREEN}   ✓ Creado con ID: ${LAST_VEHICLE_ID}${NC}"
            return 0
        fi
    fi
    
    echo -e "${RED}   ✗ Error ($http_code) creando $name${NC}"
    echo "$body" | head -3
    return 1
}

# Función para agregar imágenes a un vehículo (directamente en BD)
add_images() {
    local vehicle_id=$1
    local random_seed=$2
    
    echo -e "${CYAN}   📷 Agregando imágenes...${NC}"
    
    # Insertar directamente en BD (workaround para bug de concurrencia en API)
    docker exec postgres_db psql -U postgres -d vehiclessaleservice -c "
        INSERT INTO vehicle_images (\"Id\", \"DealerId\", \"VehicleId\", \"Url\", \"ThumbnailUrl\", \"Caption\", \"ImageType\", \"SortOrder\", \"IsPrimary\", \"MimeType\", \"CreatedAt\")
        VALUES 
        (gen_random_uuid(), (SELECT \"DealerId\" FROM vehicles WHERE \"Id\" = '${vehicle_id}'), '${vehicle_id}', 'https://picsum.photos/800/600?random=${random_seed}1', 'https://picsum.photos/200/150?random=${random_seed}1', 'Vista frontal', 0, 0, true, 'image/jpeg', NOW()),
        (gen_random_uuid(), (SELECT \"DealerId\" FROM vehicles WHERE \"Id\" = '${vehicle_id}'), '${vehicle_id}', 'https://picsum.photos/800/600?random=${random_seed}2', 'https://picsum.photos/200/150?random=${random_seed}2', 'Vista lateral', 0, 1, false, 'image/jpeg', NOW()),
        (gen_random_uuid(), (SELECT \"DealerId\" FROM vehicles WHERE \"Id\" = '${vehicle_id}'), '${vehicle_id}', 'https://picsum.photos/800/600?random=${random_seed}3', 'https://picsum.photos/200/150?random=${random_seed}3', 'Interior', 1, 2, false, 'image/jpeg', NOW());
    " > /dev/null 2>&1
    echo -e "${GREEN}   ✓ 3 imágenes agregadas${NC}"
    return 0
}

# Función para publicar un vehículo
publish_vehicle() {
    local vehicle_id=$1
    
    echo -e "${CYAN}   🚀 Publicando...${NC}"
    
    response=$(curl -s -w "\n%{http_code}" -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $DEALER_TOKEN" \
        "${API_URL}/api/vehicles/${vehicle_id}/publish" \
        -d '{}')
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -eq 200 ]; then
        echo -e "${GREEN}   ✓ Publicado y visible para compradores${NC}"
        return 0
    else
        echo -e "${RED}   ✗ Error publicando: $(echo $body | grep -o '"message":"[^"]*"')${NC}"
        return 1
    fi
}

# Función completa: crear, agregar imágenes, publicar
create_and_publish_vehicle() {
    local json=$1
    local name=$2
    local random_seed=$3
    
    # Generar VIN único
    next_vin
    local json_with_vin=$(echo "$json" | sed "s/VIN_PLACEHOLDER/$CURRENT_VIN/g")
    
    create_vehicle "$json_with_vin" "$name"
    
    if [ -n "$LAST_VEHICLE_ID" ] && [ "$LAST_VEHICLE_ID" != "null" ]; then
        add_images "$LAST_VEHICLE_ID" "$random_seed"
        publish_vehicle "$LAST_VEHICLE_ID"
        echo ""
        return 0
    else
        echo ""
        return 1
    fi
}

# =====================================================
# OBTENER TOKEN
# =====================================================

echo -e "${YELLOW}🔑 Autenticando como dealer...${NC}"

# Intentar con dealer1 (creado por 01-seed-users.sh)
login_response=$(curl -s -X POST \
    -H "Content-Type: application/json" \
    -d '{"email": "dealer1@okla.com", "password": "Dealer123!@#"}' \
    "${API_URL}/api/auth/login")

DEALER_TOKEN=$(echo "$login_response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$DEALER_TOKEN" ]; then
    echo -e "${RED}Error: No se pudo obtener token.${NC}"
    echo -e "${RED}Asegúrate de tener un usuario creado con email verificado.${NC}"
    echo -e "${YELLOW}Puedes crear uno manualmente:${NC}"
    echo "  1. POST /api/auth/register con email/password"
    echo "  2. docker exec postgres_db psql -U postgres -d authservice -c \"UPDATE \\\"Users\\\" SET \\\"EmailConfirmed\\\" = true WHERE \\\"Email\\\" = 'tu@email.com';\""
    exit 1
fi

echo -e "${GREEN}✓ Token obtenido${NC}"
echo ""

# =====================================================
# VEHÍCULOS DE PRUEBA - REPÚBLICA DOMINICANA
# =====================================================

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}           🚗 SEDANES (3)               ${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

create_and_publish_vehicle "{
    \"title\": \"2024 Toyota Corolla LE - Nuevo\",
    \"description\": \"Toyota Corolla completamente nuevo. El sedan más vendido del mundo, conocido por su confiabilidad y bajo consumo de combustible. Incluye Toyota Safety Sense 2.0.\",
    \"price\": 1350000,
    \"currency\": \"DOP\",
    \"make\": \"Toyota\",
    \"model\": \"Corolla\",
    \"trim\": \"LE\",
    \"year\": 2024,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 0,
    \"bodyStyle\": 0,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"1.8L\",
    \"horsepower\": 139,
    \"transmission\": 0,
    \"driveType\": 0,
    \"mileage\": 0,
    \"mileageUnit\": 0,
    \"condition\": 0,
    \"exteriorColor\": \"Blanco Super\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"AutoMax RD\",
    \"sellerEmail\": \"ventas@automax.do\",
    \"sellerPhone\": \"+18095551001\",
    \"isFeatured\": true
}" "2024 Toyota Corolla LE" "101"

create_and_publish_vehicle "{
    \"title\": \"2023 Honda Civic Touring - Impecable\",
    \"description\": \"Honda Civic Touring con 8,000 km. Techo solar, asientos de cuero, sistema de navegación. Un solo dueño, historial de mantenimiento completo.\",
    \"price\": 1680000,
    \"currency\": \"DOP\",
    \"make\": \"Honda\",
    \"model\": \"Civic\",
    \"trim\": \"Touring\",
    \"year\": 2023,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 0,
    \"bodyStyle\": 0,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"1.5L Turbo\",
    \"horsepower\": 180,
    \"transmission\": 0,
    \"driveType\": 0,
    \"mileage\": 8000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Gris Metálico\",
    \"interiorColor\": \"Beige\",
    \"city\": \"Santiago\",
    \"state\": \"Santiago\",
    \"country\": \"DO\",
    \"sellerName\": \"Honda Santiago\",
    \"sellerEmail\": \"ventas@hondasantiago.do\",
    \"sellerPhone\": \"+18095552002\",
    \"isFeatured\": true
}" "2023 Honda Civic Touring" "102"

create_and_publish_vehicle "{
    \"title\": \"2022 Hyundai Elantra SEL - Económico\",
    \"description\": \"Hyundai Elantra SEL con excelente economía de combustible. Pantalla táctil de 8 pulgadas, Apple CarPlay y Android Auto.\",
    \"price\": 1150000,
    \"currency\": \"DOP\",
    \"make\": \"Hyundai\",
    \"model\": \"Elantra\",
    \"trim\": \"SEL\",
    \"year\": 2022,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 0,
    \"bodyStyle\": 0,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"2.0L\",
    \"horsepower\": 147,
    \"transmission\": 0,
    \"driveType\": 0,
    \"mileage\": 25000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Azul\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"Hyundai RD\",
    \"sellerEmail\": \"info@hyundai.do\",
    \"sellerPhone\": \"+18095553003\",
    \"isFeatured\": false
}" "2022 Hyundai Elantra SEL" "103"

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}           🚙 SUVs (5)                  ${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

create_and_publish_vehicle "{
    \"title\": \"2024 Toyota RAV4 XLE - Híbrido\",
    \"description\": \"RAV4 híbrido nuevo. La combinación perfecta de eficiencia y espacio. AWD, 41 MPG combinado, Toyota Safety Sense 2.5+.\",
    \"price\": 2450000,
    \"currency\": \"DOP\",
    \"make\": \"Toyota\",
    \"model\": \"RAV4\",
    \"trim\": \"XLE Hybrid\",
    \"year\": 2024,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 5,
    \"seats\": 5,
    \"fuelType\": 2,
    \"engineSize\": \"2.5L Hybrid\",
    \"horsepower\": 219,
    \"transmission\": 0,
    \"driveType\": 1,
    \"mileage\": 0,
    \"mileageUnit\": 0,
    \"condition\": 0,
    \"exteriorColor\": \"Verde Bosque\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"Toyota Dominicana\",
    \"sellerEmail\": \"ventas@toyota.do\",
    \"sellerPhone\": \"+18095554001\",
    \"isFeatured\": true
}" "2024 Toyota RAV4 XLE Híbrido" "201"

create_and_publish_vehicle "{
    \"title\": \"2023 Honda CR-V EX-L - Turbo\",
    \"description\": \"Honda CR-V nueva generación. Motor turbo 1.5L, asientos de cuero, techo solar panorámico. La SUV familiar por excelencia.\",
    \"price\": 2280000,
    \"currency\": \"DOP\",
    \"make\": \"Honda\",
    \"model\": \"CR-V\",
    \"trim\": \"EX-L\",
    \"year\": 2023,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 5,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"1.5L Turbo\",
    \"horsepower\": 190,
    \"transmission\": 0,
    \"driveType\": 1,
    \"mileage\": 5000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Blanco Perla\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Punta Cana\",
    \"state\": \"La Altagracia\",
    \"country\": \"DO\",
    \"sellerName\": \"Honda Punta Cana\",
    \"sellerEmail\": \"sales@hondapc.do\",
    \"sellerPhone\": \"+18095555002\",
    \"isFeatured\": true
}" "2023 Honda CR-V EX-L" "202"

create_and_publish_vehicle "{
    \"title\": \"2024 Mazda CX-5 Signature - Premium\",
    \"description\": \"Mazda CX-5 tope de línea. Motor turbo 2.5L, interiores de cuero Nappa, Bose premium sound. El SUV con el mejor manejo de su clase.\",
    \"price\": 2650000,
    \"currency\": \"DOP\",
    \"make\": \"Mazda\",
    \"model\": \"CX-5\",
    \"trim\": \"Signature\",
    \"year\": 2024,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 5,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"2.5L Turbo\",
    \"horsepower\": 256,
    \"transmission\": 0,
    \"driveType\": 1,
    \"mileage\": 0,
    \"mileageUnit\": 0,
    \"condition\": 0,
    \"exteriorColor\": \"Rojo Soul Crystal\",
    \"interiorColor\": \"Marrón\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"Mazda RD\",
    \"sellerEmail\": \"ventas@mazda.do\",
    \"sellerPhone\": \"+18095556003\",
    \"isFeatured\": true
}" "2024 Mazda CX-5 Signature" "203"

create_and_publish_vehicle "{
    \"title\": \"2022 Nissan X-Trail S - 7 Pasajeros\",
    \"description\": \"Nissan X-Trail con tercera fila. Ideal para familias grandes. Cámara 360, sensores de estacionamiento, pantalla de 12 pulgadas.\",
    \"price\": 1850000,
    \"currency\": \"DOP\",
    \"make\": \"Nissan\",
    \"model\": \"X-Trail\",
    \"trim\": \"S\",
    \"year\": 2022,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 5,
    \"seats\": 7,
    \"fuelType\": 0,
    \"engineSize\": \"2.5L\",
    \"horsepower\": 181,
    \"transmission\": 0,
    \"driveType\": 0,
    \"mileage\": 35000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Plata\",
    \"interiorColor\": \"Gris\",
    \"city\": \"La Romana\",
    \"state\": \"La Romana\",
    \"country\": \"DO\",
    \"sellerName\": \"Nissan La Romana\",
    \"sellerEmail\": \"info@nissanlr.do\",
    \"sellerPhone\": \"+18095557004\",
    \"isFeatured\": false
}" "2022 Nissan X-Trail S" "204"

create_and_publish_vehicle "{
    \"title\": \"2024 Kia Sportage LX - Nuevo Diseño\",
    \"description\": \"Kia Sportage completamente nuevo. Diseño futurista, pantalla curva de 12.3 pulgadas, asistente de conducción de nivel 2.\",
    \"price\": 1780000,
    \"currency\": \"DOP\",
    \"make\": \"Kia\",
    \"model\": \"Sportage\",
    \"trim\": \"LX\",
    \"year\": 2024,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 5,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"2.5L\",
    \"horsepower\": 187,
    \"transmission\": 0,
    \"driveType\": 0,
    \"mileage\": 0,
    \"mileageUnit\": 0,
    \"condition\": 0,
    \"exteriorColor\": \"Negro Midnight\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"Kia Motors RD\",
    \"sellerEmail\": \"ventas@kia.do\",
    \"sellerPhone\": \"+18095558005\",
    \"isFeatured\": false
}" "2024 Kia Sportage LX" "205"

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}          🛻 PICKUPS (3)                ${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

create_and_publish_vehicle "{
    \"title\": \"2024 Toyota Hilux SRV - Diésel 4x4\",
    \"description\": \"Toyota Hilux la pickup más vendida en RD. Motor diésel 2.8L turbo, caja automática, 4x4. Ideal para trabajo y aventura.\",
    \"price\": 3200000,
    \"currency\": \"DOP\",
    \"make\": \"Toyota\",
    \"model\": \"Hilux\",
    \"trim\": \"SRV\",
    \"year\": 2024,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 2,
    \"bodyStyle\": 2,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 1,
    \"engineSize\": \"2.8L Turbo Diesel\",
    \"horsepower\": 204,
    \"transmission\": 0,
    \"driveType\": 2,
    \"mileage\": 0,
    \"mileageUnit\": 0,
    \"condition\": 0,
    \"exteriorColor\": \"Gris Oscuro\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"Toyota Dominicana\",
    \"sellerEmail\": \"hilux@toyota.do\",
    \"sellerPhone\": \"+18095559001\",
    \"isFeatured\": true
}" "2024 Toyota Hilux SRV" "301"

create_and_publish_vehicle "{
    \"title\": \"2023 Ford Ranger XLT - Bi-Turbo\",
    \"description\": \"Ford Ranger nueva generación. Motor EcoBlue bi-turbo de 2.0L, tecnología SYNC 4, cámara 360. Capacidad de carga de 1 tonelada.\",
    \"price\": 2850000,
    \"currency\": \"DOP\",
    \"make\": \"Ford\",
    \"model\": \"Ranger\",
    \"trim\": \"XLT\",
    \"year\": 2023,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 2,
    \"bodyStyle\": 2,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 1,
    \"engineSize\": \"2.0L Bi-Turbo\",
    \"horsepower\": 210,
    \"transmission\": 0,
    \"driveType\": 2,
    \"mileage\": 15000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Azul Lightning\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santiago\",
    \"state\": \"Santiago\",
    \"country\": \"DO\",
    \"sellerName\": \"Ford Santiago\",
    \"sellerEmail\": \"ventas@fordsantiago.do\",
    \"sellerPhone\": \"+18095550002\",
    \"isFeatured\": false
}" "2023 Ford Ranger XLT" "302"

create_and_publish_vehicle "{
    \"title\": \"2021 Mitsubishi L200 GLX - Trabajo Duro\",
    \"description\": \"Mitsubishi L200 probada en las condiciones más difíciles. Motor diésel 2.4L, tracción 4x4 seleccionable. Excelente para trabajo pesado.\",
    \"price\": 1950000,
    \"currency\": \"DOP\",
    \"make\": \"Mitsubishi\",
    \"model\": \"L200\",
    \"trim\": \"GLX\",
    \"year\": 2021,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 2,
    \"bodyStyle\": 2,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 1,
    \"engineSize\": \"2.4L Turbo Diesel\",
    \"horsepower\": 181,
    \"transmission\": 1,
    \"driveType\": 2,
    \"mileage\": 65000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Blanco\",
    \"interiorColor\": \"Gris\",
    \"city\": \"San Pedro de Macorís\",
    \"state\": \"San Pedro de Macorís\",
    \"country\": \"DO\",
    \"sellerName\": \"Mitsubishi SPM\",
    \"sellerEmail\": \"ventas@mitsuspm.do\",
    \"sellerPhone\": \"+18095551003\",
    \"isFeatured\": false
}" "2021 Mitsubishi L200 GLX" "303"

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}        🏔️ TODOTERRENOS (2)             ${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

create_and_publish_vehicle "{
    \"title\": \"2024 Toyota Land Cruiser Prado TX - Lujo Off-Road\",
    \"description\": \"Land Cruiser Prado, el SUV de lujo todoterreno por excelencia. Motor V6 de 4.0L, sistema KDSS, 7 asientos. Legendaria confiabilidad Toyota.\",
    \"price\": 4800000,
    \"currency\": \"DOP\",
    \"make\": \"Toyota\",
    \"model\": \"Land Cruiser Prado\",
    \"trim\": \"TX\",
    \"year\": 2024,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 5,
    \"seats\": 7,
    \"fuelType\": 0,
    \"engineSize\": \"4.0L V6\",
    \"horsepower\": 270,
    \"transmission\": 0,
    \"driveType\": 2,
    \"mileage\": 0,
    \"mileageUnit\": 0,
    \"condition\": 0,
    \"exteriorColor\": \"Blanco Perla\",
    \"interiorColor\": \"Beige\",
    \"city\": \"Santo Domingo\",
    \"state\": \"DN\",
    \"country\": \"DO\",
    \"sellerName\": \"Toyota Premium RD\",
    \"sellerEmail\": \"premium@toyota.do\",
    \"sellerPhone\": \"+18095552001\",
    \"isFeatured\": true
}" "2024 Toyota Land Cruiser Prado TX" "401"

create_and_publish_vehicle "{
    \"title\": \"2023 Jeep Wrangler Rubicon - Aventura Total\",
    \"description\": \"Jeep Wrangler Rubicon el todoterreno más capaz del mercado. Ejes Dana 44, diferenciales bloqueables, techo removible. Vive la aventura.\",
    \"price\": 4200000,
    \"currency\": \"DOP\",
    \"make\": \"Jeep\",
    \"model\": \"Wrangler\",
    \"trim\": \"Rubicon\",
    \"year\": 2023,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 1,
    \"bodyStyle\": 1,
    \"doors\": 4,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"3.6L V6\",
    \"horsepower\": 285,
    \"transmission\": 0,
    \"driveType\": 2,
    \"mileage\": 12000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Verde Sarge\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Jarabacoa\",
    \"state\": \"La Vega\",
    \"country\": \"DO\",
    \"sellerName\": \"Jeep Adventure RD\",
    \"sellerEmail\": \"adventure@jeep.do\",
    \"sellerPhone\": \"+18095553002\",
    \"isFeatured\": true
}" "2023 Jeep Wrangler Rubicon" "402"

echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo -e "${BLUE}         💰 ECONÓMICOS (2)              ${NC}"
echo -e "${BLUE}═══════════════════════════════════════${NC}"
echo ""

create_and_publish_vehicle "{
    \"title\": \"2023 Suzuki Swift GL - Citadino Ideal\",
    \"description\": \"Suzuki Swift compacto y eficiente. Perfecto para la ciudad, bajo consumo, fácil de estacionar. Pantalla táctil, cámara de reversa.\",
    \"price\": 850000,
    \"currency\": \"DOP\",
    \"make\": \"Suzuki\",
    \"model\": \"Swift\",
    \"trim\": \"GL\",
    \"year\": 2023,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 0,
    \"bodyStyle\": 3,
    \"doors\": 5,
    \"seats\": 5,
    \"fuelType\": 0,
    \"engineSize\": \"1.2L\",
    \"horsepower\": 83,
    \"transmission\": 0,
    \"driveType\": 0,
    \"mileage\": 18000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Rojo\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Santo Domingo Este\",
    \"state\": \"Santo Domingo\",
    \"country\": \"DO\",
    \"sellerName\": \"Suzuki RD\",
    \"sellerEmail\": \"ventas@suzuki.do\",
    \"sellerPhone\": \"+18095554001\",
    \"isFeatured\": false
}" "2023 Suzuki Swift GL" "501"

create_and_publish_vehicle "{
    \"title\": \"2022 Kia Picanto EX - Primer Auto\",
    \"description\": \"Kia Picanto ideal para primer auto o segundo vehículo. Económico, confiable, bajo costo de mantenimiento. Garantía de fábrica vigente.\",
    \"price\": 720000,
    \"currency\": \"DOP\",
    \"make\": \"Kia\",
    \"model\": \"Picanto\",
    \"trim\": \"EX\",
    \"year\": 2022,
    \"vin\": \"VIN_PLACEHOLDER\",
    \"vehicleType\": 0,
    \"bodyStyle\": 3,
    \"doors\": 5,
    \"seats\": 4,
    \"fuelType\": 0,
    \"engineSize\": \"1.0L\",
    \"horsepower\": 67,
    \"transmission\": 1,
    \"driveType\": 0,
    \"mileage\": 28000,
    \"mileageUnit\": 0,
    \"condition\": 1,
    \"exteriorColor\": \"Amarillo\",
    \"interiorColor\": \"Negro\",
    \"city\": \"Los Alcarrizos\",
    \"state\": \"Santo Domingo\",
    \"country\": \"DO\",
    \"sellerName\": \"Kia Usados RD\",
    \"sellerEmail\": \"usados@kia.do\",
    \"sellerPhone\": \"+18095555002\",
    \"isFeatured\": false
}" "2022 Kia Picanto EX" "502"

# =====================================================
# RESUMEN
# =====================================================

echo ""
echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                    RESUMEN                                 ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Contar vehículos creados
count=$(curl -s "${API_URL}/api/vehicles?pageSize=1" | grep -o '"totalCount":[0-9]*' | cut -d: -f2)

echo -e "${GREEN}✅ Vehículos activos en el sistema: ${count}${NC}"
echo ""
echo -e "${CYAN}Categorías:${NC}"
echo -e "  • Sedanes:      3 vehículos"
echo -e "  • SUVs:         5 vehículos"
echo -e "  • Pickups:      3 vehículos"
echo -e "  • Todoterrenos: 2 vehículos"
echo -e "  • Económicos:   2 vehículos"
echo -e "  ─────────────────────────"
echo -e "  Total:         15 vehículos"
echo ""
echo -e "${GREEN}✅ Seed de vehículos completado${NC}"
