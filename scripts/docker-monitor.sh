#!/bin/bash
################################################################################
# Docker Disk Monitor
# 
# Monitora el uso de espacio en disco y Docker
# Ejecutar: bash docker-monitor.sh
#
# Muestra:
# - Espacio del disco
# - Uso de Docker (imágenes, contenedores, volúmenes, cache)
# - Contenedores activos
# - Imágenes y tamaños
#
# Autor: Gregory Moreno
# Fecha: Enero 2026
################################################################################

set -e

echo ""
echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║           🐳 DOCKER DISK MONITOR - MONITOR DE DISCO                ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

# Verificar que Docker esté corriendo
if ! docker ps > /dev/null 2>&1; then
    echo "❌ Docker no está en ejecución"
    exit 1
fi

# ============================================================================
# 1. ESPACIO DEL DISCO
# ============================================================================
echo "📊 ESPACIO DEL DISCO"
echo "─────────────────────────────────────────────────────────────────────"

DISK_INFO=$(df -h / | tail -1)
DISK_USED=$(echo $DISK_INFO | awk '{print $3}')
DISK_AVAIL=$(echo $DISK_INFO | awk '{print $4}')
DISK_PERCENT=$(echo $DISK_INFO | awk '{print $5}')

echo "Usado:       $DISK_USED"
echo "Disponible: $DISK_AVAIL"
echo "Porcentaje: $DISK_PERCENT"

# Alertas
PERCENT_NUM=${DISK_PERCENT%\%}
if [ "$PERCENT_NUM" -gt 90 ]; then
    echo "🚨 CRÍTICO: Disco > 90%, limpieza urgente"
elif [ "$PERCENT_NUM" -gt 80 ]; then
    echo "⚠️  ADVERTENCIA: Disco > 80%, considera limpiar"
elif [ "$PERCENT_NUM" -gt 60 ]; then
    echo "ℹ️  INFO: Disco > 60%, monitorear"
else
    echo "✅ OK: Disco en buen estado"
fi

echo ""

# ============================================================================
# 2. USO DE DOCKER
# ============================================================================
echo "🐳 USO DE DOCKER"
echo "─────────────────────────────────────────────────────────────────────"

docker system df

echo ""

# ============================================================================
# 3. CONTENEDORES ACTIVOS
# ============================================================================
echo "🔧 CONTENEDORES EN EJECUCIÓN"
echo "─────────────────────────────────────────────────────────────────────"

ACTIVE_CONTAINERS=$(docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Size}}" 2>/dev/null)

if [ -z "$ACTIVE_CONTAINERS" ]; then
    echo "Ninguno"
else
    echo "$ACTIVE_CONTAINERS"
fi

echo ""

# ============================================================================
# 4. CONTENEDORES PARADOS
# ============================================================================
STOPPED_COUNT=$(docker ps -a --filter "status=exited" --format "{{.Names}}" | wc -l)

echo "⏹️  CONTENEDORES PARADOS: $STOPPED_COUNT"
echo "─────────────────────────────────────────────────────────────────────"

if [ "$STOPPED_COUNT" -gt 0 ]; then
    echo "⚠️  Hay contenedores parados que podrían liberarse:"
    docker ps -a --filter "status=exited" --format "table {{.Names}}\t{{.CreatedAt}}"
else
    echo "✅ Sin contenedores parados"
fi

echo ""

# ============================================================================
# 5. IMÁGENES
# ============================================================================
echo "🖼️  IMÁGENES DOCKER (TOP 10 POR TAMAÑO)"
echo "─────────────────────────────────────────────────────────────────────"

docker images --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}" | sort -k3 -hr | head -11

echo ""

# ============================================================================
# 6. RECOMENDACIONES
# ============================================================================
echo "💡 RECOMENDACIONES"
echo "─────────────────────────────────────────────────────────────────────"

if [ "$PERCENT_NUM" -gt 80 ]; then
    echo "Ejecutar limpieza:"
    echo "  docker system prune -a --volumes"
    echo ""
fi

DOCKER_SIZE=$(docker system df --format "{{.Size}}" | head -1 | sed 's/[^0-9]*//g')

if [ "$DOCKER_SIZE" -gt 30000 ]; then
    echo "Docker usa > 30GB, considera:"
    echo "  docker image prune -a"
    echo "  docker builder prune -a"
    echo ""
fi

if [ "$STOPPED_COUNT" -gt 5 ]; then
    echo "Muchos contenedores parados:"
    echo "  docker container prune"
    echo ""
fi

echo "✅ Monitor completado"
echo ""
