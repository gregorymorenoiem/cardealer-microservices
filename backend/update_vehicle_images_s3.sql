-- =====================================================
-- Update Vehicle Images URLs to AWS S3
-- =====================================================
-- This script updates all vehicle image URLs from Unsplash IDs 
-- to full S3 URLs in the okla-images-2026 bucket
-- =====================================================

-- Backup current URLs (optional - para referencia)
-- SELECT "Id", "Url", "ThumbnailUrl" FROM vehicle_images;

-- Update URLs to full S3 paths with .jpg extension
UPDATE vehicle_images 
SET 
  "Url" = CONCAT('https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/', "Url", '.jpg'),
  "ThumbnailUrl" = CONCAT('https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/', "ThumbnailUrl", '.jpg')
WHERE 
  "Url" NOT LIKE 'http%'  -- Solo actualizar si no es ya una URL completa
  AND "Url" LIKE 'photo-%'; -- Solo actualizar si tiene el patrón photo-*

-- Verificar resultados
SELECT 
  vi."Id",
  v."Make",
  v."Model",
  vi."Url",
  vi."ThumbnailUrl",
  vi."IsPrimary"
FROM vehicle_images vi
JOIN vehicles v ON vi."VehicleId" = v."Id"
ORDER BY v."Make", v."Model", vi."SortOrder"
LIMIT 10;
