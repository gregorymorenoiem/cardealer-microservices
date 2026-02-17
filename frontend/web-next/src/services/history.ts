/**
 * Viewing History Service
 *
 * API client for tracking and managing vehicle viewing history
 */

import { apiClient } from '@/lib/api-client';

// =============================================================================
// TYPES
// =============================================================================

export interface ViewedVehicle {
  id: string;
  vehicleId: string;
  viewedAt: string;
  vehicle: {
    id: string;
    slug: string;
    title: string;
    make: string;
    model: string;
    year: number;
    price: number;
    mileage: number;
    location: string;
    imageUrl: string | null;
    dealerName: string;
    status: 'active' | 'sold' | 'pending' | 'removed';
  };
  isFavorite: boolean;
}

export interface ViewingHistoryResponse {
  items: ViewedVehicle[];
  total: number;
  totalFavorites: number;
  oldestDate: string | null;
}

// Local storage key for unauthenticated users
const HISTORY_STORAGE_KEY = 'okla-viewing-history';
const MAX_LOCAL_HISTORY = 50;

// =============================================================================
// LOCAL STORAGE (for unauthenticated users)
// =============================================================================

interface LocalHistoryItem {
  vehicleId: string;
  slug: string;
  title: string;
  price: number;
  imageUrl: string | null;
  viewedAt: string;
}

function getLocalHistory(): LocalHistoryItem[] {
  if (typeof window === 'undefined') return [];

  try {
    const stored = localStorage.getItem(HISTORY_STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

function setLocalHistory(items: LocalHistoryItem[]): void {
  if (typeof window === 'undefined') return;

  try {
    // Keep only the most recent items
    const trimmed = items.slice(0, MAX_LOCAL_HISTORY);
    localStorage.setItem(HISTORY_STORAGE_KEY, JSON.stringify(trimmed));
  } catch {
    // Storage full or disabled
  }
}

function addToLocalHistory(item: Omit<LocalHistoryItem, 'viewedAt'>): void {
  const history = getLocalHistory();

  // Remove if already exists
  const filtered = history.filter(h => h.vehicleId !== item.vehicleId);

  // Add to beginning
  filtered.unshift({
    ...item,
    viewedAt: new Date().toISOString(),
  });

  setLocalHistory(filtered);
}

function removeFromLocalHistory(vehicleId: string): void {
  const history = getLocalHistory();
  setLocalHistory(history.filter(h => h.vehicleId !== vehicleId));
}

function clearLocalHistory(): void {
  if (typeof window === 'undefined') return;
  localStorage.removeItem(HISTORY_STORAGE_KEY);
}

// =============================================================================
// API FUNCTIONS
// =============================================================================

/**
 * Get viewing history for the current user
 */
export async function getHistory(params?: {
  page?: number;
  pageSize?: number;
  days?: number;
}): Promise<ViewingHistoryResponse> {
  const response = await apiClient.get<ViewingHistoryResponse>('/api/history/views', {
    params,
  });
  return response.data;
}

/**
 * Record a vehicle view
 */
export async function recordView(vehicleId: string): Promise<void> {
  await apiClient.post(`/api/history/views/${vehicleId}`);
}

/**
 * Remove a single item from history
 */
export async function removeFromHistory(vehicleId: string): Promise<void> {
  await apiClient.delete(`/api/history/views/${vehicleId}`);
}

/**
 * Clear all viewing history
 */
export async function clearHistory(): Promise<void> {
  await apiClient.delete('/api/history/views');
}

/**
 * Sync local history to server (when user logs in)
 */
export async function syncLocalHistory(): Promise<void> {
  const localHistory = getLocalHistory();

  if (localHistory.length === 0) return;

  try {
    await apiClient.post('/api/history/views/sync', {
      items: localHistory.map(h => ({
        vehicleId: h.vehicleId,
        viewedAt: h.viewedAt,
      })),
    });

    // Clear local history after sync
    clearLocalHistory();
  } catch (error) {
    console.error('Failed to sync local history:', error);
  }
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

export function formatTimeAgo(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return 'Ahora';
  if (diffMins < 60) return `Hace ${diffMins} min`;
  if (diffHours < 24) return `Hace ${diffHours}h`;
  if (diffDays === 1) return 'Ayer';
  if (diffDays < 7) return `Hace ${diffDays} días`;

  return date.toLocaleDateString('es-DO', {
    month: 'short',
    day: 'numeric',
  });
}

export function groupHistoryByDate<T extends { viewedAt: string }>(
  items: T[]
): Record<string, T[]> {
  const groups: Record<string, T[]> = {};

  items.forEach(item => {
    const date = new Date(item.viewedAt);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    let key: string;
    if (date.toDateString() === today.toDateString()) {
      key = 'Hoy';
    } else if (date.toDateString() === yesterday.toDateString()) {
      key = 'Ayer';
    } else {
      key = date.toLocaleDateString('es-DO', {
        weekday: 'long',
        day: 'numeric',
        month: 'long',
      });
      // Capitalize first letter
      key = key.charAt(0).toUpperCase() + key.slice(1);
    }

    if (!groups[key]) groups[key] = [];
    groups[key].push(item);
  });

  return groups;
}

export function calculateDaysInHistory(items: { viewedAt: string }[]): number {
  if (items.length === 0) return 0;

  const dates = items.map(i => new Date(i.viewedAt).toDateString());
  const uniqueDates = new Set(dates);
  return uniqueDates.size;
}

// =============================================================================
// EXPORT SERVICE OBJECT
// =============================================================================

export const historyService = {
  // API functions
  getHistory,
  recordView,
  removeFromHistory,
  clearHistory,
  syncLocalHistory,

  // Local storage functions
  local: {
    get: getLocalHistory,
    add: addToLocalHistory,
    remove: removeFromLocalHistory,
    clear: clearLocalHistory,
  },

  // Helper functions
  formatTimeAgo,
  groupByDate: groupHistoryByDate,
  calculateDaysInHistory,
};
