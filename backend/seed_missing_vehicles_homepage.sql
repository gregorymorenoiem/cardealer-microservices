-- ============================================================
-- SCRIPT: Completar vehículos faltantes en secciones del homepage
-- Base de datos: vehiclessaleservice
-- Fecha: 2026-03-14
-- Propósito: Agrega vehículos con los tipos de carrocería y combustible
--            que faltan en las secciones del homepage de OKLA.
-- Secciones afectadas:
--   • Hatchbacks  (+2 → mínimo 3 total)
--   • Camionetas  (+1 → mínimo 2 total)
--   • Deportivos  (+2 → mínimo 2 total, eran 0)
--   • Convertibles (+2 → mínimo 2 total, eran 0)
--   • Vans        (+2 → mínimo 2 total, eran 0)
--   • Minivans    (+2 → mínimo 2 total, eran 0)
--   • Híbridos    (+1 → mínimo 2 total)
--   • Eléctricos  (+1 → mínimo 2 total)
--   • Premium     (+3 de alto valor para PremiumSpot rotation)
--
-- NOTA: Los valores de BodyStyle/FuelType son strings (HasConversion<string>())
-- Ejecución: docker exec -i postgres_db psql -U postgres -d vehiclessaleservice < seed_missing_vehicles_homepage.sql
-- ============================================================

\echo '======================================'
\echo 'Insertando vehículos faltantes del homepage...'
\echo '======================================'

\c vehiclessaleservice

-- ============================================================
-- HATCHBACKS (necesita 2 más; ya existe al menos 1 de seed previo)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Hatchback 1: Hyundai i20
(
    'c0000001-0000-4000-8000-000000000101'::uuid,
    '2023 Hyundai i20 Active', 'Hatchback compacto ideal para la ciudad. Bajo consumo, fácil de estacionar y con todas las comodidades modernas.',
    1150000, 'DOP', 'Active',
    'AutoCenter DR', 'Dealer', '+1(809)555-0101', true, 4.5,
    'KMHCU41AAPU100001', 'Hyundai', 'i20', 'Active', 2023,
    'Car', 'Hatchback', 4, 5,
    'Gasoline', '1.4L I4', 100, 'Automatic', 'FWD',
    8300, 'Km', 'New',
    'Phantom Black', 'Black',
    'La Romana', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
),
-- Hatchback 2: Kia Rio LX
(
    'c0000001-0000-4000-8000-000000000102'::uuid,
    '2024 Kia Rio LX', 'Hatchback moderno y económico. Perfecto para la movilidad urbana de República Dominicana con bajo costo de mantenimiento.',
    1200000, 'DOP', 'Active',
    'Kia Santo Domingo', 'Dealer', '+1(809)555-0102', true, 4.6,
    'KNADM5E14P6100002', 'Kia', 'Rio', 'LX', 2024,
    'Car', 'Hatchback', 4, 5,
    'Gasoline', '1.6L I4', 120, 'Automatic', 'FWD',
    5200, 'Km', 'New',
    'Aurora Black', 'Charcoal',
    'Distrito Nacional', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- CAMIONETAS / PICKUP (necesita 1 más)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
(
    'c0000002-0000-4000-8000-000000000201'::uuid,
    '2023 Ford Ranger Wildtrak 4x4', 'Pickup de trabajo y aventura. Motor EcoBlue turbodiesel, doble cabina, conectividad total y acabado premium.',
    2950000, 'DOP', 'Active',
    'Ford Santo Domingo', 'Dealer', '+1(809)555-0201', true, 4.7,
    'MNAA2G06XP3200001', 'Ford', 'Ranger', 'Wildtrak 4x4', 2023,
    'Truck', 'Pickup', 4, 5,
    'Diesel', '2.0L EcoBlue Diesel', 170, 'Automatic', 'FourWD',
    18000, 'Km', 'Used',
    'Magnetic Gray', 'Ebony',
    'Santo Domingo', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- DEPORTIVOS / SPORTS CAR (necesita 2, eran 0)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Deportivo 1: Ford Mustang GT
(
    'c0000003-0000-4000-8000-000000000301'::uuid,
    '2023 Ford Mustang GT Premium', 'El icónico deportivo americano. Motor V8 5.0L de 450 hp, equipamiento premium y sonido inconfundible. Lista para la pista.',
    4200000, 'DOP', 'Active',
    'PerformanceRD', 'Dealer', '+1(809)555-0301', true, 4.9,
    '1FA6P8CF8P5300001', 'Ford', 'Mustang', 'GT Premium', 2023,
    'Car', 'SportsCar', 2, 4,
    'Gasoline', '5.0L V8', 450, 'Manual', 'RWD',
    8900, 'Km', 'Used',
    'Race Red', 'Ebony',
    'Distrito Nacional', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1612825173281-9a193378527e?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=75","sortOrder":2,"isPrimary":false}]',
    NOW(), NOW()
),
-- Deportivo 2: Chevrolet Camaro SS
(
    'c0000003-0000-4000-8000-000000000302'::uuid,
    '2023 Chevrolet Camaro SS', 'Deportivo de alto rendimiento con motor V8 6.2L de 455 hp. Frenos Brembo, diferencial electrónico, sonido de legado.',
    4800000, 'DOP', 'Active',
    'PerformanceRD', 'Dealer', '+1(809)555-0302', true, 4.8,
    '1G1FF1R77P0300002', 'Chevrolet', 'Camaro', 'SS', 2023,
    'Car', 'SportsCar', 2, 4,
    'Gasoline', '6.2L V8', 455, 'Automatic', 'RWD',
    5500, 'Km', 'Used',
    'Rally Green', 'Jet Black',
    'Punta Cana', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1542362567-b07e54358753?w=800&q=75","sortOrder":2,"isPrimary":false}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- CONVERTIBLES (necesita 2, eran 0)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Convertible 1: Mazda MX-5 Miata
(
    'c0000004-0000-4000-8000-000000000401'::uuid,
    '2023 Mazda MX-5 Miata Grand Touring', 'El convertible más alabado del mundo. Manejo preciso, techo retráctil de tela y diseño deportivo atemporal. Perfecto para el clima dominicano.',
    2950000, 'DOP', 'Active',
    'Mazda Dominicana', 'Dealer', '+1(809)555-0401', true, 4.9,
    'JM1NDBA79P0400001', 'Mazda', 'MX-5 Miata', 'Grand Touring', 2023,
    'Car', 'Convertible', 2, 2,
    'Gasoline', '2.0L I4', 181, 'Manual', 'RWD',
    7800, 'Km', 'Used',
    'Soul Red Crystal', 'Black',
    'Boca Chica', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
),
-- Convertible 2: Ford Mustang Convertible
(
    'c0000004-0000-4000-8000-000000000402'::uuid,
    '2024 Ford Mustang Convertible EcoBoost', 'Experiencia cielo abierto con el emblemático poni americano. Motor turbo 2.3L de 330 hp. Techo power soft-top.',
    3800000, 'DOP', 'Active',
    'Ford Distrito Nacional', 'Dealer', '+1(809)555-0402', true, 4.7,
    '1FATP8FF3P0400002', 'Ford', 'Mustang', 'Convertible EcoBoost', 2024,
    'Car', 'Convertible', 2, 4,
    'Gasoline', '2.3L Turbo I4', 330, 'Automatic', 'RWD',
    4100, 'Km', 'New',
    'Oxford White', 'Ebony',
    'Distrito Nacional', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- VANS (necesita 2, eran 0)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Van 1: Toyota HiAce
(
    'c0000005-0000-4000-8000-000000000501'::uuid,
    '2023 Toyota HiAce Van GL', 'La van más confiable de la isla. Motor diesel 2.8L duradero, alta capacidad de carga, perfecta para negocios y turismo.',
    2650000, 'DOP', 'Active',
    'Toyota Comercial DR', 'Dealer', '+1(809)555-0501', true, 4.8,
    'JTFHX22P2P0500001', 'Toyota', 'HiAce', 'Van GL', 2023,
    'Van', 'Van', 4, 14,
    'Diesel', '2.8L Diesel I4', 150, 'Manual', 'RWD',
    32000, 'Km', 'Used',
    'Beige', 'Gray',
    'Santo Domingo Norte', 'RD', 'DO',
    false, false,
    '[{"url":"https://images.unsplash.com/photo-1533473359331-2969f3c6b787?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
),
-- Van 2: Nissan NV350 Urvan
(
    'c0000005-0000-4000-8000-000000000502'::uuid,
    '2024 Nissan NV350 Urvan Premium', 'Van de pasajeros con 15 asientos, techo alto, A/C frontal y trasero. Ideal para servicios de transporte y turismo en RD.',
    2850000, 'DOP', 'Active',
    'Nissan Comercial RD', 'Dealer', '+1(809)555-0502', true, 4.6,
    'JN8AG2KRXP3500002', 'Nissan', 'NV350', 'Urvan Premium', 2024,
    'Van', 'Van', 4, 15,
    'Diesel', '2.5L Diesel I4', 163, 'Manual', 'RWD',
    14700, 'Km', 'New',
    'White', 'Gray',
    'Distrito Nacional', 'RD', 'DO',
    false, false,
    '[{"url":"https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- MINIVANS (necesita 2, eran 0)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Minivan 1: Honda Odyssey EX-L
(
    'c0000006-0000-4000-8000-000000000601'::uuid,
    '2023 Honda Odyssey EX-L', 'La minivan familiar por excelencia. 8 pasajeros, Magic Seat, pantallas traseras, WiFi integrado y motor V6 de 280 hp.',
    3200000, 'DOP', 'Active',
    'Honda Familiar DR', 'Dealer', '+1(809)555-0601', true, 4.8,
    '5FNRL6H88PB600001', 'Honda', 'Odyssey', 'EX-L', 2023,
    'Car', 'Minivan', 4, 8,
    'Gasoline', '3.5L V6', 280, 'Automatic', 'FWD',
    16400, 'Km', 'Used',
    'Lunar Silver', 'Gray',
    'Distrito Nacional', 'RD', 'DO',
    false, false,
    '[{"url":"https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
),
-- Minivan 2: Kia Carnival EX
(
    'c0000006-0000-4000-8000-000000000602'::uuid,
    '2024 Kia Carnival EX', 'La nueva generación de minivan con diseño tipo MPV de lujo. 8 pasajeros, asientos reclinables individuales, sistema de sonido premium Bose.',
    3100000, 'DOP', 'Active',
    'Kia Familiar DR', 'Dealer', '+1(809)555-0602', true, 4.7,
    'KNDNB4H38P6600002', 'Kia', 'Carnival', 'EX', 2024,
    'Car', 'Minivan', 4, 8,
    'Gasoline', '3.5L V6', 290, 'Automatic', 'FWD',
    5800, 'Km', 'New',
    'Snow White Pearl', 'Black',
    'Santiago', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&q=75","sortOrder":1,"isPrimary":true}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- HÍBRIDOS (necesita 1 más; ya existe 1 de seed previo)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
(
    'c0000007-0000-4000-8000-000000000701'::uuid,
    '2024 Toyota RAV4 Hybrid XSE', 'SUV híbrida con 219 hp combinados, tracción total eléctrica y 38 mpg promedio. La opción más eficiente del mercado dominicano.',
    3200000, 'DOP', 'Active',
    'Toyota DR Oficial', 'Dealer', '+1(809)555-0701', true, 4.9,
    '2T3RWRFV1PW700001', 'Toyota', 'RAV4', 'Hybrid XSE', 2024,
    'SUV', 'SUV', 4, 5,
    'Hybrid', '2.5L Hybrid I4', 219, 'eCVT', 'AWD',
    7200, 'Km', 'New',
    'Blueprint', 'Black',
    'Santiago', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800&q=75","sortOrder":2,"isPrimary":false}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- ELÉCTRICOS (necesita 1 más; ya existe 1 de seed previo)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
(
    'c0000008-0000-4000-8000-000000000801'::uuid,
    '2024 Tesla Model Y Standard Range', 'SUV eléctrica con autonomía de 480 km, Autopilot incluido, carga de 250 kW y pantalla de 15.4". El futuro ya llegó a RD.',
    4650000, 'DOP', 'Active',
    'Tesla RD Oficial', 'Dealer', '+1(809)555-0801', true, 5.0,
    '5YJYGDEE9PF800001', 'Tesla', 'Model Y', 'Standard Range', 2024,
    'SUV', 'SUV', 4, 5,
    'Electric', 'Single Motor Electric', 283, 'Automatic', 'RWD',
    8200, 'Km', 'New',
    'Pearl White', 'Black',
    'Santo Domingo', 'RD', 'DO',
    true, false,
    '[{"url":"https://images.unsplash.com/photo-1556189250-72ba954cfc2b?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1542362567-b07e54358753?w=800&q=75","sortOrder":2,"isPrimary":false}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- PREMIUM: 3 vehículos de alto valor para enriquecer PremiumSpot
-- (La rotación selecciona por price_desc — vehículos de alto precio
--  aparecen naturalmente en la sección Premium con IsFeatured=true)
-- ============================================================

INSERT INTO "Vehicles" (
    "Id", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "Images",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Premium 1: BMW X7
(
    'c0000009-0000-4000-8000-000000000901'::uuid,
    '2024 BMW X7 xDrive40i M Sport', 'SUV premium de 7 pasajeros con motor inline-6 turbo de 375 hp. Paquete M Sport completo, techo panorámico Sky Lounge, masajes de asientos.',
    7800000, 'DOP', 'Active',
    'BMW Dominicana Premier', 'Dealer', '+1(809)555-0901', true, 5.0,
    '5UXCW2C07P9900001', 'BMW', 'X7', 'xDrive40i M Sport', 2024,
    'SUV', 'SUV', 4, 7,
    'Gasoline', '3.0L TwinPower Turbo I6', 375, 'Automatic', 'AWD',
    4200, 'Km', 'New',
    'Carbon Black Metallic', 'Ivory White',
    'Distrito Nacional', 'RD', 'DO',
    true, true,
    '[{"url":"https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&q=75","sortOrder":2,"isPrimary":false},{"url":"https://images.unsplash.com/photo-1616422285623-13ff0162193c?w=800&q=75","sortOrder":3,"isPrimary":false}]',
    NOW(), NOW()
),
-- Premium 2: Land Rover Defender 110
(
    'c0000009-0000-4000-8000-000000000902'::uuid,
    '2024 Land Rover Defender 110 X', '4x4 de lujo con capacidad todoterreno extrema. Motor P400 de 400 hp, aire suspensión adaptativa, blindaje opcional. Equipado para la aventura dominicana.',
    8900000, 'DOP', 'Active',
    'Land Rover Casa de Campo', 'Dealer', '+1(809)555-0902', true, 5.0,
    'SALE1EE22P1900002', 'Land Rover', 'Defender', '110 X', 2024,
    'SUV', 'SUV', 4, 5,
    'Gasoline', '3.0L Turbo I6', 400, 'Automatic', 'AWD',
    2800, 'Km', 'New',
    'Gondwana Stone', 'Light Cloud',
    'Punta Cana', 'RD', 'DO',
    true, true,
    '[{"url":"https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1533473359331-2969f3c6b787?w=800&q=75","sortOrder":2,"isPrimary":false},{"url":"https://images.unsplash.com/photo-1583121274602-3e2820c69888?w=800&q=75","sortOrder":3,"isPrimary":false}]',
    NOW(), NOW()
),
-- Premium 3: Maserati Ghibli Modena
(
    'c0000009-0000-4000-8000-000000000903'::uuid,
    '2023 Maserati Ghibli Modena Q4', 'El sedán italiano por excelencia. Motor V6 biturbo de 350 hp, tracción total Q4, interior en cuero Pieno Fiore. Un símbolo de status en RD.',
    9200000, 'DOP', 'Active',
    'Maserati Casa de Campo', 'Dealer', '+1(809)555-0903', true, 5.0,
    'ZAM57YTL7P1900003', 'Maserati', 'Ghibli', 'Modena Q4', 2023,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '3.0L V6 Biturbo', 350, 'Automatic', 'AWD',
    6100, 'Km', 'Used',
    'Nero Ribelle', 'Cuoio',
    'Casa de Campo', 'RD', 'DO',
    true, true,
    '[{"url":"https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&q=75","sortOrder":1,"isPrimary":true},{"url":"https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&q=75","sortOrder":2,"isPrimary":false},{"url":"https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800&q=75","sortOrder":3,"isPrimary":false}]',
    NOW(), NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- VERIFICACIÓN POST-INSERCIÓN
-- ============================================================

\echo ''
\echo '====== Resumen de vehículos por BodyStyle ======'
SELECT "BodyStyle", COUNT(*) as total, STRING_AGG("Make" || ' ' || "Model", ', ' ORDER BY "Make") as modelos
FROM "Vehicles"
WHERE "Status" = 'Active'
GROUP BY "BodyStyle"
ORDER BY "BodyStyle";

\echo ''
\echo '====== Resumen de vehículos por FuelType ======'
SELECT "FuelType", COUNT(*) as total
FROM "Vehicles"
WHERE "Status" = 'Active'
GROUP BY "FuelType"
ORDER BY "FuelType";

\echo ''
\echo '====== Vehículos Premium (IsFeatured=true, precio > 3M DOP) ======'
SELECT "Make", "Model", "Year", "Price", "BodyStyle", "FuelType"
FROM "Vehicles"
WHERE "IsFeatured" = true AND "Price" > 3000000 AND "Status" = 'Active'
ORDER BY "Price" DESC
LIMIT 20;

\echo ''
\echo '✅ Script completado exitosamente.'
