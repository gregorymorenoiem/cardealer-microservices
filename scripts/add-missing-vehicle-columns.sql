-- =====================================================
-- Agregar columnas faltantes a la tabla vehicles
-- =====================================================
-- Columnas que existen en Vehicle.cs pero no en la tabla DB

-- Identificación adicional
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "StockNumber" VARCHAR(50);
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "MakeId" UUID;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "ModelId" UUID;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "Trim" VARCHAR(100);
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "Generation" VARCHAR(100);

-- Motor y transmisión
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "Cylinders" INTEGER;

-- Kilometraje y condición
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "PreviousOwners" INTEGER;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "AccidentHistory" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "HasCleanTitle" BOOLEAN NOT NULL DEFAULT true;

-- Apariencia
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "InteriorMaterial" VARCHAR(50);

-- Economía de combustible
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "MpgCity" INTEGER;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "MpgHighway" INTEGER;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "MpgCombined" INTEGER;

-- Historial y certificaciones
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "IsCertified" BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "CertificationProgram" VARCHAR(100);
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "CarfaxReportUrl" VARCHAR(500);
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "LastServiceDate" TIMESTAMP WITH TIME ZONE;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "ServiceHistoryNotes" TEXT;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "WarrantyInfo" VARCHAR(500);

-- Características y equipamiento
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "PackagesJson" JSONB DEFAULT '[]'::jsonb;

-- Métricas de engagement (renombrar si es necesario)
DO $$
BEGIN
    -- Renombrar Views a ViewCount si existe
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'vehicles' AND column_name = 'Views') THEN
        ALTER TABLE vehicles RENAME COLUMN "Views" TO "ViewCount";
    END IF;
    
    -- Renombrar Favorites a FavoriteCount si existe
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'vehicles' AND column_name = 'Favorites') THEN
        ALTER TABLE vehicles RENAME COLUMN "Favorites" TO "FavoriteCount";
    END IF;
EXCEPTION
    WHEN duplicate_column THEN NULL;
END $$;

ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "ViewCount" INTEGER NOT NULL DEFAULT 0;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "FavoriteCount" INTEGER NOT NULL DEFAULT 0;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "InquiryCount" INTEGER NOT NULL DEFAULT 0;

-- Fechas
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "PublishedAt" TIMESTAMP WITH TIME ZONE;
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "SoldAt" TIMESTAMP WITH TIME ZONE;

-- Categoría
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "CategoryId" UUID;

-- Seller info
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "SellerPhone" VARCHAR(50);
ALTER TABLE vehicles ADD COLUMN IF NOT EXISTS "SellerEmail" VARCHAR(255);

-- =====================================================
-- Verificación
-- =====================================================
DO $$
DECLARE
    col_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO col_count 
    FROM information_schema.columns 
    WHERE table_name = 'vehicles' AND table_schema = 'public';
    
    RAISE NOTICE '';
    RAISE NOTICE '✅ Columnas actualizadas en vehicles:';
    RAISE NOTICE '   Total columnas: %', col_count;
END $$;
