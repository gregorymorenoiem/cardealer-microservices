-- Script SQL para crear 10 SUVs para el dealer de República Dominicana
-- Dealer ID: 89de6284-dba0-46f1-8c6b-f2de21ec113b

-- Insertar 10 SUVs
INSERT INTO vehicles (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status", "VIN",
    "Make", "Model", "Year", "Condition", "BodyStyle", "Transmission", "FuelType",
    "Mileage", "ExteriorColor", "InteriorColor", "Doors", "Seats",
    "City", "State", "Country", "Latitude", "Longitude",
    "SellerId", "SellerName", "SellerPhone", "SellerEmail",
    "IsFeatured", "ViewCount", "CreatedAt", "UpdatedAt", "PublishedAt"
) VALUES
-- 1. BMW X5 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid, 
 'BMW X5 2024', 'Luxury SUV with M Sport package. Panoramic roof, heated seats, and premium Harman Kardon audio.', 
 68500, 'USD', 'active', '1HGBH41JXMN200001',
 'BMW', 'X5', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 145, 'Black', 'Cognac', 4, 7,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 145, NOW(), NOW(), NOW()),

-- 2. Mercedes-Benz GLE 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Mercedes-Benz GLE 2024', 'Elegance and capability combined. MBUX infotainment system and 4MATIC AWD.',
 72000, 'USD', 'active', '1HGBH41JXMN200002',
 'Mercedes-Benz', 'GLE', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 112, 'Silver', 'Black', 4, 5,
 'Santiago', 'Santiago', 'DO', 19.4517, -70.6970,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 112, NOW(), NOW(), NOW()),

-- 3. Audi Q7 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Audi Q7 2024', 'Three-row luxury SUV with Quattro AWD and Virtual Cockpit technology.',
 65900, 'USD', 'active', '1HGBH41JXMN200003',
 'Audi', 'Q7', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 189, 'White', 'Beige', 4, 7,
 'Punta Cana', 'La Altagracia', 'DO', 18.5601, -68.3725,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 189, NOW(), NOW(), NOW()),

-- 4. Tesla Model X 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Tesla Model X 2024', 'Electric SUV with falcon-wing doors. Long range, Autopilot, and premium interior.',
 89990, 'USD', 'active', '1HGBH41JXMN200004',
 'Tesla', 'Model X', 2024, 'new', 'SUV', 'Automatic', 'Electric',
 0, 'Pearl White', 'Cream', 4, 7,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 267, NOW(), NOW(), NOW()),

-- 5. Lexus RX 350 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Lexus RX 350 2024', 'Refined luxury crossover with legendary Lexus reliability and comfort.',
 54500, 'USD', 'active', '1HGBH41JXMN200005',
 'Lexus', 'RX 350', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 156, 'Atomic Silver', 'Tan', 4, 5,
 'La Romana', 'La Romana', 'DO', 18.4273, -68.9728,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 156, NOW(), NOW(), NOW()),

-- 6. Honda CR-V 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Honda CR-V 2024', 'Practical and efficient compact SUV. Honda Sensing included, spacious interior.',
 34500, 'USD', 'active', '1HGBH41JXMN200006',
 'Honda', 'CR-V', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 389, 'Steel Blue', 'Gray', 4, 5,
 'San Pedro de Macoris', 'San Pedro de Macoris', 'DO', 18.4539, -69.3086,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 389, NOW(), NOW(), NOW()),

-- 7. Toyota RAV4 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Toyota RAV4 2024', 'Best-selling compact SUV. Toyota Safety Sense 2.0 and all-wheel drive.',
 32800, 'USD', 'active', '1HGBH41JXMN200007',
 'Toyota', 'RAV4', 2024, 'new', 'SUV', 'Automatic', 'Hybrid',
 512, 'Ruby Red', 'Black', 4, 5,
 'Puerto Plata', 'Puerto Plata', 'DO', 19.7934, -70.6884,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 512, NOW(), NOW(), NOW()),

-- 8. Mazda CX-5 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Mazda CX-5 2024', 'Premium feel compact SUV with KODO design. Bose audio and heads-up display.',
 31500, 'USD', 'active', '1HGBH41JXMN200008',
 'Mazda', 'CX-5', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 234, 'Soul Red', 'Parchment', 4, 5,
 'Higuey', 'La Altagracia', 'DO', 18.6150, -68.7078,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 234, NOW(), NOW(), NOW()),

-- 9. Hyundai Tucson 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Hyundai Tucson 2024', 'Bold new design with advanced hybrid powertrain option. 10-year warranty.',
 29900, 'USD', 'active', '1HGBH41JXMN200009',
 'Hyundai', 'Tucson', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 278, 'Amazon Gray', 'Gray', 4, 5,
 'Barahona', 'Barahona', 'DO', 18.2083, -71.1003,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 278, NOW(), NOW(), NOW()),

-- 10. Kia Sportage 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Kia Sportage 2024', 'Award-winning design with dual panoramic display. Excellent value proposition.',
 28800, 'USD', 'active', '1HGBH41JXMN200010',
 'Kia', 'Sportage', 2024, 'new', 'SUV', 'Automatic', 'Gasoline',
 198, 'Moss Green', 'Black', 4, 5,
 'Samana', 'Samana', 'DO', 19.2056, -69.3361,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 198, NOW(), NOW(), NOW());

-- Agregar imágenes
INSERT INTO vehicle_images ("Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "IsPrimary", "SortOrder", "ImageType", "CreatedAt")
SELECT gen_random_uuid(), v."DealerId", v."Id", 
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'BMW' THEN 'photo-1519641471654-76ce0107ad1b'
    WHEN v."Make" = 'Mercedes-Benz' THEN 'photo-1606016159991-dfe4f2746ad5'
    WHEN v."Make" = 'Audi' THEN 'photo-1519641471654-76ce0107ad1b'
    WHEN v."Make" = 'Tesla' THEN 'photo-1560958089-b8a1929cea89'
    WHEN v."Make" = 'Lexus' THEN 'photo-1606016159991-dfe4f2746ad5'
    WHEN v."Make" = 'Honda' THEN 'photo-1533473359331-0135ef1b58bf'
    WHEN v."Make" = 'Toyota' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'Mazda' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Hyundai' THEN 'photo-1580273916550-e323be2ae537'
    WHEN v."Make" = 'Kia' THEN 'photo-1552519507-da3b142c6e3d'
  END || '?w=800',
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'BMW' THEN 'photo-1519641471654-76ce0107ad1b'
    WHEN v."Make" = 'Mercedes-Benz' THEN 'photo-1606016159991-dfe4f2746ad5'
    WHEN v."Make" = 'Audi' THEN 'photo-1519641471654-76ce0107ad1b'
    WHEN v."Make" = 'Tesla' THEN 'photo-1560958089-b8a1929cea89'
    WHEN v."Make" = 'Lexus' THEN 'photo-1606016159991-dfe4f2746ad5'
    WHEN v."Make" = 'Honda' THEN 'photo-1533473359331-0135ef1b58bf'
    WHEN v."Make" = 'Toyota' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'Mazda' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Hyundai' THEN 'photo-1580273916550-e323be2ae537'
    WHEN v."Make" = 'Kia' THEN 'photo-1552519507-da3b142c6e3d'
  END || '?w=400',
  true, 1, 0, NOW()
FROM vehicles v
WHERE v."DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid 
  AND v."BodyStyle" = 'SUV'
  AND NOT EXISTS (SELECT 1 FROM vehicle_images vi WHERE vi."VehicleId" = v."Id");

SELECT COUNT(*) as "Total SUVs" FROM vehicles WHERE "DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid AND "BodyStyle" = 'SUV';
