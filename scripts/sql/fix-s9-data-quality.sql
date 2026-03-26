-- Sprint 9 Data Quality Fixes
-- S9-BUG-002: Fix "Santo DomingoNorte" → "Santo Domingo Norte" (2 vehicles)
-- S9-WARN-001: Fix Hybrid↔Electric fuel type swap (~10 vehicles)
-- Date: 2026-03-29

BEGIN;

-- ============================================================
-- S9-BUG-002: Fix city name "Santo DomingoNorte" (missing space)
-- ============================================================
UPDATE vehicles
SET "City" = 'Santo Domingo Norte',
    "UpdatedAt" = NOW()
WHERE "City" = 'Santo DomingoNorte';

-- Also fix any other known variants
UPDATE vehicles
SET "City" = 'Santo Domingo Este',
    "UpdatedAt" = NOW()
WHERE "City" = 'Santo DomingoEste';

UPDATE vehicles
SET "City" = 'Santo Domingo Oeste',
    "UpdatedAt" = NOW()
WHERE "City" = 'Santo DomingoOeste';

-- ============================================================
-- S9-WARN-001: Fix Hybrid↔Electric fuel type swap
-- Tesla vehicles should be Electric (currently mislabeled as Hybrid)
-- ============================================================
UPDATE vehicles
SET "FuelType" = 'Electric',
    "UpdatedAt" = NOW()
WHERE "Make" = 'Tesla'
  AND "FuelType" = 'Hybrid';

-- ============================================================
-- Vehicles with "Hybrid" in model name should be Hybrid (currently mislabeled as Electric)
-- ============================================================
UPDATE vehicles
SET "FuelType" = 'Hybrid',
    "UpdatedAt" = NOW()
WHERE "Model" ILIKE '%Hybrid%'
  AND "FuelType" = 'Electric';

COMMIT;
