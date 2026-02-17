#!/bin/bash
# Start LLM server natively on macOS with Metal GPU acceleration
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

export MODEL_PATH="$SCRIPT_DIR/models/okla-llama3-8b-q4_k_m.gguf"
export HOST=0.0.0.0
export PORT=8000
export N_CTX=4096
export N_GPU_LAYERS=99
export N_THREADS=4
export N_BATCH=512
export MAX_TOKENS=600

# Kill any existing process on port 8000
kill -9 $(lsof -ti:8000) 2>/dev/null
sleep 1

echo "Starting OKLA LLM Server (Native Metal)..."
echo "Model: $MODEL_PATH"
echo "GPU Layers: $N_GPU_LAYERS, Context: $N_CTX"

"$SCRIPT_DIR/.venv/bin/python3" "$SCRIPT_DIR/server.py"
