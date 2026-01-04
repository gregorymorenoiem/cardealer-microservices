#!/bin/bash

# Script para crear 10 Sedanes para el dealer de República Dominicana
# Basado en los mock data de VehiclesOnlyHomePage.tsx

API_URL="http://localhost:18443"
USER_ID=$(cat /tmp/dealer_rd_user_id.txt)
DEALER_ID=$(cat /tmp/dealer_rd_dealer_id.txt)
TOKEN=$(cat /tmp/dealer_rd_token.txt)

echo "🚗 Creando 10 Sedanes para AutoVentas RD..."
echo "================================================"
echo "User ID: $USER_ID"
echo "Dealer ID: $DEALER_ID"
echo ""

# Array de sedanes con sus datos
declare -a SEDANS=(
  "BMW Serie 3 2024|48500|photo-1555215695-3004980ad54e|Santo Domingo, RD|BMW|Serie 3|2024|Sedan|Gasolina|127|Automatic|Black|4|127000|Excellent|Premium sedan with advanced technology|new"
  "Mercedes-Benz C-Class 2024|52000|photo-1618843479313-40f8afb4b4d8|Santiago, RD|Mercedes-Benz|C-Class|2024|Sedan|Gasolina|98|Automatic|Silver|4|98000|Excellent|Luxury and performance combined|new"
  "Audi A4 2024|45900|photo-1606664515524-ed2f786a0bd6|Punta Cana, RD|Audi|A4|2024|Sedan|Gasolina|156|Automatic|White|4|156000|Excellent|Sophisticated design with Quattro AWD|new"
  "Tesla Model 3 2024|42990|photo-1560958089-b8a1929cea89|Santo Domingo, RD|Tesla|Model 3|2024|Sedan|Electric|234|Automatic|Blue|4|0|Excellent|Electric performance sedan with autopilot|new"
  "Lexus ES 350 2024|44500|photo-1555215695-3004980ad54e|La Romana, RD|Lexus|ES 350|2024|Sedan|Gasolina|89|Automatic|Pearl White|4|89000|Excellent|Japanese luxury and reliability|new"
  "Honda Accord 2024|32500|photo-1533473359331-0135ef1b58bf|San Pedro, RD|Honda|Accord|2024|Sedan|Gasolina|312|Automatic|Gray|4|312000|Good|Spacious and efficient family sedan|new"
  "Toyota Camry 2024|29800|photo-1621007947382-bb3c3994e3fb|Puerto Plata, RD|Toyota|Camry|2024|Sedan|Hybrid|445|Automatic|Red|4|445000|Excellent|Hybrid efficiency meets reliability|new"
  "Mazda 6 2024|28500|photo-1580273916550-e323be2ae537|Higüey, RD|Mazda|6|2024|Sedan|Gasolina|178|Automatic|Deep Blue|4|178000|Good|Sporty styling with premium features|new"
  "Hyundai Sonata 2024|27900|photo-1618843479313-40f8afb4b4d8|Barahona, RD|Hyundai|Sonata|2024|Sedan|Gasolina|201|Automatic|Silver|4|201000|Good|Value-packed midsize sedan|new"
  "Kia K5 2024|26800|photo-1606664515524-ed2f786a0bd6|Samaná, RD|Kia|K5|2024|Sedan|Gasolina|167|Automatic|White|4|167000|Good|Bold design with tech features|new"
)

CREATED_COUNT=0

for sedan_data in "${SEDANS[@]}"; do
  IFS='|' read -r title price photo location make model year bodyStyle fuelType reviews transmission color doors mileage condition description state <<< "$sedan_data"
  
  CREATED_COUNT=$((CREATED_COUNT + 1))
  
  echo "[$CREATED_COUNT/10] Creando: $title"
  
  # Crear el vehículo
  RESPONSE=$(curl -s -X POST "$API_URL/api/vehicles-sale" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{
      "title": "'"$title"'",
      "description": "'"$description"'",
      "price": '"$price"',
      "make": "'"$make"'",
      "model": "'"$model"'",
      "year": '"$year"',
      "mileage": '"$mileage"',
      "condition": "'"$state"'",
      "bodyStyle": "'"$bodyStyle"'",
      "transmission": "'"$transmission"'",
      "fuelType": "'"$fuelType"'",
      "exteriorColor": "'"$color"'",
      "interiorColor": "Beige",
      "numberOfDoors": '"$doors"',
      "numberOfSeats": 5,
      "vin": "1HGBH41JXMN'$(printf "%06d" $CREATED_COUNT)'",
      "location": "'"$location"'",
      "address": "Av. Principal, '"$location"'",
      "city": "'"${location%%,*}"'",
      "state": "Distrito Nacional",
      "country": "República Dominicana",
      "zipCode": "10111",
      "latitude": 18.4861,
      "longitude": -69.9312,
      "status": "active",
      "isFeatured": true,
      "viewCount": '"$reviews"',
      "customFieldsJson": "{\"rating\":4.8,\"reviews\":'"$reviews"'}"
    }')
  
  # Extraer vehicle ID
  VEHICLE_ID=$(echo "$RESPONSE" | jq -r '.data.id // .id // empty')
  
  if [ -n "$VEHICLE_ID" ] && [ "$VEHICLE_ID" != "null" ]; then
    echo "   ✅ Creado con ID: $VEHICLE_ID"
    
    # Agregar imagen
    IMAGE_RESPONSE=$(curl -s -X POST "$API_URL/api/vehicles-sale/$VEHICLE_ID/images" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d '{
        "photoId": "'"$photo"'",
        "isPrimary": true,
        "displayOrder": 1
      }')
    
    echo "   📸 Imagen agregada: $photo"
  else
    echo "   ❌ Error creando vehículo"
    echo "   Response: $RESPONSE" | jq '.'
  fi
  
  echo ""
  sleep 0.5
done

echo "================================================"
echo "✅ $CREATED_COUNT Sedanes creados exitosamente!"
echo "================================================"
