/**
 * Responsive Design Utilities
 *
 * Breakpoint constants and helpers matching Tailwind CSS v4 defaults.
 * Use these when you need programmatic access to breakpoints (e.g., in JS
 * hooks or resize observers). For CSS-only responsiveness, prefer Tailwind
 * classes directly (mobile-first: sm:, md:, lg:, xl:, 2xl:).
 */

// ─────────────────────────────────────────────
// Breakpoint Constants (px) — match Tailwind v4
// ─────────────────────────────────────────────

export const BREAKPOINTS = {
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536,
} as const;

export type Breakpoint = keyof typeof BREAKPOINTS;

// ─────────────────────────────────────────────
// Layout Constants
// ─────────────────────────────────────────────

/** Navbar height in pixels — keep in sync with navbar.tsx h-16 / lg:h-18 */
export const NAVBAR_HEIGHT = {
  mobile: 64, // h-16
  desktop: 72, // lg:h-18 (4.5rem = 72px)
} as const;

/** Common container max-widths */
export const CONTAINER_MAX_WIDTH = {
  default: '80rem', // max-w-7xl (1280px)
  wide: '100rem', // 2xl:max-w-[1600px]
} as const;

/** Minimum touch target size per WCAG 2.5.5 (44×44 CSS pixels) */
export const MIN_TOUCH_TARGET = 44;

// ─────────────────────────────────────────────
// Media Query Helpers
// ─────────────────────────────────────────────

/**
 * Returns a media query string for a given breakpoint.
 * Useful with `window.matchMedia()` or CSS-in-JS.
 *
 * @example
 * const isDesktop = window.matchMedia(mediaQuery('lg')).matches;
 */
export function mediaQuery(breakpoint: Breakpoint): string {
  return `(min-width: ${BREAKPOINTS[breakpoint]}px)`;
}

/**
 * Check if the current viewport is at or above a given breakpoint.
 * Only works client-side. Returns false during SSR.
 */
export function isAboveBreakpoint(breakpoint: Breakpoint): boolean {
  if (typeof window === 'undefined') return false;
  return window.matchMedia(mediaQuery(breakpoint)).matches;
}

/**
 * Check if the current device is likely mobile based on viewport width.
 * Returns true for viewports below the `md` breakpoint (768px).
 */
export function isMobileViewport(): boolean {
  return !isAboveBreakpoint('md');
}

/**
 * Check if the device supports touch input.
 */
export function isTouchDevice(): boolean {
  if (typeof window === 'undefined') return false;
  return 'ontouchstart' in window || navigator.maxTouchPoints > 0;
}
