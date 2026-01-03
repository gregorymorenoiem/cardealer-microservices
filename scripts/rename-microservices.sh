#!/bin/bash
# Script para renombrar todos los archivos y contenidos de los microservicios copiados

set -e

echo "🔧 Renombrando VehiclesRentService..."

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/VehiclesRentService

# Renombrar archivos .csproj
find . -name "ProductService*.csproj" -type f | while read file; do
    newname=$(echo "$file" | sed 's/ProductService/VehiclesRentService/g')
    mv "$file" "$newname"
    echo "  Renamed: $file → $newname"
done

# Renombrar carpetas
for dir in ProductService.*; do
    if [ -d "$dir" ]; then
        newdir=$(echo "$dir" | sed 's/ProductService/VehiclesRentService/g')
        mv "$dir" "$newdir"
        echo "  Renamed folder: $dir → $newdir"
    fi
done

# Renombrar .sln
if [ -f "ProductService.sln" ]; then
    mv ProductService.sln VehiclesRentService.sln
    echo "  Renamed: ProductService.sln → VehiclesRentService.sln"
fi

# Actualizar contenido de todos los archivos
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) -exec sed -i '' 's/ProductService/VehiclesRentService/g' {} +
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) -exec sed -i '' 's/productservice/vehiclesrentservice/g' {} +

# Actualizar puerto en Dockerfile y configs
find . -type f -name "*.json" -exec sed -i '' 's/15006/15070/g' {} +

echo "✅ VehiclesRentService actualizado (puerto 15070)"

# ============================================

echo ""
echo "🔧 Renombrando PropertiesSaleService..."

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/PropertiesSaleService

# Renombrar archivos .csproj
find . -name "ProductService*.csproj" -type f | while read file; do
    newname=$(echo "$file" | sed 's/ProductService/PropertiesSaleService/g')
    mv "$file" "$newname"
    echo "  Renamed: $file → $newname"
done

# Renombrar carpetas
for dir in ProductService.*; do
    if [ -d "$dir" ]; then
        newdir=$(echo "$dir" | sed 's/ProductService/PropertiesSaleService/g')
        mv "$dir" "$newdir"
        echo "  Renamed folder: $dir → $newdir"
    fi
done

# Renombrar .sln
if [ -f "ProductService.sln" ]; then
    mv ProductService.sln PropertiesSaleService.sln
    echo "  Renamed: ProductService.sln → PropertiesSaleService.sln"
fi

# Actualizar contenido de todos los archivos
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) -exec sed -i '' 's/ProductService/PropertiesSaleService/g' {} +
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) -exec sed -i '' 's/productservice/propertiessaleservice/g' {} +

# Actualizar puerto
find . -type f -name "*.json" -exec sed -i '' 's/15006/15071/g' {} +

echo "✅ PropertiesSaleService actualizado (puerto 15071)"

# ============================================

echo ""
echo "🔧 Renombrando PropertiesRentService..."

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/PropertiesRentService

# Renombrar archivos .csproj
find . -name "ProductService*.csproj" -type f | while read file; do
    newname=$(echo "$file" | sed 's/ProductService/PropertiesRentService/g')
    mv "$file" "$newname"
    echo "  Renamed: $file → $newname"
done

# Renombrar carpetas
for dir in ProductService.*; do
    if [ -d "$dir" ]; then
        newdir=$(echo "$dir" | sed 's/ProductService/PropertiesRentService/g')
        mv "$dir" "$newdir"
        echo "  Renamed folder: $dir → $newdir"
    fi
done

# Renombrar .sln
if [ -f "ProductService.sln" ]; then
    mv ProductService.sln PropertiesRentService.sln
    echo "  Renamed: ProductService.sln → PropertiesRentService.sln"
fi

# Actualizar contenido de todos los archivos
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) -exec sed -i '' 's/ProductService/PropertiesRentService/g' {} +
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "Dockerfile*" \) -exec sed -i '' 's/productservice/propertiesrentservice/g' {} +

# Actualizar puerto
find . -type f -name "*.json" -exec sed -i '' 's/15006/15072/g' {} +

echo "✅ PropertiesRentService actualizado (puerto 15072)"

# ============================================

echo ""
echo "🎉 ¡Todos los servicios renombrados exitosamente!"
echo ""
echo "📦 Servicios creados:"
echo "  1. VehiclesSaleService (puerto 15006) - Ya existía como ProductService"
echo "  2. VehiclesRentService (puerto 15070) ✅"
echo "  3. PropertiesSaleService (puerto 15071) ✅"
echo "  4. PropertiesRentService (puerto 15072) ✅"
