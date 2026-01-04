#!/bin/bash

# Script para crear dealer en República Dominicana
# Santo Domingo, República Dominicana
# Coordenadas: 18.4861, -69.9312

API_URL="http://localhost:18443"

echo "🇩🇴 Creando dealer en República Dominicana..."
echo "================================================"

# Paso 1: Registrar usuario
echo ""
echo "📝 Paso 1: Registrando usuario dealer..."

REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "autoventas.rd@cardealer.com",
    "password": "AutoVentasRD2024!",
    "userName": "autoventas_rd",
    "fullName": "AutoVentas República Dominicana",
    "phoneNumber": "+1-809-555-0123",
    "accountType": "dealer"
  }')

echo "$REGISTER_RESPONSE" | jq '.'

# Extraer userId
USER_ID=$(echo "$REGISTER_RESPONSE" | jq -r '.data.userId // .userId // empty')

if [ -z "$USER_ID" ] || [ "$USER_ID" == "null" ]; then
  echo "❌ Error: No se pudo registrar el usuario"
  exit 1
fi

echo "✅ Usuario registrado con ID: $USER_ID"

# Paso 2: Login para obtener token
echo ""
echo "🔐 Paso 2: Obteniendo token de autenticación..."

LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "autoventas.rd@cardealer.com",
    "password": "AutoVentasRD2024!"
  }')

echo "$LOGIN_RESPONSE" | jq '.'

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.token // .token // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo "❌ Error: No se pudo obtener el token"
  exit 1
fi

echo "✅ Token obtenido"

# Paso 3: Crear perfil de dealer
echo ""
echo "🏢 Paso 3: Creando perfil de dealer..."

# Calcular fecha de suscripción (1 año)
SUBSCRIPTION_START=$(date -u +"%Y-%m-%dT%H:%M:%S.000Z")
SUBSCRIPTION_END=$(date -u -v+1y +"%Y-%m-%dT%H:%M:%S.000Z")

DEALER_RESPONSE=$(curl -s -X POST "$API_URL/api/users/$USER_ID/dealer" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "businessName": "AutoVentas República Dominicana",
    "businessType": "Dealership",
    "taxId": "RNC-1-30-12345-6",
    "website": "https://autoventas-rd.com",
    "description": "Concesionario líder en República Dominicana con más de 20 años de experiencia. Ofrecemos vehículos de calidad con financiamiento y garantía extendida.",
    "address": "Av. Winston Churchill #1515",
    "city": "Santo Domingo",
    "state": "Distrito Nacional",
    "country": "República Dominicana",
    "zipCode": "10111",
    "latitude": 18.4861,
    "longitude": -69.9312,
    "phoneNumber": "+1-809-555-0123",
    "email": "autoventas.rd@cardealer.com",
    "subscriptionTier": "premium",
    "subscriptionStatus": "active",
    "subscriptionStartDate": "'$SUBSCRIPTION_START'",
    "subscriptionEndDate": "'$SUBSCRIPTION_END'",
    "logoUrl": "https://images.unsplash.com/photo-1599305445671-ac291c95aaa9?w=200",
    "bannerUrl": "https://images.unsplash.com/photo-1492144534655-ae79c964c9d7?w=1200",
    "socialMedia": {
      "facebook": "https://facebook.com/autoventas.rd",
      "instagram": "https://instagram.com/autoventas_rd",
      "twitter": "https://twitter.com/autoventas_rd"
    },
    "operatingHours": {
      "monday": "08:00 AM - 06:00 PM",
      "tuesday": "08:00 AM - 06:00 PM",
      "wednesday": "08:00 AM - 06:00 PM",
      "thursday": "08:00 AM - 06:00 PM",
      "friday": "08:00 AM - 06:00 PM",
      "saturday": "09:00 AM - 02:00 PM",
      "sunday": "Cerrado"
    }
  }')

echo "$DEALER_RESPONSE" | jq '.'

DEALER_ID=$(echo "$DEALER_RESPONSE" | jq -r '.data.id // .id // .dealerId // empty')

if [ -z "$DEALER_ID" ] || [ "$DEALER_ID" == "null" ]; then
  echo "❌ Error: No se pudo crear el dealer"
  exit 1
fi

echo ""
echo "✅ Dealer creado exitosamente!"
echo "================================================"
echo "📋 Información del Dealer:"
echo "   User ID: $USER_ID"
echo "   Dealer ID: $DEALER_ID"
echo "   Email: autoventas.rd@cardealer.com"
echo "   Password: AutoVentasRD2024!"
echo "   Ubicación: Santo Domingo, República Dominicana"
echo "   Coordenadas: 18.4861, -69.9312"
echo "================================================"

# Guardar IDs para uso posterior
echo "$USER_ID" > /tmp/dealer_rd_user_id.txt
echo "$DEALER_ID" > /tmp/dealer_rd_dealer_id.txt
echo "$TOKEN" > /tmp/dealer_rd_token.txt

echo ""
echo "💾 IDs guardados en /tmp/ para uso posterior"
echo ""
