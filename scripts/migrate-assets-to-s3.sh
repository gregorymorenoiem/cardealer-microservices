#!/bin/bash
# ==============================================================================
# Script: migrate-assets-to-s3.sh
# Descripción: Migra imágenes del frontend a AWS S3 para carga rápida
# Uso: ./scripts/migrate-assets-to-s3.sh
# ==============================================================================

set -e  # Exit on error

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuración
BUCKET_NAME="okla-images-2026"
REGION="us-east-2"
FRONTEND_PUBLIC_DIR="frontend/web/public"
UPLOAD_COUNT=0
SKIP_COUNT=0
ERROR_COUNT=0

echo -e "${BLUE}============================================================${NC}"
echo -e "${BLUE}  📦 Migración de Assets del Frontend a AWS S3${NC}"
echo -e "${BLUE}============================================================${NC}"
echo ""

# Verificar AWS CLI
if ! command -v aws &> /dev/null; then
    echo -e "${RED}❌ AWS CLI no está instalado${NC}"
    echo -e "${YELLOW}   Instalar con: brew install awscli${NC}"
    exit 1
fi

# Verificar credenciales AWS
if ! aws sts get-caller-identity > /dev/null 2>&1; then
    echo -e "${RED}❌ Credenciales de AWS no configuradas${NC}"
    echo -e "${YELLOW}   Configurar con: aws configure${NC}"
    exit 1
fi

echo -e "${GREEN}✅ AWS CLI configurado correctamente${NC}"
echo -e "   Bucket: ${BUCKET_NAME}"
echo -e "   Región: ${REGION}"
echo ""

# Verificar que el bucket existe
if ! aws s3 ls "s3://${BUCKET_NAME}" --region "${REGION}" > /dev/null 2>&1; then
    echo -e "${RED}❌ No se puede acceder al bucket ${BUCKET_NAME}${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Bucket accesible${NC}"
echo ""

# Función para subir archivo a S3
upload_file() {
    local file_path="$1"
    local relative_path="${file_path#frontend/web/public/}"
    local s3_key="frontend/assets/${relative_path}"
    
    # Determinar Content-Type
    local content_type
    case "${file_path##*.}" in
        jpg|jpeg)
            content_type="image/jpeg"
            ;;
        png)
            content_type="image/png"
            ;;
        gif)
            content_type="image/gif"
            ;;
        svg)
            content_type="image/svg+xml"
            ;;
        webp)
            content_type="image/webp"
            ;;
        mp4)
            content_type="video/mp4"
            ;;
        pdf)
            content_type="application/pdf"
            ;;
        *)
            content_type="application/octet-stream"
            ;;
    esac
    
    # Verificar si el archivo ya existe en S3
    if aws s3api head-object --bucket "${BUCKET_NAME}" --key "${s3_key}" --region "${REGION}" > /dev/null 2>&1; then
        echo -e "${YELLOW}⚠️  Ya existe: ${s3_key}${NC}"
        ((SKIP_COUNT++))
        return 0
    fi
    
    # Subir archivo a S3 (sin ACL, el bucket debe estar configurado como público)
    if aws s3 cp "${file_path}" "s3://${BUCKET_NAME}/${s3_key}" \
        --region "${REGION}" \
        --content-type "${content_type}" \
        --metadata "source=frontend-migration,migrated-date=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
        > /dev/null 2>&1; then
        echo -e "${GREEN}✅ Subido: ${s3_key}${NC}"
        ((UPLOAD_COUNT++))
    else
        echo -e "${RED}❌ Error subiendo: ${file_path}${NC}"
        ((ERROR_COUNT++))
    fi
}

# Buscar y subir imágenes
echo -e "${BLUE}[1/4] Buscando imágenes en ${FRONTEND_PUBLIC_DIR}...${NC}"
echo ""

if [ ! -d "${FRONTEND_PUBLIC_DIR}" ]; then
    echo -e "${RED}❌ Directorio no encontrado: ${FRONTEND_PUBLIC_DIR}${NC}"
    exit 1
fi

# Contar archivos
TOTAL_FILES=$(find "${FRONTEND_PUBLIC_DIR}" -type f \( -iname "*.jpg" -o -iname "*.jpeg" -o -iname "*.png" -o -iname "*.gif" -o -iname "*.svg" -o -iname "*.webp" -o -iname "*.mp4" \) | wc -l | xargs)

echo -e "${BLUE}Encontrados ${TOTAL_FILES} archivos multimedia${NC}"
echo ""

echo -e "${BLUE}[2/4] Subiendo archivos a S3...${NC}"
echo ""

# Subir cada archivo
while IFS= read -r file; do
    upload_file "$file"
done < <(find "${FRONTEND_PUBLIC_DIR}" -type f \( -iname "*.jpg" -o -iname "*.jpeg" -o -iname "*.png" -o -iname "*.gif" -o -iname "*.svg" -o -iname "*.webp" -o -iname "*.mp4" \))

echo ""
echo -e "${BLUE}[3/4] Generando archivo de mapeo de URLs...${NC}"
echo ""

# Crear archivo JSON con URLs de S3
MAPPING_FILE="frontend/web/src/config/s3-assets-map.json"
mkdir -p "$(dirname "${MAPPING_FILE}")"

cat > "${MAPPING_FILE}" << EOF
{
  "baseUrl": "https://${BUCKET_NAME}.s3.${REGION}.amazonaws.com/frontend/assets",
  "cdnUrl": "",
  "assets": {
EOF

# Agregar cada archivo al JSON
FIRST=true
while IFS= read -r file; do
    relative_path="${file#frontend/web/public/}"
    s3_key="frontend/assets/${relative_path}"
    s3_url="https://${BUCKET_NAME}.s3.${REGION}.amazonaws.com/${s3_key}"
    
    if [ "$FIRST" = true ]; then
        FIRST=false
    else
        echo "," >> "${MAPPING_FILE}"
    fi
    
    echo -n "    \"${relative_path}\": \"${s3_url}\"" >> "${MAPPING_FILE}"
done < <(find "${FRONTEND_PUBLIC_DIR}" -type f \( -iname "*.jpg" -o -iname "*.jpeg" -o -iname "*.png" -o -iname "*.gif" -o -iname "*.svg" -o -iname "*.webp" -o -iname "*.mp4" \))

cat >> "${MAPPING_FILE}" << EOF

  }
}
EOF

echo -e "${GREEN}✅ Mapeo generado: ${MAPPING_FILE}${NC}"
echo ""

echo -e "${BLUE}[4/4] Crear helper de assets...${NC}"
echo ""

# Crear helper TypeScript
HELPER_FILE="frontend/web/src/utils/assetLoader.ts"
mkdir -p "$(dirname "${HELPER_FILE}")"

cat > "${HELPER_FILE}" << 'EOF'
// ==============================================================================
// Asset Loader - Carga de imágenes desde AWS S3
// Generado automáticamente por: scripts/migrate-assets-to-s3.sh
// ==============================================================================

import assetsMap from '../config/s3-assets-map.json';

/**
 * Obtiene la URL de un asset desde S3
 * @param path - Ruta relativa del asset (ej: 'images/vehicles/car1.jpg')
 * @returns URL completa del asset en S3
 */
export const getAssetUrl = (path: string): string => {
  // Normalizar path (remover / inicial)
  const normalizedPath = path.startsWith('/') ? path.slice(1) : path;
  
  // Buscar en el mapeo
  const url = (assetsMap.assets as Record<string, string>)[normalizedPath];
  
  if (url) {
    return url;
  }
  
  // Fallback: construir URL dinámicamente
  const baseUrl = assetsMap.cdnUrl || assetsMap.baseUrl;
  return `${baseUrl}/${normalizedPath}`;
};

/**
 * Obtiene múltiples URLs de assets
 * @param paths - Array de rutas relativas
 * @returns Array de URLs completas
 */
export const getAssetUrls = (paths: string[]): string[] => {
  return paths.map(path => getAssetUrl(path));
};

/**
 * Precarga una imagen
 * @param url - URL de la imagen
 * @returns Promise que se resuelve cuando la imagen carga
 */
export const preloadImage = (url: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve();
    img.onerror = reject;
    img.src = url;
  });
};

/**
 * Precarga múltiples imágenes
 * @param urls - Array de URLs
 * @returns Promise que se resuelve cuando todas cargan
 */
export const preloadImages = async (urls: string[]): Promise<void> => {
  await Promise.all(urls.map(url => preloadImage(url)));
};

export default {
  getAssetUrl,
  getAssetUrls,
  preloadImage,
  preloadImages
};
EOF

echo -e "${GREEN}✅ Helper creado: ${HELPER_FILE}${NC}"
echo ""

# Resumen
echo -e "${BLUE}============================================================${NC}"
echo -e "${BLUE}  📊 Resumen de Migración${NC}"
echo -e "${BLUE}============================================================${NC}"
echo -e "${GREEN}Archivos encontrados:${NC} ${TOTAL_FILES}"
echo -e "${GREEN}Subidos exitosamente:${NC} ${UPLOAD_COUNT}"
echo -e "${YELLOW}Omitidos (ya existían):${NC} ${SKIP_COUNT}"
echo -e "${RED}Errores:${NC} ${ERROR_COUNT}"
echo ""

if [ $ERROR_COUNT -eq 0 ]; then
    echo -e "${GREEN}✅ Migración completada exitosamente!${NC}"
    echo ""
    echo -e "${BLUE}Próximos pasos:${NC}"
    echo -e "  1. Importar helper en componentes:"
    echo -e "     ${YELLOW}import { getAssetUrl } from '@/utils/assetLoader';${NC}"
    echo -e "  2. Usar en lugar de rutas locales:"
    echo -e "     ${YELLOW}<img src={getAssetUrl('images/car.jpg')} />${NC}"
    echo -e "  3. Configurar CDN (CloudFlare/CloudFront) para ${BUCKET_NAME}"
    echo ""
else
    echo -e "${RED}⚠️  Migración completada con errores${NC}"
    exit 1
fi
