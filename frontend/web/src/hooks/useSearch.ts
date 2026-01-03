/**
 * Search & Filters TanStack Query Hooks
 * 
 * Provides hooks for search operations, saved searches, and filter management.
 * Connects to SearchService backend for Elasticsearch queries.
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { UseQueryOptions } from '@tanstack/react-query';
import {
  getSavedSearches,
  getSavedSearchById,
  createSavedSearch,
  updateSavedSearch,
  deleteSavedSearch,
  toggleNotifications,
  checkForNewResults,
  runSavedSearch,
  buildSearchUrl,
  formatFilters,
} from '@/services/savedSearchService';
import type { SavedSearch, SavedSearchFilters } from '@/services/savedSearchService';
import { getAllVehicles, searchVehicles } from '@/services/vehicleService';
import type { VehicleFilters, PaginatedVehicles } from '@/services/vehicleService';

// ============================================================================
// QUERY KEY FACTORY
// ============================================================================

export const searchKeys = {
  all: ['search'] as const,
  // Vehicles search
  vehicles: () => [...searchKeys.all, 'vehicles'] as const,
  vehiclesList: (filters: VehicleFilters, page: number, pageSize: number) => 
    [...searchKeys.vehicles(), 'list', { filters, page, pageSize }] as const,
  vehiclesSearch: (query: string, page: number, pageSize: number) => 
    [...searchKeys.vehicles(), 'search', { query, page, pageSize }] as const,
  // Saved searches
  savedSearches: () => [...searchKeys.all, 'saved'] as const,
  savedSearchesList: () => [...searchKeys.savedSearches(), 'list'] as const,
  savedSearchDetail: (id: string) => [...searchKeys.savedSearches(), 'detail', id] as const,
  savedSearchResults: (id: string, page: number, pageSize: number) => 
    [...searchKeys.savedSearches(), 'results', id, { page, pageSize }] as const,
  savedSearchCheck: (id: string) => [...searchKeys.savedSearches(), 'check', id] as const,
  // Autocomplete & suggestions
  autocomplete: () => [...searchKeys.all, 'autocomplete'] as const,
  suggestions: (query: string) => [...searchKeys.autocomplete(), query] as const,
  // Recent searches (local)
  recentSearches: () => [...searchKeys.all, 'recent'] as const,
  // Popular searches
  popularSearches: () => [...searchKeys.all, 'popular'] as const,
};

// ============================================================================
// VEHICLE SEARCH HOOKS
// ============================================================================

/**
 * Hook for searching vehicles with filters
 */
export function useVehicleSearch(
  filters: VehicleFilters,
  page: number = 1,
  pageSize: number = 12,
  options?: Partial<UseQueryOptions<PaginatedVehicles>>
) {
  return useQuery({
    queryKey: searchKeys.vehiclesList(filters, page, pageSize),
    queryFn: () => getAllVehicles(filters, page, pageSize),
    staleTime: 5 * 60 * 1000, // 5 minutes
    ...options,
  });
}

/**
 * Hook for text search
 */
export function useVehicleTextSearch(
  query: string,
  page: number = 1,
  pageSize: number = 12,
  filters?: VehicleFilters,
  options?: Partial<UseQueryOptions<PaginatedVehicles>>
) {
  return useQuery({
    queryKey: searchKeys.vehiclesSearch(query, page, pageSize),
    queryFn: () => searchVehicles(query, filters, page, pageSize),
    enabled: query.length >= 2,
    staleTime: 2 * 60 * 1000, // 2 minutes
    ...options,
  });
}

// ============================================================================
// SAVED SEARCHES HOOKS
// ============================================================================

/**
 * Hook for fetching all saved searches
 */
export function useSavedSearches(options?: Partial<UseQueryOptions<SavedSearch[]>>) {
  return useQuery({
    queryKey: searchKeys.savedSearchesList(),
    queryFn: getSavedSearches,
    staleTime: 60 * 1000, // 1 minute
    ...options,
  });
}

/**
 * Hook for fetching a single saved search
 */
export function useSavedSearch(id: string, options?: Partial<UseQueryOptions<SavedSearch>>) {
  return useQuery({
    queryKey: searchKeys.savedSearchDetail(id),
    queryFn: () => getSavedSearchById(id),
    enabled: !!id,
    ...options,
  });
}

/**
 * Hook for running a saved search and getting results
 */
export function useSavedSearchResults(
  id: string,
  page: number = 1,
  pageSize: number = 12,
  options?: Partial<UseQueryOptions<PaginatedVehicles>>
) {
  return useQuery({
    queryKey: searchKeys.savedSearchResults(id, page, pageSize),
    queryFn: () => runSavedSearch(id, page, pageSize),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
    ...options,
  });
}

/**
 * Hook for checking new results in a saved search
 */
export function useCheckSavedSearchResults(id: string) {
  return useQuery({
    queryKey: searchKeys.savedSearchCheck(id),
    queryFn: () => checkForNewResults(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook for creating a new saved search
 */
export function useCreateSavedSearch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ name, filters, notificationsEnabled }: {
      name: string;
      filters: SavedSearchFilters;
      notificationsEnabled?: boolean;
    }) => createSavedSearch(name, filters, notificationsEnabled),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: searchKeys.savedSearchesList() });
    },
  });
}

/**
 * Hook for updating a saved search
 */
export function useUpdateSavedSearch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, updates }: {
      id: string;
      updates: {
        name?: string;
        filters?: SavedSearchFilters;
        notificationsEnabled?: boolean;
      };
    }) => updateSavedSearch(id, updates),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: searchKeys.savedSearchDetail(id) });
      queryClient.invalidateQueries({ queryKey: searchKeys.savedSearchesList() });
    },
  });
}

/**
 * Hook for deleting a saved search
 */
export function useDeleteSavedSearch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteSavedSearch(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: searchKeys.savedSearchesList() });
    },
  });
}

/**
 * Hook for toggling notifications on a saved search
 */
export function useToggleSavedSearchNotifications() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, enabled }: { id: string; enabled: boolean }) => 
      toggleNotifications(id, enabled),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: searchKeys.savedSearchDetail(id) });
      queryClient.invalidateQueries({ queryKey: searchKeys.savedSearchesList() });
    },
  });
}

// ============================================================================
// RECENT SEARCHES (Local Storage)
// ============================================================================

const RECENT_SEARCHES_KEY = 'cardealer_recent_searches';
const MAX_RECENT_SEARCHES = 10;

interface RecentSearch {
  query: string;
  filters?: SavedSearchFilters;
  timestamp: number;
  resultsCount?: number;
}

/**
 * Get recent searches from localStorage
 */
export function getRecentSearches(): RecentSearch[] {
  try {
    const stored = localStorage.getItem(RECENT_SEARCHES_KEY);
    return stored ? JSON.parse(stored) : [];
  } catch {
    return [];
  }
}

/**
 * Add a search to recent searches
 */
export function addRecentSearch(search: Omit<RecentSearch, 'timestamp'>): void {
  try {
    const searches = getRecentSearches();
    
    // Remove duplicate if exists
    const filtered = searches.filter(s => 
      s.query !== search.query || JSON.stringify(s.filters) !== JSON.stringify(search.filters)
    );
    
    // Add new search at the beginning
    filtered.unshift({
      ...search,
      timestamp: Date.now(),
    });
    
    // Keep only the last N searches
    const trimmed = filtered.slice(0, MAX_RECENT_SEARCHES);
    
    localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(trimmed));
  } catch (e) {
    console.error('Error saving recent search:', e);
  }
}

/**
 * Clear recent searches
 */
export function clearRecentSearches(): void {
  localStorage.removeItem(RECENT_SEARCHES_KEY);
}

/**
 * Hook for recent searches
 */
export function useRecentSearches() {
  return useQuery({
    queryKey: searchKeys.recentSearches(),
    queryFn: () => Promise.resolve(getRecentSearches()),
    staleTime: Infinity, // Don't refetch automatically
  });
}

/**
 * Hook for adding to recent searches
 */
export function useAddRecentSearch() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (search: Omit<RecentSearch, 'timestamp'>) => {
      addRecentSearch(search);
      return search;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: searchKeys.recentSearches() });
    },
  });
}

/**
 * Hook for clearing recent searches
 */
export function useClearRecentSearches() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      clearRecentSearches();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: searchKeys.recentSearches() });
    },
  });
}

// ============================================================================
// AUTOCOMPLETE / SUGGESTIONS
// ============================================================================

// Mock popular searches - in production, this would come from SearchService
const popularSearchesData = [
  'Toyota Camry',
  'Honda Civic',
  'Ford F-150',
  'Tesla Model 3',
  'BMW 3 Series',
  'Mercedes-Benz C-Class',
  'Chevrolet Silverado',
  'Audi A4',
];

/**
 * Hook for popular searches
 */
export function usePopularSearches() {
  return useQuery({
    queryKey: searchKeys.popularSearches(),
    queryFn: async () => {
      // TODO: Fetch from SearchService when available
      return popularSearchesData;
    },
    staleTime: 24 * 60 * 60 * 1000, // 24 hours
  });
}

/**
 * Hook for search suggestions/autocomplete
 */
export function useSearchSuggestions(query: string) {
  const popularSearches = usePopularSearches();
  const recentSearches = useRecentSearches();

  return useQuery({
    queryKey: searchKeys.suggestions(query),
    queryFn: async () => {
      // Filter popular searches matching query
      const matchingPopular = (popularSearches.data || [])
        .filter(s => s.toLowerCase().includes(query.toLowerCase()));
      
      // Filter recent searches matching query
      const matchingRecent = (recentSearches.data || [])
        .filter(s => s.query.toLowerCase().includes(query.toLowerCase()))
        .map(s => s.query);
      
      // Combine and deduplicate
      const combined = [...new Set([...matchingRecent, ...matchingPopular])];
      
      return combined.slice(0, 8);
    },
    enabled: query.length >= 2,
    staleTime: 30 * 1000, // 30 seconds
  });
}

// ============================================================================
// COMPOSITE HOOKS
// ============================================================================

/**
 * Hook for complete search functionality
 * Combines vehicle search with saved searches and recent searches
 */
export function useSearchPage(
  filters: VehicleFilters,
  page: number = 1,
  pageSize: number = 12
) {
  const vehicles = useVehicleSearch(filters, page, pageSize);
  const savedSearches = useSavedSearches();
  const recentSearches = useRecentSearches();
  const popularSearches = usePopularSearches();
  const createSavedSearch = useCreateSavedSearch();
  const addRecentSearch = useAddRecentSearch();

  return {
    // Search results
    vehicles: vehicles.data?.vehicles ?? [],
    total: vehicles.data?.total ?? 0,
    totalPages: vehicles.data?.totalPages ?? 0,
    currentPage: page,
    isLoading: vehicles.isLoading,
    isError: vehicles.isError,
    
    // Saved searches
    savedSearches: savedSearches.data ?? [],
    savedSearchesLoading: savedSearches.isLoading,
    
    // Recent & popular
    recentSearches: recentSearches.data ?? [],
    popularSearches: popularSearches.data ?? [],
    
    // Actions
    saveSearch: (name: string, notificationsEnabled?: boolean) => 
      createSavedSearch.mutate({ name, filters, notificationsEnabled }),
    trackSearch: (query: string, resultsCount?: number) =>
      addRecentSearch.mutate({ query, filters, resultsCount }),
    
    // Action states
    isSaving: createSavedSearch.isPending,
    
    // Refetch
    refetch: vehicles.refetch,
    
    // Utilities
    buildSearchUrl,
    formatFilters,
  };
}

/**
 * Hook for saved searches management page
 */
export function useSavedSearchesPage() {
  const savedSearches = useSavedSearches();
  const deleteMutation = useDeleteSavedSearch();
  const toggleNotificationsMutation = useToggleSavedSearchNotifications();

  return {
    searches: savedSearches.data ?? [],
    isLoading: savedSearches.isLoading,
    isError: savedSearches.isError,
    
    deleteSearch: (id: string) => deleteMutation.mutate(id),
    toggleNotifications: (id: string, enabled: boolean) => 
      toggleNotificationsMutation.mutate({ id, enabled }),
    
    isDeleting: deleteMutation.isPending,
    isToggling: toggleNotificationsMutation.isPending,
    
    refetch: savedSearches.refetch,
    
    // Utilities
    buildSearchUrl,
    formatFilters,
  };
}

/**
 * Hook for saved search detail page
 */
export function useSavedSearchDetail(id: string, page: number = 1, pageSize: number = 12) {
  const savedSearch = useSavedSearch(id);
  const results = useSavedSearchResults(id, page, pageSize);
  const checkNew = useCheckSavedSearchResults(id);
  const updateMutation = useUpdateSavedSearch();
  const deleteMutation = useDeleteSavedSearch();
  const toggleNotificationsMutation = useToggleSavedSearchNotifications();

  return {
    // Search details
    search: savedSearch.data,
    isLoading: savedSearch.isLoading,
    isError: savedSearch.isError,
    
    // Results
    vehicles: results.data?.vehicles ?? [],
    total: results.data?.total ?? 0,
    totalPages: results.data?.totalPages ?? 0,
    resultsLoading: results.isLoading,
    
    // New results check
    newResultsCount: checkNew.data?.newResults ?? 0,
    
    // Actions
    update: (updates: { name?: string; filters?: SavedSearchFilters; notificationsEnabled?: boolean }) =>
      updateMutation.mutate({ id, updates }),
    delete: () => deleteMutation.mutate(id),
    toggleNotifications: (enabled: boolean) => 
      toggleNotificationsMutation.mutate({ id, enabled }),
    
    // Action states
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
    isToggling: toggleNotificationsMutation.isPending,
    
    // Refetch
    refetch: () => {
      savedSearch.refetch();
      results.refetch();
      checkNew.refetch();
    },
    
    // Utilities
    buildSearchUrl,
    formatFilters,
  };
}

/**
 * Hook for search bar/autocomplete component
 */
export function useSearchBar(query: string) {
  const suggestions = useSearchSuggestions(query);
  const recentSearches = useRecentSearches();
  const popularSearches = usePopularSearches();
  const addRecent = useAddRecentSearch();
  const clearRecent = useClearRecentSearches();

  return {
    // Suggestions based on current query
    suggestions: suggestions.data ?? [],
    suggestionsLoading: suggestions.isLoading,
    
    // Show recent/popular when query is empty
    recentSearches: recentSearches.data ?? [],
    popularSearches: popularSearches.data ?? [],
    
    // Actions
    trackSearch: (searchQuery: string, resultsCount?: number) =>
      addRecent.mutate({ query: searchQuery, resultsCount }),
    clearRecentSearches: () => clearRecent.mutate(),
    
    // For showing empty state vs suggestions
    hasQuery: query.length >= 2,
    showRecent: query.length < 2 && (recentSearches.data?.length ?? 0) > 0,
    showPopular: query.length < 2 && (recentSearches.data?.length ?? 0) === 0,
  };
}
