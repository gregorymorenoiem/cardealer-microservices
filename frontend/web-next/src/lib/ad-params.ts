/**
 * Google Ads Click ID (gclid) + UTM Capture Utility
 *
 * SEM FIX CRITICAL-1: Captures gclid and UTM params from the landing URL,
 * persists them in localStorage with 90-day expiry, and provides them to
 * conversion events so Google Ads can attribute conversions to specific
 * keywords/campaigns.
 *
 * Usage:
 * - Call captureAdParams() once on app mount (in TrackingProvider)
 * - Call getAdParams() when firing conversion events
 * - Call getGclid() for Google Ads specific attribution
 */

const STORAGE_KEY = 'okla_ad_params';
const EXPIRY_DAYS = 90;

export interface AdParams {
  gclid?: string;
  utm_source?: string;
  utm_medium?: string;
  utm_campaign?: string;
  utm_term?: string;
  utm_content?: string;
  fbclid?: string;
  ttclid?: string;
  landing_page?: string;
  captured_at?: string;
}

/**
 * Capture gclid and UTM params from the current URL.
 * Store in localStorage with 90-day expiry.
 * Called once on every page load — only overwrites if new params are present.
 */
export function captureAdParams(): void {
  if (typeof window === 'undefined') return;

  try {
    const params = new URLSearchParams(window.location.search);
    const newParams: AdParams = {};

    // Capture Google Ads click ID
    const gclid = params.get('gclid');
    if (gclid) newParams.gclid = gclid;

    // Capture UTM parameters
    const utmKeys = [
      'utm_source',
      'utm_medium',
      'utm_campaign',
      'utm_term',
      'utm_content',
    ] as const;
    for (const key of utmKeys) {
      const val = params.get(key);
      if (val) newParams[key] = val;
    }

    // Capture Facebook click ID
    const fbclid = params.get('fbclid');
    if (fbclid) newParams.fbclid = fbclid;

    // Capture TikTok click ID
    const ttclid = params.get('ttclid');
    if (ttclid) newParams.ttclid = ttclid;

    // Only store if we found at least one ad-related param
    if (Object.keys(newParams).length === 0) return;

    newParams.landing_page = window.location.pathname;
    newParams.captured_at = new Date().toISOString();

    // Store with expiry
    const data = {
      params: newParams,
      expires: Date.now() + EXPIRY_DAYS * 24 * 60 * 60 * 1000,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(data));

    // Also set first-party cookies for server-side access and Safari ITP resilience
    // Safari ITP purges localStorage after 7 days for cross-site tracking domains,
    // but first-party cookies with explicit max-age survive up to the specified duration.
    if (gclid) {
      document.cookie = `_gclid=${gclid}; max-age=${EXPIRY_DAYS * 86400}; path=/; SameSite=Lax; Secure`;
    }
    if (fbclid) {
      document.cookie = `_fbc=fb.1.${Date.now()}.${fbclid}; max-age=${EXPIRY_DAYS * 86400}; path=/; SameSite=Lax; Secure`;
    }
    if (ttclid) {
      document.cookie = `_ttclid=${ttclid}; max-age=${EXPIRY_DAYS * 86400}; path=/; SameSite=Lax; Secure`;
    }
  } catch {
    // localStorage not available — silently fail
  }
}

/**
 * Get stored ad params (gclid + UTMs).
 * Returns null if expired or not set.
 * Falls back to first-party cookies if localStorage was purged (Safari ITP).
 */
export function getAdParams(): AdParams | null {
  if (typeof window === 'undefined') return null;

  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return _getAdParamsFromCookies();

    const data = JSON.parse(raw);
    if (data.expires && Date.now() > data.expires) {
      localStorage.removeItem(STORAGE_KEY);
      return _getAdParamsFromCookies();
    }

    return data.params || null;
  } catch {
    return _getAdParamsFromCookies();
  }
}

/**
 * Safari ITP fallback: read click IDs from first-party cookies
 * when localStorage has been purged (7-day cap for cross-site tracking domains).
 */
function _getAdParamsFromCookies(): AdParams | null {
  if (typeof document === 'undefined') return null;
  try {
    const cookies = document.cookie.split(';').reduce(
      (acc, c) => {
        const [k, v] = c.trim().split('=');
        if (k && v) acc[k] = v;
        return acc;
      },
      {} as Record<string, string>
    );

    const result: AdParams = {};
    if (cookies['_gclid']) result.gclid = cookies['_gclid'];
    if (cookies['_fbc']) {
      // _fbc format: fb.1.<timestamp>.<fbclid>
      const parts = cookies['_fbc'].split('.');
      if (parts.length >= 4) result.fbclid = parts.slice(3).join('.');
    }
    if (cookies['_ttclid']) result.ttclid = cookies['_ttclid'];

    return Object.keys(result).length > 0 ? result : null;
  } catch {
    return null;
  }
}

/**
 * Get the Google Ads click ID (gclid) for conversion attribution.
 */
export function getGclid(): string | undefined {
  return getAdParams()?.gclid;
}

/**
 * Get UTM parameters for campaign attribution.
 */
export function getUtmParams(): Pick<
  AdParams,
  'utm_source' | 'utm_medium' | 'utm_campaign' | 'utm_term' | 'utm_content'
> {
  const params = getAdParams();
  if (!params) return {};
  return {
    utm_source: params.utm_source,
    utm_medium: params.utm_medium,
    utm_campaign: params.utm_campaign,
    utm_term: params.utm_term,
    utm_content: params.utm_content,
  };
}
