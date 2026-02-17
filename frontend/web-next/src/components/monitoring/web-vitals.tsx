'use client';

/**
 * Web Vitals Component
 *
 * Tracks and reports Core Web Vitals metrics:
 * - LCP (Largest Contentful Paint): Loading performance
 * - FID (First Input Delay): Interactivity
 * - CLS (Cumulative Layout Shift): Visual stability
 * - FCP (First Contentful Paint): Initial render
 * - TTFB (Time to First Byte): Server response
 * - INP (Interaction to Next Paint): Responsiveness
 *
 * Can send metrics to analytics service for monitoring.
 */

import { useEffect } from 'react';
import { usePathname } from 'next/navigation';

type WebVitalsMetric = {
  id: string;
  name: string;
  value: number;
  rating: 'good' | 'needs-improvement' | 'poor';
  delta: number;
  navigationType: string;
};

interface WebVitalsConfig {
  /** Whether to enable vitals tracking */
  enabled?: boolean;
  /** Whether to log to console (dev mode) */
  debug?: boolean;
  /** Custom analytics endpoint */
  analyticsEndpoint?: string;
  /** Callback when metric is captured */
  onMetric?: (metric: WebVitalsMetric) => void;
}

const DEFAULT_CONFIG: WebVitalsConfig = {
  enabled: true,
  debug: process.env.NODE_ENV === 'development',
  analyticsEndpoint: process.env.NEXT_PUBLIC_ANALYTICS_ENDPOINT,
};

// Thresholds for Core Web Vitals ratings
const THRESHOLDS = {
  LCP: { good: 2500, poor: 4000 },
  CLS: { good: 0.1, poor: 0.25 },
  FCP: { good: 1800, poor: 3000 },
  TTFB: { good: 800, poor: 1800 },
  INP: { good: 200, poor: 500 },
};

function getRating(name: string, value: number): 'good' | 'needs-improvement' | 'poor' {
  const threshold = THRESHOLDS[name as keyof typeof THRESHOLDS];
  if (!threshold) return 'good';

  if (value <= threshold.good) return 'good';
  if (value <= threshold.poor) return 'needs-improvement';
  return 'poor';
}

function formatMetricValue(name: string, value: number): string {
  switch (name) {
    case 'CLS':
      return value.toFixed(3);
    case 'LCP':
    case 'FCP':
    case 'TTFB':
    case 'FID':
    case 'INP':
      return `${Math.round(value)}ms`;
    default:
      return value.toFixed(2);
  }
}

// Color codes for console logging
const RATING_COLORS = {
  good: '\x1b[32m', // Green
  'needs-improvement': '\x1b[33m', // Yellow
  poor: '\x1b[31m', // Red
};

const RESET_COLOR = '\x1b[0m';

/**
 * Send metric to analytics endpoint
 */
async function sendToAnalytics(metric: WebVitalsMetric, endpoint: string, pathname: string) {
  try {
    await fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ...metric,
        pathname,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        connection: (navigator as unknown as { connection?: { effectiveType: string } }).connection
          ?.effectiveType,
      }),
      keepalive: true,
    });
  } catch {
    // Silently fail - don't block user experience
  }
}

/**
 * WebVitals tracking component
 */
export function WebVitals(config: WebVitalsConfig = {}) {
  const pathname = usePathname();
  const { enabled, debug, analyticsEndpoint, onMetric } = {
    ...DEFAULT_CONFIG,
    ...config,
  };

  useEffect(() => {
    if (!enabled) return;

    // Dynamic import of web-vitals library
    import('web-vitals')
      .then(({ onLCP, onCLS, onFCP, onTTFB, onINP }) => {
        const handleMetric = (metric: {
          id: string;
          name: string;
          value: number;
          delta: number;
          navigationType: string;
        }) => {
          const rating = getRating(metric.name, metric.value);
          const webVitalsMetric: WebVitalsMetric = {
            ...metric,
            rating,
          };

          // Debug logging
          if (debug) {
            const color = RATING_COLORS[rating];
            console.log(
              `${color}[Web Vitals]${RESET_COLOR} ${metric.name}: ${formatMetricValue(metric.name, metric.value)} (${rating})`
            );
          }

          // Custom callback
          if (onMetric) {
            onMetric(webVitalsMetric);
          }

          // Send to analytics
          if (analyticsEndpoint) {
            sendToAnalytics(webVitalsMetric, analyticsEndpoint, pathname);
          }
        };

        // Register all vitals (FID deprecated in v5, replaced by INP)
        onLCP(handleMetric);
        onCLS(handleMetric);
        onFCP(handleMetric);
        onTTFB(handleMetric);
        onINP(handleMetric);
      })
      .catch(() => {
        // web-vitals library not available
        if (debug) {
          console.warn('[Web Vitals] Library not available');
        }
      });
  }, [enabled, debug, analyticsEndpoint, onMetric, pathname]);

  // This component doesn't render anything
  return null;
}

/**
 * Hook to track custom performance marks
 */
export function usePerformanceMark() {
  const mark = (name: string) => {
    if (typeof performance !== 'undefined') {
      performance.mark(name);
    }
  };

  const measure = (name: string, startMark: string, endMark?: string) => {
    if (typeof performance !== 'undefined') {
      try {
        const entry = performance.measure(name, startMark, endMark || performance.now().toString());
        return entry.duration;
      } catch {
        return null;
      }
    }
    return null;
  };

  return { mark, measure };
}

/**
 * Hook to track component render time
 */
export function useRenderTime(componentName: string) {
  useEffect(() => {
    const startTime = performance.now();

    return () => {
      const endTime = performance.now();
      const renderTime = endTime - startTime;

      if (process.env.NODE_ENV === 'development') {
        console.log(`[Render Time] ${componentName}: ${renderTime.toFixed(2)}ms`);
      }
    };
  }, [componentName]);
}

export default WebVitals;
