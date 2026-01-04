-- =================================================
-- SEED DATA: Vehículos para VehiclesSaleService
-- =================================================
-- Ejecutar en la base de datos vehiclessaleservice
-- Conexión: Host=localhost;Port=25432;Database=vehiclessaleservice;Username=postgres;Password=password
-- =================================================

-- Limpiar datos existentes (opcional - comentar si no quieres perder datos)
-- DELETE FROM "VehicleImages";
-- DELETE FROM "Vehicles";

-- Dealer y Seller por defecto para demo
DO $$
DECLARE
    default_dealer_id UUID := '00000000-0000-0000-0000-000000000001'::uuid;
    default_seller_id UUID := '00000000-0000-0000-0000-000000000002'::uuid;
    v_id UUID;
    img_id UUID;
BEGIN

-- =================================================
-- Vehículo 1: Tesla Model 3
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Tesla Model 3',
    'Tesla Model 3 Long Range con Autopilot, audio premium y todas las actualizaciones recientes.',
    42990, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Tesla', 'Model 3', 2023, 0, -- New
    0, -- Sedan
    1, -- Automatic
    2, -- Electric
    2, -- AWD
    0, -- Car
    5200, 0, -- Miles
    'Pearl White', 'Black', 'Dual Motor Electric', 480,
    'Los Angeles', 'CA', 'USA',
    '["Autopilot", "Premium Audio", "Glass Roof", "Navigation", "Heated Seats"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 2: BMW 3 Series
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2022 BMW 3 Series',
    'BMW 330i con M Sport Package, asientos de cuero y tecnología avanzada.',
    38500, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'BMW', '3 Series', 2022, 1, -- Used
    0, -- Sedan
    1, -- Automatic
    0, -- Gasoline
    1, -- RWD
    0, -- Car
    15000, 0, -- Miles
    'Alpine White', 'Cognac', '2.0L Turbo I4', 255,
    'Miami', 'FL', 'USA',
    '["M Sport Package", "Leather Seats", "Navigation", "Sunroof", "Apple CarPlay"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 3: Toyota Camry
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2021 Toyota Camry',
    'Toyota Camry SE certificado con garantía extendida y historial de servicio completo.',
    24900, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Toyota', 'Camry', 2021, 2, -- Certified Pre-Owned
    0, -- Sedan
    1, -- Automatic
    0, -- Gasoline
    0, -- FWD
    0, -- Car
    28000, 0, -- Miles
    'Midnight Black', 'Gray', '2.5L I4', 203,
    'Houston', 'TX', 'USA',
    '["Toyota Safety Sense", "Lane Departure", "Adaptive Cruise", "Apple CarPlay"]',
    false, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 4: Ford Mustang
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Ford Mustang',
    'Ford Mustang GT con Performance Package, escape activo y frenos Brembo.',
    55990, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Ford', 'Mustang', 2023, 0, -- New
    3, -- Coupe
    0, -- Manual
    0, -- Gasoline
    1, -- RWD
    0, -- Car
    3500, 0, -- Miles
    'Race Red', 'Black', '5.0L V8', 450,
    'Dallas', 'TX', 'USA',
    '["Performance Package", "Active Exhaust", "Brembo Brakes", "Track Apps"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1544636331-e26879cd4d9b?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 5: Honda Accord
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Honda Accord',
    'Honda Accord Hybrid Touring con navegación y asientos de cuero.',
    32500, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Honda', 'Accord', 2023, 1, -- Used
    0, -- Sedan
    2, -- CVT
    3, -- Hybrid
    0, -- FWD
    0, -- Car
    8000, 0, -- Miles
    'Lunar Silver', 'Black', '2.0L Hybrid', 212,
    'Chicago', 'IL', 'USA',
    '["Hybrid System", "Leather Seats", "Honda Sensing", "Sunroof", "Navigation"]',
    false, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1619767886558-efdc259cde1a?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 6: Jeep Grand Cherokee
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2022 Jeep Grand Cherokee',
    'Jeep Grand Cherokee Limited con sistema 4x4 Quadra-Trac II.',
    48700, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Jeep', 'Grand Cherokee', 2022, 1, -- Used
    1, -- SUV
    1, -- Automatic
    0, -- Gasoline
    3, -- 4WD
    0, -- Car
    22000, 0, -- Miles
    'Diamond Black', 'Black', '3.6L V6', 293,
    'Denver', 'CO', 'USA',
    '["Quadra-Trac II", "Leather Seats", "Panoramic Sunroof", "Uconnect 5"]',
    false, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 7: Audi A4
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Audi A4',
    'Audi A4 Premium Plus con Quattro AWD y Virtual Cockpit.',
    45900, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Audi', 'A4', 2023, 0, -- New
    0, -- Sedan
    1, -- Automatic
    0, -- Gasoline
    2, -- AWD
    0, -- Car
    5000, 0, -- Miles
    'Glacier White', 'Brown', '2.0L Turbo I4', 261,
    'San Diego', 'CA', 'USA',
    '["Quattro AWD", "Virtual Cockpit", "S Line Package", "B&O Audio"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 8: Mercedes-Benz GLC
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Mercedes-Benz GLC',
    'Mercedes-Benz GLC 300 4MATIC con AMG Line y Burmester audio.',
    52900, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Mercedes-Benz', 'GLC', 2023, 1, -- Used
    1, -- SUV
    1, -- Automatic
    0, -- Gasoline
    2, -- AWD
    0, -- Car
    12000, 0, -- Miles
    'Obsidian Black', 'Macchiato Beige', '2.0L Turbo I4', 258,
    'Seattle', 'WA', 'USA',
    '["AMG Line", "4MATIC AWD", "Burmester Audio", "MBUX Infotainment"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1563720223185-11003d516935?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 9: Lexus ES 350
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2022 Lexus ES 350',
    'Lexus ES 350 F Sport con suspensión deportiva adaptativa.',
    44200, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Lexus', 'ES 350', 2022, 2, -- Certified Pre-Owned
    0, -- Sedan
    1, -- Automatic
    0, -- Gasoline
    0, -- FWD
    0, -- Car
    18000, 0, -- Miles
    'Atomic Silver', 'Black', '3.5L V6', 302,
    'Phoenix', 'AZ', 'USA',
    '["F Sport Package", "Adaptive Suspension", "Mark Levinson Audio", "Sunroof"]',
    false, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 10: Porsche Cayenne
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2022 Porsche Cayenne',
    'Porsche Cayenne con paquete Sport Chrono y suspensión neumática.',
    89500, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Porsche', 'Cayenne', 2022, 1, -- Used
    1, -- SUV
    1, -- Automatic
    0, -- Gasoline
    2, -- AWD
    0, -- Car
    15000, 0, -- Miles
    'Carrara White', 'Black', '3.0L Turbo V6', 335,
    'Las Vegas', 'NV', 'USA',
    '["Sport Chrono", "Air Suspension", "Bose Audio", "Panoramic Roof"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 11: Ford F-150
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Ford F-150',
    'Ford F-150 Lariat con motor EcoBoost y capacidad de remolque de 14,000 lbs.',
    55900, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Ford', 'F-150', 2023, 0, -- New
    2, -- Truck
    1, -- Automatic
    0, -- Gasoline
    3, -- 4WD
    1, -- Truck
    5000, 0, -- Miles
    'Iconic Silver', 'Black', '3.5L EcoBoost V6', 400,
    'Houston', 'TX', 'USA',
    '["EcoBoost", "Pro Power Onboard", "Max Tow Package", "B&O Audio"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 12: Toyota Tacoma
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Toyota Tacoma',
    'Toyota Tacoma TRD Pro con suspensión FOX y capacidad off-road.',
    42500, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Toyota', 'Tacoma', 2023, 1, -- Used
    2, -- Truck
    1, -- Automatic
    0, -- Gasoline
    3, -- 4WD
    1, -- Truck
    8000, 0, -- Miles
    'Army Green', 'Black', '3.5L V6', 278,
    'Seattle', 'WA', 'USA',
    '["TRD Pro", "FOX Suspension", "Crawl Control", "Multi-Terrain Select"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1612544448445-b8232cff3b6c?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 13: Rivian R1T
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Rivian R1T',
    'Rivian R1T Adventure Package con rango de 314 millas.',
    73000, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Rivian', 'R1T', 2023, 0, -- New
    2, -- Truck
    1, -- Automatic
    2, -- Electric
    2, -- AWD
    1, -- Truck
    5000, 0, -- Miles
    'Rivian Blue', 'Ocean Coast', 'Quad Motor Electric', 835,
    'Portland', 'OR', 'USA',
    '["Quad Motor", "Air Suspension", "Camp Kitchen", "Adventure Gear Tunnel"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1583121274602-3e2820c69888?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 14: Porsche 911 Carrera
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2022 Porsche 911 Carrera',
    'Porsche 911 Carrera con Sport Chrono Package y PASM.',
    115000, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Porsche', '911 Carrera', 2022, 1, -- Used
    3, -- Coupe
    1, -- Automatic
    0, -- Gasoline
    1, -- RWD
    0, -- Car
    8000, 0, -- Miles
    'Guards Red', 'Black', '3.0L Twin-Turbo Flat-6', 379,
    'Beverly Hills', 'CA', 'USA',
    '["Sport Chrono", "PASM Suspension", "Bose Audio", "Sport Exhaust"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1596468138838-8c50b8d9b2c5?w=800&h=600&fit=crop', 0, 0, true, NOW());

-- =================================================
-- Vehículo 15: Land Rover Range Rover Sport
-- =================================================
v_id := gen_random_uuid();
INSERT INTO "Vehicles" (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerId", "SellerName", "Make", "Model", "Year", "Condition",
    "BodyStyle", "Transmission", "FuelType", "DriveType", "VehicleType",
    "Mileage", "MileageUnit", "ExteriorColor", "InteriorColor", "EngineSize",
    "Horsepower", "City", "State", "Country", "FeaturesJson", "IsFeatured",
    "CreatedAt", "UpdatedAt"
) VALUES (
    v_id, default_dealer_id, '2023 Land Rover Range Rover Sport',
    'Range Rover Sport HSE Dynamic con Terrain Response 2.',
    95000, 'USD', 1, default_seller_id, 'CarDealer Demo',
    'Land Rover', 'Range Rover Sport', 2023, 0, -- New
    1, -- SUV
    1, -- Automatic
    0, -- Gasoline
    2, -- AWD
    0, -- Car
    8000, 0, -- Miles
    'Santorini Black', 'Ebony', '3.0L Turbo I6', 395,
    'San Francisco', 'CA', 'USA',
    '["Terrain Response 2", "Air Suspension", "Meridian Audio", "Panoramic Roof"]',
    true, NOW(), NOW()
);

img_id := gen_random_uuid();
INSERT INTO "VehicleImages" ("Id", "DealerId", "VehicleId", "Url", "ImageType", "SortOrder", "IsPrimary", "CreatedAt")
VALUES (img_id, default_dealer_id, v_id, 'https://images.unsplash.com/photo-1551830820-330a71b99659?w=800&h=600&fit=crop', 0, 0, true, NOW());

RAISE NOTICE '✅ Seed completado: 15 vehículos insertados';

END $$;

-- Verificar inserción
SELECT COUNT(*) as total_vehicles FROM "Vehicles";
SELECT COUNT(*) as total_images FROM "VehicleImages";
