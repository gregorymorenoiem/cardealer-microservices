# 🔍 SQL de Validación - Verificar Seeding v2.0

**Propósito:** Validar que todos los datos se generaron correctamente  
**Ejecutar después:** Fase 7 completada  
**Base de Datos:** PostgreSQL (cardealer)

---

## 📊 VALIDACIÓN DE CANTIDADES

### Catálogos

```sql
-- Verificar Makes (debe ser 10)
SELECT COUNT(*) as total_makes,
       STRING_AGG(DISTINCT "Name", ', ') as makes_list
FROM catalog_makes;

-- Resultado esperado: 10 makes
-- Toyota, Honda, Nissan, Ford, BMW, Mercedes-Benz, Tesla, Hyundai, Porsche, Chevrolet

-- Verificar Models (debe ser 60+)
SELECT COUNT(*) as total_models,
       "MakeId",
       (SELECT "Name" FROM catalog_makes WHERE "Id" = m."MakeId") as make_name,
       COUNT(*) as models_per_make
FROM catalog_models m
GROUP BY "MakeId"
ORDER BY models_per_make DESC;

-- Resultado esperado: ~60 models distribuidos entre 10 makes

-- Verificar Years (debe ser 15)
SELECT COUNT(*) as total_years,
       MIN("Value") as min_year,
       MAX("Value") as max_year
FROM catalog_years;

-- Resultado esperado: 15 years (ej: 2010-2024)

-- Verificar BodyStyles (debe ser 7)
SELECT COUNT(*) as total_styles,
       STRING_AGG(DISTINCT "Name", ', ') as styles_list
FROM catalog_body_styles;

-- Resultado esperado: 7 styles

-- Verificar FuelTypes (debe ser 5)
SELECT COUNT(*) as total_fuel_types,
       STRING_AGG(DISTINCT "Name", ', ') as fuel_types_list
FROM catalog_fuel_types;

-- Resultado esperado: 5 tipos (Gasoline, Diesel, Hybrid, Electric, Plug-in Hybrid)

-- Verificar Colors (debe ser 16+)
SELECT COUNT(*) as total_colors,
       STRING_AGG(DISTINCT "Name", ', ') as colors_list
FROM catalog_colors;

-- Resultado esperado: 16+ colores
```

---

### Usuarios

```sql
-- Total de usuarios (debe ser 42)
SELECT COUNT(*) as total_users,
       COUNT(CASE WHEN "Role" = 'Buyer' THEN 1 END) as buyers,
       COUNT(CASE WHEN "Role" = 'Seller' THEN 1 END) as sellers,
       COUNT(CASE WHEN "Role" = 'Dealer' THEN 1 END) as dealers,
       COUNT(CASE WHEN "Role" = 'Admin' THEN 1 END) as admins
FROM users;

-- Resultado esperado:
-- total_users: 42
-- buyers: 10
-- sellers: 10
-- dealers: 30
-- admins: 2

-- Ver distribución por tipo
SELECT "Role",
       COUNT(*) as count,
       ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM users), 1) as percentage
FROM users
GROUP BY "Role"
ORDER BY count DESC;

-- Resultado esperado:
-- Dealer: 30 (71%)
-- Buyer: 10 (24%)
-- Seller: 10 (24%)
-- Admin: 2 (5%)

-- Usuarios sin duplicado de email
SELECT COUNT(*) as total_unique_emails
FROM (SELECT DISTINCT "Email" FROM users) t;

-- Resultado esperado: 42 (no hay duplicados)
```

---

### Dealers

```sql
-- Total de dealers (debe ser 30)
SELECT COUNT(*) as total_dealers,
       "Type",
       COUNT(*) as count_by_type
FROM dealers
GROUP BY "Type"
ORDER BY count_by_type DESC;

-- Resultado esperado: ~30 total distribuido en tipos
-- Independent: ~10
-- Chain: ~8
-- MultipleStore: ~7
-- Franchise: ~5

-- Dealers con locations
SELECT d."Id",
       d."BusinessName",
       COUNT(dl."Id") as location_count
FROM dealers d
LEFT JOIN dealer_locations dl ON d."Id" = dl."DealerId"
GROUP BY d."Id", d."BusinessName"
HAVING COUNT(dl."Id") >= 2
ORDER BY location_count DESC;

-- Resultado esperado: 30 dealers, cada uno con 2-3 locations

-- Total de locations (debe ser 60-90)
SELECT COUNT(*) as total_locations,
       COUNT(DISTINCT "DealerId") as dealers_with_locations
FROM dealer_locations;

-- Resultado esperado: 60-90 locations para 30 dealers

-- Dealers por status
SELECT "Status",
       COUNT(*) as count
FROM dealers
GROUP BY "Status"
ORDER BY count DESC;

-- Resultado esperado:
-- Most should be "Active"
-- Some "Pending"
-- Few "Suspended"
```

---

### Vehículos

```sql
-- Total de vehículos (debe ser 150)
SELECT COUNT(*) as total_vehicles,
       COUNT(CASE WHEN "Status" = 'Active' THEN 1 END) as active_vehicles,
       COUNT(CASE WHEN "IsFeatured" = true THEN 1 END) as featured_vehicles
FROM vehicles;

-- Resultado esperado:
-- total_vehicles: 150
-- active_vehicles: ~140-150
-- featured_vehicles: ~15 (10%)

-- Vehículos por marca (distribución)
SELECT "Make",
       COUNT(*) as count,
       ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM vehicles), 1) as percentage
FROM vehicles
GROUP BY "Make"
ORDER BY count DESC;

-- Resultado esperado:
-- Toyota: 45 (30%)
-- Nissan: 22 (15%)
-- Ford: 22 (15%)
-- Honda: 16 (11%)
-- BMW: 15 (10%)
-- Mercedes-Benz: 15 (10%)
-- Hyundai: 15 (10%)
-- Tesla: 12 (8%)
-- Porsche: 10 (7%)
-- Chevrolet: 8 (5%)

-- Vehículos por body style
SELECT "BodyStyle",
       COUNT(*) as count
FROM vehicles
GROUP BY "BodyStyle"
ORDER BY count DESC;

-- Resultado esperado:
-- Sedan: 40+
-- SUV: 40+
-- Truck: 30+

-- Vehículos por fuel type
SELECT "FuelType",
       COUNT(*) as count
FROM vehicles
GROUP BY "FuelType"
ORDER BY count DESC;

-- Resultado esperado: Distribuidos entre Gasoline, Diesel, Hybrid, Electric

-- Vehículos sin dealer válido (error!)
SELECT COUNT(*) as orphaned_vehicles
FROM vehicles
WHERE "DealerId" NOT IN (SELECT "Id" FROM dealers);

-- Resultado esperado: 0 (todos deben tener dealer)

-- Vehículos con specs completos
SELECT
    COUNT(*) as total_vehicles,
    COUNT(CASE WHEN "Engine" IS NOT NULL THEN 1 END) as with_engine,
    COUNT(CASE WHEN "Horsepower" > 0 THEN 1 END) as with_horsepower,
    COUNT(CASE WHEN "Features" IS NOT NULL AND array_length("Features", 1) > 0 THEN 1 END) as with_features
FROM vehicles;

-- Resultado esperado: Todos con specs completos
```

---

## 🖼️ VALIDACIÓN DE IMÁGENES

```sql
-- Total de imágenes (debe ser 1,500)
SELECT COUNT(*) as total_images,
       COUNT(DISTINCT "VehicleId") as vehicles_with_images
FROM vehicle_images;

-- Resultado esperado:
-- total_images: 1,500
-- vehicles_with_images: 150

-- Imágenes por vehículo (debe ser 10 cada una)
SELECT "VehicleId",
       COUNT(*) as image_count
FROM vehicle_images
GROUP BY "VehicleId"
HAVING COUNT(*) <> 10
ORDER BY image_count DESC;

-- Resultado esperado: 0 registros (todos con 10 imágenes)

-- Verificar imagen primaria
SELECT "VehicleId",
       COUNT(CASE WHEN "IsPrimary" = true THEN 1 END) as primary_count
FROM vehicle_images
GROUP BY "VehicleId"
HAVING COUNT(CASE WHEN "IsPrimary" = true THEN 1 END) <> 1;

-- Resultado esperado: 0 registros (todos con 1 primaria)

-- URLs con formato correcto
SELECT COUNT(*) as total_picsum_urls
FROM vehicle_images
WHERE "ImageUrl" LIKE 'https://picsum.photos/seed/%';

-- Resultado esperado: 1,500 (100%)

-- Primeras 10 imágenes como muestra
SELECT "VehicleId",
       "ImageUrl",
       "DisplayOrder",
       "IsPrimary"
FROM vehicle_images
ORDER BY "VehicleId", "DisplayOrder"
LIMIT 10;
```

---

## 🏠 VALIDACIÓN DE HOMEPAGE SECTIONS

```sql
-- Total de secciones (debe ser 8)
SELECT COUNT(*) as total_sections,
       STRING_AGG(DISTINCT "Name", ', ') as section_names
FROM homepage_section_configs
WHERE "IsActive" = true;

-- Resultado esperado: 8 secciones activas

-- Vehículos asignados por sección (debe ser 90 total)
SELECT
    hsc."Name",
    hsc."DisplayOrder",
    hsc."MaxItems",
    COUNT(vhs."VehicleId") as assigned_count
FROM homepage_section_configs hsc
LEFT JOIN vehicle_homepage_sections vhs ON hsc."Id" = vhs."HomepageSectionConfigId"
WHERE hsc."IsActive" = true
GROUP BY hsc."Id", hsc."Name", hsc."DisplayOrder", hsc."MaxItems"
ORDER BY hsc."DisplayOrder";

-- Resultado esperado:
-- Carousel Principal: 5
-- Sedanes: 10
-- SUVs: 10
-- Camionetas: 10
-- Deportivos: 10
-- Destacados: 9
-- Lujo: 10
-- Eléctricos: 10
-- TOTAL: 90

-- Verificar vehículos duplicados en secciones
SELECT "VehicleId",
       COUNT(DISTINCT "HomepageSectionConfigId") as section_count
FROM vehicle_homepage_sections
GROUP BY "VehicleId"
HAVING COUNT(DISTINCT "HomepageSectionConfigId") > 1
ORDER BY section_count DESC;

-- Resultado esperado: Algunos vehículos pueden estar en múltiples secciones (OK)

-- Distribuir vehículos sin asignar
SELECT COUNT(DISTINCT v."Id") as unassigned_vehicles
FROM vehicles v
WHERE v."Id" NOT IN (SELECT DISTINCT "VehicleId" FROM vehicle_homepage_sections);

-- Resultado esperado: 60 (150 - 90 asignados)
```

---

## 🔗 VALIDACIÓN DE RELACIONES

### Favorites

```sql
-- Total de favorites (debe ser 50+)
SELECT COUNT(*) as total_favorites,
       COUNT(DISTINCT "UserId") as unique_users,
       COUNT(DISTINCT "VehicleId") as unique_vehicles
FROM favorites;

-- Resultado esperado:
-- total_favorites: 50+
-- unique_users: 5 (buyers)
-- unique_vehicles: ~20-30

-- Favorites por usuario
SELECT
    u."Email",
    COUNT(f."Id") as favorite_count
FROM users u
LEFT JOIN favorites f ON u."Id" = f."UserId"
WHERE u."Role" = 'Buyer'
GROUP BY u."Id", u."Email"
ORDER BY favorite_count DESC;

-- Resultado esperado: 5 buyers con 10+ favorites cada uno

-- Verificar FKs válidas
SELECT COUNT(*) as invalid_favorites
FROM favorites f
WHERE f."UserId" NOT IN (SELECT "Id" FROM users)
   OR f."VehicleId" NOT IN (SELECT "Id" FROM vehicles);

-- Resultado esperado: 0
```

### Price Alerts

```sql
-- Total de alerts (debe ser 15+)
SELECT COUNT(*) as total_alerts,
       COUNT(DISTINCT "UserId") as unique_users
FROM price_alerts;

-- Resultado esperado:
-- total_alerts: 15+
-- unique_users: 3

-- Alertas por usuario
SELECT
    u."Email",
    COUNT(pa."Id") as alert_count
FROM users u
LEFT JOIN price_alerts pa ON u."Id" = pa."UserId"
WHERE u."Role" = 'Buyer'
GROUP BY u."Id", u."Email"
ORDER BY alert_count DESC;

-- Resultado esperado: 3 buyers con 5+ alerts cada uno
```

### Reviews

```sql
-- Total de reviews (debe ser 150+)
SELECT COUNT(*) as total_reviews,
       COUNT(DISTINCT "DealerId") as dealers_reviewed,
       AVG("Rating") as avg_rating
FROM dealer_reviews;

-- Resultado esperado:
-- total_reviews: 150+
-- dealers_reviewed: ~30
-- avg_rating: ~3.0-3.5

-- Reviews por dealer (distribución)
SELECT
    d."BusinessName",
    COUNT(dr."Id") as review_count,
    ROUND(AVG(dr."Rating")::numeric, 2) as avg_rating
FROM dealers d
LEFT JOIN dealer_reviews dr ON d."Id" = dr."DealerId"
GROUP BY d."Id", d."BusinessName"
HAVING COUNT(dr."Id") > 0
ORDER BY review_count DESC
LIMIT 10;

-- Resultado esperado: Top 10 dealers con más reviews

-- Distribución de ratings
SELECT
    "Rating",
    COUNT(*) as count,
    ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM dealer_reviews), 1) as percentage
FROM dealer_reviews
GROUP BY "Rating"
ORDER BY "Rating" DESC;

-- Resultado esperado: Distribución de 1-5 estrellas
```

### Activity Logs

```sql
-- Total de logs (debe ser 100+)
SELECT COUNT(*) as total_logs,
       COUNT(DISTINCT "Action") as unique_actions,
       COUNT(DISTINCT "ResourceType") as resource_types
FROM activity_logs;

-- Resultado esperado:
-- total_logs: 100+
-- unique_actions: 6 (view, favorite, compare, contact, purchase, search)
-- resource_types: 5 (vehicle, dealer, listing, user, alert)

-- Distribución de acciones
SELECT
    "Action",
    COUNT(*) as count,
    ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM activity_logs), 1) as percentage
FROM activity_logs
GROUP BY "Action"
ORDER BY count DESC;

-- Distribución de recursos
SELECT
    "ResourceType",
    COUNT(*) as count
FROM activity_logs
GROUP BY "ResourceType"
ORDER BY count DESC;

-- Logs en últimos 90 días
SELECT COUNT(*) as recent_logs
FROM activity_logs
WHERE "Timestamp" >= NOW() - INTERVAL '90 days';

-- Resultado esperado: ~100
```

---

## ✅ CHECKLIST DE VALIDACIÓN COMPLETA

```sql
-- Script todo-en-uno para ejecutar todas las validaciones

-- 1. CATÁLOGOS
SELECT '=== CATÁLOGOS ===' as section;
SELECT COUNT(*) as makes_count FROM catalog_makes;
SELECT COUNT(*) as models_count FROM catalog_models;
SELECT COUNT(*) as years_count FROM catalog_years;
SELECT COUNT(*) as body_styles_count FROM catalog_body_styles;
SELECT COUNT(*) as fuel_types_count FROM catalog_fuel_types;
SELECT COUNT(*) as colors_count FROM catalog_colors;

-- 2. USUARIOS
SELECT '=== USUARIOS ===' as section;
SELECT COUNT(*) as total_users FROM users;
SELECT "Role", COUNT(*) FROM users GROUP BY "Role";

-- 3. DEALERS
SELECT '=== DEALERS ===' as section;
SELECT COUNT(*) as total_dealers FROM dealers;
SELECT COUNT(*) as total_locations FROM dealer_locations;

-- 4. VEHÍCULOS
SELECT '=== VEHÍCULOS ===' as section;
SELECT COUNT(*) as total_vehicles FROM vehicles;
SELECT COUNT(*) as vehicles_with_images FROM (
    SELECT DISTINCT "VehicleId" FROM vehicle_images
) t;

-- 5. IMÁGENES
SELECT '=== IMÁGENES ===' as section;
SELECT COUNT(*) as total_images FROM vehicle_images;

-- 6. HOMEPAGE
SELECT '=== HOMEPAGE ===' as section;
SELECT COUNT(*) as total_sections FROM homepage_section_configs WHERE "IsActive" = true;
SELECT COUNT(*) as total_assigned_vehicles FROM vehicle_homepage_sections;

-- 7. RELACIONES
SELECT '=== RELACIONES ===' as section;
SELECT COUNT(*) as total_favorites FROM favorites;
SELECT COUNT(*) as total_alerts FROM price_alerts;
SELECT COUNT(*) as total_reviews FROM dealer_reviews;
SELECT COUNT(*) as total_logs FROM activity_logs;

-- RESUMEN FINAL
SELECT '=== RESUMEN FINAL ===' as section;
SELECT
    (SELECT COUNT(*) FROM catalog_makes) as makes,
    (SELECT COUNT(*) FROM vehicles) as vehicles,
    (SELECT COUNT(*) FROM vehicle_images) as images,
    (SELECT COUNT(*) FROM users) as users,
    (SELECT COUNT(*) FROM dealers) as dealers,
    (SELECT COUNT(*) FROM dealer_locations) as locations,
    (SELECT COUNT(*) FROM favorites) as favorites,
    (SELECT COUNT(*) FROM price_alerts) as alerts,
    (SELECT COUNT(*) FROM dealer_reviews) as reviews,
    (SELECT COUNT(*) FROM activity_logs) as logs;
```

---

## 📈 DASHBOARD DE VALIDACIÓN

```sql
-- Query para construir un dashboard de estado

WITH stats AS (
    SELECT
        'Catalogs' as category,
        (SELECT COUNT(*) FROM catalog_makes) as expected,
        (SELECT COUNT(*) FROM catalog_makes) as actual,
        CASE WHEN (SELECT COUNT(*) FROM catalog_makes) = 10 THEN '✅' ELSE '❌' END as status
    UNION ALL
    SELECT
        'Models',
        60,
        (SELECT COUNT(*) FROM catalog_models),
        CASE WHEN (SELECT COUNT(*) FROM catalog_models) >= 60 THEN '✅' ELSE '❌' END
    UNION ALL
    SELECT
        'Users',
        42,
        (SELECT COUNT(*) FROM users),
        CASE WHEN (SELECT COUNT(*) FROM users) = 42 THEN '✅' ELSE '❌' END
    UNION ALL
    SELECT
        'Vehicles',
        150,
        (SELECT COUNT(*) FROM vehicles),
        CASE WHEN (SELECT COUNT(*) FROM vehicles) = 150 THEN '✅' ELSE '❌' END
    UNION ALL
    SELECT
        'Images',
        1500,
        (SELECT COUNT(*) FROM vehicle_images),
        CASE WHEN (SELECT COUNT(*) FROM vehicle_images) = 1500 THEN '✅' ELSE '❌' END
    UNION ALL
    SELECT
        'Favorites',
        50,
        (SELECT COUNT(*) FROM favorites),
        CASE WHEN (SELECT COUNT(*) FROM favorites) >= 50 THEN '✅' ELSE '❌' END
    UNION ALL
    SELECT
        'Alerts',
        15,
        (SELECT COUNT(*) FROM price_alerts),
        CASE WHEN (SELECT COUNT(*) FROM price_alerts) >= 15 THEN '✅' ELSE '❌' END
    UNION ALL
    SELECT
        'Reviews',
        150,
        (SELECT COUNT(*) FROM dealer_reviews),
        CASE WHEN (SELECT COUNT(*) FROM dealer_reviews) >= 150 THEN '✅' ELSE '❌' END
)
SELECT
    category,
    expected,
    actual,
    CASE WHEN expected <= actual THEN '✅ PASS' ELSE '❌ FAIL' END as result,
    actual - expected as difference
FROM stats
ORDER BY category;
```

---

## 🚨 ERRORES COMUNES A DETECTAR

```sql
-- 1. Vehículos sin imágenes
SELECT COUNT(*) as vehicles_without_images
FROM vehicles v
WHERE v."Id" NOT IN (SELECT DISTINCT "VehicleId" FROM vehicle_images);

-- Esperado: 0

-- 2. Vehículos sin dealer
SELECT COUNT(*) as vehicles_without_dealer
FROM vehicles
WHERE "DealerId" IS NULL;

-- Esperado: 0

-- 3. Imágenes con URLs inválidas
SELECT COUNT(*) as invalid_image_urls
FROM vehicle_images
WHERE "ImageUrl" IS NULL OR "ImageUrl" = '';

-- Esperado: 0

-- 4. Usuarios duplicados
SELECT COUNT(*) - COUNT(DISTINCT "Email") as duplicate_emails
FROM users;

-- Esperado: 0

-- 5. Favoritos sin vehículo
SELECT COUNT(*) as orphaned_favorites
FROM favorites f
WHERE f."VehicleId" NOT IN (SELECT "Id" FROM vehicles);

-- Esperado: 0

-- 6. Dealers sin ubicación
SELECT COUNT(*) as dealers_without_location
FROM dealers d
WHERE d."Id" NOT IN (SELECT DISTINCT "DealerId" FROM dealer_locations);

-- Esperado: 0
```

---

**Ejecuta estas queries después de completar el seeding para validar que todo se generó correctamente** ✅
