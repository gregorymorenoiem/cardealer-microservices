#!/bin/bash
# Script para llenar las secciones del homepage con vehículos via API

API_URL="http://localhost:15070"
DB_CONTAINER="vehiclessaleservice-db"

echo "🚗 Llenando secciones del homepage..."

# Función para obtener IDs de vehículos por BodyStyle
get_vehicle_ids() {
    local body_style="$1"
    local limit="${2:-10}"
    docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -t -c \
        "SELECT \"Id\" FROM vehicles WHERE \"BodyStyle\" = '$body_style' ORDER BY \"Price\" DESC LIMIT $limit;" \
        | tr -d ' ' | grep -v '^$'
}

# Función para obtener IDs por marca premium
get_premium_vehicle_ids() {
    local makes="$1"
    local limit="${2:-10}"
    docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -t -c \
        "SELECT \"Id\" FROM vehicles WHERE \"Make\" IN ($makes) ORDER BY \"Price\" DESC LIMIT $limit;" \
        | tr -d ' ' | grep -v '^$'
}

# Función para asignar vehículos a una sección via API
assign_to_section() {
    local slug="$1"
    shift
    local vehicle_ids=("$@")
    
    # Construir JSON array
    local json_ids=$(printf '"%s",' "${vehicle_ids[@]}" | sed 's/,$//')
    local payload="{\"vehicleIds\": [$json_ids]}"
    
    echo "  📌 Asignando ${#vehicle_ids[@]} vehículos a '$slug'..."
    
    response=$(curl -s -X POST "$API_URL/api/homepagesections/$slug/vehicles/bulk" \
        -H "Content-Type: application/json" \
        -d "$payload")
    
    echo "    ✅ $response"
}

# Limpiar asignaciones existentes
echo "🧹 Limpiando asignaciones existentes..."
docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -c \
    "DELETE FROM vehicle_homepage_sections;" > /dev/null 2>&1

# 1. CAROUSEL PRINCIPAL - Mezcla de tipos
echo ""
echo "1️⃣  Carousel Principal (vehículos variados)..."
carousel_ids=($(docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -t -c "
    WITH ranked AS (
        SELECT \"Id\", \"BodyStyle\", ROW_NUMBER() OVER (PARTITION BY \"BodyStyle\" ORDER BY \"Price\" DESC) as rn
        FROM vehicles WHERE \"BodyStyle\" IN ('Sedan', 'SUV', 'Pickup', 'Coupe')
    )
    SELECT \"Id\" FROM ranked WHERE rn <= 3 LIMIT 10;
" | tr -d ' ' | grep -v '^$'))
assign_to_section "carousel" "${carousel_ids[@]}"

# 2. SEDANES
echo ""
echo "2️⃣  Sedanes..."
sedan_ids=($(get_vehicle_ids "Sedan" 10))
assign_to_section "sedanes" "${sedan_ids[@]}"

# 3. SUVs
echo ""
echo "3️⃣  SUVs..."
suv_ids=($(get_vehicle_ids "SUV" 10))
assign_to_section "suvs" "${suv_ids[@]}"

# 4. CAMIONETAS
echo ""
echo "4️⃣  Camionetas (Pickups)..."
pickup_ids=($(get_vehicle_ids "Pickup" 10))
assign_to_section "camionetas" "${pickup_ids[@]}"

# 5. DEPORTIVOS
echo ""
echo "5️⃣  Deportivos (Coupes)..."
coupe_ids=($(get_vehicle_ids "Coupe" 10))
assign_to_section "deportivos" "${coupe_ids[@]}"

# 6. DESTACADOS - Marcas premium
echo ""
echo "6️⃣  Destacados (marcas premium)..."
destacados_ids=($(get_premium_vehicle_ids "'Mercedes-Benz','BMW','Audi','Porsche','Tesla'" 10))
assign_to_section "destacados" "${destacados_ids[@]}"

# 7. LUJO
echo ""
echo "7️⃣  Lujo..."
lujo_ids=($(get_premium_vehicle_ids "'Mercedes-Benz','BMW','Audi','Porsche','Lexus'" 10))
assign_to_section "lujo" "${lujo_ids[@]}"

# Verificar resultados
echo ""
echo "📊 Verificando resultados..."
curl -s "$API_URL/api/homepagesections/homepage" | jq '.[] | {name: .name, vehicles: (.vehicles | length)}'

echo ""
echo "✅ ¡Secciones llenadas!"

# 8. ELÉCTRICOS
echo ""
echo "8️⃣  Vehículos Eléctricos..."
electric_ids=($(docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -t -c "SELECT \"Id\" FROM vehicles WHERE \"FuelType\" = 'Electric' ORDER BY \"Price\" DESC LIMIT 10;" | tr -d ' ' | grep -v '^$'))
assign_to_section "electricos" "${electric_ids[@]}"

# 9. EFICIENCIA (Toyota, Honda - marcas conocidas por eficiencia)
echo ""
echo "9️⃣  Eficiencia Total (Toyota, Honda)..."
eficiencia_ids=($(docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -t -c "SELECT \"Id\" FROM vehicles WHERE \"Make\" IN ('Toyota', 'Honda') ORDER BY \"Price\" ASC LIMIT 10;" | tr -d ' ' | grep -v '^$'))
assign_to_section "eficiencia" "${eficiencia_ids[@]}"

# 10. MUSCLE & PERFORMANCE (Mustang, 911, Coupes de alto rendimiento)
echo ""
echo "🔟 Muscle & Performance..."
muscle_ids=($(docker exec $DB_CONTAINER psql -U postgres -d vehiclessaleservice -t -c "SELECT \"Id\" FROM vehicles WHERE \"Make\" IN ('Ford', 'Porsche', 'BMW') AND \"BodyStyle\" = 'Coupe' ORDER BY \"Price\" DESC LIMIT 10;" | tr -d ' ' | grep -v '^$'))
assign_to_section "muscle-performance" "${muscle_ids[@]}"
