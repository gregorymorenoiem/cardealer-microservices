#!/bin/bash
# Script para iniciar el frontend de forma persistente

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/frontend/web-next

# Matar procesos anteriores
pkill -f "next dev" 2>/dev/null
sleep 1

# Limpiar cache
rm -rf .next

# Iniciar en foreground (mantener vivo)
exec pnpm dev
