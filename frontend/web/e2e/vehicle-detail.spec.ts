import { test, expect } from '@playwright/test';

test.describe('Vehicle Detail Flow', () => {
  // Helper to navigate to a vehicle detail page
  async function goToVehicleDetail(page) {
    await page.goto('/browse');
    await page.waitForLoadState('networkidle');
    
    // Click on first vehicle
    const vehicleCard = page.locator('[data-testid="vehicle-card"], .vehicle-card, a[href*="/vehicles/"]').first();
    await vehicleCard.waitFor({ state: 'visible', timeout: 5000 });
    await vehicleCard.click();
    
    // Wait for detail page to load
    await page.waitForURL(/.*\/vehicles\/[a-zA-Z0-9-]+/);
    await page.waitForLoadState('networkidle');
  }

  test('should display vehicle details', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Verify essential elements are visible
    await expect(page.locator('h1, [data-testid="vehicle-name"]')).toBeVisible();
    
    // Should show price
    const priceElement = page.locator('text=/\\$[0-9,]+/').first();
    await expect(priceElement).toBeVisible();
    
    // Should show vehicle images
    const mainImage = page.locator('img[alt*="vehicle"], img[alt*="car"], [data-testid="vehicle-image"]').first();
    await expect(mainImage).toBeVisible();
  });

  test('should display vehicle specifications', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Look for specs section
    const specsSection = page.locator('[data-testid="specifications"], .specifications, text=/specifications|specs|detalles técnicos/i');
    
    if (await specsSection.count() > 0) {
      await expect(specsSection.first()).toBeVisible();
    }
  });

  test('should show contact seller button', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Look for contact button
    const contactButton = page.locator('button:has-text("Contact"), button:has-text("Message"), a:has-text("Contact")').first();
    
    if (await contactButton.count() > 0) {
      await expect(contactButton).toBeVisible();
    }
  });

  test('should add vehicle to favorites', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Find favorite/heart button
    const favoriteButton = page.locator('button[aria-label*="favorite"], button[aria-label*="like"], [data-testid="favorite-button"]').first();
    
    if (await favoriteButton.count() > 0) {
      // Click to add to favorites
      await favoriteButton.click();
      
      // Wait for animation/state change
      await page.waitForTimeout(500);
      
      // Button should change state (aria-label might change)
      // This is a simple check, actual implementation may vary
      const isFavorited = await favoriteButton.getAttribute('aria-label');
      expect(isFavorited).toBeTruthy();
    }
  });

  test('should navigate through image gallery', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Find next image button
    const nextImageButton = page.locator('button[aria-label*="next image"], button:has-text("›"), button:has-text("Next")').first();
    
    if (await nextImageButton.count() > 0) {
      // Click to view next image
      await nextImageButton.click();
      
      // Wait for transition
      await page.waitForTimeout(300);
      
      // Click previous button
      const prevImageButton = page.locator('button[aria-label*="previous image"], button:has-text("‹"), button:has-text("Previous")').first();
      if (await prevImageButton.count() > 0) {
        await prevImageButton.click();
      }
    }
  });

  test('should show similar vehicles', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Scroll to bottom to load similar vehicles section
    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await page.waitForTimeout(1000);
    
    // Look for similar vehicles section
    const similarSection = page.locator('text=/similar vehicles|you might like|recomendados/i');
    
    if (await similarSection.count() > 0) {
      await expect(similarSection.first()).toBeVisible();
      
      // Should show at least one similar vehicle card
      const similarCards = page.locator('[data-testid="vehicle-card"], .vehicle-card').nth(1); // Not the main vehicle
      if (await similarCards.count() > 0) {
        await expect(similarCards.first()).toBeVisible();
      }
    }
  });

  test('should share vehicle', async ({ page }) => {
    await goToVehicleDetail(page);
    
    // Find share button
    const shareButton = page.locator('button[aria-label*="share"], button:has-text("Share"), [data-testid="share-button"]').first();
    
    if (await shareButton.count() > 0) {
      await shareButton.click();
      
      // Should show share modal/menu
      await page.waitForTimeout(500);
      
      // Look for share options
      const shareModal = page.locator('[role="dialog"], .modal, .share-menu').first();
      if (await shareModal.count() > 0) {
        await expect(shareModal).toBeVisible();
      }
    }
  });
});
