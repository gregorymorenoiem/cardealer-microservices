/**
 * Image Optimization Utilities for OKLA
 *
 * Optimized for slow internet connections common in Dominican Republic.
 * Uses the Network Information API to detect connection speed and adapt
 * image quality accordingly.
 *
 * Key features:
 * - Connection-aware quality selection
 * - Blur placeholder generation (tiny SVG base64)
 * - Responsive srcSet helpers
 * - Low-quality image preview URL generation (for S3/DO Spaces)
 */

// =============================================================================
// TYPES
// =============================================================================

/** Network Information API (experimental — Chrome/Edge/Android) */
interface NetworkInformation {
  effectiveType: 'slow-2g' | '2g' | '3g' | '4g';
  downlink: number; // Mbps
  rtt: number; // ms
  saveData: boolean;
  addEventListener?: (type: string, listener: () => void) => void;
  removeEventListener?: (type: string, listener: () => void) => void;
}

declare global {
  interface Navigator {
    connection?: NetworkInformation;
    mozConnection?: NetworkInformation;
    webkitConnection?: NetworkInformation;
  }
}

export type ConnectionSpeed = 'slow' | 'medium' | 'fast' | 'unknown';

// =============================================================================
// CONNECTION DETECTION
// =============================================================================

/**
 * Detect the user's connection speed using the Network Information API.
 * Falls back to 'unknown' when the API is not available (Safari, Firefox).
 */
export function getConnectionSpeed(): ConnectionSpeed {
  if (typeof navigator === 'undefined') return 'unknown';

  const conn = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
  if (!conn) return 'unknown';

  // Respect Data Saver mode
  if (conn.saveData) return 'slow';

  switch (conn.effectiveType) {
    case 'slow-2g':
    case '2g':
      return 'slow';
    case '3g':
      return 'medium';
    case '4g':
      return 'fast';
    default:
      return 'unknown';
  }
}

/**
 * Get optimal image quality based on connection speed.
 * Lower quality = smaller file = faster load on slow connections.
 *
 * - slow: 60 (aggressive compression, ~40% smaller than q=75)
 * - medium: 70 (balanced for 3G connections)
 * - fast/unknown: 75 (default — good visual quality)
 */
export function getOptimalQuality(speed?: ConnectionSpeed): number {
  const s = speed ?? getConnectionSpeed();
  switch (s) {
    case 'slow':
      return 60;
    case 'medium':
      return 70;
    case 'fast':
    case 'unknown':
    default:
      return 75;
  }
}

/**
 * Check if the user has opted into Data Saver / Lite mode.
 */
export function isDataSaverEnabled(): boolean {
  if (typeof navigator === 'undefined') return false;
  const conn = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
  return conn?.saveData ?? false;
}

// =============================================================================
// BLUR PLACEHOLDERS
// =============================================================================

/**
 * Base64-encoded SVG blur placeholder.
 * A tiny grey rectangle that displays instantly while the real image loads.
 * Size: ~120 bytes — negligible impact on HTML payload.
 */
export const BLUR_DATA_URL =
  'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAwIiBoZWlnaHQ9IjMwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZTJlOGYwIi8+PC9zdmc+';

/**
 * Generate a tinted blur placeholder with a custom color.
 * Useful for branded sections or dark-mode aware placeholders.
 *
 * @param color - CSS hex color (without #), e.g. 'e2e8f0' for slate-200
 * @param width - SVG viewBox width
 * @param height - SVG viewBox height
 */
export function generateBlurDataURL(
  color: string = 'e2e8f0',
  width: number = 400,
  height: number = 300
): string {
  const svg = `<svg width="${width}" height="${height}" xmlns="http://www.w3.org/2000/svg"><rect width="100%" height="100%" fill="#${color}"/></svg>`;
  if (typeof btoa === 'function') {
    return `data:image/svg+xml;base64,${btoa(svg)}`;
  }
  // SSR fallback
  return `data:image/svg+xml;base64,${Buffer.from(svg).toString('base64')}`;
}

/**
 * Generate a gradient blur placeholder (looks better for vehicle images).
 * Two-tone gradient that hints at a car photo loading.
 */
export function generateGradientBlurDataURL(
  colorFrom: string = 'f1f5f9',
  colorTo: string = 'e2e8f0'
): string {
  const svg = `<svg width="400" height="300" xmlns="http://www.w3.org/2000/svg"><defs><linearGradient id="g" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stop-color="#${colorFrom}"/><stop offset="100%" stop-color="#${colorTo}"/></linearGradient></defs><rect width="100%" height="100%" fill="url(#g)"/></svg>`;
  if (typeof btoa === 'function') {
    return `data:image/svg+xml;base64,${btoa(svg)}`;
  }
  return `data:image/svg+xml;base64,${Buffer.from(svg).toString('base64')}`;
}

// =============================================================================
// RESPONSIVE IMAGE HELPERS
// =============================================================================

/** Standard vehicle card image sizes for Next.js `sizes` prop */
export const VEHICLE_CARD_SIZES = '(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw';

/** Vehicle gallery main image sizes */
export const VEHICLE_GALLERY_SIZES = '(max-width: 768px) 100vw, (max-width: 1200px) 66vw, 800px';

/** Vehicle gallery thumbnail sizes */
export const VEHICLE_THUMBNAIL_SIZES = '80px';

/** Homepage featured vehicle card sizes */
export const FEATURED_CARD_SIZES = '(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw';

// =============================================================================
// LOW-QUALITY PREVIEW URLS
// =============================================================================

/**
 * Generate a low-quality preview URL for S3/DO Spaces images.
 * Appends query params for on-the-fly resizing if the CDN supports it.
 *
 * For Next.js Image component, this is handled automatically via the
 * built-in image optimizer. This function is useful for non-Next.js contexts
 * (e.g., `<img>` tags in banner ads, og:image meta tags).
 *
 * @param url - Original image URL
 * @param width - Desired preview width (default: 40px for blur effect)
 */
export function getLowQualityPreviewUrl(url: string, width: number = 40): string {
  if (!url) return '';

  // For Next.js optimized images, use the built-in optimizer
  if (typeof window !== 'undefined') {
    return `/_next/image?url=${encodeURIComponent(url)}&w=${width}&q=30`;
  }

  return url;
}

/**
 * Determine if an image should be loaded eagerly (above the fold) or lazily.
 * First 3 items in a list are considered "above the fold".
 *
 * @param index - Item index in the list (0-based)
 * @param aboveFoldCount - Number of items considered above the fold
 */
export function shouldLoadEagerly(index: number, aboveFoldCount: number = 3): boolean {
  return index < aboveFoldCount;
}

/**
 * Determine if an image should have `priority` flag (tells Next.js to preload).
 * Only the first visible item should have priority to avoid blocking.
 *
 * @param index - Item index in the list (0-based)
 */
export function shouldBePriority(index: number): boolean {
  return index === 0;
}
