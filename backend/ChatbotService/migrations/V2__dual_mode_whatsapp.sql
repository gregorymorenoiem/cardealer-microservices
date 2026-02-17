-- ============================================================
-- Migration V2: Dual-Mode Chat Engine + WhatsApp + RAG (pgvector)
-- ChatbotService - OKLA Marketplace
-- ============================================================

-- 1. Enable pgvector extension for RAG embeddings
CREATE EXTENSION IF NOT EXISTS vector;

-- 2. Add dual-mode and handoff columns to chat_sessions
ALTER TABLE chat_sessions
    ADD COLUMN IF NOT EXISTS chat_mode INTEGER NOT NULL DEFAULT 3,         -- 1=SingleVehicle, 2=DealerInventory, 3=General
    ADD COLUMN IF NOT EXISTS vehicle_id UUID,                              -- FK to vehicle for SingleVehicle mode
    ADD COLUMN IF NOT EXISTS dealer_id UUID,                               -- FK to dealer for DealerInventory mode
    ADD COLUMN IF NOT EXISTS handoff_status INTEGER NOT NULL DEFAULT 0,    -- 0=BotActive, 1=HumanActive, 2=PendingHuman, 3=ReturnedToBot
    ADD COLUMN IF NOT EXISTS handoff_agent_id UUID,                        -- Agent who took over
    ADD COLUMN IF NOT EXISTS handoff_agent_name VARCHAR(200),
    ADD COLUMN IF NOT EXISTS handoff_at TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS handoff_reason VARCHAR(500);

-- 3. Indexes for new columns
CREATE INDEX IF NOT EXISTS ix_chat_sessions_chat_mode ON chat_sessions (chat_mode);
CREATE INDEX IF NOT EXISTS ix_chat_sessions_vehicle_id ON chat_sessions (vehicle_id);
CREATE INDEX IF NOT EXISTS ix_chat_sessions_dealer_id ON chat_sessions (dealer_id);
CREATE INDEX IF NOT EXISTS ix_chat_sessions_handoff_status ON chat_sessions (handoff_status);

-- 4. Vehicle embeddings table for RAG (pgvector)
CREATE TABLE IF NOT EXISTS vehicle_embeddings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vehicle_id UUID NOT NULL,
    dealer_id UUID NOT NULL,
    embedding vector(384) NOT NULL,          -- all-MiniLM-L6-v2 = 384 dimensions
    embedding_text TEXT NOT NULL,            -- The text that was embedded
    metadata JSONB NOT NULL DEFAULT '{}',   -- VehicleEmbeddingMetadata as JSON
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 5. Indexes for vehicle_embeddings
CREATE UNIQUE INDEX IF NOT EXISTS ix_vehicle_embeddings_vehicle_id ON vehicle_embeddings (vehicle_id);
CREATE INDEX IF NOT EXISTS ix_vehicle_embeddings_dealer_id ON vehicle_embeddings (dealer_id);

-- 6. IVFFlat index for vector similarity search (fast ANN)
-- Note: Requires at least ~100 rows for IVFFlat to be effective
-- For small datasets, pgvector falls back to exact search automatically
CREATE INDEX IF NOT EXISTS ix_vehicle_embeddings_vector ON vehicle_embeddings
    USING ivfflat (embedding vector_cosine_ops)
    WITH (lists = 100);

-- 7. Add session_type value for WhatsApp (if enum doesn't include it yet)
-- The SessionType enum in code handles this, but add comment for clarity:
-- SessionType: 0=Web, 1=Mobile, 2=WhatsApp, 3=Facebook, 4=API

-- ============================================================
-- Rollback script (run manually if needed):
-- ============================================================
-- DROP INDEX IF EXISTS ix_vehicle_embeddings_vector;
-- DROP INDEX IF EXISTS ix_vehicle_embeddings_dealer_id;
-- DROP INDEX IF EXISTS ix_vehicle_embeddings_vehicle_id;
-- DROP TABLE IF EXISTS vehicle_embeddings;
-- ALTER TABLE chat_sessions
--     DROP COLUMN IF EXISTS chat_mode,
--     DROP COLUMN IF EXISTS vehicle_id,
--     DROP COLUMN IF EXISTS dealer_id,
--     DROP COLUMN IF EXISTS handoff_status,
--     DROP COLUMN IF EXISTS handoff_agent_id,
--     DROP COLUMN IF EXISTS handoff_agent_name,
--     DROP COLUMN IF EXISTS handoff_at,
--     DROP COLUMN IF EXISTS handoff_reason;
-- DROP EXTENSION IF EXISTS vector;
