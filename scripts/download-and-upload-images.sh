#!/bin/bash
#
# Script para descargar imágenes de Unsplash y subirlas a S3
# Bucket: okla-images-2026
# Estructura: frontend/assets/{category}/{type}/{photoId}.jpg
#

set -e

S3_BUCKET="okla-images-2026"
TEMP_DIR="/tmp/okla-images"

# Crear directorios temporales
mkdir -p "$TEMP_DIR/vehicles/sale"
mkdir -p "$TEMP_DIR/vehicles/rent"
mkdir -p "$TEMP_DIR/properties/sale"
mkdir -p "$TEMP_DIR/lodging"

echo "=== Descargando imágenes de Unsplash ==="

# =============================================
# VEHÍCULOS EN VENTA (vehicles/sale)
# =============================================
echo ""
echo "📁 Categoría: vehicles/sale"
VEHICLES_SALE=(
  "photo-1618843479313-40f8afb4b4d8"  # Mercedes-Benz
  "photo-1555215695-3004980ad54e"     # BMW Serie 7
  "photo-1503376780353-7e6692767b70"  # Porsche 911
  "photo-1606664515524-ed2f786a0bd6"  # Audi RS7
  "photo-1617788138017-80ad40651399"  # Tesla Model S
  "photo-1606016159991-dfe4f2746ad5"  # Range Rover Sport
  "photo-1492144534655-ae79c964c9d7"  # Vertical: Vehículos
)

for photo in "${VEHICLES_SALE[@]}"; do
  echo "  ⬇️  Descargando $photo..."
  curl -sL "https://images.unsplash.com/${photo}?w=800&h=600&fit=crop" -o "$TEMP_DIR/vehicles/sale/${photo}.jpg"
done

# =============================================
# VEHÍCULOS EN RENTA (vehicles/rent)
# =============================================
echo ""
echo "📁 Categoría: vehicles/rent"
VEHICLES_RENT=(
  "photo-1549317661-bd32c8ce0db2"     # BMW X5
  "photo-1563720223185-11003d516935"  # Mercedes GLE
  "photo-1619767886558-efdc259cde1a"  # Porsche Cayenne
  "photo-1533473359331-0135ef1b58bf"  # Cadillac Escalade
  "photo-1560958089-b8a1929cea89"     # Tesla Model X
  "photo-1551830820-330a71b99659"     # Range Rover Velar
  "photo-1449965408869-eaa3f722e40d"  # Vertical: Renta
)

for photo in "${VEHICLES_RENT[@]}"; do
  echo "  ⬇️  Descargando $photo..."
  curl -sL "https://images.unsplash.com/${photo}?w=800&h=600&fit=crop" -o "$TEMP_DIR/vehicles/rent/${photo}.jpg"
done

# =============================================
# PROPIEDADES EN VENTA (properties/sale)
# =============================================
echo ""
echo "📁 Categoría: properties/sale"
PROPERTIES_SALE=(
  "photo-1600607687939-ce8a6c25118c"  # Penthouse lujo
  "photo-1600596542815-ffad4c1539a9"  # Villa piscina
  "photo-1502672260266-1c1ef2d93688"  # Apartamento moderno
  "photo-1600585154340-be6161a56a0c"  # Casa colonial
  "photo-1600607688969-a5bfcd646154"  # Loft industrial
  "photo-1613490493576-7fde63acd811"  # Mansión
)

for photo in "${PROPERTIES_SALE[@]}"; do
  echo "  ⬇️  Descargando $photo..."
  curl -sL "https://images.unsplash.com/${photo}?w=800&h=600&fit=crop" -o "$TEMP_DIR/properties/sale/${photo}.jpg"
done

# =============================================
# HOSPEDAJE (lodging)
# =============================================
echo ""
echo "📁 Categoría: lodging"
LODGING=(
  "photo-1582719478250-c89cae4dc85b"  # Suite mar
  "photo-1560448204-e02f11c3d0e2"     # Apartamento ejecutivo
  "photo-1602002418082-a4443e081dd1"  # Villa alberca
  "photo-1587061949409-02df41d5e562"  # Cabaña montaña
  "photo-1578683010236-d716f9a3f461"  # Penthouse terraza
  "photo-1499793983690-e29da59ef1c2"  # Casa playa
  "photo-1566073771259-6a8506099945"  # Vertical: Hospedaje
)

for photo in "${LODGING[@]}"; do
  echo "  ⬇️  Descargando $photo..."
  curl -sL "https://images.unsplash.com/${photo}?w=800&h=600&fit=crop" -o "$TEMP_DIR/lodging/${photo}.jpg"
done

# =============================================
# SUBIR A S3
# =============================================
echo ""
echo "=== Subiendo a S3: $S3_BUCKET ==="

echo ""
echo "📤 Subiendo vehicles/sale..."
aws s3 sync "$TEMP_DIR/vehicles/sale/" "s3://$S3_BUCKET/frontend/assets/vehicles/sale/" --delete

echo ""
echo "📤 Subiendo vehicles/rent..."
aws s3 sync "$TEMP_DIR/vehicles/rent/" "s3://$S3_BUCKET/frontend/assets/vehicles/rent/" --delete

echo ""
echo "📤 Subiendo properties/sale..."
aws s3 sync "$TEMP_DIR/properties/sale/" "s3://$S3_BUCKET/frontend/assets/properties/sale/" --delete

echo ""
echo "📤 Subiendo lodging..."
aws s3 sync "$TEMP_DIR/lodging/" "s3://$S3_BUCKET/frontend/assets/lodging/" --delete

# =============================================
# VERIFICAR
# =============================================
echo ""
echo "=== Verificando subidas ==="

echo ""
echo "📂 vehicles/sale:"
aws s3 ls "s3://$S3_BUCKET/frontend/assets/vehicles/sale/" | wc -l | xargs echo "   Total archivos:"

echo ""
echo "📂 vehicles/rent:"
aws s3 ls "s3://$S3_BUCKET/frontend/assets/vehicles/rent/" | wc -l | xargs echo "   Total archivos:"

echo ""
echo "📂 properties/sale:"
aws s3 ls "s3://$S3_BUCKET/frontend/assets/properties/sale/" | wc -l | xargs echo "   Total archivos:"

echo ""
echo "📂 lodging:"
aws s3 ls "s3://$S3_BUCKET/frontend/assets/lodging/" | wc -l | xargs echo "   Total archivos:"

# =============================================
# LIMPIAR
# =============================================
echo ""
echo "🧹 Limpiando archivos temporales..."
rm -rf "$TEMP_DIR"

echo ""
echo "✅ ¡Completado! Imágenes subidas a s3://$S3_BUCKET/frontend/assets/"
