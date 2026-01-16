# 🚀 SEEDING EXECUTION GUIDE - MediaService Integration

**Fecha:** Enero 15, 2026  
**Objetivo:** Ejecutar seeding respetando arquitectura de microservicios  
**Criticidad:** ⭐⭐⭐ MediaService maneja TODAS las imágenes

---

## 📋 Resumen Ejecutivo

```
┌──────────────────────────────────────────────────────────────────┐
│                   SEEDING EN 5 FASES                             │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  FASE 1: SQL (Catálogo)           ⏱️ 3 minutos                  │
│  ├─ 20 vehicle_makes                                            │
│  ├─ 35+ vehicle_models                                          │
│  └─ 100+ vehicle_trims                                          │
│                                                                  │
│  FASE 2: API REST (Vehículos)     ⏱️ 30 minutos                 │
│  ├─ 150 vehículos (sin imágenes)                               │
│  └─ Cada uno con: make, model, year, price, mileage, etc.     │
│                                                                  │
│  FASE 3: API MediaService (Imágenes) ⏱️ 45 minutos             │
│  ├─ 1,500 imágenes (10 por vehículo)                          │
│  ├─ Upload a MediaService (puerto 16070)                      │
│  └─ Usando picsum.photos como fuente                           │
│                                                                  │
│  FASE 4: API REST (Asociar)       ⏱️ 5 minutos                  │
│  ├─ Vincular imágenes a vehículos                             │
│  └─ Marcar imagen principal                                    │
│                                                                  │
│  FASE 5: SQL (Homepage)            ⏱️ 2 minutos                 │
│  ├─ 8 homepage_section_configs                                │
│  ├─ 90 vehicle_homepage_sections (assignments)                │
│  └─ Distribución: 10-20 vehículos por sección                 │
│                                                                  │
│  TOTAL: ~85 MINUTOS DE EJECUCIÓN                               │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## ⚡ OPCIÓN RÁPIDA (Sin imágenes reales)

Si necesitas datos rápidamente SIN subir 1,500 imágenes a MediaService:

```bash
# 1. FASE 1: Catálogo (3 min)
docker exec -i postgres_db psql -U postgres -d vehiclessaleservice < scripts/seed_catalog.sql

# 2. FASE 2: Vehículos (30 min)
# [crear script y ejecutar]

# 3. FASE 5: Homepage (2 min)
docker exec -i postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
INSERT INTO homepage_section_configs ... (ver script)
SQL

# Total: 35 minutos
# Resultado: 150 vehículos sin imágenes (problema visual)
```

---

## ⭐ OPCIÓN COMPLETA (Recomendada)

Incluir el seeding de imágenes en MediaService para UI funcional:

```bash
# TOTAL: ~85 minutos
# Resultado: 150 vehículos + 1,500 imágenes en MediaService
```

---

## 🔍 Pre-requisitos

Verificar que todos los servicios estén corriendo:

```bash
# 1. Ver estado de Docker
docker ps | grep -E "vehiclessaleservice|mediaservice|postgres"

# Esperado:
# postgres_db  ✅ Running on :5433
# vehiclessaleservice ✅ Running on :15070
# mediaservice ✅ Running on :16070

# 2. Verificar conectividad
curl http://localhost:15070/health   # VehiclesSaleService
curl http://localhost:16070/health   # MediaService
curl http://localhost:5433           # PostgreSQL

# 3. Verificar base de datos vacía
docker exec postgres_db psql -U postgres -d vehiclessaleservice \
  -c "SELECT COUNT(*) FROM vehicles;"
# Esperado: 0
```

---

## FASE 1: Catálogo (SQL)

### 🎯 Objetivo

Insertar 20 marcas, 35+ modelos y 100+ trims en la base de datos

### ⏱️ Tiempo: 3 minutos

### 📝 Comando

```bash
docker exec -i postgres_db psql -U postgres -d vehiclessaleservice < scripts/seed_catalog.sql
```

### ✅ Verificación

```bash
docker exec postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
\echo '📊 FASE 1: Catálogo'
SELECT 'Marcas: ' || COUNT(*)::TEXT FROM vehicle_makes;
SELECT 'Modelos: ' || COUNT(*)::TEXT FROM vehicle_models;
SELECT 'Trims: ' || COUNT(*)::TEXT FROM vehicle_trims;
SQL
```

**Resultado esperado:**

```
Marcas: 20
Modelos: 35
Trims: 100
```

---

## FASE 2: Vehículos (API REST)

### 🎯 Objetivo

Crear 150 vehículos distribuidostringentes por marca

### ⏱️ Tiempo: 30 minutos

### 📝 Script

```bash
cat > seeding-vehicles.sh << 'SCRIPT'
#!/bin/bash
set -e

API_URL="http://localhost:15070/api/vehicles"
TOTAL=150
COUNT=0

echo "🚗 FASE 2: Creando $TOTAL vehículos..."

# Distribution: Toyota(45), Honda(22), Nissan(22), Ford(16), BMW(10), etc.
declare -A MAKES=(
    ["Toyota"]=45
    ["Honda"]=22
    ["Nissan"]=22
    ["Ford"]=16
    ["BMW"]=10
    ["Mercedes-Benz"]=8
    ["Chevrolet"]=7
    ["Audi"]=6
    ["Porsche"]=5
    ["Tesla"]=5
    ["Jeep"]=4
    ["RAM"]=4
    ["GMC"]=3
)

for make in "${!MAKES[@]}"; do
    qty=${MAKES[$make]}

    for ((i=1; i<=qty; i++)); do
        COUNT=$((COUNT+1))

        # Variables aleatorias
        year=$((2018 + RANDOM % 7))
        price=$((18000 + RANDOM % 50000))
        mileage=$((10000 + RANDOM % 120000))
        vin=$(openssl rand -hex 8 | cut -c1-17)

        # API POST
        curl -s -X POST "$API_URL" \
            -H "Content-Type: application/json" \
            -d "{
                \"title\": \"$year $make\",
                \"description\": \"Excelente vehículo en buenas condiciones\",
                \"price\": $price,
                \"currency\": \"USD\",
                \"make\": \"$make\",
                \"model\": \"Model\",
                \"year\": $year,
                \"vin\": \"$vin\",
                \"mileage\": $mileage,
                \"mileageUnit\": \"Miles\",
                \"status\": \"Active\",
                \"condition\": \"Used\",
                \"doors\": 4,
                \"seats\": 5,
                \"fuelType\": \"Gasoline\",
                \"transmission\": \"Automatic\",
                \"driveType\": \"FWD\",
                \"bodyStyle\": \"Sedan\",
                \"exteriorColor\": \"White\",
                \"interiorColor\": \"Black\",
                \"city\": \"Santo Domingo\",
                \"state\": \"Distrito Nacional\",
                \"country\": \"Dominican Republic\",
                \"images\": []
            }" > /dev/null 2>&1

        # Progress
        pct=$((COUNT * 100 / TOTAL))
        echo -ne "\r  [$pct%] $COUNT/$TOTAL vehículos"

        sleep 0.2
    done
done

echo ""
echo "✅ FASE 2 completada: $COUNT vehículos creados"
SCRIPT

chmod +x seeding-vehicles.sh
./seeding-vehicles.sh
```

### ✅ Verificación

```bash
docker exec postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
\echo '📊 FASE 2: Vehículos'
SELECT 'Total: ' || COUNT(*)::TEXT FROM vehicles;
SELECT 'Por marca: ' || make || ' = ' || COUNT(*)::TEXT FROM vehicles GROUP BY make ORDER BY COUNT(*) DESC LIMIT 5;
SQL
```

**Resultado esperado:**

```
Total: 150
Por marca:
  Toyota = 45
  Honda = 22
  Nissan = 22
  Ford = 16
  BMW = 10
```

---

## FASE 3: Imágenes via MediaService

### 🎯 Objetivo

Subir 1,500 imágenes a MediaService (10 por vehículo)

### ⏱️ Tiempo: 45 minutos

### 📝 Script

```bash
cat > seeding-images.sh << 'SCRIPT'
#!/bin/bash
set -e

MEDIA_API="http://localhost:16070/api/media/upload"
VEHICLES_API="http://localhost:15070/api/vehicles"
TOTAL_IMAGES=1500
IMAGES_PER_VEHICLE=10
COUNT=0

echo "📸 FASE 3: Subiendo $TOTAL_IMAGES imágenes..."

# Obtener lista de vehículos (máx 1000)
vehicles=$(curl -s "$VEHICLES_API?pageSize=1000" | \
    grep -o '"id":"[^"]*"' | cut -d'"' -f4 | head -150)

for vehicle_id in $vehicles; do
    if [ -z "$vehicle_id" ]; then
        continue
    fi

    for ((i=1; i<=IMAGES_PER_VEHICLE; i++)); do
        COUNT=$((COUNT+1))

        # URL de imagen aleatoria
        seed=$((RANDOM * 1000 + i))
        image_url="https://picsum.photos/seed/${seed}/800/600"

        # Descargar y uploadear a MediaService
        curl -s -X POST "$MEDIA_API" \
            -F "file=@-" \
            -F "vehicleId=$vehicle_id" \
            -F "sortOrder=$((i-1))" \
            -F "isPrimary=$([[ $i -eq 1 ]] && echo 'true' || echo 'false')" \
            < <(curl -s "$image_url") \
            > /dev/null 2>&1

        # Progress
        pct=$((COUNT * 100 / TOTAL_IMAGES))
        echo -ne "\r  [$pct%] $COUNT/$TOTAL_IMAGES imágenes"

        sleep 0.1
    done
done

echo ""
echo "✅ FASE 3 completada: $COUNT imágenes subidas a MediaService"
SCRIPT

chmod +x seeding-images.sh
./seeding-images.sh
```

### ✅ Verificación

```bash
# Verificar imágenes en MediaService database
docker exec postgres_db psql -U postgres -d mediaservice << 'SQL'
\echo '📊 FASE 3: Imágenes'
SELECT 'Total: ' || COUNT(*)::TEXT FROM media_files WHERE entity_type = 'Vehicle';
SELECT 'Promedio por vehículo: ' || ROUND(COUNT(*)::NUMERIC / 150, 1)::TEXT FROM media_files;
SQL
```

**Resultado esperado:**

```
Total: 1500
Promedio por vehículo: 10.0
```

---

## FASE 4: Asociar Imágenes (Automático)

### 🎯 Objetivo

MediaService ya asocia las imágenes en FASE 3

### ⏱️ Tiempo: 0 minutos (automático)

**Nota:** Si MediaService no asocia automáticamente, usar:

```bash
# PUT /api/vehicles/{vehicleId}/images
curl -X PUT "http://localhost:15070/api/vehicles/{vehicleId}/images" \
    -H "Content-Type: application/json" \
    -d '{"mediaIds": ["id1", "id2", ...]}'
```

---

## FASE 5: Homepage Sections (SQL)

### 🎯 Objetivo

Configurar 8 secciones del homepage y asignar 150 vehículos

### ⏱️ Tiempo: 2 minutos

### 📝 Script

```bash
docker exec -i postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'

-- 1. Crear secciones del homepage
INSERT INTO homepage_section_configs (
    "Id", "Name", "Slug", "DisplayOrder", "MaxItems", "IsActive", "CreatedAt", "UpdatedAt"
) VALUES
('550e8400-e29b-41d4-a716-446655440001', 'Carousel Principal', 'carousel', 1, 5, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440002', 'Sedanes', 'sedanes', 2, 10, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440003', 'SUVs', 'suvs', 3, 10, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440004', 'Camionetas', 'camionetas', 4, 10, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440005', 'Deportivos', 'deportivos', 5, 10, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440006', 'Destacados', 'destacados', 6, 9, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440007', 'Lujo', 'lujo', 7, 10, true, NOW(), NOW()),
('550e8400-e29b-41d4-a716-446655440008', 'Eficiencia', 'eficiencia', 8, 10, true, NOW(), NOW())
ON CONFLICT ("Name") DO NOTHING;

-- 2. Asignar vehículos a secciones (máx MaxItems por sección)
INSERT INTO vehicle_homepage_sections (
    "VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned"
)
SELECT
    v."Id",
    h."Id",
    ROW_NUMBER() OVER (PARTITION BY h."Id" ORDER BY v."CreatedAt" DESC),
    false
FROM vehicles v
CROSS JOIN homepage_section_configs h
WHERE v."Status" = 0  -- Active
  AND h."IsActive" = true
  AND ROW_NUMBER() OVER (PARTITION BY h."Id" ORDER BY v."CreatedAt" DESC) <= h."MaxItems"
ON CONFLICT DO NOTHING;

SQL
```

### ✅ Verificación

```bash
docker exec postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
\echo '📊 FASE 5: Homepage'
SELECT 'Secciones: ' || COUNT(*)::TEXT FROM homepage_section_configs WHERE "IsActive" = true;
SELECT 'Asignaciones: ' || COUNT(*)::TEXT FROM vehicle_homepage_sections;
SELECT "Name" || ': ' || COUNT(*)::TEXT FROM vehicle_homepage_sections
  JOIN homepage_section_configs ON vehicle_homepage_sections."HomepageSectionConfigId" = homepage_section_configs."Id"
  GROUP BY "Name"
  ORDER BY "DisplayOrder";
SQL
```

**Resultado esperado:**

```
Secciones: 8
Asignaciones: 80-90
Carousel Principal: 5
Sedanes: 10
SUVs: 10
Camionetas: 10
Deportivos: 10
Destacados: 9
Lujo: 10
Eficiencia: 10
```

---

## 📊 Verificación Final Completa

```bash
docker exec postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
\echo '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
\echo '                  ✅ SEEDING COMPLETADO'
\echo '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
\echo ''

\echo '🏭 FASE 1: CATÁLOGO'
SELECT '   Marcas:' || COUNT(*)::TEXT FROM vehicle_makes;
SELECT '   Modelos:' || COUNT(*)::TEXT FROM vehicle_models;
SELECT '   Trims:' || COUNT(*)::TEXT FROM vehicle_trims;

\echo ''
\echo '🚗 FASE 2: VEHÍCULOS'
SELECT '   Total:' || COUNT(*)::TEXT FROM vehicles;
SELECT '   Activos:' || COUNT(*)::TEXT FROM vehicles WHERE "Status" = 0;

\echo ''
\echo '📸 FASE 3: IMÁGENES (MediaService)'
-- Verificar en mediaservice DB
-- SELECT '   Total:' || COUNT(*)::TEXT FROM media_files WHERE entity_type = 'Vehicle';

\echo ''
\echo '🏠 FASE 5: HOMEPAGE'
SELECT '   Secciones:' || COUNT(*)::TEXT FROM homepage_section_configs WHERE "IsActive" = true;
SELECT '   Asignaciones:' || COUNT(*)::TEXT FROM vehicle_homepage_sections;

\echo ''
\echo '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━'
SQL
```

---

## 🎯 Checklist de Ejecución

```
FASE 1: Catálogo (SQL)
  [ ] Ejecutar seed_catalog.sql
  [ ] Verificar: 20 marcas, 35 modelos, 100 trims

FASE 2: Vehículos (API)
  [ ] Crear y ejecutar seeding-vehicles.sh
  [ ] Verificar: 150 vehículos

FASE 3: Imágenes (MediaService)
  [ ] Crear y ejecutar seeding-images.sh
  [ ] Verificar: 1,500 imágenes en mediaservice DB

FASE 5: Homepage (SQL)
  [ ] Ejecutar insert de secciones
  [ ] Verificar: 8 secciones, 80-90 asignaciones

VALIDACIÓN FINAL
  [ ] Dashboard muestra todos los números
  [ ] Frontend muestra datos (homepage, search, etc.)
  [ ] MediaService responde con imágenes
  [ ] Todas las vistas cargan datos reales
```

---

## 🚨 Troubleshooting

### "Cannot find module 'axios'" (No es relevante con curl)

✅ Usando curl, no axios

### "Connection refused: localhost:16070"

❌ MediaService no está corriendo

```bash
docker ps | grep mediaservice
docker logs mediaservice
```

### "Vehicle count = 0 after FASE 2"

❌ API REST falló

```bash
curl -X POST http://localhost:15070/api/vehicles \
  -H "Content-Type: application/json" \
  -d '{"title": "Test", "price": 25000, ...}'
```

### "Images count = 0 after FASE 3"

❌ MediaService no recibió uploads

```bash
docker logs mediaservice | grep -i upload
```

### "Homepage sections = 0 after FASE 5"

❌ SQL falló (verificar syntax)

```bash
docker exec postgres_db psql -U postgres -d vehiclessaleservice \
  -c "SELECT COUNT(*) FROM homepage_section_configs;"
```

---

## 📝 Resumen

| Fase      | Componente      | Tiempo     | Comando                    |
| --------- | --------------- | ---------- | -------------------------- |
| 1         | SQL → Catálogo  | 3 min      | `psql < seed_catalog.sql`  |
| 2         | API → Vehículos | 30 min     | `./seeding-vehicles.sh`    |
| 3         | API → Imágenes  | 45 min     | `./seeding-images.sh`      |
| 5         | SQL → Homepage  | 2 min      | `psql < seed_homepage.sql` |
| **TOTAL** |                 | **80 min** |                            |

---

_Última actualización: Enero 15, 2026_  
_⭐ MediaService maneja TODAS las imágenes_  
_✅ Guía completa para seeding con microservicios_
