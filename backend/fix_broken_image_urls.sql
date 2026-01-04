-- Reemplazar las 7 URLs de imágenes rotas con URLs que funcionan
-- Imágenes rotas identificadas:
-- photo-1562141961-944a7deeb782.jpg (403)
-- photo-1568605117036-5fe5e7bab0b7.jpg (403)
-- photo-1594502879440-eca1c5d70adf.jpg (403)
-- photo-1610768764270-790fbec18178.jpg (403)
-- photo-1611651338412-8403fa6e3599.jpg (403)
-- photo-1614162692292-7ac56d7f7f1e.jpg (403)
-- photo-1617531653332-bd46c24f2068.jpg (403)

-- Imágenes de reemplazo (validadas con HTTP 200):
-- photo-1536700503339-1e4b06520771.jpg
-- photo-1549317661-bd32c8ce0db2.jpg
-- photo-1552519507-da3b142c6e3d.jpg
-- photo-1560958089-b8a1929cea89.jpg
-- photo-1606016159991-dfe4f2746ad5.jpg
-- photo-1617788138017-80ad40651399.jpg
-- photo-1618843479313-40f8afb4b4d8.jpg

BEGIN;

-- 1. Reemplazar photo-1562141961-944a7deeb782 (Audi RS7 SortOrder 3)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1536700503339-1e4b06520771.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1536700503339-1e4b06520771.jpg'
WHERE "Url" LIKE '%photo-1562141961-944a7deeb782%';

-- 2. Reemplazar photo-1568605117036-5fe5e7bab0b7 (Audi RS7 SortOrder 4)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1549317661-bd32c8ce0db2.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1549317661-bd32c8ce0db2.jpg'
WHERE "Url" LIKE '%photo-1568605117036-5fe5e7bab0b7%';

-- 3. Reemplazar photo-1594502879440-eca1c5d70adf (Audi RS7 SortOrder 5)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1552519507-da3b142c6e3d.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1552519507-da3b142c6e3d.jpg'
WHERE "Url" LIKE '%photo-1594502879440-eca1c5d70adf%';

-- 4. Reemplazar photo-1617531653332-bd46c24f2068 (Honda CR-V SortOrder 5)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1560958089-b8a1929cea89.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1560958089-b8a1929cea89.jpg'
WHERE "Url" LIKE '%photo-1617531653332-bd46c24f2068%';

-- 5. Reemplazar photo-1610768764270-790fbec18178 (Porsche 911 SortOrder 3)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1606016159991-dfe4f2746ad5.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1606016159991-dfe4f2746ad5.jpg'
WHERE "Url" LIKE '%photo-1610768764270-790fbec18178%';

-- 6. Reemplazar photo-1614162692292-7ac56d7f7f1e (Porsche 911 SortOrder 5)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1617788138017-80ad40651399.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1617788138017-80ad40651399.jpg'
WHERE "Url" LIKE '%photo-1614162692292-7ac56d7f7f1e%';

-- 7. Reemplazar photo-1611651338412-8403fa6e3599 (Tesla Model S SortOrder 4)
UPDATE vehicle_images 
SET "Url" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1618843479313-40f8afb4b4d8.jpg',
    "ThumbnailUrl" = 'https://okla-images-2026.s3.us-east-2.amazonaws.com/frontend/assets/vehicles/sale/photo-1618843479313-40f8afb4b4d8.jpg'
WHERE "Url" LIKE '%photo-1611651338412-8403fa6e3599%';

-- Verificación
SELECT 
    v."Make", 
    v."Model", 
    vi."SortOrder",
    substring(vi."Url" from 'photo-[^.]+\.jpg') as new_filename
FROM vehicles v
JOIN vehicle_images vi ON v."Id" = vi."VehicleId"
WHERE 
    vi."Url" LIKE '%photo-1536700503339%'
    OR vi."Url" LIKE '%photo-1549317661%'
    OR vi."Url" LIKE '%photo-1552519507%'
    OR vi."Url" LIKE '%photo-1560958089%'
    OR vi."Url" LIKE '%photo-1606016159%'
    OR vi."Url" LIKE '%photo-1617788138%'
    OR vi."Url" LIKE '%photo-1618843479%'
ORDER BY v."Make", vi."SortOrder";

COMMIT;
