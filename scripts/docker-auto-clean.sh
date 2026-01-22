#!/bin/bash
################################################################################
# Docker Auto Clean
#
# Limpia automáticamente Docker si el disco está lleno
# Ejecutar: bash docker-auto-clean.sh
#
# Niveles de limpieza:
# - Nivel 1 (<70%): Sin limpieza
# - Nivel 2 (70-80%): Prune normal
# - Nivel 3 (80-90%): Prune agresivo
# - Nivel 4 (>90%): Prune forzado + builder cache
#
# Autor: Gregory Moreno
# Fecha: Enero 2026
################################################################################

set -e

echo ""
echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║         🧹 DOCKER AUTO CLEAN - LIMPIEZA AUTOMÁTICA                 ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

# Verificar que Docker esté corriendo
if ! docker ps > /dev/null 2>&1; then
    echo "❌ Docker no está en ejecución"
    exit 1
fi

# Obtener uso de disco
DISK_INFO=$(df / | tail -1)
DISK_USED=$(echo $DISK_INFO | awk '{print $3}')
DISK_PERCENT=$(echo $DISK_INFO | awk '{print $5}')
PERCENT_NUM=${DISK_PERCENT%\%}

echo "📊 Estado del disco: $DISK_USED usado ($DISK_PERCENT)"
echo ""

# ============================================================================
# DECISIÓN DE LIMPIEZA
# ============================================================================

if [ "$PERCENT_NUM" -lt 70 ]; then
    echo "✅ Disco en buen estado ($PERCENT_NUM%), sin limpieza necesaria"
    exit 0

elif [ "$PERCENT_NUM" -lt 80 ]; then
    echo "⚠️  Disco > 70% ($PERCENT_NUM%), ejecutando limpieza normal..."
    echo ""
    docker system prune --volumes -f
    echo "✅ Limpieza normal completada"

elif [ "$PERCENT_NUM" -lt 90 ]; then
    echo "⚠️  ADVERTENCIA: Disco > 80% ($PERCENT_NUM%), limpieza agresiva..."
    echo ""
    
    echo "1️⃣  Deteniendo contenedores..."
    docker-compose down 2>/dev/null || true
    
    echo "2️⃣  Removiendo contenedores parados..."
    docker container prune -f
    
    echo "3️⃣  Removiendo imágenes no usadas..."
    docker image prune -a -f
    
    echo "4️⃣  Removiendo volúmenes no usados..."
    docker volume prune -f
    
    echo "5️⃣  Limpiando cache de build..."
    docker builder prune -a -f
    
    echo "✅ Limpieza agresiva completada"

else
    echo "🚨 CRÍTICO: Disco > 90% ($PERCENT_NUM%), limpieza forzada URGENTE..."
    echo ""
    
    echo "1️⃣  Deteniendo Docker..."
    osascript -e 'quit app "Docker"' 2>/dev/null || true
    sleep 5
    
    echo "2️⃣  Esperando a que Docker se cierre completamente..."
    sleep 30
    
    echo "3️⃣  Reiniciando Docker..."
    open -a Docker
    sleep 60
    
    echo "4️⃣  Ejecutando limpieza forzada..."
    docker system prune -a --volumes --force || true
    docker builder prune -a --force || true
    
    echo "✅ Limpieza FORZADA completada"
fi

# ============================================================================
# VERIFICACIÓN FINAL
# ============================================================================

echo ""
echo "📊 Verificación final:"
echo ""

NEW_DISK_INFO=$(df / | tail -1)
NEW_DISK_USED=$(echo $NEW_DISK_INFO | awk '{print $3}')
NEW_PERCENT=$(echo $NEW_DISK_INFO | awk '{print $5}')

echo "Nuevo uso: $NEW_DISK_USED ($NEW_PERCENT)"

DOCKER_SIZE=$(docker system df --format "{{.Size}}" 2>/dev/null | head -1 || echo "N/A")
echo "Docker size: $DOCKER_SIZE"

echo ""
echo "✅ Limpieza finalizada"
echo ""
