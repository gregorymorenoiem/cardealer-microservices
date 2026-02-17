#!/bin/bash

# Token JWT para autenticación
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFkNzNjZGJjLTljY2UtNDQ1Ny1hZThkLTdlNGJjMzUyMmE4MCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3R1c2VyMTc2Nzg2ODA1OEBva2xhLmNvbS5kbyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJ0ZXN0dXNlcjE3Njc4NjgwNTgiLCJlbWFpbF92ZXJpZmllZCI6ImZhbHNlIiwic2VjdXJpdHlfc3RhbXAiOiJNTUlBSDIyWUxCVlFJSEM1UjVVNldTV01BNFo3NjZERyIsImp0aSI6Ijc0MGRiODg4LWY5OTUtNGQ1OS1iYzhlLTUwNzllM2IxZDYzYiIsImRlYWxlcklkIjoiIiwiYWNjb3VudF90eXBlIjoiMSIsImV4cCI6MTc2Nzg3MTY1OCwiaXNzIjoiQXV0aFNlcnZpY2UtRGV2IiwiYXVkIjoiQ2FyR3VydXMtRGV2In0.ye-UobS7w2bg5dyfOvvC-x8wTzRNKMzlA5_5RB2hyrE"

echo "================================================================="
echo "   TESTING SPRINT 1 SERVICES - LOCAL DOCKER DEPLOYMENT"
echo "================================================================="

echo ""
echo "=== 1. MAINTENANCESERVICE (Port 5061) ==="
echo "-----------------------------------------------------------"
echo "[1.1] GET /api/maintenance/current (Public - Current maintenance window)"
curl -s http://localhost:5061/api/maintenance/current
echo ""

echo ""
echo "[1.2] GET /api/maintenance (List all - Admin required)"
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5061/api/maintenance
echo ""

echo ""
echo "=== 2. COMPARISONSERVICE (Port 5066) ==="
echo "-----------------------------------------------------------"
echo "[2.1] GET /api/comparisons (List user comparisons - Auth required)"
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5066/api/comparisons
echo ""

echo ""
echo "[2.2] POST /api/comparisons (Create comparison - Auth required)"
# Obtener 2 vehicle IDs para comparar
VEHICLE1=$(curl -s "http://localhost:15070/api/vehicles?pageNumber=1&pageSize=1" | grep -o '"id":"[^"]*' | head -1 | cut -d'"' -f4)
VEHICLE2=$(curl -s "http://localhost:15070/api/vehicles?pageNumber=1&pageSize=2" | grep -o '"id":"[^"]*' | tail -1 | cut -d'"' -f4)

if [ -n "$VEHICLE1" ] && [ -n "$VEHICLE2" ]; then
    curl -s -X POST -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{\"vehicleIds\":[\"$VEHICLE1\",\"$VEHICLE2\"]}" \
      http://localhost:5066/api/comparisons
    echo ""
else
    echo "Could not fetch vehicle IDs"
fi

echo ""
echo "=== 3. ALERTSERVICE (Port 5067) ==="
echo "-----------------------------------------------------------"
echo "[3.1] GET /api/alerts/price-alerts (List price alerts - Auth required)"
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5067/api/alerts/price-alerts
echo ""

echo ""
echo "[3.2] GET /api/alerts/saved-searches (List saved searches - Auth required)"
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5067/api/alerts/saved-searches
echo ""

echo ""
echo "[3.3] GET /api/alerts/free-days-left (Early Bird status - Auth required)"
curl -s -H "Authorization: Bearer $TOKEN" http://localhost:5067/api/alerts/free-days-left
echo ""

echo ""
echo "[3.4] POST /api/alerts/price-alerts (Create price alert - Auth required)"
if [ -n "$VEHICLE1" ]; then
    curl -s -X POST -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{\"vehicleId\":\"$VEHICLE1\",\"targetPrice\":1500000,\"notifyOnPriceDrop\":true,\"notifyOnPriceIncrease\":false}" \
      http://localhost:5067/api/alerts/price-alerts
    echo ""
else
    echo "No vehicle ID available"
fi

echo ""
echo "================================================================="
echo "   TESTING COMPLETED"
echo "================================================================="
