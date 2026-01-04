-- =====================================================
-- Seed de Vehículos para VehiclesSaleService
-- =====================================================
-- Estructura S3: https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/{photoId}.jpg
-- 
-- NOTA: En Url guardamos el photoId, el frontend construye la URL completa
-- =====================================================

-- Dealer ID fijo para seed (demo)
-- En producción cada dealer tiene su UUID real

-- =====================================================
-- VEHÍCULOS EN VENTA (6 vehículos del HomePage.tsx)
-- =====================================================

INSERT INTO vehicles (
  "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
  "Make", "Model", "Year", "Condition", "BodyStyle", "Transmission", "FuelType",
  "DriveType", "VehicleType", "Mileage", "MileageUnit", "ExteriorColor",
  "City", "State", "Country", "IsFeatured", "Views", "CreatedAt", "UpdatedAt"
) VALUES
  -- v1: Mercedes-Benz Clase C AMG 2024
  (
    'a1111111-1111-1111-1111-111111111111',
    '00000000-0000-0000-0000-000000000001',
    'Mercedes-Benz Clase C AMG 2024',
    'Vehículo de lujo con todas las prestaciones premium. Motor AMG de alto rendimiento.',
    75000, 'USD', 1,
    'Mercedes-Benz', 'Clase C AMG', 2024, 1, 1, 1, 1,
    0, 0, 1200, 0, 'Silver',
    'Miami', 'FL', 'USA', true, 47, NOW(), NOW()
  ),
  -- v2: BMW Serie 7 Executive Package
  (
    'a2222222-2222-2222-2222-222222222222',
    '00000000-0000-0000-0000-000000000001',
    'BMW Serie 7 Executive Package',
    'Sedán ejecutivo con tecnología de última generación y máximo confort.',
    95000, 'USD', 1,
    'BMW', 'Serie 7', 2024, 1, 1, 1, 1,
    0, 0, 800, 0, 'Black',
    'Los Angeles', 'CA', 'USA', true, 62, NOW(), NOW()
  ),
  -- v3: Porsche 911 Carrera S
  (
    'a3333333-3333-3333-3333-333333333333',
    '00000000-0000-0000-0000-000000000001',
    'Porsche 911 Carrera S',
    'El ícono deportivo por excelencia. Rendimiento excepcional y diseño atemporal.',
    135000, 'USD', 1,
    'Porsche', '911 Carrera S', 2024, 1, 2, 1, 1,
    0, 0, 500, 0, 'Red',
    'New York', 'NY', 'USA', true, 89, NOW(), NOW()
  ),
  -- v4: Audi RS7 Sportback 2024
  (
    'a4444444-4444-4444-4444-444444444444',
    '00000000-0000-0000-0000-000000000001',
    'Audi RS7 Sportback 2024',
    'Deportividad y elegancia en un solo vehículo. Tecnología Quattro.',
    128000, 'USD', 1,
    'Audi', 'RS7 Sportback', 2024, 1, 3, 1, 1,
    0, 0, 1500, 0, 'Gray',
    'Dallas', 'TX', 'USA', false, 31, NOW(), NOW()
  ),
  -- v5: Tesla Model S Plaid
  (
    'a5555555-5555-5555-5555-555555555555',
    '00000000-0000-0000-0000-000000000001',
    'Tesla Model S Plaid',
    'El sedán eléctrico más rápido del mundo. Autonomía excepcional.',
    108000, 'USD', 1,
    'Tesla', 'Model S Plaid', 2024, 1, 1, 1, 3,
    0, 0, 2000, 0, 'White',
    'San Francisco', 'CA', 'USA', false, 56, NOW(), NOW()
  ),
  -- v6: Range Rover Sport HSE
  (
    'a6666666-6666-6666-6666-666666666666',
    '00000000-0000-0000-0000-000000000001',
    'Range Rover Sport HSE',
    'SUV de lujo con capacidades todoterreno inigualables.',
    89000, 'USD', 1,
    'Range Rover', 'Sport HSE', 2024, 1, 4, 1, 1,
    1, 0, 3000, 0, 'Green',
    'Phoenix', 'AZ', 'USA', false, 28, NOW(), NOW()
  );

-- =====================================================
-- IMÁGENES DE VEHÍCULOS
-- =====================================================
-- Guardamos el photoId en Url, el frontend construye la URL completa con getS3ImageUrl()

INSERT INTO vehicle_images (
  "Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "Caption",
  "ImageType", "SortOrder", "IsPrimary", "MimeType", "CreatedAt"
) VALUES
  -- Mercedes-Benz
  (
    'b1111111-1111-1111-1111-111111111111',
    '00000000-0000-0000-0000-000000000001',
    'a1111111-1111-1111-1111-111111111111',
    'photo-1618843479313-40f8afb4b4d8',
    'photo-1618843479313-40f8afb4b4d8',
    'Mercedes-Benz Clase C AMG - Vista Principal',
    0, 0, true, 'image/jpeg', NOW()
  ),
  -- BMW Serie 7
  (
    'b2222222-2222-2222-2222-222222222222',
    '00000000-0000-0000-0000-000000000001',
    'a2222222-2222-2222-2222-222222222222',
    'photo-1555215695-3004980ad54e',
    'photo-1555215695-3004980ad54e',
    'BMW Serie 7 Executive - Vista Principal',
    0, 0, true, 'image/jpeg', NOW()
  ),
  -- Porsche 911
  (
    'b3333333-3333-3333-3333-333333333333',
    '00000000-0000-0000-0000-000000000001',
    'a3333333-3333-3333-3333-333333333333',
    'photo-1503376780353-7e6692767b70',
    'photo-1503376780353-7e6692767b70',
    'Porsche 911 Carrera S - Vista Principal',
    0, 0, true, 'image/jpeg', NOW()
  ),
  -- Audi RS7
  (
    'b4444444-4444-4444-4444-444444444444',
    '00000000-0000-0000-0000-000000000001',
    'a4444444-4444-4444-4444-444444444444',
    'photo-1606664515524-ed2f786a0bd6',
    'photo-1606664515524-ed2f786a0bd6',
    'Audi RS7 Sportback - Vista Principal',
    0, 0, true, 'image/jpeg', NOW()
  ),
  -- Tesla Model S
  (
    'b5555555-5555-5555-5555-555555555555',
    '00000000-0000-0000-0000-000000000001',
    'a5555555-5555-5555-5555-555555555555',
    'photo-1617788138017-80ad40651399',
    'photo-1617788138017-80ad40651399',
    'Tesla Model S Plaid - Vista Principal',
    0, 0, true, 'image/jpeg', NOW()
  ),
  -- Range Rover
  (
    'b6666666-6666-6666-6666-666666666666',
    '00000000-0000-0000-0000-000000000001',
    'a6666666-6666-6666-6666-666666666666',
    'photo-1606016159991-dfe4f2746ad5',
    'photo-1606016159991-dfe4f2746ad5',
    'Range Rover Sport HSE - Vista Principal',
    0, 0, true, 'image/jpeg', NOW()
  );

-- =====================================================
-- CATÁLOGO DE MARCAS
-- =====================================================
INSERT INTO vehicle_makes ("Id", "Name", "LogoUrl", "Country", "IsActive", "SortOrder", "CreatedAt")
VALUES
  ('11111111-1111-1111-1111-111111111101', 'Mercedes-Benz', NULL, 'Germany', true, 1, NOW()),
  ('11111111-1111-1111-1111-111111111102', 'BMW', NULL, 'Germany', true, 2, NOW()),
  ('11111111-1111-1111-1111-111111111103', 'Porsche', NULL, 'Germany', true, 3, NOW()),
  ('11111111-1111-1111-1111-111111111104', 'Audi', NULL, 'Germany', true, 4, NOW()),
  ('11111111-1111-1111-1111-111111111105', 'Tesla', NULL, 'USA', true, 5, NOW()),
  ('11111111-1111-1111-1111-111111111106', 'Range Rover', NULL, 'UK', true, 6, NOW()),
  ('11111111-1111-1111-1111-111111111107', 'Cadillac', NULL, 'USA', true, 7, NOW()),
  ('11111111-1111-1111-1111-111111111108', 'Lexus', NULL, 'Japan', true, 8, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- CATÁLOGO DE MODELOS
-- =====================================================
INSERT INTO vehicle_models ("Id", "MakeId", "Name", "Category", "IsActive", "SortOrder", "CreatedAt")
VALUES
  -- Mercedes-Benz
  ('22222222-2222-2222-2222-222222222201', '11111111-1111-1111-1111-111111111101', 'Clase C AMG', 'Sedan', true, 1, NOW()),
  ('22222222-2222-2222-2222-222222222202', '11111111-1111-1111-1111-111111111101', 'GLE Coupe', 'SUV', true, 2, NOW()),
  -- BMW
  ('22222222-2222-2222-2222-222222222203', '11111111-1111-1111-1111-111111111102', 'Serie 7', 'Sedan', true, 1, NOW()),
  ('22222222-2222-2222-2222-222222222204', '11111111-1111-1111-1111-111111111102', 'X5', 'SUV', true, 2, NOW()),
  -- Porsche
  ('22222222-2222-2222-2222-222222222205', '11111111-1111-1111-1111-111111111103', '911 Carrera S', 'Coupe', true, 1, NOW()),
  ('22222222-2222-2222-2222-222222222206', '11111111-1111-1111-1111-111111111103', 'Cayenne', 'SUV', true, 2, NOW()),
  -- Audi
  ('22222222-2222-2222-2222-222222222207', '11111111-1111-1111-1111-111111111104', 'RS7 Sportback', 'Sportback', true, 1, NOW()),
  -- Tesla
  ('22222222-2222-2222-2222-222222222208', '11111111-1111-1111-1111-111111111105', 'Model S Plaid', 'Sedan', true, 1, NOW()),
  ('22222222-2222-2222-2222-222222222209', '11111111-1111-1111-1111-111111111105', 'Model X', 'SUV', true, 2, NOW()),
  -- Range Rover
  ('22222222-2222-2222-2222-222222222210', '11111111-1111-1111-1111-111111111106', 'Sport HSE', 'SUV', true, 1, NOW()),
  ('22222222-2222-2222-2222-222222222211', '11111111-1111-1111-1111-111111111106', 'Velar', 'SUV', true, 2, NOW()),
  -- Cadillac
  ('22222222-2222-2222-2222-222222222212', '11111111-1111-1111-1111-111111111107', 'Escalade', 'SUV', true, 1, NOW())
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
DO $$
DECLARE
  v_count INTEGER;
  i_count INTEGER;
  m_count INTEGER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM vehicles;
  SELECT COUNT(*) INTO i_count FROM vehicle_images;
  SELECT COUNT(*) INTO m_count FROM vehicle_makes;
  
  RAISE NOTICE '';
  RAISE NOTICE '✅ Seed completado exitosamente:';
  RAISE NOTICE '   📦 Vehículos: %', v_count;
  RAISE NOTICE '   🖼️  Imágenes: %', i_count;
  RAISE NOTICE '   🏭 Marcas: %', m_count;
  RAISE NOTICE '';
  RAISE NOTICE '📁 Las imágenes en Url contienen photoIds:';
  RAISE NOTICE '   Ejemplo: photo-1618843479313-40f8afb4b4d8';
  RAISE NOTICE '';
  RAISE NOTICE '🌐 El frontend debe construir la URL completa:';
  RAISE NOTICE '   https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/{photoId}.jpg';
END $$;
