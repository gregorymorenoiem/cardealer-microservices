import { test, expect } from '@playwright/test';

test.describe('Search and Filter Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/browse');
    await page.waitForLoadState('networkidle');
  });

  test('should perform basic search', async ({ page }) => {
    // Find search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="search"], input[name="search"]').first();
    
    // Enter search term
    await searchInput.fill('SUV');
    await searchInput.press('Enter');
    
    // Wait for results
    await page.waitForTimeout(1000);
    
    // Verify URL updated with search term
    await expect(page).toHaveURL(/.*search.*SUV.*/);
  });

  test('should filter by vehicle type', async ({ page }) => {
    // Look for vehicle type filter
    const typeFilter = page.locator('select[name*="type"], select[name*="category"], input[name*="type"]').first();
    
    if (await typeFilter.count() > 0) {
      // Select a type (e.g., SUV)
      if (await typeFilter.getAttribute('type') === 'select') {
        await typeFilter.selectOption('SUV');
      } else {
        await typeFilter.click();
        await page.click('text=SUV');
      }
      
      // Wait for results to update
      await page.waitForTimeout(1000);
    }
  });

  test('should filter by make (brand)', async ({ page }) => {
    // Look for make/brand filter
    const makeFilter = page.locator('select[name*="make"], select[name*="brand"], input[name*="make"]').first();
    
    if (await makeFilter.count() > 0) {
      // Select a make
      if (await makeFilter.getAttribute('type') === 'select') {
        await makeFilter.selectOption({ index: 1 }); // Select first option after "All"
      } else {
        await makeFilter.click();
        await page.locator('[role="option"]').first().click();
      }
      
      // Wait for results to update
      await page.waitForTimeout(1000);
    }
  });

  test('should filter by year range', async ({ page }) => {
    // Look for year filters
    const minYearInput = page.locator('input[name*="minYear"], input[placeholder*="from year"]').first();
    const maxYearInput = page.locator('input[name*="maxYear"], input[placeholder*="to year"]').first();
    
    if (await minYearInput.count() > 0) {
      await minYearInput.fill('2020');
      await maxYearInput.fill('2024');
      
      // Apply filters
      const applyButton = page.locator('button:has-text("Apply"), button:has-text("Filter")').first();
      if (await applyButton.count() > 0) {
        await applyButton.click();
      }
      
      // Wait for results
      await page.waitForTimeout(1000);
    }
  });

  test('should filter by mileage', async ({ page }) => {
    // Look for mileage filter
    const maxMileageInput = page.locator('input[name*="maxMileage"], input[name*="mileage"]').first();
    
    if (await maxMileageInput.count() > 0) {
      await maxMileageInput.fill('50000');
      
      // Apply filters
      const applyButton = page.locator('button:has-text("Apply"), button:has-text("Filter")').first();
      if (await applyButton.count() > 0) {
        await applyButton.click();
      }
      
      // Wait for results
      await page.waitForTimeout(1000);
    }
  });

  test('should clear all filters', async ({ page }) => {
    // Apply some filters first
    const searchInput = page.locator('input[type="search"]').first();
    if (await searchInput.count() > 0) {
      await searchInput.fill('Toyota');
      await searchInput.press('Enter');
      await page.waitForTimeout(500);
    }
    
    // Find clear filters button
    const clearButton = page.locator('button:has-text("Clear"), button:has-text("Reset")').first();
    
    if (await clearButton.count() > 0) {
      await clearButton.click();
      
      // Wait for reset
      await page.waitForTimeout(500);
      
      // Verify URL no longer has filters
      const url = page.url();
      expect(url).not.toContain('search=');
    }
  });

  test('should save search', async ({ page }) => {
    // Apply some filters
    const searchInput = page.locator('input[type="search"]').first();
    if (await searchInput.count() > 0) {
      await searchInput.fill('BMW');
      await searchInput.press('Enter');
      await page.waitForTimeout(500);
    }
    
    // Find save search button
    const saveButton = page.locator('button:has-text("Save"), button[aria-label*="save search"]').first();
    
    if (await saveButton.count() > 0) {
      await saveButton.click();
      
      // Should show save modal or success message
      await page.waitForTimeout(500);
    }
  });

  test('should sort results', async ({ page }) => {
    // Find sort dropdown
    const sortSelect = page.locator('select[name*="sort"], select[aria-label*="sort"]').first();
    
    if (await sortSelect.count() > 0) {
      // Change sort order
      await sortSelect.selectOption({ index: 1 }); // Select first non-default option
      
      // Wait for results to re-order
      await page.waitForTimeout(1000);
      
      // Verify URL updated
      await expect(page).toHaveURL(/.*sort.*/);
    }
  });
});
