-- 🔍 Script para verificar y corregir AccountType de usuarios dealer
-- Problema: Los usuarios dealer tienen accountType 'individual' en lugar de 'dealer'

-- 1. Verificar estado actual de los usuarios dealer
SELECT 
    email,
    account_type,
    full_name,
    id,
    created_at
FROM users 
WHERE email IN (
    'dealer.free@cardealer.com',
    'dealer.basic@cardealer.com', 
    'dealer.pro@cardealer.com',
    'dealer.enterprise@cardealer.com'
);

-- 2. Verificar el enum de account_type (para saber qué valores usar)
SELECT enum_range(NULL::account_type);

-- 3. Corregir los AccountType de los usuarios dealer
-- Cambiar de 'individual' (1) a 'dealer' (2)

UPDATE users 
SET account_type = 'dealer'  -- O usar el valor numérico correspondiente según el enum
WHERE email IN (
    'dealer.free@cardealer.com',
    'dealer.basic@cardealer.com', 
    'dealer.pro@cardealer.com',
    'dealer.enterprise@cardealer.com'
) AND account_type != 'dealer';

-- 4. Verificar que el cambio se aplicó correctamente
SELECT 
    email,
    account_type,
    full_name,
    id
FROM users 
WHERE email IN (
    'dealer.free@cardealer.com',
    'dealer.basic@cardealer.com', 
    'dealer.pro@cardealer.com',
    'dealer.enterprise@cardealer.com'
);

-- 5. (OPCIONAL) Verificar si existe tabla de dealers relacionada
SELECT * FROM information_schema.tables WHERE table_name LIKE '%dealer%';

-- 6. (OPCIONAL) Si existe tabla dealers, verificar relación
-- SELECT 
--     u.email,
--     u.account_type,
--     d.business_name,
--     d.status
-- FROM users u
-- LEFT JOIN dealers d ON u.id = d.user_id
-- WHERE u.email IN (
--     'dealer.free@cardealer.com',
--     'dealer.basic@cardealer.com', 
--     'dealer.pro@cardealer.com',
--     'dealer.enterprise@cardealer.com'
-- );