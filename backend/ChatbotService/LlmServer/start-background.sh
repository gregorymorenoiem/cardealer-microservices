#!/bin/bash
# Start LLM server natively in background with log file
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOG_FILE="/tmp/llm-server.log"
PID_FILE="/tmp/llm-server.pid"

export MODEL_PATH="$SCRIPT_DIR/models/okla-llama3-8b-q4_k_m.gguf"
export HOST=0.0.0.0
export PORT=8000
export N_CTX=2048
export N_GPU_LAYERS=99
export N_THREADS=4
export N_BATCH=512
export MAX_TOKENS=200

# Kill any existing process
if [ -f "$PID_FILE" ]; then
    kill -9 $(cat "$PID_FILE") 2>/dev/null
    rm "$PID_FILE"
fi
kill -9 $(lsof -ti:8000) 2>/dev/null
sleep 1

echo "Starting OKLA LLM Server (Native Metal)..."
"$SCRIPT_DIR/.venv/bin/python3" "$SCRIPT_DIR/server.py" > "$LOG_FILE" 2>&1 &
echo $! > "$PID_FILE"
echo "PID: $(cat $PID_FILE)"
echo "Log: $LOG_FILE"

# Wait for model to load
for i in $(seq 1 30); do
    sleep 2
    if curl -s http://localhost:8000/health > /dev/null 2>&1; then
        echo "✅ LLM Server ready!"
        curl -s http://localhost:8000/health | python3 -m json.tool
        exit 0
    fi
    echo "  Loading model... ($i)"
done

echo "❌ Server failed to start. Check $LOG_FILE"
cat "$LOG_FILE" | tail -20
exit 1
