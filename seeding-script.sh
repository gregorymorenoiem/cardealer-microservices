#!/bin/bash

# Script de seeding para OKLA Marketplace
# Ejecuta seeding a través del API REST

API_BASE="http://localhost:15070/api"

echo "🌱 SEEDING V2.0 - Iniciando..."
echo ""

# Array de marcas
declare -A MAKES_MAP

# Función para crear una marca
create_make() {
    local name=$1
    local logo=$2
    local country=$3
    
    response=$(curl -s -X POST "${API_BASE}/catalog/makes" \
        -H "Content-Type: application/json" \
        -d "{
            \"name\": \"${name}\",
            \"logoUrl\": \"${logo}\",
            \"country\": \"${country}\",
            \"isPopular\": true
        }")
    
    # Extraer ID del response (asumiendo que retorna JSON con id)
    make_id=$(echo $response | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
    MAKES_MAP[$name]=$make_id
    
    echo "  ✓ Marca creada: $name (ID: $make_id)"
}

# Función para crear un modelo
create_model() {
    local make_name=$1
    local model_name=$2
    local make_id=${MAKES_MAP[$make_name]}
    
    curl -s -X POST "${API_BASE}/catalog/models" \
        -H "Content-Type: application/json" \
        -d "{
            \"makeId\": \"${make_id}\",
            \"name\": \"${model_name}\"
        }" > /dev/null
    
    echo "    ✓ Modelo creado: $model_name"
}

# Función para crear un vehículo
create_vehicle() {
    local make=$1
    local model=$2
    local year=$3
    local price=$4
    local mileage=$5
    local make_id=${MAKES_MAP[$make]}
    
    # Generar VIN aleatorio
    vin=$(cat /dev/urandom | tr -dc 'A-HJ-NPR-Z0-9' | fold -w 17 | head -n 1)
    
    curl -s -X POST "${API_BASE}/vehicles" \
        -H "Content-Type: application/json" \
        -d "{
            \"title\": \"${year} ${make} ${model}\",
            \"description\": \"Excelente ${year} ${make} ${model} en muy buenas condiciones.\",
            \"price\": ${price},
            \"currency\": \"USD\",
            \"status\": \"Active\",
            \"sellerId\": \"00000000-0000-0000-0000-000000000001\",
            \"sellerName\": \"AutoDealer RD\",
            \"sellerType\": \"Dealer\",
            \"sellerPhone\": \"809-555-0100\",
            \"sellerCity\": \"Santo Domingo\",
            \"sellerState\": \"Distrito Nacional\",
            \"makeId\": \"${make_id}\",
            \"make\": \"${make}\",
            \"model\": \"${model}\",
            \"year\": ${year},
            \"vin\": \"${vin}\",
            \"vehicleType\": \"Car\",
            \"bodyStyle\": \"Sedan\",
            \"doors\": 4,
            \"seats\": 5,
            \"fuelType\": \"Gasoline\",
            \"engineSize\": \"2.5L\",
            \"transmission\": \"Automatic\",
            \"driveType\": \"FWD\",
            \"mileage\": ${mileage},
            \"mileageUnit\": \"Miles\",
            \"condition\": \"Used\",
            \"exteriorColor\": \"White\",
            \"interiorColor\": \"Black\",
            \"city\": \"Santo Domingo\",
            \"state\": \"Distrito Nacional\",
            \"country\": \"Dominican Republic\"
        }" > /dev/null
}

echo "🏭 Paso 1: Creando marcas..."
create_make "Toyota" "https://picsum.photos/seed/toyota/200/200" "Japan"
sleep 0.5
create_make "Honda" "https://picsum.photos/seed/honda/200/200" "Japan"
sleep 0.5
create_make "Nissan" "https://picsum.photos/seed/nissan/200/200" "Japan"
sleep 0.5
create_make "Ford" "https://picsum.photos/seed/ford/200/200" "USA"
sleep 0.5
create_make "BMW" "https://picsum.photos/seed/bmw/200/200" "Germany"

echo ""
echo "📦 Paso 2: Creando modelos..."
create_model "Toyota" "Corolla"
create_model "Toyota" "Camry"
create_model "Toyota" "RAV4"
create_model "Honda" "Civic"
create_model "Honda" "Accord"
create_model "Nissan" "Altima"
create_model "Nissan" "Rogue"
create_model "Ford" "F-150"
create_model "Ford" "Mustang"
create_model "BMW" "3 Series"

echo ""
echo "🚗 Paso 3: Creando vehículos (muestra de 10)..."

# Crear algunos vehículos de muestra
for i in {1..2}; do
    create_vehicle "Toyota" "Corolla" $((2018 + RANDOM % 6)) $((20000 + RANDOM % 30000)) $((10000 + RANDOM % 80000))
    echo -n "."
    sleep 0.3
done

for i in {1..2}; do
    create_vehicle "Honda" "Civic" $((2018 + RANDOM % 6)) $((22000 + RANDOM % 28000)) $((8000 + RANDOM % 70000))
    echo -n "."
    sleep 0.3
done

for i in {1..2}; do
    create_vehicle "Nissan" "Altima" $((2018 + RANDOM % 6)) $((18000 + RANDOM % 25000)) $((15000 + RANDOM % 90000))
    echo -n "."
    sleep 0.3
done

for i in {1..2}; do
    create_vehicle "Ford" "F-150" $((2018 + RANDOM % 6)) $((35000 + RANDOM % 40000)) $((20000 + RANDOM % 100000))
    echo -n "."
    sleep 0.3
done

for i in {1..2}; do
    create_vehicle "BMW" "3 Series" $((2018 + RANDOM % 6)) $((40000 + RANDOM % 35000)) $((12000 + RANDOM % 60000))
    echo -n "."
    sleep 0.3
done

echo ""
echo ""
echo "✅ Seeding completado!"
echo "Puedes crear más vehículos ejecutando los comandos create_vehicle manualmente."
echo "O modificar este script para crear los 150 vehículos completos."
