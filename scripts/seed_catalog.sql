-- ==============================================
-- Vehicle Catalog Seed for VehiclesSaleService
-- Adapted for EF Core table structure
-- ==============================================

-- ========================================
-- MARCAS (20 marcas populares)
-- ========================================

INSERT INTO vehicle_makes ("Id", "Name", "Slug", "Country", "IsPopular", "SortOrder", "IsActive", "CreatedAt", "UpdatedAt") VALUES 
('da221d7f-6417-4415-a076-74810c5daa83', 'Toyota', 'toyota', 'Japan', true, 1, true, NOW(), NOW()),
('468e04a7-440d-45c7-8efe-bb6049f13599', 'Honda', 'honda', 'Japan', true, 2, true, NOW(), NOW()),
('55dd59a5-c6c3-4338-ae58-c2f78f5da17d', 'Ford', 'ford', 'USA', true, 3, true, NOW(), NOW()),
('ce3935b0-0bcf-4e6a-9a2a-1adfa97197da', 'Chevrolet', 'chevrolet', 'USA', true, 4, true, NOW(), NOW()),
('743acb02-e065-4384-82f9-ee53a9e1e126', 'Nissan', 'nissan', 'Japan', true, 5, true, NOW(), NOW()),
('31f137e9-c9b9-45ca-80c2-9d31324aae07', 'Jeep', 'jeep', 'USA', true, 6, true, NOW(), NOW()),
('5c46089d-3620-4c15-a20d-8b324381111a', 'RAM', 'ram', 'USA', true, 7, true, NOW(), NOW()),
('88de7383-e1c8-4faa-b5e7-3b821efd3f7f', 'GMC', 'gmc', 'USA', true, 8, true, NOW(), NOW()),
('feaee75e-b4bb-4c51-969e-e1dba3bb7a74', 'Hyundai', 'hyundai', 'South Korea', true, 9, true, NOW(), NOW()),
('62f1435f-5c02-42e8-8159-37c7c3c8a16c', 'Kia', 'kia', 'South Korea', true, 10, true, NOW(), NOW()),
('530d14a3-f5f9-4534-8fb7-06332345888a', 'Tesla', 'tesla', 'USA', true, 11, true, NOW(), NOW()),
('9ce379c1-b6b8-4127-8b43-cb6e68cb80e8', 'BMW', 'bmw', 'Germany', true, 12, true, NOW(), NOW()),
('d1da17ce-e560-466c-bf5f-73bdb1880252', 'Mercedes-Benz', 'mercedes-benz', 'Germany', true, 13, true, NOW(), NOW()),
('8033d53d-d91f-4b3e-8c25-4e69efd01b79', 'Audi', 'audi', 'Germany', true, 14, true, NOW(), NOW()),
('e03f3d94-3c48-453e-9a83-b6fe8b77feda', 'Lexus', 'lexus', 'Japan', true, 15, true, NOW(), NOW()),
('91ed8235-c5cf-4b4e-b577-d0e154b8bcc6', 'Subaru', 'subaru', 'Japan', false, 16, true, NOW(), NOW()),
('2e1b6b2d-f9ad-41ac-aa9b-6d3a83451c51', 'Volkswagen', 'volkswagen', 'Germany', false, 17, true, NOW(), NOW()),
('08865ad9-83fd-4f19-9656-f1c8a573b424', 'Mazda', 'mazda', 'Japan', false, 18, true, NOW(), NOW()),
('e4644d85-b19c-47e5-a766-99ae15e1f6e2', 'Dodge', 'dodge', 'USA', false, 19, true, NOW(), NOW()),
('a06b1575-2450-4a7f-94d0-e69454440fde', 'Porsche', 'porsche', 'Germany', false, 20, true, NOW(), NOW())
ON CONFLICT ("Name") DO UPDATE SET "UpdatedAt" = NOW();

-- ========================================
-- MODELOS (modelos populares por marca)
-- ========================================

INSERT INTO vehicle_models ("Id", "MakeId", "Name", "Slug", "VehicleType", "DefaultBodyStyle", "StartYear", "IsPopular", "IsActive", "CreatedAt", "UpdatedAt") VALUES 
-- Toyota
('121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'da221d7f-6417-4415-a076-74810c5daa83', 'Camry', 'camry', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('4d50e2a5-f838-4cae-b786-6debd38b66e9', 'da221d7f-6417-4415-a076-74810c5daa83', 'Corolla', 'corolla', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('d0668813-5a6c-4a32-a3d4-e4c062aa02e5', 'da221d7f-6417-4415-a076-74810c5daa83', 'RAV4', 'rav4', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('7ae46567-e01e-4733-908c-86bf139b685d', 'da221d7f-6417-4415-a076-74810c5daa83', 'Tacoma', 'tacoma', 'Car', 'Pickup', 2020, true, true, NOW(), NOW()),
('e8c5f4d3-2b1a-4c6e-9f8d-7a5b3c2e1d0f', 'da221d7f-6417-4415-a076-74810c5daa83', 'Highlander', 'highlander', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
-- Honda
('a2d98baf-5761-46e3-b06b-2043dad4bc93', '468e04a7-440d-45c7-8efe-bb6049f13599', 'Civic', 'civic', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('9f0b1464-0c6f-4ddb-b236-3478c0ad4d21', '468e04a7-440d-45c7-8efe-bb6049f13599', 'Accord', 'accord', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('4cac6d25-5800-4753-bb74-d566aeca1983', '468e04a7-440d-45c7-8efe-bb6049f13599', 'CR-V', 'cr-v', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('b7d8e9f0-1c2a-3b4c-5d6e-7f8a9b0c1d2e', '468e04a7-440d-45c7-8efe-bb6049f13599', 'Pilot', 'pilot', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
-- Ford
('8eaa8a6d-5542-45e2-80cf-a6dd593e40af', '55dd59a5-c6c3-4338-ae58-c2f78f5da17d', 'F-150', 'f-150', 'Car', 'Pickup', 2020, true, true, NOW(), NOW()),
('428a5c06-a9c4-43c7-bbcb-1d272fd93d3e', '55dd59a5-c6c3-4338-ae58-c2f78f5da17d', 'Mustang', 'mustang', 'Car', 'Coupe', 2020, true, true, NOW(), NOW()),
('c3e4f5a6-7b8c-9d0e-1f2a-3b4c5d6e7f8a', '55dd59a5-c6c3-4338-ae58-c2f78f5da17d', 'Explorer', 'explorer', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('d4f5a6b7-8c9d-0e1f-2a3b-4c5d6e7f8a9b', '55dd59a5-c6c3-4338-ae58-c2f78f5da17d', 'Bronco', 'bronco', 'Car', 'SUV', 2021, true, true, NOW(), NOW()),
-- Tesla
('ec17d2c2-5b65-4f8d-ba0e-d3c403b2c143', '530d14a3-f5f9-4534-8fb7-06332345888a', 'Model 3', 'model-3', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('cac12afd-a83a-4cba-b95a-32ad42dacf90', '530d14a3-f5f9-4534-8fb7-06332345888a', 'Model Y', 'model-y', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('f5a6b7c8-9d0e-1f2a-3b4c-5d6e7f8a9b0c', '530d14a3-f5f9-4534-8fb7-06332345888a', 'Model S', 'model-s', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
-- Chevrolet
('562a02dd-4a39-4e81-a288-5a513902a11d', 'ce3935b0-0bcf-4e6a-9a2a-1adfa97197da', 'Silverado 1500', 'silverado-1500', 'Car', 'Pickup', 2020, true, true, NOW(), NOW()),
('a6b7c8d9-0e1f-2a3b-4c5d-6e7f8a9b0c1d', 'ce3935b0-0bcf-4e6a-9a2a-1adfa97197da', 'Tahoe', 'tahoe', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('b7c8d9e0-1f2a-3b4c-5d6e-7f8a9b0c1d2e', 'ce3935b0-0bcf-4e6a-9a2a-1adfa97197da', 'Camaro', 'camaro', 'Car', 'Coupe', 2020, true, true, NOW(), NOW()),
-- BMW
('e66419bb-c023-4e6f-9973-f166f2d6f228', '9ce379c1-b6b8-4127-8b43-cb6e68cb80e8', '3 Series', '3-series', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('c8d9e0f1-2a3b-4c5d-6e7f-8a9b0c1d2e3f', '9ce379c1-b6b8-4127-8b43-cb6e68cb80e8', 'X5', 'x5', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('d9e0f1a2-3b4c-5d6e-7f8a-9b0c1d2e3f4a', '9ce379c1-b6b8-4127-8b43-cb6e68cb80e8', 'M4', 'm4', 'Car', 'Coupe', 2020, true, true, NOW(), NOW()),
-- Mercedes-Benz
('db10f458-7ed0-4937-9ae8-a6ce9762caa2', 'd1da17ce-e560-466c-bf5f-73bdb1880252', 'C-Class', 'c-class', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('e0f1a2b3-4c5d-6e7f-8a9b-0c1d2e3f4a5b', 'd1da17ce-e560-466c-bf5f-73bdb1880252', 'GLE', 'gle', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('f1a2b3c4-5d6e-7f8a-9b0c-1d2e3f4a5b6c', 'd1da17ce-e560-466c-bf5f-73bdb1880252', 'AMG GT', 'amg-gt', 'Car', 'Coupe', 2020, true, true, NOW(), NOW()),
-- Audi
('a2b3c4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d', '8033d53d-d91f-4b3e-8c25-4e69efd01b79', 'A4', 'a4', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
('b3c4d5e6-7f8a-9b0c-1d2e-3f4a5b6c7d8e', '8033d53d-d91f-4b3e-8c25-4e69efd01b79', 'Q5', 'q5', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('c4d5e6f7-8a9b-0c1d-2e3f-4a5b6c7d8e9f', '8033d53d-d91f-4b3e-8c25-4e69efd01b79', 'RS7', 'rs7', 'Car', 'Sedan', 2020, true, true, NOW(), NOW()),
-- Jeep
('d5e6f7a8-9b0c-1d2e-3f4a-5b6c7d8e9f0a', '31f137e9-c9b9-45ca-80c2-9d31324aae07', 'Wrangler', 'wrangler', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
('e6f7a8b9-0c1d-2e3f-4a5b-6c7d8e9f0a1b', '31f137e9-c9b9-45ca-80c2-9d31324aae07', 'Grand Cherokee', 'grand-cherokee', 'Car', 'SUV', 2020, true, true, NOW(), NOW()),
-- Porsche
('f7a8b9c0-1d2e-3f4a-5b6c-7d8e9f0a1b2c', 'a06b1575-2450-4a7f-94d0-e69454440fde', '911', '911', 'Car', 'Coupe', 2020, true, true, NOW(), NOW()),
('a8b9c0d1-2e3f-4a5b-6c7d-8e9f0a1b2c3d', 'a06b1575-2450-4a7f-94d0-e69454440fde', 'Cayenne', 'cayenne', 'Car', 'SUV', 2020, true, true, NOW(), NOW())
ON CONFLICT ("MakeId", "Name") DO UPDATE SET "UpdatedAt" = NOW();

-- ========================================
-- TRIMS (con specs para auto-fill)
-- ========================================

INSERT INTO vehicle_trims ("Id", "ModelId", "Name", "Slug", "Year", "EngineSize", "Horsepower", "Torque", "FuelType", "Transmission", "DriveType", "MpgCity", "MpgHighway", "BaseMSRP", "IsActive", "CreatedAt", "UpdatedAt") VALUES 
-- Toyota Camry 2024
('4a336e1c-90dd-46f2-8d2a-7cefcb4d2786', '121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'LE', 'le', 2024, '2.5L I4', 203, 184, 'Gasoline', 'Automatic', 'FWD', 28, 39, 28400.00, true, NOW(), NOW()),
('34c4bb70-5123-46cb-a72a-8d04ee13498a', '121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'SE', 'se', 2024, '2.5L I4', 203, 184, 'Gasoline', 'Automatic', 'FWD', 28, 39, 29495.00, true, NOW(), NOW()),
('65a66c1b-12ca-4e74-a3b5-f5100ad725b5', '121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'XLE', 'xle', 2024, '2.5L I4', 203, 184, 'Gasoline', 'Automatic', 'FWD', 28, 39, 31170.00, true, NOW(), NOW()),
('825fa17f-a443-4726-a683-c0a5ac351ed3', '121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'XSE', 'xse', 2024, '2.5L I4', 203, 184, 'Gasoline', 'Automatic', 'FWD', 28, 39, 32920.00, true, NOW(), NOW()),
-- Toyota Camry 2023
('d5a65d01-eb26-4824-863a-5717bfbcdb45', '121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'LE', 'le', 2023, '2.5L I4', 203, 184, 'Gasoline', 'Automatic', 'FWD', 28, 39, 26420.00, true, NOW(), NOW()),
('b8037a1b-3024-498a-93d2-52a4f93a9f56', '121f7dec-89cf-48dd-90d2-c027a8d6ffce', 'SE', 'se', 2023, '2.5L I4', 203, 184, 'Gasoline', 'Automatic', 'FWD', 28, 39, 27760.00, true, NOW(), NOW()),
-- Honda Civic 2024
('10e76964-9e5c-44b1-9eeb-55d6b27ce990', 'a2d98baf-5761-46e3-b06b-2043dad4bc93', 'LX', 'lx', 2024, '2.0L I4', 158, 138, 'Gasoline', 'CVT', 'FWD', 31, 40, 24950.00, true, NOW(), NOW()),
('ff7300f3-2c65-47ea-ba06-ac95d9778cd9', 'a2d98baf-5761-46e3-b06b-2043dad4bc93', 'Sport', 'sport', 2024, '2.0L I4', 158, 138, 'Gasoline', 'CVT', 'FWD', 31, 40, 26600.00, true, NOW(), NOW()),
('1dcaa042-19f8-47d7-965c-da5b8b794a5b', 'a2d98baf-5761-46e3-b06b-2043dad4bc93', 'EX', 'ex', 2024, '1.5L Turbo I4', 180, 177, 'Gasoline', 'CVT', 'FWD', 33, 42, 28300.00, true, NOW(), NOW()),
('07fcbd65-cd94-4c6c-998d-bc8497b1d9aa', 'a2d98baf-5761-46e3-b06b-2043dad4bc93', 'Touring', 'touring', 2024, '1.5L Turbo I4', 180, 177, 'Gasoline', 'CVT', 'FWD', 33, 42, 30900.00, true, NOW(), NOW()),
-- Ford F-150 2024
('2d020481-39b9-40c3-abc8-df34c954c022', '8eaa8a6d-5542-45e2-80cf-a6dd593e40af', 'XL', 'xl', 2024, '3.3L V6', 290, 265, 'Gasoline', 'Automatic', 'RWD', 18, 25, 36495.00, true, NOW(), NOW()),
('ebc3a976-627c-4ad1-be17-770c067c0e93', '8eaa8a6d-5542-45e2-80cf-a6dd593e40af', 'XLT', 'xlt', 2024, '2.7L EcoBoost V6', 325, 400, 'Gasoline', 'Automatic', '4WD', 19, 24, 45495.00, true, NOW(), NOW()),
('c19905f8-3e82-40b2-8abd-2aca17b59a6b', '8eaa8a6d-5542-45e2-80cf-a6dd593e40af', 'Lariat', 'lariat', 2024, '3.5L EcoBoost V6', 400, 500, 'Gasoline', 'Automatic', '4WD', 18, 23, 56495.00, true, NOW(), NOW()),
-- Ford Mustang 2024
('3f8d579e-f97b-4efc-9b41-17ec6629f7a4', '428a5c06-a9c4-43c7-bbcb-1d272fd93d3e', 'EcoBoost', 'ecoboost', 2024, '2.3L EcoBoost I4', 315, 350, 'Gasoline', 'Manual', 'RWD', 21, 32, 32515.00, true, NOW(), NOW()),
('6dd44ad5-a219-4cff-b9d9-1e2eb67d8317', '428a5c06-a9c4-43c7-bbcb-1d272fd93d3e', 'GT', 'gt', 2024, '5.0L V8', 480, 415, 'Gasoline', 'Manual', 'RWD', 17, 26, 44090.00, true, NOW(), NOW()),
('d2de9f64-bfe1-40bc-8100-5c3122d7378f', '428a5c06-a9c4-43c7-bbcb-1d272fd93d3e', 'Dark Horse', 'dark-horse', 2024, '5.0L V8', 500, 418, 'Gasoline', 'Manual', 'RWD', 17, 24, 60775.00, true, NOW(), NOW()),
-- Tesla Model 3 2024
('295073f5-5e5b-4d5a-9781-22e32882c008', 'ec17d2c2-5b65-4f8d-ba0e-d3c403b2c143', 'Standard Range Plus', 'standard-range-plus', 2024, 'Electric', 271, 317, 'Electric', 'Single-Speed', 'RWD', 132, 132, 40240.00, true, NOW(), NOW()),
('ed271c2e-fe45-4100-89af-50917ddb8ff3', 'ec17d2c2-5b65-4f8d-ba0e-d3c403b2c143', 'Long Range', 'long-range', 2024, 'Dual Motor', 366, 376, 'Electric', 'Single-Speed', 'AWD', 131, 131, 47240.00, true, NOW(), NOW()),
('be0161bd-8d26-4b8c-854a-efa6ce1da6cd', 'ec17d2c2-5b65-4f8d-ba0e-d3c403b2c143', 'Performance', 'performance', 2024, 'Dual Motor', 455, 480, 'Electric', 'Single-Speed', 'AWD', 120, 120, 53240.00, true, NOW(), NOW()),
-- Tesla Model Y 2024
('3a9a00ae-fa94-4196-a7c2-d26bddaddf37', 'cac12afd-a83a-4cba-b95a-32ad42dacf90', 'Long Range', 'long-range', 2024, 'Dual Motor', 350, 376, 'Electric', 'Single-Speed', 'AWD', 123, 117, 47990.00, true, NOW(), NOW()),
('d545359b-c47b-4e77-88ee-51bc23c8d136', 'cac12afd-a83a-4cba-b95a-32ad42dacf90', 'Performance', 'performance', 2024, 'Dual Motor', 456, 497, 'Electric', 'Single-Speed', 'AWD', 115, 106, 52990.00, true, NOW(), NOW()),
-- Chevrolet Silverado 1500 2024
('55ce36c5-2595-4aba-8b82-901ec75da572', '562a02dd-4a39-4e81-a288-5a513902a11d', 'Work Truck', 'work-truck', 2024, '2.7L Turbo I4', 310, 430, 'Gasoline', 'Automatic', 'RWD', 20, 24, 37495.00, true, NOW(), NOW()),
('7c188253-cf55-4ef1-aa63-cf9101900718', '562a02dd-4a39-4e81-a288-5a513902a11d', 'LT', 'lt', 2024, '5.3L V8', 355, 383, 'Gasoline', 'Automatic', '4WD', 16, 21, 50795.00, true, NOW(), NOW()),
('b83006db-7725-4557-9c84-4ca082d30b2a', '562a02dd-4a39-4e81-a288-5a513902a11d', 'High Country', 'high-country', 2024, '6.2L V8', 420, 460, 'Gasoline', 'Automatic', '4WD', 15, 20, 66495.00, true, NOW(), NOW()),
-- BMW 3 Series 2024
('6ee1b306-d2f9-4f7a-bab3-8dd5ea187a79', 'e66419bb-c023-4e6f-9973-f166f2d6f228', '330i', '330i', 2024, '2.0L Turbo I4', 255, 295, 'Gasoline', 'Automatic', 'RWD', 26, 36, 46245.00, true, NOW(), NOW()),
('732ec6cb-c556-4f69-8c39-da187e311cab', 'e66419bb-c023-4e6f-9973-f166f2d6f228', '330i xDrive', '330i-xdrive', 2024, '2.0L Turbo I4', 255, 295, 'Gasoline', 'Automatic', 'AWD', 25, 34, 48245.00, true, NOW(), NOW()),
('0666c109-424d-4867-b2b3-c6d19e2ca9ce', 'e66419bb-c023-4e6f-9973-f166f2d6f228', 'M340i', 'm340i', 2024, '3.0L Turbo I6', 382, 369, 'Gasoline', 'Automatic', 'AWD', 23, 32, 58095.00, true, NOW(), NOW()),
-- Mercedes-Benz C-Class 2024
('925e55a8-8aa7-4ce4-ac39-21ad31fbaf52', 'db10f458-7ed0-4937-9ae8-a6ce9762caa2', 'C 300', 'c-300', 2024, '2.0L Turbo I4', 255, 295, 'Gasoline', 'Automatic', 'RWD', 24, 33, 47800.00, true, NOW(), NOW()),
('d7285aad-d70f-4b6b-ae44-229f74dfe2b7', 'db10f458-7ed0-4937-9ae8-a6ce9762caa2', 'C 300 4MATIC', 'c-300-4matic', 2024, '2.0L Turbo I4', 255, 295, 'Gasoline', 'Automatic', 'AWD', 23, 31, 49800.00, true, NOW(), NOW()),
-- Audi A4 2024
('67ff6b6f-21ca-4cea-9f78-633c75e9a17c', 'a2b3c4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d', '40 TFSI', '40-tfsi', 2024, '2.0L Turbo I4', 201, 236, 'Gasoline', 'Automatic', 'FWD', 26, 35, 41500.00, true, NOW(), NOW()),
('2023d102-3fb0-499e-926d-01f4ad1d1853', 'a2b3c4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d', '45 TFSI quattro', '45-tfsi-quattro', 2024, '2.0L Turbo I4', 261, 273, 'Gasoline', 'Automatic', 'AWD', 24, 32, 45700.00, true, NOW(), NOW()),
-- Jeep Wrangler 2024
('a9b0c1d2-3e4f-5a6b-7c8d-9e0f1a2b3c4d', 'd5e6f7a8-9b0c-1d2e-3f4a-5b6c7d8e9f0a', 'Sport', 'sport', 2024, '3.6L V6', 285, 260, 'Gasoline', 'Manual', '4WD', 17, 25, 33490.00, true, NOW(), NOW()),
('b0c1d2e3-4f5a-6b7c-8d9e-0f1a2b3c4d5e', 'd5e6f7a8-9b0c-1d2e-3f4a-5b6c7d8e9f0a', 'Sahara', 'sahara', 2024, '3.6L V6', 285, 260, 'Gasoline', 'Automatic', '4WD', 17, 22, 44195.00, true, NOW(), NOW()),
('c1d2e3f4-5a6b-7c8d-9e0f-1a2b3c4d5e6f', 'd5e6f7a8-9b0c-1d2e-3f4a-5b6c7d8e9f0a', 'Rubicon', 'rubicon', 2024, '3.6L V6', 285, 260, 'Gasoline', 'Automatic', '4WD', 17, 22, 49295.00, true, NOW(), NOW()),
-- Porsche 911 2024
('d2e3f4a5-6b7c-8d9e-0f1a-2b3c4d5e6f7a', 'f7a8b9c0-1d2e-3f4a-5b6c-7d8e9f0a1b2c', 'Carrera', 'carrera', 2024, '3.0L Twin-Turbo H6', 379, 331, 'Gasoline', 'PDK', 'RWD', 18, 24, 116950.00, true, NOW(), NOW()),
('e3f4a5b6-7c8d-9e0f-1a2b-3c4d5e6f7a8b', 'f7a8b9c0-1d2e-3f4a-5b6c-7d8e9f0a1b2c', 'Carrera S', 'carrera-s', 2024, '3.0L Twin-Turbo H6', 443, 390, 'Gasoline', 'PDK', 'RWD', 17, 24, 133950.00, true, NOW(), NOW()),
('f4a5b6c7-8d9e-0f1a-2b3c-4d5e6f7a8b9c', 'f7a8b9c0-1d2e-3f4a-5b6c-7d8e9f0a1b2c', 'Turbo', 'turbo', 2024, '3.7L Twin-Turbo H6', 572, 553, 'Gasoline', 'PDK', 'AWD', 15, 20, 175950.00, true, NOW(), NOW())
ON CONFLICT ("ModelId", "Year", "Name") DO UPDATE SET "BaseMSRP" = EXCLUDED."BaseMSRP", "UpdatedAt" = NOW();

-- ========================================
-- CATEGORIES
-- ========================================

INSERT INTO categories ("Id", "Name", "Slug", "Description", "ParentId", "IsActive", "SortOrder", "DealerId", "CreatedAt", "UpdatedAt") VALUES 
('1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d', 'Sedans', 'sedans', 'Four-door passenger vehicles', NULL, true, 1, '00000000-0000-0000-0000-000000000000', NOW(), NOW()),
('2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e', 'SUVs', 'suvs', 'Sport Utility Vehicles', NULL, true, 2, '00000000-0000-0000-0000-000000000000', NOW(), NOW()),
('3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f', 'Trucks', 'trucks', 'Pickup trucks and commercial vehicles', NULL, true, 3, '00000000-0000-0000-0000-000000000000', NOW(), NOW()),
('4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a', 'Sports Cars', 'sports-cars', 'High-performance sports cars', NULL, true, 4, '00000000-0000-0000-0000-000000000000', NOW(), NOW()),
('5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b', 'Electric', 'electric', 'Electric and Hybrid vehicles', NULL, true, 5, '00000000-0000-0000-0000-000000000000', NOW(), NOW()),
('6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c', 'Luxury', 'luxury', 'Luxury and premium vehicles', NULL, true, 6, '00000000-0000-0000-0000-000000000000', NOW(), NOW())
ON CONFLICT ("Id") DO UPDATE SET "UpdatedAt" = NOW();
