'use client';

import { useEffect, useState, useCallback } from 'react';

interface ServiceWorkerConfig {
  /** Path to the service worker file */
  swPath?: string;
  /** Callback when a new version is available */
  onUpdate?: (registration: ServiceWorkerRegistration) => void;
  /** Callback when the service worker is successfully registered */
  onSuccess?: (registration: ServiceWorkerRegistration) => void;
  /** Callback on error */
  onError?: (error: Error) => void;
  /** Enable auto-reload on update */
  autoReload?: boolean;
}

interface UseServiceWorkerReturn {
  /** Whether the service worker is registered */
  isRegistered: boolean;
  /** Whether a new version is available */
  hasUpdate: boolean;
  /** Whether the app is offline */
  isOffline: boolean;
  /** Update to the new version */
  updateServiceWorker: () => void;
  /** Skip waiting and activate new service worker immediately */
  skipWaiting: () => void;
  /** Clear all caches */
  clearCache: () => Promise<void>;
  /** Current service worker registration */
  registration: ServiceWorkerRegistration | null;
}

/**
 * Hook for managing service worker lifecycle
 */
export function useServiceWorker(config: ServiceWorkerConfig = {}): UseServiceWorkerReturn {
  const { swPath = '/sw.js', onUpdate, onSuccess, onError, autoReload = false } = config;

  const [isRegistered, setIsRegistered] = useState(false);
  const [hasUpdate, setHasUpdate] = useState(false);
  const [isOffline, setIsOffline] = useState(false);
  const [registration, setRegistration] = useState<ServiceWorkerRegistration | null>(null);
  const [waitingWorker, setWaitingWorker] = useState<ServiceWorker | null>(null);

  // Handle online/offline status
  useEffect(() => {
    const updateOnlineStatus = () => {
      setIsOffline(!navigator.onLine);
    };

    setIsOffline(!navigator.onLine);

    window.addEventListener('online', updateOnlineStatus);
    window.addEventListener('offline', updateOnlineStatus);

    return () => {
      window.removeEventListener('online', updateOnlineStatus);
      window.removeEventListener('offline', updateOnlineStatus);
    };
  }, []);

  // Register service worker
  useEffect(() => {
    if (typeof window === 'undefined' || !('serviceWorker' in navigator)) {
      console.log('[SW] Service workers not supported');
      return;
    }

    // Only register in production or when explicitly enabled
    if (process.env.NODE_ENV !== 'production' && !process.env.NEXT_PUBLIC_ENABLE_SW) {
      console.log('[SW] Skipping registration in development');
      return;
    }

    const registerServiceWorker = async () => {
      try {
        const reg = await navigator.serviceWorker.register(swPath, {
          scope: '/',
          updateViaCache: 'none',
        });

        console.log('[SW] Service worker registered:', reg.scope);
        setRegistration(reg);
        setIsRegistered(true);

        // Check if there's a waiting worker
        if (reg.waiting) {
          setWaitingWorker(reg.waiting);
          setHasUpdate(true);
          onUpdate?.(reg);
        }

        // Listen for update found
        reg.addEventListener('updatefound', () => {
          const newWorker = reg.installing;
          if (!newWorker) return;

          newWorker.addEventListener('statechange', () => {
            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
              console.log('[SW] New version available');
              setWaitingWorker(newWorker);
              setHasUpdate(true);
              onUpdate?.(reg);

              if (autoReload) {
                newWorker.postMessage({ type: 'SKIP_WAITING' });
              }
            }
          });
        });

        // Check for updates periodically (every hour)
        setInterval(
          () => {
            reg.update();
          },
          60 * 60 * 1000
        );

        onSuccess?.(reg);
      } catch (error) {
        console.error('[SW] Registration failed:', error);
        onError?.(error as Error);
      }
    };

    // Wait for page load
    if (document.readyState === 'complete') {
      registerServiceWorker();
    } else {
      window.addEventListener('load', registerServiceWorker);
    }

    // Listen for controller change (new SW activated)
    const handleControllerChange = () => {
      console.log('[SW] Controller changed, reloading...');
      window.location.reload();
    };

    navigator.serviceWorker.addEventListener('controllerchange', handleControllerChange);

    return () => {
      navigator.serviceWorker.removeEventListener('controllerchange', handleControllerChange);
    };
  }, [swPath, onUpdate, onSuccess, onError, autoReload]);

  // Update to new service worker
  const updateServiceWorker = useCallback(() => {
    if (registration) {
      registration.update();
    }
  }, [registration]);

  // Skip waiting and activate new service worker
  const skipWaiting = useCallback(() => {
    if (waitingWorker) {
      waitingWorker.postMessage({ type: 'SKIP_WAITING' });
    }
  }, [waitingWorker]);

  // Clear all caches
  const clearCache = useCallback(async () => {
    if ('caches' in window) {
      const cacheNames = await caches.keys();
      await Promise.all(cacheNames.map(name => caches.delete(name)));
      console.log('[SW] All caches cleared');
    }
  }, []);

  return {
    isRegistered,
    hasUpdate,
    isOffline,
    updateServiceWorker,
    skipWaiting,
    clearCache,
    registration,
  };
}

/**
 * Component that registers the service worker and provides update UI
 */
interface ServiceWorkerProviderProps {
  children: React.ReactNode;
  /** Show update notification UI */
  showUpdateNotification?: boolean;
}

export function ServiceWorkerProvider({
  children,
  showUpdateNotification = true,
}: ServiceWorkerProviderProps) {
  const { hasUpdate, skipWaiting, isOffline } = useServiceWorker({
    autoReload: false,
  });

  const [showNotification, setShowNotification] = useState(false);

  useEffect(() => {
    if (hasUpdate && showUpdateNotification) {
      setShowNotification(true);
    }
  }, [hasUpdate, showUpdateNotification]);

  const handleUpdate = () => {
    setShowNotification(false);
    skipWaiting();
  };

  const handleDismiss = () => {
    setShowNotification(false);
  };

  return (
    <>
      {children}

      {/* Offline indicator */}
      {isOffline && (
        <div className="fixed top-0 right-0 left-0 z-50 bg-yellow-500 py-2 text-center text-sm font-medium text-yellow-900">
          <span className="flex items-center justify-center gap-2">
            <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21"
              />
            </svg>
            Sin conexión a internet
          </span>
        </div>
      )}

      {/* Update notification */}
      {showNotification && (
        <div className="animate-slide-up fixed right-4 bottom-20 left-4 z-50 rounded-xl bg-blue-600 p-4 text-white shadow-2xl sm:right-4 sm:left-auto sm:w-80">
          <div className="flex items-start gap-3">
            <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-blue-500">
              <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                />
              </svg>
            </div>
            <div className="flex-1">
              <h3 className="mb-1 font-semibold">Nueva versión disponible</h3>
              <p className="mb-3 text-sm text-blue-100">
                Hay una nueva versión de OKLA disponible.
              </p>
              <div className="flex gap-2">
                <button
                  onClick={handleUpdate}
                  className="rounded-lg bg-white px-4 py-1.5 text-sm font-medium text-blue-600 transition-colors hover:bg-blue-50"
                >
                  Actualizar
                </button>
                <button
                  onClick={handleDismiss}
                  className="rounded-lg px-4 py-1.5 text-sm font-medium text-blue-100 transition-colors hover:text-white"
                >
                  Después
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

export default ServiceWorkerProvider;
