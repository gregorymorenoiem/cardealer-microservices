/**
 * Google Indexing API Integration
 *
 * Submits new and updated vehicle URLs to Google for rapid indexing (<48h).
 * Uses the Google Indexing API v3 for URL_UPDATED/URL_DELETED notifications.
 *
 * Requirements:
 * - GOOGLE_INDEXING_API_KEY env variable (service account JSON key, base64-encoded)
 * - Google Search Console property verified for okla.com.do
 * - Service account granted "Owner" permission in Search Console
 *
 * @see https://developers.google.com/search/apis/indexing-api/v3/quickstart
 */

const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do';
const INDEXING_API_ENDPOINT = 'https://indexing.googleapis.com/v3/urlNotifications:publish';

interface IndexingResult {
  url: string;
  success: boolean;
  error?: string;
  notifyTime?: string;
}

/**
 * Get OAuth2 access token from service account credentials.
 * Uses the Google Auth Library pattern for server-side Node.js.
 */
async function getAccessToken(): Promise<string | null> {
  const keyBase64 = process.env.GOOGLE_INDEXING_API_KEY;
  if (!keyBase64) {
    console.warn('[Google Indexing] GOOGLE_INDEXING_API_KEY not set — skipping indexing request');
    return null;
  }

  try {
    // Decode the base64-encoded service account JSON key
    const keyJson = JSON.parse(Buffer.from(keyBase64, 'base64').toString('utf-8'));

    // Create JWT for Google OAuth2
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

    // Sign the JWT with the private key
    const crypto = await import('crypto');
    const sign = crypto.createSign('RSA-SHA256');
    sign.update(`${header}.${payload}`);
    const signature = sign.sign(keyJson.private_key, 'base64url');
    const jwt = `${header}.${payload}.${signature}`;

    // Exchange JWT for access token
    const tokenResponse = await fetch('https://oauth2.googleapis.com/token', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: `grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion=${jwt}`,
    });

    if (!tokenResponse.ok) {
      console.error('[Google Indexing] Token exchange failed:', tokenResponse.status);
      return null;
    }

    const tokenData = await tokenResponse.json();
    return tokenData.access_token;
  } catch (error) {
    console.error('[Google Indexing] Failed to get access token:', error);
    return null;
  }
}

/**
 * Notify Google that a URL has been updated or created.
 * This triggers Googlebot to crawl the URL within minutes, not days.
 */
export async function notifyGoogleUrlUpdated(vehicleSlug: string): Promise<IndexingResult> {
  const url = `${SITE_URL}/vehiculos/${vehicleSlug}`;

  const accessToken = await getAccessToken();
  if (!accessToken) {
    return { url, success: false, error: 'No access token available' };
  }

  try {
    const response = await fetch(INDEXING_API_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({
        url,
        type: 'URL_UPDATED',
      }),
    });

    if (!response.ok) {
      const errorBody = await response.text();
      console.error(`[Google Indexing] URL_UPDATED failed for ${url}:`, response.status, errorBody);
      return { url, success: false, error: `HTTP ${response.status}: ${errorBody}` };
    }

    const data = await response.json();
    console.log(`[Google Indexing] ✅ URL_UPDATED submitted: ${url}`);
    return {
      url,
      success: true,
      notifyTime: data.urlNotificationMetadata?.latestUpdate?.notifyTime,
    };
  } catch (error) {
    console.error(`[Google Indexing] Network error for ${url}:`, error);
    return { url, success: false, error: String(error) };
  }
}

/**
 * Notify Google that a URL has been removed (vehicle unpublished/sold).
 */
export async function notifyGoogleUrlDeleted(vehicleSlug: string): Promise<IndexingResult> {
  const url = `${SITE_URL}/vehiculos/${vehicleSlug}`;

  const accessToken = await getAccessToken();
  if (!accessToken) {
    return { url, success: false, error: 'No access token available' };
  }

  try {
    const response = await fetch(INDEXING_API_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({
        url,
        type: 'URL_DELETED',
      }),
    });

    if (!response.ok) {
      const errorBody = await response.text();
      console.error(`[Google Indexing] URL_DELETED failed for ${url}:`, response.status, errorBody);
      return { url, success: false, error: `HTTP ${response.status}: ${errorBody}` };
    }

    console.log(`[Google Indexing] ✅ URL_DELETED submitted: ${url}`);
    return { url, success: true };
  } catch (error) {
    console.error(`[Google Indexing] Network error for ${url}:`, error);
    return { url, success: false, error: String(error) };
  }
}

/**
 * Batch notify Google about multiple vehicle URLs.
 * Respects the API rate limit of 200 requests/day.
 */
export async function batchNotifyGoogleUrls(
  vehicleSlugs: string[],
  type: 'URL_UPDATED' | 'URL_DELETED' = 'URL_UPDATED'
): Promise<IndexingResult[]> {
  // Google Indexing API allows 200 requests/day for new properties
  const MAX_BATCH = 200;
  const slugsToProcess = vehicleSlugs.slice(0, MAX_BATCH);

  if (vehicleSlugs.length > MAX_BATCH) {
    console.warn(
      `[Google Indexing] Truncating batch from ${vehicleSlugs.length} to ${MAX_BATCH} (API daily limit)`
    );
  }

  const results: IndexingResult[] = [];

  for (const slug of slugsToProcess) {
    const result =
      type === 'URL_UPDATED'
        ? await notifyGoogleUrlUpdated(slug)
        : await notifyGoogleUrlDeleted(slug);
    results.push(result);

    // Rate limiting: 1 request per 100ms to avoid quota errors
    await new Promise(resolve => setTimeout(resolve, 100));
  }

  const successCount = results.filter(r => r.success).length;
  console.log(
    `[Google Indexing] Batch complete: ${successCount}/${results.length} successful (${type})`
  );

  return results;
}

/**
 * Check the indexing status of a URL via the Indexing API.
 */
export async function getUrlIndexingStatus(
  vehicleSlug: string
): Promise<{ url: string; lastCrawled?: string; lastUpdated?: string } | null> {
  const url = `${SITE_URL}/vehiculos/${vehicleSlug}`;

  const accessToken = await getAccessToken();
  if (!accessToken) return null;

  try {
    const encodedUrl = encodeURIComponent(url);
    const response = await fetch(
      `https://indexing.googleapis.com/v3/urlNotifications/metadata?url=${encodedUrl}`,
      {
        headers: { Authorization: `Bearer ${accessToken}` },
      }
    );

    if (!response.ok) return null;

    const data = await response.json();
    return {
      url,
      lastCrawled: data.latestUpdate?.notifyTime,
      lastUpdated: data.latestRemove?.notifyTime,
    };
  } catch {
    return null;
  }
}

/**
 * Ping Google and Bing to re-crawl the sitemap.
 * Should be called after any sitemap change (vehicle publish/unpublish).
 *
 * Note: Google deprecated the explicit /ping endpoint in 2023 but still processes it.
 * Primary discovery is via GSC + sitemap.xml registration.
 *
 * @see https://www.bing.com/indexnow (Bing also supports IndexNow protocol)
 */
export async function pingSitemapToSearchEngines(): Promise<void> {
  const sitemapUrl = encodeURIComponent(`${SITE_URL}/sitemap.xml`);
  const engines = [
    { name: 'Google', url: `https://www.google.com/ping?sitemap=${sitemapUrl}` },
    { name: 'Bing', url: `https://www.bing.com/ping?sitemap=${sitemapUrl}` },
  ];

  for (const engine of engines) {
    try {
      const response = await fetch(engine.url, { method: 'GET' });
      console.log(
        `[Google Indexing] Sitemap ping → ${engine.name}: ${response.ok ? '✅ OK' : `⚠️ ${response.status}`}`
      );
    } catch (error) {
      console.warn(`[Google Indexing] Sitemap ping to ${engine.name} failed:`, error);
    }
  }
}
