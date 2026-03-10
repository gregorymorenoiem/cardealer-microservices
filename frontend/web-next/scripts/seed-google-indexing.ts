#!/usr/bin/env npx tsx
/**
 * Google Indexing API — Daily Seed Script
 *
 * Submits up to 200 vehicle URLs per day to Google Indexing API.
 * Tracks progress in a local JSON file so it can resume across runs.
 *
 * Usage:
 *   npx tsx scripts/seed-google-indexing.ts          # Submit next 200 pending
 *   npx tsx scripts/seed-google-indexing.ts --status  # Show progress
 *   npx tsx scripts/seed-google-indexing.ts --reset   # Reset progress
 *   npx tsx scripts/seed-google-indexing.ts --dry-run  # Preview without submitting
 *
 * Schedule via cron (daily at 3am):
 *   0 3 * * * cd /path/to/frontend/web-next && npx tsx scripts/seed-google-indexing.ts >> logs/indexing.log 2>&1
 *
 * Requirements:
 *   - GOOGLE_INDEXING_API_KEY env (base64-encoded service account JSON)
 *   - NEXT_PUBLIC_API_URL or INTERNAL_API_URL for backend vehicle list
 *   - NEXT_PUBLIC_SITE_URL for the production site URL
 *
 * Google Indexing API Limits:
 *   - Default: 200 requests/day (unverified properties)
 *   - After quota increase request: up to 10,000/day
 *
 * @see https://developers.google.com/search/apis/indexing-api/v3/quota-pricing
 */

import * as fs from 'fs';
import * as path from 'path';

// ─── Configuration ───────────────────────────────────────────
const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do';
const API_URL =
  process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443';
const DAILY_QUOTA = parseInt(process.env.GOOGLE_INDEXING_DAILY_QUOTA || '200', 10);
const PROGRESS_FILE = path.resolve(__dirname, '../.indexing-progress.json');
const DELAY_MS = 150; // ms between API calls (rate limiting)

// ─── Types ───────────────────────────────────────────────────
interface IndexingProgress {
  /** URLs already submitted (slug → timestamp) */
  submitted: Record<string, string>;
  /** URLs that failed on last attempt (slug → error + retryCount) */
  failed: Record<string, { error: string; retryCount: number; lastAttempt: string }>;
  /** Last run timestamp */
  lastRun: string;
  /** Total URLs discovered */
  totalDiscovered: number;
}

interface VehicleSitemapItem {
  slug: string;
  updatedAt?: string;
}

// ─── Progress Persistence ────────────────────────────────────
function loadProgress(): IndexingProgress {
  try {
    if (fs.existsSync(PROGRESS_FILE)) {
      return JSON.parse(fs.readFileSync(PROGRESS_FILE, 'utf-8'));
    }
  } catch (error) {
    console.warn('[Seed] ⚠️  Could not load progress file, starting fresh:', error);
  }
  return { submitted: {}, failed: {}, lastRun: '', totalDiscovered: 0 };
}

function saveProgress(progress: IndexingProgress): void {
  fs.writeFileSync(PROGRESS_FILE, JSON.stringify(progress, null, 2), 'utf-8');
}

// ─── Google Indexing API Client ──────────────────────────────
async function getAccessToken(): Promise<string | null> {
  const keyBase64 = process.env.GOOGLE_INDEXING_API_KEY;
  if (!keyBase64) {
    console.error('[Seed] ❌ GOOGLE_INDEXING_API_KEY not set');
    return null;
  }

  try {
    const crypto = await import('crypto');
    const keyJson = JSON.parse(Buffer.from(keyBase64, 'base64').toString('utf-8'));
    const now = Math.floor(Date.now() / 1000);
    const header = Buffer.from(JSON.stringify({ alg: 'RS256', typ: 'JWT' })).toString('base64url');
    const payload = Buffer.from(
      JSON.stringify({
        iss: keyJson.client_email,
        scope: 'https://www.googleapis.com/auth/indexing',
        aud: 'https://oauth2.googleapis.com/token',
        iat: now,
        exp: now + 3600,
      })
    ).toString('base64url');

    const sign = crypto.createSign('RSA-SHA256');
    sign.update(`${header}.${payload}`);
    const signature = sign.sign(keyJson.private_key, 'base64url');
    const jwt = `${header}.${payload}.${signature}`;

    const tokenResponse = await fetch('https://oauth2.googleapis.com/token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: `grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion=${jwt}`,
    });

    if (!tokenResponse.ok) {
      console.error(
        '[Seed] ❌ Token exchange failed:',
        tokenResponse.status,
        await tokenResponse.text()
      );
      return null;
    }

    const tokenData = await tokenResponse.json();
    return tokenData.access_token;
  } catch (error) {
    console.error('[Seed] ❌ Failed to get access token:', error);
    return null;
  }
}

async function submitUrl(
  accessToken: string,
  slug: string,
  type: 'URL_UPDATED' | 'URL_DELETED' = 'URL_UPDATED'
): Promise<{ success: boolean; error?: string }> {
  const url = `${SITE_URL}/vehiculos/${slug}`;

  try {
    const response = await fetch('https://indexing.googleapis.com/v3/urlNotifications:publish', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({ url, type }),
    });

    if (response.ok) {
      return { success: true };
    }

    const errorBody = await response.text();
    // 429 = quota exceeded — stop batch immediately
    if (response.status === 429) {
      return { success: false, error: `QUOTA_EXCEEDED (${response.status})` };
    }
    return { success: false, error: `HTTP ${response.status}: ${errorBody.slice(0, 200)}` };
  } catch (error) {
    return { success: false, error: String(error) };
  }
}

// ─── Vehicle Discovery ───────────────────────────────────────
async function fetchAllVehicleSlugs(): Promise<VehicleSitemapItem[]> {
  console.log(`[Seed] Fetching vehicle list from ${API_URL}/api/vehicles/sitemap ...`);

  try {
    const response = await fetch(`${API_URL}/api/vehicles/sitemap`);
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${await response.text()}`);
    }

    const data = await response.json();
    // Handle both array and wrapper object formats
    const items: VehicleSitemapItem[] = Array.isArray(data)
      ? data
      : (data?.items ?? data?.data ?? []);
    console.log(`[Seed] Discovered ${items.length} active vehicles`);
    return items;
  } catch (error) {
    console.error('[Seed] ❌ Failed to fetch vehicle list:', error);
    return [];
  }
}

// ─── Sitemap Ping ────────────────────────────────────────────
async function pingSitemapToSearchEngines(): Promise<void> {
  const sitemapUrl = encodeURIComponent(`${SITE_URL}/sitemap.xml`);
  const engines = [
    `https://www.google.com/ping?sitemap=${sitemapUrl}`,
    `https://www.bing.com/ping?sitemap=${sitemapUrl}`,
  ];

  for (const pingUrl of engines) {
    try {
      const resp = await fetch(pingUrl, { method: 'GET' });
      console.log(
        `[Seed] 🔔 Sitemap ping → ${resp.ok ? '✅' : '⚠️ ' + resp.status} ${pingUrl.split('?')[0]}`
      );
    } catch (error) {
      console.warn(`[Seed] ⚠️  Sitemap ping failed: ${pingUrl}`, error);
    }
  }
}

// ─── Main Entry Point ────────────────────────────────────────
async function main(): Promise<void> {
  const args = process.argv.slice(2);
  const isDryRun = args.includes('--dry-run');
  const showStatus = args.includes('--status');
  const resetProgress = args.includes('--reset');

  console.log('═══════════════════════════════════════════════════════════');
  console.log('  OKLA — Google Indexing API Daily Seed Script');
  console.log(`  ${new Date().toISOString()}`);
  console.log('═══════════════════════════════════════════════════════════');

  // ── Reset ──
  if (resetProgress) {
    if (fs.existsSync(PROGRESS_FILE)) {
      fs.unlinkSync(PROGRESS_FILE);
    }
    console.log('[Seed] ✅ Progress reset. All URLs will be re-submitted on next run.');
    return;
  }

  const progress = loadProgress();

  // ── Status ──
  if (showStatus) {
    const submittedCount = Object.keys(progress.submitted).length;
    const failedCount = Object.keys(progress.failed).length;
    console.log(`\n📊 Indexing Progress:`);
    console.log(`   Total discovered:   ${progress.totalDiscovered}`);
    console.log(`   Submitted (OK):     ${submittedCount}`);
    console.log(`   Failed (pending):   ${failedCount}`);
    console.log(`   Remaining:          ${progress.totalDiscovered - submittedCount}`);
    console.log(`   Last run:           ${progress.lastRun || 'never'}`);
    console.log(`   Daily quota:        ${DAILY_QUOTA}`);
    console.log(
      `   Est. days remaining: ${Math.ceil((progress.totalDiscovered - submittedCount) / DAILY_QUOTA)}`
    );
    if (failedCount > 0) {
      console.log(`\n⚠️  Failed URLs (will be retried):`);
      for (const [slug, info] of Object.entries(progress.failed).slice(0, 10)) {
        console.log(`   - ${slug} (attempt ${info.retryCount}, error: ${info.error})`);
      }
      if (failedCount > 10) console.log(`   ... and ${failedCount - 10} more`);
    }
    return;
  }

  // ── Fetch all vehicles ──
  const allVehicles = await fetchAllVehicleSlugs();
  if (allVehicles.length === 0) {
    console.error('[Seed] ❌ No vehicles found. Check API_URL and backend status.');
    process.exit(1);
  }
  progress.totalDiscovered = allVehicles.length;

  // ── Determine pending URLs ──
  // Priority: failed retries first, then new URLs
  const failedSlugs = Object.entries(progress.failed)
    .filter(([, info]) => info.retryCount < 3) // Max 3 retries
    .map(([slug]) => slug);

  const newSlugs = allVehicles
    .map(v => v.slug)
    .filter(slug => !progress.submitted[slug] && !progress.failed[slug]);

  const pendingQueue = [...failedSlugs, ...newSlugs];

  console.log(`\n📋 Queue Summary:`);
  console.log(`   Already submitted:  ${Object.keys(progress.submitted).length}`);
  console.log(`   Failed (retrying):  ${failedSlugs.length}`);
  console.log(`   New URLs:           ${newSlugs.length}`);
  console.log(`   Total in queue:     ${pendingQueue.length}`);
  console.log(`   Batch size (quota): ${DAILY_QUOTA}`);

  if (pendingQueue.length === 0) {
    console.log('\n✅ All URLs already submitted! Nothing to do.');
    await pingSitemapToSearchEngines();
    progress.lastRun = new Date().toISOString();
    saveProgress(progress);
    return;
  }

  const batch = pendingQueue.slice(0, DAILY_QUOTA);
  console.log(`\n🚀 Processing batch of ${batch.length} URLs...`);

  if (isDryRun) {
    console.log('\n[DRY RUN] Would submit these URLs:');
    batch.forEach((slug, i) => console.log(`   ${i + 1}. ${SITE_URL}/vehiculos/${slug}`));
    return;
  }

  // ── Get access token ──
  const accessToken = await getAccessToken();
  if (!accessToken) {
    console.error('[Seed] ❌ Cannot proceed without access token. Check GOOGLE_INDEXING_API_KEY.');
    process.exit(1);
  }

  // ── Submit batch ──
  let successCount = 0;
  let failCount = 0;
  let quotaExhausted = false;

  for (let i = 0; i < batch.length; i++) {
    const slug = batch[i];
    const progressPct = (((i + 1) / batch.length) * 100).toFixed(1);

    if (quotaExhausted) {
      console.log(`[Seed] ⏸️  Quota exhausted, stopping at ${i}/${batch.length}`);
      break;
    }

    const result = await submitUrl(accessToken, slug);

    if (result.success) {
      successCount++;
      progress.submitted[slug] = new Date().toISOString();
      delete progress.failed[slug];
      process.stdout.write(
        `\r   [${progressPct}%] ✅ ${successCount} ok, ${failCount} fail — ${slug}          `
      );
    } else {
      failCount++;
      const existing = progress.failed[slug];
      progress.failed[slug] = {
        error: result.error || 'Unknown error',
        retryCount: (existing?.retryCount || 0) + 1,
        lastAttempt: new Date().toISOString(),
      };

      if (result.error?.includes('QUOTA_EXCEEDED')) {
        quotaExhausted = true;
        console.log(
          `\n[Seed] ⚠️  Google API quota exceeded at URL ${i + 1}. Will resume tomorrow.`
        );
      } else {
        process.stdout.write(
          `\r   [${progressPct}%] ❌ ${successCount} ok, ${failCount} fail — ${slug} (${result.error})     `
        );
      }
    }

    // Save progress every 50 URLs (crash-safe)
    if ((i + 1) % 50 === 0) {
      saveProgress(progress);
    }

    // Rate limiting
    await new Promise(resolve => setTimeout(resolve, DELAY_MS));
  }

  console.log('\n');

  // ── Save final progress ──
  progress.lastRun = new Date().toISOString();
  saveProgress(progress);

  // ── Ping search engines ──
  await pingSitemapToSearchEngines();

  // ── Summary ──
  const totalSubmitted = Object.keys(progress.submitted).length;
  const totalFailed = Object.keys(progress.failed).length;
  const remaining = allVehicles.length - totalSubmitted;

  console.log('\n═══════════════════════════════════════════════════════════');
  console.log('  📊 Batch Complete');
  console.log('═══════════════════════════════════════════════════════════');
  console.log(`  This run:    ${successCount} submitted, ${failCount} failed`);
  console.log(`  Overall:     ${totalSubmitted}/${allVehicles.length} indexed`);
  console.log(`  Failed:      ${totalFailed} (will retry next run)`);
  console.log(`  Remaining:   ${remaining}`);
  console.log(`  Est. days:   ${Math.ceil(remaining / DAILY_QUOTA)}`);
  console.log('═══════════════════════════════════════════════════════════');
}

main().catch(error => {
  console.error('[Seed] Fatal error:', error);
  process.exit(1);
});
