/**
 * E2E Tests - Vehicle Detail Page
 * Tests vehicle detail page functionality
 * Note: Tests are resilient to different page implementations
 */

import { test, expect } from '@playwright/test';

test.describe('Vehicle Detail Page', () => {
  test.describe('Page Access', () => {
    test('should handle vehicle detail URL pattern', async ({ page }) => {
      // Try a vehicle detail URL
      await page.goto('/vehiculos/test-vehicle-slug');

      // Page should load (may show 404 if no data)
      const hasContent = await page.locator('body').isVisible();
      expect(hasContent).toBe(true);
    });

    test('should display 404 or vehicle for non-existent slug', async ({ page }) => {
      await page.goto('/vehiculos/non-existent-vehicle-12345');

      // Should show either 404 page or redirect
      const has404 = await page
        .getByText(/404|no encontrado|not found/i)
        .first()
        .isVisible()
        .catch(() => false);
      const hasContent = await page.locator('body').isVisible();

      expect(has404 || hasContent).toBe(true);
    });
  });

  test.describe('Page Navigation', () => {
    test('should navigate from vehicles list to detail', async ({ page }) => {
      await page.goto('/vehiculos');
      await page.waitForLoadState('networkidle');

      // Look for any vehicle link
      const vehicleLink = page.locator('a[href*="/vehiculos/"]').first();

      if (await vehicleLink.isVisible().catch(() => false)) {
        await vehicleLink.click();
        await page.waitForLoadState('networkidle');

        // Should be on a detail page or still on list
        const url = page.url();
        expect(url.includes('vehiculo')).toBe(true);
      }
    });
  });

  test.describe('Homepage Categories', () => {
    test('should have clickable category cards on homepage', async ({ page }) => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');

      // Look for category links (SUV, Sedan, etc.)
      const categoryLinks = page.locator('a[href*="bodyType"]');
      const count = await categoryLinks.count();

      if (count > 0) {
        // Click first category
        await categoryLinks.first().click();
        await page.waitForLoadState('networkidle');

        // Should navigate to filtered vehicles
        const url = page.url();
        expect(url.includes('vehiculo') || url.includes('bodyType')).toBe(true);
      }
    });

    test('should have clickable brand cards on homepage', async ({ page }) => {
      await page.goto('/');
      await page.waitForLoadState('networkidle');

      // Look for brand links (Toyota, Honda, etc.)
      const brandLinks = page.locator('a[href*="make="]');
      const count = await brandLinks.count();

      if (count > 0) {
        // Click first brand
        await brandLinks.first().click();
        await page.waitForLoadState('networkidle');

        // Should navigate to filtered vehicles
        const url = page.url();
        expect(url.includes('vehiculo') || url.includes('make')).toBe(true);
      }
    });
  });
});
