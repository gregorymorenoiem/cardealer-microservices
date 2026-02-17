-- Script para insertar datos de prueba en LeadScoringService
-- Ejecutar después de que el servicio haya creado las tablas automáticamente

-- Nota: Estos UUIDs deben corresponder a un dealer real en el sistema
-- Para desarrollo, usaremos UUIDs de ejemplo

-- Primero, insertar reglas de scoring (si no existen)
INSERT INTO scoring_rules ("Id", "Name", "Description", "ActionType", "ScoreImpact", "Configuration", "IsActive", "CreatedAt", "UpdatedAt")
SELECT * FROM (VALUES
    (gen_random_uuid(), 'ViewListing', 'Points for viewing a vehicle listing', 0, 2, '{"maxPerDay": 10, "decayHours": 24}', true, NOW(), NOW()),
    (gen_random_uuid(), 'ViewImages', 'Points for viewing vehicle images', 1, 3, '{"maxPerDay": 5}', true, NOW(), NOW()),
    (gen_random_uuid(), 'ClickPhone', 'Points for clicking phone number', 3, 15, '{"maxPerSession": 1}', true, NOW(), NOW()),
    (gen_random_uuid(), 'ClickEmail', 'Points for clicking email', 4, 12, '{"maxPerSession": 1}', true, NOW(), NOW()),
    (gen_random_uuid(), 'ClickWhatsApp', 'Points for clicking WhatsApp', 5, 18, '{"maxPerSession": 1}', true, NOW(), NOW()),
    (gen_random_uuid(), 'SendMessage', 'Points for sending a message', 6, 20, '{"maxPerDay": 3}', true, NOW(), NOW()),
    (gen_random_uuid(), 'AddToFavorites', 'Points for adding to favorites', 7, 10, '{"maxPerVehicle": 1}', true, NOW(), NOW()),
    (gen_random_uuid(), 'AddToComparison', 'Points for adding to comparison', 10, 8, '{"maxPerSession": 3}', true, NOW(), NOW()),
    (gen_random_uuid(), 'ScheduleTestDrive', 'Points for scheduling test drive', 11, 25, '{"maxPerVehicle": 1}', true, NOW(), NOW()),
    (gen_random_uuid(), 'RequestFinancing', 'Points for requesting financing', 12, 22, '{"maxPerVehicle": 1}', true, NOW(), NOW()),
    (gen_random_uuid(), 'MakeOffer', 'Points for making an offer', 13, 30, '{"maxPerVehicle": 3}', true, NOW(), NOW())
) AS v("Id", "Name", "Description", "ActionType", "ScoreImpact", "Configuration", "IsActive", "CreatedAt", "UpdatedAt")
WHERE NOT EXISTS (SELECT 1 FROM scoring_rules LIMIT 1);

-- Nota: Para los leads de prueba, usaremos estos UUIDs de ejemplo:
-- DealerId: Debe coincidir con un dealer real del sistema
-- En desarrollo, puedes crear un usuario dealer primero y usar su ID

-- Variables de ejemplo (cambiar según tu entorno)
DO $$
DECLARE
    v_dealer_id UUID := '11111111-1111-1111-1111-111111111111'; -- Cambiar por un dealer real
    v_dealer_name TEXT := 'Auto Demo Dealer';
    v_lead1_id UUID := gen_random_uuid();
    v_lead2_id UUID := gen_random_uuid();
    v_lead3_id UUID := gen_random_uuid();
    v_lead4_id UUID := gen_random_uuid();
    v_lead5_id UUID := gen_random_uuid();
BEGIN
    -- Insertar leads de prueba
    
    -- Lead 1: HOT Lead (score 85)
    INSERT INTO leads (
        "Id", "UserId", "UserEmail", "UserFullName", "UserPhone",
        "VehicleId", "VehicleTitle", "VehiclePrice",
        "DealerId", "DealerName",
        "Score", "Temperature", "ConversionProbability",
        "EngagementScore", "RecencyScore", "IntentScore",
        "ViewCount", "ContactCount", "FavoriteCount", "ShareCount", "ComparisonCount",
        "HasScheduledTestDrive", "HasRequestedFinancing",
        "TotalTimeSpentSeconds", "AverageSessionDurationSeconds",
        "Status", "Source",
        "FirstInteractionAt", "LastInteractionAt", "LastContactedAt", "ConvertedAt",
        "DealerNotes", "CreatedAt", "UpdatedAt"
    ) VALUES (
        v_lead1_id,
        gen_random_uuid(), 'carlos.martinez@gmail.com', 'Carlos Martínez', '809-555-0001',
        gen_random_uuid(), 'Toyota Corolla 2023 Automático', 1250000.00,
        v_dealer_id, v_dealer_name,
        85, 2, 75.50,  -- Score=85, Temperature=Hot(2), 75.5% probability
        35, 28, 22,     -- Engagement, Recency, Intent
        15, 5, 2, 1, 2,
        true, true,
        3600, 240,
        1, 0,          -- Status=Contacted, Source=OrganicSearch
        NOW() - INTERVAL '7 days', NOW() - INTERVAL '2 hours', NOW() - INTERVAL '1 day', NULL,
        'Cliente muy interesado, quiere financiamiento', NOW(), NOW()
    );
    
    -- Lead 2: WARM Lead (score 55)
    INSERT INTO leads (
        "Id", "UserId", "UserEmail", "UserFullName", "UserPhone",
        "VehicleId", "VehicleTitle", "VehiclePrice",
        "DealerId", "DealerName",
        "Score", "Temperature", "ConversionProbability",
        "EngagementScore", "RecencyScore", "IntentScore",
        "ViewCount", "ContactCount", "FavoriteCount", "ShareCount", "ComparisonCount",
        "HasScheduledTestDrive", "HasRequestedFinancing",
        "TotalTimeSpentSeconds", "AverageSessionDurationSeconds",
        "Status", "Source",
        "FirstInteractionAt", "LastInteractionAt", "LastContactedAt", "ConvertedAt",
        "DealerNotes", "CreatedAt", "UpdatedAt"
    ) VALUES (
        v_lead2_id,
        gen_random_uuid(), 'maria.rodriguez@hotmail.com', 'María Rodríguez', '849-555-0002',
        gen_random_uuid(), 'Honda Civic 2022 EX', 1150000.00,
        v_dealer_id, v_dealer_name,
        55, 1, 45.00,  -- Score=55, Temperature=Warm(1), 45% probability
        22, 20, 13,
        8, 2, 1, 0, 1,
        false, false,
        1800, 150,
        0, 1,          -- Status=New, Source=DirectListing
        NOW() - INTERVAL '5 days', NOW() - INTERVAL '12 hours', NULL, NULL,
        NULL, NOW(), NOW()
    );
    
    -- Lead 3: COLD Lead (score 25)
    INSERT INTO leads (
        "Id", "UserId", "UserEmail", "UserFullName", "UserPhone",
        "VehicleId", "VehicleTitle", "VehiclePrice",
        "DealerId", "DealerName",
        "Score", "Temperature", "ConversionProbability",
        "EngagementScore", "RecencyScore", "IntentScore",
        "ViewCount", "ContactCount", "FavoriteCount", "ShareCount", "ComparisonCount",
        "HasScheduledTestDrive", "HasRequestedFinancing",
        "TotalTimeSpentSeconds", "AverageSessionDurationSeconds",
        "Status", "Source",
        "FirstInteractionAt", "LastInteractionAt", "LastContactedAt", "ConvertedAt",
        "DealerNotes", "CreatedAt", "UpdatedAt"
    ) VALUES (
        v_lead3_id,
        gen_random_uuid(), 'jose.perez@yahoo.com', 'José Pérez', NULL,
        gen_random_uuid(), 'Hyundai Elantra 2021', 850000.00,
        v_dealer_id, v_dealer_name,
        25, 0, 15.00,  -- Score=25, Temperature=Cold(0), 15% probability
        10, 8, 7,
        3, 0, 0, 0, 0,
        false, false,
        300, 100,
        0, 3,          -- Status=New, Source=SocialMedia
        NOW() - INTERVAL '14 days', NOW() - INTERVAL '10 days', NULL, NULL,
        NULL, NOW(), NOW()
    );
    
    -- Lead 4: HOT Lead reciente (score 78)
    INSERT INTO leads (
        "Id", "UserId", "UserEmail", "UserFullName", "UserPhone",
        "VehicleId", "VehicleTitle", "VehiclePrice",
        "DealerId", "DealerName",
        "Score", "Temperature", "ConversionProbability",
        "EngagementScore", "RecencyScore", "IntentScore",
        "ViewCount", "ContactCount", "FavoriteCount", "ShareCount", "ComparisonCount",
        "HasScheduledTestDrive", "HasRequestedFinancing",
        "TotalTimeSpentSeconds", "AverageSessionDurationSeconds",
        "Status", "Source",
        "FirstInteractionAt", "LastInteractionAt", "LastContactedAt", "ConvertedAt",
        "DealerNotes", "CreatedAt", "UpdatedAt"
    ) VALUES (
        v_lead4_id,
        gen_random_uuid(), 'ana.santos@gmail.com', 'Ana Santos', '829-555-0004',
        gen_random_uuid(), 'Kia Sportage 2024 LX', 1650000.00,
        v_dealer_id, v_dealer_name,
        78, 2, 68.00,  -- Score=78, Temperature=Hot(2), 68% probability
        30, 28, 20,
        12, 4, 1, 2, 1,
        true, false,
        2700, 225,
        2, 0,          -- Status=Qualified, Source=OrganicSearch
        NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 hour', NOW() - INTERVAL '6 hours', NULL,
        'Ya agendó test drive para el sábado', NOW(), NOW()
    );
    
    -- Lead 5: WARM en negociación (score 62)
    INSERT INTO leads (
        "Id", "UserId", "UserEmail", "UserFullName", "UserPhone",
        "VehicleId", "VehicleTitle", "VehiclePrice",
        "DealerId", "DealerName",
        "Score", "Temperature", "ConversionProbability",
        "EngagementScore", "RecencyScore", "IntentScore",
        "ViewCount", "ContactCount", "FavoriteCount", "ShareCount", "ComparisonCount",
        "HasScheduledTestDrive", "HasRequestedFinancing",
        "TotalTimeSpentSeconds", "AverageSessionDurationSeconds",
        "Status", "Source",
        "FirstInteractionAt", "LastInteractionAt", "LastContactedAt", "ConvertedAt",
        "DealerNotes", "CreatedAt", "UpdatedAt"
    ) VALUES (
        v_lead5_id,
        gen_random_uuid(), 'pedro.gomez@outlook.com', 'Pedro Gómez', '809-555-0005',
        gen_random_uuid(), 'Mazda CX-5 2023 Signature', 2100000.00,
        v_dealer_id, v_dealer_name,
        62, 1, 52.00,  -- Score=62, Temperature=Warm(1), 52% probability
        25, 22, 15,
        10, 3, 1, 1, 2,
        false, true,
        2100, 175,
        4, 4,          -- Status=Negotiating, Source=Referral
        NOW() - INTERVAL '10 days', NOW() - INTERVAL '4 hours', NOW() - INTERVAL '2 days', NULL,
        'Pidió descuento, esperando respuesta de gerencia', NOW(), NOW()
    );
    
    -- Insertar acciones para Lead 1 (HOT)
    INSERT INTO lead_actions ("Id", "LeadId", "ActionType", "Description", "ScoreImpact", "OccurredAt", "Metadata", "IpAddress", "UserAgent")
    VALUES
        (gen_random_uuid(), v_lead1_id, 0, 'Viewed vehicle listing', 2, NOW() - INTERVAL '7 days', NULL, '192.168.1.100', 'Chrome/120'),
        (gen_random_uuid(), v_lead1_id, 7, 'Added to favorites', 10, NOW() - INTERVAL '6 days', NULL, '192.168.1.100', 'Chrome/120'),
        (gen_random_uuid(), v_lead1_id, 3, 'Clicked phone number', 15, NOW() - INTERVAL '5 days', NULL, '192.168.1.100', 'Chrome/120'),
        (gen_random_uuid(), v_lead1_id, 5, 'Clicked WhatsApp', 18, NOW() - INTERVAL '4 days', NULL, '192.168.1.100', 'Chrome/120'),
        (gen_random_uuid(), v_lead1_id, 11, 'Scheduled test drive', 25, NOW() - INTERVAL '3 days', '{"date": "2026-01-15"}', '192.168.1.100', 'Chrome/120'),
        (gen_random_uuid(), v_lead1_id, 12, 'Requested financing', 22, NOW() - INTERVAL '2 days', NULL, '192.168.1.100', 'Chrome/120');
    
    -- Insertar acciones para Lead 4 (HOT)
    INSERT INTO lead_actions ("Id", "LeadId", "ActionType", "Description", "ScoreImpact", "OccurredAt", "Metadata", "IpAddress", "UserAgent")
    VALUES
        (gen_random_uuid(), v_lead4_id, 0, 'Viewed vehicle listing', 2, NOW() - INTERVAL '3 days', NULL, '192.168.1.200', 'Safari/17'),
        (gen_random_uuid(), v_lead4_id, 1, 'Viewed vehicle images', 3, NOW() - INTERVAL '3 days', NULL, '192.168.1.200', 'Safari/17'),
        (gen_random_uuid(), v_lead4_id, 10, 'Added to comparison', 8, NOW() - INTERVAL '2 days', NULL, '192.168.1.200', 'Safari/17'),
        (gen_random_uuid(), v_lead4_id, 11, 'Scheduled test drive', 25, NOW() - INTERVAL '1 day', '{"date": "2026-01-18"}', '192.168.1.200', 'Safari/17');
    
    RAISE NOTICE 'Datos de prueba insertados exitosamente para dealer %', v_dealer_id;
END $$;

-- Mostrar estadísticas
SELECT 
    'Total Leads' as metric, COUNT(*)::text as value FROM leads
UNION ALL
SELECT 
    'HOT Leads' as metric, COUNT(*)::text FROM leads WHERE "Temperature" = 2
UNION ALL
SELECT 
    'WARM Leads' as metric, COUNT(*)::text FROM leads WHERE "Temperature" = 1
UNION ALL
SELECT 
    'COLD Leads' as metric, COUNT(*)::text FROM leads WHERE "Temperature" = 0
UNION ALL
SELECT 
    'Average Score' as metric, ROUND(AVG("Score")::numeric, 1)::text FROM leads;
