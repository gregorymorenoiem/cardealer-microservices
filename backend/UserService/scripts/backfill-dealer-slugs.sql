-- ============================================================================
-- Backfill Dealer Slugs
-- Run ONCE after deploying the AddDealerSlug migration
-- This generates URL-friendly slugs from BusinessName + City for existing dealers
-- ============================================================================

-- Step 1: Generate slugs from BusinessName + City
-- Pattern: "Auto Plaza Santo Domingo" in "Santo Domingo" → "auto-plaza-santo-domingo-santo-domingo"
UPDATE "Dealers"
SET "Slug" = CONCAT(
    TRIM(BOTH '-' FROM
        REGEXP_REPLACE(
            REGEXP_REPLACE(
                LOWER(
                    -- Remove diacritics by translating common Spanish characters
                    TRANSLATE(
                        "BusinessName",
                        'áéíóúüñÁÉÍÓÚÜÑ',
                        'aeiouunAEIOUUN'
                    )
                ),
                '[^a-z0-9]+', '-', 'g'  -- Replace non-alphanumeric with hyphens
            ),
            '-+', '-', 'g'  -- Collapse multiple hyphens
        )
    ),
    '-',
    TRIM(BOTH '-' FROM
        REGEXP_REPLACE(
            REGEXP_REPLACE(
                LOWER(
                    TRANSLATE(
                        COALESCE("City", ''),
                        'áéíóúüñÁÉÍÓÚÜÑ',
                        'aeiouunAEIOUUN'
                    )
                ),
                '[^a-z0-9]+', '-', 'g'
            ),
            '-+', '-', 'g'
        )
    )
)
WHERE "Slug" IS NULL OR "Slug" = '';

-- Step 2: Handle duplicates by appending a numeric suffix
-- This finds dealers that share the same slug and appends -2, -3, etc.
WITH duplicates AS (
    SELECT "Id", "Slug",
           ROW_NUMBER() OVER (PARTITION BY "Slug" ORDER BY "CreatedAt") AS rn
    FROM "Dealers"
    WHERE "Slug" IS NOT NULL AND "Slug" != ''
)
UPDATE "Dealers" d
SET "Slug" = d."Slug" || '-' || dup.rn
FROM duplicates dup
WHERE d."Id" = dup."Id"
  AND dup.rn > 1;

-- Step 3: Verify results
SELECT "Id", "BusinessName", "City", "Slug", "CreatedAt"
FROM "Dealers"
ORDER BY "CreatedAt" DESC
LIMIT 50;

-- Step 4: Verify no empty slugs remain
SELECT COUNT(*) AS "EmptySlugs"
FROM "Dealers"
WHERE "Slug" IS NULL OR "Slug" = '';

-- Step 5: Verify no duplicate slugs
SELECT "Slug", COUNT(*) AS "Count"
FROM "Dealers"
WHERE "Slug" IS NOT NULL AND "Slug" != ''
GROUP BY "Slug"
HAVING COUNT(*) > 1;
