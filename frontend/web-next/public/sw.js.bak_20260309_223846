/**
 * Service Worker for OKLA PWA
 *
 * Features:
 * - Cache-first strategy for static assets
 * - Network-first for API calls
 * - Offline fallback page
 * - Background sync for form submissions
 * - Push notifications support
 */

const CACHE_VERSION = 'v1.0.0';
const STATIC_CACHE = `okla-static-${CACHE_VERSION}`;
const DYNAMIC_CACHE = `okla-dynamic-${CACHE_VERSION}`;
const IMAGE_CACHE = `okla-images-${CACHE_VERSION}`;

// Assets to cache immediately on install
const STATIC_ASSETS = [
  '/',
  '/offline',
  '/vehiculos',
  '/manifest.json',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png',
];

// Routes that should use network-first strategy
const NETWORK_FIRST_ROUTES = ['/api/', '/auth/', '/dashboard', '/mis-vehiculos', '/dealer/'];

// Routes that should never be cached
const NO_CACHE_ROUTES = ['/api/auth', '/api/checkout', '/api/payment'];

// Routes to completely ignore (Next.js internals, HMR, etc.)
const IGNORE_ROUTES = ['/_next/', '/__nextjs', '/__webpack_hmr', '/sw.js'];

/**
 * Safely put a response into cache.
 * Cache.put() throws on opaque responses, redirects, and network errors.
 */
async function safeCachePut(cache, request, response) {
  try {
    // Only cache basic (same-origin) responses with status 200
    if (response.type === 'opaque' || response.status !== 200 || response.type === 'error') {
      return;
    }
    await cache.put(request, response);
  } catch (error) {
    console.warn('[SW] Failed to cache:', request.url, error.message);
  }
}

// =============================================================================
// INSTALL EVENT
// =============================================================================

self.addEventListener('install', event => {
  console.log('[SW] Installing service worker...');

  event.waitUntil(
    caches
      .open(STATIC_CACHE)
      .then(cache => {
        console.log('[SW] Caching static assets');
        return cache.addAll(STATIC_ASSETS);
      })
      .then(() => {
        console.log('[SW] Static assets cached');
        return self.skipWaiting();
      })
      .catch(error => {
        console.error('[SW] Failed to cache static assets:', error);
      })
  );
});

// =============================================================================
// ACTIVATE EVENT
// =============================================================================

self.addEventListener('activate', event => {
  console.log('[SW] Activating service worker...');

  event.waitUntil(
    caches
      .keys()
      .then(cacheNames => {
        return Promise.all(
          cacheNames
            .filter(name => {
              // Delete old caches
              return (
                name.startsWith('okla-') &&
                name !== STATIC_CACHE &&
                name !== DYNAMIC_CACHE &&
                name !== IMAGE_CACHE
              );
            })
            .map(name => {
              console.log('[SW] Deleting old cache:', name);
              return caches.delete(name);
            })
        );
      })
      .then(() => {
        console.log('[SW] Claiming clients');
        return self.clients.claim();
      })
  );
});

// =============================================================================
// FETCH EVENT
// =============================================================================

self.addEventListener('fetch', event => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET requests
  if (request.method !== 'GET') {
    return;
  }

  // Skip chrome-extension and other non-http requests
  if (!url.protocol.startsWith('http')) {
    return;
  }

  // Skip Next.js internals, HMR, and other ignored routes
  if (IGNORE_ROUTES.some(route => url.pathname.startsWith(route))) {
    return;
  }

  // Skip no-cache routes
  if (NO_CACHE_ROUTES.some(route => url.pathname.startsWith(route))) {
    return;
  }

  // Handle different strategies based on route
  if (NETWORK_FIRST_ROUTES.some(route => url.pathname.startsWith(route))) {
    event.respondWith(networkFirst(request));
  } else if (isImageRequest(request)) {
    event.respondWith(cacheFirstWithExpiry(request, IMAGE_CACHE, 7 * 24 * 60 * 60 * 1000)); // 7 days
  } else if (isStaticAsset(request)) {
    event.respondWith(cacheFirst(request, STATIC_CACHE));
  } else {
    event.respondWith(staleWhileRevalidate(request, DYNAMIC_CACHE));
  }
});

// =============================================================================
// CACHING STRATEGIES
// =============================================================================

/**
 * Cache-first strategy
 * Good for static assets that don't change often
 */
async function cacheFirst(request, cacheName) {
  const cachedResponse = await caches.match(request);
  if (cachedResponse) {
    return cachedResponse;
  }

  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(cacheName);
      await safeCachePut(cache, request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    return offlineFallback(request);
  }
}

/**
 * Network-first strategy
 * Good for dynamic content that changes frequently
 */
async function networkFirst(request) {
  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(DYNAMIC_CACHE);
      await safeCachePut(cache, request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
      return cachedResponse;
    }
    return offlineFallback(request);
  }
}

/**
 * Stale-while-revalidate strategy
 * Serves cached content immediately while fetching updates
 */
async function staleWhileRevalidate(request, cacheName) {
  const cache = await caches.open(cacheName);
  const cachedResponse = await cache.match(request);

  const fetchPromise = fetch(request)
    .then(async networkResponse => {
      if (networkResponse.ok) {
        await safeCachePut(cache, request, networkResponse.clone());
      }
      return networkResponse;
    })
    .catch(() => null);

  return cachedResponse || (await fetchPromise) || offlineFallback(request);
}

/**
 * Cache-first with expiry
 * Good for images that should be cached but eventually refreshed
 */
async function cacheFirstWithExpiry(request, cacheName, maxAge) {
  const cache = await caches.open(cacheName);
  const cachedResponse = await cache.match(request);

  if (cachedResponse) {
    const dateHeader = cachedResponse.headers.get('date');
    if (dateHeader) {
      const cachedDate = new Date(dateHeader).getTime();
      if (Date.now() - cachedDate < maxAge) {
        return cachedResponse;
      }
    } else {
      return cachedResponse;
    }
  }

  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      await safeCachePut(cache, request, networkResponse.clone());
    }
    return networkResponse;
  } catch (error) {
    if (cachedResponse) {
      return cachedResponse;
    }
    return offlineFallback(request);
  }
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

function isImageRequest(request) {
  const url = new URL(request.url);
  return (
    request.destination === 'image' || /\.(jpg|jpeg|png|gif|webp|avif|svg|ico)$/i.test(url.pathname)
  );
}

function isStaticAsset(request) {
  const url = new URL(request.url);
  return /\.(js|css|woff|woff2|ttf|eot)$/i.test(url.pathname);
}

async function offlineFallback(request) {
  const url = new URL(request.url);

  // For navigation requests, show offline page
  if (request.mode === 'navigate') {
    const offlinePage = await caches.match('/offline');
    if (offlinePage) {
      return offlinePage;
    }
  }

  // For images, return a placeholder
  if (isImageRequest(request)) {
    return new Response(
      '<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200"><rect fill="#f0f0f0" width="200" height="200"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="#999">Offline</text></svg>',
      {
        headers: { 'Content-Type': 'image/svg+xml' },
      }
    );
  }

  // For API requests, return error JSON
  if (url.pathname.startsWith('/api/')) {
    return new Response(JSON.stringify({ error: 'No hay conexión a internet', offline: true }), {
      status: 503,
      headers: { 'Content-Type': 'application/json' },
    });
  }

  // Generic offline response
  return new Response('Sin conexión', { status: 503 });
}

// =============================================================================
// PUSH NOTIFICATIONS
// =============================================================================

self.addEventListener('push', event => {
  console.log('[SW] Push notification received');

  let data = {
    title: 'OKLA',
    body: 'Tienes una nueva notificación',
    icon: '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    tag: 'default',
    data: { url: '/' },
  };

  if (event.data) {
    try {
      data = { ...data, ...event.data.json() };
    } catch (e) {
      data.body = event.data.text();
    }
  }

  const options = {
    body: data.body,
    icon: data.icon,
    badge: data.badge,
    tag: data.tag,
    data: data.data,
    vibrate: [100, 50, 100],
    actions: data.actions || [
      { action: 'open', title: 'Ver' },
      { action: 'close', title: 'Cerrar' },
    ],
    requireInteraction: data.requireInteraction || false,
  };

  event.waitUntil(self.registration.showNotification(data.title, options));
});

self.addEventListener('notificationclick', event => {
  console.log('[SW] Notification clicked:', event.action);

  event.notification.close();

  if (event.action === 'close') {
    return;
  }

  const urlToOpen = event.notification.data?.url || '/';

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true }).then(clientList => {
      // If there's already a window open, focus it
      for (const client of clientList) {
        if (client.url.includes(self.location.origin) && 'focus' in client) {
          client.navigate(urlToOpen);
          return client.focus();
        }
      }
      // Otherwise open a new window
      if (clients.openWindow) {
        return clients.openWindow(urlToOpen);
      }
    })
  );
});

// =============================================================================
// BACKGROUND SYNC
// =============================================================================

self.addEventListener('sync', event => {
  console.log('[SW] Background sync:', event.tag);

  if (event.tag === 'sync-favorites') {
    event.waitUntil(syncFavorites());
  } else if (event.tag === 'sync-messages') {
    event.waitUntil(syncMessages());
  }
});

async function syncFavorites() {
  try {
    // Get pending favorites from IndexedDB
    const pendingFavorites = await getPendingFromDB('favorites');

    for (const favorite of pendingFavorites) {
      await fetch('/api/favorites', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(favorite),
      });
      await removeFromPendingDB('favorites', favorite.id);
    }

    console.log('[SW] Favorites synced successfully');
  } catch (error) {
    console.error('[SW] Failed to sync favorites:', error);
    throw error;
  }
}

async function syncMessages() {
  try {
    const pendingMessages = await getPendingFromDB('messages');

    for (const message of pendingMessages) {
      await fetch('/api/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(message),
      });
      await removeFromPendingDB('messages', message.id);
    }

    console.log('[SW] Messages synced successfully');
  } catch (error) {
    console.error('[SW] Failed to sync messages:', error);
    throw error;
  }
}

// Placeholder functions for IndexedDB operations
async function getPendingFromDB(storeName) {
  // Implementation would use IndexedDB
  return [];
}

async function removeFromPendingDB(storeName, id) {
  // Implementation would use IndexedDB
}

// =============================================================================
// MESSAGE HANDLING
// =============================================================================

self.addEventListener('message', event => {
  console.log('[SW] Message received:', event.data);

  if (event.data.type === 'SKIP_WAITING') {
    self.skipWaiting();
  }

  if (event.data.type === 'CLEAR_CACHE') {
    event.waitUntil(
      caches.keys().then(cacheNames => {
        return Promise.all(cacheNames.map(name => caches.delete(name)));
      })
    );
  }

  if (event.data.type === 'GET_VERSION') {
    event.ports[0].postMessage({ version: CACHE_VERSION });
  }
});
