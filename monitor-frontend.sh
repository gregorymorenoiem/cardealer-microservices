#!/bin/bash
# Monitor del frontend - detecta caídas y reinicia automáticamente

FRONTEND_DIR="/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/frontend/web-next"
LOG_FILE="/tmp/frontend.log"
MONITOR_LOG="/tmp/frontend-monitor.log"

check_frontend() {
    curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/ 2>/dev/null
}

restart_frontend() {
    echo "[$(date)] Reiniciando frontend..." >> "$MONITOR_LOG"
    pkill -f "next dev" 2>/dev/null
    sleep 2
    cd "$FRONTEND_DIR"
    rm -rf .next
    NODE_OPTIONS="--max-old-space-size=4096" nohup pnpm dev > "$LOG_FILE" 2>&1 &
    sleep 5
}

echo "[$(date)] Monitor iniciado" >> "$MONITOR_LOG"

while true; do
    STATUS=$(check_frontend)
    
    if [ "$STATUS" != "200" ]; then
        echo "[$(date)] Frontend caído (status: $STATUS). Reiniciando..." >> "$MONITOR_LOG"
        
        # Capturar últimos logs antes de reiniciar
        echo "[$(date)] Últimos logs antes de caída:" >> "$MONITOR_LOG"
        tail -50 "$LOG_FILE" >> "$MONITOR_LOG" 2>/dev/null
        echo "---" >> "$MONITOR_LOG"
        
        restart_frontend
    fi
    
    sleep 10  # Verificar cada 10 segundos
done
