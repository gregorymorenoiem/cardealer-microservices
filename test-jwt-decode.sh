#!/bin/bash

echo "🧪 Test JWT Decode - Verificando AccountType"
echo "=============================================="
echo ""

# Login con cuenta dealer
RESPONSE=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "dealer@okla.com.do",
    "password": "Dealer123!"
  }')

echo "📦 Response completo:"
echo "$RESPONSE" | python3 -m json.tool
echo ""

# Extraer accessToken
TOKEN=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('accessToken', ''))")

if [ -z "$TOKEN" ]; then
  echo "❌ No se pudo obtener el token"
  exit 1
fi

echo "🔑 Token obtenido (primeros 50 chars): ${TOKEN:0:50}..."
echo ""

# Decodificar JWT payload
echo "📋 JWT Payload decodificado:"
echo "$TOKEN" | cut -d'.' -f2 | base64 -d 2>/dev/null | python3 -m json.tool
echo ""

# Extraer campos específicos
echo "🎯 Campos relevantes:"
PAYLOAD=$(echo "$TOKEN" | cut -d'.' -f2 | base64 -d 2>/dev/null)
echo "$PAYLOAD" | python3 -c "
import sys, json
data = json.load(sys.stdin)
print(f\"  - account_type: {data.get('account_type', 'NOT FOUND')} (tipo: {type(data.get('account_type', 'N/A')).__name__})\")
print(f\"  - dealerId: {data.get('dealerId', 'NOT FOUND')}\")
print(f\"  - email: {data.get('email', 'NOT FOUND')}\")
print(f\"  - name: {data.get('name', 'NOT FOUND')}\")
"

echo ""
echo "✅ Test completado"
