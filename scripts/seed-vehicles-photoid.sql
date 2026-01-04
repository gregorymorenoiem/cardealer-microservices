-- =====================================================
-- Seed de Vehículos con PhotoIds para S3
-- =====================================================
-- Ejecutar después de crear las tablas con create-vehicle-tables.sql
-- 
-- Estructura de imágenes en S3:
-- https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/{photoId}.jpg
-- 
-- En la base de datos solo se guarda el photoId, ej: "photo-1618843479313-40f8afb4b4d8"
-- El frontend construye la URL completa con getImageUrl()
-- =====================================================

-- Limpiar datos existentes
TRUNCATE vehicles CASCADE;
TRUNCATE vehicle_makes CASCADE;
TRUNCATE vehicle_models CASCADE;

-- =====================================================
-- CATÁLOGO DE MARCAS
-- =====================================================
INSERT INTO vehicle_makes (id, name, logo_url, is_popular, sort_order) VALUES
  ('11111111-1111-1111-1111-111111111101', 'Mercedes-Benz', NULL, true, 1),
  ('11111111-1111-1111-1111-111111111102', 'BMW', NULL, true, 2),
  ('11111111-1111-1111-1111-111111111103', 'Porsche', NULL, true, 3),
  ('11111111-1111-1111-1111-111111111104', 'Audi', NULL, true, 4),
  ('11111111-1111-1111-1111-111111111105', 'Tesla', NULL, true, 5),
  ('11111111-1111-1111-1111-111111111106', 'Range Rover', NULL, true, 6),
  ('11111111-1111-1111-1111-111111111107', 'Cadillac', NULL, true, 7),
  ('11111111-1111-1111-1111-111111111108', 'Lexus', NULL, true, 8),
  ('11111111-1111-1111-1111-111111111109', 'Maserati', NULL, true, 9),
  ('11111111-1111-1111-1111-111111111110', 'Bentley', NULL, true, 10);

-- =====================================================
-- CATÁLOGO DE MODELOS
-- =====================================================
INSERT INTO vehicle_models (id, make_id, name, is_popular, sort_order) VALUES
  -- Mercedes-Benz
  ('22222222-2222-2222-2222-222222222201', '11111111-1111-1111-1111-111111111101', 'Clase C AMG', true, 1),
  ('22222222-2222-2222-2222-222222222202', '11111111-1111-1111-1111-111111111101', 'GLE Coupe', true, 2),
  -- BMW
  ('22222222-2222-2222-2222-222222222203', '11111111-1111-1111-1111-111111111102', 'Serie 7', true, 1),
  ('22222222-2222-2222-2222-222222222204', '11111111-1111-1111-1111-111111111102', 'X5', true, 2),
  -- Porsche
  ('22222222-2222-2222-2222-222222222205', '11111111-1111-1111-1111-111111111103', '911 Carrera S', true, 1),
  ('22222222-2222-2222-2222-222222222206', '11111111-1111-1111-1111-111111111103', 'Cayenne', true, 2),
  -- Audi
  ('22222222-2222-2222-2222-222222222207', '11111111-1111-1111-1111-111111111104', 'RS7 Sportback', true, 1),
  -- Tesla
  ('22222222-2222-2222-2222-222222222208', '11111111-1111-1111-1111-111111111105', 'Model S Plaid', true, 1),
  ('22222222-2222-2222-2222-222222222209', '11111111-1111-1111-1111-111111111105', 'Model X', true, 2),
  -- Range Rover
  ('22222222-2222-2222-2222-222222222210', '11111111-1111-1111-1111-111111111106', 'Sport HSE', true, 1),
  ('22222222-2222-2222-2222-222222222211', '11111111-1111-1111-1111-111111111106', 'Velar', true, 2),
  -- Cadillac
  ('22222222-2222-2222-2222-222222222212', '11111111-1111-1111-1111-111111111107', 'Escalade', true, 1);

-- =====================================================
-- VEHÍCULOS EN VENTA (6 vehículos)
-- =====================================================
-- Dealer ID fijo para seed
-- En producción, cada dealer tendrá su propio UUID

INSERT INTO vehicles (
  id, dealer_id, title, make, model, year, price, mileage, 
  condition, transmission, fuel_type, body_type, exterior_color,
  city, state, country, status, is_featured, views_count, created_at
) VALUES
  -- v1: Mercedes-Benz Clase C AMG 2024
  (
    'a1111111-1111-1111-1111-111111111111',
    '00000000-0000-0000-0000-000000000001',
    'Mercedes-Benz Clase C AMG 2024',
    'Mercedes-Benz', 'Clase C AMG', 2024, 75000, 1200,
    'New', 'Automatic', 'Gasoline', 'Sedan', 'Silver',
    'Miami', 'FL', 'USA', 'Active', true, 47, NOW()
  ),
  -- v2: BMW Serie 7 Executive Package
  (
    'a2222222-2222-2222-2222-222222222222',
    '00000000-0000-0000-0000-000000000001',
    'BMW Serie 7 Executive Package',
    'BMW', 'Serie 7', 2024, 95000, 800,
    'New', 'Automatic', 'Gasoline', 'Sedan', 'Black',
    'Los Angeles', 'CA', 'USA', 'Active', true, 62, NOW()
  ),
  -- v3: Porsche 911 Carrera S
  (
    'a3333333-3333-3333-3333-333333333333',
    '00000000-0000-0000-0000-000000000001',
    'Porsche 911 Carrera S',
    'Porsche', '911 Carrera S', 2024, 135000, 500,
    'New', 'Automatic', 'Gasoline', 'Coupe', 'Red',
    'New York', 'NY', 'USA', 'Active', true, 89, NOW()
  ),
  -- v4: Audi RS7 Sportback 2024
  (
    'a4444444-4444-4444-4444-444444444444',
    '00000000-0000-0000-0000-000000000001',
    'Audi RS7 Sportback 2024',
    'Audi', 'RS7 Sportback', 2024, 128000, 1500,
    'New', 'Automatic', 'Gasoline', 'Sedan', 'Gray',
    'Dallas', 'TX', 'USA', 'Active', false, 31, NOW()
  ),
  -- v5: Tesla Model S Plaid
  (
    'a5555555-5555-5555-5555-555555555555',
    '00000000-0000-0000-0000-000000000001',
    'Tesla Model S Plaid',
    'Tesla', 'Model S Plaid', 2024, 108000, 2000,
    'New', 'Automatic', 'Electric', 'Sedan', 'White',
    'San Francisco', 'CA', 'USA', 'Active', false, 56, NOW()
  ),
  -- v6: Range Rover Sport HSE
  (
    'a6666666-6666-6666-6666-666666666666',
    '00000000-0000-0000-0000-000000000001',
    'Range Rover Sport HSE',
    'Range Rover', 'Sport HSE', 2024, 89000, 3000,
    'New', 'Automatic', 'Gasoline', 'SUV', 'Green',
    'Phoenix', 'AZ', 'USA', 'Active', false, 28, NOW()
  );

-- =====================================================
-- IMÁGENES DE VEHÍCULOS (solo photoId, no URL completa)
-- =====================================================
-- El frontend usa getImageUrl(photoId, 'vehicles', 'sale') 
-- para construir: https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/{photoId}.jpg

INSERT INTO vehicle_images (id, vehicle_id, image_id, is_primary, sort_order, created_at) VALUES
  -- Mercedes-Benz
  ('b1111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-111111111111', 'photo-1618843479313-40f8afb4b4d8', true, 0, NOW()),
  -- BMW Serie 7
  ('b2222222-2222-2222-2222-222222222222', 'a2222222-2222-2222-2222-222222222222', 'photo-1555215695-3004980ad54e', true, 0, NOW()),
  -- Porsche 911
  ('b3333333-3333-3333-3333-333333333333', 'a3333333-3333-3333-3333-333333333333', 'photo-1503376780353-7e6692767b70', true, 0, NOW()),
  -- Audi RS7
  ('b4444444-4444-4444-4444-444444444444', 'a4444444-4444-4444-4444-444444444444', 'photo-1606664515524-ed2f786a0bd6', true, 0, NOW()),
  -- Tesla Model S
  ('b5555555-5555-5555-5555-555555555555', 'a5555555-5555-5555-5555-555555555555', 'photo-1617788138017-80ad40651399', true, 0, NOW()),
  -- Range Rover
  ('b6666666-6666-6666-6666-666666666666', 'a6666666-6666-6666-6666-666666666666', 'photo-1606016159991-dfe4f2746ad5', true, 0, NOW());

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
DO $$
DECLARE
  v_count INTEGER;
  i_count INTEGER;
BEGIN
  SELECT COUNT(*) INTO v_count FROM vehicles;
  SELECT COUNT(*) INTO i_count FROM vehicle_images;
  
  RAISE NOTICE '✅ Seed completado:';
  RAISE NOTICE '   - Vehículos: %', v_count;
  RAISE NOTICE '   - Imágenes: %', i_count;
  RAISE NOTICE '';
  RAISE NOTICE '📁 Estructura S3 esperada:';
  RAISE NOTICE '   okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/{photoId}.jpg';
END $$;
