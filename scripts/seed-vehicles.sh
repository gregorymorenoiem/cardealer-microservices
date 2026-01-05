#!/bin/bash
#
# Script para crear vehículos de prueba para dealers con imágenes de Unsplash
#
# Uso:
#   ./seed-vehicles.sh           # Procesa el primer dealer (prueba)
#   ./seed-vehicles.sh 1         # Procesa solo el dealer #1
#   ./seed-vehicles.sh 0         # Procesa TODOS los dealers
#
# Cada dealer recibe 5 vehículos con 5 imágenes cada uno

set -e

DEALER_INDEX=${1:-1}
VEHICLES_PER_DEALER=5
IMAGES_PER_VEHICLE=5

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo ""
echo -e "${MAGENTA}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${MAGENTA}  🚗 SEED VEHICLES FOR DEALERS - CarDealer Microservices${NC}"
echo -e "${MAGENTA}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# ============================================================================
# PASO 1: Obtener dealers de UserService
# ============================================================================
echo -e "${CYAN}ℹ️  Obteniendo dealers de UserService...${NC}"

DEALERS_RAW=$(docker exec userservice-db psql -U postgres -d userservice -t -A -F '|' -c '
SELECT "Id", "BusinessName", "TradeName", "Email", "Phone", "Address", "City", "State", "Latitude", "Longitude"
FROM "Dealers" 
WHERE "IsDeleted" = false AND "IsActive" = true
ORDER BY "BusinessName"
')

# Convertir a array
IFS=$'\n' read -r -d '' -a DEALERS_ARRAY <<< "$DEALERS_RAW" || true

DEALER_COUNT=${#DEALERS_ARRAY[@]}
echo -e "${GREEN}✅ Encontrados $DEALER_COUNT dealers${NC}"

# ============================================================================
# PASO 2: Obtener catálogo de VehiclesSaleService
# ============================================================================
echo -e "${CYAN}ℹ️  Obteniendo catálogo de vehículos...${NC}"

CATALOG_RAW=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -F '|' -c '
SELECT 
    m."Id", m."Name", m."Slug",
    mo."Id", mo."Name", mo."Slug", mo."VehicleType", COALESCE(mo."DefaultBodyStyle", '\''Sedan'\''),
    t."Id", t."Name", t."Year", t."BaseMSRP", t."EngineSize", t."Horsepower", t."Torque",
    t."FuelType", t."Transmission", t."DriveType", t."MpgCity", t."MpgHighway", t."MpgCombined"
FROM vehicle_makes m 
JOIN vehicle_models mo ON mo."MakeId" = m."Id" 
JOIN vehicle_trims t ON t."ModelId" = mo."Id" 
WHERE t."IsActive" = true
ORDER BY RANDOM()
')

IFS=$'\n' read -r -d '' -a CATALOG_ARRAY <<< "$CATALOG_RAW" || true
CATALOG_COUNT=${#CATALOG_ARRAY[@]}
echo -e "${GREEN}✅ Encontrados $CATALOG_COUNT trims en el catálogo${NC}"

# ============================================================================
# PASO 3: Obtener categorías
# ============================================================================
echo -e "${CYAN}ℹ️  Obteniendo categorías...${NC}"

CARS_CAT=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c 'SELECT "Id" FROM categories WHERE "Name" = '\''Cars'\''')
TRUCKS_CAT=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c 'SELECT "Id" FROM categories WHERE "Name" = '\''Trucks'\''')
SUVS_CAT=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c 'SELECT "Id" FROM categories WHERE "Name" = '\''SUVs & Crossovers'\''')

echo -e "${GREEN}✅ Categorías obtenidas${NC}"

# ============================================================================
# FUNCIONES AUXILIARES
# ============================================================================

generate_uuid() {
    uuidgen | tr '[:upper:]' '[:lower:]'
}

random_number() {
    local min=$1
    local max=$2
    echo $(( RANDOM % (max - min + 1) + min ))
}

random_color() {
    local colors=("Black" "White" "Silver" "Gray" "Red" "Blue" "Pearl White" "Midnight Black" "Ocean Blue" "Burgundy")
    echo "${colors[$RANDOM % ${#colors[@]}]}"
}

random_interior_color() {
    local colors=("Black" "Beige" "Gray" "Brown" "Tan" "Cream")
    echo "${colors[$RANDOM % ${#colors[@]}]}"
}

random_interior_material() {
    local materials=("Leather" "Cloth" "Leatherette" "Premium Leather" "Synthetic")
    echo "${materials[$RANDOM % ${#materials[@]}]}"
}

random_condition() {
    local rand=$(random_number 1 100)
    if [ $rand -le 30 ]; then
        echo "New"
    elif [ $rand -le 70 ]; then
        echo "Used"
    else
        echo "Certified"
    fi
}

get_mileage() {
    local condition=$1
    if [ "$condition" = "New" ]; then
        echo $(random_number 0 500)
    else
        echo $(random_number 5000 80000)
    fi
}

get_category_id() {
    local vehicle_type=$1
    case $vehicle_type in
        *Truck*|*Pickup*) echo "$TRUCKS_CAT" ;;
        *SUV*|*Crossover*) echo "$SUVS_CAT" ;;
        *) echo "$CARS_CAT" ;;
    esac
}

generate_vin() {
    local chars="ABCDEFGHJKLMNPRSTUVWXYZ0123456789"
    local vin="1"
    for i in {1..16}; do
        vin+="${chars:RANDOM%${#chars}:1}"
    done
    echo "$vin"
}

get_features_json() {
    local features=(
        "Bluetooth" "Apple CarPlay" "Android Auto" "Navigation" "Backup Camera"
        "Blind Spot Monitor" "Lane Departure Warning" "Adaptive Cruise Control"
        "Heated Seats" "Sunroof" "LED Headlights" "Keyless Entry" "Push Button Start"
    )
    local count=$(random_number 5 10)
    local json="["
    local first=true
    local used=()
    
    # Pick random features without shuf
    while [ ${#used[@]} -lt $count ]; do
        local idx=$((RANDOM % ${#features[@]}))
        local already_used=false
        for u in "${used[@]}"; do
            if [ "$u" -eq "$idx" ]; then
                already_used=true
                break
            fi
        done
        if [ "$already_used" = false ]; then
            used+=($idx)
            if [ "$first" = true ]; then
                first=false
            else
                json+=","
            fi
            json+="\"${features[$idx]}\""
        fi
    done
    json+="]"
    echo "$json"
}

# ============================================================================
# PASO 4: Procesar dealers
# ============================================================================

TOTAL_VEHICLES=0
TOTAL_IMAGES=0

# Determinar qué dealers procesar
if [ "$DEALER_INDEX" -eq 0 ]; then
    START_IDX=0
    END_IDX=$((DEALER_COUNT - 1))
    echo -e "${CYAN}ℹ️  Procesando TODOS los $DEALER_COUNT dealers${NC}"
else
    START_IDX=$((DEALER_INDEX - 1))
    END_IDX=$((DEALER_INDEX - 1))
    echo -e "${CYAN}ℹ️  Procesando solo dealer #$DEALER_INDEX${NC}"
fi

for dealer_idx in $(seq $START_IDX $END_IDX); do
    DEALER_LINE="${DEALERS_ARRAY[$dealer_idx]}"
    
    if [ -z "$DEALER_LINE" ]; then
        continue
    fi
    
    # Parsear dealer
    IFS='|' read -r D_ID D_BUSINESS D_TRADE D_EMAIL D_PHONE D_ADDRESS D_CITY D_STATE D_LAT D_LON <<< "$DEALER_LINE"
    
    echo ""
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}  🏢 Dealer: $D_BUSINESS${NC}"
    echo -e "${BLUE}  📍 $D_CITY, $D_STATE${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    
    # Seleccionar trims aleatorios (compatible con macOS - sin shuf)
    SHUFFLED_CATALOG=()
    USED_INDICES=()
    while [ ${#SHUFFLED_CATALOG[@]} -lt $VEHICLES_PER_DEALER ] && [ ${#SHUFFLED_CATALOG[@]} -lt $CATALOG_COUNT ]; do
        rand_idx=$((RANDOM % CATALOG_COUNT))
        already_used=false
        for u in "${USED_INDICES[@]}"; do
            if [ "$u" -eq "$rand_idx" ]; then
                already_used=true
                break
            fi
        done
        if [ "$already_used" = false ]; then
            USED_INDICES+=($rand_idx)
            SHUFFLED_CATALOG+=($rand_idx)
        fi
    done
    
    vehicle_num=0
    for catalog_idx in "${SHUFFLED_CATALOG[@]}"; do
        ((vehicle_num++))
        CATALOG_LINE="${CATALOG_ARRAY[$catalog_idx]}"
        
        if [ -z "$CATALOG_LINE" ]; then
            continue
        fi
        
        # Parsear catálogo
        IFS='|' read -r MAKE_ID MAKE MAKE_SLUG MODEL_ID MODEL MODEL_SLUG VEHICLE_TYPE BODY_STYLE \
                       TRIM_ID TRIM YEAR BASE_MSRP ENGINE_SIZE HORSEPOWER TORQUE \
                       FUEL_TYPE TRANSMISSION DRIVE_TYPE MPG_CITY MPG_HIGHWAY MPG_COMBINED <<< "$CATALOG_LINE"
        
        # Generar datos del vehículo
        VEHICLE_ID=$(generate_uuid)
        CONDITION=$(random_condition)
        MILEAGE=$(get_mileage "$CONDITION")
        EXT_COLOR=$(random_color)
        INT_COLOR=$(random_interior_color)
        INT_MATERIAL=$(random_interior_material)
        CATEGORY_ID=$(get_category_id "$VEHICLE_TYPE")
        VIN=$(generate_vin)
        STOCK_NUM="STK-$(random_number 10000 99999)"
        
        # Precio con variación
        PRICE_VAR=$(random_number -3000 5000)
        BASE_PRICE=${BASE_MSRP:-35000}
        PRICE=$(echo "$BASE_PRICE + $PRICE_VAR" | bc)
        if (( $(echo "$PRICE < 15000" | bc -l) )); then
            PRICE=15000
        fi
        
        # Valores por defecto
        HORSEPOWER=${HORSEPOWER:-200}
        TORQUE=${TORQUE:-200}
        MPG_CITY=${MPG_CITY:-25}
        MPG_HIGHWAY=${MPG_HIGHWAY:-32}
        MPG_COMBINED=${MPG_COMBINED:-28}
        ENGINE_SIZE=${ENGINE_SIZE:-2.0L}
        FUEL_TYPE=${FUEL_TYPE:-Gasoline}
        TRANSMISSION=${TRANSMISSION:-Automatic}
        DRIVE_TYPE=${DRIVE_TYPE:-FWD}
        VEHICLE_TYPE=${VEHICLE_TYPE:-Car}
        BODY_STYLE=${BODY_STYLE:-Sedan}
        
        TITLE="$YEAR $MAKE $MODEL $TRIM"
        DESCRIPTION="Hermoso $YEAR $MAKE $MODEL $TRIM en excelentes condiciones. Motor $ENGINE_SIZE con $HORSEPOWER HP. Transmision $TRANSMISSION, traccion $DRIVE_TYPE. Color exterior $EXT_COLOR con interior $INT_COLOR en $INT_MATERIAL. Consumo: $MPG_CITY ciudad / $MPG_HIGHWAY carretera MPG. Disponible en $D_TRADE, $D_CITY."
        
        # Escapar comillas simples
        TITLE_ESC=$(echo "$TITLE" | sed "s/'/''/g")
        DESC_ESC=$(echo "$DESCRIPTION" | sed "s/'/''/g")
        D_TRADE_ESC=$(echo "$D_TRADE" | sed "s/'/''/g")
        
        FEATURES_JSON=$(get_features_json)
        
        IS_CERTIFIED="false"
        if [ "$CONDITION" = "Certified" ]; then
            IS_CERTIFIED="true"
        fi
        
        IS_FEATURED="false"
        if [ $(random_number 1 10) -le 3 ]; then
            IS_FEATURED="true"
        fi
        
        PREV_OWNERS=$(random_number 0 3)
        VIEW_COUNT=$(random_number 50 500)
        FAV_COUNT=$(random_number 5 50)
        INQ_COUNT=$(random_number 1 20)
        
        echo -e "${CYAN}  ℹ️  Creando vehículo $vehicle_num/$VEHICLES_PER_DEALER: $TITLE${NC}"
        
        # SQL para insertar vehículo
        VEHICLE_SQL="INSERT INTO vehicles (
            \"Id\", \"DealerId\", \"Title\", \"Description\", \"Price\", \"Currency\", \"Status\",
            \"SellerId\", \"SellerName\", \"SellerType\", \"SellerPhone\", \"SellerEmail\", \"SellerVerified\", \"SellerCity\", \"SellerState\",
            \"VIN\", \"StockNumber\", \"MakeId\", \"Make\", \"ModelId\", \"Model\", \"Trim\", \"Year\",
            \"VehicleType\", \"BodyStyle\", \"Doors\", \"Seats\", \"FuelType\", \"EngineSize\", \"Horsepower\", \"Torque\",
            \"Transmission\", \"DriveType\", \"Mileage\", \"MileageUnit\", \"Condition\",
            \"ExteriorColor\", \"InteriorColor\", \"InteriorMaterial\",
            \"MpgCity\", \"MpgHighway\", \"MpgCombined\",
            \"City\", \"State\", \"Country\", \"Latitude\", \"Longitude\",
            \"IsCertified\", \"HasCleanTitle\", \"AccidentHistory\", \"PreviousOwners\",
            \"FeaturesJson\", \"PackagesJson\", \"ViewCount\", \"FavoriteCount\", \"InquiryCount\",
            \"CreatedAt\", \"UpdatedAt\", \"PublishedAt\", \"IsDeleted\", \"IsFeatured\", \"CategoryId\"
        ) VALUES (
            '$VEHICLE_ID', '$D_ID', '$TITLE_ESC', '$DESC_ESC', 
            $PRICE, 'USD', 'Active',
            '$D_ID', '$D_TRADE_ESC', 1, '$D_PHONE', '$D_EMAIL', true, '$D_CITY', '$D_STATE',
            '$VIN', '$STOCK_NUM', '$MAKE_ID', '$MAKE', '$MODEL_ID', '$MODEL', '$TRIM', $YEAR,
            '$VEHICLE_TYPE', '$BODY_STYLE', 4, 5, '$FUEL_TYPE', '$ENGINE_SIZE', $HORSEPOWER, $TORQUE,
            '$TRANSMISSION', '$DRIVE_TYPE', $MILEAGE, 'Miles', '$CONDITION',
            '$EXT_COLOR', '$INT_COLOR', '$INT_MATERIAL',
            $MPG_CITY, $MPG_HIGHWAY, $MPG_COMBINED,
            '$D_CITY', '$D_STATE', 'DO', $D_LAT, $D_LON,
            $IS_CERTIFIED, true, false, $PREV_OWNERS,
            '$FEATURES_JSON', '[]', $VIEW_COUNT, $FAV_COUNT, $INQ_COUNT,
            NOW(), NOW(), NOW(), false, $IS_FEATURED, '$CATEGORY_ID'
        );"
        
        if docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -c "$VEHICLE_SQL" > /dev/null 2>&1; then
            ((TOTAL_VEHICLES++))
            
            # Crear imágenes
            for img_idx in $(seq 1 $IMAGES_PER_VEHICLE); do
                IMAGE_ID=$(generate_uuid)
                SIG=$(generate_uuid | cut -c1-8)
                SEARCH_MAKE=$(echo "$MAKE" | tr ' ' '-' | tr -cd '[:alnum:]-')
                SEARCH_MODEL=$(echo "$MODEL" | tr ' ' '-' | tr -cd '[:alnum:]-')
                
                IMAGE_URL="https://source.unsplash.com/800x600/?${SEARCH_MAKE},${SEARCH_MODEL},car&sig=${SIG}"
                THUMB_URL="https://source.unsplash.com/400x300/?${SEARCH_MAKE},${SEARCH_MODEL},car&sig=${SIG}t"
                
                IS_PRIMARY="false"
                CAPTION="Vista adicional"
                if [ $img_idx -eq 1 ]; then
                    IS_PRIMARY="true"
                    CAPTION="Vista principal"
                elif [ $img_idx -eq 2 ]; then
                    CAPTION="Interior"
                elif [ $img_idx -eq 3 ]; then
                    CAPTION="Vista lateral"
                elif [ $img_idx -eq 4 ]; then
                    CAPTION="Motor"
                fi
                
                IMAGE_SQL="INSERT INTO vehicle_images (
                    \"Id\", \"DealerId\", \"VehicleId\", \"Url\", \"ThumbnailUrl\", \"Caption\",
                    \"ImageType\", \"SortOrder\", \"IsPrimary\", \"CreatedAt\"
                ) VALUES (
                    '$IMAGE_ID', '$D_ID', '$VEHICLE_ID',
                    '$IMAGE_URL', '$THUMB_URL', '$CAPTION',
                    0, $img_idx, $IS_PRIMARY, NOW()
                );"
                
                if docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -c "$IMAGE_SQL" > /dev/null 2>&1; then
                    ((TOTAL_IMAGES++))
                fi
            done
            
            echo -e "${GREEN}    ✓ Vehículo creado con $IMAGES_PER_VEHICLE imágenes${NC}"
        else
            echo -e "${RED}    ✗ Error creando vehículo${NC}"
        fi
    done
done

# ============================================================================
# RESUMEN FINAL
# ============================================================================
echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  ✅ SEED COMPLETADO${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "  📊 Resumen:"
echo -e "     • Dealers procesados: $((END_IDX - START_IDX + 1))"
echo -e "     • Vehículos creados:  $TOTAL_VEHICLES"
echo -e "     • Imágenes creadas:   $TOTAL_IMAGES"
echo ""

# Verificar conteo final
VEHICLE_COUNT=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -c 'SELECT COUNT(*) FROM vehicles WHERE "IsDeleted" = false')
IMAGE_COUNT=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -c 'SELECT COUNT(*) FROM vehicle_images')

echo -e "  🚗 Total vehículos en BD: ${CYAN}$(echo $VEHICLE_COUNT | tr -d ' ')${NC}"
echo -e "  🖼️  Total imágenes en BD:  ${CYAN}$(echo $IMAGE_COUNT | tr -d ' ')${NC}"
echo ""
