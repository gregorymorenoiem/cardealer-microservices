-- Migration: Add Specialties field to SellerProfile table
-- Date: 2026-02-24
-- Purpose: Add support for seller specialties (areas of expertise)
-- Risk: LOW - Column is additive, not destructive

BEGIN TRANSACTION;

-- ============================================
-- Add Specialties column to seller_profiles table
-- ============================================
-- Store specialties as a PostgreSQL text[] array
-- Examples: ARRAY['Sedanes', 'SUVs', 'Camionetas']
ALTER TABLE public.seller_profiles
ADD COLUMN IF NOT EXISTS specialties TEXT[] DEFAULT ARRAY[]::TEXT[];

-- Add comment for documentation
COMMENT ON COLUMN public.seller_profiles.specialties IS 
'Array of seller specialties/areas of expertise (e.g., ["Sedanes", "SUVs", "Camionetas"])';

-- ============================================
-- Create index for faster searches
-- ============================================
-- This index allows efficient queries like:
-- WHERE specialties && ARRAY['Sedanes'] (overlapping)
-- WHERE 'Sedanes' = ANY(specialties) (contains)
CREATE INDEX IF NOT EXISTS idx_seller_profiles_specialties 
ON public.seller_profiles USING GIN(specialties);

COMMENT ON INDEX idx_seller_profiles_specialties IS
'Index for efficient searching of seller specialties';

-- ============================================
-- Verify the changes
-- ============================================
SELECT 
    column_name,
    data_type,
    column_default,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'seller_profiles' 
  AND column_name = 'specialties';

-- ============================================
-- Migration Info
-- ============================================
-- Rollback script (if needed):
-- ALTER TABLE public.seller_profiles DROP COLUMN specialties;
-- DROP INDEX IF EXISTS idx_seller_profiles_specialties;

COMMIT;
