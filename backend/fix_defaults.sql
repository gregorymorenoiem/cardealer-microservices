-- Fix default values for boolean columns
ALTER TABLE "Users" ALTER COLUMN "LockoutEnabled" SET DEFAULT true;
ALTER TABLE "Users" ALTER COLUMN "TwoFactorEnabled" SET DEFAULT false;
ALTER TABLE "Users" ALTER COLUMN "EmailConfirmed" SET DEFAULT false;
ALTER TABLE "Users" ALTER COLUMN "PhoneNumberConfirmed" SET DEFAULT false;
ALTER TABLE "Users" ALTER COLUMN "AccessFailedCount" SET DEFAULT 0;
