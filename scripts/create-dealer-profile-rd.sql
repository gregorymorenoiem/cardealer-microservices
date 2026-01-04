-- Script SQL para crear el perfil de dealer en la base de datos
-- Usuario ID: f5fb27f0-ee1f-4817-ac96-832c35271f62

-- Insertar en la tabla Dealers
INSERT INTO "Dealers" (
    "Id", 
    "OwnerUserId", 
    "BusinessName", 
    "DealerType",
    "TaxId",
    "Website",
    "Description",
    "Address",
    "City",
    "State",
    "Country",
    "ZipCode",
    "Latitude",
    "Longitude",
    "Phone",
    "Email",
    "VerificationStatus",
    "TotalListings",
    "ActiveListings",
    "TotalSales",
    "AverageRating",
    "TotalReviews",
    "ResponseTimeMinutes",
    "IsActive",
    "AcceptsFinancing",
    "AcceptsTradeIn",
    "OffersWarranty",
    "HomeDelivery",
    "MaxListings",
    "IsFeatured",
    "CreatedAt",
    "UpdatedAt",
    "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid,
    'AutoVentas República Dominicana',
    1,
    'RNC-1-30-12345-6',
    'https://autoventas-rd.com',
    'Concesionario líder en República Dominicana con más de 20 años de experiencia. Ofrecemos vehículos de calidad con financiamiento y garantía extendida.',
    'Av. Winston Churchill #1515',
    'Santo Domingo',
    'Distrito Nacional',
    'DO',
    '10111',
    18.4861,
    -69.9312,
    '+1-809-555-0123',
    'autoventas.rd@cardealer.com',
    2,
    0,
    0,
    0,
    4.9,
    0,
    30,
    true,
    true,
    true,
    true,
    true,
    100,
    true,
    NOW(),
    NOW(),
    false
)
RETURNING "Id", "BusinessName";

-- Actualizar el usuario para asociarlo con el dealer
UPDATE "Users" 
SET "DealerId" = (SELECT "Id" FROM "Dealers" WHERE "OwnerUserId" = 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid),
    "UpdatedAt" = NOW()
WHERE "Id" = 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid;

-- Verificar
SELECT u."Id" as "UserId", u."Email", u."DealerId", d."BusinessName"
FROM "Users" u
LEFT JOIN "Dealers" d ON d."Id" = u."DealerId"
WHERE u."Id" = 'f5fb27f0-ee1f-4817-ac96-832c35271f62'::uuid;
