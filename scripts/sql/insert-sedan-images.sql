-- Agregar imágenes a los sedanes creados
INSERT INTO vehicle_images ("Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "IsPrimary", "SortOrder", "ImageType", "CreatedAt")
SELECT gen_random_uuid(), v."DealerId", v."Id", 
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'BMW' THEN 'photo-1555215695-3004980ad54e'
    WHEN v."Make" = 'Mercedes-Benz' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Audi' THEN 'photo-1606664515524-ed2f786a0bd6'
    WHEN v."Make" = 'Tesla' THEN 'photo-1560958089-b8a1929cea89'
    WHEN v."Make" = 'Lexus' THEN 'photo-1555215695-3004980ad54e'
    WHEN v."Make" = 'Honda' THEN 'photo-1533473359331-0135ef1b58bf'
    WHEN v."Make" = 'Toyota' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'Mazda' THEN 'photo-1580273916550-e323be2ae537'
    WHEN v."Make" = 'Hyundai' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Kia' THEN 'photo-1606664515524-ed2f786a0bd6'
  END || '?w=800',
  'https://images.unsplash.com/' || 
  CASE 
    WHEN v."Make" = 'BMW' THEN 'photo-1555215695-3004980ad54e'
    WHEN v."Make" = 'Mercedes-Benz' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Audi' THEN 'photo-1606664515524-ed2f786a0bd6'
    WHEN v."Make" = 'Tesla' THEN 'photo-1560958089-b8a1929cea89'
    WHEN v."Make" = 'Lexus' THEN 'photo-1555215695-3004980ad54e'
    WHEN v."Make" = 'Honda' THEN 'photo-1533473359331-0135ef1b58bf'
    WHEN v."Make" = 'Toyota' THEN 'photo-1621007947382-bb3c3994e3fb'
    WHEN v."Make" = 'Mazda' THEN 'photo-1580273916550-e323be2ae537'
    WHEN v."Make" = 'Hyundai' THEN 'photo-1618843479313-40f8afb4b4d8'
    WHEN v."Make" = 'Kia' THEN 'photo-1606664515524-ed2f786a0bd6'
  END || '?w=400',
  true, 1, 0, NOW()
FROM vehicles v
WHERE v."DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid 
  AND v."BodyStyle" = 'Sedan'
  AND NOT EXISTS (SELECT 1 FROM vehicle_images vi WHERE vi."VehicleId" = v."Id");

SELECT COUNT(*) as "Imagenes Sedanes" FROM vehicle_images vi
JOIN vehicles v ON vi."VehicleId" = v."Id"
WHERE v."DealerId" = '89de6284-dba0-46f1-8c6b-f2de21ec113b'::uuid AND v."BodyStyle" = 'Sedan';
