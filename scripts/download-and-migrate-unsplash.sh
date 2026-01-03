#!/bin/bash

# Script para descargar imágenes de Unsplash y migrarlas a AWS S3
# ================================================================

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # Sin color

# Configuración AWS S3
BUCKET_NAME="okla-images-2026"
REGION="us-east-2"
S3_PREFIX="frontend/assets/vehicles"

# Directorio temporal
TEMP_DIR="./temp-unsplash-downloads"
FRONTEND_DIR="./frontend/web/src"

echo -e "${BLUE}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   Descarga y Migración de Imágenes Unsplash a AWS S3    ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""

# 1. Verificar dependencias
echo -e "${YELLOW}📋 Verificando dependencias...${NC}"

if ! command -v aws &> /dev/null; then
    echo -e "${RED}❌ ERROR: AWS CLI no está instalado${NC}"
    echo "Instalar con: brew install awscli"
    exit 1
fi

if ! command -v curl &> /dev/null; then
    echo -e "${RED}❌ ERROR: curl no está instalado${NC}"
    exit 1
fi

if ! command -v jq &> /dev/null; then
    echo -e "${YELLOW}⚠️  AVISO: jq no está instalado (se usará sed como alternativa)${NC}"
fi

echo -e "${GREEN}✅ Dependencias OK${NC}"
echo ""

# 2. Crear directorio temporal
echo -e "${YELLOW}📁 Creando directorio temporal...${NC}"
mkdir -p "$TEMP_DIR"
echo -e "${GREEN}✅ Directorio creado: $TEMP_DIR${NC}"
echo ""

# 3. Extraer URLs únicas de Unsplash del código
echo -e "${YELLOW}🔍 Buscando URLs de Unsplash en el código...${NC}"

# Buscar en todos los archivos TypeScript/JavaScript
# Usar egrep para compatibilidad con macOS
find "$FRONTEND_DIR" -type f \( -name "*.ts" -o -name "*.tsx" -o -name "*.js" -o -name "*.jsx" \) \
    -exec grep -h "https://images.unsplash.com/photo-" {} + \
    | grep -o "https://images.unsplash.com/photo-[a-zA-Z0-9_-]*[?][^'\"]*" \
    | sort -u > "$TEMP_DIR/unsplash-urls.txt"

TOTAL_URLS=$(wc -l < "$TEMP_DIR/unsplash-urls.txt" | tr -d ' ')
echo -e "${GREEN}✅ Encontradas $TOTAL_URLS URLs únicas de Unsplash${NC}"
echo ""

if [ "$TOTAL_URLS" -eq 0 ]; then
    echo -e "${RED}❌ No se encontraron URLs de Unsplash${NC}"
    exit 1
fi

# 4. Descargar imágenes
echo -e "${YELLOW}📥 Descargando imágenes desde Unsplash...${NC}"
DOWNLOADED=0
FAILED=0

# Crear archivo de mapeo URL → S3 URL
MAPPING_FILE="$TEMP_DIR/url-mapping.json"
echo "{" > "$MAPPING_FILE"
echo '  "baseUrl": "https://'"$BUCKET_NAME"'.s3.'"$REGION"'.amazonaws.com/'"$S3_PREFIX"'",' >> "$MAPPING_FILE"
echo '  "mappings": {' >> "$MAPPING_FILE"

FIRST_ENTRY=true

while IFS= read -r url; do
    # Extraer el ID de la foto (ej: photo-1560958089-b8a1929cea89)
    PHOTO_ID=$(echo "$url" | grep -o "photo-[a-zA-Z0-9\-]*" | head -1)
    
    if [ -z "$PHOTO_ID" ]; then
        echo -e "${RED}❌ No se pudo extraer ID de: $url${NC}"
        ((FAILED++))
        continue
    fi
    
    # Detectar tamaño de la imagen (w=800, w=400, etc)
    WIDTH=$(echo "$url" | grep -o "w=[0-9]*" | cut -d= -f2)
    HEIGHT=$(echo "$url" | grep -o "h=[0-9]*" | cut -d= -f2)
    
    if [ -z "$WIDTH" ]; then
        WIDTH="800"
    fi
    if [ -z "$HEIGHT" ]; then
        HEIGHT="600"
    fi
    
    # Nombre del archivo
    FILENAME="${PHOTO_ID}-${WIDTH}x${HEIGHT}.jpg"
    LOCAL_PATH="$TEMP_DIR/$FILENAME"
    S3_KEY="$S3_PREFIX/$FILENAME"
    
    # Verificar si ya existe localmente
    if [ -f "$LOCAL_PATH" ] && [ -s "$LOCAL_PATH" ]; then
        echo -e "${YELLOW}⚠️  Ya existe localmente: $FILENAME (omitiendo descarga)${NC}"
        
        # Agregar al mapeo aunque no se descargue
        if [ "$FIRST_ENTRY" = false ]; then
            echo "," >> "$MAPPING_FILE"
        fi
        echo -n "    \"$url\": \"https://$BUCKET_NAME.s3.$REGION.amazonaws.com/$S3_KEY\"" >> "$MAPPING_FILE"
        FIRST_ENTRY=false
        ((DOWNLOADED++))
        
        continue
    fi
    
    # Verificar si ya existe en S3
    if aws s3api head-object --bucket "$BUCKET_NAME" --key "$S3_KEY" --region "$REGION" &>/dev/null; then
        echo -e "${YELLOW}⚠️  Ya existe en S3: $FILENAME (omitiendo descarga)${NC}"
        
        # Agregar al mapeo aunque no se descargue
        if [ "$FIRST_ENTRY" = false ]; then
            echo "," >> "$MAPPING_FILE"
        fi
        echo -n "    \"$url\": \"https://$BUCKET_NAME.s3.$REGION.amazonaws.com/$S3_KEY\"" >> "$MAPPING_FILE"
        FIRST_ENTRY=false
        
        continue
    fi
    
    # Descargar imagen (usar URL sin parámetros para obtener mejor calidad)
    BASE_URL=$(echo "$url" | cut -d'?' -f1)
    DOWNLOAD_URL="${BASE_URL}?w=${WIDTH}&h=${HEIGHT}&fit=crop&fm=jpg&q=80"
    
    echo -e "${BLUE}⬇️  Descargando: $FILENAME...${NC}"
    
    if curl -sS -o "$LOCAL_PATH" "$DOWNLOAD_URL"; then
        # Verificar que se descargó correctamente
        if [ -f "$LOCAL_PATH" ] && [ -s "$LOCAL_PATH" ]; then
            FILE_SIZE=$(stat -f%z "$LOCAL_PATH" 2>/dev/null || stat -c%s "$LOCAL_PATH" 2>/dev/null)
            echo -e "${GREEN}✅ Descargado: $FILENAME ($FILE_SIZE bytes)${NC}"
            ((DOWNLOADED++))
            
            # Agregar al mapeo
            if [ "$FIRST_ENTRY" = false ]; then
                echo "," >> "$MAPPING_FILE"
            fi
            echo -n "    \"$url\": \"https://$BUCKET_NAME.s3.$REGION.amazonaws.com/$S3_KEY\"" >> "$MAPPING_FILE"
            FIRST_ENTRY=false
        else
            echo -e "${RED}❌ Error: archivo vacío o no válido${NC}"
            ((FAILED++))
        fi
    else
        echo -e "${RED}❌ Error al descargar: $url${NC}"
        ((FAILED++))
    fi
    
    # Pequeña pausa para no sobrecargar Unsplash
    sleep 0.2
    
done < "$TEMP_DIR/unsplash-urls.txt"

# Cerrar JSON
echo "" >> "$MAPPING_FILE"
echo "  }" >> "$MAPPING_FILE"
echo "}" >> "$MAPPING_FILE"

echo ""
echo -e "${GREEN}📊 Resumen de descargas:${NC}"
echo -e "   ✅ Descargadas: $DOWNLOADED"
echo -e "   ❌ Fallidas: $FAILED"
echo ""

# 5. Subir a S3
echo -e "${YELLOW}☁️  Subiendo imágenes a AWS S3...${NC}"
UPLOADED=0
UPLOAD_FAILED=0

for file in "$TEMP_DIR"/*.jpg; do
    if [ ! -f "$file" ]; then
        continue
    fi
    
    FILENAME=$(basename "$file")
    S3_KEY="$S3_PREFIX/$FILENAME"
    
    # Verificar si ya existe en S3
    if aws s3api head-object --bucket "$BUCKET_NAME" --key "$S3_KEY" --region "$REGION" &>/dev/null; then
        echo -e "${YELLOW}⚠️  Ya existe en S3: $FILENAME (omitiendo)${NC}"
        continue
    fi
    
    echo -e "${BLUE}⬆️  Subiendo: $FILENAME...${NC}"
    
    if aws s3 cp "$file" "s3://$BUCKET_NAME/$S3_KEY" \
        --region "$REGION" \
        --content-type "image/jpeg" \
        --metadata "source=unsplash,migrated-date=$(date +%Y-%m-%d)" \
        &>/dev/null; then
        echo -e "${GREEN}✅ Subido: $FILENAME${NC}"
        ((UPLOADED++))
    else
        echo -e "${RED}❌ Error al subir: $FILENAME${NC}"
        ((UPLOAD_FAILED++))
    fi
done

echo ""
echo -e "${GREEN}📊 Resumen de uploads a S3:${NC}"
echo -e "   ✅ Subidos: $UPLOADED"
echo -e "   ❌ Fallidos: $UPLOAD_FAILED"
echo ""

# 6. Generar script de reemplazo para el código frontend
echo -e "${YELLOW}📝 Generando script de actualización del código...${NC}"

REPLACE_SCRIPT="$TEMP_DIR/update-frontend-urls.sh"
cat > "$REPLACE_SCRIPT" << 'EOF'
#!/bin/bash

# Script generado automáticamente para reemplazar URLs de Unsplash por S3
# ========================================================================

FRONTEND_DIR="./frontend/web/src"
MAPPING_FILE="./temp-unsplash-downloads/url-mapping.json"

if [ ! -f "$MAPPING_FILE" ]; then
    echo "❌ ERROR: No se encontró el archivo de mapeo"
    exit 1
fi

echo "🔄 Actualizando URLs en el código frontend..."

# Contar reemplazos
TOTAL_REPLACED=0

EOF

# Generar comandos de reemplazo para cada URL
while IFS= read -r url; do
    PHOTO_ID=$(echo "$url" | grep -o "photo-[a-zA-Z0-9\-]*" | head -1)
    WIDTH=$(echo "$url" | grep -o "w=[0-9]*" | cut -d= -f2)
    HEIGHT=$(echo "$url" | grep -o "h=[0-9]*" | cut -d= -f2)
    
    if [ -z "$WIDTH" ]; then WIDTH="800"; fi
    if [ -z "$HEIGHT" ]; then HEIGHT="600"; fi
    
    FILENAME="${PHOTO_ID}-${WIDTH}x${HEIGHT}.jpg"
    S3_URL="https://$BUCKET_NAME.s3.$REGION.amazonaws.com/$S3_PREFIX/$FILENAME"
    
    # Escapar caracteres especiales para sed
    URL_ESCAPED=$(echo "$url" | sed 's/[&/\]/\\&/g')
    S3_ESCAPED=$(echo "$S3_URL" | sed 's/[&/\]/\\&/g')
    
    # Agregar comando de reemplazo
    cat >> "$REPLACE_SCRIPT" << EOF

# Reemplazar: $url
find "\$FRONTEND_DIR" -type f \( -name "*.ts" -o -name "*.tsx" -o -name "*.js" -o -name "*.jsx" \) -exec sed -i '' "s|$URL_ESCAPED|$S3_ESCAPED|g" {} +
echo "✅ Reemplazado: $FILENAME"
((TOTAL_REPLACED++))
EOF

done < "$TEMP_DIR/unsplash-urls.txt"

cat >> "$REPLACE_SCRIPT" << 'EOF'

echo ""
echo "✅ COMPLETADO: $TOTAL_REPLACED URLs actualizadas"
EOF

chmod +x "$REPLACE_SCRIPT"
echo -e "${GREEN}✅ Script generado: $REPLACE_SCRIPT${NC}"
echo ""

# 7. Copiar archivo de mapeo al proyecto
cp "$MAPPING_FILE" "./frontend/web/src/config/unsplash-to-s3-mapping.json"
echo -e "${GREEN}✅ Archivo de mapeo copiado a: frontend/web/src/config/unsplash-to-s3-mapping.json${NC}"
echo ""

# 8. Resumen final
echo -e "${BLUE}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                  RESUMEN FINAL                           ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${GREEN}✅ URLs encontradas:${NC} $TOTAL_URLS"
echo -e "${GREEN}✅ Imágenes descargadas:${NC} $DOWNLOADED"
echo -e "${GREEN}✅ Imágenes subidas a S3:${NC} $UPLOADED"
echo -e "${GREEN}✅ Bucket S3:${NC} s3://$BUCKET_NAME/$S3_PREFIX/"
echo ""
echo -e "${YELLOW}📌 SIGUIENTE PASO:${NC}"
echo -e "   Ejecuta el script de actualización del código:"
echo -e "   ${BLUE}$REPLACE_SCRIPT${NC}"
echo ""
echo -e "${YELLOW}📌 O ejecuta manualmente:${NC}"
echo -e "   ${BLUE}bash $REPLACE_SCRIPT${NC}"
echo ""
echo -e "${GREEN}✅ ¡Migración completada exitosamente!${NC}"
