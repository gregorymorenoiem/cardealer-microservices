#!/bin/bash
# Process local images using the AI Worker Docker container
# Usage: ./run_local_process.sh

echo "=============================================="
echo "🖼️  AI Processing - Docker Worker"
echo "=============================================="

# Navigate to test-data directory
cd "$(dirname "$0")"

# Check if photos exist
if [ ! -d "photos" ] || [ -z "$(ls -A photos 2>/dev/null)" ]; then
    echo "❌ No photos found in photos/ directory"
    exit 1
fi

echo "📷 Photos found:"
ls -la photos/
echo ""

# Create processed directory
mkdir -p processed

# Run the worker container with local mounts
echo "🚀 Starting AI Worker with local mounts..."

docker run --rm -it \
    --name ai-local-processor \
    -v "$(pwd)/photos:/app/input:ro" \
    -v "$(pwd)/processed:/app/output" \
    -e PROCESS_LOCAL=true \
    -e INPUT_DIR=/app/input \
    -e OUTPUT_DIR=/app/output \
    ghcr.io/gregorymorenoiem/cardealer-ai-worker-sam2-cpu:latest \
    python3 /app/process_local_batch.py

echo ""
echo "✅ Processing complete!"
echo "📁 Output saved to: $(pwd)/processed/"
echo ""
echo "💡 View results at: http://localhost:8888"
echo "   (Run: python3 serve.py)"
