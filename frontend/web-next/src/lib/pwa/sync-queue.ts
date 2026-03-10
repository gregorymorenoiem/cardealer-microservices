/**
 * Background Sync Queue — Client-side helper
 *
 * Provides APIs to enqueue data into IndexedDB for
 * the service worker to sync when connectivity is restored.
 *
 * Usage:
 *   import { enqueueFavorite, enqueueMessage, enqueueVehicleDraft } from '@/lib/pwa/sync-queue';
 *   await enqueueFavorite({ vehicleId: '123', action: 'add' });
 */

const DB_NAME = 'okla-sync-db';
const DB_VERSION = 1;

function openSyncDB(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);
    request.onerror = () => reject(request.error);
    request.onsuccess = () => resolve(request.result);
    request.onupgradeneeded = event => {
      const db = (event.target as IDBOpenDBRequest).result;
      if (!db.objectStoreNames.contains('favorites')) {
        db.createObjectStore('favorites', { keyPath: 'id', autoIncrement: true });
      }
      if (!db.objectStoreNames.contains('messages')) {
        db.createObjectStore('messages', { keyPath: 'id', autoIncrement: true });
      }
      if (!db.objectStoreNames.contains('vehicle-drafts')) {
        db.createObjectStore('vehicle-drafts', { keyPath: 'id', autoIncrement: true });
      }
    };
  });
}

async function addToPendingDB(storeName: string, data: Record<string, unknown>): Promise<void> {
  const db = await openSyncDB();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(storeName, 'readwrite');
    const store = tx.objectStore(storeName);
    const request = store.add({ ...data, createdAt: Date.now() });
    request.onsuccess = () => resolve();
    request.onerror = () => reject(request.error);
  });
}

async function requestSync(tag: string): Promise<void> {
  if ('serviceWorker' in navigator && 'SyncManager' in window) {
    const registration = await navigator.serviceWorker.ready;
    await (
      registration as unknown as { sync: { register: (tag: string) => Promise<void> } }
    ).sync.register(tag);
  }
}

/**
 * Enqueue a favorite toggle for background sync.
 */
export async function enqueueFavorite(data: {
  vehicleId: string;
  action: 'add' | 'remove';
}): Promise<void> {
  await addToPendingDB('favorites', data);
  await requestSync('sync-favorites');
}

/**
 * Enqueue a message for background sync (e.g., contact a dealer).
 */
export async function enqueueMessage(data: {
  recipientId: string;
  vehicleId?: string;
  content: string;
}): Promise<void> {
  await addToPendingDB('messages', data);
  await requestSync('sync-messages');
}

/**
 * Enqueue a vehicle draft for background sync (offline publish).
 */
export async function enqueueVehicleDraft(data: {
  payload: Record<string, unknown>;
  authToken?: string;
}): Promise<void> {
  await addToPendingDB('vehicle-drafts', data);
  await requestSync('sync-vehicle-drafts');
}

/**
 * Get all pending items for a given sync queue.
 */
export async function getPendingItems(storeName: string): Promise<unknown[]> {
  try {
    const db = await openSyncDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction(storeName, 'readonly');
      const store = tx.objectStore(storeName);
      const request = store.getAll();
      request.onsuccess = () => resolve(request.result || []);
      request.onerror = () => reject(request.error);
    });
  } catch {
    return [];
  }
}
