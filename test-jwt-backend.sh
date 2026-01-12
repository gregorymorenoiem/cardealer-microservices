#!/bin/bash

echo "🧪 Test JWT Backend - AccountType en JWT"
echo "=========================================="
echo ""

# Login
RESPONSE=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "dealer@okla.com.do", "password": "Dealer123!"}')

echo "📦 Login response:"
echo "$RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$RESPONSE"
echo ""

# Extraer token
TOKEN=$(echo "$RESPONSE" | python3 -c "import sys, json; r = json.load(sys.stdin); print(r.get('accessToken', ''))" 2>/dev/null)

if [ -z "$TOKEN" ]; then
  echo "❌ No se pudo obtener token"
  exit 1
fi

echo "🔑 Token JWT (primeros 60 chars): ${TOKEN:0:60}..."
echo ""

# Decodificar payload
echo "📋 JWT Payload:"
PAYLOAD=$(echo "$TOKEN" | cut -d'.' -f2 | base64 -d 2>/dev/null)
echo "$PAYLOAD" | python3 -m json.tool
echo ""

# Extraer account_type específicamente
echo "🎯 Campo account_type:"
echo "$PAYLOAD" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    account_type = data.get('account_type', 'NOT_FOUND')
    print(f'  Valor: {account_type}')
    print(f'  Tipo: {type(account_type).__name__}')
    if account_type == 1:
        print('  ✅ Es Dealer (1)')
    elif account_type == '1':
        print('  ✅ Es Dealer como string (\"1\")')
    else:
        print(f'  ❌ Valor inesperado: {account_type}')
except Exception as e:
    print(f'  ❌ Error: {e}')
"

