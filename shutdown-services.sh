#!/bin/bash

# 🛑 Script para bajar servicios OKLA de forma ordenada

set -e

cd "$(dirname "$0")"

echo "════════════════════════════════════════════════════════════════════════════"
echo "🛑 DETENIENDO PLATAFORMA OKLA"
echo "════════════════════════════════════════════════════════════════════════════"

# Opciones
REMOVE_VOLUMES=${1:-"--keep-data"}

echo ""
if [ "$REMOVE_VOLUMES" = "--remove-volumes" ]; then
    echo "⚠️ MODO: Eliminar contenedores Y volúmenes (DESTRUCTIVO)"
    echo ""
    docker compose down -v
else
    echo "📦 MODO: Detener contenedores pero mantener volúmenes (SEGURO)"
    echo ""
    docker compose down
fi

echo ""
echo "════════════════════════════════════════════════════════════════════════════"
echo "✅ Servicios detenidos"
echo "════════════════════════════════════════════════════════════════════════════"
echo ""
echo "Uso:"
echo "  ./shutdown-services.sh               # Mantiene datos"
echo "  ./shutdown-services.sh --remove-volumes  # Elimina todo"
echo ""
