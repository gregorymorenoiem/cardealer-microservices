#!/bin/bash

# Script para eliminar todas las definiciones de servicios *-db del compose.yaml
# manteniendo solo postgres_db

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Crear backup
cp compose.yaml compose.yaml.backup-before-db-removal

echo "Eliminando servicios de DB individuales..."

# Lista de todos los servicios -db a eliminar
services_to_remove=(
    "authservice-db"
    "notificationservice-db"
    "userservice-db"
    "roleservice-db"
    "adminservice-db"
    "mediaservice-db"
    "reportsservice-db"
    "billingservice-db"
    "financeservice-db"
    "messagebusservice-db"
    "vehiclessaleservice-db"
    "invoicingservice-db"
    "crmservice-db"
    "contactservice-db"
    "appointmentservice-db"
    "marketingservice-db"
    "realestateservice-db"
    "auditservice-db"
    "backupdrservice-db"
    "schedulerservice-db"
    "configurationservice-db"
    "featuretoggleservice-db"
    "ratelimitingservice-db"
    "maintenanceservice-db"
    "comparisonservice-db"
    "alertservice-db"
)

# Para cada servicio, eliminar su definición completa
for service in "${services_to_remove[@]}"; do
    echo "Eliminando $service..."
    
    # Encontrar la línea donde comienza la definición del servicio
    start_line=$(grep -n "^  $service:" compose.yaml | cut -d: -f1)
    
    if [[ -n "$start_line" ]]; then
        # Encontrar la siguiente definición de servicio (que empiece con 2 espacios y termine en :)
        # o el final del archivo
        end_line=$(tail -n +$((start_line + 1)) compose.yaml | grep -n "^  [a-zA-Z].*:$\|^volumes:\|^networks:" | head -1 | cut -d: -f1)
        
        if [[ -n "$end_line" ]]; then
            # Ajustar el número de línea
            end_line=$((start_line + end_line - 1))
        else
            # Si no encuentra otra sección, ir hasta el final del archivo
            end_line=$(wc -l < compose.yaml)
        fi
        
        # Crear un archivo temporal sin las líneas del servicio
        head -n $((start_line - 1)) compose.yaml > compose.tmp
        tail -n +$((end_line)) compose.yaml >> compose.tmp
        
        # Reemplazar el archivo original
        mv compose.tmp compose.yaml
        
        echo "✅ Eliminado $service (líneas $start_line-$((end_line-1)))"
    else
        echo "❌ No se encontró $service"
    fi
done

echo ""
echo "🔍 Verificando que no queden servicios -db..."
remaining=$(grep -c "^  .*-db:$" compose.yaml)

if [[ $remaining -eq 0 ]]; then
    echo "✅ ¡Todos los servicios *-db individuales han sido eliminados!"
    echo "✅ Solo queda postgres_db consolidado"
else
    echo "⚠️  Aún quedan $remaining servicios -db por eliminar"
    echo "Servicios restantes:"
    grep "^  .*-db:$" compose.yaml
fi

# Verificar que postgres_db aún existe
if grep -q "^  postgres_db:" compose.yaml; then
    echo "✅ postgres_db sigue presente"
else
    echo "❌ ERROR: postgres_db fue eliminado accidentalmente!"
fi

echo ""
echo "📊 Estadísticas finales:"
echo "- Líneas totales: $(wc -l < compose.yaml)"
echo "- Backup creado en: compose.yaml.backup-before-db-removal"