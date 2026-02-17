#!/bin/bash
echo "Testing Dealer Logins..."
echo "========================================"

for dealer in "Auto Económico RD:info@autoeconomico.com.do" "Demo Auto Sales RD:dealer@okla.com.do" "Premium Motors RD:ventas@premiummotors.com.do" "Mega Auto Group:contacto@megaautogroup.com.do"; do
    IFS=':' read -r name email <<< "$dealer"
    echo ""
    echo "Testing: $name ($email)"
    
    response=$(curl -s -X POST http://localhost:18443/api/auth/login \
        -H "Content-Type: application/json" \
        -d "{\"email\": \"$email\", \"password\": \"Dealer123!\"}")
    
    success=$(echo $response | python3 -c "import json,sys; print(json.load(sys.stdin).get('success', False))")
    
    if [ "$success" = "True" ]; then
        echo "✅ Login exitoso"
        dealerId=$(echo $response | python3 -c "import json,sys,base64; data=json.load(sys.stdin); token=data['data']['accessToken']; parts=token.split('.'); payload=parts[1]; padding=len(payload)%4; payload+='='*(4-padding) if padding else ''; decoded=base64.b64decode(payload); pj=json.loads(decoded); print(pj.get('dealerId','NO'))")
        echo "   DealerId: $dealerId"
    else
        echo "❌ Login falló"
    fi
done
