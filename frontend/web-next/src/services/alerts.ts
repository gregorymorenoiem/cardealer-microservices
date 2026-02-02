/**
 * Alerts Service - API client for price alerts and saved searches
 * Connects via API Gateway to AlertService
 */

import { apiClient } from '@/lib/api-client';
import type { VehicleSearchParams, PaginatedResponse } from '@/types';

// ============================================================
// API TYPES
// ============================================================

// -----------------------------------------------------------------------------
// Price Alerts
// -----------------------------------------------------------------------------

export interface PriceAlert {
  id: string;
  userId: string;
  vehicleId: string;
  vehicleTitle: string; // Display title (e.g., "Toyota Camry 2024")
  vehicleName?: string; // Alias for compatibility
  vehicleImageUrl?: string;
  vehicleSlug: string;
  currentPrice: number;
  targetPrice: number;
  currency: 'DOP' | 'USD';
  notifyOnAnyChange: boolean;
  notifyPercentageThreshold?: number;
  isActive: boolean;
  isTriggered: boolean; // Whether the target price was reached
  lastCheckedAt: string;
  lastNotifiedAt?: string;
  priceHistory: PriceHistoryEntry[];
  createdAt: string;
  updatedAt: string;
}

export interface PriceHistoryEntry {
  price: number;
  date: string;
  change?: number;
  changePercentage?: number;
}

export interface CreatePriceAlertRequest {
  vehicleId: string;
  targetPrice?: number;
  notifyOnAnyChange?: boolean;
  notifyPercentageThreshold?: number;
}

export interface UpdatePriceAlertRequest {
  targetPrice?: number;
  notifyOnAnyChange?: boolean;
  notifyPercentageThreshold?: number;
  isActive?: boolean;
}

// -----------------------------------------------------------------------------
// Saved Searches
// -----------------------------------------------------------------------------

export interface SavedSearch {
  id: string;
  userId: string;
  name: string;
  searchParams: VehicleSearchParams;
  notifyNewListings: boolean;
  notifyFrequency: NotifyFrequency;
  matchCount: number;
  newMatchCount: number;
  lastMatchedAt?: string;
  lastNotifiedAt?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export type NotifyFrequency = 'instant' | 'daily' | 'weekly' | 'never';

export interface CreateSavedSearchRequest {
  name: string;
  searchParams: VehicleSearchParams;
  notifyNewListings?: boolean;
  notifyFrequency?: NotifyFrequency;
}

export interface UpdateSavedSearchRequest {
  name?: string;
  searchParams?: VehicleSearchParams;
  notifyNewListings?: boolean;
  notifyFrequency?: NotifyFrequency;
  isActive?: boolean;
}

export interface SavedSearchMatch {
  vehicleId: string;
  vehicleName: string;
  vehicleImageUrl?: string;
  vehicleSlug: string;
  price: number;
  matchedAt: string;
  isNew: boolean;
}

// -----------------------------------------------------------------------------
// Alert Stats
// -----------------------------------------------------------------------------

export interface AlertStats {
  totalPriceAlerts: number;
  activePriceAlerts: number;
  priceDropsThisMonth: number;
  totalSavedSearches: number;
  activeSavedSearches: number;
  newMatchesThisWeek: number;
}

// ============================================================
// API FUNCTIONS - PRICE ALERTS
// ============================================================

/**
 * Get user's price alerts
 */
export async function getPriceAlerts(
  params: { isActive?: boolean; page?: number; pageSize?: number } = {}
): Promise<PaginatedResponse<PriceAlert>> {
  const response = await apiClient.get<{
    items: PriceAlert[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>('/api/pricealerts', { params });

  return {
    items: response.data.items,
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

/**
 * Get price alert by ID
 */
export async function getPriceAlertById(id: string): Promise<PriceAlert> {
  const response = await apiClient.get<PriceAlert>(`/api/pricealerts/${id}`);
  return response.data;
}

/**
 * Get price alert for a specific vehicle
 */
export async function getPriceAlertForVehicle(vehicleId: string): Promise<PriceAlert | null> {
  try {
    const response = await apiClient.get<PriceAlert>(`/api/pricealerts/vehicle/${vehicleId}`);
    return response.data;
  } catch {
    return null;
  }
}

/**
 * Create a new price alert
 */
export async function createPriceAlert(data: CreatePriceAlertRequest): Promise<PriceAlert> {
  const response = await apiClient.post<PriceAlert>('/api/pricealerts', data);
  return response.data;
}

/**
 * Update a price alert
 */
export async function updatePriceAlert(
  id: string,
  data: UpdatePriceAlertRequest
): Promise<PriceAlert> {
  const response = await apiClient.put<PriceAlert>(`/api/pricealerts/${id}`, data);
  return response.data;
}

/**
 * Delete a price alert
 */
export async function deletePriceAlert(id: string): Promise<void> {
  await apiClient.delete(`/api/pricealerts/${id}`);
}

/**
 * Toggle price alert active status
 */
export async function togglePriceAlert(id: string): Promise<PriceAlert> {
  const response = await apiClient.post<PriceAlert>(`/api/pricealerts/${id}/toggle`);
  return response.data;
}

/**
 * Get price history for a vehicle
 */
export async function getVehiclePriceHistory(vehicleId: string): Promise<PriceHistoryEntry[]> {
  const response = await apiClient.get<PriceHistoryEntry[]>(
    `/api/pricealerts/history/${vehicleId}`
  );
  return response.data;
}

// ============================================================
// API FUNCTIONS - SAVED SEARCHES
// ============================================================

/**
 * Get user's saved searches
 */
export async function getSavedSearches(
  params: { isActive?: boolean; page?: number; pageSize?: number } = {}
): Promise<PaginatedResponse<SavedSearch>> {
  const response = await apiClient.get<{
    items: SavedSearch[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>('/api/savedsearches', { params });

  return {
    items: response.data.items,
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

/**
 * Get saved search by ID
 */
export async function getSavedSearchById(id: string): Promise<SavedSearch> {
  const response = await apiClient.get<SavedSearch>(`/api/savedsearches/${id}`);
  return response.data;
}

/**
 * Create a new saved search
 */
export async function createSavedSearch(data: CreateSavedSearchRequest): Promise<SavedSearch> {
  const response = await apiClient.post<SavedSearch>('/api/savedsearches', data);
  return response.data;
}

/**
 * Update a saved search
 */
export async function updateSavedSearch(
  id: string,
  data: UpdateSavedSearchRequest
): Promise<SavedSearch> {
  const response = await apiClient.put<SavedSearch>(`/api/savedsearches/${id}`, data);
  return response.data;
}

/**
 * Delete a saved search
 */
export async function deleteSavedSearch(id: string): Promise<void> {
  await apiClient.delete(`/api/savedsearches/${id}`);
}

/**
 * Toggle saved search active status
 */
export async function toggleSavedSearch(id: string): Promise<SavedSearch> {
  const response = await apiClient.post<SavedSearch>(`/api/savedsearches/${id}/toggle`);
  return response.data;
}

/**
 * Get matches for a saved search
 */
export async function getSavedSearchMatches(
  id: string,
  params: { page?: number; pageSize?: number } = {}
): Promise<PaginatedResponse<SavedSearchMatch>> {
  const response = await apiClient.get<{
    items: SavedSearchMatch[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>(`/api/savedsearches/${id}/matches`, { params });

  return {
    items: response.data.items,
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

/**
 * Mark saved search matches as seen
 */
export async function markMatchesAsSeen(id: string): Promise<void> {
  await apiClient.post(`/api/savedsearches/${id}/mark-seen`);
}

/**
 * Run saved search (get current results)
 */
export async function runSavedSearch(id: string): Promise<PaginatedResponse<SavedSearchMatch>> {
  const response = await apiClient.post<{
    items: SavedSearchMatch[];
    pagination: {
      page: number;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>(`/api/savedsearches/${id}/run`);

  return {
    items: response.data.items,
    pagination: {
      ...response.data.pagination,
      hasNextPage: response.data.pagination.page < response.data.pagination.totalPages,
      hasPreviousPage: response.data.pagination.page > 1,
    },
  };
}

// ============================================================
// API FUNCTIONS - STATS
// ============================================================

/**
 * Get alert statistics for the user
 */
export async function getAlertStats(): Promise<AlertStats> {
  const response = await apiClient.get<AlertStats>('/api/alerts/stats');
  return response.data;
}

// ============================================================
// HELPER FUNCTIONS
// ============================================================

/**
 * Format price change
 */
export function formatPriceChange(change: number, currency: 'DOP' | 'USD' = 'DOP'): string {
  const symbol = currency === 'USD' ? '$' : 'RD$';
  const prefix = change > 0 ? '+' : '';
  return `${prefix}${symbol}${Math.abs(change).toLocaleString()}`;
}

/**
 * Format percentage change
 */
export function formatPercentageChange(percentage: number): string {
  const prefix = percentage > 0 ? '+' : '';
  return `${prefix}${percentage.toFixed(1)}%`;
}

/**
 * Get change color class
 */
export function getChangeColor(change: number): string {
  if (change < 0) return 'text-emerald-600'; // Price dropped = good for buyer
  if (change > 0) return 'text-red-600'; // Price increased = bad for buyer
  return 'text-gray-600';
}

/**
 * Format notify frequency label
 */
export function formatNotifyFrequency(frequency: NotifyFrequency): string {
  const labels: Record<NotifyFrequency, string> = {
    instant: 'Instantáneo',
    daily: 'Diario',
    weekly: 'Semanal',
    never: 'Nunca',
  };
  return labels[frequency];
}

/**
 * Build search params description
 */
export function buildSearchDescription(params: VehicleSearchParams): string {
  const parts: string[] = [];

  if (params.make) parts.push(params.make);
  if (params.model) parts.push(params.model);
  if (params.yearMin && params.yearMax) {
    parts.push(`${params.yearMin}-${params.yearMax}`);
  } else if (params.yearMin) {
    parts.push(`${params.yearMin}+`);
  } else if (params.yearMax) {
    parts.push(`Hasta ${params.yearMax}`);
  }
  if (params.priceMax) {
    parts.push(`< RD$${(params.priceMax / 1000).toFixed(0)}K`);
  }
  if (params.bodyType) parts.push(params.bodyType);
  if (params.province) parts.push(params.province);

  return parts.join(' • ') || 'Todos los vehículos';
}

// ============================================================
// SERVICE EXPORT
// ============================================================

export const alertService = {
  // Price alerts
  getPriceAlerts,
  getPriceAlertById,
  getPriceAlertForVehicle,
  createPriceAlert,
  updatePriceAlert,
  deletePriceAlert,
  togglePriceAlert,
  getVehiclePriceHistory,
  // Saved searches
  getSavedSearches,
  getSavedSearchById,
  createSavedSearch,
  updateSavedSearch,
  deleteSavedSearch,
  toggleSavedSearch,
  getSavedSearchMatches,
  markMatchesAsSeen,
  runSavedSearch,
  // Stats
  getAlertStats,
  // Helpers
  formatPriceChange,
  formatPercentageChange,
  getChangeColor,
  formatNotifyFrequency,
  buildSearchDescription,
};

export default alertService;
