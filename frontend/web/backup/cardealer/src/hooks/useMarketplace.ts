/**
 * useMarketplace - React Query hooks for marketplace data
 */

import { useQuery, useInfiniteQuery } from '@tanstack/react-query';
import { marketplaceService } from '@/services/marketplaceService';
import type {
  Listing,
  MarketplaceSearchParams,
  VehicleSearchParams,
  PropertySearchParams,
  MarketplaceVertical,
} from '@/types/marketplace';

// Query keys
export const marketplaceKeys = {
  all: ['marketplace'] as const,
  categories: () => [...marketplaceKeys.all, 'categories'] as const,
  listings: (params?: MarketplaceSearchParams) => [...marketplaceKeys.all, 'listings', params] as const,
  listing: (id: string) => [...marketplaceKeys.all, 'listing', id] as const,
  vehicles: (params?: VehicleSearchParams) => [...marketplaceKeys.all, 'vehicles', params] as const,
  properties: (params?: PropertySearchParams) => [...marketplaceKeys.all, 'properties', params] as const,
  featured: (vertical?: MarketplaceVertical) => [...marketplaceKeys.all, 'featured', vertical] as const,
  similar: (id: string) => [...marketplaceKeys.all, 'similar', id] as const,
};

/**
 * Get marketplace categories/verticals
 */
export function useCategories() {
  return useQuery({
    queryKey: marketplaceKeys.categories(),
    queryFn: () => marketplaceService.getCategories(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get all listings with optional filtering
 */
export function useListings(params: MarketplaceSearchParams = {}) {
  return useQuery({
    queryKey: marketplaceKeys.listings(params),
    queryFn: () => marketplaceService.getListings(params),
  });
}

/**
 * Get infinite listings with pagination
 */
export function useInfiniteListings(params: Omit<MarketplaceSearchParams, 'page'> = {}) {
  return useInfiniteQuery({
    queryKey: [...marketplaceKeys.listings(params), 'infinite'],
    queryFn: ({ pageParam = 1 }) => marketplaceService.getListings({ ...params, page: pageParam }),
    getNextPageParam: (lastPage) => {
      if (lastPage.page < lastPage.totalPages) {
        return lastPage.page + 1;
      }
      return undefined;
    },
    initialPageParam: 1,
  });
}

/**
 * Get a single listing by ID
 */
export function useListing(id: string) {
  return useQuery({
    queryKey: marketplaceKeys.listing(id),
    queryFn: () => marketplaceService.getListingById(id),
    enabled: !!id,
  });
}

/**
 * Get vehicles with specific filters
 */
export function useVehicles(params: Omit<VehicleSearchParams, 'vertical'> = {}) {
  return useQuery({
    queryKey: marketplaceKeys.vehicles({ ...params, vertical: 'vehicles' }),
    queryFn: () => marketplaceService.getVehicles({ ...params, vertical: 'vehicles' }),
  });
}

/**
 * Get properties with specific filters
 */
export function useProperties(params: Omit<PropertySearchParams, 'vertical'> = {}) {
  return useQuery({
    queryKey: marketplaceKeys.properties({ ...params, vertical: 'real-estate' }),
    queryFn: () => marketplaceService.getProperties({ ...params, vertical: 'real-estate' }),
  });
}

/**
 * Get featured listings
 */
export function useFeaturedListings(limit = 8) {
  return useQuery({
    queryKey: [...marketplaceKeys.featured(), 'all', limit],
    queryFn: () => marketplaceService.getFeaturedListings(limit),
  });
}

/**
 * Get featured vehicles
 */
export function useFeaturedVehicles(limit = 8) {
  return useQuery({
    queryKey: [...marketplaceKeys.featured('vehicles'), limit],
    queryFn: () => marketplaceService.getFeaturedVehicles(limit),
  });
}

/**
 * Get featured properties
 */
export function useFeaturedProperties(limit = 8) {
  return useQuery({
    queryKey: [...marketplaceKeys.featured('real-estate'), limit],
    queryFn: () => marketplaceService.getFeaturedProperties(limit),
  });
}

/**
 * Get similar listings
 */
export function useSimilarListings(listing: Listing | null, limit = 4) {
  return useQuery({
    queryKey: marketplaceKeys.similar(listing?.id ?? ''),
    queryFn: () => marketplaceService.getSimilarListings(listing!, limit),
    enabled: !!listing,
  });
}

/**
 * Get popular makes (for vehicles)
 */
export function usePopularMakes() {
  return useQuery({
    queryKey: [...marketplaceKeys.all, 'popularMakes'],
    queryFn: () => marketplaceService.getPopularMakes(),
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

/**
 * Get popular cities (for properties)
 */
export function usePopularCities() {
  return useQuery({
    queryKey: [...marketplaceKeys.all, 'popularCities'],
    queryFn: () => marketplaceService.getPopularCities(),
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}
