-- =====================================================
-- SCRIPT: Insert Mock Vehicles into VehiclesSaleService Database
-- Date: January 12, 2026
-- Description: Inserta los vehículos que estaban en mockVehicles.ts
-- Run with: docker exec -i postgres_db psql -U postgres -d vehiclessaleservice < insert_mock_vehicles.sql
-- =====================================================

-- Verificar que estamos en la base de datos correcta
\echo '======================================'
\echo 'Insertando vehículos mock en la base de datos...'
\echo '======================================'

-- =====================================================
-- VEHÍCULO 1: Tesla Model 3 2023
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000001'::uuid,
    NULL,
    NULL,
    '2023 Tesla Model 3 Long Range AWD',
    'Brand new Tesla Model 3 with full self-driving capability. This electric sedan combines performance with efficiency, featuring a minimalist interior and cutting-edge technology.',
    42990, 'USD', 'Active',
    'Tesla Los Angeles', 'Dealer', '+1 (555) 123-4567', true, 4.8,
    '5YJ3E1EB4KF123456', 'Tesla', 'Model 3', 'Long Range', 2023,
    'Car', 'Sedan', 4, 5,
    'Electric', 'Dual Motor Electric', 480, 'Automatic', 'AWD',
    5200, 'Miles', 'New',
    'Pearl White', 'Black',
    132, 126,
    'Los Angeles', 'CA', 'USA',
    true, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 2: BMW 3 Series 2022
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000002'::uuid,
    NULL,
    NULL,
    '2022 BMW 3 Series 330i',
    'Excellent condition BMW 3 Series with low mileage. Well-maintained with full service history. Perfect blend of luxury and performance.',
    38500, 'USD', 'Active',
    'Miami Luxury Motors', 'Dealer', '+1 (555) 234-5678', true, 4.6,
    'WBA8B9C59HK987654', 'BMW', '3 Series', '330i', 2022,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '2.0L Turbo I4', 255, 'Automatic', 'RWD',
    12000, 'Miles', 'Used',
    'Alpine White', 'Black',
    26, 36,
    'Miami', 'FL', 'USA',
    true, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 3: Toyota Camry Hybrid 2021
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000003'::uuid,
    NULL,
    NULL,
    '2021 Toyota Camry Hybrid XLE',
    'Reliable and fuel-efficient Toyota Camry Hybrid. Perfect for daily commuting with excellent gas mileage and proven reliability.',
    24900, 'USD', 'Active',
    'John Smith', 'Individual', '+1 (555) 345-6789', true, 4.9,
    '4T1G11AK5MU123456', 'Toyota', 'Camry', 'XLE Hybrid', 2021,
    'Car', 'Sedan', 4, 5,
    'Hybrid', '2.5L Hybrid I4', 208, 'Automatic', 'FWD',
    28000, 'Miles', 'Used',
    'Celestial Silver', 'Ash',
    51, 53,
    'Houston', 'TX', 'USA',
    false, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 4: Ford Mustang GT 2023
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000004'::uuid,
    NULL,
    NULL,
    '2023 Ford Mustang GT Premium',
    'Powerful Ford Mustang GT with the legendary 5.0L V8 engine. Perfect for enthusiasts who love the thrill of driving.',
    35990, 'USD', 'Active',
    'Dallas Performance Cars', 'Dealer', '+1 (555) 456-7890', true, 4.7,
    '1FA6P8CF3L5123456', 'Ford', 'Mustang', 'GT Premium', 2023,
    'Car', 'Coupe', 2, 4,
    'Gasoline', '5.0L V8', 450, 'Manual', 'RWD',
    8500, 'Miles', 'Used',
    'Race Red', 'Black',
    15, 24,
    'Dallas', 'TX', 'USA',
    true, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 5: Honda Accord 2022
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000005'::uuid,
    NULL,
    NULL,
    '2022 Honda Accord Sport 1.5T',
    'Honda Certified Pre-Owned Accord with comprehensive warranty. Spacious, comfortable, and loaded with safety features.',
    27500, 'USD', 'Active',
    'Chicago Honda', 'Dealer', '+1 (555) 567-8901', true, 4.5,
    '1HGCV1F39NA123456', 'Honda', 'Accord', 'Sport 1.5T', 2022,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '1.5L Turbo I4', 192, 'Automatic', 'FWD',
    15000, 'Miles', 'CertifiedPreOwned',
    'Platinum White', 'Gray',
    30, 38,
    'Chicago', 'IL', 'USA',
    false, true,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 6: Audi A4 2023
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000006'::uuid,
    NULL,
    NULL,
    '2023 Audi A4 Premium Plus Quattro',
    'Luxurious Audi A4 with Quattro all-wheel drive. Premium materials, advanced technology, and exceptional handling.',
    41200, 'USD', 'Active',
    'Manhattan Audi', 'Dealer', '+1 (555) 678-9012', true, 4.8,
    'WAUFFAFL5PN123456', 'Audi', 'A4', 'Premium Plus', 2023,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '2.0L Turbo I4', 261, 'Automatic', 'AWD',
    6800, 'Miles', 'Used',
    'Mythos Black', 'Black',
    24, 33,
    'New York', 'NY', 'USA',
    true, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 7: Mercedes-Benz C-Class 2022
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000007'::uuid,
    NULL,
    NULL,
    '2022 Mercedes-Benz C300 AMG Line',
    'Mercedes-Benz Certified C-Class with AMG styling. Luxury, technology, and performance in a refined package.',
    43900, 'USD', 'Active',
    'Seattle Mercedes-Benz', 'Dealer', '+1 (555) 789-0123', true, 4.9,
    'W1KWJ8DB8NA123456', 'Mercedes-Benz', 'C-Class', 'C300 AMG Line', 2022,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '2.0L Turbo I4', 255, 'Automatic', 'RWD',
    11000, 'Miles', 'CertifiedPreOwned',
    'Selenite Grey', 'Black',
    23, 33,
    'Seattle', 'WA', 'USA',
    true, true,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 8: Chevrolet Silverado 1500 2021
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000008'::uuid,
    NULL,
    NULL,
    '2021 Chevrolet Silverado 1500 LT Z71',
    'Capable Chevrolet Silverado 1500 with 4WD and towing package. Perfect for work or adventure in the Rockies.',
    39500, 'USD', 'Active',
    'Denver Chevrolet', 'Dealer', '+1 (555) 890-1234', true, 4.4,
    '1GCUYGEL5MZ123456', 'Chevrolet', 'Silverado 1500', 'LT Z71', 2021,
    'Truck', 'Pickup', 4, 6,
    'Gasoline', '5.3L V8', 355, 'Automatic', 'FourWD',
    22000, 'Miles', 'Used',
    'Summit White', 'Jet Black',
    16, 20,
    'Denver', 'CO', 'USA',
    false, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 9: Mazda CX-5 2023 (Turbo)
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000009'::uuid,
    NULL,
    NULL,
    '2023 Mazda CX-5 Signature Turbo AWD',
    'Nearly new Mazda CX-5 with premium features. Sporty handling meets practical SUV utility with upscale materials.',
    32900, 'USD', 'Active',
    'Portland Mazda', 'Dealer', '+1 (555) 901-2345', true, 4.7,
    'JM3KFBDM3P0123456', 'Mazda', 'CX-5', 'Signature Turbo', 2023,
    'SUV', 'SUV', 4, 5,
    'Gasoline', '2.5L Turbo I4', 256, 'Automatic', 'AWD',
    4500, 'Miles', 'Used',
    'Soul Red Crystal', 'Black',
    22, 27,
    'Portland', 'OR', 'USA',
    false, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 10: Volkswagen Jetta 2022
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000010'::uuid,
    NULL,
    NULL,
    '2022 Volkswagen Jetta SE',
    'Fuel-efficient VW Jetta with modern technology. Great for city driving with surprising interior space.',
    22900, 'USD', 'Active',
    'Sarah Johnson', 'Individual', '+1 (555) 012-3456', true, 4.8,
    '3VWC57BU8NM123456', 'Volkswagen', 'Jetta', 'SE', 2022,
    'Car', 'Sedan', 4, 5,
    'Gasoline', '1.4L Turbo I4', 147, 'Automatic', 'FWD',
    18000, 'Miles', 'Used',
    'Pure White', 'Titan Black',
    30, 41,
    'Austin', 'TX', 'USA',
    false, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- VEHÍCULO 11: Mazda CX-5 2023 (Standard)
-- =====================================================
INSERT INTO "Vehicles" (
    "Id", "DealerId", "SellerId", "Title", "Description", "Price", "Currency", "Status",
    "SellerName", "SellerType", "SellerPhone", "SellerVerified", "SellerRating",
    "VIN", "Make", "Model", "Trim", "Year",
    "VehicleType", "BodyStyle", "Doors", "Seats",
    "FuelType", "EngineSize", "Horsepower", "Transmission", "DriveType",
    "Mileage", "MileageUnit", "Condition",
    "ExteriorColor", "InteriorColor",
    "MpgCity", "MpgHighway",
    "City", "State", "Country",
    "IsFeatured", "IsCertified",
    "CreatedAt", "UpdatedAt"
) VALUES (
    'b0000000-0000-0000-0000-000000000011'::uuid,
    NULL,
    NULL,
    '2023 Mazda CX-5 Premium Plus AWD',
    'Stunning Mazda CX-5 in Soul Red Crystal. Premium features, AWD, and exceptional fuel economy. Meticulously maintained with full service history.',
    31500, 'USD', 'Active',
    'Premium Auto Group', 'Dealer', '+1 (555) 123-4567', true, 4.6,
    'JM3KFBDM5P0123456', 'Mazda', 'CX-5', 'Premium Plus', 2023,
    'SUV', 'SUV', 4, 5,
    'Gasoline', '2.5L I4', 187, 'Automatic', 'AWD',
    8500, 'Miles', 'Used',
    'Soul Red Crystal', 'Black Leather',
    25, 31,
    'Portland', 'OR', 'USA',
    false, false,
    NOW(), NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- INSERTAR IMÁGENES PARA CADA VEHÍCULO
-- =====================================================

-- Imágenes para Tesla Model 3
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000001'::uuid, 'https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=800&h=600&fit=crop', 0, true, NOW()),
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000001'::uuid, 'https://images.unsplash.com/photo-1561580125-028ee3bd62eb?w=800&h=600&fit=crop', 1, false, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para BMW 3 Series
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000002'::uuid, 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=800&h=600&fit=crop', 0, true, NOW()),
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000002'::uuid, 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800&h=600&fit=crop', 1, false, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Toyota Camry
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000003'::uuid, 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Ford Mustang
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000004'::uuid, 'https://images.unsplash.com/photo-1547744152-14d985cb937f?w=800&h=600&fit=crop', 0, true, NOW()),
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000004'::uuid, 'https://images.unsplash.com/photo-1494905998402-395d579af36f?w=800&h=600&fit=crop', 1, false, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Honda Accord
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000005'::uuid, 'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Audi A4
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000006'::uuid, 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=800&h=600&fit=crop', 0, true, NOW()),
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000006'::uuid, 'https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=800&h=600&fit=crop', 1, false, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Mercedes-Benz C-Class
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000007'::uuid, 'https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Chevrolet Silverado
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000008'::uuid, 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Mazda CX-5 (Turbo)
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000009'::uuid, 'https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Volkswagen Jetta
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000010'::uuid, 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- Imágenes para Mazda CX-5 (Standard)
INSERT INTO "VehicleImages" ("Id", "VehicleId", "ImageUrl", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
    (gen_random_uuid(), 'b0000000-0000-0000-0000-000000000011'::uuid, 'https://images.unsplash.com/photo-1617814076367-b759c7d7e738?w=800&h=600&fit=crop', 0, true, NOW())
ON CONFLICT DO NOTHING;

-- =====================================================
-- ASIGNAR VEHÍCULOS A SECCIONES DEL HOMEPAGE
-- =====================================================

-- Primero obtener IDs de las secciones
DO $$
DECLARE
    carousel_id uuid;
    sedanes_id uuid;
    suv_id uuid;
    destacados_id uuid;
    deportivos_id uuid;
    lujo_id uuid;
BEGIN
    -- Obtener IDs de secciones
    SELECT "Id" INTO carousel_id FROM "HomepageSectionConfigs" WHERE "Slug" = 'carousel-principal' LIMIT 1;
    SELECT "Id" INTO sedanes_id FROM "HomepageSectionConfigs" WHERE "Slug" = 'sedanes' LIMIT 1;
    SELECT "Id" INTO suv_id FROM "HomepageSectionConfigs" WHERE "Slug" = 'suvs' LIMIT 1;
    SELECT "Id" INTO destacados_id FROM "HomepageSectionConfigs" WHERE "Slug" = 'destacados' LIMIT 1;
    SELECT "Id" INTO deportivos_id FROM "HomepageSectionConfigs" WHERE "Slug" = 'deportivos' LIMIT 1;
    SELECT "Id" INTO lujo_id FROM "HomepageSectionConfigs" WHERE "Slug" = 'lujo' LIMIT 1;
    
    -- Carousel Principal (Tesla, BMW, Audi, Mercedes, Mustang)
    IF carousel_id IS NOT NULL THEN
        INSERT INTO "VehicleHomepageSections" ("VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
        VALUES 
            ('b0000000-0000-0000-0000-000000000001'::uuid, carousel_id, 1, true, NOW()),
            ('b0000000-0000-0000-0000-000000000002'::uuid, carousel_id, 2, true, NOW()),
            ('b0000000-0000-0000-0000-000000000006'::uuid, carousel_id, 3, true, NOW()),
            ('b0000000-0000-0000-0000-000000000007'::uuid, carousel_id, 4, true, NOW()),
            ('b0000000-0000-0000-0000-000000000004'::uuid, carousel_id, 5, true, NOW())
        ON CONFLICT DO NOTHING;
    END IF;
    
    -- Sedanes (Tesla, BMW, Toyota, Honda, Audi, Mercedes, VW)
    IF sedanes_id IS NOT NULL THEN
        INSERT INTO "VehicleHomepageSections" ("VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
        VALUES 
            ('b0000000-0000-0000-0000-000000000001'::uuid, sedanes_id, 1, false, NOW()),
            ('b0000000-0000-0000-0000-000000000002'::uuid, sedanes_id, 2, false, NOW()),
            ('b0000000-0000-0000-0000-000000000003'::uuid, sedanes_id, 3, false, NOW()),
            ('b0000000-0000-0000-0000-000000000005'::uuid, sedanes_id, 4, false, NOW()),
            ('b0000000-0000-0000-0000-000000000006'::uuid, sedanes_id, 5, false, NOW()),
            ('b0000000-0000-0000-0000-000000000007'::uuid, sedanes_id, 6, false, NOW()),
            ('b0000000-0000-0000-0000-000000000010'::uuid, sedanes_id, 7, false, NOW())
        ON CONFLICT DO NOTHING;
    END IF;
    
    -- SUVs (Mazda CX-5 x2, Silverado)
    IF suv_id IS NOT NULL THEN
        INSERT INTO "VehicleHomepageSections" ("VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
        VALUES 
            ('b0000000-0000-0000-0000-000000000009'::uuid, suv_id, 1, false, NOW()),
            ('b0000000-0000-0000-0000-000000000011'::uuid, suv_id, 2, false, NOW()),
            ('b0000000-0000-0000-0000-000000000008'::uuid, suv_id, 3, false, NOW())
        ON CONFLICT DO NOTHING;
    END IF;
    
    -- Destacados (Featured vehicles: Tesla, BMW, Mustang, Audi, Mercedes)
    IF destacados_id IS NOT NULL THEN
        INSERT INTO "VehicleHomepageSections" ("VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
        VALUES 
            ('b0000000-0000-0000-0000-000000000001'::uuid, destacados_id, 1, true, NOW()),
            ('b0000000-0000-0000-0000-000000000002'::uuid, destacados_id, 2, true, NOW()),
            ('b0000000-0000-0000-0000-000000000004'::uuid, destacados_id, 3, true, NOW()),
            ('b0000000-0000-0000-0000-000000000006'::uuid, destacados_id, 4, true, NOW()),
            ('b0000000-0000-0000-0000-000000000007'::uuid, destacados_id, 5, true, NOW())
        ON CONFLICT DO NOTHING;
    END IF;
    
    -- Deportivos (Mustang, Tesla)
    IF deportivos_id IS NOT NULL THEN
        INSERT INTO "VehicleHomepageSections" ("VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
        VALUES 
            ('b0000000-0000-0000-0000-000000000004'::uuid, deportivos_id, 1, true, NOW()),
            ('b0000000-0000-0000-0000-000000000001'::uuid, deportivos_id, 2, false, NOW())
        ON CONFLICT DO NOTHING;
    END IF;
    
    -- Lujo (BMW, Audi, Mercedes)
    IF lujo_id IS NOT NULL THEN
        INSERT INTO "VehicleHomepageSections" ("VehicleId", "HomepageSectionConfigId", "SortOrder", "IsPinned", "CreatedAt")
        VALUES 
            ('b0000000-0000-0000-0000-000000000002'::uuid, lujo_id, 1, false, NOW()),
            ('b0000000-0000-0000-0000-000000000006'::uuid, lujo_id, 2, false, NOW()),
            ('b0000000-0000-0000-0000-000000000007'::uuid, lujo_id, 3, false, NOW())
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- =====================================================
-- VERIFICACIÓN FINAL
-- =====================================================
\echo ''
\echo '======================================'
\echo 'Verificación de datos insertados:'
\echo '======================================'

SELECT 'Vehículos insertados:' as info, COUNT(*) as total FROM "Vehicles" WHERE "Id" LIKE 'b0000000-0000-0000-0000-00000000000%';
SELECT 'Imágenes insertadas:' as info, COUNT(*) as total FROM "VehicleImages" WHERE "VehicleId" IN (SELECT "Id" FROM "Vehicles" WHERE "Id"::text LIKE 'b0000000-0000-0000-0000-00000000000%');

\echo ''
\echo 'Lista de vehículos:'
SELECT "Id", "Make", "Model", "Year", "Price", "Status" FROM "Vehicles" WHERE "Id"::text LIKE 'b0000000-0000-0000-0000-00000000000%' ORDER BY "CreatedAt";

\echo ''
\echo '======================================'
\echo '¡Script completado exitosamente!'
\echo '======================================'
