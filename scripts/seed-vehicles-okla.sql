-- ============================================================
-- SCRIPT: Seed inicial de vehículos para OKLA
-- Base de datos: vehiclessaleservice
-- Generado: 2026-03-xx
-- NOTA: Columnas con HasConversion<string>(): Status, VehicleType, BodyStyle,
--       FuelType, Transmission, DriveType, MileageUnit, Condition
-- NOTA: SellerType es integer (0=Seller, 1=Dealer, 2=Franchise)
-- NOTA: MileageUnit enum value: Kilometers (NOT Km)
-- Ejecución: docker exec -i postgres_db psql -U postgres -d vehiclessaleservice < scripts/seed-vehicles-okla.sql
-- ============================================================

\set ON_ERROR_STOP on

-- ============================================================
-- VEHÍCULOS (10 vehículos variados para cubrir las secciones del homepage)
-- DealerId/SellerId = 00000000-0000-0000-0000-000000000001 (OKLA catalog dealer)
-- ============================================================

INSERT INTO vehicles (
    "Id", "DealerId", "Title", "Description",
    "Price", "Currency", "Status",
    "SellerId", "SellerName", "SellerType", "SellerPhone", "SellerVerified",
    "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "Country",
    "AccidentHistory", "HasCleanTitle", "IsCertified",
    "ViewCount", "FavoriteCount", "InquiryCount",
    "IsDeleted", "IsFeatured"
) VALUES

-- 1. Sedan - Toyota Camry
(
    'b1000001-0000-4000-8000-000000000001'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Toyota Camry LE 2023',
    'Sedán ejecutivo en excelente estado. Motor 2.5L con 203 HP, bajo consumo de combustible, sistema de seguridad Toyota Safety Sense. Ideal para uso diario en Santo Domingo.',
    2450000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Toyota', 'Camry', 'LE', 2023,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '2.5L I4', 203, 'Automatic', 'FWD',
    18500, 'Kilometers', 'Used',
    'Midnight Black', 'Black',
    'Santo Domingo', 'DO',
    false, true, false,
    0, 0, 0, false, true
),

-- 2. SUV - Honda CR-V
(
    'b1000002-0000-4000-8000-000000000002'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Honda CR-V EX-L 2024',
    'SUV compacta con tracción AWD. Interior de lujo con cuero, pantalla táctil de 9 pulgadas, sunroof panorámico y sistema Honda Sensing completo. Perfecta para familias.',
    3200000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Honda', 'CR-V', 'EX-L', 2024,
    'SUV', 'SUV', 4, 5,
    'Gasoline', '1.5L Turbo I4', 190, 'Automatic', 'AWD',
    8200, 'Kilometers', 'New',
    'Sonic Gray Pearl', 'Black',
    'Santo Domingo', 'DO',
    false, true, true,
    0, 0, 0, false, true
),

-- 3. Pickup - Toyota Hilux
(
    'b1000003-0000-4000-8000-000000000003'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Toyota Hilux SR5 4x4 2022',
    'Pickup 4x4 doble cabina, ideal para trabajo y aventura. Motor diesel 2.8L turbo, caja manual de 6 velocidades, diferencial mecánico delantero. Muy popular en República Dominicana.',
    2850000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Toyota', 'Hilux', 'SR5 4x4', 2022,
    'Truck', 'Pickup', 4, 5,
    'Diesel', '2.8L Turbo I4', 204, 'Manual', 'FourWD',
    42000, 'Kilometers', 'Used',
    'White', 'Black',
    'Santiago', 'DO',
    false, true, false,
    0, 0, 0, false, true
),

-- 4. Hatchback - Volkswagen Golf
(
    'b1000004-0000-4000-8000-000000000004'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Volkswagen Golf GTI 2023',
    'Hatchback deportivo con motor 2.0 TSI de 245 HP y transmisión DSG de 7 velocidades. Asientos de cuero rojo, diferencial electrónico XDS+. El mejor GTI de todos los tiempos.',
    3800000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Volkswagen', 'Golf', 'GTI', 2023,
    'Car', 'Hatchback', 4, 5,
    'Gasoline', '2.0L TSI', 245, 'Automatic', 'FWD',
    12000, 'Kilometers', 'Used',
    'Deep Black Pearl', 'Black',
    'Santo Domingo', 'DO',
    false, true, false,
    0, 0, 0, false, false
),

-- 5. Van - Toyota Hiace
(
    'b1000005-0000-4000-8000-000000000005'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Toyota Hiace Commuter 2022',
    'Van para pasajeros con capacidad de 15 personas. Motor 2.8L diesel, amplio espacio interior, climatización de doble zona, ideal para tours, transporte corporativo y colegios.',
    4200000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Toyota', 'Hiace', 'Commuter', 2022,
    'Van', 'Van', 4, 15,
    'Diesel', '2.8L I4 Diesel', 150, 'Manual', 'RWD',
    65000, 'Kilometers', 'Used',
    'White', 'Gray',
    'Santo Domingo', 'DO',
    false, true, false,
    0, 0, 0, false, false
),

-- 6. Convertible - Mazda MX-5 Miata
(
    'b1000006-0000-4000-8000-000000000006'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Mazda MX-5 Miata RF 2023',
    'Roadster convertible con techo duro retráctil. El auto deportivo más puro del mercado. Motor SKYACTIV-G 2.0L de 181 HP, transmisión manual de 6 velocidades. Perfecto para la carretera 6 Lanes.',
    3500000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Mazda', 'MX-5 Miata', 'RF Grand Touring', 2023,
    'Car', 'Convertible', 2, 2,
    'Gasoline', '2.0L SKYACTIV-G', 181, 'Manual', 'RWD',
    6500, 'Kilometers', 'New',
    'Soul Red Crystal', 'Black',
    'Santo Domingo', 'DO',
    false, true, false,
    0, 0, 0, false, false
),

-- 7. Sports Car - Ford Mustang
(
    'b1000007-0000-4000-8000-000000000007'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Ford Mustang GT 5.0 2022',
    'Pony car icónico con motor V8 5.0L de 450 HP. Transmisión automática Selectshift de 10 velocidades, modo pista, launch control y diferencial electrónico de deslizamiento limitado.',
    5800000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Ford', 'Mustang', 'GT Premium', 2022,
    'Car', 'SportsCar', 2, 4,
    'Gasoline', '5.0L V8', 450, 'Automatic', 'RWD',
    28000, 'Kilometers', 'Used',
    'Race Red', 'Ebony',
    'Santo Domingo', 'DO',
    false, true, false,
    0, 0, 0, false, true
),

-- 8. Electric - Tesla Model 3
(
    'b1000008-0000-4000-8000-000000000008'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Tesla Model 3 Long Range 2023',
    'Sedán eléctrico con 576 km de autonomía. Aceleración de 0-100 en 4.4s, carga rápida Supercharger, Autopilot incluido. El futuro de la movilidad en República Dominicana.',
    6200000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Tesla', 'Model 3', 'Long Range AWD', 2023,
    'Car', 'Sedan', 4, 5,
    'Electric', 'Dual Motor Electric', 346, 'Automatic', 'AWD',
    15000, 'Kilometers', 'Used',
    'Pearl White', 'White',
    'Santo Domingo', 'DO',
    false, true, false,
    0, 0, 0, false, true
),

-- 9. Hybrid - Toyota RAV4 Hybrid
(
    'b1000009-0000-4000-8000-000000000009'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Toyota RAV4 Hybrid XSE 2023',
    'SUV híbrido con sistema eAWD. Motor 2.5L más motores eléctricos delantero y trasero, 219 HP combinados. Excelente eficiencia de combustible, ideal para ciudad y carretera.',
    4100000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Toyota', 'RAV4', 'Hybrid XSE', 2023,
    'SUV', 'SUV', 4, 5,
    'Hybrid', '2.5L I4 + Electric', 219, 'Automatic', 'AWD',
    22000, 'Kilometers', 'Used',
    'Magnetic Gray', 'Black',
    'Santiago', 'DO',
    false, true, false,
    0, 0, 0, false, false
),

-- 10. Luxury Sedan - Mercedes-Benz Clase C
(
    'b1000010-0000-4000-8000-000000000010'::uuid,
    '00000000-0000-0000-0000-000000000001'::uuid,
    'Mercedes-Benz C300 AMG Line 2024',
    'Sedán de lujo con diseño AMG Line. Motor 2.0L turbo de 255 HP, transmisión 9G-Tronic, sistema MBUX con pantallas curvas, asientos de cuero Nappa. El auto más vendido en su segmento en RD.',
    7500000, 'DOP', 'Active',
    '00000000-0000-0000-0000-000000000001'::uuid, 'OKLA Motors', 1, '+1(809)200-6552', true,
    'Mercedes-Benz', 'C300', 'AMG Line', 2024,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '2.0L Turbo I4', 255, 'Automatic', 'RWD',
    5000, 'Kilometers', 'New',
    'Obsidian Black', 'Macchiato Beige',
    'Santo Domingo', 'DO',
    false, true, true,
    0, 0, 0, false, true
)

ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- IMÁGENES (una imagen principal por vehículo via Unsplash)
-- ============================================================

INSERT INTO vehicle_images (
    "Id", "DealerId", "VehicleId", "Url",
    "ImageType", "SortOrder", "IsPrimary"
) VALUES

-- Toyota Camry
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000001-0000-4000-8000-000000000001'::uuid,
 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&q=75', 0, 1, true),

-- Honda CR-V
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000002-0000-4000-8000-000000000002'::uuid,
 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&q=75', 0, 1, true),

-- Toyota Hilux
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000003-0000-4000-8000-000000000003'::uuid,
 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&q=75', 0, 1, true),

-- VW Golf GTI
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000004-0000-4000-8000-000000000004'::uuid,
 'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800&q=75', 0, 1, true),

-- Toyota Hiace
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000005-0000-4000-8000-000000000005'::uuid,
 'https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800&q=75', 0, 1, true),

-- Mazda MX-5
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000006-0000-4000-8000-000000000006'::uuid,
 'https://images.unsplash.com/photo-1544636331-e26879cd4d9b?w=800&q=75', 0, 1, true),

-- Ford Mustang
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000007-0000-4000-8000-000000000007'::uuid,
 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=75', 0, 1, true),

-- Tesla Model 3
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000008-0000-4000-8000-000000000008'::uuid,
 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&q=75', 0, 1, true),

-- Toyota RAV4 Hybrid
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000009-0000-4000-8000-000000000009'::uuid,
 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&q=75', 0, 1, true),

-- Mercedes-Benz C300
(gen_random_uuid(), '00000000-0000-0000-0000-000000000001'::uuid, 'b1000010-0000-4000-8000-000000000010'::uuid,
 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=75', 0, 1, true)

ON CONFLICT DO NOTHING;

SELECT 'Seed completado: ' || COUNT(*) || ' vehículos insertados.' FROM vehicles WHERE "DealerId" = '00000000-0000-0000-0000-000000000001';
