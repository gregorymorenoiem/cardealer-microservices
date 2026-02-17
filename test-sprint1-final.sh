#!/bin/bash
# SPRINT 1 SERVICES - FINAL TESTING
# ===================================

TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFkNzNjZGJjLTljY2UtNDQ1Ny1hZThkLTdlNGJjMzUyMmE4MCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3R1c2VyMTc2Nzg2ODA1OEBva2xhLmNvbS5kbyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJ0ZXN0dXNlcjE3Njc4NjgwNTgiLCJlbWFpbF92ZXJpZmllZCI6ImZhbHNlIiwic2VjdXJpdHlfc3RhbXAiOiJNTUlBSDIyWUxCVlFJSEM1UjVVNldTV01BNFo3NjZERyIsImp0aSI6Ijc0MGRiODg4LWY5OTUtNGQ1OS1iYzhlLTUwNzllM2IxZDYzYiIsImRlYWxlcklkIjoiIiwiYWNjb3VudF90eXBlIjoiMSIsImV4cCI6MTc2Nzg3MTY1OCwiaXNzIjoiQXV0aFNlcnZpY2UtRGV2IiwiYXVkIjoiQ2FyR3VydXMtRGV2In0.ye-UobS7w2bg5dyfOvvC-x8wTzRNKMzlA5_5RB2hyrE"

echo "======================================================================="
echo "   ✅ SPRINT 1 - NEW SERVICES TESTING (FINAL)"
echo "======================================================================="

# Test 1: MaintenanceService
echo ""
echo "📋 1. MAINTENANCESERVICE (Port 5061)"
echo "-------------------------------------------------------------------"
echo "[✓] GET /api/Maintenance/status (Public endpoint)"
MAINTENANCE_RESPONSE=$(curl -s http://localhost:5061/api/Maintenance/status)
echo "$MAINTENANCE_RESPONSE" | python3 -m json.tool 2>/dev/null
if echo "$MAINTENANCE_RESPONSE" | grep -q "isMaintenanceMode"; then
    echo "✅ PASS - MaintenanceService is working"
else
    echo "❌ FAIL - MaintenanceService returned unexpected response"
fi

# Test 2: ComparisonService  
echo ""
echo "📊 2. COMPARISONSERVICE (Port 5066)"
echo "-------------------------------------------------------------------"
echo "[✓] GET /api/Comparison (List comparisons - Auth required)"
COMPARISON_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5066/api/Comparison)
if [ "$COMPARISON_RESPONSE" = "[]" ] || echo "$COMPARISON_RESPONSE" | grep -q "\["; then
    echo "✅ PASS - ComparisonService is working (empty list or with data)"
    echo "$COMPARISON_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$COMPARISON_RESPONSE"
else
    echo "Response: $COMPARISON_RESPONSE"
    echo "❌ FAIL - ComparisonService returned unexpected response"
fi

# Test 3: AlertService
echo ""
echo "🔔 3. ALERTSERVICE (Port 5067)"
echo "-------------------------------------------------------------------"
echo "[✓] GET /api/Alert/price-alerts (List price alerts)"
ALERT_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5067/api/Alert/price-alerts)
if [ "$ALERT_RESPONSE" = "[]" ] || echo "$ALERT_RESPONSE" | grep -q "\["; then
    echo "✅ PASS - AlertService (price-alerts) is working"
    echo "$ALERT_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$ALERT_RESPONSE"
else
    echo "Response: $ALERT_RESPONSE"
    echo "❌ FAIL - AlertService returned unexpected response"
fi

echo ""
echo "[✓] GET /api/Alert/saved-searches (List saved searches)"
SAVED_SEARCH_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5067/api/Alert/saved-searches)
if [ "$SAVED_SEARCH_RESPONSE" = "[]" ] || echo "$SAVED_SEARCH_RESPONSE" | grep -q "\["; then
    echo "✅ PASS - AlertService (saved-searches) is working"
    echo "$SAVED_SEARCH_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$SAVED_SEARCH_RESPONSE"
else
    echo "Response: $SAVED_SEARCH_RESPONSE"
    echo "❌ FAIL - AlertService saved-searches returned unexpected response"
fi

# Health checks
echo ""
echo "🏥 4. HEALTH CHECKS"
echo "-------------------------------------------------------------------"
for service in "maintenanceservice:5061" "comparisonservice:5066" "alertservice:5067"; do
    NAME=$(echo $service | cut -d: -f1)
    PORT=$(echo $service | cut -d: -f2)
    HEALTH=$(curl -s http://localhost:$PORT/health)
    if [ "$HEALTH" = "Healthy" ]; then
        echo "✅ $NAME - Healthy"
    else
        echo "❌ $NAME - $HEALTH"
    fi
done

echo ""
echo "======================================================================="
echo "   ✅ TESTING COMPLETED"
echo "======================================================================="
echo ""
echo "📦 Services Status:"
docker compose ps | grep -E "(maintenance|comparison|alert)" | awk '{print "  - "$1" ("$NF")"}'
echo ""
