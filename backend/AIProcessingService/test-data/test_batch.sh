#!/bin/bash
# Batch test script for AIProcessingService
# Uses local images from datasets folder

MEDIA_SERVICE="http://localhost:15020"
AI_SERVICE="http://localhost:5070"
JWT="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0LXVzZXItMDAxIiwibmFtZSI6IlRlc3QgVXNlciIsImVtYWlsIjoidGVzdEBva2xhLmNvbS5kbyIsInJvbGUiOiJBZG1pbiIsImlhdCI6MTc2OTQzMjY1NSwiZXhwIjoxNzY5NTE5MDU1LCJpc3MiOiJPS0xBIiwiYXVkIjoiT0tMQS1Vc2VycyJ9.jrNyaXr0fJThSGOKg3r0vBBu7VKZ5NFXNM7Wfyh_-zU"

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DATASETS_DIR="$SCRIPT_DIR/datasets"

process_image() {
    local IMAGE_FILE="$1"
    local IMAGE_NAME=$(basename "$IMAGE_FILE" .jpg)
    local VEHICLE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
    
    echo ""
    echo "🚗 Processing: $IMAGE_NAME"
    echo "   Vehicle ID: $VEHICLE_ID"
    
    # Upload to MediaService
    UPLOAD_RESPONSE=$(curl -s -X POST "$MEDIA_SERVICE/api/media/upload/image" \
        -F "file=@$IMAGE_FILE" \
        -F "folder=ai-test-vehicles")
    
    IMAGE_URL=$(echo "$UPLOAD_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('url', ''))" 2>/dev/null)
    
    if [ -z "$IMAGE_URL" ]; then
        echo "   ❌ Upload failed"
        return 1
    fi
    
    echo "   ✅ Uploaded to S3"
    
    # Create request JSON
    REQUEST_JSON=$(cat << EOF
{"vehicleId": "$VEHICLE_ID", "imageUrl": "$IMAGE_URL", "type": "FullPipeline"}
EOF
)
    
    # Send to AI Processing
    PROCESS_RESPONSE=$(curl -s -X POST "$AI_SERVICE/api/aiprocessing/process" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $JWT" \
        -d "$REQUEST_JSON")
    
    JOB_ID=$(echo "$PROCESS_RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('jobId', ''))" 2>/dev/null)
    
    if [ -z "$JOB_ID" ]; then
        echo "   ❌ Processing failed: $PROCESS_RESPONSE"
        return 1
    fi
    
    echo "   ✅ Job queued: $JOB_ID"
    return 0
}

echo "=============================================="
echo "🧪 AI PROCESSING SERVICE - BATCH TEST"
echo "=============================================="
echo "Media Service: $MEDIA_SERVICE"
echo "AI Service: $AI_SERVICE"
echo ""

# Count images
TOTAL_IMAGES=$(ls -1 "$DATASETS_DIR"/*.jpg 2>/dev/null | wc -l | tr -d ' ')
echo "📁 Found $TOTAL_IMAGES images in datasets/"

# Process images (limit to first 5 for quick test)
COUNT=0
MAX_IMAGES=${1:-5}  # Default to 5, or use first argument

for IMAGE in "$DATASETS_DIR"/*.jpg; do
    if [ $COUNT -ge $MAX_IMAGES ]; then
        break
    fi
    
    process_image "$IMAGE"
    COUNT=$((COUNT + 1))
done

echo ""
echo "=============================================="
echo "📊 SUMMARY"
echo "=============================================="
echo "Processed: $COUNT images"
echo ""
echo "Check worker logs with:"
echo "  docker-compose -f docker-compose.cpu.yaml logs --tail 50 ai-worker-sam2"
