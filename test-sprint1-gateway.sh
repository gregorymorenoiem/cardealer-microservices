#!/bin/bash
# SPRINT 1 - FINAL TEST VIA GATEWAY
# ==================================

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFkNzNjZGJjLTljY2UtNDQ1Ny1hZThkLTdlNGJjMzUyMmE4MCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3R1c2VyMTc2Nzg2ODA1OEBva2xhLmNvbS5kbyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJ0ZXN0dXNlcjE3Njc4NjgwNTgiLCJlbWFpbF92ZXJpZmllZCI6ImZhbHNlIiwic2VjdXJpdHlfc3RhbXAiOiJNTUlBSDIyWUxCVlFJSEM1UjVVNldTV01BNFo3NjZERyIsImp0aSI6Ijc0MGRiODg4LWY5OTUtNGQ1OS1iYzhlLTUwNzllM2IxZDYzYiIsImRlYWxlcklkIjoiIiwiYWNjb3VudF90eXBlIjoiMSIsImV4cCI6MTc2Nzg3MTY1OCwiaXNzIjoiQXV0aFNlcnZpY2UtRGV2IiwiYXVkIjoiQ2FyR3VydXMtRGV2In0.ye-UobS7w2bg5dyfOvvC-x8wTzRNKMzlA5_5RB2hyrE"

echo "======================================================================="
echo "   ✅ SPRINT 1 - TESTING VIA GATEWAY (Port 18443)"
echo "======================================================================="
echo ""

# Test Gateway Health
echo "🔧 Gateway Health Check:"
GATEWAY_HEALTH=$(curl -s http://localhost:18443/health)
if [ "$GATEWAY_HEALTH" = "Gateway is healthy" ]; then
    echo "✅ Gateway: $GATEWAY_HEALTH"
else
    echo "❌ Gateway health check failed: $GATEWAY_HEALTH"
    exit 1
fi

echo ""
echo "======================================================================="
echo "   TESTING NEW SPRINT 1 SERVICES"
echo "======================================================================="

# Test 1: MaintenanceService
echo ""
echo "📋 1. MAINTENANCESERVICE"
echo "-------------------------------------------------------------------"
echo "[GET /api/maintenance/status] (Public endpoint)"
MAINTENANCE_RESPONSE=$(curl -s http://localhost:18443/api/maintenance/status)
if echo "$MAINTENANCE_RESPONSE" | grep -q "isMaintenanceMode"; then
    echo "✅ MaintenanceService via Gateway - WORKING"
    echo "   Response: $MAINTENANCE_RESPONSE"
else
    echo "❌ MaintenanceService - FAILED"
    echo "   Response: $MAINTENANCE_RESPONSE"
fi

# Test 2: ComparisonService
echo ""
echo "📊 2. COMPARISONSERVICE"
echo "-------------------------------------------------------------------"
echo "[GET /api/comparisons] (Auth required)"
COMPARISON_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:18443/api/comparisons)
if [ "$COMPARISON_RESPONSE" = "[]" ] || echo "$COMPARISON_RESPONSE" | grep -q "\["; then
    echo "✅ ComparisonService via Gateway - WORKING"
    echo "   Response: $COMPARISON_RESPONSE"
else
    echo "❌ ComparisonService - FAILED"
    echo "   Response: $COMPARISON_RESPONSE"
fi

# Test 3: AlertService - Price Alerts
echo ""
echo "🔔 3. ALERTSERVICE - PRICE ALERTS"
echo "-------------------------------------------------------------------"
echo "[GET /api/pricealerts] (Auth required)"
PRICEALERTS_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:18443/api/pricealerts)
if [ "$PRICEALERTS_RESPONSE" = "[]" ] || echo "$PRICEALERTS_RESPONSE" | grep -q "\["; then
    echo "✅ AlertService (PriceAlerts) via Gateway - WORKING"
    echo "   Response: $PRICEALERTS_RESPONSE"
else
    echo "❌ AlertService PriceAlerts - FAILED"
    echo "   Response: $PRICEALERTS_RESPONSE"
fi

# Test 4: AlertService - Saved Searches
echo ""
echo "🔔 4. ALERTSERVICE - SAVED SEARCHES"
echo "-------------------------------------------------------------------"
echo "[GET /api/savedsearches] (Auth required)"
SAVEDSEARCHES_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:18443/api/savedsearches)
if [ "$SAVEDSEARCHES_RESPONSE" = "[]" ] || echo "$SAVEDSEARCHES_RESPONSE" | grep -q "\["; then
    echo "✅ AlertService (SavedSearches) via Gateway - WORKING"
    echo "   Response: $SAVEDSEARCHES_RESPONSE"
else
    echo "❌ AlertService SavedSearches - FAILED"
    echo "   Response: $SAVEDSEARCHES_RESPONSE"
fi

echo ""
echo "======================================================================="
echo "   TESTING EXISTING SERVICES (Smoke Test)"
echo "======================================================================="

# Test 5: Vehicles (existing service)
echo ""
echo "🚗 5. VEHICLESSALESERVICE"
echo "-------------------------------------------------------------------"
echo "[GET /api/vehicles?pageNumber=1&pageSize=1]"
VEHICLES_RESPONSE=$(curl -s "http://localhost:18443/api/vehicles?pageNumber=1&pageSize=1")
if echo "$VEHICLES_RESPONSE" | grep -q "totalCount"; then
    TOTAL=$(echo "$VEHICLES_RESPONSE" | grep -o '"totalCount":[0-9]*' | cut -d: -f2)
    echo "✅ VehiclesSaleService via Gateway - WORKING"
    echo "   Total vehicles: $TOTAL"
else
    echo "❌ VehiclesSaleService - FAILED"
fi

echo ""
echo "======================================================================="
echo "   ✅ ALL TESTS COMPLETED"
echo "======================================================================="
echo ""
echo "📦 Services Status:"
docker compose ps | grep -E "(gateway|maintenance|comparison|alert)" | awk '{print "  - "$1" ("$7")"}'
echo ""
