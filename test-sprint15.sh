#!/bin/bash

# Test Sprint 15 - ReviewService Endpoints
# Token JWT válido para usuario 22222222-2222-2222-2222-222222222222

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMjIyMjIyMi0yMjIyLTIyMjItMjIyMi0yMjIyMjIyMjIyMjIiLCJlbWFpbCI6InRlc3RAb2tsYS5jb20uZG8iLCJyb2xlIjoiVXNlciIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJpc3MiOiJodHRwczovL2FwaS5va2xhLmNvbS5kbyIsImF1ZCI6Im9rbGEtY2xpZW50cyIsImV4cCI6MTc2ODA3NDA0OCwiaWF0IjoxNzY3OTg3NjQ4fQ.EG7kQ3rn3XJwpm8StZ_hkLqVIxi4Eii7dklTb5vmaV8"

BASE_URL="http://localhost:5063"

echo "=========================================="
echo "   SPRINT 15 - REVIEWSERVICE API TESTS   "
echo "=========================================="
echo ""

echo "1. HEALTH CHECK:"
curl -s "$BASE_URL/health"
echo -e "\n"

echo "2. GET /api/Reviews/seller/{sellerId} (public):"
curl -s "$BASE_URL/api/Reviews/seller/11111111-1111-1111-1111-111111111111"
echo -e "\n"

echo "3. GET /api/Reviews/seller/{sellerId}/summary (public):"
curl -s "$BASE_URL/api/Reviews/seller/11111111-1111-1111-1111-111111111111/summary"
echo -e "\n"

echo "4. GET /api/Reviews/seller/{sellerId}/badges (public):"
curl -s "$BASE_URL/api/Reviews/seller/11111111-1111-1111-1111-111111111111/badges"
echo -e "\n"

echo "5. POST /api/Reviews (authenticated - creating review):"
REVIEW_RESPONSE=$(curl -s -X POST "$BASE_URL/api/Reviews" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"sellerId":"11111111-1111-1111-1111-111111111111","buyerId":"22222222-2222-2222-2222-222222222222","transactionId":"33333333-3333-3333-3333-333333333333","rating":5,"title":"Excelente vendedor","content":"Muy profesional y rapido. El vehiculo estaba en perfectas condiciones. Recomendado al 100%!","vehicleId":"44444444-4444-4444-4444-444444444444","isVerifiedPurchase":true}')
echo "$REVIEW_RESPONSE"
REVIEW_ID=$(echo "$REVIEW_RESPONSE" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)
echo "Review ID: $REVIEW_ID"
echo ""

echo "6. GET /api/Reviews/{reviewId} (public):"
if [ -n "$REVIEW_ID" ]; then
  curl -s "$BASE_URL/api/Reviews/$REVIEW_ID"
else
  echo "No review ID available, skipping..."
fi
echo -e "\n"

echo "7. POST /api/Reviews/{reviewId}/vote (authenticated - voting helpful):"
if [ -n "$REVIEW_ID" ]; then
  curl -s -X POST "$BASE_URL/api/Reviews/$REVIEW_ID/vote" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d '{"isHelpful":true}'
else
  echo "No review ID available, skipping..."
fi
echo -e "\n"

echo "8. GET /api/Reviews/{reviewId}/vote-stats (public):"
if [ -n "$REVIEW_ID" ]; then
  curl -s "$BASE_URL/api/Reviews/$REVIEW_ID/vote-stats"
else
  echo "No review ID available, skipping..."
fi
echo -e "\n"

echo "9. GET /api/Reviews/requests/buyer/{buyerId} (authenticated):"
curl -s "$BASE_URL/api/Reviews/requests/buyer/22222222-2222-2222-2222-222222222222" \
  -H "Authorization: Bearer $TOKEN"
echo -e "\n"

echo "10. Verificando summary después de crear review:"
curl -s "$BASE_URL/api/Reviews/seller/11111111-1111-1111-1111-111111111111/summary"
echo -e "\n"

echo "=========================================="
echo "            TESTS COMPLETADOS            "
echo "=========================================="
