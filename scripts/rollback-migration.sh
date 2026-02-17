#!/bin/bash

# Script de rollback para regresar a bases de datos individuales si hay problemas

set -e

echo "🔄 Iniciando rollback a bases de datos individuales"
echo "=================================================="

echo "📋 Paso 1: Deteniendo postgres_db"
echo "================================="
docker-compose stop postgres_db || true

echo ""
echo "🐘 Paso 2: Reiniciando servicios de bases de datos individuales"
echo "=============================================================="
docker-compose up -d errorservice-db authservice-db notificationservice-db userservice-db roleservice-db adminservice-db mediaservice-db reportsservice-db billingservice-db financeservice-db messagebusservice-db vehiclessaleservice-db || true

echo ""
echo "⏳ Paso 3: Esperando que las bases de datos estén listas..."
echo "=========================================================="
sleep 30

echo ""
echo "🔍 Paso 4: Verificando que las bases de datos están corriendo"
echo "==========================================================="

declare -a DB_CONTAINERS=("errorservice-db" "authservice-db" "notificationservice-db" "userservice-db" "roleservice-db" "adminservice-db" "mediaservice-db" "reportsservice-db" "billingservice-db")

for container in "${DB_CONTAINERS[@]}"; do
    if docker ps --format "table {{.Names}}" | grep -q "$container"; then
        echo "✅ $container está corriendo"
    else
        echo "❌ $container no está corriendo"
    fi
done

echo ""
echo "🎯 Rollback completado"
echo "===================="
echo ""
echo "📋 Para completar el rollback:"
echo "1. Verifique que todos los servicios *-db estén corriendo"
echo "2. Reinicie los servicios de aplicación: docker-compose restart"
echo "3. Verifique que las aplicaciones puedan conectar a sus bases de datos"
echo ""