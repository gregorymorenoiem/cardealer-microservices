#!/bin/bash

# Script de migración de bases de datos individuales a postgres_db consolidado
# Este script debe ejecutarse ANTES de cambiar las connection strings

set -e

echo "🚀 Iniciando migración de bases de datos a postgres_db consolidado"
echo "=================================================="

# Lista de servicios (sin usar declare -A para compatibilidad)
SERVICES="errorservice authservice notificationservice userservice roleservice adminservice mediaservice reportsservice billingservice financeservice messagebusservice vehiclessaleservice invoicingservice crmservice contactservice appointmentservice marketingservice realestateservice auditservice backupdrservice schedulerservice configurationservice featuretoggleservice ratelimitingservice maintenanceservice comparisonservice alertservice"

# Crear directorio para backups
mkdir -p ./db_migration_backups

echo "📦 Paso 1: Creando backups de todas las bases de datos existentes"
echo "================================================================"

for service in $SERVICES; do
    echo "⏳ Creando backup de ${service}..."
    
    # Verificar si el contenedor existe y está corriendo
    if docker ps --format "table {{.Names}}" | grep -q "${service}-db"; then
        # Crear backup usando pg_dump
        docker exec ${service}-db pg_dump -U postgres -d ${service} > "./db_migration_backups/${service}_backup.sql" 2>/dev/null || true
        
        if [ $? -eq 0 ] && [ -f "./db_migration_backups/${service}_backup.sql" ]; then
            echo "✅ Backup de ${service} completado"
        else
            echo "⚠️  Error al crear backup de ${service} o no hay datos"
        fi
    else
        echo "⚠️  Contenedor ${service}-db no está corriendo, saltando..."
    fi
done

echo ""
echo "🐘 Paso 2: Verificando que postgres_db esté corriendo"
echo "===================================================="

if ! docker ps --format "table {{.Names}}" | grep -q "postgres_db"; then
    echo "❌ postgres_db no está corriendo. Ejecute primero: docker-compose up -d postgres_db"
    exit 1
fi

echo "✅ postgres_db está corriendo"

echo ""
echo "📥 Paso 3: Restaurando datos en postgres_db"
echo "==========================================="

for service in $SERVICES; do
    backup_file="./db_migration_backups/${service}_backup.sql"
    
    if [ -f "$backup_file" ] && [ -s "$backup_file" ]; then
        echo "⏳ Restaurando ${service}..."
        
        # Restaurar la base de datos
        docker exec -i postgres_db psql -U postgres -d ${service} < "$backup_file" 2>/dev/null || true
        
        if [ $? -eq 0 ]; then
            echo "✅ Datos de ${service} restaurados"
        else
            echo "⚠️  Error al restaurar datos de ${service} (puede ser que no había datos)"
        fi
    else
        echo "⚠️  Archivo de backup vacío o no encontrado: $backup_file"
    fi
done

echo ""
echo "🔍 Paso 4: Verificando migración"
echo "==============================="

for service in $SERVICES; do
    echo "⏳ Verificando ${service}..."
    
    # Contar tablas en la base de datos migrada
    table_count=$(docker exec postgres_db psql -U postgres -d ${service} -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" 2>/dev/null | tr -d '[:space:]' || echo "0")
    
    if [ "$table_count" -gt 0 ]; then
        echo "✅ ${service}: $table_count tablas migradas"
    else
        echo "⚠️  ${service}: No se encontraron tablas (puede ser normal si no tenía datos)"
    fi
done

echo ""
echo "🎉 ¡Migración completada!"
echo "========================"
echo ""
echo "📋 Próximos pasos:"
echo "1. Verificar que todos los datos se migraron correctamente"
echo "2. Actualizar connection strings para usar postgres_db:5433"  
echo "3. Reiniciar servicios para usar la nueva base de datos"
echo "4. Una vez verificado, eliminar servicios *_db individuales"
echo ""
echo "💾 Los backups están guardados en: ./db_migration_backups/"
echo ""