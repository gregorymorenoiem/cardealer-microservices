/**
 * Custom hook for OKLA Analytics SDK
 * Sprint 9 - Event Tracking Service
 */

import { useEffect } from 'react';
import type { OklaAnalyticsConfig, SearchParams, VehicleViewParams, FilterParams } from '../types/okla-analytics';

/**
 * Hook to initialize OKLA Analytics SDK
 * Call this once in your App component
 */
export const useOklaAnalytics = (config?: OklaAnalyticsConfig) => {
  useEffect(() => {
    // Wait for SDK to load
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      const defaultConfig: OklaAnalyticsConfig = {
        apiUrl: import.meta.env.VITE_API_URL || 'http://localhost:8080',
        batchSize: 10,
        flushIntervalMs: 5000,
        autoTrack: true,
        debug: import.meta.env.DEV, // Enable debug in development
      };

      window.OklaAnalytics.init({ ...defaultConfig, ...config });
      
      console.log('[OKLA Analytics] SDK initialized');
    } else {
      console.warn('[OKLA Analytics] SDK not loaded. Make sure okla-analytics.js is included in index.html');
    }
  }, []);
};

/**
 * Hook to set user ID when user logs in
 */
export const useAnalyticsUserId = (userId: string | null) => {
  useEffect(() => {
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      if (userId) {
        window.OklaAnalytics.setUserId(userId);
      } else {
        window.OklaAnalytics.clearUserId();
      }
    }
  }, [userId]);
};

/**
 * Hook to provide analytics tracking methods
 * Use this in components that need to track events
 */
export const useAnalyticsTracking = () => {
  const trackSearch = (params: SearchParams) => {
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      window.OklaAnalytics.trackSearch(params);
    }
  };

  const trackVehicleView = (params: VehicleViewParams) => {
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      window.OklaAnalytics.trackVehicleView(params);
    }
  };

  const trackFilter = (params: FilterParams) => {
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      window.OklaAnalytics.trackFilter(params);
    }
  };

  const trackCustomEvent = (eventType: string, data?: Record<string, any>) => {
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      window.OklaAnalytics.trackCustom(eventType, data);
    }
  };

  const flush = () => {
    if (typeof window !== 'undefined' && window.OklaAnalytics) {
      window.OklaAnalytics.flush();
    }
  };

  return {
    trackSearch,
    trackVehicleView,
    trackFilter,
    trackCustomEvent,
    flush,
  };
};
