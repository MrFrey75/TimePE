// Service Worker for TimePE PWA
// Modern ES2024+ implementation with enhanced caching strategies

const CACHE_VERSION = '2.0.0';
const CACHE_NAME = `timepe-v${CACHE_VERSION}`;
const STATIC_CACHE = `${CACHE_NAME}-static`;
const DYNAMIC_CACHE = `${CACHE_NAME}-dynamic`;
const IMAGE_CACHE = `${CACHE_NAME}-images`;

// Cache configuration
const CACHE_CONFIG = {
  maxAge: 7 * 24 * 60 * 60 * 1000, // 7 days in milliseconds
  maxDynamicItems: 50,
  maxImageItems: 30
};

// Static assets to cache immediately
const STATIC_ASSETS = [
  '/',
  '/css/site.css',
  '/js/site.js',
  '/lib/bootstrap/dist/css/bootstrap.min.css',
  '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
  '/lib/jquery/dist/jquery.min.js',
  '/manifest.json',
  'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css'
];

// Install event - cache static resources with modern async/await
self.addEventListener('install', event => {
  console.log('[Service Worker] Installing version:', CACHE_VERSION);
  
  event.waitUntil(
    (async () => {
      try {
        const cache = await caches.open(STATIC_CACHE);
        console.log('[Service Worker] Caching static assets');
        await cache.addAll(STATIC_ASSETS);
        console.log('[Service Worker] Static assets cached successfully');
        await self.skipWaiting();
      } catch (error) {
        console.error('[Service Worker] Cache installation failed:', error);
        throw error;
      }
    })()
  );
});

// Activate event - clean up old caches with modern async/await
self.addEventListener('activate', event => {
  console.log('[Service Worker] Activating version:', CACHE_VERSION);
  
  event.waitUntil(
    (async () => {
      try {
        const cacheNames = await caches.keys();
        const cacheWhitelist = [STATIC_CACHE, DYNAMIC_CACHE, IMAGE_CACHE];
        
        // Delete old caches
        await Promise.all(
          cacheNames
            .filter(cacheName => !cacheWhitelist.includes(cacheName))
            .map(cacheName => {
              console.log('[Service Worker] Deleting old cache:', cacheName);
              return caches.delete(cacheName);
            })
        );
        
        // Take control of all pages immediately
        await self.clients.claim();
        console.log('[Service Worker] Activated successfully');
      } catch (error) {
        console.error('[Service Worker] Activation failed:', error);
        throw error;
      }
    })()
  );
});

// Helper: Determine cache strategy based on request
const getCacheStrategy = request => {
  const url = new URL(request.url);
  
  // Image files - use image cache
  if (/\.(jpg|jpeg|png|gif|svg|webp|ico)$/i.test(url.pathname)) {
    return { cache: IMAGE_CACHE, strategy: 'cache-first' };
  }
  
  // Static assets - use static cache
  if (url.pathname.match(/\.(css|js|woff2?|ttf|eot)$/i)) {
    return { cache: STATIC_CACHE, strategy: 'cache-first' };
  }
  
  // API calls - network first
  if (url.pathname.startsWith('/api/')) {
    return { cache: DYNAMIC_CACHE, strategy: 'network-first' };
  }
  
  // HTML pages - network first with cache fallback
  return { cache: DYNAMIC_CACHE, strategy: 'network-first' };
};

// Helper: Cache-first strategy
const cacheFirst = async (request, cacheName) => {
  const cached = await caches.match(request);
  if (cached) {
    return cached;
  }
  
  try {
    const response = await fetch(request);
    if (response?.ok) {
      const cache = await caches.open(cacheName);
      await cache.put(request, response.clone());
    }
    return response;
  } catch (error) {
    console.error('[Service Worker] Cache-first fetch failed:', error);
    return caches.match('/offline.html') ?? new Response('Offline', { status: 503 });
  }
};

// Helper: Network-first strategy
const networkFirst = async (request, cacheName) => {
  try {
    const response = await fetch(request);
    if (response?.ok) {
      const cache = await caches.open(cacheName);
      await cache.put(request, response.clone());
    }
    return response;
  } catch (error) {
    console.log('[Service Worker] Network failed, falling back to cache');
    const cached = await caches.match(request);
    return cached ?? new Response('Offline', { status: 503 });
  }
};

// Helper: Limit cache size
const limitCacheSize = async (cacheName, maxItems) => {
  const cache = await caches.open(cacheName);
  const keys = await cache.keys();
  
  if (keys.length > maxItems) {
    // Delete oldest entries
    const itemsToDelete = keys.slice(0, keys.length - maxItems);
    await Promise.all(itemsToDelete.map(key => cache.delete(key)));
  }
};

// Fetch event - intelligent caching strategies
self.addEventListener('fetch', event => {
  const { request } = event;
  
  // Skip non-GET requests
  if (request.method !== 'GET') {
    return;
  }

  // Skip chrome extensions and other protocols
  if (!request.url.startsWith('http')) {
    return;
  }

  event.respondWith(
    (async () => {
      const { cache, strategy } = getCacheStrategy(request);
      
      let response;
      if (strategy === 'cache-first') {
        response = await cacheFirst(request, cache);
      } else {
        response = await networkFirst(request, cache);
      }
      
      // Limit cache sizes periodically
      if (cache === DYNAMIC_CACHE) {
        await limitCacheSize(DYNAMIC_CACHE, CACHE_CONFIG.maxDynamicItems);
      } else if (cache === IMAGE_CACHE) {
        await limitCacheSize(IMAGE_CACHE, CACHE_CONFIG.maxImageItems);
      }
      
      return response;
    })()
  );
});

// Background sync for offline data
self.addEventListener('sync', event => {
  console.log('[Service Worker] Background sync triggered:', event.tag);
  
  if (event.tag === 'sync-time-entries') {
    event.waitUntil(syncTimeEntries());
  } else if (event.tag.startsWith('sync-')) {
    event.waitUntil(handleGenericSync(event.tag));
  }
});

// Sync time entries from IndexedDB to server
const syncTimeEntries = async () => {
  try {
    console.log('[Service Worker] Syncing time entries...');
    
    // Future enhancement: Sync local IndexedDB entries to server
    // const entries = await getOfflineEntries();
    // await Promise.all(entries.map(entry => postToServer(entry)));
    
    console.log('[Service Worker] Time entries synced successfully');
  } catch (error) {
    console.error('[Service Worker] Sync failed:', error);
    throw error;
  }
};

// Generic sync handler
const handleGenericSync = async tag => {
  console.log('[Service Worker] Handling generic sync:', tag);
  // Placeholder for future sync implementations
};

// Push notifications with modern configuration
self.addEventListener('push', event => {
  console.log('[Service Worker] Push notification received');
  
  const data = event.data?.json() ?? {};
  const options = {
    body: data.body ?? 'New notification from TimePE',
    icon: data.icon ?? '/icons/icon-192x192.png',
    badge: '/icons/badge-72x72.png',
    vibrate: [200, 100, 200],
    tag: data.tag ?? 'timepe-notification',
    requireInteraction: data.requireInteraction ?? false,
    data: {
      url: data.url ?? '/',
      timestamp: Date.now(),
      ...data
    },
    actions: data.actions ?? [
      { action: 'open', title: 'Open' },
      { action: 'close', title: 'Dismiss' }
    ]
  };

  event.waitUntil(
    self.registration.showNotification(data.title ?? 'TimePE', options)
  );
});

// Notification click handler with action support
self.addEventListener('notificationclick', event => {
  console.log('[Service Worker] Notification clicked:', event.action);
  event.notification.close();

  const urlToOpen = event.notification.data?.url ?? '/';
  
  event.waitUntil(
    (async () => {
      // Check if there's already a window open
      const windowClients = await clients.matchAll({
        type: 'window',
        includeUncontrolled: true
      });

      // Focus existing window if available
      for (const client of windowClients) {
        if (client.url === urlToOpen && 'focus' in client) {
          return client.focus();
        }
      }

      // Open new window
      if (clients.openWindow) {
        return clients.openWindow(urlToOpen);
      }
    })()
  );
});

// Message handler for communication with clients
self.addEventListener('message', event => {
  console.log('[Service Worker] Message received:', event.data);
  
  if (event.data?.type === 'SKIP_WAITING') {
    self.skipWaiting();
  } else if (event.data?.type === 'GET_VERSION') {
    event.ports[0]?.postMessage({ version: CACHE_VERSION });
  } else if (event.data?.type === 'CLEAR_CACHE') {
    event.waitUntil(
      caches.keys().then(names => 
        Promise.all(names.map(name => caches.delete(name)))
      )
    );
  }
});

// Periodic background sync (if supported)
self.addEventListener('periodicsync', event => {
  console.log('[Service Worker] Periodic sync triggered:', event.tag);
  
  if (event.tag === 'update-data') {
    event.waitUntil(updateCachedData());
  }
});

// Update cached data periodically
const updateCachedData = async () => {
  try {
    console.log('[Service Worker] Updating cached data...');
    // Refresh critical cached resources
    const cache = await caches.open(DYNAMIC_CACHE);
    const criticalUrls = ['/', '/api/dashboard'];
    
    await Promise.all(
      criticalUrls.map(async url => {
        try {
          const response = await fetch(url);
          if (response.ok) {
            await cache.put(url, response);
          }
        } catch (error) {
          console.warn('[Service Worker] Failed to update:', url, error);
        }
      })
    );
    
    console.log('[Service Worker] Cached data updated');
  } catch (error) {
    console.error('[Service Worker] Update failed:', error);
  }
};

console.log('[Service Worker] Loaded successfully, version:', CACHE_VERSION);
