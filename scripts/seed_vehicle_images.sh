#!/bin/bash
# ===============================================================================
# OKLA Motors - Script de Seeding de Imágenes
# ===============================================================================
# Este script inserta 5 imágenes por cada vehículo en la base de datos
# usando URLs de Picsum (Lorem Picsum) para imágenes de prueba.
#
# Autor: Gregory Moreno
# Fecha: Enero 2026
# ===============================================================================

set -e

# Configuración
POSTGRES_CONTAINER="postgres_db"
DATABASE="vehiclessaleservice"
POSTGRES_USER="postgres"
IMAGES_PER_VEHICLE=5

echo "==============================================================================="
echo "🖼️  OKLA Motors - Seeding de Imágenes de Vehículos"
echo "==============================================================================="
echo ""

# Verificar que el contenedor de PostgreSQL está corriendo
if ! docker ps | grep -q "$POSTGRES_CONTAINER"; then
    echo "❌ Error: El contenedor $POSTGRES_CONTAINER no está corriendo"
    exit 1
fi

# Contar vehículos existentes
VEHICLE_COUNT=$(docker exec $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $DATABASE -t -c "SELECT COUNT(*) FROM vehicles;" | tr -d ' ')
echo "📊 Vehículos en la base de datos: $VEHICLE_COUNT"

# Contar imágenes existentes
IMAGE_COUNT=$(docker exec $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $DATABASE -t -c "SELECT COUNT(*) FROM vehicle_images;" | tr -d ' ')
echo "📸 Imágenes existentes: $IMAGE_COUNT"

if [ "$IMAGE_COUNT" != "0" ]; then
    echo ""
    echo "⚠️  Ya existen imágenes en la base de datos."
    read -p "¿Desea eliminarlas y crear nuevas? (s/n): " CONFIRM
    if [ "$CONFIRM" != "s" ]; then
        echo "Operación cancelada."
        exit 0
    fi
    echo "🗑️  Eliminando imágenes existentes..."
    docker exec $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $DATABASE -c "DELETE FROM vehicle_images;"
fi

TOTAL_IMAGES=$((VEHICLE_COUNT * IMAGES_PER_VEHICLE))
echo ""
echo "📋 Plan de seeding:"
echo "   • Vehículos: $VEHICLE_COUNT"
echo "   • Imágenes por vehículo: $IMAGES_PER_VEHICLE"
echo "   • Total imágenes a crear: $TOTAL_IMAGES"
echo ""
echo "🚀 Iniciando seeding..."

# Crear script SQL para insertar imágenes
SQL_SCRIPT=$(cat <<'EOSQL'
DO $$
DECLARE
    v_record RECORD;
    v_dealer_id UUID;
    v_image_id UUID;
    v_count INT := 0;
    v_url TEXT;
    v_thumb_url TEXT;
    v_caption TEXT;
    v_image_type INT;
    v_captions TEXT[] := ARRAY['Vista exterior principal', 'Vista lateral', 'Interior del vehículo', 'Detalle del motor', 'Vista trasera'];
    v_image_types INT[] := ARRAY[0, 0, 1, 3, 0]; -- 0=Exterior, 1=Interior, 3=Engine
BEGIN
    FOR v_record IN SELECT "Id", "DealerId" FROM vehicles LOOP
        FOR i IN 1..5 LOOP
            v_image_id := gen_random_uuid();
            v_url := 'https://picsum.photos/seed/' || v_record."Id"::TEXT || '-' || i::TEXT || '/1280/720';
            v_thumb_url := 'https://picsum.photos/seed/' || v_record."Id"::TEXT || '-' || i::TEXT || '/200/150';
            v_caption := v_captions[i];
            v_image_type := v_image_types[i];
            v_dealer_id := COALESCE(v_record."DealerId", '00000000-0000-0000-0000-000000000000'::UUID);
            
            INSERT INTO vehicle_images (
                "Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", 
                "Caption", "ImageType", "SortOrder", "IsPrimary", 
                "FileSize", "MimeType", "Width", "Height", "CreatedAt"
            ) VALUES (
                v_image_id, 
                v_dealer_id,
                v_record."Id", 
                v_url, 
                v_thumb_url,
                v_caption, 
                v_image_type, 
                i - 1, 
                (i = 1),
                500000 + (i * 100000),
                'image/jpeg',
                1280,
                720,
                NOW()
            );
            v_count := v_count + 1;
        END LOOP;
        
        -- Mostrar progreso cada 50 vehículos
        IF v_count % 250 = 0 THEN
            RAISE NOTICE 'Procesadas % imágenes...', v_count;
        END IF;
    END LOOP;
    
    RAISE NOTICE 'Total imágenes insertadas: %', v_count;
END $$;
EOSQL
)

# Ejecutar el script SQL
echo "$SQL_SCRIPT" | docker exec -i $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $DATABASE

# Verificar resultado
FINAL_COUNT=$(docker exec $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $DATABASE -t -c "SELECT COUNT(*) FROM vehicle_images;" | tr -d ' ')

echo ""
echo "==============================================================================="
echo "📊 RESUMEN DEL SEEDING"
echo "==============================================================================="
echo "   • Imágenes creadas: $FINAL_COUNT"
echo "   • Vehículos con imágenes: $VEHICLE_COUNT"
echo ""

# Verificar algunas imágenes de ejemplo
echo "📸 Ejemplo de imágenes insertadas:"
docker exec $POSTGRES_CONTAINER psql -U $POSTGRES_USER -d $DATABASE -c "
SELECT 
    v.\"Title\" as vehicle,
    COUNT(vi.\"Id\") as images,
    MIN(vi.\"Url\") as sample_url
FROM vehicles v
LEFT JOIN vehicle_images vi ON v.\"Id\" = vi.\"VehicleId\"
GROUP BY v.\"Id\", v.\"Title\"
ORDER BY RANDOM()
LIMIT 3;
"

echo ""
echo "✅ Seeding de imágenes completado!"
