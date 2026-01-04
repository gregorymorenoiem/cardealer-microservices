-- Script SQL para crear 10 Deportivos para el dealer de República Dominicana
-- Dealer ID: 89de6284-dba0-46f1-8c6b-f2de21ec113b

INSERT INTO vehicles (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status", "VIN",
    "Make", "Model", "Year", "Condition", "BodyStyle", "Transmission", "FuelType",
    "Mileage", "ExteriorColor", "InteriorColor", "Doors", "Seats",
    "City", "State", "Country", "Latitude", "Longitude",
    "SellerId", "SellerName", "SellerPhone", "SellerEmail",
    "IsFeatured", "ViewCount", "CreatedAt", "UpdatedAt", "PublishedAt"
) VALUES
-- 1. Porsche 911 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid, 
 'Porsche 911 Carrera S 2024', 'The iconic sports car. 443 HP twin-turbo flat-six, PDK transmission, sport chrono package.', 
 128900, 'USD', 'active', '1HGBH41JXMN400001',
 'Porsche', '911', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 234, 'Guards Red', 'Black Leather', 2, 4,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 1234, NOW(), NOW(), NOW()),

-- 2. BMW M4 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'BMW M4 Competition 2024', 'Pure driving pleasure. 503 HP twin-turbo inline-6, xDrive available.',
 85500, 'USD', 'active', '1HGBH41JXMN400002',
 'BMW', 'M4', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 567, 'Isle of Man Green', 'Silverstone Merino', 2, 4,
 'Santiago', 'Santiago', 'DO', 19.4517, -70.6970,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 890, NOW(), NOW(), NOW()),

-- 3. Mercedes-AMG GT 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Mercedes-AMG GT 63 2024', 'Handcrafted AMG V8 biturbo. 577 HP, AMG RIDE CONTROL+.',
 145000, 'USD', 'active', '1HGBH41JXMN400003',
 'Mercedes-Benz', 'AMG GT', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 189, 'Obsidian Black', 'Red/Black Nappa', 2, 4,
 'Punta Cana', 'La Altagracia', 'DO', 18.5601, -68.3725,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 567, NOW(), NOW(), NOW()),

-- 4. Audi RS7 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Audi RS7 Sportback 2024', 'Performance meets luxury. 621 HP twin-turbo V8, quattro AWD.',
 126900, 'USD', 'active', '1HGBH41JXMN400004',
 'Audi', 'RS7', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 312, 'Nardo Gray', 'Black Valcona', 4, 4,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 456, NOW(), NOW(), NOW()),

-- 5. Chevrolet Corvette 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Chevrolet Corvette Stingray 2024', 'Mid-engine American icon. 495 HP LT2 V8, magnetic ride control.',
 69800, 'USD', 'active', '1HGBH41JXMN400005',
 'Chevrolet', 'Corvette', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 678, 'Torch Red', 'Jet Black', 2, 2,
 'La Romana', 'La Romana', 'DO', 18.4273, -68.9728,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 1123, NOW(), NOW(), NOW()),

-- 6. Ford Mustang GT 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Ford Mustang GT 2024', 'All-new S650 generation. 480 HP Coyote V8, active exhaust.',
 55900, 'USD', 'active', '1HGBH41JXMN400006',
 'Ford', 'Mustang', 2024, 'new', 'Coupe', 'Manual', 'Gasoline',
 423, 'Vapor Blue', 'Ebony', 2, 4,
 'San Pedro de Macoris', 'San Pedro de Macoris', 'DO', 18.4539, -69.3086,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 789, NOW(), NOW(), NOW()),

-- 7. Dodge Challenger 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Dodge Challenger SRT Hellcat 2024', 'Supercharged muscle. 717 HP Hemi V8, Launch Assist.',
 78000, 'USD', 'active', '1HGBH41JXMN400007',
 'Dodge', 'Challenger', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 234, 'Plum Crazy', 'Black Laguna', 2, 5,
 'Puerto Plata', 'Puerto Plata', 'DO', 19.7934, -70.6884,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 654, NOW(), NOW(), NOW()),

-- 8. Nissan GT-R 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Nissan GT-R Premium 2024', 'Godzilla legend continues. 565 HP twin-turbo V6, ATTESA E-TS AWD.',
 115000, 'USD', 'active', '1HGBH41JXMN400008',
 'Nissan', 'GT-R', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 145, 'Ultimate Silver', 'Black Leather', 2, 4,
 'Higuey', 'La Altagracia', 'DO', 18.6150, -68.7078,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 432, NOW(), NOW(), NOW()),

-- 9. Lexus LC 500 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Lexus LC 500 2024', 'Luxury grand tourer. 471 HP naturally aspirated V8, 10-speed auto.',
 98900, 'USD', 'active', '1HGBH41JXMN400009',
 'Lexus', 'LC 500', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 367, 'Infrared', 'Black Semi-Aniline', 2, 4,
 'Barahona', 'Barahona', 'DO', 18.2083, -71.1003,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 321, NOW(), NOW(), NOW()),

-- 10. Toyota GR Supra 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Toyota GR Supra 3.0 2024', 'The legend reborn. 382 HP inline-6 turbo, limited-slip diff.',
 56250, 'USD', 'active', '1HGBH41JXMN400010',
 'Toyota', 'Supra', 2024, 'new', 'Coupe', 'Automatic', 'Gasoline',
 289, 'Nitro Yellow', 'Black Alcantara', 2, 2,
 'Samana', 'Samana', 'DO', 19.2056, -69.3361,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 567, NOW(), NOW(), NOW());

-- Agregar imágenes para Deportivos
INSERT INTO vehicle_images ("Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "IsPrimary", "SortOrder", "ImageType", "CreatedAt")
SELECT gen_random_uuid(), v."DealerId", v."Id", 
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'Porsche' THEN 'photo-1503376780353-7e6692767b70'
    WHEN v."Make" = 'BMW' THEN 'photo-1555215695-3004980ad54e'
    WHEN v."Make" = 'Mercedes-Benz' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Audi' THEN 'photo-1606664515524-ed2f786a0bd6'
    WHEN v."Make" = 'Chevrolet' THEN 'photo-1552519507-da3b142c6e3d'
    WHEN v."Make" = 'Ford' THEN 'photo-1584060622420-0673aad1066a'
    WHEN v."Make" = 'Dodge' THEN 'photo-1612544448445-b8232cff3b6c'
    WHEN v."Make" = 'Nissan' THEN 'photo-1590362891991-f776e747a588'
    WHEN v."Make" = 'Lexus' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'Toyota' AND v."Model" = 'Supra' THEN 'photo-1580273916550-e323be2ae537'
    ELSE 'photo-1503376780353-7e6692767b70'
  END || '?w=800',
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'Porsche' THEN 'photo-1503376780353-7e6692767b70'
    WHEN v."Make" = 'BMW' THEN 'photo-1555215695-3004980ad54e'
    WHEN v."Make" = 'Mercedes-Benz' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Audi' THEN 'photo-1606664515524-ed2f786a0bd6'
    WHEN v."Make" = 'Chevrolet' THEN 'photo-1552519507-da3b142c6e3d'
    WHEN v."Make" = 'Ford' THEN 'photo-1584060622420-0673aad1066a'
    WHEN v."Make" = 'Dodge' THEN 'photo-1612544448445-b8232cff3b6c'
    WHEN v."Make" = 'Nissan' THEN 'photo-1590362891991-f776e747a588'
    WHEN v."Make" = 'Lexus' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'Toyota' AND v."Model" = 'Supra' THEN 'photo-1580273916550-e323be2ae537'
    ELSE 'photo-1503376780353-7e6692767b70'
  END || '?w=400',
  true, 1, 0, NOW()
FROM vehicles v
WHERE v."DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid 
  AND v."BodyStyle" = 'Coupe'
  AND NOT EXISTS (SELECT 1 FROM vehicle_images vi WHERE vi."VehicleId" = v."Id");

-- Verificar total de vehículos
SELECT "BodyStyle", COUNT(*) as "Count" 
FROM vehicles 
WHERE "DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid 
GROUP BY "BodyStyle" 
ORDER BY "BodyStyle";

SELECT COUNT(*) as "Total Vehicles" FROM vehicles WHERE "DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid;
