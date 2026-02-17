-- ClickHouse Schema para Event Tracking Service
-- ClickHouse es óptimo para analytics de alto volumen y agregaciones rápidas

-- Tabla principal de eventos
CREATE TABLE IF NOT EXISTS tracked_events
(
    id UUID,
    event_type String,
    timestamp DateTime64(3, 'UTC'),
    user_id Nullable(String),
    session_id String,
    ip_address String,
    user_agent String,
    referrer Nullable(String),
    current_url String,
    device_type String,
    browser String,
    operating_system String,
    country Nullable(String),
    city Nullable(String),
    event_data Nullable(String), -- JSON string con data específica del evento
    source Nullable(String),
    campaign Nullable(String),
    medium Nullable(String),
    content Nullable(String)
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp) -- Particionar por mes
ORDER BY (event_type, timestamp, session_id)
TTL timestamp + INTERVAL 90 DAY -- Auto-delete después de 90 días
SETTINGS index_granularity = 8192;

-- Tabla de archivo para eventos antiguos (retención indefinida)
CREATE TABLE IF NOT EXISTS tracked_events_archive
(
    id UUID,
    event_type String,
    timestamp DateTime64(3, 'UTC'),
    user_id Nullable(String),
    session_id String,
    ip_address String,
    user_agent String,
    referrer Nullable(String),
    current_url String,
    device_type String,
    browser String,
    operating_system String,
    country Nullable(String),
    city Nullable(String),
    event_data Nullable(String),
    source Nullable(String),
    campaign Nullable(String),
    medium Nullable(String),
    content Nullable(String)
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(timestamp)
ORDER BY (event_type, timestamp, session_id)
SETTINGS index_granularity = 8192;

-- Índices para búsquedas comunes
-- ClickHouse usa skip indexes para mejorar performance

-- Índice para búsquedas por usuario
ALTER TABLE tracked_events ADD INDEX idx_user_id user_id TYPE bloom_filter GRANULARITY 1;

-- Índice para búsquedas por sesión
ALTER TABLE tracked_events ADD INDEX idx_session_id session_id TYPE bloom_filter GRANULARITY 1;

-- Índice para filtrar por país
ALTER TABLE tracked_events ADD INDEX idx_country country TYPE bloom_filter GRANULARITY 1;

-- Índice para filtrar por tipo de dispositivo
ALTER TABLE tracked_events ADD INDEX idx_device_type device_type TYPE bloom_filter GRANULARITY 1;

-- Vista materializada para conteo rápido de eventos por tipo y día
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_events_by_type_daily
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(event_date)
ORDER BY (event_type, event_date)
AS
SELECT
    event_type,
    toDate(timestamp) AS event_date,
    count() AS event_count,
    uniq(session_id) AS unique_sessions,
    uniq(user_id) AS unique_users
FROM tracked_events
GROUP BY event_type, event_date;

-- Vista materializada para top búsquedas
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_top_searches
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(search_date)
ORDER BY (search_query, search_date)
AS
SELECT
    JSONExtractString(event_data, 'SearchQuery') AS search_query,
    toDate(timestamp) AS search_date,
    count() AS search_count,
    avg(JSONExtractInt(event_data, 'ResultsCount')) AS avg_results,
    countIf(JSONExtractInt(event_data, 'ClickedPosition') > 0) AS clicks
FROM tracked_events
WHERE event_type = 'Search'
GROUP BY search_query, search_date;

-- Vista materializada para vehículos más vistos
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_vehicle_views
ENGINE = SummingMergeTree()
PARTITION BY toYYYYMM(view_date)
ORDER BY (vehicle_id, view_date)
AS
SELECT
    JSONExtractString(event_data, 'VehicleId') AS vehicle_id,
    JSONExtractString(event_data, 'VehicleTitle') AS vehicle_title,
    toDate(timestamp) AS view_date,
    count() AS view_count,
    avg(JSONExtractInt(event_data, 'TimeSpentSeconds')) AS avg_time_spent,
    countIf(JSONExtractInt(event_data, 'ClickedContact') = 1) AS contact_clicks,
    countIf(JSONExtractInt(event_data, 'AddedToFavorites') = 1) AS favorite_adds
FROM tracked_events
WHERE event_type = 'VehicleView'
GROUP BY vehicle_id, vehicle_title, view_date;

-- Query de ejemplo para analytics dashboard
-- Top 20 búsquedas de los últimos 30 días:
/*
SELECT 
    search_query,
    sum(search_count) AS total_searches,
    avg(avg_results) AS avg_results,
    sum(clicks) / sum(search_count) AS ctr
FROM mv_top_searches
WHERE search_date >= today() - 30
GROUP BY search_query
ORDER BY total_searches DESC
LIMIT 20;
*/

-- Top 20 vehículos más vistos con métricas:
/*
SELECT 
    vehicle_id,
    any(vehicle_title) AS title,
    sum(view_count) AS total_views,
    avg(avg_time_spent) AS avg_time,
    sum(contact_clicks) AS contacts,
    sum(favorite_adds) AS favorites,
    (sum(contact_clicks) / sum(view_count)) AS conversion_rate
FROM mv_vehicle_views
WHERE view_date >= today() - 30
GROUP BY vehicle_id
ORDER BY total_views DESC
LIMIT 20;
*/

-- Conteo de eventos por tipo en los últimos 7 días:
/*
SELECT 
    event_type,
    sum(event_count) AS total_events,
    sum(unique_sessions) AS sessions,
    sum(unique_users) AS users
FROM mv_events_by_type_daily
WHERE event_date >= today() - 7
GROUP BY event_type
ORDER BY total_events DESC;
*/
