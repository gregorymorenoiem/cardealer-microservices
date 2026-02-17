-- Add DealerId column to products table
ALTER TABLE products ADD COLUMN "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX "IX_products_DealerId" ON products("DealerId");

-- Add DealerId column to product_images table  
ALTER TABLE product_images ADD COLUMN "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX "IX_product_images_DealerId" ON product_images("DealerId");

-- Add DealerId column to categories table
ALTER TABLE categories ADD COLUMN "DealerId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
CREATE INDEX "IX_categories_DealerId" ON categories("DealerId");

-- Note: ProductCustomField does NOT implement ITenantEntity, so no DealerId needed
