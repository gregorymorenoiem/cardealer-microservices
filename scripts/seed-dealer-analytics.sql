-- ============================================
-- Script para poblar DealerAnalyticsService con datos de prueba
-- ============================================

\c dealeranalyticsservice;

-- ============================================
-- 1. Crear tablas si no existen
-- ============================================

-- Tabla DealerAnalytics
CREATE TABLE IF NOT EXISTS "DealerAnalytics" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" UUID NOT NULL,
    "Date" TIMESTAMP NOT NULL,
    "TotalViews" INT NOT NULL DEFAULT 0,
    "UniqueViews" INT NOT NULL DEFAULT 0,
    "AverageViewDuration" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalContacts" INT NOT NULL DEFAULT 0,
    "PhoneCalls" INT NOT NULL DEFAULT 0,
    "WhatsAppMessages" INT NOT NULL DEFAULT 0,
    "EmailInquiries" INT NOT NULL DEFAULT 0,
    "TestDriveRequests" INT NOT NULL DEFAULT 0,
    "ActualSales" INT NOT NULL DEFAULT 0,
    "ConversionRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalRevenue" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AverageVehiclePrice" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "RevenuePerView" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ActiveListings" INT NOT NULL DEFAULT 0,
    "AverageDaysOnMarket" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "SoldVehicles" INT NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Tabla ProfileViews
CREATE TABLE IF NOT EXISTS "ProfileViews" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" UUID NOT NULL,
    "ViewedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "ViewerIpAddress" VARCHAR(50),
    "ViewerUserAgent" TEXT,
    "ViewerUserId" UUID,
    "ReferrerUrl" TEXT,
    "ViewedPage" VARCHAR(100),
    "DurationSeconds" INT NOT NULL DEFAULT 0,
    "DeviceType" VARCHAR(50),
    "Browser" VARCHAR(100),
    "OperatingSystem" VARCHAR(100),
    "Country" VARCHAR(100),
    "City" VARCHAR(100)
);

-- Tabla ContactEvents
CREATE TABLE IF NOT EXISTS "ContactEvents" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" UUID NOT NULL,
    "ClickedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "ContactType" INT NOT NULL,
    "ViewerIpAddress" VARCHAR(50),
    "ViewerUserId" UUID,
    "ContactValue" VARCHAR(200),
    "VehicleId" UUID,
    "Source" VARCHAR(100),
    "DeviceType" VARCHAR(50),
    "ConvertedToInquiry" BOOLEAN NOT NULL DEFAULT FALSE,
    "TimeToConversion" INTERVAL
);

-- Tabla ConversionFunnels
CREATE TABLE IF NOT EXISTS "ConversionFunnels" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" UUID NOT NULL,
    "Date" TIMESTAMP NOT NULL,
    "TotalViews" INT NOT NULL DEFAULT 0,
    "TotalContacts" INT NOT NULL DEFAULT 0,
    "TestDriveRequests" INT NOT NULL DEFAULT 0,
    "ActualSales" INT NOT NULL DEFAULT 0,
    "ViewToContactRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ContactToTestDriveRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TestDriveToSaleRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "OverallConversionRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "AverageTimeToSale" DECIMAL(18,2) NOT NULL DEFAULT 0
);

-- Tabla MarketBenchmarks
CREATE TABLE IF NOT EXISTS "MarketBenchmarks" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Date" TIMESTAMP NOT NULL,
    "VehicleCategory" VARCHAR(100) NOT NULL,
    "PriceRange" VARCHAR(100) NOT NULL,
    "MarketAveragePrice" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "MarketAverageDaysOnMarket" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "MarketAverageViews" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "MarketConversionRate" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PricePercentile25" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PricePercentile50" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PricePercentile75" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TotalDealersInSample" INT NOT NULL DEFAULT 0,
    "TotalVehiclesInSample" INT NOT NULL DEFAULT 0
);

-- Tabla DealerInsights
CREATE TABLE IF NOT EXISTS "DealerInsights" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DealerId" UUID NOT NULL,
    "Type" VARCHAR(100) NOT NULL,
    "Priority" VARCHAR(50) NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "ActionRecommendation" TEXT,
    "PotentialImpact" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Confidence" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsActedUpon" BOOLEAN NOT NULL DEFAULT FALSE,
    "ActionDate" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "ExpiresAt" TIMESTAMP
);

-- ============================================
-- 2. Datos de prueba (3 dealers)
-- ============================================

-- Dealer IDs de prueba
DO $$
DECLARE
    dealer1_id UUID := 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11';
    dealer2_id UUID := 'b1ffd89a-8a1c-4ef9-bb7e-7cc0ce491b22';
    dealer3_id UUID := 'c2ffe7ab-9b2d-4ef0-bb8f-8dd1df502c33';
    start_date DATE := CURRENT_DATE - INTERVAL '90 days';
    i INT;
BEGIN
    
    -- ============================================
    -- Dealer 1: Auto Elite (High Performance)
    -- ============================================
    
    -- Analytics diarios (últimos 90 días)
    FOR i IN 0..89 LOOP
        INSERT INTO "DealerAnalytics" (
            "Id", "DealerId", "Date", "TotalViews", "UniqueViews", "AverageViewDuration",
            "TotalContacts", "PhoneCalls", "WhatsAppMessages", "EmailInquiries",
            "TestDriveRequests", "ActualSales", "ConversionRate",
            "TotalRevenue", "AverageVehiclePrice", "RevenuePerView",
            "ActiveListings", "AverageDaysOnMarket", "SoldVehicles"
        ) VALUES (
            gen_random_uuid(),
            dealer1_id,
            start_date + (i || ' days')::INTERVAL,
            FLOOR(RANDOM() * 300 + 100)::INT, -- 100-400 views
            FLOOR(RANDOM() * 200 + 50)::INT,  -- 50-250 unique
            ROUND((RANDOM() * 180 + 60)::NUMERIC, 2), -- 60-240 seconds
            FLOOR(RANDOM() * 25 + 5)::INT,    -- 5-30 contacts
            FLOOR(RANDOM() * 8 + 2)::INT,     -- 2-10 calls
            FLOOR(RANDOM() * 12 + 3)::INT,    -- 3-15 whatsapp
            FLOOR(RANDOM() * 5 + 0)::INT,     -- 0-5 emails
            FLOOR(RANDOM() * 8 + 1)::INT,     -- 1-9 test drives
            FLOOR(RANDOM() * 4 + 0)::INT,     -- 0-4 sales
            ROUND((RANDOM() * 3 + 1)::NUMERIC, 2), -- 1-4% conversion
            ROUND((RANDOM() * 150000 + 50000)::NUMERIC, 2), -- 50K-200K revenue
            ROUND((RANDOM() * 30000 + 20000)::NUMERIC, 2),  -- 20K-50K avg price
            ROUND((RANDOM() * 500 + 100)::NUMERIC, 2),      -- 100-600 per view
            FLOOR(RANDOM() * 20 + 10)::INT,   -- 10-30 active
            ROUND((RANDOM() * 30 + 15)::NUMERIC, 2),  -- 15-45 days on market
            FLOOR(RANDOM() * 3 + 0)::INT      -- 0-3 sold
        );
    END LOOP;
    
    -- Profile Views (últimos 30 días)
    FOR i IN 1..300 LOOP
        INSERT INTO "ProfileViews" (
            "DealerId", "ViewedAt", "ViewerIpAddress", "DeviceType", 
            "DurationSeconds", "Browser", "Country", "City", "ViewedPage"
        ) VALUES (
            dealer1_id,
            NOW() - (RANDOM() * INTERVAL '30 days'),
            '192.168.' || FLOOR(RANDOM() * 255) || '.' || FLOOR(RANDOM() * 255),
            CASE FLOOR(RANDOM() * 3)
                WHEN 0 THEN 'mobile'
                WHEN 1 THEN 'desktop'
                ELSE 'tablet'
            END,
            FLOOR(RANDOM() * 300 + 30)::INT,
            CASE FLOOR(RANDOM() * 3)
                WHEN 0 THEN 'Chrome'
                WHEN 1 THEN 'Safari'
                ELSE 'Firefox'
            END,
            'República Dominicana',
            CASE FLOOR(RANDOM() * 3)
                WHEN 0 THEN 'Santo Domingo'
                WHEN 1 THEN 'Santiago'
                ELSE 'Puerto Plata'
            END,
            CASE FLOOR(RANDOM() * 3)
                WHEN 0 THEN 'profile'
                WHEN 1 THEN 'vehicles'
                ELSE 'contact'
            END
        );
    END LOOP;
    
    -- Contact Events
    FOR i IN 1..85 LOOP
        INSERT INTO "ContactEvents" (
            "DealerId", "ClickedAt", "ContactType", "DeviceType",
            "Source", "ConvertedToInquiry"
        ) VALUES (
            dealer1_id,
            NOW() - (RANDOM() * INTERVAL '30 days'),
            FLOOR(RANDOM() * 4 + 1)::INT, -- 1-5: Phone, Email, WhatsApp, Website, Social
            CASE FLOOR(RANDOM() * 2)
                WHEN 0 THEN 'mobile'
                ELSE 'desktop'
            END,
            CASE FLOOR(RANDOM() * 3)
                WHEN 0 THEN 'search'
                WHEN 1 THEN 'direct'
                ELSE 'social'
            END,
            RANDOM() > 0.7 -- 30% conversion
        );
    END LOOP;
    
    -- Conversion Funnel (últimos 30 días)
    FOR i IN 0..29 LOOP
        DECLARE
            views INT := FLOOR(RANDOM() * 150 + 50)::INT;
            contacts INT := FLOOR(views * (RANDOM() * 0.15 + 0.05))::INT;
            test_drives INT := FLOOR(contacts * (RANDOM() * 0.4 + 0.2))::INT;
            sales INT := FLOOR(test_drives * (RANDOM() * 0.5 + 0.2))::INT;
        BEGIN
            INSERT INTO "ConversionFunnels" (
                "DealerId", "Date", "TotalViews", "TotalContacts", 
                "TestDriveRequests", "ActualSales",
                "ViewToContactRate", "ContactToTestDriveRate",
                "TestDriveToSaleRate", "OverallConversionRate",
                "AverageTimeToSale"
            ) VALUES (
                dealer1_id,
                CURRENT_DATE - (i || ' days')::INTERVAL,
                views,
                contacts,
                test_drives,
                sales,
                ROUND((contacts::DECIMAL / NULLIF(views, 0) * 100), 2),
                ROUND((test_drives::DECIMAL / NULLIF(contacts, 0) * 100), 2),
                ROUND((sales::DECIMAL / NULLIF(test_drives, 0) * 100), 2),
                ROUND((sales::DECIMAL / NULLIF(views, 0) * 100), 2),
                ROUND((RANDOM() * 10 + 5)::NUMERIC, 2)
            );
        END;
    END LOOP;
    
    -- Dealer Insights
    INSERT INTO "DealerInsights" ("DealerId", "Type", "Priority", "Title", "Description", "ActionRecommendation", "PotentialImpact", "Confidence") VALUES
    (dealer1_id, 'Pricing', 'High', 'Vehículos con precio por encima del mercado', 'Tienes 5 vehículos con precios 15% por encima del promedio del mercado', 'Ajusta los precios de Toyota Corolla 2022 y similares', 12.5, 0.85),
    (dealer1_id, 'Engagement', 'Medium', 'Aumento en vistas móviles', 'Las vistas desde móvil aumentaron 35% este mes', 'Optimiza las imágenes de vehículos para móvil', 8.3, 0.92),
    (dealer1_id, 'Conversion', 'High', 'Baja tasa de conversión en test drives', 'Solo 25% de test drives resultan en venta (promedio mercado: 42%)', 'Mejora el proceso de seguimiento post-test drive', 18.7, 0.78),
    (dealer1_id, 'Performance', 'Low', 'Tiempo de respuesta excelente', 'Tu tiempo promedio de respuesta (2.1 hrs) está 40% mejor que el mercado', 'Mantén este nivel de servicio', 5.0, 0.95);
    
    -- ============================================
    -- Dealer 2: Motors Plus (Medium Performance)
    -- ============================================
    
    FOR i IN 0..89 LOOP
        INSERT INTO "DealerAnalytics" (
            "Id", "DealerId", "Date", "TotalViews", "UniqueViews", "AverageViewDuration",
            "TotalContacts", "PhoneCalls", "WhatsAppMessages", "EmailInquiries",
            "TestDriveRequests", "ActualSales", "ConversionRate",
            "TotalRevenue", "AverageVehiclePrice", "RevenuePerView",
            "ActiveListings", "AverageDaysOnMarket", "SoldVehicles"
        ) VALUES (
            gen_random_uuid(),
            dealer2_id,
            start_date + (i || ' days')::INTERVAL,
            FLOOR(RANDOM() * 150 + 50)::INT,
            FLOOR(RANDOM() * 100 + 25)::INT,
            ROUND((RANDOM() * 120 + 40)::NUMERIC, 2),
            FLOOR(RANDOM() * 15 + 3)::INT,
            FLOOR(RANDOM() * 5 + 1)::INT,
            FLOOR(RANDOM() * 8 + 2)::INT,
            FLOOR(RANDOM() * 3 + 0)::INT,
            FLOOR(RANDOM() * 5 + 0)::INT,
            FLOOR(RANDOM() * 2 + 0)::INT,
            ROUND((RANDOM() * 2 + 0.5)::NUMERIC, 2),
            ROUND((RANDOM() * 80000 + 20000)::NUMERIC, 2),
            ROUND((RANDOM() * 20000 + 15000)::NUMERIC, 2),
            ROUND((RANDOM() * 300 + 50)::NUMERIC, 2),
            FLOOR(RANDOM() * 15 + 5)::INT,
            ROUND((RANDOM() * 40 + 20)::NUMERIC, 2),
            FLOOR(RANDOM() * 2 + 0)::INT
        );
    END LOOP;
    
    -- Profile Views
    FOR i IN 1..150 LOOP
        INSERT INTO "ProfileViews" (
            "DealerId", "ViewedAt", "ViewerIpAddress", "DeviceType", 
            "DurationSeconds", "Browser", "Country", "City", "ViewedPage"
        ) VALUES (
            dealer2_id,
            NOW() - (RANDOM() * INTERVAL '30 days'),
            '10.0.' || FLOOR(RANDOM() * 255) || '.' || FLOOR(RANDOM() * 255),
            CASE FLOOR(RANDOM() * 3)
                WHEN 0 THEN 'mobile'
                WHEN 1 THEN 'desktop'
                ELSE 'tablet'
            END,
            FLOOR(RANDOM() * 200 + 20)::INT,
            'Chrome',
            'República Dominicana',
            'Santiago',
            'vehicles'
        );
    END LOOP;
    
    -- Contact Events
    FOR i IN 1..45 LOOP
        INSERT INTO "ContactEvents" (
            "DealerId", "ClickedAt", "ContactType", "DeviceType",
            "Source", "ConvertedToInquiry"
        ) VALUES (
            dealer2_id,
            NOW() - (RANDOM() * INTERVAL '30 days'),
            FLOOR(RANDOM() * 3 + 1)::INT,
            'mobile',
            'search',
            RANDOM() > 0.6
        );
    END LOOP;
    
    -- Insights
    INSERT INTO "DealerInsights" ("DealerId", "Type", "Priority", "Title", "Description", "ActionRecommendation", "PotentialImpact", "Confidence") VALUES
    (dealer2_id, 'Visibility', 'High', 'Pocas fotos en listings', 'Tus vehículos tienen promedio de 3.2 fotos (recomendado: 10+)', 'Agrega más fotos de calidad a tus publicaciones', 15.2, 0.88),
    (dealer2_id, 'Response', 'Medium', 'Tiempo de respuesta lento', 'Respondes en promedio 8.5 hrs (mercado: 3.2 hrs)', 'Configura notificaciones push para responder más rápido', 11.4, 0.91);
    
    -- ============================================
    -- Dealer 3: Premium Auto House (Low Activity)
    -- ============================================
    
    FOR i IN 0..89 LOOP
        INSERT INTO "DealerAnalytics" (
            "Id", "DealerId", "Date", "TotalViews", "UniqueViews", "AverageViewDuration",
            "TotalContacts", "PhoneCalls", "WhatsAppMessages", "EmailInquiries",
            "TestDriveRequests", "ActualSales", "ConversionRate",
            "TotalRevenue", "AverageVehiclePrice", "RevenuePerView",
            "ActiveListings", "AverageDaysOnMarket", "SoldVehicles"
        ) VALUES (
            gen_random_uuid(),
            dealer3_id,
            start_date + (i || ' days')::INTERVAL,
            FLOOR(RANDOM() * 80 + 20)::INT,
            FLOOR(RANDOM() * 50 + 10)::INT,
            ROUND((RANDOM() * 90 + 30)::NUMERIC, 2),
            FLOOR(RANDOM() * 8 + 1)::INT,
            FLOOR(RANDOM() * 3 + 0)::INT,
            FLOOR(RANDOM() * 4 + 1)::INT,
            FLOOR(RANDOM() * 2 + 0)::INT,
            FLOOR(RANDOM() * 3 + 0)::INT,
            FLOOR(RANDOM() * 1 + 0)::INT,
            ROUND((RANDOM() * 1.5 + 0.2)::NUMERIC, 2),
            ROUND((RANDOM() * 50000 + 10000)::NUMERIC, 2),
            ROUND((RANDOM() * 15000 + 10000)::NUMERIC, 2),
            ROUND((RANDOM() * 200 + 30)::NUMERIC, 2),
            FLOOR(RANDOM() * 10 + 3)::INT,
            ROUND((RANDOM() * 50 + 25)::NUMERIC, 2),
            FLOOR(RANDOM() * 1 + 0)::INT
        );
    END LOOP;
    
    -- Profile Views
    FOR i IN 1..75 LOOP
        INSERT INTO "ProfileViews" (
            "DealerId", "ViewedAt", "ViewerIpAddress", "DeviceType", 
            "DurationSeconds", "Browser", "Country", "City", "ViewedPage"
        ) VALUES (
            dealer3_id,
            NOW() - (RANDOM() * INTERVAL '30 days'),
            '172.16.' || FLOOR(RANDOM() * 255) || '.' || FLOOR(RANDOM() * 255),
            'mobile',
            FLOOR(RANDOM() * 150 + 15)::INT,
            'Safari',
            'República Dominicana',
            'Santo Domingo',
            'profile'
        );
    END LOOP;
    
    -- ============================================
    -- Market Benchmarks (Datos generales del mercado)
    -- ============================================
    
    -- Sedanes
    INSERT INTO "MarketBenchmarks" ("Date", "VehicleCategory", "PriceRange", 
        "MarketAveragePrice", "MarketAverageDaysOnMarket", "MarketAverageViews",
        "MarketConversionRate", "PricePercentile25", "PricePercentile50", "PricePercentile75",
        "TotalDealersInSample", "TotalVehiclesInSample")
    VALUES (
        CURRENT_DATE, 'Sedanes', '$15K-$25K',
        19500, 32.5, 185, 2.8,
        16000, 19500, 23000,
        45, 238
    );
    
    -- SUVs
    INSERT INTO "MarketBenchmarks" ("Date", "VehicleCategory", "PriceRange", 
        "MarketAveragePrice", "MarketAverageDaysOnMarket", "MarketAverageViews",
        "MarketConversionRate", "PricePercentile25", "PricePercentile50", "PricePercentile75",
        "TotalDealersInSample", "TotalVehiclesInSample")
    VALUES (
        CURRENT_DATE, 'SUVs', '$25K-$40K',
        32000, 28.3, 245, 3.2,
        27000, 32000, 37000,
        52, 312
    );
    
    -- Pickups
    INSERT INTO "MarketBenchmarks" ("Date", "VehicleCategory", "PriceRange", 
        "MarketAveragePrice", "MarketAverageDaysOnMarket", "MarketAverageViews",
        "MarketConversionRate", "PricePercentile25", "PricePercentile50", "PricePercentile75",
        "TotalDealersInSample", "TotalVehiclesInSample")
    VALUES (
        CURRENT_DATE, 'Pickups', '$30K-$50K',
        38500, 35.7, 198, 2.5,
        32000, 38500, 45000,
        38, 156
    );
    
    -- Deportivos
    INSERT INTO "MarketBenchmarks" ("Date", "VehicleCategory", "PriceRange", 
        "MarketAveragePrice", "MarketAverageDaysOnMarket", "MarketAverageViews",
        "MarketConversionRate", "PricePercentile25", "PricePercentile50", "PricePercentile75",
        "TotalDealersInSample", "TotalVehiclesInSample")
    VALUES (
        CURRENT_DATE, 'Deportivos', '$40K-$80K',
        58000, 45.2, 320, 1.8,
        45000, 58000, 72000,
        28, 89
    );
    
END $$;

-- ============================================
-- 3. Crear índices para mejor performance
-- ============================================

CREATE INDEX IF NOT EXISTS idx_dealer_analytics_dealer_date ON "DealerAnalytics"("DealerId", "Date");
CREATE INDEX IF NOT EXISTS idx_profile_views_dealer_date ON "ProfileViews"("DealerId", "ViewedAt");
CREATE INDEX IF NOT EXISTS idx_contact_events_dealer_date ON "ContactEvents"("DealerId", "ClickedAt");
CREATE INDEX IF NOT EXISTS idx_conversion_funnel_dealer_date ON "ConversionFunnels"("DealerId", "Date");
CREATE INDEX IF NOT EXISTS idx_dealer_insights_dealer ON "DealerInsights"("DealerId");
CREATE INDEX IF NOT EXISTS idx_market_benchmarks_date ON "MarketBenchmarks"("Date");

-- ============================================
-- 4. Verificar datos insertados
-- ============================================

SELECT 'DealerAnalytics' AS table_name, COUNT(*) AS record_count FROM "DealerAnalytics"
UNION ALL
SELECT 'ProfileViews', COUNT(*) FROM "ProfileViews"
UNION ALL
SELECT 'ContactEvents', COUNT(*) FROM "ContactEvents"
UNION ALL
SELECT 'ConversionFunnels', COUNT(*) FROM "ConversionFunnels"
UNION ALL
SELECT 'MarketBenchmarks', COUNT(*) FROM "MarketBenchmarks"
UNION ALL
SELECT 'DealerInsights', COUNT(*) FROM "DealerInsights";

\echo '✅ Seed data loaded successfully!'
\echo ''
\echo 'Dealer IDs creados:'
\echo '  - Auto Elite (High):    a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'
\echo '  - Motors Plus (Medium): b1ffd89a-8a1c-4ef9-bb7e-7cc0ce491b22'
\echo '  - Premium Auto (Low):   c2ffe7ab-9b2d-4ef0-bb8f-8dd1df502c33'
