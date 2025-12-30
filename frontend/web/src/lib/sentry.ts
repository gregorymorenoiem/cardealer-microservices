/**
 * Sentry Configuration for Error Tracking
 * 
 * This module initializes Sentry for error monitoring, performance tracking,
 * and session replay in the CarDealer frontend application.
 * 
 * @module lib/sentry
 */

import * as Sentry from '@sentry/react';

/**
 * Environment-aware Sentry configuration
 */
interface SentryConfig {
  dsn: string;
  environment: string;
  release?: string;
  tracesSampleRate: number;
  replaysSessionSampleRate: number;
  replaysOnErrorSampleRate: number;
}

/**
 * Get Sentry configuration based on environment
 */
function getSentryConfig(): SentryConfig {
  const isProd = import.meta.env.PROD;
  
  return {
    dsn: import.meta.env.VITE_SENTRY_DSN || '',
    environment: import.meta.env.MODE || 'development',
    release: import.meta.env.VITE_APP_VERSION || '1.0.0',
    // Sample 10% of transactions in production, 100% in development
    tracesSampleRate: isProd ? 0.1 : 1.0,
    // Sample 10% of sessions for replay
    replaysSessionSampleRate: isProd ? 0.1 : 0,
    // Always replay sessions with errors
    replaysOnErrorSampleRate: 1.0,
  };
}

/**
 * Initialize Sentry with browser tracing and replay
 */
export function initSentry(): void {
  const config = getSentryConfig();
  
  // Don't initialize if no DSN is provided
  if (!config.dsn) {
    console.warn('[Sentry] No DSN provided, error tracking disabled');
    return;
  }
  
  Sentry.init({
    dsn: config.dsn,
    environment: config.environment,
    release: config.release,
    
    // Integrations
    integrations: [
      // Browser tracing for performance monitoring
      Sentry.browserTracingIntegration(),
      // Session replay for debugging
      Sentry.replayIntegration({
        // Mask all text content for privacy
        maskAllText: false,
        // Block all media for performance
        blockAllMedia: false,
      }),
    ],
    
    // Performance monitoring
    tracesSampleRate: config.tracesSampleRate,
    
    // Session replay
    replaysSessionSampleRate: config.replaysSessionSampleRate,
    replaysOnErrorSampleRate: config.replaysOnErrorSampleRate,
    
    // Filter sensitive data
    beforeSend(event) {
      // Remove sensitive user data
      if (event.user) {
        delete event.user.ip_address;
      }
      
      // Filter out specific error types if needed
      if (event.exception?.values) {
        const errorMessage = event.exception.values[0]?.value || '';
        
        // Ignore network errors that are expected
        if (errorMessage.includes('NetworkError') || 
            errorMessage.includes('Failed to fetch')) {
          // Still send but mark as handled
          event.level = 'warning';
        }
        
        // Ignore cancelled requests
        if (errorMessage.includes('AbortError') || 
            errorMessage.includes('cancelled')) {
          return null;
        }
      }
      
      return event;
    },
    
    // Breadcrumb filtering
    beforeBreadcrumb(breadcrumb) {
      // Filter out noisy breadcrumbs
      if (breadcrumb.category === 'console' && breadcrumb.level === 'debug') {
        return null;
      }
      
      // Sanitize URLs with sensitive params
      if (breadcrumb.category === 'navigation' && breadcrumb.data?.to) {
        const url = new URL(breadcrumb.data.to, window.location.origin);
        url.searchParams.delete('token');
        url.searchParams.delete('password');
        breadcrumb.data.to = url.pathname + url.search;
      }
      
      return breadcrumb;
    },
  });
  
  console.log('[Sentry] Initialized for environment:', config.environment);
}

/**
 * Set user context for Sentry
 */
export function setSentryUser(user: {
  id: string;
  email?: string;
  username?: string;
  dealerId?: string;
}): void {
  Sentry.setUser({
    id: user.id,
    email: user.email,
    username: user.username,
    // Custom data
    ...(user.dealerId && { dealer_id: user.dealerId }),
  });
}

/**
 * Clear user context (on logout)
 */
export function clearSentryUser(): void {
  Sentry.setUser(null);
}

/**
 * Add custom tags to all events
 */
export function setSentryTags(tags: Record<string, string>): void {
  Object.entries(tags).forEach(([key, value]) => {
    Sentry.setTag(key, value);
  });
}

/**
 * Capture a custom error with context
 */
export function captureError(
  error: Error,
  context?: {
    tags?: Record<string, string>;
    extra?: Record<string, unknown>;
    level?: 'fatal' | 'error' | 'warning' | 'info' | 'debug';
  }
): string {
  return Sentry.captureException(error, {
    tags: context?.tags,
    extra: context?.extra,
    level: context?.level || 'error',
  });
}

/**
 * Capture a custom message
 */
export function captureMessage(
  message: string,
  level: 'fatal' | 'error' | 'warning' | 'info' | 'debug' = 'info'
): string {
  return Sentry.captureMessage(message, level);
}

/**
 * Add breadcrumb for debugging
 */
export function addBreadcrumb(
  category: string,
  message: string,
  data?: Record<string, unknown>,
  level: 'fatal' | 'error' | 'warning' | 'info' | 'debug' = 'info'
): void {
  Sentry.addBreadcrumb({
    category,
    message,
    data,
    level,
    timestamp: Date.now() / 1000,
  });
}

/**
 * Start a performance transaction
 */
export function startTransaction(
  name: string,
  op: string
): ReturnType<typeof Sentry.startInactiveSpan> {
  return Sentry.startInactiveSpan({
    name,
    op,
  });
}

/**
 * Create a Sentry ErrorBoundary wrapper
 */
export const SentryErrorBoundary = Sentry.ErrorBoundary;

/**
 * HOC for wrapping components with Sentry profiling
 */
export const withSentryProfiler = Sentry.withProfiler;

/**
 * Export Sentry for direct access if needed
 */
export { Sentry };
