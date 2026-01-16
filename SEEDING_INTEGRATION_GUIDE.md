# � SEEDING V2.0 - Arquitectura con MediaService

**Estrategia completa de seeding respetando la arquitectura de microservicios**

> ⚠️ **IMPORTANTE:** Las imágenes se manejan con **MediaService**, no en VehiclesSaleService

---

## 🏗️ Flujo de Seeding en 5 Fases

```
FASE 1: SQL → Catálogo (20 marcas, 35 modelos, 100 trims)
   ↓
FASE 2: API REST → Vehículos (150 vehículos sin imágenes)
   ↓
FASE 3: API REST → Imágenes (1,500 imágenes via MediaService)
   ↓
FASE 4: API REST → Asociar Imágenes a Vehículos
   ↓
FASE 5: SQL → Homepage Sections (8 secciones, 90 asignaciones)
```

---

## 📍 UBICACIÓN: VehiclesSaleService.Api/Program.cs

## ⚡ Ejecución Rápida (Recomendado)

### 1. Catálogo (3 minutos)

```bash
docker exec -i postgres_db psql -U postgres -d vehiclessaleservice < scripts/seed_catalog.sql

# Verificar
docker exec postgres_db psql -U postgres -d vehiclessaleservice \
  -c "SELECT COUNT(*) FROM vehicle_makes;"
# Resultado: 20
```

### 2. Vehículos (30 minutos)

```bash
# Crear script
cat > seeding-vehicles.sh << 'SCRIPT'
#!/bin/bash
API="http://localhost:15070/api"

# Distribution: Toyota(45), Honda(22), Nissan(22), Ford(16), BMW(10)...
for make in "Toyota:45" "Honda:22" "Nissan:22" "Ford:16"; do
    IFS=':' read -r make_name qty <<< "$make"
    for ((i=1; i<=qty; i++)); do
        year=$((2018 + RANDOM % 7))
        price=$((18000 + RANDOM % 35000))
        mileage=$((10000 + RANDOM % 120000))
        vin=$(uuidgen | tr '[:upper:]' '[:lower:]' | sed 's/-//g' | cut -c1-17)

        curl -s -X POST "$API/vehicles" \
            -H "Content-Type: application/json" \
            -d "{
                \"title\": \"$year $make_name\",
                \"price\": $price,
                \"year\": $year,
                \"vin\": \"$vin\",
                \"mileage\": $mileage,
                \"status\": \"Active\",
                \"make\": \"$make_name\",
                \"model\": \"Model\",
                \"images\": []
            }" > /dev/null

        echo -ne "\r  $i/$qty"
        sleep 0.2
    done
done
echo ""
SCRIPT

chmod +x seeding-vehicles.sh
./seeding-vehicles.sh
```

### 3. Imágenes via MediaService (45 minutos)

```bash
# MediaService endpoint
MEDIA_API="http://localhost:16070/api/media"

# Obtener lista de vehículos
vehicles=$(curl -s "http://localhost:15070/api/vehicles?pageSize=1000" \
    | grep -o '"id":"[^"]*"' | cut -d'"' -f4)

count=0
for vehicle_id in $vehicles; do
    for ((i=1; i<=10; i++)); do
        count=$((count+1))

        # Usar picsum.photos para imágenes aleatorias
        image_url="https://picsum.photos/seed/${count}/800/600"

        # Upload a MediaService
        curl -s -X POST "$MEDIA_API/upload" \
            -H "Content-Type: multipart/form-data" \
            -F "file=@-" \
            -F "vehicleId=$vehicle_id" \
            -F "sortOrder=$((i-1))" \
            -F "isPrimary=$([[ $i -eq 1 ]] && echo 'true' || echo 'false')" \
            < <(curl -s "$image_url") > /dev/null

        echo -ne "\r  $count/1500 imágenes"
        sleep 0.1
    done
done
```

### 4. Homepage Sections (2 minutos)

```bash
docker exec -i postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
-- Crear secciones
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

-- Asignar vehículos a secciones
INSERT INTO vehicle_homepage_sections (
    "VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned"
)
SELECT
    v.id,
    h.id,
    ROW_NUMBER() OVER (PARTITION BY h.id ORDER BY v.created_at DESC),
    false
FROM vehicles v
CROSS JOIN homepage_section_configs h
WHERE v.status = 'Active'
  AND h.is_active = true
  AND ROW_NUMBER() OVER (PARTITION BY h.id ORDER BY v.created_at DESC) <= h.max_items
ON CONFLICT DO NOTHING;
SQL
```

---

## 📊 Verificación Completa

```bash
docker exec postgres_db psql -U postgres -d vehiclessaleservice << 'SQL'
\echo '✅ SEEDING DASHBOARD'
SELECT '🏭 Marcas' as seccion, COUNT(*)::TEXT FROM vehicle_makes
UNION ALL
SELECT '📦 Modelos', COUNT(*)::TEXT FROM vehicle_models
UNION ALL
SELECT '🚗 Vehículos Activos', COUNT(*)::TEXT FROM vehicles WHERE status = 'Active'
UNION ALL
SELECT '📸 Imágenes', COUNT(*)::TEXT FROM vehicle_images
UNION ALL
SELECT '🏠 Secciones', COUNT(*)::TEXT FROM homepage_section_configs WHERE is_active = true
UNION ALL
SELECT '🔗 Asignaciones', COUNT(*)::TEXT FROM vehicle_homepage_sections;
SQL
```

**Resultado esperado:**

```
✅ SEEDING DASHBOARD
seccion            | count
───────────────────┼──────
🏭 Marcas          | 20
📦 Modelos         | 35
🚗 Vehículos Activos | 150
📸 Imágenes        | 1500
🏠 Secciones       | 8
🔗 Asignaciones    | 90
```

---

## 🎯 Integración Opcional: Auto-Seeding en Program.cs

Si deseas que el seeding se ejecute automáticamente al iniciar:

```csharp
var app = builder.Build();

// === SEEDING (SOLO EN DESARROLLO) ===
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🌱 Ejecutando seeding...");

            // 1. Catálogo (SQL)
            // 2. Vehículos (API)
            // 3. Imágenes (MediaService)
            // 4. Homepage (SQL)

            logger.LogInformation("✅ Seeding completado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error en seeding");
        }
    }
}
// === FIN SEEDING ===

app.UseSwagger();
app.UseSwaggerUI();
app.Run();
```

---

## 🔧 Consideraciones de MediaService

### Paso 3: Alternativa - CLI Commands

Si prefieres ejecutar seeding manualmente:

```csharp
// En Program.cs
var app = builder.Build();

// Agregar comandos personalizados
if (args.Contains("seed:all"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.SeedAllAsync();
        return;
    }
}

if (args.Contains("seed:clean"))
{
    using (var scope = app.Services.CreateScope())
    {
        var seeding = scope.ServiceProvider.GetRequiredService<DatabaseSeedingService>();
        await seeding.CleanAllAsync();
        return;
    }
}

app.UseSwagger();
---

## 🔧 Consideraciones de MediaService

### MediaService Endpoints

- **Upload:** `POST /api/media/upload` (multipart/form-data)
- **List:** `GET /api/media?vehicleId={vehicleId}`
- **Delete:** `DELETE /api/media/{mediaId}`
- **Health:** `GET /health`

### Puertos

- **VehiclesSaleService:** 15070 (desde host)
- **MediaService:** 16070 (desde host)
- **Gateway:** 18443 (enruta a todos)

---

## ✅ Checklist Final

- [ ] FASE 1: `seed_catalog.sql` ejecutado → 20 marcas
- [ ] FASE 2: 150 vehículos creados vía API
- [ ] FASE 3: 1,500 imágenes subidas a MediaService
- [ ] FASE 4: Imágenes asociadas a vehículos
- [ ] FASE 5: Homepage sections configuradas
- [ ] ✅ Verificación: Dashboard muestra todos los números
- [ ] ✅ Frontend: Todas las vistas muestran datos reales

---

## 🎯 Estado Actual (Enero 15, 2026)

**Completado:**
✅ Scripts SQL de catálogo creados
✅ Arquitectura de 5 fases diseñada
✅ Documentación de MediaService integrada

**Pendiente:**
⏳ Ejecutar scripts en orden
⏳ Validar datos en PostgreSQL
⏳ Probar imágenes en MediaService
⏳ Verificar frontend con datos reales

_Guía completa respetando arquitectura de microservicios_
_MediaService = responsable de todas las imágenes_
_Última actualización: Enero 15, 2026_
```
