-- =============================================================================
-- MIGRACIONES PARA AGREGAR DealerId A SERVICIOS MULTI-TENANT
-- Fecha: 31 Diciembre 2025
-- =============================================================================

-- ============ UserService ============
\c userservice

-- Tabla ApplicationUsers
ALTER TABLE "ApplicationUsers" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_ApplicationUsers_DealerId" ON "ApplicationUsers"("DealerId");

-- Tabla Dealers (esta tabla representa dealers, pero también necesita DealerId para multi-tenancy)
ALTER TABLE "Dealers" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Dealers_DealerId" ON "Dealers"("DealerId");

-- ============ RoleService ============
\c roleservice

-- Tabla Roles
ALTER TABLE "Roles" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Roles_DealerId" ON "Roles"("DealerId");

-- Tabla Permissions
ALTER TABLE "Permissions" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Permissions_DealerId" ON "Permissions"("DealerId");

-- ============ MediaService ============
\c mediaservice

-- Tabla MediaFiles
ALTER TABLE "MediaFiles" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_MediaFiles_DealerId" ON "MediaFiles"("DealerId");

-- Tabla MediaFolders
ALTER TABLE "MediaFolders" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_MediaFolders_DealerId" ON "MediaFolders"("DealerId");

-- ============ ErrorService ============
\c errorservice

-- Tabla Errors
ALTER TABLE "Errors" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Errors_DealerId" ON "Errors"("DealerId");

-- ============ ReportsService ============
\c reportsservice

-- Tabla Reports
ALTER TABLE "Reports" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Reports_DealerId" ON "Reports"("DealerId");

-- ============ BillingService ============
\c billingservice

-- Tabla Invoices
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Invoices_DealerId" ON "Invoices"("DealerId");

-- Tabla Payments
ALTER TABLE "Payments" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Payments_DealerId" ON "Payments"("DealerId");

-- ============ FinanceService ============
\c financeservice

-- Tabla Accounts
ALTER TABLE "Accounts" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Accounts_DealerId" ON "Accounts"("DealerId");

-- Tabla Transactions
ALTER TABLE "Transactions" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Transactions_DealerId" ON "Transactions"("DealerId");

-- ============ InvoicingService ============
\c invoicingservice

-- Tabla Invoices
ALTER TABLE "Invoices" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Invoices_DealerId" ON "Invoices"("DealerId");

-- Tabla InvoiceItems
ALTER TABLE "InvoiceItems" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_InvoiceItems_DealerId" ON "InvoiceItems"("DealerId");

-- ============ CRMService ============
\c crmservice

-- Tabla Contacts
ALTER TABLE "Contacts" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Contacts_DealerId" ON "Contacts"("DealerId");

-- Tabla Opportunities
ALTER TABLE "Opportunities" ADD COLUMN IF NOT EXISTS "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX IF NOT EXISTS "IX_Opportunities_DealerId" ON "Opportunities"("DealerId");

-- =============================================================================
-- FIN DE MIGRACIONES
-- =============================================================================
