/**
 * Service Worker Registration
 * Enables offline support and caching for better performance in low-bandwidth areas
 * 
 * Features:
 * - Cache-first strategy for static assets
 * - Network-first for API calls
 * - Offline fallback page
 * - Background sync for failed requests
 */

// Check if service workers are supported
const isServiceWorkerSupported = 'serviceWorker' in navigator;

export interface ServiceWorkerConfig {
  onSuccess?: (registration: ServiceWorkerRegistration) => void;
  onUpdate?: (registration: ServiceWorkerRegistration) => void;
  onOffline?: () => void;
  onOnline?: () => void;
}

export const registerServiceWorker = async (config?: ServiceWorkerConfig) => {
  if (!isServiceWorkerSupported) {
    console.log('Service Workers not supported');
    return;
  }

  // Only register in production
  if (import.meta.env.DEV) {
    console.log('Service Worker disabled in development');
    return;
  }

  try {
    const registration = await navigator.serviceWorker.register('/sw.js', {
      scope: '/',
    });

    // Check for updates
    registration.addEventListener('updatefound', () => {
      const installingWorker = registration.installing;
      if (!installingWorker) return;

      installingWorker.addEventListener('statechange', () => {
        if (installingWorker.state === 'installed') {
          if (navigator.serviceWorker.controller) {
            // New content available
            console.log('New content available, please refresh.');
            config?.onUpdate?.(registration);
          } else {
            // Content cached for offline use
            console.log('Content cached for offline use.');
            config?.onSuccess?.(registration);
          }
        }
      });
    });

    // Handle offline/online events
    window.addEventListener('offline', () => {
      console.log('App is offline');
      config?.onOffline?.();
    });

    window.addEventListener('online', () => {
      console.log('App is online');
      config?.onOnline?.();
    });

    return registration;
  } catch (error) {
    console.error('Service Worker registration failed:', error);
  }
};

export const unregisterServiceWorker = async () => {
  if (!isServiceWorkerSupported) return;

  const registration = await navigator.serviceWorker.ready;
  await registration.unregister();
};

// ============================================
// Cache Management Utilities
// ============================================

export const clearCache = async (cacheName?: string) => {
  if (!('caches' in window)) return;

  if (cacheName) {
    await caches.delete(cacheName);
  } else {
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames.map(name => caches.delete(name)));
  }
};

export const getCacheSize = async (): Promise<number> => {
  if (!('caches' in window) || !navigator.storage?.estimate) {
    return 0;
  }

  const estimate = await navigator.storage.estimate();
  return estimate.usage || 0;
};

export const formatBytes = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

export default {
  registerServiceWorker,
  unregisterServiceWorker,
  clearCache,
  getCacheSize,
  formatBytes,
};
