-- Agregar DealerId a ErrorService
-- Tablas: error_logs

ALTER TABLE "error_logs" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_error_logs_DealerId" ON "error_logs"("DealerId");
