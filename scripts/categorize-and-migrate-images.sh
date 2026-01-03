#!/bin/bash

# Script para categorizar y migrar imágenes por tipo de negocio
# =============================================================

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuración AWS S3
BUCKET_NAME="okla-images-2026"
REGION="us-east-2"

# Directorio temporal
TEMP_DIR="./temp-unsplash-downloads"
FRONTEND_DIR="./frontend/web/src"

echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   Categorización y Migración de Imágenes por Tipo     ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
echo ""

# 1. Crear estructura de carpetas por categoría
echo -e "${YELLOW}📁 Creando estructura de carpetas...${NC}"
mkdir -p "$TEMP_DIR/vehicles/sale"
mkdir -p "$TEMP_DIR/vehicles/rent"
mkdir -p "$TEMP_DIR/properties/sale"
mkdir -p "$TEMP_DIR/properties/rent"
echo -e "${GREEN}✅ Estructura creada${NC}"
echo ""

# 2. Analizar y categorizar imágenes
echo -e "${YELLOW}🔍 Analizando y categorizando imágenes...${NC}"

# Función para extraer URLs de un archivo y categorizarlas
categorize_images() {
    local file=$1
    local category=$2
    local type=$3
    
    # Extraer URLs únicas
    grep -o "https://images.unsplash.com/photo-[a-zA-Z0-9_-]*[?][^'\"]*" "$file" 2>/dev/null | sort -u | while read url; do
        echo "$url|$category|$type"
    done
}

# Crear archivo de mapeo
MAPPING_FILE="$TEMP_DIR/image-categories.txt"
> "$MAPPING_FILE"

# Vehículos (todo mockVehicles.ts es venta de vehículos)
if [ -f "$FRONTEND_DIR/data/mockVehicles.ts" ]; then
    echo -e "${BLUE}  📝 Procesando: mockVehicles.ts (venta de vehículos)${NC}"
    categorize_images "$FRONTEND_DIR/data/mockVehicles.ts" "vehicles" "sale" >> "$MAPPING_FILE"
fi

# Propiedades - analizar por listingType
if [ -f "$FRONTEND_DIR/data/mockProperties.ts" ]; then
    echo -e "${BLUE}  📝 Procesando: mockProperties.ts (propiedades)${NC}"
    
    # Extraer secciones por listingType
    awk '/listingType:.*sale/{flag=1; buf=""} flag{buf=buf"\n"$0} /^  \},/{if(flag && buf~/listingType.*sale/) print buf; flag=0}' "$FRONTEND_DIR/data/mockProperties.ts" | \
        grep -o "https://images.unsplash.com/photo-[a-zA-Z0-9_-]*[?][^'\"]*" | sort -u | while read url; do
        echo "$url|properties|sale"
    done >> "$MAPPING_FILE"
    
    awk '/listingType:.*rent/{flag=1; buf=""} flag{buf=buf"\n"$0} /^  \},/{if(flag && buf~/listingType.*rent/) print buf; flag=0}' "$FRONTEND_DIR/data/mockProperties.ts" | \
        grep -o "https://images.unsplash.com/photo-[a-zA-Z0-9_-]*[?][^'\"]*" | sort -u | while read url; do
        echo "$url|properties|rent"
    done >> "$MAPPING_FILE"
fi

# HomePage, OklaHomePage, etc - asumir vehículos/venta por defecto
for page in HomePage OklaHomePage OklaPremiumPage OklaDetailPage; do
    if [ -f "$FRONTEND_DIR/pages/${page}.tsx" ]; then
        echo -e "${BLUE}  📝 Procesando: ${page}.tsx (vehículos)${NC}"
        categorize_images "$FRONTEND_DIR/pages/${page}.tsx" "vehicles" "sale" >> "$MAPPING_FILE"
    fi
done

# Admin y mensajes - vehículos/venta
for file in mockAdmin mockMessages mockDealers; do
    if [ -f "$FRONTEND_DIR/data/${file}.ts" ]; then
        echo -e "${BLUE}  📝 Procesando: ${file}.ts${NC}"
        categorize_images "$FRONTEND_DIR/data/${file}.ts" "vehicles" "sale" >> "$MAPPING_FILE"
    fi
done

# Remover duplicados y contar
sort -u "$MAPPING_FILE" -o "$MAPPING_FILE"
TOTAL_URLS=$(wc -l < "$MAPPING_FILE" | tr -d ' ')
echo ""
echo -e "${GREEN}✅ Encontradas $TOTAL_URLS URLs categorizadas${NC}"
echo ""

# 3. Mostrar resumen de categorización
echo -e "${YELLOW}📊 Resumen por categoría:${NC}"
echo -e "  🚗 Vehículos (venta):    $(grep "|vehicles|sale" "$MAPPING_FILE" | wc -l | tr -d ' ')"
echo -e "  🚗 Vehículos (renta):    $(grep "|vehicles|rent" "$MAPPING_FILE" | wc -l | tr -d ' ')"
echo -e "  🏠 Propiedades (venta):  $(grep "|properties|sale" "$MAPPING_FILE" | wc -l | tr -d ' ')"
echo -e "  🏠 Propiedades (renta):  $(grep "|properties|rent" "$MAPPING_FILE" | wc -l | tr -d ' ')"
echo ""

# 4. Descargar y organizar por categoría
echo -e "${YELLOW}📥 Descargando imágenes...${NC}"

DOWNLOADED=0
SKIPPED=0
FAILED=0

while IFS='|' read -r url category type; do
    # Extraer info de la URL
    PHOTO_ID=$(echo "$url" | grep -o "photo-[a-zA-Z0-9_-]*" | head -1)
    WIDTH=$(echo "$url" | grep -o "w=[0-9]*" | cut -d= -f2)
    HEIGHT=$(echo "$url" | grep -o "h=[0-9]*" | cut -d= -f2)
    
    if [ -z "$WIDTH" ]; then WIDTH="800"; fi
    if [ -z "$HEIGHT" ]; then HEIGHT="600"; fi
    
    FILENAME="${PHOTO_ID}-${WIDTH}x${HEIGHT}.jpg"
    LOCAL_PATH="$TEMP_DIR/$category/$type/$FILENAME"
    
    # Verificar si ya existe
    if [ -f "$LOCAL_PATH" ] && [ -s "$LOCAL_PATH" ]; then
        ((SKIPPED++))
        continue
    fi
    
    # Descargar
    BASE_URL=$(echo "$url" | cut -d'?' -f1)
    DOWNLOAD_URL="${BASE_URL}?w=${WIDTH}&h=${HEIGHT}&fit=crop&fm=jpg&q=80"
    
    if curl -sS -o "$LOCAL_PATH" "$DOWNLOAD_URL"; then
        if [ -f "$LOCAL_PATH" ] && [ -s "$LOCAL_PATH" ]; then
            FILE_SIZE=$(stat -f%z "$LOCAL_PATH" 2>/dev/null || stat -c%s "$LOCAL_PATH" 2>/dev/null)
            echo -e "${GREEN}✅ $category/$type/$FILENAME ($FILE_SIZE bytes)${NC}"
            ((DOWNLOADED++))
        else
            echo -e "${RED}❌ Error: archivo vacío - $FILENAME${NC}"
            rm -f "$LOCAL_PATH"
            ((FAILED++))
        fi
    else
        echo -e "${RED}❌ Error al descargar: $url${NC}"
        ((FAILED++))
    fi
    
    sleep 0.1
    
done < "$MAPPING_FILE"

echo ""
echo -e "${GREEN}📊 Resumen de descargas:${NC}"
echo -e "   ✅ Descargadas: $DOWNLOADED"
echo -e "   ⚠️  Omitidas: $SKIPPED"
echo -e "   ❌ Fallidas: $FAILED"
echo ""

# 5. Subir a S3 por categoría
echo -e "${YELLOW}☁️  Subiendo a AWS S3...${NC}"

UPLOADED=0

for category in vehicles properties; do
    for type in sale rent; do
        DIR="$TEMP_DIR/$category/$type"
        if [ ! -d "$DIR" ]; then continue; fi
        
        S3_PREFIX="frontend/assets/$category/$type"
        
        find "$DIR" -type f -name "*.jpg" | while read file; do
            FILENAME=$(basename "$file")
            S3_KEY="$S3_PREFIX/$FILENAME"
            
            # Verificar si ya existe en S3
            if aws s3api head-object --bucket "$BUCKET_NAME" --key "$S3_KEY" --region "$REGION" &>/dev/null; then
                echo -e "${YELLOW}⚠️  Ya en S3: $category/$type/$FILENAME${NC}"
                continue
            fi
            
            # Subir
            if aws s3 cp "$file" "s3://$BUCKET_NAME/$S3_KEY" \
                --region "$REGION" \
                --content-type "image/jpeg" \
                --metadata "source=unsplash,category=$category,type=$type" \
                --quiet; then
                echo -e "${GREEN}✅ S3: $category/$type/$FILENAME${NC}"
                ((UPLOADED++))
            else
                echo -e "${RED}❌ Error S3: $FILENAME${NC}"
            fi
        done
    done
done

echo ""
echo -e "${GREEN}📊 Total subidos a S3: $UPLOADED${NC}"
echo ""

# 6. Generar mapeo JSON
echo -e "${YELLOW}📝 Generando archivo de mapeo...${NC}"

JSON_FILE="$FRONTEND_DIR/config/s3-image-mapping.json"
mkdir -p "$(dirname "$JSON_FILE")"

cat > "$JSON_FILE" << 'EOFJS'
{
  "baseUrl": "https://okla-images-2026.s3.us-east-2.amazonaws.com",
  "categories": {
    "vehicles": {
      "sale": "frontend/assets/vehicles/sale",
      "rent": "frontend/assets/vehicles/rent"
    },
    "properties": {
      "sale": "frontend/assets/properties/sale",
      "rent": "frontend/assets/properties/rent"
    }
  },
  "mappings": {
EOFJS

# Generar mapeos
FIRST=true
while IFS='|' read -r url category type; do
    PHOTO_ID=$(echo "$url" | grep -o "photo-[a-zA-Z0-9_-]*" | head -1)
    WIDTH=$(echo "$url" | grep -o "w=[0-9]*" | cut -d= -f2)
    HEIGHT=$(echo "$url" | grep -o "h=[0-9]*" | cut -d= -f2)
    if [ -z "$WIDTH" ]; then WIDTH="800"; fi
    if [ -z "$HEIGHT" ]; then HEIGHT="600"; fi
    
    FILENAME="${PHOTO_ID}-${WIDTH}x${HEIGHT}.jpg"
    S3_URL="https://$BUCKET_NAME.s3.$REGION.amazonaws.com/frontend/assets/$category/$type/$FILENAME"
    
    if [ "$FIRST" = false ]; then
        echo "," >> "$JSON_FILE"
    fi
    echo -n "    \"$url\": \"$S3_URL\"" >> "$JSON_FILE"
    FIRST=false
done < "$MAPPING_FILE"

cat >> "$JSON_FILE" << 'EOFJS'

  }
}
EOFJS

echo -e "${GREEN}✅ Mapeo generado: $JSON_FILE${NC}"
echo ""

# 7. Resumen final
echo -e "${BLUE}╔════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                   RESUMEN FINAL                        ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${GREEN}✅ URLs procesadas:${NC} $TOTAL_URLS"
echo -e "${GREEN}✅ Imágenes descargadas:${NC} $DOWNLOADED"
echo -e "${GREEN}✅ Imágenes subidas a S3:${NC} $UPLOADED"
echo ""
echo -e "${YELLOW}📦 Estructura en S3:${NC}"
echo -e "   s3://$BUCKET_NAME/frontend/assets/vehicles/sale/"
echo -e "   s3://$BUCKET_NAME/frontend/assets/vehicles/rent/"
echo -e "   s3://$BUCKET_NAME/frontend/assets/properties/sale/"
echo -e "   s3://$BUCKET_NAME/frontend/assets/properties/rent/"
echo ""
echo -e "${GREEN}✅ ¡Migración completada!${NC}"
