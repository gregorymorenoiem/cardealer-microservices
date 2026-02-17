-- Script: clean-db.sql
-- Descripción: Limpia todos los datos de la base de datos
-- Uso: psql -h localhost -U postgres -d cardealer -f clean-db.sql
-- Precaución: ⚠️ BORRA TODOS LOS DATOS

-- Variables
-- Cambiar estas según necesidad:
-- psql -h localhost -U postgres -d cardealer -v ON_ERROR_STOP=1 -f clean-db.sql

\echo '🗑️  Iniciando limpieza de base de datos...'

BEGIN TRANSACTION;

-- Deshabilitar foreign key constraints durante limpieza
SET session_replication_role = 'replica';

-- TRUNCATE en orden de dependencias (tables que NO tienen FK primero)
\echo '  ├─ Limpiando vehicle_images...'
TRUNCATE TABLE IF EXISTS vehicle_images CASCADE;

\echo '  ├─ Limpiando vehicles...'
TRUNCATE TABLE IF EXISTS vehicles CASCADE;

\echo '  ├─ Limpiando vehicle_models...'
TRUNCATE TABLE IF EXISTS vehicle_models CASCADE;

\echo '  ├─ Limpiando vehicle_makes...'
TRUNCATE TABLE IF EXISTS vehicle_makes CASCADE;

\echo '  ├─ Limpiando categories...'
TRUNCATE TABLE IF EXISTS categories CASCADE;

\echo '  ├─ Limpiando dealers...'
TRUNCATE TABLE IF EXISTS dealers CASCADE;

\echo '  ├─ Limpiando dealer_locations...'
TRUNCATE TABLE IF EXISTS dealer_locations CASCADE;

\echo '  └─ Limpiando users...'
TRUNCATE TABLE IF EXISTS users CASCADE;

-- Re-habilitar foreign key constraints
SET session_replication_role = 'origin';

-- RESET de SEQUENCES (auto-increment)
\echo ''
\echo '  Reseteando sequences...'
ALTER SEQUENCE IF EXISTS users_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS dealers_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS dealer_locations_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS vehicles_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS vehicle_images_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS vehicle_makes_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS vehicle_models_id_seq RESTART WITH 1;
ALTER SEQUENCE IF EXISTS categories_id_seq RESTART WITH 1;

COMMIT;

\echo ''
\echo '✅ Base de datos limpiada exitosamente!'
\echo ''

-- VERIFICACIÓN FINAL
\echo '📊 Estado actual:'
SELECT 
    (SELECT COUNT(*) FROM users) as usuarios,
    (SELECT COUNT(*) FROM dealers) as dealers,
    (SELECT COUNT(*) FROM vehicles) as vehiculos,
    (SELECT COUNT(*) FROM vehicle_images) as imagenes;

\echo ''
\echo 'Tabla | Registros'
\echo '------+-----------'
SELECT table_name, 0 as registros 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_type = 'BASE TABLE'
ORDER BY table_name;
