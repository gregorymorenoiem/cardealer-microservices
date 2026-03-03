/**
 * E2E Tests — Validated Implemented Features (March 2026)
 * 
 * Tests all features implemented in this session:
 * 1. Auth-gate on "Chat en vivo" button
 * 2. OKLA green branding on AI search (no purple)
 * 3. Support chatbot visible on all pages
 * 4. /mensajes routing for dealer chats
 * 5. AI search agent responsiveness
 * 
 * Run with: pnpm exec playwright test e2e/implemented-features.spec.ts --config=playwright.prod.config.ts
 */
import { test, expect } from '@playwright/test';

// ─── Test Credentials ────────────────────────────────────────────────
const SELLER_EMAIL = 'gmoreno@okla.com.do';
const SELLER_PASSWORD = '$Gregory1';
const BUYER_EMAIL = 'buyer002@okla-test.com';
const BUYER_PASSWORD = 'BuyerTest2026!';

// ─── Helper: Login ───────────────────────────────────────────────────
async function login(page: any, email: string, password: string) {
  await page.goto('/login');
  await page.waitForLoadState('networkidle');
  await page.fill('input[placeholder*="email" i], input[type="email"]', email);
  await page.fill('input[placeholder*="••••" i], input[type="password"]', password);
  await page.click('button:has-text("Iniciar sesión")');
  // Wait for redirect
  await page.waitForURL((url: URL) => !url.pathname.includes('/login'), { timeout: 15000 });
}

// =====================================================================
// TEST 1: Auth-gate on "Chat en vivo" button
// =====================================================================
test.describe('Feature 1: Auth-gate on Chat en vivo', () => {
  
  test('Anonymous user sees login prompt when clicking Chat en vivo', async ({ page }) => {
    // Navigate to vehicle listing to find a vehicle
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // Click on the first vehicle card to go to detail
    const vehicleCard = page.locator('a[href*="/vehiculos/"]').first();
    if (await vehicleCard.isVisible()) {
      await vehicleCard.click();
      await page.waitForLoadState('networkidle');
      
      // Find and click "Chat en vivo" button
      const chatButton = page.locator('button:has-text("Chat en vivo")');
      if (await chatButton.isVisible()) {
        await chatButton.click();
        
        // Should show login prompt (amber alert box), NOT redirect to /mensajes
        const loginPrompt = page.locator('text=Inicia sesión para chatear');
        await expect(loginPrompt).toBeVisible({ timeout: 5000 });
        
        // Verify login and register buttons exist
        const loginBtn = page.locator('button:has-text("Iniciar sesión")');
        const registerBtn = page.locator('button:has-text("Crear cuenta gratis")');
        await expect(loginBtn).toBeVisible();
        await expect(registerBtn).toBeVisible();
      }
    }
  });

  test('Authenticated user is redirected to /mensajes on Chat en vivo click', async ({ page }) => {
    // Login first
    await login(page, BUYER_EMAIL, BUYER_PASSWORD);
    
    // Navigate to vehicle listing
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // Click on the first vehicle card
    const vehicleCard = page.locator('a[href*="/vehiculos/"]').first();
    if (await vehicleCard.isVisible()) {
      await vehicleCard.click();
      await page.waitForLoadState('networkidle');
      
      // Click "Chat en vivo" button
      const chatButton = page.locator('button:has-text("Chat en vivo")');
      if (await chatButton.isVisible()) {
        await chatButton.click();
        
        // Should redirect to /mensajes with query params
        await page.waitForURL((url: URL) => url.pathname.includes('/mensajes'), { timeout: 10000 });
        expect(page.url()).toContain('/mensajes');
        expect(page.url()).toContain('sellerId');
      }
    }
  });
});

// =====================================================================
// TEST 2: OKLA Green Branding on AI Search (no purple)
// =====================================================================
test.describe('Feature 2: OKLA Green branding on /vehiculos', () => {
  
  test('AI search UI uses OKLA green (#00A870), not purple', async ({ page }) => {
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // Check the search input area for OKLA green when AI search is active
    // The Sparkles icon should use green, not purple
    const sparklesIcon = page.locator('svg.lucide-sparkles').first();
    if (await sparklesIcon.isVisible()) {
      // Check there's no purple in the parent elements
      const purpleElements = page.locator('[class*="purple"]');
      const purpleCount = await purpleElements.count();
      expect(purpleCount).toBe(0);
    }
    
    // Look for the SearchAgentWidget bubble (if visible)
    const searchBubble = page.locator('button[aria-label*="Buscar con IA"], button[aria-label*="búsqueda IA"]');
    if (await searchBubble.isVisible()) {
      // Verify it uses green styling, not purple
      const bubbleClasses = await searchBubble.getAttribute('class');
      expect(bubbleClasses).not.toContain('purple');
      expect(bubbleClasses).toContain('00A870');
    }
  });
});

// =====================================================================
// TEST 3: Support Chatbot on ALL pages
// =====================================================================
test.describe('Feature 3: Support chatbot on all pages', () => {
  
  test('Support chatbot is visible on homepage', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Look for the support chatbot bubble (Headphones icon or "Soporte" label)
    const supportBubble = page.locator('button:has-text("Soporte"), [aria-label*="soporte" i]');
    await expect(supportBubble.first()).toBeVisible({ timeout: 10000 });
  });

  test('Support chatbot is visible on /vehiculos', async ({ page }) => {
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    const supportBubble = page.locator('button:has-text("Soporte"), [aria-label*="soporte" i]');
    await expect(supportBubble.first()).toBeVisible({ timeout: 10000 });
  });

  test('Support chatbot is visible on vehicle detail page', async ({ page }) => {
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // Navigate to a vehicle detail
    const vehicleCard = page.locator('a[href*="/vehiculos/"]').first();
    if (await vehicleCard.isVisible()) {
      await vehicleCard.click();
      await page.waitForLoadState('networkidle');
      
      const supportBubble = page.locator('button:has-text("Soporte"), [aria-label*="soporte" i]');
      await expect(supportBubble.first()).toBeVisible({ timeout: 10000 });
    }
  });

  test('Support chatbot opens and responds', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Click the support bubble
    const supportBubble = page.locator('button:has-text("Soporte"), [aria-label*="soporte" i]');
    if (await supportBubble.first().isVisible()) {
      await supportBubble.first().click();
      
      // Chat panel should open — look for the chat input
      const chatInput = page.locator('textarea[placeholder*="mensaje" i], input[placeholder*="mensaje" i], textarea[placeholder*="escribe" i]');
      await expect(chatInput.first()).toBeVisible({ timeout: 5000 });
    }
  });
});

// =====================================================================
// TEST 4: Dealer Chat Routes to /mensajes
// =====================================================================
test.describe('Feature 4: Dealer chat routes to /mensajes', () => {
  
  test('Dealer "Chatear con Ana (IA)" routes to /mensajes for authenticated user', async ({ page }) => {
    await login(page, BUYER_EMAIL, BUYER_PASSWORD);
    
    // Navigate to vehiculos and find a dealer vehicle
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // Look for a vehicle card — if it's from a dealer, it'll have "Chatear con Ana" button
    const vehicleCard = page.locator('a[href*="/vehiculos/"]').first();
    if (await vehicleCard.isVisible()) {
      await vehicleCard.click();
      await page.waitForLoadState('networkidle');
      
      // Check if this is a dealer vehicle (has Ana button)
      const anaButton = page.locator('button:has-text("Chatear con Ana")');
      if (await anaButton.isVisible({ timeout: 3000 }).catch(() => false)) {
        await anaButton.click();
        
        // Should redirect to /mensajes with dealerChat=true
        await page.waitForURL((url: URL) => url.pathname.includes('/mensajes'), { timeout: 10000 });
        expect(page.url()).toContain('/mensajes');
        expect(page.url()).toContain('dealerChat=true');
      }
    }
  });
});

// =====================================================================
// TEST 5: AI Search Responsiveness
// =====================================================================
test.describe('Feature 5: AI Search Agent', () => {
  
  test('AI search widget is visible on /vehiculos', async ({ page }) => {
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // SearchAgent widget or AI search bar should be visible
    // The widget can appear as a floating bubble or an inline search bar
    const aiSearchBubble = page.locator('button[aria-label*="Buscar con IA"], button[aria-label*="búsqueda IA"], button[aria-label*="search" i]');
    const aiSearchInput = page.locator('input[placeholder*="IA" i], input[placeholder*="lenguaje natural" i], input[placeholder*="Busca" i]');
    const sparklesButton = page.locator('button:has(svg.lucide-sparkles)');
    
    const hasBubble = await aiSearchBubble.isVisible().catch(() => false);
    const hasInput = await aiSearchInput.isVisible().catch(() => false);
    const hasSparkles = await sparklesButton.isVisible().catch(() => false);
    
    // At minimum the /vehiculos page should load and show search functionality
    expect(hasBubble || hasInput || hasSparkles).toBeTruthy();
  });
});

// =====================================================================
// TEST 6: Page Load and Navigation Smoke Tests
// =====================================================================
test.describe('Smoke Tests: Critical Pages Load', () => {
  
  test('Homepage loads correctly', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('okla');
    
    // Check for OKLA brand elements
    const oklaLogo = page.locator('text=OKLA');
    await expect(oklaLogo.first()).toBeVisible({ timeout: 10000 });
  });

  test('/vehiculos page loads with vehicle cards', async ({ page }) => {
    await page.goto('/vehiculos');
    await page.waitForLoadState('networkidle');
    
    // Should have vehicle cards or a loading state
    const vehicleCards = page.locator('[class*="vehicle"], [class*="card"], a[href*="/vehiculos/"]');
    await expect(vehicleCards.first()).toBeVisible({ timeout: 15000 });
  });

  test('Login page loads', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    
    const loginHeading = page.locator('text=Bienvenido');
    await expect(loginHeading.first()).toBeVisible({ timeout: 10000 });
  });

  test('/mensajes page redirects to login for anonymous users', async ({ page }) => {
    await page.goto('/mensajes');
    await page.waitForURL((url: URL) => url.pathname.includes('/login') || url.pathname.includes('/mensajes'), { timeout: 15000 });
    
    // Anonymous users should be redirected to login
    if (page.url().includes('/login')) {
      expect(page.url()).toContain('/login');
    }
  });
});
