/// <reference lib="webworker" />

/**
 * Service Worker for CarDealer Marketplace
 * Optimized caching strategy for Dominican Republic's varying network conditions
 * 
 * Strategies:
 * - Cache First: Static assets (images, fonts, CSS, JS)
 * - Network First: API calls, dynamic content
 * - Stale While Revalidate: HTML pages
 */

declare const self: ServiceWorkerGlobalScope;

const CACHE_VERSION = 'v1';
const STATIC_CACHE = `static-${CACHE_VERSION}`;
const DYNAMIC_CACHE = `dynamic-${CACHE_VERSION}`;
const IMAGE_CACHE = `images-${CACHE_VERSION}`;

// Assets to precache
const PRECACHE_ASSETS = [
  '/',
  '/index.html',
  '/manifest.json',
  '/images/placeholder-image.svg',
  '/images/logo.svg',
  '/offline.html',
];

// Cache size limits
const MAX_DYNAMIC_CACHE_ITEMS = 50;
const MAX_IMAGE_CACHE_ITEMS = 100;

// ============================================
// Install Event - Precache Static Assets
// ============================================

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(STATIC_CACHE).then((cache) => {
      console.log('Precaching static assets');
      return cache.addAll(PRECACHE_ASSETS);
    })
  );
  // Activate immediately
  self.skipWaiting();
});

// ============================================
// Activate Event - Clean Old Caches
// ============================================

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames
          .filter((name) => {
            return (
              name.startsWith('static-') ||
              name.startsWith('dynamic-') ||
              name.startsWith('images-')
            ) && !name.includes(CACHE_VERSION);
          })
          .map((name) => {
            console.log('Deleting old cache:', name);
            return caches.delete(name);
          })
      );
    })
  );
  // Take control of all pages immediately
  self.clients.claim();
});

// ============================================
// Fetch Event - Caching Strategies
// ============================================

self.addEventListener('fetch', (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // Skip non-GET requests
  if (request.method !== 'GET') return;

  // Skip chrome-extension and other non-http(s) requests
  if (!url.protocol.startsWith('http')) return;

  // Strategy based on request type
  if (isImageRequest(request)) {
    event.respondWith(cacheFirstWithExpiration(request, IMAGE_CACHE, MAX_IMAGE_CACHE_ITEMS));
  } else if (isStaticAsset(request)) {
    event.respondWith(cacheFirst(request, STATIC_CACHE));
  } else if (isApiRequest(request)) {
    event.respondWith(networkFirstWithTimeout(request, DYNAMIC_CACHE, 5000));
  } else if (isNavigationRequest(request)) {
    event.respondWith(staleWhileRevalidate(request, DYNAMIC_CACHE));
  } else {
    event.respondWith(networkFirstWithTimeout(request, DYNAMIC_CACHE, 3000));
  }
});

// ============================================
// Request Type Helpers
// ============================================

function isImageRequest(request: Request): boolean {
  const url = new URL(request.url);
  return (
    request.destination === 'image' ||
    /\.(jpg|jpeg|png|gif|webp|avif|svg|ico)$/i.test(url.pathname)
  );
}

function isStaticAsset(request: Request): boolean {
  const url = new URL(request.url);
  return /\.(css|js|woff2?|ttf|eot)$/i.test(url.pathname);
}

function isApiRequest(request: Request): boolean {
  const url = new URL(request.url);
  return url.pathname.startsWith('/api/');
}

function isNavigationRequest(request: Request): boolean {
  return request.mode === 'navigate';
}

// ============================================
// Caching Strategies
// ============================================

// Cache First - Good for static assets
async function cacheFirst(request: Request, cacheName: string): Promise<Response> {
  const cache = await caches.open(cacheName);
  const cachedResponse = await cache.match(request);
  
  if (cachedResponse) {
    return cachedResponse;
  }

  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch {
    return new Response('Network error', { status: 503 });
  }
}

// Cache First with LRU expiration - Good for images
async function cacheFirstWithExpiration(
  request: Request, 
  cacheName: string, 
  maxItems: number
): Promise<Response> {
  const cache = await caches.open(cacheName);
  const cachedResponse = await cache.match(request);
  
  if (cachedResponse) {
    return cachedResponse;
  }

  try {
    const networkResponse = await fetch(request);
    
    if (networkResponse.ok) {
      // Clean old items if cache is full
      const keys = await cache.keys();
      if (keys.length >= maxItems) {
        // Delete oldest items (FIFO)
        const deleteCount = Math.ceil(maxItems * 0.2); // Delete 20%
        for (let i = 0; i < deleteCount; i++) {
          await cache.delete(keys[i]);
        }
      }
      cache.put(request, networkResponse.clone());
    }
    
    return networkResponse;
  } catch {
    // Return placeholder for failed image loads
    const placeholder = await cache.match('/images/placeholder-image.svg');
    return placeholder || new Response('Image not available', { status: 503 });
  }
}

// Network First with Timeout - Good for API calls
async function networkFirstWithTimeout(
  request: Request, 
  cacheName: string, 
  timeout: number
): Promise<Response> {
  const cache = await caches.open(cacheName);

  try {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeout);

    const networkResponse = await fetch(request, { signal: controller.signal });
    clearTimeout(timeoutId);

    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone());
    }
    
    return networkResponse;
  } catch {
    const cachedResponse = await cache.match(request);
    
    if (cachedResponse) {
      return cachedResponse;
    }

    // For navigation requests, show offline page
    if (request.mode === 'navigate') {
      const offlinePage = await cache.match('/offline.html');
      if (offlinePage) return offlinePage;
    }

    return new Response(JSON.stringify({ error: 'Offline' }), {
      status: 503,
      headers: { 'Content-Type': 'application/json' },
    });
  }
}

// Stale While Revalidate - Good for HTML pages
async function staleWhileRevalidate(request: Request, cacheName: string): Promise<Response> {
  const cache = await caches.open(cacheName);
  const cachedResponse = await cache.match(request);

  const fetchPromise = fetch(request).then((networkResponse) => {
    if (networkResponse.ok) {
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  }).catch(() => {
    // If network fails and we have cache, that's fine
    // If no cache, we'll return the offline page below
    return null;
  });

  // Return cached version immediately, update in background
  if (cachedResponse) {
    fetchPromise; // Start background update
    return cachedResponse;
  }

  // No cache, wait for network
  const networkResponse = await fetchPromise;
  
  if (networkResponse) {
    return networkResponse;
  }

  // Offline fallback
  const offlinePage = await cache.match('/offline.html');
  return offlinePage || new Response('Offline', { status: 503 });
}

// ============================================
// Background Sync for Failed Requests
// ============================================

self.addEventListener('sync', (event) => {
  if (event.tag === 'sync-favorites') {
    event.waitUntil(syncFavorites());
  }
  if (event.tag === 'sync-messages') {
    event.waitUntil(syncMessages());
  }
});

async function syncFavorites(): Promise<void> {
  // Implement background sync for favorites
  console.log('Syncing favorites...');
}

async function syncMessages(): Promise<void> {
  // Implement background sync for messages
  console.log('Syncing messages...');
}

// ============================================
// Push Notifications
// ============================================

self.addEventListener('push', (event) => {
  if (!event.data) return;

  const data = event.data.json();
  
  event.waitUntil(
    self.registration.showNotification(data.title, {
      body: data.body,
      icon: '/images/icon-192.png',
      badge: '/images/badge-72.png',
      tag: data.tag || 'default',
      data: data.url,
      actions: data.actions,
    })
  );
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();

  const url = event.notification.data || '/';
  
  event.waitUntil(
    self.clients.matchAll({ type: 'window' }).then((clients) => {
      // Focus existing window if available
      for (const client of clients) {
        if (client.url === url && 'focus' in client) {
          return client.focus();
        }
      }
      // Open new window
      return self.clients.openWindow(url);
    })
  );
});

export {};
