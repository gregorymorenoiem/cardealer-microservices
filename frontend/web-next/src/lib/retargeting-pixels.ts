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

/**
 * Initialize all retargeting pixels.
 * Call this once when the app loads (in TrackingProvider or layout).
 * Pixels are deferred via requestIdleCallback to avoid blocking INP.
 */
export function initRetargetingPixels(): void {
  if (typeof window === 'undefined' || initialized) return;
  initialized = true;

  // Defer pixel initialization to avoid blocking interactivity
  const deferInit = (fn: () => void) => {
    if (typeof requestIdleCallback !== 'undefined') {
      requestIdleCallback(fn);
    } else {
      setTimeout(fn, 3000);
    }
  };

  // --- Facebook Pixel (deferred) ---
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
      window.fbq?.('track', 'PageView');
    });
  }

  // --- Google Ads (reuse existing gtag from GoogleAnalytics component) ---
  // NOTE: GA4 gtag.js is already loaded by GoogleAnalytics component in layout.
  // We only need to configure Google Ads here — do NOT load gtag.js again.
  if (GOOGLE_ADS_ID) {
    // Wait for gtag to be available (loaded by GoogleAnalytics component)
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
    // Defer to avoid blocking main thread
    if (typeof requestIdleCallback !== 'undefined') {
      requestIdleCallback(configureGoogleAds);
    } else {
      setTimeout(configureGoogleAds, 2000);
    }
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
 * Track a page view across all pixels
 */
export function trackPageView(url?: string): void {
  if (typeof window === 'undefined') return;

  // Facebook
  if (window.fbq) {
    window.fbq('track', 'PageView');
  }

  // Google Analytics 4
  if (window.gtag && GA_MEASUREMENT_ID) {
    window.gtag('event', 'page_view', {
      page_location: url || window.location.href,
      page_title: document.title,
    });
  }

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

  // Facebook — Search event
  if (window.fbq) {
    window.fbq('track', 'Search', {
      search_string: params.query || `${params.make || ''} ${params.model || ''}`.trim(),
      content_category: 'Vehicles',
      content_type: 'vehicle',
      value: params.priceMax || 0,
      currency: 'DOP',
    });
  }

  // Google — custom event
  if (window.gtag) {
    window.gtag('event', 'search', {
      search_term: params.query || `${params.make || ''} ${params.model || ''}`.trim(),
      vehicle_make: params.make,
      vehicle_model: params.model,
      price_min: params.priceMin,
      price_max: params.priceMax,
      results_count: params.resultsCount,
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

  // Facebook — ViewContent (Auto Inventory Ads compatible)
  if (window.fbq) {
    window.fbq('track', 'ViewContent', {
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
  }

  // Google — view_item (Enhanced Ecommerce)
  if (window.gtag) {
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

  if (window.fbq) {
    window.fbq('track', 'AddToWishlist', {
      content_ids: [params.vehicleId],
      content_name: params.title,
      content_type: 'vehicle',
      value: params.price,
      currency: 'DOP',
    });
  }

  if (window.gtag) {
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

  // Facebook — Lead event (high-value conversion)
  if (window.fbq) {
    window.fbq('track', 'Lead', {
      content_ids: [params.vehicleId],
      content_name: params.title,
      content_category: params.contactMethod,
      value: params.price,
      currency: 'DOP',
    });
  }

  // Google — generate_lead (conversion event)
  if (window.gtag) {
    window.gtag('event', 'generate_lead', {
      currency: 'DOP',
      value: params.price,
      contact_method: params.contactMethod,
      vehicle_id: params.vehicleId,
      dealer_id: params.dealerId,
    });

    // If Google Ads conversion tracking is set up
    if (GOOGLE_ADS_ID) {
      window.gtag('event', 'conversion', {
        send_to: `${GOOGLE_ADS_ID}/lead`,
        value: params.price,
        currency: 'DOP',
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

  if (window.fbq) {
    window.fbq('track', 'InitiateCheckout', {
      content_ids: params.vehicleId ? [params.vehicleId] : [],
      content_type: 'vehicle',
      value: params.loanAmount,
      currency: 'DOP',
    });
  }

  if (window.gtag) {
    window.gtag('event', 'begin_checkout', {
      currency: 'DOP',
      value: params.loanAmount,
      items: params.vehicleId
        ? [{ item_id: params.vehicleId, price: params.loanAmount, quantity: 1 }]
        : [],
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
