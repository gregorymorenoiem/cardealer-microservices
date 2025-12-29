/**
 * Performance Optimizations
 * Sprint 8: Mobile Optimization & Polish
 * 
 * Utilities for improving performance and mobile UX
 */

/**
 * Debounce function for search and scroll events
 */
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout;
  
  return function executedFunction(...args: Parameters<T>) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

/**
 * Throttle function for scroll and resize events
 */
export function throttle<T extends (...args: any[]) => any>(
  func: T,
  limit: number
): (...args: Parameters<T>) => void {
  let inThrottle: boolean;
  
  return function executedFunction(...args: Parameters<T>) {
    if (!inThrottle) {
      func(...args);
      inThrottle = true;
      setTimeout(() => (inThrottle = false), limit);
    }
  };
}

/**
 * Detect if device is mobile/tablet
 */
export function isMobileDevice(): boolean {
  if (typeof window === 'undefined') return false;
  
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
    navigator.userAgent
  );
}

/**
 * Detect if device has touch support
 */
export function hasTouchSupport(): boolean {
  if (typeof window === 'undefined') return false;
  
  return (
    'ontouchstart' in window ||
    navigator.maxTouchPoints > 0 ||
    (navigator as any).msMaxTouchPoints > 0
  );
}

/**
 * Get optimal image size based on viewport
 */
export function getOptimalImageSize(): 'small' | 'medium' | 'large' {
  if (typeof window === 'undefined') return 'medium';
  
  const width = window.innerWidth;
  
  if (width < 640) return 'small'; // Mobile
  if (width < 1024) return 'medium'; // Tablet
  return 'large'; // Desktop
}

/**
 * Preload critical images
 */
export function preloadImage(src: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve();
    img.onerror = reject;
    img.src = src;
  });
}

/**
 * Intersection Observer for lazy loading
 */
export function createIntersectionObserver(
  callback: IntersectionObserverCallback,
  options?: IntersectionObserverInit
): IntersectionObserver | null {
  if (typeof window === 'undefined' || !('IntersectionObserver' in window)) {
    return null;
  }
  
  return new IntersectionObserver(callback, {
    root: null,
    rootMargin: '50px',
    threshold: 0.01,
    ...options,
  });
}

/**
 * Optimize scroll performance
 */
export function optimizeScrollPerformance() {
  if (typeof document === 'undefined') return;
  
  // Add will-change for scroll containers
  const scrollContainers = document.querySelectorAll('[data-scroll-container]');
  scrollContainers.forEach((container) => {
    (container as HTMLElement).style.willChange = 'transform';
  });
}

/**
 * Request Idle Callback polyfill
 */
export const requestIdleCallback =
  typeof window !== 'undefined' && 'requestIdleCallback' in window
    ? window.requestIdleCallback
    : (cb: IdleRequestCallback) => setTimeout(cb, 1);

/**
 * Cancel Idle Callback polyfill
 */
export const cancelIdleCallback =
  typeof window !== 'undefined' && 'cancelIdleCallback' in window
    ? window.cancelIdleCallback
    : (id: number) => clearTimeout(id);

/**
 * Measure performance metric
 */
export function measurePerformance(name: string, fn: () => void) {
  if (typeof performance === 'undefined') {
    fn();
    return;
  }
  
  performance.mark(`${name}-start`);
  fn();
  performance.mark(`${name}-end`);
  performance.measure(name, `${name}-start`, `${name}-end`);
  
  const measure = performance.getEntriesByName(name)[0];
  console.log(`[Performance] ${name}: ${measure.duration.toFixed(2)}ms`);
}

/**
 * Get device pixel ratio for responsive images
 */
export function getDevicePixelRatio(): number {
  if (typeof window === 'undefined') return 1;
  return window.devicePixelRatio || 1;
}

/**
 * Check if reduced motion is preferred
 */
export function prefersReducedMotion(): boolean {
  if (typeof window === 'undefined') return false;
  
  const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
  return mediaQuery.matches;
}

/**
 * Smooth scroll to element
 */
export function smoothScrollTo(elementId: string, offset: number = 0) {
  if (typeof document === 'undefined') return;
  
  const element = document.getElementById(elementId);
  if (!element) return;
  
  const top = element.getBoundingClientRect().top + window.pageYOffset - offset;
  
  window.scrollTo({
    top,
    behavior: prefersReducedMotion() ? 'auto' : 'smooth',
  });
}

/**
 * Generate srcset for responsive images
 */
export function generateSrcSet(baseUrl: string, sizes: number[]): string {
  return sizes.map((size) => `${baseUrl}?w=${size} ${size}w`).join(', ');
}

/**
 * Check if connection is slow (Save-Data mode)
 */
export function isSlowConnection(): boolean {
  if (typeof navigator === 'undefined') return false;
  
  const connection = (navigator as any).connection;
  if (!connection) return false;
  
  // Check for Save-Data mode
  if ('saveData' in connection && connection.saveData) return true;
  
  // Check for slow effective type (2g, slow-2g)
  if ('effectiveType' in connection) {
    return ['slow-2g', '2g'].includes(connection.effectiveType);
  }
  
  return false;
}
