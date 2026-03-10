/**
 * Retargeting Pixel Manager — OKLA
 *
 * Integrates Facebook Pixel, Google Ads Remarketing, Google Analytics 4,
 * and TikTok Pixel for retargeting users across social media and search.
 *
 * When a user searches for vehicles on OKLA, these pixels fire events
 * so that the user sees relevant OKLA ads on Facebook, Instagram, Google,
 * YouTube, and TikTok.
 *
 * Configuration: Set pixel IDs via environment variables.
 */

// =============================================================================
// PIXEL IDS (from env)
// =============================================================================

const FB_PIXEL_ID = process.env.NEXT_PUBLIC_FB_PIXEL_ID || '';
const GA_MEASUREMENT_ID = process.env.NEXT_PUBLIC_GA_MEASUREMENT_ID || '';
const GOOGLE_ADS_ID = process.env.NEXT_PUBLIC_GOOGLE_ADS_ID || '';
const TIKTOK_PIXEL_ID = process.env.NEXT_PUBLIC_TIKTOK_PIXEL_ID || '';

// =============================================================================
// TYPE DECLARATIONS
// =============================================================================

declare global {
  interface Window {
    fbq?: (...args: unknown[]) => void;
    // gtag and dataLayer are declared in google-analytics.tsx
    ttq?: {
      track: (...args: unknown[]) => void;
      identify: (params: Record<string, unknown>) => void;
      page: () => void;
    };
  }
}

// =============================================================================
// INITIALIZATION
// =============================================================================

let initialized = false;

// =============================================================================
// FB PIXEL EVENT QUEUE — Safari ITP & Deferred Init Race Condition Fix
// =============================================================================
// P0-01 FIX: FB Pixel is deferred via requestIdleCallback. Events fired before
// fbq loads are silently dropped. This queue captures them and flushes once fbq
// is available. Without this, ViewContent on first page load from Meta ads is lost.
const fbEventQueue: Array<[string, ...unknown[]]> = [];
let fbPixelReady = false;

/**
 * Queue-aware fbq caller. Fires immediately if fbq is loaded, otherwise queues.
 */
function safeFbq(eventType: string, ...args: unknown[]): void {
  if (fbPixelReady && window.fbq) {
    window.fbq(eventType, ...args);
  } else {
    fbEventQueue.push([eventType, ...args]);
  }
}

/**
 * Flush queued FB Pixel events after fbq has loaded.
 */
function flushFbEventQueue(): void {
  fbPixelReady = true;
  while (fbEventQueue.length > 0) {
    const [eventType, ...args] = fbEventQueue.shift()!;
    window.fbq?.(eventType, ...args);
  }
}

// =============================================================================
// UTM CONTEXT HELPER
// =============================================================================

/**
 * Get persisted UTM params for attaching to analytics events.
 * Falls back to ad-params.ts localStorage (survives SPA navigation).
 * Returns empty object if no UTMs are available.
 */
function getUtmContext(): Record<string, string | undefined> {
  try {
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    const { getAdParams } = require('@/lib/ad-params');
    const params = getAdParams();
    if (!params) return {};
    return {
      utm_source: params.utm_source,
      utm_medium: params.utm_medium,
      utm_campaign: params.utm_campaign,
      utm_term: params.utm_term,
      utm_content: params.utm_content,
    };
  } catch {
    return {};
  }
}

/**
 * Get persisted gclid for Google Ads attribution.
 */
function getPersistedGclid(): string | undefined {
  try {
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    const { getGclid } = require('@/lib/ad-params');
    return getGclid();
  } catch {
    return undefined;
  }
}

/**
 * Initialize all retargeting pixels.
 * Call this once when the app loads (in TrackingProvider or layout).
 * Pixels are deferred via requestIdleCallback to avoid blocking INP.
 */
export function initRetargetingPixels(): void {
  if (typeof window === 'undefined' || initialized) return;
  initialized = true;

  // SEM FIX: Warn in production if conversion pixel IDs are missing
  // Without these, Google Ads Smart Bidding cannot optimize for conversions
  if (process.env.NODE_ENV === 'production') {
    if (!GOOGLE_ADS_ID) {
      console.warn(
        '[OKLA Pixels] ⚠️ NEXT_PUBLIC_GOOGLE_ADS_ID is not set. Google Ads conversion tracking is disabled. Smart Bidding cannot optimize.'
      );
    }
    if (!FB_PIXEL_ID) {
      console.warn(
        '[OKLA Pixels] ⚠️ NEXT_PUBLIC_FB_PIXEL_ID is not set. Facebook/Meta retargeting is disabled.'
      );
    }
    // P1-04 FIX: Warn when Google Ads ID is set but conversion label is missing.
    // Without the label, generate_lead fires to GA4 but the Google Ads conversion
    // event silently skips — Smart Bidding gets zero conversion data.
    if (GOOGLE_ADS_ID && !process.env.NEXT_PUBLIC_GOOGLE_ADS_CONVERSION_LABEL) {
      console.warn(
        '[OKLA Pixels] ⚠️ NEXT_PUBLIC_GOOGLE_ADS_CONVERSION_LABEL is not set. Google Ads conversion events will NOT be sent. Smart Bidding has no conversion data.'
      );
    }
  }

  // Defer pixel initialization to avoid blocking interactivity
  const deferInit = (fn: () => void) => {
    if (typeof requestIdleCallback !== 'undefined') {
      requestIdleCallback(fn);
    } else {
      setTimeout(fn, 3000);
    }
  };

  // --- Facebook Pixel (deferred, with event queue flush) ---
  if (FB_PIXEL_ID) {
    deferInit(() => {
      /* eslint-disable */
      (function (f: any, b: any, e: any, v: any, n?: any, t?: any, s?: any) {
        if (f.fbq) return;
        n = f.fbq = function () {
          n.callMethod ? n.callMethod.apply(n, arguments) : n.queue.push(arguments);
        };
        if (!f._fbq) f._fbq = n;
        n.push = n;
        n.loaded = !0;
        n.version = '2.0';
        n.queue = [];
        t = b.createElement(e) as HTMLScriptElement;
        t.async = !0;
        t.src = v;
        s = b.getElementsByTagName(e)[0];
        s?.parentNode?.insertBefore(t, s);
      })(window, document, 'script', 'https://connect.facebook.net/en_US/fbevents.js');
      /* eslint-enable */

      window.fbq?.('init', FB_PIXEL_ID);
      // P2-01 FIX: Do NOT fire PageView here — trackPageView() is called by
      // TrackingProvider on route change and is the single source of FB PageViews.
      // Firing it here causes ~2x FB PageView inflation on initial load.

      // P0-01 FIX: Flush any events that were queued before fbq loaded
      // (e.g., ViewContent fired on vehicle detail page before idle callback ran)
      flushFbEventQueue();
    });
  }

  // --- Google Ads (reuse existing gtag from GoogleAnalytics component) ---
  // NOTE: GA4 gtag.js is already loaded by GoogleAnalytics component in layout.
  // We only need to configure Google Ads here — do NOT load gtag.js again.
  // P0-02 FIX: Do NOT defer Google Ads config behind requestIdleCallback.
  // Google Ads remarketing audience is built from config-triggered PageViews.
  // Deferring means bouncers (<3s) are never added to remarketing audiences,
  // and Smart Bidding loses signal data for fast-exit users.
  if (GOOGLE_ADS_ID) {
    const configureGoogleAds = () => {
      if (typeof window.gtag === 'function') {
        window.gtag('config', GOOGLE_ADS_ID);
      } else {
        // gtag not yet loaded — set up dataLayer and wait
        window.dataLayer = window.dataLayer || [];
        window.gtag = function () {
          // eslint-disable-next-line prefer-rest-params
          window.dataLayer?.push(arguments);
        };
        window.gtag('config', GOOGLE_ADS_ID);
      }
    };
    // Run immediately — gtag is likely already available from GoogleAnalytics afterInteractive
    configureGoogleAds();
  }

  // --- TikTok Pixel (deferred) ---
  if (TIKTOK_PIXEL_ID) {
    deferInit(() => {
      /* eslint-disable */
      (function (w: any, d: any, t: any) {
        w.TiktokAnalyticsObject = t;
        const ttq = (w[t] = w[t] || []) as any;
        ttq.methods = [
          'page',
          'track',
          'identify',
          'instances',
          'debug',
          'on',
          'off',
          'once',
          'ready',
          'alias',
          'group',
          'enableCookie',
          'disableCookie',
        ];
        ttq.setAndDefer = function (t: any, e: any) {
          t[e] = function () {
            t.push([e].concat(Array.prototype.slice.call(arguments, 0)));
          };
        };
        for (let i = 0; i < ttq.methods.length; i++) {
          ttq.setAndDefer(ttq, ttq.methods[i]);
        }
        ttq.instance = function (t: any) {
          const e = ttq._i[t] || [];
          for (let n = 0; n < ttq.methods.length; n++) {
            ttq.setAndDefer(e, ttq.methods[n]);
          }
          return e;
        };
        ttq.load = function (e: any, n?: any) {
          const i = 'https://analytics.tiktok.com/i18n/pixel/events.js';
          ttq._i = ttq._i || {};
          ttq._i[e] = [];
          ttq._i[e]._u = i;
          ttq._t = ttq._t || {};
          ttq._t[e] = +new Date();
          ttq._o = ttq._o || {};
          ttq._o[e] = n || {};
          const o = d.createElement('script') as HTMLScriptElement;
          o.type = 'text/javascript';
          o.async = true;
          o.src = i + '?sdkid=' + e + '&lib=' + t;
          const a = d.getElementsByTagName('script')[0];
          a?.parentNode?.insertBefore(o, a);
        };
        ttq.load(TIKTOK_PIXEL_ID);
        ttq.page();
      })(window, document, 'ttq');
      /* eslint-enable */
    });
  }
}

// =============================================================================
// EVENT TRACKING
// =============================================================================

/**
 * Track a page view across all pixels.
 * NOTE: GA4 page_view is NOT fired here — it's handled by AnalyticsPageView
 * (google-analytics.tsx) via gtag('config', ...) which automatically sends
 * a page_view with proper UTM attribution. Firing it here too would cause
 * ~2x page_view inflation in GA4 reports.
 */
export function trackPageView(_url?: string): void {
  if (typeof window === 'undefined') return;

  // Facebook (uses event queue — P0-01 fix)
  safeFbq('track', 'PageView');

  // TikTok
  if (window.ttq) {
    window.ttq.page();
  }
}

/**
 * Track when a user searches for vehicles.
 * This creates a retargeting audience of "vehicle searchers" on all platforms.
 */
export function trackVehicleSearch(params: {
  query?: string;
  make?: string;
  model?: string;
  priceMin?: number;
  priceMax?: number;
  resultsCount?: number;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — Search event (uses event queue for deferred init)
  safeFbq('track', 'Search', {
    search_string: params.query || `${params.make || ''} ${params.model || ''}`.trim(),
    content_category: 'Vehicles',
    content_type: 'vehicle',
    value: params.priceMax || 0,
    currency: 'DOP',
  });

  // Google — custom event
  if (window.gtag) {
    const utm = getUtmContext();
    window.gtag('event', 'search', {
      search_term: params.query || `${params.make || ''} ${params.model || ''}`.trim(),
      vehicle_make: params.make,
      vehicle_model: params.model,
      price_min: params.priceMin,
      price_max: params.priceMax,
      results_count: params.resultsCount,
      // UTM FIX: Attach campaign params for event-level attribution
      ...utm,
    });
  }

  // TikTok — Search event
  if (window.ttq) {
    window.ttq.track('Search', {
      query: params.query || `${params.make || ''} ${params.model || ''}`.trim(),
      content_type: 'vehicle',
    });
  }
}

/**
 * Track when a user views a specific vehicle.
 * Creates "vehicle viewers" retargeting audiences.
 */
export function trackVehicleView(params: {
  vehicleId: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  condition?: string;
  image?: string;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — ViewContent (Auto Inventory Ads compatible, uses event queue)
  safeFbq('track', 'ViewContent', {
    content_ids: [params.vehicleId],
    content_name: params.title,
    content_type: 'vehicle',
    content_category: `${params.make} ${params.model}`,
    value: params.price,
    currency: 'DOP',
    contents: [
      {
        id: params.vehicleId,
        quantity: 1,
      },
    ],
  });

  // Google — view_item (Enhanced Ecommerce)
  if (window.gtag) {
    const utm = getUtmContext();
    window.gtag('event', 'view_item', {
      currency: 'DOP',
      value: params.price,
      items: [
        {
          item_id: params.vehicleId,
          item_name: params.title,
          item_brand: params.make,
          item_category: params.model,
          item_variant: params.condition || 'used',
          price: params.price,
          quantity: 1,
        },
      ],
      // UTM FIX: Attach campaign params for event-level attribution
      ...utm,
    });
  }

  // TikTok — ViewContent
  if (window.ttq) {
    window.ttq.track('ViewContent', {
      content_id: params.vehicleId,
      content_name: params.title,
      content_type: 'vehicle',
      value: params.price,
      currency: 'DOP',
    });
  }
}

/**
 * Track when a user adds a vehicle to favorites.
 * Creates "high-intent" retargeting audiences.
 */
export function trackAddToWishlist(params: {
  vehicleId: string;
  title: string;
  price: number;
  make: string;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — AddToWishlist (uses event queue for deferred init)
  safeFbq('track', 'AddToWishlist', {
    content_ids: [params.vehicleId],
    content_name: params.title,
    content_type: 'vehicle',
    value: params.price,
    currency: 'DOP',
  });

  if (window.gtag) {
    const utm = getUtmContext();
    window.gtag('event', 'add_to_wishlist', {
      currency: 'DOP',
      value: params.price,
      items: [
        {
          item_id: params.vehicleId,
          item_name: params.title,
          item_brand: params.make,
          price: params.price,
        },
      ],
      // UTM FIX: Attach campaign params for event-level attribution
      ...utm,
    });
  }

  if (window.ttq) {
    window.ttq.track('AddToWishlist', {
      content_id: params.vehicleId,
      content_name: params.title,
      value: params.price,
      currency: 'DOP',
    });
  }
}

/**
 * Track when a user contacts a dealer (call, WhatsApp, message).
 * Creates "high-intent contacted" retargeting audiences.
 */
export function trackContactDealer(params: {
  vehicleId: string;
  title: string;
  price: number;
  contactMethod: 'call' | 'whatsapp' | 'message' | 'form';
  dealerId: string;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — Lead event (high-value conversion, uses event queue)
  safeFbq('track', 'Lead', {
    content_ids: [params.vehicleId],
    content_name: params.title,
    content_category: params.contactMethod,
    value: params.price,
    currency: 'DOP',
  });

  // Google — generate_lead (conversion event)
  if (window.gtag) {
    const utm = getUtmContext();
    const gclid = getPersistedGclid();
    window.gtag('event', 'generate_lead', {
      currency: 'DOP',
      value: params.price,
      contact_method: params.contactMethod,
      vehicle_id: params.vehicleId,
      dealer_id: params.dealerId,
      // UTM FIX: Attach campaign + gclid for full lead attribution
      ...utm,
      ...(gclid ? { gclid } : {}),
    });

    // SEM FIX: Google Ads conversion tracking with proper conversion label.
    // The send_to must be AW-XXXXXXXXX/AbCdEfGh (from Google Ads UI),
    // NOT AW-XXXXXXXXX/lead which is invalid and records zero conversions.
    const GOOGLE_ADS_CONVERSION_LABEL = process.env.NEXT_PUBLIC_GOOGLE_ADS_CONVERSION_LABEL || '';
    if (GOOGLE_ADS_ID && GOOGLE_ADS_CONVERSION_LABEL) {
      window.gtag('event', 'conversion', {
        send_to: `${GOOGLE_ADS_ID}/${GOOGLE_ADS_CONVERSION_LABEL}`,
        value: params.price,
        currency: 'DOP',
        transaction_id: params.vehicleId, // Dedup conversions
        ...(gclid ? { gclid } : {}),
      });
    }
  }

  // TikTok — SubmitForm (closest to lead event)
  if (window.ttq) {
    window.ttq.track('SubmitForm', {
      content_id: params.vehicleId,
      content_name: params.title,
      value: params.price,
      currency: 'DOP',
    });
  }
}

/**
 * Track when a user uses the financing calculator.
 */
export function trackFinancingIntent(params: {
  vehicleId?: string;
  loanAmount: number;
  downPayment: number;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — InitiateCheckout (uses event queue for deferred init)
  safeFbq('track', 'InitiateCheckout', {
    content_ids: params.vehicleId ? [params.vehicleId] : [],
    content_type: 'vehicle',
    value: params.loanAmount,
    currency: 'DOP',
  });

  if (window.gtag) {
    const utm = getUtmContext();
    window.gtag('event', 'begin_checkout', {
      currency: 'DOP',
      value: params.loanAmount,
      items: params.vehicleId
        ? [{ item_id: params.vehicleId, price: params.loanAmount, quantity: 1 }]
        : [],
      // UTM FIX: Attach campaign params for financing intent attribution
      ...utm,
    });
  }

  if (window.ttq) {
    window.ttq.track('InitiateCheckout', {
      content_id: params.vehicleId,
      value: params.loanAmount,
      currency: 'DOP',
    });
  }
}

/**
 * Track when a user starts a chatbot session.
 * Creates "high-intent" remarketing audiences — chat initiation signals
 * strong purchase intent but the user hasn't converted yet.
 * P1-03 FIX: Added vehicleId + vehicleTitle for Dynamic Remarketing audiences
 * ("users who chatted about THIS vehicle but didn't buy").
 */
export function trackChatStart(params: {
  dealerId?: string;
  channel?: string;
  vehicleId?: string;
  vehicleTitle?: string;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — Contact event (high-intent signal)
  safeFbq('track', 'Contact', {
    content_category: 'ChatAgent',
    content_type: 'chat_start',
    // P1-03: Enable Dynamic Remarketing for chat users
    ...(params.vehicleId ? { content_ids: [params.vehicleId] } : {}),
    ...(params.vehicleTitle ? { content_name: params.vehicleTitle } : {}),
  });

  // Google — custom event for remarketing audience creation
  if (window.gtag) {
    const utm = getUtmContext();
    window.gtag('event', 'chat_start', {
      event_category: 'engagement',
      dealer_id: params.dealerId,
      channel: params.channel || 'web',
      // P1-03: Pass vehicle context for Dynamic Remarketing
      ...(params.vehicleId ? { item_id: params.vehicleId } : {}),
      ...(params.vehicleTitle ? { item_name: params.vehicleTitle } : {}),
      ...utm,
    });
  }

  // TikTok — Contact event
  if (window.ttq) {
    window.ttq.track('Contact', {
      content_type: 'chat_start',
      ...(params.vehicleId ? { content_id: params.vehicleId } : {}),
    });
  }
}

/**
 * Identify a user across pixels when they log in.
 */
export function identifyUser(params: {
  email?: string;
  phone?: string;
  firstName?: string;
  lastName?: string;
  city?: string;
}): void {
  if (typeof window === 'undefined') return;

  // Facebook — Advanced Matching
  if (window.fbq && (params.email || params.phone)) {
    window.fbq('init', FB_PIXEL_ID, {
      em: params.email?.toLowerCase(),
      ph: params.phone?.replace(/\D/g, ''),
      fn: params.firstName?.toLowerCase(),
      ln: params.lastName?.toLowerCase(),
      ct: params.city?.toLowerCase(),
      country: 'do',
    });
  }

  // Google — user properties
  if (window.gtag) {
    window.gtag('set', 'user_properties', {
      city: params.city,
      country: 'DO',
    });
  }

  // TikTok — identify
  if (window.ttq) {
    window.ttq.identify({
      email: params.email,
      phone_number: params.phone,
    });
  }
}

// =============================================================================
// HELPER: Check if any pixel is configured
// =============================================================================

export function hasRetargetingPixels(): boolean {
  return !!(FB_PIXEL_ID || GA_MEASUREMENT_ID || GOOGLE_ADS_ID || TIKTOK_PIXEL_ID);
}

export function getConfiguredPixels(): string[] {
  const pixels: string[] = [];
  if (FB_PIXEL_ID) pixels.push('Facebook Pixel');
  if (GA_MEASUREMENT_ID) pixels.push('Google Analytics 4');
  if (GOOGLE_ADS_ID) pixels.push('Google Ads');
  if (TIKTOK_PIXEL_ID) pixels.push('TikTok Pixel');
  return pixels;
}
