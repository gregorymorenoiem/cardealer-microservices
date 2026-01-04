-- Script SQL para crear 10 Sedanes para el dealer de República Dominicana
-- Dealer ID: 89de6284-dba0-46f1-8c6b-f2de21ec113b
-- User ID: f5fb27f0-ee1f-4817-ac96-832c35271f62

-- Insertar 10 Sedanes
INSERT INTO vehicles (
    "Id", "DealerId", "Title", "Description", "Price", "Currency", "Status", "VIN",
    "Make", "Model", "Year", "Condition", "BodyStyle", "Transmission", "FuelType",
    "Mileage", "ExteriorColor", "InteriorColor", "Doors", "Seats",
    "City", "State", "Country", "Latitude", "Longitude",
    "SellerId", "SellerName", "SellerPhone", "SellerEmail",
    "IsFeatured", "ViewCount", "CreatedAt", "UpdatedAt", "PublishedAt"
) VALUES
-- 1. BMW Serie 3 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid, 
 'BMW Serie 3 2024', 'Premium sedan with advanced technology, leather interior, and sport package. Impeccable condition.', 
 48500, 'USD', 'active', '1HGBH41JXMN100001',
 'BMW', 'Serie 3', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 127, 'Black', 'Beige', 4, 5,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 127, NOW(), NOW(), NOW()),

-- 2. Mercedes-Benz C-Class 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Mercedes-Benz C-Class 2024', 'Luxury and performance combined. AMG styling package with panoramic sunroof.',
 52000, 'USD', 'active', '1HGBH41JXMN100002',
 'Mercedes-Benz', 'C-Class', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 98, 'Silver', 'Black', 4, 5,
 'Santiago', 'Santiago', 'DO', 19.4517, -70.6970,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 98, NOW(), NOW(), NOW()),

-- 3. Audi A4 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Audi A4 2024', 'Sophisticated design with Quattro AWD. Virtual cockpit and premium sound system.',
 45900, 'USD', 'active', '1HGBH41JXMN100003',
 'Audi', 'A4', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 156, 'White', 'Gray', 4, 5,
 'Punta Cana', 'La Altagracia', 'DO', 18.5601, -68.3725,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 156, NOW(), NOW(), NOW()),

-- 4. Tesla Model 3 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Tesla Model 3 2024', 'Electric performance sedan with autopilot. Long range battery, zero emissions.',
 42990, 'USD', 'active', '1HGBH41JXMN100004',
 'Tesla', 'Model 3', 2024, 'new', 'Sedan', 'Automatic', 'Electric',
 0, 'Blue', 'White', 4, 5,
 'Santo Domingo', 'Distrito Nacional', 'DO', 18.4861, -69.9312,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 234, NOW(), NOW(), NOW()),

-- 5. Lexus ES 350 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Lexus ES 350 2024', 'Japanese luxury and reliability. Mark Levinson audio system included.',
 44500, 'USD', 'active', '1HGBH41JXMN100005',
 'Lexus', 'ES 350', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 89, 'Pearl White', 'Tan', 4, 5,
 'La Romana', 'La Romana', 'DO', 18.4273, -68.9728,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 89, NOW(), NOW(), NOW()),

-- 6. Honda Accord 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Honda Accord 2024', 'Spacious and efficient family sedan. Honda Sensing safety suite included.',
 32500, 'USD', 'active', '1HGBH41JXMN100006',
 'Honda', 'Accord', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 312, 'Gray', 'Black', 4, 5,
 'San Pedro de Macoris', 'San Pedro de Macoris', 'DO', 18.4539, -69.3086,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 312, NOW(), NOW(), NOW()),

-- 7. Toyota Camry 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Toyota Camry 2024', 'Hybrid efficiency meets reliability. Toyota Safety Sense 2.5+ standard.',
 29800, 'USD', 'active', '1HGBH41JXMN100007',
 'Toyota', 'Camry', 2024, 'new', 'Sedan', 'Automatic', 'Hybrid',
 445, 'Red', 'Black', 4, 5,
 'Puerto Plata', 'Puerto Plata', 'DO', 19.7934, -70.6884,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 445, NOW(), NOW(), NOW()),

-- 8. Mazda 6 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Mazda 6 2024', 'Sporty styling with premium features. Bose sound system and sunroof.',
 28500, 'USD', 'active', '1HGBH41JXMN100008',
 'Mazda', '6', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 178, 'Deep Blue', 'Parchment', 4, 5,
 'Higuey', 'La Altagracia', 'DO', 18.6150, -68.7078,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 178, NOW(), NOW(), NOW()),

-- 9. Hyundai Sonata 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Hyundai Sonata 2024', 'Value-packed midsize sedan with bold design. 10-year warranty included.',
 27900, 'USD', 'active', '1HGBH41JXMN100009',
 'Hyundai', 'Sonata', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 201, 'Silver', 'Gray', 4, 5,
 'Barahona', 'Barahona', 'DO', 18.2083, -71.1003,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 201, NOW(), NOW(), NOW()),

-- 10. Kia K5 2024
(gen_random_uuid(), '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid,
 'Kia K5 2024', 'Bold design with tech features. Apple CarPlay, Android Auto, and wireless charging.',
 26800, 'USD', 'active', '1HGBH41JXMN100010',
 'Kia', 'K5', 2024, 'new', 'Sedan', 'Automatic', 'Gasoline',
 167, 'White', 'Black', 4, 5,
 'Samana', 'Samana', 'DO', 19.2056, -69.3361,
 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid, 'AutoVentas RD', '+1-809-555-0123', 'autoventas.rd@cardealer.com',
 true, 167, NOW(), NOW(), NOW());

-- Verificar creación
SELECT "Id", "Title", "Make", "Model", "Price", "BodyStyle" FROM vehicles WHERE "DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid AND "BodyStyle" = 'Sedan';
