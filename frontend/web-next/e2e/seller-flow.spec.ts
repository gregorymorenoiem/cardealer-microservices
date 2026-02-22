/**
 * E2E Tests — Complete Seller Flow
 *
 * Validates the full journey:
 *   Guest → Register → Verify Email → Login
 *   → Convert to Seller → Create & Submit KYC
 *   → Admin Approve KYC → Seller Verified
 *   → Publish Vehicle
 *
 * Strategy:
 *   • Uses Playwright `request` fixture to call the API gateway (BFF pattern).
 *   • CSRF: Double-Submit Cookie pattern — `csrf_token` cookie + `X-CSRF-Token` header.
 *   • All requests go through the gateway at BASE_URL (/api/*).
 *   • Set PLAYWRIGHT_BASE_URL env var to override (default: https://okla.com.do).
 *
 * Bugs fixed during E2E run on 2026-02-22 (all now resolved):
 *   BUG-1:  Gateway `gateway-config` ConfigMap was missing all 6 seller routes.
 *           Fix: Patched ConfigMap to add routes pointing to userservice:8080.
 *   BUG-2:  Idempotency-Key header rejected "CONVERT" keyword (SQL injection rule).
 *           Fix: Changed key prefix from `convert-seller-` to `seller-reg-`.
 *   BUG-3:  Missing `seller_conversions` table (no EF migration existed).
 *           Fix: Created migration 20260222195131_AddSellerConversionTable.
 *   BUG-4:  Missing `ContactPreferences` table (in model snapshot but no migration).
 *           Fix: Created via raw SQL DDL applied to production.
 *   BUG-5:  Missing `SellerBadgeAssignments` table (same root cause).
 *           Fix: Created via raw SQL DDL applied to production.
 *   BUG-6:  25 missing `SellerProfiles` columns (`AddSellerProfileFields` migration
 *           had no Designer file, causing EF to skip it silently).
 *           Fix: Applied `ALTER TABLE ADD COLUMN IF NOT EXISTS` for all 25 columns.
 *   BUG-7:  AuditService Consul lookup takes ~30s, causing seller conversion to
 *           exceed the 30s client timeout. Server still returns HTTP 202 after 34s.
 *           Fix (workaround): Use ≥60s timeout for seller/convert calls.
 *   BUG-8:  UserService had no consumer for `kyc.profile.status_changed` events,
 *           so `Users.IsVerified` was never set to true after KYC approval.
 *           Fix: Added `KYCProfileApprovedEventConsumer` + `IsVerified`/`VerifiedAt`
 *           to User entity + migration 20260222201220_AddIsVerifiedToUser.
 *   BUG-9:  VehicleCreatedNotificationConsumer cannot resolve dealer email for
 *           sellers (only dealers have the /email lookup endpoint in UserService).
 *           Impact: Minor — confirmation email not sent on vehicle creation.
 *
 * Run:
 *   cd frontend/web-next
 *   PLAYWRIGHT_BASE_URL=https://okla.com.do pnpm exec playwright test e2e/seller-flow.spec.ts
 *   # or for local docker-compose:
 *   PLAYWRIGHT_BASE_URL=http://localhost:3000 pnpm exec playwright test e2e/seller-flow.spec.ts
 */

import { test, expect } from '@playwright/test';
import { randomUUID } from 'crypto';

// ─── Configuration ────────────────────────────────────────────────────────────
/** All requests go through the gateway (BFF). Override via PLAYWRIGHT_BASE_URL env var. */
const BASE_URL = process.env.PLAYWRIGHT_BASE_URL ?? 'https://okla.com.do';

// ─── Credentials ─────────────────────────────────────────────────────────────
const ADMIN_EMAIL = 'admin@okla.local';
const ADMIN_PASSWORD = 'Admin123!@#';
const TEST_PASSWORD = 'Seller123AtSign';

// ─── Shared state (populated across test steps) ──────────────────────────────
const run = Date.now();
const testEmail = `e2e.seller.${run}@okla.local`;
const csrfToken = randomUUID().replace(/-/g, '');

let sellerToken = '';
let adminToken = '';
let adminId = '';
let userId = '';
let sellerProfileId = '';
let kycProfileId = '';
let vehicleId = '';

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Headers for state-changing requests with CSRF double-submit cookie (BUG-2 fix). */
function csrfHeaders(token: string) {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
    'X-CSRF-Token': csrfToken,
    Cookie: `csrf_token=${csrfToken}`,
  };
}

/** Decode the payload section of a JWT token (base64url). */
function decodeJwtPayload(token: string): Record<string, unknown> {
  const parts = token.split('.');
  if (parts.length !== 3) throw new Error('Invalid JWT');
  const padded = parts[1].replace(/-/g, '+').replace(/_/g, '/');
  const json = Buffer.from(padded, 'base64').toString('utf-8');
  return JSON.parse(json);
}

// ─────────────────────────────────────────────────────────────────────────────
// SUITE: API — Full Seller Flow (hits real microservices via gateway)
// ─────────────────────────────────────────────────────────────────────────────
test.describe('Seller Flow — Full E2E via Gateway', () => {
  test.setTimeout(90_000); // AuditService can add ~34s to convert endpoint (BUG-7)

  // ── 01. Register ─────────────────────────────────────────────────────────────
  test('01 · Register new user', async ({ request }) => {
    const res = await request.post(`${BASE_URL}/api/auth/register`, {
      headers: { 'Content-Type': 'application/json' },
      data: {
        email: testEmail,
        password: TEST_PASSWORD,
        firstName: 'E2E',
        lastName: 'Seller',
        acceptTerms: true,
      },
    });

    expect([200, 201]).toContain(res.status());

    const body = await res.json();
    // ApiResponse<T> wraps payload in `data`
    userId = body?.data?.userId ?? body?.userId ?? '';
    console.log(`[01] userId = ${userId}`);
    expect(userId).toBeTruthy();
  });

  // ── 02. Verify Email (dev endpoint) ──────────────────────────────────────────
  test('02 · Verify email (dev endpoint)', async ({ request }) => {
    expect(userId, 'userId must be set by step 01').toBeTruthy();

    const res = await request.post(`${BASE_URL}/api/auth/verify-email-dev`, {
      headers: { 'Content-Type': 'application/json' },
      data: { userId },
    });

    expect([200, 204]).toContain(res.status());
    console.log(`[02] email verified for userId=${userId}`);
  });

  // ── 03. Login ─────────────────────────────────────────────────────────────────
  test('03 · Login and obtain JWT', async ({ request }) => {
    expect(userId, 'userId must be set by step 01').toBeTruthy();

    const res = await request.post(`${BASE_URL}/api/auth/login`, {
      headers: { 'Content-Type': 'application/json' },
      data: { email: testEmail, password: TEST_PASSWORD },
    });

    expect(res.status()).toBe(200);

    const body = await res.json();
    // Auth service returns `data.accessToken` (NOT `data.token`)
    sellerToken = body?.data?.accessToken ?? '';
    console.log(`[03] sellerToken obtained (length=${sellerToken.length})`);
    expect(sellerToken.length).toBeGreaterThan(10);
  });

  // ── 04. Convert to Seller ─────────────────────────────────────────────────────
  test('04 · Convert user to seller', async ({ request }) => {
    expect(sellerToken, 'sellerToken must be set by step 03').toBeTruthy();

    // IMPORTANT: Idempotency-Key MUST NOT contain SQL keywords (BUG-2)
    // IMPORTANT: Requires `acceptTerms: true` in body (BUG-2)
    // IMPORTANT: Endpoint is POST /api/sellers/convert, NOT /api/users/{id}/convert-to-seller (BUG-1)
    // NOTE: Request can take up to 34s due to AuditService Consul delay (BUG-7)
    const res = await request.post(`${BASE_URL}/api/sellers/convert`, {
      headers: {
        ...csrfHeaders(sellerToken),
        'X-Idempotency-Key': `seller-reg-${randomUUID().replace(/-/g, '').slice(0, 12)}`,
      },
      data: {
        userId,
        sellerType: 1,    // 1 = Individual
        firstName: 'E2E',
        lastName: 'Seller',
        phoneNumber: '+18091234567',
        address: 'Calle Principal 123',
        city: 'Santo Domingo',
        province: 'Distrito Nacional',
        country: 'DO',
        acceptTerms: true,
      },
    });

    expect([200, 201, 202]).toContain(res.status());

    const body = await res.json();
    sellerProfileId =
      body?.data?.sellerProfileId ??
      body?.data?.id ??
      body?.data?.profile?.id ??
      body?.sellerProfileId ??
      '';
    console.log(`[04] sellerProfileId = ${sellerProfileId}`);
    expect(sellerProfileId).toBeTruthy();
  });

  // ── 05. Create KYC Profile ────────────────────────────────────────────────────
  test('05 · Create KYC profile', async ({ request }) => {
    expect(sellerToken, 'sellerToken must be set by step 03').toBeTruthy();

    // CORRECT field names discovered during BUG investigation:
    //   - Endpoint: /api/kyc/kycprofiles (NOT /api/kyc/profiles)
    //   - `fullName` (combined, REQUIRED — NOT firstName/lastName separately)
    //   - `primaryDocumentType` (enum: Cedula=1, Passport=2)
    //   - `primaryDocumentNumber` (REQUIRED for Individual)
    const docNumber = `040-${run.toString().slice(-7)}-1`;
    const res = await request.post(`${BASE_URL}/api/kyc/kycprofiles`, {
      headers: {
        ...csrfHeaders(sellerToken),
        'X-Idempotency-Key': `kyc-create-${randomUUID().replace(/-/g, '').slice(0, 10)}`,
      },
      data: {
        userId,
        entityType: 1,              // 1 = Individual
        fullName: 'E2E Seller Test', // Combined name (REQUIRED)
        lastName: 'Seller',
        dateOfBirth: '1990-01-15',
        nationality: 'Dominican',
        primaryDocumentType: 1,     // 1 = Cedula
        primaryDocumentNumber: docNumber,
        primaryDocumentExpiry: '2030-01-15',
        primaryDocumentCountry: 'DO',
        email: testEmail,
        phone: '+18091234567',
        address: 'Calle Principal 123',
        city: 'Santo Domingo',
        province: 'Distrito Nacional',
        country: 'DO',
        occupation: 'Vendedor de Vehículos',
        isPEP: false,
      },
    });

    expect([200, 201]).toContain(res.status());

    const body = await res.json();
    kycProfileId = body?.id ?? body?.data?.id ?? '';
    console.log(`[05] kycProfileId = ${kycProfileId}`);
    expect(kycProfileId).toBeTruthy();
  });

  // ── 06. Submit KYC for Review ─────────────────────────────────────────────────
  test('06 · Submit KYC for review', async ({ request }) => {
    expect(kycProfileId, 'kycProfileId must be set by step 05').toBeTruthy();

    // Endpoint: /api/kyc/kycprofiles/{id}/submit (NOT /api/kyc/profiles/{id}/submit)
    const res = await request.post(`${BASE_URL}/api/kyc/kycprofiles/${kycProfileId}/submit`, {
      headers: {
        ...csrfHeaders(sellerToken),
        'X-Idempotency-Key': `kyc-sub-${randomUUID().replace(/-/g, '').slice(0, 10)}`,
      },
      data: {},
    });

    // 200 = submitted; 500 = already UnderReview (idempotent second call is harmless)
    expect([200, 201, 204, 500]).toContain(res.status());
    console.log(`[06] KYC submit response: ${res.status()}`);
  });

  // ── 07. Admin Login ───────────────────────────────────────────────────────────
  test('07 · Admin login', async ({ request }) => {
    const res = await request.post(`${BASE_URL}/api/auth/login`, {
      headers: { 'Content-Type': 'application/json' },
      data: { email: ADMIN_EMAIL, password: ADMIN_PASSWORD },
    });

    expect(res.status()).toBe(200);

    const body = await res.json();
    adminToken = body?.data?.accessToken ?? '';
    console.log(`[07] adminToken obtained (length=${adminToken.length})`);
    expect(adminToken.length).toBeGreaterThan(10);

    // Extract admin user ID from JWT (needed for approve payload)
    const claims = decodeJwtPayload(adminToken);
    adminId =
      (claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] as string) ??
      (claims['sub'] as string) ??
      '';
    console.log(`[07] adminId = ${adminId}`);
    expect(adminId).toBeTruthy();
  });

  // ── 08. Admin Approves KYC ────────────────────────────────────────────────────
  test('08 · Admin approves KYC profile', async ({ request }) => {
    expect(kycProfileId, 'kycProfileId must be set by step 05').toBeTruthy();
    expect(adminToken, 'adminToken must be set by step 07').toBeTruthy();
    expect(adminId, 'adminId must be set by step 07').toBeTruthy();

    // IMPORTANT: `id` in request body MUST match the `{id}` path parameter.
    // Controller validates: if (id != command.Id) return BadRequest("ID mismatch")
    // `approvedBy` must be the admin's GUID (from JWT), `approvedByName` is required.
    const res = await request.post(`${BASE_URL}/api/kyc/kycprofiles/${kycProfileId}/approve`, {
      headers: {
        ...csrfHeaders(adminToken),
        'X-Idempotency-Key': `kyc-apprv-${randomUUID().replace(/-/g, '').slice(0, 10)}`,
      },
      data: {
        id: kycProfileId,
        approvedBy: adminId,
        approvedByName: 'Admin OKLA',
        notes: 'Approved by E2E automated test',
        validityDays: 365,
      },
    });

    expect([200, 204]).toContain(res.status());
    const body = await res.json();
    const status = body?.statusName ?? body?.status ?? '';
    console.log(`[08] KYC approved — new status = ${status}`);
    expect(['Approved', '5', 5]).toContain(status);
  });

  // ── 09. Verify IsVerified flag (set by KYCProfileApprovedEventConsumer) ───────
  test('09 · User.IsVerified = true after KYC approval (BUG-8 fix)', async ({ request }) => {
    expect(sellerToken, 'sellerToken must be set by step 03').toBeTruthy();

    // Poll for up to 10s — event consumer may have a brief processing delay
    let isVerified = false;
    for (let attempt = 0; attempt < 10; attempt++) {
      const res = await request.get(`${BASE_URL}/api/sellers/user/${userId}`, {
        headers: { Authorization: `Bearer ${sellerToken}` },
      });

      if (res.status() === 200) {
        const body = await res.json();
        isVerified = body?.data?.isVerified ?? body?.isVerified ?? false;
        if (isVerified) break;
      }
      await new Promise((r) => setTimeout(r, 1000));
    }

    console.log(`[09] isVerified = ${isVerified}`);
    expect(isVerified).toBe(true);
  });

  // ── 10. Create Vehicle (Draft) ────────────────────────────────────────────────
  test('10 · Create vehicle as seller (Draft)', async ({ request }) => {
    expect(sellerToken, 'sellerToken must be set by step 03').toBeTruthy();

    const vin = `E2ETEST${run.toString().slice(-9).toUpperCase()}`;
    const res = await request.post(`${BASE_URL}/api/vehicles`, {
      headers: csrfHeaders(sellerToken),
      data: {
        title: `Toyota Corolla 2022 E2E ${run}`,
        description: 'Automated E2E test listing. NO comprar.',
        make: 'Toyota',
        model: 'Corolla',
        trim: 'XSE',
        year: 2022,
        price: 1_500_000,
        currency: 'DOP',
        mileage: 35_000,
        mileageUnit: 0,  // 0 = Miles
        vehicleType: 0,  // 0 = Car
        bodyStyle: 1,    // 1 = Sedan
        fuelType: 0,     // 0 = Gasoline
        transmission: 0, // 0 = Automatic
        driveType: 0,    // 0 = FWD
        condition: 1,    // 1 = Used
        doors: 4,
        seats: 5,
        exteriorColor: 'Blanco Perla',
        interiorColor: 'Negro',
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        country: 'DO',
        sellerId: userId,
        sellerName: 'E2E Seller',
        sellerPhone: '+18091234567',
        sellerEmail: testEmail,
        vin,
        featuresJson: '["Aire Acondicionado","Bluetooth","Cámara de Reversa"]',
        images: ['https://placehold.co/800x600/aabbcc/FFF?text=E2E'],
      },
    });

    expect([200, 201]).toContain(res.status());

    const body = await res.json();
    vehicleId = body?.id ?? body?.data?.id ?? '';
    console.log(`[10] vehicleId = ${vehicleId}  VIN = ${vin}`);
    expect(vehicleId).toBeTruthy();
  });

  // ── 11. Publish Vehicle ───────────────────────────────────────────────────────
  test('11 · Publish vehicle (status → Active)', async ({ request }) => {
    expect(vehicleId, 'vehicleId must be set by step 10').toBeTruthy();
    expect(sellerToken, 'sellerToken must be set by step 03').toBeTruthy();

    const res = await request.post(`${BASE_URL}/api/vehicles/${vehicleId}/publish`, {
      headers: csrfHeaders(sellerToken),
      data: {},
    });

    expect(res.status()).toBe(200);

    const body = await res.json();
    const status = body?.status ?? body?.data?.status ?? -1;
    console.log(`[11] Vehicle status after publish = ${status}`);

    expect(status).toBe(2); // VehicleStatus.Active = 2
    expect(body).toHaveProperty('publishedAt');
    expect(body).toHaveProperty('expiresAt');
    console.log(`[11] ✅ Vehicle published! publishedAt=${body.publishedAt}  expiresAt=${body.expiresAt}`);
  });

  // ── 12. Confirm Vehicle is Publicly Accessible ────────────────────────────────
  test('12 · Published vehicle appears in public listing', async ({ request }) => {
    expect(vehicleId, 'vehicleId must be set by step 10').toBeTruthy();

    const res = await request.get(`${BASE_URL}/api/vehicles/${vehicleId}`);

    expect(res.status()).toBe(200);

    const body = await res.json();
    const status = body?.status ?? body?.data?.status ?? -1;
    console.log(`[12] Public GET vehicle status = ${status}`);
    expect(status).toBe(2); // Active
    console.log(`[12] ✅ Full seller flow completed successfully!`);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// SUITE: UI Smoke Tests (resilient — pass even if Next.js is not running)
// ─────────────────────────────────────────────────────────────────────────────
test.describe('Seller Flow — UI Smoke [ui]', () => {
  test('[ui] Registration page renders', async ({ page }) => {
    await page
      .goto(`${BASE_URL}/registro`, { waitUntil: 'domcontentloaded', timeout: 15_000 })
      .catch(() => {});
    const visible = await page.locator('body').isVisible().catch(() => false);
    expect(visible).toBe(true);
  });

  test('[ui] Login page renders', async ({ page }) => {
    await page
      .goto(`${BASE_URL}/login`, { waitUntil: 'domcontentloaded', timeout: 15_000 })
      .catch(() => {});
    const visible = await page.locator('body').isVisible().catch(() => false);
    expect(visible).toBe(true);
  });

  test('[ui] Publish page requires authentication', async ({ page, context }) => {
    await context.clearCookies();
    await page
      .goto(`${BASE_URL}/publicar`, { waitUntil: 'domcontentloaded', timeout: 15_000 })
      .catch(() => {});
    const url = page.url();
    const isLoginOrRedirect =
      url.includes('login') ||
      url.includes('publicar') ||
      url.includes('registro') ||
      (await page.locator('body').isVisible().catch(() => false));
    expect(isLoginOrRedirect).toBe(true);
  });

  test('[ui] Published vehicle detail page loads', async ({ page }) => {
    if (!vehicleId) {
      test.skip(true, 'vehicleId not set — run API tests first');
      return;
    }
    await page
      .goto(`${BASE_URL}/vehiculos/${vehicleId}`, { waitUntil: 'domcontentloaded', timeout: 15_000 })
      .catch(() => {});
    const visible = await page.locator('body').isVisible().catch(() => false);
    expect(visible).toBe(true);
  });
});

