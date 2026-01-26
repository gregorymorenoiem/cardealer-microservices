#!/bin/bash
# =============================================================================
# Test Script for AIProcessingService AI Workers
# =============================================================================

API_URL="http://localhost:5070"
TEST_DATA_DIR="$(dirname "$0")"

echo "🚗 AIProcessingService - Test Suite"
echo "===================================="
echo ""

# Check if API is running
echo "1️⃣ Checking API Health..."
HEALTH=$(curl -s "$API_URL/health")
if [ "$HEALTH" = "Healthy" ]; then
    echo "   ✅ API is healthy"
else
    echo "   ❌ API is not responding. Start it with:"
    echo "      docker-compose -f docker-compose.cpu.yaml up -d"
    exit 1
fi

echo ""
echo "2️⃣ Checking Workers..."

# Check RabbitMQ queues
QUEUES=$(curl -s -u guest:guest "http://localhost:15679/api/queues" 2>/dev/null | grep -o '"name":"[^"]*"' | wc -l)
if [ "$QUEUES" -gt 0 ]; then
    echo "   ✅ RabbitMQ has $QUEUES queues"
else
    echo "   ⚠️ RabbitMQ may not be accessible"
fi

echo ""
echo "3️⃣ Testing CLIP Classification..."

# Test with a sample image URL (using a public car image)
CLIP_RESPONSE=$(curl -s -X POST "$API_URL/api/aiprocessing/analyze" \
    -H "Content-Type: application/json" \
    -d '{
        "vehicleId": "test-vehicle-001",
        "imageUrl": "https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=800"
    }')

if echo "$CLIP_RESPONSE" | grep -q "jobId\|error\|id"; then
    echo "   ✅ CLIP endpoint responding"
    echo "   Response: $(echo $CLIP_RESPONSE | head -c 200)"
else
    echo "   ❌ CLIP endpoint failed"
    echo "   Response: $CLIP_RESPONSE"
fi

echo ""
echo "4️⃣ Testing SAM2 Segmentation..."

SAM_RESPONSE=$(curl -s -X POST "$API_URL/api/aiprocessing/process" \
    -H "Content-Type: application/json" \
    -d '{
        "vehicleId": "test-vehicle-002",
        "userId": "test-user",
        "imageUrl": "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800",
        "processingType": "BackgroundRemoval",
        "options": {}
    }')

if echo "$SAM_RESPONSE" | grep -q "jobId\|error\|id"; then
    echo "   ✅ SAM2 endpoint responding"
    echo "   Response: $(echo $SAM_RESPONSE | head -c 200)"
else
    echo "   ❌ SAM2 endpoint failed"
    echo "   Response: $SAM_RESPONSE"
fi

echo ""
echo "5️⃣ Testing Backgrounds Endpoint..."

BG_RESPONSE=$(curl -s "$API_URL/api/backgrounds")
if echo "$BG_RESPONSE" | grep -q "name\|Showroom\|error"; then
    echo "   ✅ Backgrounds endpoint responding"
    echo "   Available backgrounds: $(echo $BG_RESPONSE | grep -o '"name":"[^"]*"' | wc -l)"
else
    echo "   ❌ Backgrounds endpoint failed"
fi

echo ""
echo "6️⃣ Listing Test Images..."
echo "   Photos:"
ls -1 "$TEST_DATA_DIR/photos/" 2>/dev/null | while read f; do echo "      - $f"; done
echo "   Videos:"
ls -1 "$TEST_DATA_DIR/videos/" 2>/dev/null | while read f; do echo "      - $f"; done

echo ""
echo "===================================="
echo "✅ Test Suite Complete"
echo ""
echo "📝 Next Steps:"
echo "   1. Check RabbitMQ UI: http://localhost:15679"
echo "   2. Check Swagger UI: http://localhost:5070/swagger"
echo "   3. View worker logs: docker logs ai-worker-clip-cpu"
echo ""
