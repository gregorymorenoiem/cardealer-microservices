-- =================================================
-- MIGRACIÓN: Crear tablas de vehículos para VehiclesSaleService
-- =================================================
-- Ejecutar en: vehiclessaleservice database
-- Este script crea las tablas vehicles, vehicle_images, vehicle_makes, vehicle_models, vehicle_trims
-- =================================================

-- 1. Crear tabla vehicle_makes (marcas)
CREATE TABLE IF NOT EXISTS vehicle_makes (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" varchar(100) NOT NULL,
    "Country" varchar(100),
    "LogoUrl" varchar(500),
    "IsActive" boolean NOT NULL DEFAULT true,
    "SortOrder" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW()
);

-- 2. Crear tabla vehicle_models (modelos)
CREATE TABLE IF NOT EXISTS vehicle_models (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "MakeId" uuid NOT NULL REFERENCES vehicle_makes("Id") ON DELETE CASCADE,
    "Name" varchar(100) NOT NULL,
    "Category" varchar(50),
    "IsActive" boolean NOT NULL DEFAULT true,
    "SortOrder" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW()
);

-- 3. Crear tabla vehicle_trims (versiones)
CREATE TABLE IF NOT EXISTS vehicle_trims (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "ModelId" uuid NOT NULL REFERENCES vehicle_models("Id") ON DELETE CASCADE,
    "Name" varchar(100) NOT NULL,
    "Year" integer NOT NULL,
    "EngineSize" varchar(50),
    "Horsepower" integer,
    "Transmission" varchar(50),
    "DriveType" varchar(50),
    "FuelType" varchar(50),
    "MSRP" numeric(18,2),
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW()
);

-- 4. Crear tabla vehicles (vehículos en venta)
CREATE TABLE IF NOT EXISTS vehicles (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" uuid NOT NULL,
    "Title" varchar(500) NOT NULL,
    "Description" text,
    "Price" numeric(18,2) NOT NULL,
    "Currency" varchar(3) NOT NULL DEFAULT 'USD',
    "Status" integer NOT NULL DEFAULT 1,  -- 0=Draft, 1=Active, 2=Sold, 3=Inactive
    "VIN" varchar(17),
    "Make" varchar(100) NOT NULL,
    "Model" varchar(100) NOT NULL,
    "Year" integer NOT NULL,
    "Condition" integer NOT NULL DEFAULT 1,  -- 0=New, 1=Used, 2=CertifiedPreOwned
    "BodyStyle" integer NOT NULL DEFAULT 0,  -- 0=Sedan, 1=SUV, 2=Truck, 3=Coupe, etc.
    "Transmission" integer NOT NULL DEFAULT 1,  -- 0=Manual, 1=Automatic, 2=CVT
    "FuelType" integer NOT NULL DEFAULT 0,  -- 0=Gasoline, 1=Diesel, 2=Electric, 3=Hybrid
    "DriveType" integer NOT NULL DEFAULT 0,  -- 0=FWD, 1=RWD, 2=AWD, 3=4WD
    "VehicleType" integer NOT NULL DEFAULT 0,  -- 0=Car, 1=Truck, 2=Motorcycle, etc.
    "Mileage" integer NOT NULL DEFAULT 0,
    "MileageUnit" integer NOT NULL DEFAULT 0,  -- 0=Miles, 1=Kilometers
    "ExteriorColor" varchar(50),
    "InteriorColor" varchar(50),
    "EngineSize" varchar(100),
    "Horsepower" integer,
    "Torque" integer,
    "Doors" integer,
    "Seats" integer,
    "City" varchar(100),
    "State" varchar(100),
    "ZipCode" varchar(20),
    "Country" varchar(100) DEFAULT 'USA',
    "Latitude" decimal(10,8),
    "Longitude" decimal(11,8),
    "SellerId" uuid,
    "SellerName" varchar(200),
    "SellerPhone" varchar(50),
    "SellerEmail" varchar(255),
    "FeaturesJson" jsonb DEFAULT '[]'::jsonb,
    "CustomFieldsJson" jsonb DEFAULT '{}'::jsonb,
    "IsFeatured" boolean NOT NULL DEFAULT false,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "Views" integer NOT NULL DEFAULT 0,
    "Favorites" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "ExpiresAt" timestamp with time zone
);

-- 5. Crear tabla vehicle_images (imágenes de vehículos)
CREATE TABLE IF NOT EXISTS vehicle_images (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" uuid NOT NULL,
    "VehicleId" uuid NOT NULL REFERENCES vehicles("Id") ON DELETE CASCADE,
    "Url" varchar(1000) NOT NULL,
    "ThumbnailUrl" varchar(1000),
    "Caption" varchar(500),
    "ImageType" integer NOT NULL DEFAULT 0,  -- 0=Exterior, 1=Interior, 2=Engine, etc.
    "SortOrder" integer NOT NULL DEFAULT 0,
    "IsPrimary" boolean NOT NULL DEFAULT false,
    "FileSize" bigint,
    "MimeType" varchar(100),
    "Width" integer,
    "Height" integer,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW()
);

-- 6. Crear índices para vehicles
CREATE INDEX IF NOT EXISTS "IX_vehicles_DealerId" ON vehicles("DealerId");
CREATE INDEX IF NOT EXISTS "IX_vehicles_Make" ON vehicles("Make");
CREATE INDEX IF NOT EXISTS "IX_vehicles_Model" ON vehicles("Model");
CREATE INDEX IF NOT EXISTS "IX_vehicles_Year" ON vehicles("Year");
CREATE INDEX IF NOT EXISTS "IX_vehicles_Price" ON vehicles("Price");
CREATE INDEX IF NOT EXISTS "IX_vehicles_Status" ON vehicles("Status");
CREATE INDEX IF NOT EXISTS "IX_vehicles_CreatedAt" ON vehicles("CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_vehicles_IsFeatured" ON vehicles("IsFeatured");
CREATE INDEX IF NOT EXISTS "IX_vehicles_BodyStyle" ON vehicles("BodyStyle");
CREATE INDEX IF NOT EXISTS "IX_vehicles_FuelType" ON vehicles("FuelType");
CREATE INDEX IF NOT EXISTS "IX_vehicles_Condition" ON vehicles("Condition");

-- 7. Crear índices para vehicle_images
CREATE INDEX IF NOT EXISTS "IX_vehicle_images_VehicleId" ON vehicle_images("VehicleId");
CREATE INDEX IF NOT EXISTS "IX_vehicle_images_DealerId" ON vehicle_images("DealerId");

-- 8. Crear índices para catalogo
CREATE INDEX IF NOT EXISTS "IX_vehicle_models_MakeId" ON vehicle_models("MakeId");
CREATE INDEX IF NOT EXISTS "IX_vehicle_trims_ModelId" ON vehicle_trims("ModelId");

-- 9. Registrar migración en historial de EF Core
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260102000000_AddVehicleTables', '8.0.11')
ON CONFLICT ("MigrationId") DO NOTHING;

-- Mensaje de éxito
DO $$ BEGIN RAISE NOTICE '✅ Tablas de vehículos creadas exitosamente'; END $$;
