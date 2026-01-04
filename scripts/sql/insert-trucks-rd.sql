-- Script SQL para crear 10 Camionetas/Trucks para el dealer de República Dominicana
-- Dealer ID: 89de6284-dba0-46f1-8c6b-f2de21ec113b

INSERT INTO vehicles (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status", "VIN",
    "Make", "Model", "Year", "Condition", "BodyStyle", "Transmission", "FuelType",
    "Mileage", "ExteriorColor", "InteriorColor", "Doors", "Seats",
    "City", "State", "Country", "Latitude", "Longitude",
    "SellerId", "SellerName", "SellerPhone", "SellerEmail",
    "IsFeatured", "ViewCount", "CreatedAt", "UpdatedAt", "PublishedAt"
) VALUES
-- 1. Ford F-150 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid, 
 'Ford F-150 2024', 'Best-selling truck in America. 5.0L V8, 4x4, tow package, and bed liner included.', 
 52500, 'USD', 'active', '1HGBH41JXMN300001',
 'Ford', 'F-150', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 567, 'Oxford White', 'Gray', 4, 5,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 567, NOW(), NOW(), NOW()),

-- 2. Chevrolet Silverado 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Chevrolet Silverado 2024', 'Rugged and reliable. 6.2L V8 with 10-speed automatic transmission.',
 49800, 'USD', 'active', '1HGBH41JXMN300002',
 'Chevrolet', 'Silverado', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 423, 'Black', 'Jet Black', 4, 5,
 'Santiago', 'Santiago', 'DO', 19.4517, -70.6970,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 423, NOW(), NOW(), NOW()),

-- 3. RAM 1500 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'RAM 1500 2024', 'Luxury meets capability. Air suspension, 12-inch touchscreen, and Harman Kardon audio.',
 54900, 'USD', 'active', '1HGBH41JXMN300003',
 'RAM', '1500', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 389, 'Diamond Black', 'Indigo', 4, 5,
 'Punta Cana', 'La Altagracia', 'DO', 18.5601, -68.3725,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 389, NOW(), NOW(), NOW()),

-- 4. Toyota Tundra 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Toyota Tundra 2024', 'All-new design with i-FORCE MAX hybrid powertrain. Unmatched reliability.',
 51990, 'USD', 'active', '1HGBH41JXMN300004',
 'Toyota', 'Tundra', 2024, 'new', 'Truck', 'Automatic', 'Hybrid',
 312, 'Celestial Silver', 'Black', 4, 5,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 312, NOW(), NOW(), NOW()),

-- 5. GMC Sierra 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'GMC Sierra 2024', 'Denali Ultimate trim. CarbonPro bed, MultiPro tailgate, and Super Cruise.',
 53500, 'USD', 'active', '1HGBH41JXMN300005',
 'GMC', 'Sierra', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 278, 'Summit White', 'Jet Black', 4, 5,
 'La Romana', 'La Romana', 'DO', 18.4273, -68.9728,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 278, NOW(), NOW(), NOW()),

-- 6. Nissan Titan 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Nissan Titan 2024', 'Powerful 5.6L V8 engine. PRO-4X off-road package available.',
 47500, 'USD', 'active', '1HGBH41JXMN300006',
 'Nissan', 'Titan', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 167, 'Gun Metallic', 'Charcoal', 4, 5,
 'San Pedro de Macoris', 'San Pedro de Macoris', 'DO', 18.4539, -69.3086,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 167, NOW(), NOW(), NOW()),

-- 7. Ford Ranger 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Ford Ranger 2024', 'Midsize truck with Tremor off-road package. 2.3L EcoBoost engine.',
 38800, 'USD', 'active', '1HGBH41JXMN300007',
 'Ford', 'Ranger', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 234, 'Cactus Gray', 'Ebony', 4, 5,
 'Puerto Plata', 'Puerto Plata', 'DO', 19.7934, -70.6884,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 234, NOW(), NOW(), NOW()),

-- 8. Chevrolet Colorado 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Chevrolet Colorado 2024', 'All-new design with available turbo diesel. ZR2 off-road variant.',
 36500, 'USD', 'active', '1HGBH41JXMN300008',
 'Chevrolet', 'Colorado', 2024, 'new', 'Truck', 'Automatic', 'Diesel',
 189, 'Nitro Yellow', 'Black', 4, 5,
 'Higuey', 'La Altagracia', 'DO', 18.6150, -68.7078,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 189, NOW(), NOW(), NOW()),

-- 9. Toyota Tacoma 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Toyota Tacoma 2024', 'All-new generation with i-FORCE MAX. TRD Pro package with Fox shocks.',
 42900, 'USD', 'active', '1HGBH41JXMN300009',
 'Toyota', 'Tacoma', 2024, 'new', 'Truck', 'Manual', 'Hybrid',
 456, 'Terra', 'Black', 4, 5,
 'Barahona', 'Barahona', 'DO', 18.2083, -71.1003,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 456, NOW(), NOW(), NOW()),

-- 10. Honda Ridgeline 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Honda Ridgeline 2024', 'Unique unibody truck with in-bed trunk. AWD standard, Honda Sensing.',
 41800, 'USD', 'active', '1HGBH41JXMN300010',
 'Honda', 'Ridgeline', 2024, 'new', 'Truck', 'Automatic', 'Gasoline',
 145, 'Modern Steel', 'Black', 4, 5,
 'Samana', 'Samana', 'DO', 19.2056, -69.3361,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 145, NOW(), NOW(), NOW());

-- Agregar imágenes
INSERT INTO vehicle_images ("Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "IsPrimary", "SortOrder", "ImageType", "CreatedAt")
SELECT gen_random_uuid(), v."DealerId", v."Id", 
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'Ford' AND v."Model" = 'F-150' THEN 'photo-1544636331-e26879cd4d9b'
    WHEN v."Make" = 'Chevrolet' AND v."Model" = 'Silverado' THEN 'photo-1590362891991-f776e747a588'
    WHEN v."Make" = 'RAM' THEN 'photo-1533473359331-0135ef1b58bf'
    WHEN v."Make" = 'Toyota' AND v."Model" = 'Tundra' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'GMC' THEN 'photo-1580273916550-e323be2ae537'
    WHEN v."Make" = 'Nissan' THEN 'photo-1544636331-e26879cd4d9b'
    WHEN v."Make" = 'Ford' AND v."Model" = 'Ranger' THEN 'photo-1590362891991-f776e747a588'
    WHEN v."Make" = 'Chevrolet' AND v."Model" = 'Colorado' THEN 'photo-1560958089-b8a1929cea89'
    WHEN v."Make" = 'Toyota' AND v."Model" = 'Tacoma' THEN 'photo-1606016159991-dfe4f2746ad5'
    WHEN v."Make" = 'Honda' THEN 'photo-1533473359331-0135ef1b58bf'
    ELSE 'photo-1544636331-e26879cd4d9b'
  END || '?w=800',
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'Ford' AND v."Model" = 'F-150' THEN 'photo-1544636331-e26879cd4d9b'
    WHEN v."Make" = 'Chevrolet' AND v."Model" = 'Silverado' THEN 'photo-1590362891991-f776e747a588'
    WHEN v."Make" = 'RAM' THEN 'photo-1533473359331-0135ef1b58bf'
    WHEN v."Make" = 'Toyota' AND v."Model" = 'Tundra' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'GMC' THEN 'photo-1580273916550-e323be2ae537'
    WHEN v."Make" = 'Nissan' THEN 'photo-1544636331-e26879cd4d9b'
    WHEN v."Make" = 'Ford' AND v."Model" = 'Ranger' THEN 'photo-1590362891991-f776e747a588'
    WHEN v."Make" = 'Chevrolet' AND v."Model" = 'Colorado' THEN 'photo-1560958089-b8a1929cea89'
    WHEN v."Make" = 'Toyota' AND v."Model" = 'Tacoma' THEN 'photo-1606016159991-dfe4f2746ad5'
    WHEN v."Make" = 'Honda' THEN 'photo-1533473359331-0135ef1b58bf'
    ELSE 'photo-1544636331-e26879cd4d9b'
  END || '?w=400',
  true, 1, 0, NOW()
FROM vehicles v
WHERE v."DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid 
  AND v."BodyStyle" = 'Truck'
  AND NOT EXISTS (SELECT 1 FROM vehicle_images vi WHERE vi."VehicleId" = v."Id");

SELECT COUNT(*) as "Total Trucks" FROM vehicles WHERE "DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid AND "BodyStyle" = 'Truck';
