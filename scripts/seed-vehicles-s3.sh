#!/bin/bash
# ═══════════════════════════════════════════════════════════════════════════
# SEED VEHICLES WITH S3 IMAGES
# 
# Este script:
# 1. Descarga fotos de carros de Unsplash
# 2. Las sube a AWS S3
# 3. Crea vehículos en la BD con el photoId (solo nombre, sin extensión)
#
# USO:
#   ./seed-vehicles-s3.sh         # Solo primer dealer (modo prueba)
#   ./seed-vehicles-s3.sh --all   # Todos los dealers
# ═══════════════════════════════════════════════════════════════════════════

set -e

# Modo de ejecución
TEST_MODE=true
if [[ "$1" == "--all" ]]; then
    TEST_MODE=false
fi

# Configuración
S3_BUCKET="okla-images-2026"
S3_REGION="us-east-2"
S3_PATH="frontend/assets/vehicles/sale"
TEMP_DIR="/tmp/vehicle-images"
VEHICLES_PER_DEALER=5
IMAGES_PER_VEHICLE=5

# IDs de fotos de CARROS verificados de Unsplash (alta calidad)
# Fuente: https://unsplash.com/s/photos/car
CAR_PHOTO_IDS=(
    # Sedanes y deportivos
    "photo-1494976388531-d1058494cdd8"   # BMW rojo
    "photo-1503376780353-7e6692767b70"   # Porsche 911
    "photo-1555215695-3004980ad54e"      # Mercedes AMG GT
    "photo-1544636331-e26879cd4d9b"      # Audi R8
    "photo-1492144534655-ae79c964c9d7"   # BMW i8
    "photo-1552519507-da3b142c6e3d"      # Lamborghini
    "photo-1525609004556-c46c7d6cf023"   # Porsche azul
    "photo-1542362567-b07e54358753"      # Ferrari rojo
    "photo-1553440569-bcc63803a83d"      # Mercedes blanco
    "photo-1580273916550-e323be2ae537"   # BMW M4
    "photo-1618843479313-40f8afb4b4d8"   # Audi gris
    "photo-1583121274602-3e2820c69888"   # Honda Civic Type R
    "photo-1606664515524-ed2f786a0bd6"   # Mustang GT
    "photo-1612825173281-9a193378527e"   # Mercedes E-Class
    "photo-1617814076367-b759c7d7e738"   # BMW Serie 3
    # SUVs y Crossovers
    "photo-1519641471654-76ce0107ad1b"   # Range Rover
    "photo-1606016159991-dfe4f2746ad5"   # Mercedes GLE
    "photo-1605559424843-9e4c228bf1c2"   # Porsche Cayenne
    "photo-1603584173870-7f23fdae1b7a"   # Audi Q7
    "photo-1621135802920-133df287f89c"   # Tesla Model X
    "photo-1533473359331-0135ef1b58bf"   # Jeep Wrangler
    "photo-1519245659620-e859806a8d3b"   # Land Cruiser
    # Pickups
    "photo-1559416523-140ddc3d238c"      # Ford F-150
    "photo-1558618666-fcd25c85cd64"      # RAM 1500
    "photo-1612544448445-b8232cff3b6c"   # Chevrolet Silverado
    "photo-1590362891991-f776e747a588"   # Toyota Tacoma
    # Clásicos y especiales
    "photo-1536700503339-1e4b06520771"   # Ford Mustang clasico
    "photo-1547744152-14d985cb937f"      # Chevrolet Camaro
    "photo-1549317661-bd32c8ce0db2"      # Dodge Challenger
    "photo-1619767886558-efdc259cde1a"   # Tesla Model 3
    "photo-1617788138017-80ad40651399"   # Audi e-tron
    # Más variedad
    "photo-1494905998402-395d579af36f"   # Mercedes AMG
    "photo-1551830820-330a71b99659"      # Volkswagen GTI
    "photo-1560958089-b8a1929cea89"      # Hyundai Veloster
    "photo-1561580125-028ee3bd62eb"      # BMW M5
    "photo-1563720223185-11003d516935"   # Lexus LC
    "photo-1621007947382-bb3c3994e3fb"   # Mazda MX-5
    "photo-1616455579100-2ceaa4eb2d37"   # Toyota Supra
    "photo-1609521263047-f8f205293f24"   # Honda NSX
    "photo-1614162692292-7ac56d7f702e"   # Nissan GT-R
)

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}"
echo "═══════════════════════════════════════════════════════════════"
echo "  🚗 SEED VEHICLES WITH S3 IMAGES - CarDealer Microservices"
echo "═══════════════════════════════════════════════════════════════"
echo -e "${NC}"

# Crear directorio temporal
mkdir -p "$TEMP_DIR"

# ═══════════════════════════════════════════════════════════════════════════
# PASO 1: Descargar fotos de Unsplash
# ═══════════════════════════════════════════════════════════════════════════
echo -e "${YELLOW}📥 Paso 1: Descargando fotos de Unsplash...${NC}"

DOWNLOADED_PHOTOS=()
DOWNLOAD_COUNT=0
TOTAL_NEEDED=100  # 20 dealers × 5 vehículos = 100, pero reutilizaremos

for photo_id in "${CAR_PHOTO_IDS[@]}"; do
    OUTPUT_FILE="$TEMP_DIR/${photo_id}.jpg"
    
    if [[ -f "$OUTPUT_FILE" ]]; then
        echo "  ⏭️  $photo_id (ya existe)"
        DOWNLOADED_PHOTOS+=("$photo_id")
        continue
    fi
    
    # Descargar de Unsplash (800x600)
    UNSPLASH_URL="https://images.unsplash.com/${photo_id}?w=800&h=600&fit=crop&auto=format"
    
    echo -n "  📷 Descargando $photo_id... "
    
    if curl -sL "$UNSPLASH_URL" -o "$OUTPUT_FILE" 2>/dev/null; then
        # Verificar que no sea un error HTML
        FILE_SIZE=$(stat -f%z "$OUTPUT_FILE" 2>/dev/null || stat -c%s "$OUTPUT_FILE" 2>/dev/null)
        if [[ $FILE_SIZE -gt 10000 ]]; then
            echo -e "${GREEN}✓${NC} (${FILE_SIZE} bytes)"
            DOWNLOADED_PHOTOS+=("$photo_id")
            ((DOWNLOAD_COUNT++))
        else
            echo -e "${RED}✗ (archivo muy pequeño)${NC}"
            rm -f "$OUTPUT_FILE"
        fi
    else
        echo -e "${RED}✗ (error de descarga)${NC}"
    fi
    
    # Rate limiting
    sleep 0.3
done

echo -e "${GREEN}✅ ${#DOWNLOADED_PHOTOS[@]} fotos descargadas${NC}"

if [[ ${#DOWNLOADED_PHOTOS[@]} -lt 10 ]]; then
    echo -e "${RED}❌ No hay suficientes fotos. Necesitas al menos 10.${NC}"
    exit 1
fi

# ═══════════════════════════════════════════════════════════════════════════
# PASO 2: Subir fotos a S3
# ═══════════════════════════════════════════════════════════════════════════
echo -e "\n${YELLOW}☁️  Paso 2: Subiendo fotos a S3...${NC}"

UPLOADED_COUNT=0
for photo_id in "${DOWNLOADED_PHOTOS[@]}"; do
    LOCAL_FILE="$TEMP_DIR/${photo_id}.jpg"
    S3_KEY="${S3_PATH}/${photo_id}.jpg"
    
    echo -n "  ⬆️  $photo_id... "
    
    if aws s3 cp "$LOCAL_FILE" "s3://${S3_BUCKET}/${S3_KEY}" --quiet 2>/dev/null; then
        echo -e "${GREEN}✓${NC}"
        ((UPLOADED_COUNT++))
    else
        echo -e "${RED}✗${NC}"
    fi
done

echo -e "${GREEN}✅ ${UPLOADED_COUNT} fotos subidas a S3${NC}"

# Verificar
echo -e "\n${BLUE}🔍 Verificando en S3:${NC}"
aws s3 ls "s3://${S3_BUCKET}/${S3_PATH}/" | wc -l | xargs -I {} echo "  Total archivos en S3: {}"

# ═══════════════════════════════════════════════════════════════════════════
# PASO 3: Crear vehículos en la BD
# ═══════════════════════════════════════════════════════════════════════════
echo -e "\n${YELLOW}🚗 Paso 3: Creando vehículos en la base de datos...${NC}"

# Obtener dealers
echo -e "  ℹ️  Obteniendo dealers..."
DEALERS_JSON=$(docker exec userservice-db psql -U postgres -d userservice -t -A -c "
    SELECT json_agg(json_build_object(
        'id', d.\"Id\",
        'company', d.\"BusinessName\",
        'city', d.\"City\",
        'lat', COALESCE(d.\"Latitude\", 18.4861),
        'lng', COALESCE(d.\"Longitude\", -69.9312)
    ))
    FROM \"Dealers\" d 
    WHERE d.\"IsActive\" = true 
    AND d.\"IsDeleted\" = false;
")

DEALER_COUNT=$(echo "$DEALERS_JSON" | jq 'length')
echo -e "  ${GREEN}✅ $DEALER_COUNT dealers encontrados${NC}"

# Obtener catálogo de vehículos
echo -e "  ℹ️  Obteniendo catálogo de vehículos..."
CATALOG_JSON=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c "
    SELECT json_agg(json_build_object(
        'trim_id', t.\"Id\",
        'make', ma.\"Name\",
        'model', mo.\"Name\",
        'trim', t.\"Name\",
        'year', t.\"Year\",
        'msrp', COALESCE(t.\"BaseMSRP\", 35000),
        'body_style', COALESCE(mo.\"DefaultBodyStyle\", 'Sedan'),
        'engine', COALESCE(t.\"EngineSize\", '2.0L 4-Cylinder'),
        'horsepower', COALESCE(t.\"Horsepower\", 200),
        'transmission', COALESCE(t.\"Transmission\", 'Automatic'),
        'drivetrain', COALESCE(t.\"DriveType\", 'FWD'),
        'fuel_type', COALESCE(t.\"FuelType\", 'Gasoline'),
        'mpg_city', COALESCE(t.\"MpgCity\", 25),
        'mpg_highway', COALESCE(t.\"MpgHighway\", 32)
    ))
    FROM vehicle_trims t
    JOIN vehicle_models mo ON t.\"ModelId\" = mo.\"Id\"
    JOIN vehicle_makes ma ON mo.\"MakeId\" = ma.\"Id\"
    WHERE t.\"IsActive\" = true;
")

TRIM_COUNT=$(echo "$CATALOG_JSON" | jq 'length')
echo -e "  ${GREEN}✅ $TRIM_COUNT trims en catálogo${NC}"

# Obtener categorías
CATEGORIES_JSON=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c "
    SELECT json_agg(json_build_object('id', \"Id\", 'name', \"Name\"))
    FROM categories WHERE \"IsActive\" = true;
")

# Función para obtener ID de categoría
get_category_id() {
    local body_style="$1"
    case "$body_style" in
        "Truck"|"Pickup") echo "$CATEGORIES_JSON" | jq -r '.[] | select(.name == "Trucks") | .id' ;;
        "SUV"|"Crossover") echo "$CATEGORIES_JSON" | jq -r '.[] | select(.name == "SUVs & Crossovers") | .id' ;;
        *) echo "$CATEGORIES_JSON" | jq -r '.[] | select(.name == "Cars") | .id' ;;
    esac
}

# Contadores
TOTAL_VEHICLES=0
TOTAL_IMAGES=0

# Array de photoIds para rotar
PHOTO_ARRAY=("${DOWNLOADED_PHOTOS[@]}")
PHOTO_INDEX=0

# Determinar cuántos dealers procesar
if [[ "$TEST_MODE" == true ]]; then
    DEALERS_TO_PROCESS=1
    echo -e "\n  ${YELLOW}⚠️  MODO PRUEBA: Solo se procesará el primer dealer${NC}"
    echo -e "  ${YELLOW}   Ejecuta con --all para procesar todos${NC}\n"
else
    DEALERS_TO_PROCESS=$DEALER_COUNT
    echo -e "\n  ${GREEN}✓ Procesando TODOS los $DEALER_COUNT dealers${NC}\n"
fi

# Procesar cada dealer
for ((d=0; d<$DEALERS_TO_PROCESS; d++)); do
    DEALER=$(echo "$DEALERS_JSON" | jq -r ".[$d]")
    DEALER_ID=$(echo "$DEALER" | jq -r '.id')
    DEALER_NAME=$(echo "$DEALER" | jq -r '.company')
    DEALER_CITY=$(echo "$DEALER" | jq -r '.city')
    DEALER_LAT=$(echo "$DEALER" | jq -r '.lat')
    DEALER_LNG=$(echo "$DEALER" | jq -r '.lng')
    
    echo -e "\n  ${BLUE}🏢 Dealer $((d+1))/$DEALERS_TO_PROCESS: $DEALER_NAME ($DEALER_CITY)${NC}"
    
    # Crear 5 vehículos para este dealer
    for ((v=0; v<$VEHICLES_PER_DEALER; v++)); do
        # Seleccionar trim aleatorio
        TRIM_INDEX=$((RANDOM % TRIM_COUNT))
        TRIM=$(echo "$CATALOG_JSON" | jq -r ".[$TRIM_INDEX]")
        
        MAKE=$(echo "$TRIM" | jq -r '.make')
        MODEL=$(echo "$TRIM" | jq -r '.model')
        TRIM_NAME=$(echo "$TRIM" | jq -r '.trim')
        YEAR=$(echo "$TRIM" | jq -r '.year')
        MSRP_RAW=$(echo "$TRIM" | jq -r '.msrp')
        MSRP=${MSRP_RAW%.*}  # Remover decimales
        BODY_STYLE=$(echo "$TRIM" | jq -r '.body_style')
        ENGINE=$(echo "$TRIM" | jq -r '.engine')
        HP=$(echo "$TRIM" | jq -r '.horsepower')
        TRANSMISSION=$(echo "$TRIM" | jq -r '.transmission')
        DRIVETRAIN=$(echo "$TRIM" | jq -r '.drivetrain')
        FUEL_TYPE=$(echo "$TRIM" | jq -r '.fuel_type')
        MPG_CITY=$(echo "$TRIM" | jq -r '.mpg_city')
        MPG_HWY=$(echo "$TRIM" | jq -r '.mpg_highway')
        
        # Generar datos aleatorios
        VEHICLE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
        TITLE="$YEAR $MAKE $MODEL $TRIM_NAME"
        
        # Condición y millas
        CONDITIONS=("New" "Used" "Certified")
        CONDITION="${CONDITIONS[$((RANDOM % 3))]}"
        
        if [[ "$CONDITION" == "New" ]]; then
            MILEAGE=$((RANDOM % 500 + 10))
        else
            MILEAGE=$((RANDOM % 60000 + 5000))
        fi
        
        # Precio basado en MSRP y condición
        PRICE_VARIATION=$((RANDOM % 5000 - 2000))
        if [[ "$CONDITION" == "Used" ]]; then
            PRICE=$((MSRP - 5000 + PRICE_VARIATION))
        else
            PRICE=$((MSRP + PRICE_VARIATION))
        fi
        
        # Colores
        EXT_COLORS=("Black" "White" "Silver" "Gray" "Blue" "Red" "Green")
        INT_COLORS=("Black" "Beige" "Gray" "Brown" "White")
        EXT_COLOR="${EXT_COLORS[$((RANDOM % ${#EXT_COLORS[@]}))]}"
        INT_COLOR="${INT_COLORS[$((RANDOM % ${#INT_COLORS[@]}))]}"
        
        # VIN aleatorio
        VIN=$(cat /dev/urandom | LC_ALL=C tr -dc 'A-HJ-NPR-Z0-9' | head -c 17)
        
        # Categoría
        CATEGORY_ID=$(get_category_id "$BODY_STYLE")
        
        # Features JSON
        ALL_FEATURES='["Bluetooth","Backup Camera","Navigation","Leather Seats","Sunroof","Apple CarPlay","Android Auto","Heated Seats","Blind Spot Monitor","Lane Departure Warning","Adaptive Cruise Control","Keyless Entry","Push Start"]'
        FEATURES=$(echo "$ALL_FEATURES" | jq -c "[.[] | select(. != null)][:$((RANDOM % 6 + 5))]")
        
        # Vehicle Type basado en Body Style
        case "$BODY_STYLE" in
            "Sedan"|"Coupe"|"Convertible"|"Hatchback") VEHICLE_TYPE="Car" ;;
            "SUV"|"Crossover") VEHICLE_TYPE="SUV" ;;
            "Pickup"|"Truck") VEHICLE_TYPE="Truck" ;;
            "Van"|"Minivan") VEHICLE_TYPE="Van" ;;
            *) VEHICLE_TYPE="Car" ;;
        esac
        
        # Calcular valores booleanos
        if [[ "$CONDITION" == "Certified" ]]; then
            IS_CERTIFIED="true"
        else
            IS_CERTIFIED="false"
        fi
        
        if [[ $((RANDOM % 5)) -eq 0 ]]; then
            IS_FEATURED="true"
        else
            IS_FEATURED="false"
        fi
        
        # Insertar vehículo con las columnas correctas
        docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -q -c "
            INSERT INTO vehicles (
                \"Id\", \"DealerId\", \"Title\", \"Description\", \"Price\", \"Currency\", \"Status\",
                \"SellerId\", \"SellerName\", \"SellerType\", \"SellerVerified\", \"SellerCity\",
                \"VIN\", \"Make\", \"Model\", \"Trim\", \"Year\",
                \"VehicleType\", \"BodyStyle\", \"Doors\", \"Seats\",
                \"FuelType\", \"EngineSize\", \"Horsepower\", \"Transmission\", \"DriveType\",
                \"Mileage\", \"MileageUnit\", \"Condition\",
                \"AccidentHistory\", \"HasCleanTitle\", \"IsCertified\",
                \"ExteriorColor\", \"InteriorColor\",
                \"MpgCity\", \"MpgHighway\",
                \"City\", \"State\", \"Country\", \"Latitude\", \"Longitude\",
                \"FeaturesJson\", \"PackagesJson\",
                \"ViewCount\", \"FavoriteCount\", \"InquiryCount\",
                \"IsFeatured\", \"IsDeleted\", \"CategoryId\"
            ) VALUES (
                '$VEHICLE_ID', '$DEALER_ID', '$TITLE',
                'Beautiful $YEAR $MAKE $MODEL $TRIM_NAME in excellent condition. $ENGINE engine with $HP HP.',
                $PRICE, 'USD', 'Active',
                '$DEALER_ID', '$DEALER_NAME', 1, true, '$DEALER_CITY',
                '$VIN', '$MAKE', '$MODEL', '$TRIM_NAME', $YEAR,
                '$VEHICLE_TYPE', '$BODY_STYLE', 4, 5,
                '$FUEL_TYPE', '$ENGINE', $HP, '$TRANSMISSION', '$DRIVETRAIN',
                $MILEAGE, 'miles', '$CONDITION',
                false, true, $IS_CERTIFIED,
                '$EXT_COLOR', '$INT_COLOR',
                $MPG_CITY, $MPG_HWY,
                '$DEALER_CITY', 'Santo Domingo', 'DO', $DEALER_LAT, $DEALER_LNG,
                '$FEATURES', '[]',
                $((RANDOM % 1000)), $((RANDOM % 100)), $((RANDOM % 50)),
                $IS_FEATURED, false, '$CATEGORY_ID'
            );
        " 2>&1
        
        if [[ $? -eq 0 ]]; then
            ((TOTAL_VEHICLES++))
            echo -e "    ${GREEN}✓${NC} Vehículo $TITLE creado"
        else
            echo -e "    ${RED}✗${NC} Error creando vehículo $TITLE"
        fi
        
        # Insertar 5 imágenes con URL completa de S3
        for ((i=0; i<$IMAGES_PER_VEHICLE; i++)); do
            IMAGE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
            
            # Rotar por las fotos disponibles
            PHOTO_ID="${PHOTO_ARRAY[$PHOTO_INDEX]}"
            PHOTO_INDEX=$(( (PHOTO_INDEX + 1) % ${#PHOTO_ARRAY[@]} ))
            
            # URL completa de S3
            S3_URL="https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com/${S3_PATH}/${PHOTO_ID}.jpg"
            
            # Caption según posición
            case $i in
                0) CAPTION="Vista principal"; IS_PRIMARY="true" ;;
                1) CAPTION="Interior"; IS_PRIMARY="false" ;;
                2) CAPTION="Vista lateral"; IS_PRIMARY="false" ;;
                3) CAPTION="Motor"; IS_PRIMARY="false" ;;
                4) CAPTION="Vista adicional"; IS_PRIMARY="false" ;;
            esac
            
            # Insertar imagen con URL completa de S3
            docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -q -c "
                INSERT INTO vehicle_images (
                    \"Id\", \"DealerId\", \"VehicleId\", \"Url\", \"ThumbnailUrl\",
                    \"Caption\", \"ImageType\", \"SortOrder\", \"IsPrimary\",
                    \"Width\", \"Height\", \"CreatedAt\"
                ) VALUES (
                    '$IMAGE_ID', '$DEALER_ID', '$VEHICLE_ID',
                    '$S3_URL', '$S3_URL',
                    '$CAPTION', 0, $i, $IS_PRIMARY,
                    800, 600, NOW()
                );
            " 2>/dev/null
            
            ((TOTAL_IMAGES++))
        done
        
        echo "    ✓ $TITLE - 5 imágenes"
    done
done

# ═══════════════════════════════════════════════════════════════════════════
# RESUMEN FINAL
# ═══════════════════════════════════════════════════════════════════════════
echo -e "\n${GREEN}"
echo "═══════════════════════════════════════════════════════════════"
echo "  ✅ SEED COMPLETADO"
echo "═══════════════════════════════════════════════════════════════"
echo -e "${NC}"

# Verificar totales en BD
TOTAL_V=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c "SELECT COUNT(*) FROM vehicles;")
TOTAL_I=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c "SELECT COUNT(*) FROM vehicle_images;")
TOTAL_S3=$(aws s3 ls "s3://${S3_BUCKET}/${S3_PATH}/" | wc -l | xargs)

echo "  📊 Resumen:"
echo "  • Fotos en S3: $TOTAL_S3"
echo "  • Vehículos creados: $TOTAL_V"
echo "  • Imágenes en BD: $TOTAL_I"
echo ""
echo "  📁 S3 Path: s3://${S3_BUCKET}/${S3_PATH}/"
echo "  🌐 URL Base: https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com/${S3_PATH}/"
echo ""

# Mostrar ejemplo
echo -e "${BLUE}📷 Ejemplo de imagen:${NC}"
SAMPLE=$(docker exec vehiclessaleservice-db psql -U postgres -d vehiclessaleservice -t -A -c "
    SELECT '  DB: ' || vi.\"Url\" || E'\n  S3: https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com/${S3_PATH}/' || vi.\"Url\" || '.jpg'
    FROM vehicle_images vi LIMIT 1;
")
echo "$SAMPLE"

# Cleanup
echo -e "\n${YELLOW}🧹 Limpiando archivos temporales...${NC}"
rm -rf "$TEMP_DIR"
echo -e "${GREEN}✅ Limpio${NC}"
