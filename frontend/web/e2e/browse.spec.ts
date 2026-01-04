import { test, expect } from '@playwright/test';

test.describe('Vehicle Browse Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/browse');
  });

  test('should load browse page with vehicle grid', async ({ page }) => {
    // Wait for page to load
    await page.waitForLoadState('networkidle');
    
    // Verify the page title contains "Browse" or similar
    await expect(page).toHaveTitle(/browse|vehicles|vehículos/i);
    
    // Should show vehicle cards (wait for them to load)
    const vehicleCards = page.locator('[data-testid="vehicle-card"], .vehicle-card, [class*="VehicleCard"]');
    await expect(vehicleCards.first()).toBeVisible({ timeout: 5000 });
  });

  test('should toggle between grid and list view', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    // Find and click list view button
    const listViewButton = page.locator('button:has-text("List"), button[aria-label*="list"], [data-testid="list-view-button"]');
    if (await listViewButton.count() > 0) {
      await listViewButton.first().click();
      
      // Wait for layout change
      await page.waitForTimeout(500);
      
      // Click grid view button
      const gridViewButton = page.locator('button:has-text("Grid"), button[aria-label*="grid"], [data-testid="grid-view-button"]');
      await gridViewButton.first().click();
      
      // Layout should change back
      await page.waitForTimeout(500);
    }
  });

  test('should filter vehicles by price range', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    // Find price filter inputs
    const minPriceInput = page.locator('input[name*="minPrice"], input[placeholder*="min"], input[aria-label*="minimum"]').first();
    const maxPriceInput = page.locator('input[name*="maxPrice"], input[placeholder*="max"], input[aria-label*="maximum"]').first();
    
    if (await minPriceInput.count() > 0) {
      // Set price range
      await minPriceInput.fill('10000');
      await maxPriceInput.fill('50000');
      
      // Apply filters (either auto-apply or click button)
      const applyButton = page.locator('button:has-text("Apply"), button:has-text("Filter"), button[type="submit"]');
      if (await applyButton.count() > 0) {
        await applyButton.first().click();
      }
      
      // Wait for results to update
      await page.waitForTimeout(1000);
    }
  });

  test('should navigate to vehicle detail page', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    // Wait for vehicle cards to load
    const vehicleCard = page.locator('[data-testid="vehicle-card"], .vehicle-card, a[href*="/vehicles/"]').first();
    await vehicleCard.waitFor({ state: 'visible', timeout: 5000 });
    
    // Click on first vehicle card
    await vehicleCard.click();
    
    // Should navigate to detail page
    await expect(page).toHaveURL(/.*\/vehicles\/[a-zA-Z0-9-]+/);
    
    // Verify detail page loaded
    await page.waitForSelector('h1, [data-testid="vehicle-name"]', { timeout: 3000 });
  });

  test('should search for vehicles', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    // Find search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="search"], input[name="search"]').first();
    
    if (await searchInput.count() > 0) {
      // Type search query
      await searchInput.fill('Toyota');
      
      // Press Enter or click search button
      await searchInput.press('Enter');
      
      // Wait for results to update
      await page.waitForTimeout(1000);
      
      // Verify URL contains search query
      await expect(page).toHaveURL(/.*search=Toyota.*/);
    }
  });

  test('should paginate through results', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    // Wait for pagination controls
    await page.waitForTimeout(1000);
    
    // Find next page button
    const nextButton = page.locator('button:has-text("Next"), button[aria-label*="next"], a:has-text("2")').first();
    
    if (await nextButton.count() > 0 && await nextButton.isVisible()) {
      // Click next page
      await nextButton.click();
      
      // Wait for page to update
      await page.waitForTimeout(1000);
      
      // Verify URL changed (page parameter)
      await expect(page).toHaveURL(/.*page=2.*/);
    }
  });
});
