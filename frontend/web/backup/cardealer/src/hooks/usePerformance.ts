/**
 * Performance Hooks Collection
 * Optimized hooks for low-bandwidth scenarios in Dominican Republic
 * 
 * Features:
 * - usePrefetch: Preload data on hover/focus
 * - useLazyLoad: Intersection observer for lazy loading
 * - useNetworkStatus: Detect connection quality
 * - useImagePreloader: Preload images in background
 * - useReducedMotion: Respect user preferences
 * - useIdleCallback: Execute during idle time
 */

import { useState, useEffect, useRef, useCallback, useMemo } from 'react';

// ============================================
// Network Status Detection
// ============================================

interface NetworkInfo {
  isOnline: boolean;
  connectionType: 'slow-2g' | '2g' | '3g' | '4g' | 'wifi' | 'unknown';
  effectiveType: string;
  downlink: number; // Mbps
  rtt: number; // Round trip time in ms
  saveData: boolean; // User requested reduced data
  isSlowConnection: boolean;
}

export const useNetworkStatus = (): NetworkInfo => {
  const [networkInfo, setNetworkInfo] = useState<NetworkInfo>(() => {
    const nav = navigator as Navigator & {
      connection?: {
        effectiveType: string;
        downlink: number;
        rtt: number;
        saveData: boolean;
      };
    };

    const connection = nav.connection;
    
    return {
      isOnline: navigator.onLine,
      connectionType: (connection?.effectiveType as NetworkInfo['connectionType']) || 'unknown',
      effectiveType: connection?.effectiveType || 'unknown',
      downlink: connection?.downlink || 10,
      rtt: connection?.rtt || 50,
      saveData: connection?.saveData || false,
      isSlowConnection: (connection?.effectiveType === '2g' || connection?.effectiveType === 'slow-2g') || false,
    };
  });

  useEffect(() => {
    const nav = navigator as Navigator & {
      connection?: {
        effectiveType: string;
        downlink: number;
        rtt: number;
        saveData: boolean;
        addEventListener: (type: string, listener: () => void) => void;
        removeEventListener: (type: string, listener: () => void) => void;
      };
    };

    const updateNetworkInfo = () => {
      const connection = nav.connection;
      setNetworkInfo({
        isOnline: navigator.onLine,
        connectionType: (connection?.effectiveType as NetworkInfo['connectionType']) || 'unknown',
        effectiveType: connection?.effectiveType || 'unknown',
        downlink: connection?.downlink || 10,
        rtt: connection?.rtt || 50,
        saveData: connection?.saveData || false,
        isSlowConnection: (connection?.effectiveType === '2g' || connection?.effectiveType === 'slow-2g') || false,
      });
    };

    window.addEventListener('online', updateNetworkInfo);
    window.addEventListener('offline', updateNetworkInfo);
    nav.connection?.addEventListener?.('change', updateNetworkInfo);

    return () => {
      window.removeEventListener('online', updateNetworkInfo);
      window.removeEventListener('offline', updateNetworkInfo);
      nav.connection?.removeEventListener?.('change', updateNetworkInfo);
    };
  }, []);

  return networkInfo;
};

// ============================================
// Lazy Loading with Intersection Observer
// ============================================

interface UseLazyLoadOptions {
  rootMargin?: string;
  threshold?: number | number[];
  triggerOnce?: boolean;
}

export const useLazyLoad = <T extends HTMLElement = HTMLDivElement>(
  options: UseLazyLoadOptions = {}
): [React.RefObject<T | null>, boolean] => {
  const { rootMargin = '200px', threshold = 0.1, triggerOnce = true } = options;
  const ref = useRef<T>(null);
  const [isInView, setIsInView] = useState(false);

  useEffect(() => {
    const element = ref.current;
    if (!element) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        const inView = entry.isIntersecting;
        setIsInView(inView);
        
        if (inView && triggerOnce) {
          observer.disconnect();
        }
      },
      { rootMargin, threshold }
    );

    observer.observe(element);

    return () => observer.disconnect();
  }, [rootMargin, threshold, triggerOnce]);

  return [ref, isInView];
};

// ============================================
// Prefetch Data on Hover/Focus
// ============================================

interface UsePrefetchOptions<T> {
  fetchFn: () => Promise<T>;
  delay?: number; // Delay before prefetching (ms)
  staleTime?: number; // How long to consider data fresh (ms)
}

export const usePrefetch = <T>({ 
  fetchFn, 
  delay = 100, 
  staleTime = 5 * 60 * 1000 // 5 minutes
}: UsePrefetchOptions<T>) => {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const lastFetchTime = useRef<number>(0);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);

  const prefetch = useCallback(() => {
    const now = Date.now();
    
    // Skip if data is still fresh
    if (data && now - lastFetchTime.current < staleTime) {
      return;
    }

    // Clear any existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    // Delay prefetch to avoid unnecessary requests on quick hovers
    timeoutRef.current = setTimeout(async () => {
      if (isLoading) return;
      
      setIsLoading(true);
      try {
        const result = await fetchFn();
        setData(result);
        lastFetchTime.current = Date.now();
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Prefetch failed'));
      } finally {
        setIsLoading(false);
      }
    }, delay);
  }, [fetchFn, delay, staleTime, data, isLoading]);

  const cancelPrefetch = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
  }, []);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  return {
    data,
    isLoading,
    error,
    prefetch,
    cancelPrefetch,
    handlers: {
      onMouseEnter: prefetch,
      onMouseLeave: cancelPrefetch,
      onFocus: prefetch,
      onBlur: cancelPrefetch,
    },
  };
};

// ============================================
// Image Preloader
// ============================================

interface UseImagePreloaderOptions {
  concurrent?: number; // Max concurrent loads
  priority?: 'high' | 'low' | 'auto';
}

export const useImagePreloader = (options: UseImagePreloaderOptions = {}) => {
  const { concurrent = 3, priority = 'auto' } = options;
  const [loadedImages, setLoadedImages] = useState<Set<string>>(new Set());
  const [loadingImages, setLoadingImages] = useState<Set<string>>(new Set());
  const queueRef = useRef<string[]>([]);

  const processQueue = useCallback(() => {
    const toLoad = queueRef.current.splice(0, concurrent - loadingImages.size);
    
    toLoad.forEach((src) => {
      if (loadedImages.has(src) || loadingImages.has(src)) return;

      setLoadingImages((prev) => new Set(prev).add(src));

      const img = new Image();
      
      // Set loading priority hint
      if (priority !== 'auto') {
        // @ts-ignore - fetchPriority is valid but not in all TS versions
        img.fetchPriority = priority;
      }

      img.onload = () => {
        setLoadedImages((prev) => new Set(prev).add(src));
        setLoadingImages((prev) => {
          const next = new Set(prev);
          next.delete(src);
          return next;
        });
        processQueue();
      };

      img.onerror = () => {
        setLoadingImages((prev) => {
          const next = new Set(prev);
          next.delete(src);
          return next;
        });
        processQueue();
      };

      img.src = src;
    });
  }, [concurrent, loadedImages, loadingImages, priority]);

  const preload = useCallback((urls: string | string[]) => {
    const urlArray = Array.isArray(urls) ? urls : [urls];
    const newUrls = urlArray.filter(
      (url) => !loadedImages.has(url) && !loadingImages.has(url) && !queueRef.current.includes(url)
    );
    
    queueRef.current.push(...newUrls);
    processQueue();
  }, [loadedImages, loadingImages, processQueue]);

  const isLoaded = useCallback((url: string) => loadedImages.has(url), [loadedImages]);
  const isLoading = useCallback((url: string) => loadingImages.has(url), [loadingImages]);

  return {
    preload,
    isLoaded,
    isLoading,
    loadedCount: loadedImages.size,
    loadingCount: loadingImages.size,
  };
};

// ============================================
// Reduced Motion Detection
// ============================================

export const useReducedMotion = (): boolean => {
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(() => {
    if (typeof window === 'undefined') return false;
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  });

  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    
    const handleChange = (event: MediaQueryListEvent) => {
      setPrefersReducedMotion(event.matches);
    };

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []);

  return prefersReducedMotion;
};

// ============================================
// Idle Callback for Non-Critical Work
// ============================================

export const useIdleCallback = (
  callback: () => void,
  options: { timeout?: number; enabled?: boolean } = {}
) => {
  const { timeout = 2000, enabled = true } = options;

  useEffect(() => {
    if (!enabled) return;

    let id: number;

    if ('requestIdleCallback' in window) {
      id = window.requestIdleCallback(callback, { timeout });
      return () => window.cancelIdleCallback(id);
    } else {
      // Fallback for Safari
      const timeoutId = setTimeout(callback, 1);
      return () => clearTimeout(timeoutId);
    }
  }, [callback, timeout, enabled]);
};

// ============================================
// Virtual List for Large Lists
// ============================================

interface UseVirtualListOptions {
  itemHeight: number;
  containerHeight: number;
  overscan?: number;
}

export const useVirtualList = <T>(
  items: T[],
  options: UseVirtualListOptions
) => {
  const { itemHeight, containerHeight, overscan = 3 } = options;
  const [scrollTop, setScrollTop] = useState(0);

  const { startIndex, endIndex, visibleItems, totalHeight, offsetY } = useMemo(() => {
    const start = Math.max(0, Math.floor(scrollTop / itemHeight) - overscan);
    const visible = Math.ceil(containerHeight / itemHeight) + 2 * overscan;
    const end = Math.min(items.length, start + visible);

    return {
      startIndex: start,
      endIndex: end,
      visibleItems: items.slice(start, end),
      totalHeight: items.length * itemHeight,
      offsetY: start * itemHeight,
    };
  }, [items, scrollTop, itemHeight, containerHeight, overscan]);

  const handleScroll = useCallback((e: React.UIEvent<HTMLElement>) => {
    setScrollTop(e.currentTarget.scrollTop);
  }, []);

  return {
    visibleItems,
    startIndex,
    endIndex,
    totalHeight,
    offsetY,
    handleScroll,
    containerProps: {
      onScroll: handleScroll,
      style: { height: containerHeight, overflow: 'auto' },
    },
    innerProps: {
      style: { 
        height: totalHeight, 
        position: 'relative' as const,
      },
    },
    itemProps: {
      style: { 
        position: 'absolute' as const,
        top: offsetY,
        height: itemHeight,
        width: '100%',
      },
    },
  };
};

// ============================================
// Debounced Value (Optimized)
// ============================================

export const useDebouncedValue = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debouncedValue;
};

// ============================================
// Throttled Callback
// ============================================

export const useThrottledCallback = <T extends (...args: unknown[]) => unknown>(
  callback: T,
  delay: number
): T => {
  const lastRun = useRef(Date.now());

  return useCallback(
    ((...args) => {
      const now = Date.now();
      if (now - lastRun.current >= delay) {
        lastRun.current = now;
        return callback(...args);
      }
    }) as T,
    [callback, delay]
  );
};

// ============================================
// Progressive Image Quality
// ============================================

interface UseProgressiveImageOptions {
  lowQualitySrc: string;
  highQualitySrc: string;
}

export const useProgressiveImage = ({ lowQualitySrc, highQualitySrc }: UseProgressiveImageOptions) => {
  const [src, setSrc] = useState(lowQualitySrc);
  const [isHighQualityLoaded, setIsHighQualityLoaded] = useState(false);

  useEffect(() => {
    const img = new Image();
    img.src = highQualitySrc;
    img.onload = () => {
      setSrc(highQualitySrc);
      setIsHighQualityLoaded(true);
    };
  }, [highQualitySrc]);

  return { src, isHighQualityLoaded, isBlurred: !isHighQualityLoaded };
};

export default {
  useNetworkStatus,
  useLazyLoad,
  usePrefetch,
  useImagePreloader,
  useReducedMotion,
  useIdleCallback,
  useVirtualList,
  useDebouncedValue,
  useThrottledCallback,
  useProgressiveImage,
};
