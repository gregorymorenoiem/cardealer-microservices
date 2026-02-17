import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should navigate to login page', async ({ page }) => {
    // Click on "Login" button in navigation
    await page.click('text=Login');
    
    // Should be redirected to login page
    await expect(page).toHaveURL(/.*login/);
    
    // Verify login form is visible
    await expect(page.locator('form')).toBeVisible();
  });

  test('should show validation errors on empty login form', async ({ page }) => {
    await page.goto('/login');
    
    // Try to submit empty form
    await page.click('button[type="submit"]');
    
    // Should show validation errors (wait for them to appear)
    await page.waitForSelector('text=/required|obligatorio/i', { timeout: 3000 });
    
    // Verify we're still on login page
    await expect(page).toHaveURL(/.*login/);
  });

  test('should login with valid credentials', async ({ page }) => {
    await page.goto('/login');
    
    // Fill in the form
    await page.fill('input[type="email"]', 'test@example.com');
    await page.fill('input[type="password"]', 'Test123!');
    
    // Submit the form
    await page.click('button[type="submit"]');
    
    // Should redirect to dashboard (wait up to 5 seconds)
    await page.waitForURL(/\/(dashboard|browse)/, { timeout: 5000 });
    
    // Verify user is logged in by checking for logout button or user menu
    const loggedIn = await page.locator('text=/logout|sign out|salir/i').count() > 0;
    expect(loggedIn).toBeTruthy();
  });

  test('should navigate to register page', async ({ page }) => {
    await page.goto('/login');
    
    // Click on "Register" or "Sign Up" link
    await page.click('text=/register|sign up|crear cuenta/i');
    
    // Should be redirected to register page
    await expect(page).toHaveURL(/.*register/);
    
    // Verify register form is visible
    await expect(page.locator('form')).toBeVisible();
  });

  test('should logout successfully', async ({ page }) => {
    // First login
    await page.goto('/login');
    await page.fill('input[type="email"]', 'test@example.com');
    await page.fill('input[type="password"]', 'Test123!');
    await page.click('button[type="submit"]');
    
    // Wait for redirect after login
    await page.waitForURL(/\/(dashboard|browse)/, { timeout: 5000 });
    
    // Click logout button
    await page.click('text=/logout|sign out|salir/i');
    
    // Should redirect to home or login page
    await page.waitForURL(/\/(home|login|\/)/, { timeout: 3000 });
    
    // Verify login button is visible again
    await expect(page.locator('text=/login|iniciar sesión/i')).toBeVisible();
  });
});
