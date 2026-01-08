-- ============================================
-- FULL-TEXT SEARCH OPTIMIZATION
-- VehiclesSaleService - PostgreSQL
-- ============================================

-- Agregar columna de búsqueda full-text (tsvector)
ALTER TABLE vehicles 
ADD COLUMN IF NOT EXISTS search_vector tsvector;

-- Crear índice GIN para búsqueda rápida
CREATE INDEX IF NOT EXISTS idx_vehicles_search_vector 
ON vehicles USING GIN (search_vector);

-- Función para actualizar el vector de búsqueda
CREATE OR REPLACE FUNCTION vehicles_search_vector_update() RETURNS trigger AS $$
BEGIN
    NEW.search_vector :=
        setweight(to_tsvector('english', coalesce(NEW."Title", '')), 'A') ||
        setweight(to_tsvector('english', coalesce(NEW."Make", '')), 'A') ||
        setweight(to_tsvector('english', coalesce(NEW."Model", '')), 'A') ||
        setweight(to_tsvector('english', coalesce(NEW."Description", '')), 'B') ||
        setweight(to_tsvector('english', coalesce(NEW."Trim", '')), 'C') ||
        setweight(to_tsvector('english', coalesce(NEW."City", '')), 'C') ||
        setweight(to_tsvector('english', coalesce(NEW."State", '')), 'C');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger para auto-actualizar el vector en INSERT/UPDATE
DROP TRIGGER IF EXISTS vehicles_search_vector_trigger ON vehicles;
CREATE TRIGGER vehicles_search_vector_trigger
    BEFORE INSERT OR UPDATE ON vehicles
    FOR EACH ROW
    EXECUTE FUNCTION vehicles_search_vector_update();

-- Actualizar vectores existentes
UPDATE vehicles SET search_vector = 
    setweight(to_tsvector('english', coalesce("Title", '')), 'A') ||
    setweight(to_tsvector('english', coalesce("Make", '')), 'A') ||
    setweight(to_tsvector('english', coalesce("Model", '')), 'A') ||
    setweight(to_tsvector('english', coalesce("Description", '')), 'B') ||
    setweight(to_tsvector('english', coalesce("Trim", '')), 'C') ||
    setweight(to_tsvector('english', coalesce("City", '')), 'C') ||
    setweight(to_tsvector('english', coalesce("State", '')), 'C');

-- Índices adicionales para filtros comunes
CREATE INDEX IF NOT EXISTS idx_vehicles_make_model ON vehicles ("Make", "Model");
CREATE INDEX IF NOT EXISTS idx_vehicles_year_price ON vehicles ("Year", "Price");
CREATE INDEX IF NOT EXISTS idx_vehicles_status_created ON vehicles ("Status", "CreatedAt");
CREATE INDEX IF NOT EXISTS idx_vehicles_location ON vehicles ("State", "City");

-- Índice para favoritos (ya existe en Favorites entity config pero por si acaso)
CREATE INDEX IF NOT EXISTS idx_favorites_user_vehicle ON favorites ("UserId", "VehicleId");

-- Comentarios para documentación
COMMENT ON COLUMN vehicles.search_vector IS 'Full-text search vector with weighted fields: A=Title/Make/Model, B=Description, C=Trim/Location';
COMMENT ON INDEX idx_vehicles_search_vector IS 'GIN index for fast full-text search using tsvector';

-- Verificación
SELECT 
    'Full-text search optimization completed' as status,
    COUNT(*) as total_vehicles,
    COUNT(search_vector) as vehicles_with_search_vector
FROM vehicles;
