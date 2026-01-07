/**
 * Environment Configuration
 * 
 * Centralized access to environment variables with type safety and defaults.
 * This module provides a single source of truth for all configuration.
 * 
 * @module config/env
 * @updated 2025-01-02 - Production API URL fix via VITE_API_URL build arg
 */

/**
 * API Configuration
 */
export const apiConfig = {
  /** Main API Gateway URL */
  baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:8080',
  
  /** Individual service URLs (for direct access) */
  services: {
    auth: import.meta.env.VITE_AUTH_SERVICE_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/auth`,
    admin: import.meta.env.VITE_ADMIN_SERVICE_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/admin`,
    vehicle: import.meta.env.VITE_VEHICLE_SERVICE_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/vehicles`,
    message: import.meta.env.VITE_MESSAGE_SERVICE_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/messages`,
    notification: import.meta.env.VITE_NOTIFICATION_SERVICE_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/notifications`,
    upload: import.meta.env.VITE_UPLOAD_SERVICE_URL || `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/upload`,
  },
} as const;

/**
 * Application Configuration
 */
export const appConfig = {
  /** Application name */
  name: import.meta.env.VITE_APP_NAME || 'CarDealer',
  
  /** Application version */
  version: import.meta.env.VITE_APP_VERSION || '1.0.0',
  
  /** Base URL for SEO and sharing */
  baseUrl: import.meta.env.VITE_BASE_URL || 'https://cardealer.do',
  
  /** Current environment */
  environment: import.meta.env.MODE || 'development',
  
  /** Is production environment */
  isProduction: import.meta.env.PROD,
  
  /** Is development environment */
  isDevelopment: import.meta.env.DEV,
} as const;

/**
 * Feature Flags
 */
export const featureFlags = {
  /** Use mock authentication (for development) */
  useMockAuth: import.meta.env.VITE_USE_MOCK_AUTH === 'true',
  
  /** Enable Mock Service Worker */
  enableMsw: import.meta.env.VITE_ENABLE_MSW === 'true',
} as const;

/**
 * Monitoring Configuration
 */
export const monitoringConfig = {
  /** Sentry DSN for error tracking */
  sentryDsn: import.meta.env.VITE_SENTRY_DSN || '',
  
  /** Google Analytics tracking ID */
  gaTrackingId: import.meta.env.VITE_GA_TRACKING_ID || '',
  
  /** Is Sentry enabled */
  isSentryEnabled: !!import.meta.env.VITE_SENTRY_DSN,
  
  /** Is GA enabled */
  isGaEnabled: !!import.meta.env.VITE_GA_TRACKING_ID,
} as const;

/**
 * Third-party Integration Configuration
 */
export const integrationConfig = {
  /** Google Maps API Key */
  googleMapsKey: import.meta.env.VITE_GOOGLE_MAPS_KEY || '',
  
  /** reCAPTCHA Site Key */
  recaptchaSiteKey: import.meta.env.VITE_RECAPTCHA_SITE_KEY || '',
} as const;

/**
 * Runtime Configuration (from docker-entrypoint.sh)
 * This allows runtime configuration override for Kubernetes deployments
 */
export const runtimeConfig = {
  /** Override API URL at runtime */
  apiUrl: (window as any).__RUNTIME_CONFIG__?.API_URL || null,
  
  /** Override version at runtime */
  appVersion: (window as any).__RUNTIME_CONFIG__?.APP_VERSION || null,
  
  /** Override Sentry DSN at runtime */
  sentryDsn: (window as any).__RUNTIME_CONFIG__?.SENTRY_DSN || null,
  
  /** Runtime environment */
  environment: (window as any).__RUNTIME_CONFIG__?.ENVIRONMENT || null,
} as const;

/**
 * Get effective API URL (runtime override takes precedence)
 */
export function getApiUrl(): string {
  return runtimeConfig.apiUrl || apiConfig.baseUrl;
}

/**
 * Get effective app version (runtime override takes precedence)
 */
export function getAppVersion(): string {
  return runtimeConfig.appVersion || appConfig.version;
}

/**
 * Get effective Sentry DSN (runtime override takes precedence)
 */
export function getSentryDsn(): string {
  return runtimeConfig.sentryDsn || monitoringConfig.sentryDsn;
}

/**
 * Full configuration export
 */
export const config = {
  api: apiConfig,
  app: appConfig,
  features: featureFlags,
  monitoring: monitoringConfig,
  integrations: integrationConfig,
  runtime: runtimeConfig,
  
  // Helper functions
  getApiUrl,
  getAppVersion,
  getSentryDsn,
} as const;

export default config;
