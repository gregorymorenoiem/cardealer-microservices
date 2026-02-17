/**
 * Type definitions for OKLA Analytics SDK
 * Sprint 9 - Event Tracking Service
 */

export interface OklaAnalyticsConfig {
  apiUrl?: string;
  trackEndpoint?: string;
  batchEndpoint?: string;
  batchSize?: number;
  flushIntervalMs?: number;
  autoTrack?: boolean;
  debug?: boolean;
}

export interface SearchParams {
  query: string;
  resultsCount?: number;
  searchType?: 'vehicles' | 'dealers' | 'general';
  filters?: Record<string, any>;
  sortBy?: string;
  clickedPosition?: number;
  clickedVehicleId?: string;
}

export interface VehicleViewParams {
  vehicleId: string;
  title: string;
  price: number;
  make?: string;
  model?: string;
  year?: number;
  timeSpent?: number;
  viewedImages?: boolean;
  viewedSpecs?: boolean;
  clickedContact?: boolean;
  addedToFavorites?: boolean;
  sharedVehicle?: boolean;
  viewSource?: 'Direct' | 'SearchResults' | 'Featured' | 'Related';
}

export interface FilterParams {
  filterType: string;
  filterValue: string;
  filterOperator?: 'equals' | 'contains' | 'gte' | 'lte' | 'in';
  resultsAfterFilter?: number;
  pageContext?: 'Search' | 'Browse' | 'Favorites';
}

export interface OklaAnalyticsSDK {
  version: string;
  init: (config?: OklaAnalyticsConfig) => void;
  setUserId: (id: string) => void;
  clearUserId: () => void;
  trackSearch: (params: SearchParams) => void;
  trackVehicleView: (params: VehicleViewParams) => void;
  trackFilter: (params: FilterParams) => void;
  trackCustom: (eventType: string, data?: Record<string, any>) => void;
  flush: () => void;
  getSessionId: () => string;
  getUserId: () => string | null;
  enableDebug: () => void;
  disableDebug: () => void;
}

declare global {
  interface Window {
    OklaAnalytics: OklaAnalyticsSDK;
  }
}

export {};
