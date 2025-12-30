/**
 * Web Vitals Reporting
 * 
 * Captures and reports Core Web Vitals metrics:
 * - LCP (Largest Contentful Paint)
 * - FID (First Input Delay)
 * - CLS (Cumulative Layout Shift)
 * - FCP (First Contentful Paint)
 * - TTFB (Time to First Byte)
 * - INP (Interaction to Next Paint)
 * 
 * Integrates with analytics and monitoring services.
 */

import type { Metric } from 'web-vitals';
import { onCLS, onFCP, onINP, onLCP, onTTFB } from 'web-vitals';

type MetricHandler = (metric: Metric) => void;

interface WebVitalsConfig {
  /** Enable console logging in development */
  debug?: boolean;
  /** Report to analytics endpoint */
  analyticsEndpoint?: string;
  /** Report to Sentry */
  reportToSentry?: boolean;
  /** Custom metric handler */
  onMetric?: MetricHandler;
  /** Sample rate (0-1) for reporting */
  sampleRate?: number;
}

interface MetricReport {
  name: string;
  value: number;
  rating: 'good' | 'needs-improvement' | 'poor';
  delta: number;
  id: string;
  navigationType: string;
  url: string;
  timestamp: number;
}

const VITALS_THRESHOLDS = {
  LCP: { good: 2500, poor: 4000 },
  FID: { good: 100, poor: 300 },
  CLS: { good: 0.1, poor: 0.25 },
  FCP: { good: 1800, poor: 3000 },
  TTFB: { good: 800, poor: 1800 },
  INP: { good: 200, poor: 500 },
};

/**
 * Get rating for a metric value
 */
function getRating(name: string, value: number): 'good' | 'needs-improvement' | 'poor' {
  const thresholds = VITALS_THRESHOLDS[name as keyof typeof VITALS_THRESHOLDS];
  if (!thresholds) return 'good';
  
  if (value <= thresholds.good) return 'good';
  if (value > thresholds.poor) return 'poor';
  return 'needs-improvement';
}

/**
 * Format metric for reporting
 */
function formatMetric(metric: Metric): MetricReport {
  return {
    name: metric.name,
    value: Math.round(metric.name === 'CLS' ? metric.value * 1000 : metric.value),
    rating: metric.rating,
    delta: Math.round(metric.name === 'CLS' ? metric.delta * 1000 : metric.delta),
    id: metric.id,
    navigationType: metric.navigationType,
    url: window.location.href,
    timestamp: Date.now(),
  };
}

/**
 * Log metric to console with color coding
 */
function logMetric(metric: MetricReport) {
  const colors = {
    good: 'color: #0cce6b',
    'needs-improvement': 'color: #ffa400',
    poor: 'color: #ff4e42',
  };

  console.log(
    `%c[Web Vitals] ${metric.name}: ${metric.value}${metric.name === 'CLS' ? '' : 'ms'} (${metric.rating})`,
    colors[metric.rating]
  );
}

/**
 * Send metric to analytics endpoint
 */
async function sendToAnalytics(metric: MetricReport, endpoint: string) {
  try {
    // Use sendBeacon for reliable delivery
    if (navigator.sendBeacon) {
      navigator.sendBeacon(
        endpoint,
        JSON.stringify(metric)
      );
    } else {
      // Fallback to fetch
      await fetch(endpoint, {
        method: 'POST',
        body: JSON.stringify(metric),
        headers: {
          'Content-Type': 'application/json',
        },
        keepalive: true,
      });
    }
  } catch (error) {
    console.error('[Web Vitals] Failed to send metrics:', error);
  }
}

/**
 * Report metric to Sentry
 */
function reportToSentry(metric: MetricReport) {
  // Dynamically import Sentry to avoid bundling if not used
  import('@sentry/react').then(Sentry => {
    Sentry.setMeasurement(metric.name, metric.value, metric.name === 'CLS' ? '' : 'millisecond');
    
    // Add as custom tag for filtering
    Sentry.setTag(`web_vital_${metric.name.toLowerCase()}`, metric.rating);
    
    // Report poor metrics as breadcrumbs
    if (metric.rating === 'poor') {
      Sentry.addBreadcrumb({
        category: 'web-vitals',
        message: `Poor ${metric.name}: ${metric.value}`,
        level: 'warning',
        data: metric,
      });
    }
  }).catch(() => {
    // Sentry not available
  });
}

/**
 * Create metric handler with configuration
 */
function createMetricHandler(config: WebVitalsConfig): MetricHandler {
  const { debug, analyticsEndpoint, reportToSentry: sendToSentry, onMetric, sampleRate = 1 } = config;
  
  return (metric: Metric) => {
    // Apply sample rate
    if (Math.random() > sampleRate) return;
    
    const report = formatMetric(metric);
    
    // Debug logging
    if (debug) {
      logMetric(report);
    }
    
    // Send to analytics endpoint
    if (analyticsEndpoint) {
      sendToAnalytics(report, analyticsEndpoint);
    }
    
    // Report to Sentry
    if (sendToSentry) {
      reportToSentry(report);
    }
    
    // Custom handler
    if (onMetric) {
      onMetric(metric);
    }
  };
}

/**
 * Initialize Web Vitals reporting
 * 
 * @example
 * ```ts
 * // Basic usage with debug logging
 * initWebVitals({ debug: true });
 * 
 * // Production with analytics
 * initWebVitals({
 *   analyticsEndpoint: '/api/analytics/vitals',
 *   reportToSentry: true,
 *   sampleRate: 0.1, // 10% of users
 * });
 * 
 * // Custom handler
 * initWebVitals({
 *   onMetric: (metric) => {
 *     gtag('event', metric.name, {
 *       value: metric.value,
 *       event_category: 'Web Vitals',
 *       event_label: metric.id,
 *       non_interaction: true,
 *     });
 *   },
 * });
 * ```
 */
export function initWebVitals(config: WebVitalsConfig = {}) {
  const isDev = import.meta.env.DEV;
  
  // Default config
  const finalConfig: WebVitalsConfig = {
    debug: isDev,
    reportToSentry: !isDev,
    sampleRate: isDev ? 1 : 0.25, // 25% sampling in production
    ...config,
  };
  
  const handler = createMetricHandler(finalConfig);
  
  // Register all Core Web Vitals
  onCLS(handler);
  onFCP(handler);
  onINP(handler);
  onLCP(handler);
  onTTFB(handler);
  
  if (isDev) {
    console.log('[Web Vitals] Monitoring initialized');
  }
}

/**
 * Get current performance summary
 * Useful for displaying in admin dashboards
 */
export function getPerformanceSummary(): Promise<Record<string, MetricReport>> {
  return new Promise((resolve) => {
    const metrics: Record<string, MetricReport> = {};
    let resolved = false;
    
    const checkComplete = () => {
      if (Object.keys(metrics).length >= 5 && !resolved) {
        resolved = true;
        resolve(metrics);
      }
    };
    
    const handler: MetricHandler = (metric) => {
      metrics[metric.name] = formatMetric(metric);
      checkComplete();
    };
    
    onCLS(handler);
    onFCP(handler);
    onINP(handler);
    onLCP(handler);
    onTTFB(handler);
    
    // Timeout after 10 seconds
    setTimeout(() => {
      if (!resolved) {
        resolved = true;
        resolve(metrics);
      }
    }, 10000);
  });
}

/**
 * Report custom timing metric
 */
export function reportCustomTiming(name: string, value: number, category: string = 'custom') {
  const metric: MetricReport = {
    name,
    value: Math.round(value),
    rating: getRating(name, value),
    delta: value,
    id: `${name}-${Date.now()}`,
    navigationType: 'custom',
    url: window.location.href,
    timestamp: Date.now(),
  };
  
  if (import.meta.env.DEV) {
    console.log(`[Custom Timing] ${category}/${name}: ${value}ms`);
  }
  
  // Report to Sentry
  import('@sentry/react').then(Sentry => {
    Sentry.setMeasurement(name, value, 'millisecond');
  }).catch(() => {});
  
  return metric;
}

/**
 * Create a performance marker for custom timings
 */
export function createPerformanceMarker(name: string) {
  const startTime = performance.now();
  
  return {
    /** End the marker and report the timing */
    end: (category: string = 'custom') => {
      const duration = performance.now() - startTime;
      return reportCustomTiming(name, duration, category);
    },
    /** Get elapsed time without ending */
    elapsed: () => performance.now() - startTime,
  };
}

export default initWebVitals;
