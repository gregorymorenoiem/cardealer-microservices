#!/bin/bash

# First get a fresh token by logging in
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@test.com","password":"Test1234!"}')

echo "Login Response:"
echo "$LOGIN_RESPONSE" | jq .

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.accessToken')

echo ""
echo "Testing AUTH-2FA-001 Enable 2FA..."
echo ""

# Test 2FA enable (endpoint is /api/TwoFactor/enable)
ENABLE_RESPONSE=$(curl -s -X POST http://localhost:18443/api/TwoFactor/enable \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"type":1}')

echo "2FA Enable Response:"
echo "$ENABLE_RESPONSE" | jq .
