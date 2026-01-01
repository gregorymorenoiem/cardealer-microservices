-- Agregar DealerId a RoleService
-- Tablas: Roles, Permissions, RolePermissions

ALTER TABLE "Roles" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Roles_DealerId" ON "Roles"("DealerId");

ALTER TABLE "Permissions" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Permissions_DealerId" ON "Permissions"("DealerId");

ALTER TABLE "RolePermissions" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_RolePermissions_DealerId" ON "RolePermissions"("DealerId");
