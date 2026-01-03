#!/bin/bash
# Script para renombrar completamente los microservicios
# Ejecutar desde: /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend

set -e

echo "🔄 Renombrando microservicios..."

# Array de servicios con sus nombres
declare -A SERVICES=(
    ["VehiclesSaleService"]="VehiclesSale"
    ["VehiclesRentService"]="VehiclesRent"
    ["PropertiesSaleService"]="PropertiesSale"
    ["PropertiesRentService"]="PropertiesRent"
)

BACKEND_DIR="/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend"

for SERVICE_DIR in "${!SERVICES[@]}"; do
    SERVICE_NAME="${SERVICES[$SERVICE_DIR]}"
    FULL_PATH="$BACKEND_DIR/$SERVICE_DIR"
    
    if [ ! -d "$FULL_PATH" ]; then
        echo "❌ $SERVICE_DIR no existe, saltando..."
        continue
    fi
    
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "📦 Procesando: $SERVICE_DIR"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    cd "$FULL_PATH"
    
    # 1. Renombrar carpetas de ProductService.* a {ServiceName}Service.*
    echo "📁 Renombrando carpetas..."
    for folder in ProductService.*; do
        if [ -d "$folder" ]; then
            NEW_NAME=$(echo "$folder" | sed "s/ProductService/${SERVICE_NAME}Service/g")
            if [ "$folder" != "$NEW_NAME" ] && [ ! -d "$NEW_NAME" ]; then
                mv "$folder" "$NEW_NAME"
                echo "   ✅ $folder → $NEW_NAME"
            fi
        fi
    done
    
    # 2. Renombrar archivos .csproj
    echo "📄 Renombrando archivos .csproj..."
    find . -name "ProductService.*.csproj" -type f | while read file; do
        NEW_FILE=$(echo "$file" | sed "s/ProductService/${SERVICE_NAME}Service/g")
        if [ "$file" != "$NEW_FILE" ]; then
            mv "$file" "$NEW_FILE"
            echo "   ✅ $file → $NEW_FILE"
        fi
    done
    
    # 3. Renombrar archivos .sln
    echo "📄 Renombrando archivos .sln..."
    if [ -f "ProductService.sln" ]; then
        mv "ProductService.sln" "${SERVICE_NAME}Service.sln"
        echo "   ✅ ProductService.sln → ${SERVICE_NAME}Service.sln"
    fi
    
    # 4. Actualizar contenido de archivos
    echo "📝 Actualizando contenido de archivos..."
    
    # Actualizar todos los archivos .cs, .csproj, .sln, .json
    find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) | while read file; do
        if grep -q "ProductService" "$file" 2>/dev/null; then
            # Reemplazar namespaces y referencias
            sed -i '' "s/ProductService\./${SERVICE_NAME}Service./g" "$file"
            sed -i '' "s/ProductService/${SERVICE_NAME}Service/g" "$file"
            sed -i '' "s/productservice/${SERVICE_NAME,,}service/g" "$file"
        fi
    done
    echo "   ✅ Contenido actualizado"
    
    echo "✅ $SERVICE_DIR completado"
done

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "✅ ¡Renombramiento completado!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
