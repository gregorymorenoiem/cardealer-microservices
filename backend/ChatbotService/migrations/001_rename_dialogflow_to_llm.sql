-- ============================================================
-- Migration 001: Rename Dialogflow references to LLM-aligned names
-- Date: 2026-02-17
-- Description: Removes all Dialogflow naming debt. Renames tables,
--              columns, indexes, and constraints to reflect the
--              LLM-based architecture that replaced Dialogflow.
-- ============================================================
-- IMPORTANT: Run this ONCE on any existing chatbotservice database.
--            New databases created via EnsureCreated() will already
--            have the correct names.
-- ============================================================

BEGIN;

-- 1. RENAME TABLE
ALTER TABLE IF EXISTS dialogflow_intents RENAME TO chatbot_intents;

-- 2. RENAME COLUMNS: chatbot_configurations
ALTER TABLE chatbot_configurations RENAME COLUMN "DialogflowProjectId" TO "LlmProjectId";
ALTER TABLE chatbot_configurations RENAME COLUMN "DialogflowAgentId" TO "LlmModelId";
ALTER TABLE chatbot_configurations RENAME COLUMN "DialogflowLanguageCode" TO "LanguageCode";
ALTER TABLE chatbot_configurations RENAME COLUMN "DialogflowCredentialsJson" TO "SystemPromptText";

-- 3. RENAME COLUMNS: chat_messages
ALTER TABLE chat_messages RENAME COLUMN "DialogflowIntentName" TO "IntentName";
ALTER TABLE chat_messages RENAME COLUMN "DialogflowIntentId" TO "LlmIntentId";
ALTER TABLE chat_messages RENAME COLUMN "DialogflowParameters" TO "IntentParameters";

-- 4. RENAME COLUMNS: quick_responses
ALTER TABLE quick_responses RENAME COLUMN "BypassDialogflow" TO "BypassLlm";

-- 5. RENAME COLUMNS: chatbot_intents (formerly dialogflow_intents)
ALTER TABLE chatbot_intents RENAME COLUMN "DialogflowIntentId" TO "IntentId";

-- 6. RENAME INDEXES (EF Core convention names)
ALTER INDEX IF EXISTS "PK_dialogflow_intents" RENAME TO "PK_chatbot_intents";
ALTER INDEX IF EXISTS "IX_dialogflow_intents_Category" RENAME TO "IX_chatbot_intents_Category";
ALTER INDEX IF EXISTS "IX_dialogflow_intents_ChatbotConfigurationId" RENAME TO "IX_chatbot_intents_ChatbotConfigurationId";
ALTER INDEX IF EXISTS "IX_dialogflow_intents_DialogflowIntentId" RENAME TO "IX_chatbot_intents_IntentId";

-- 7. RENAME FK CONSTRAINTS
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname LIKE 'FK_dialogflow_intents%') THEN
        EXECUTE format(
            'ALTER TABLE chatbot_intents RENAME CONSTRAINT %I TO %I',
            (SELECT conname FROM pg_constraint WHERE conname LIKE 'FK_dialogflow_intents%' LIMIT 1),
            'FK_chatbot_intents_chatbot_configurations_ChatbotConfigurationI'
        );
    END IF;
END $$;

COMMIT;
