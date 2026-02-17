/**
 * useFavorites Hook
 *
 * Manages user favorites with React Query
 * Handles both authenticated and local storage favorites
 */

'use client';

import * as React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  favoritesService,
  FavoriteVehicle,
  AddFavoriteRequest,
  UpdateFavoriteRequest,
} from '@/services/favorites';
import { useAuth } from './use-auth';

// =============================================================================
// TYPES
// =============================================================================

export interface UseFavoritesReturn {
  /** List of favorited vehicles */
  favorites: FavoriteVehicle[];
  /** Total count of favorites */
  count: number;
  /** Loading state */
  isLoading: boolean;
  /** Error state */
  error: Error | null;
  /** Check if a vehicle is favorited */
  isFavorite: (vehicleId: string) => boolean;
  /** Add a vehicle to favorites */
  addFavorite: (
    vehicleId: string,
    options?: Omit<AddFavoriteRequest, 'vehicleId'>
  ) => Promise<void>;
  /** Remove a vehicle from favorites */
  removeFavorite: (vehicleId: string) => Promise<void>;
  /** Toggle favorite status */
  toggleFavorite: (vehicleId: string) => Promise<boolean>;
  /** Update favorite settings */
  updateFavorite: (vehicleId: string, updates: UpdateFavoriteRequest) => Promise<void>;
  /** Whether user is adding a favorite */
  isAdding: boolean;
  /** Whether user is removing a favorite */
  isRemoving: boolean;
}

// =============================================================================
// HOOK
// =============================================================================

export function useFavorites(): UseFavoritesReturn {
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();

  // Local state for unauthenticated users
  const [localFavorites, setLocalFavorites] = React.useState<string[]>(() =>
    typeof window !== 'undefined' ? favoritesService.local.getFavorites() : []
  );

  // Sync local favorites on mount and when auth state changes
  React.useEffect(() => {
    if (isAuthenticated && localFavorites.length > 0) {
      // Sync local favorites to server
      favoritesService.syncLocalFavorites().then(() => {
        setLocalFavorites([]);
        // Refetch server favorites
        queryClient.invalidateQueries({ queryKey: ['favorites'] });
      });
    }
  }, [isAuthenticated, localFavorites.length, queryClient]);

  // Query for authenticated users
  const {
    data: favoritesData,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['favorites'],
    queryFn: () => favoritesService.getFavorites(),
    enabled: isAuthenticated,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  // Add favorite mutation
  const addMutation = useMutation({
    mutationFn: (request: AddFavoriteRequest) => favoritesService.addFavorite(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] });
    },
  });

  // Remove favorite mutation
  const removeMutation = useMutation({
    mutationFn: (vehicleId: string) => favoritesService.removeFavorite(vehicleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] });
    },
  });

  // Update favorite mutation
  const updateMutation = useMutation({
    mutationFn: ({ vehicleId, updates }: { vehicleId: string; updates: UpdateFavoriteRequest }) =>
      favoritesService.updateFavorite(vehicleId, updates),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] });
    },
  });

  // Get favorites list
  const favorites = React.useMemo(() => {
    if (isAuthenticated) {
      return favoritesData?.favorites || [];
    }
    // For unauthenticated users, return empty vehicle objects with IDs
    return localFavorites.map(id => ({
      id,
      vehicleId: id,
      userId: '',
      createdAt: new Date().toISOString(),
      vehicle: {
        id,
        slug: '',
        title: '',
        make: '',
        model: '',
        year: 0,
        price: 0,
        mileage: 0,
        transmission: '',
        fuelType: '',
        bodyType: '',
        location: '',
        imageUrl: '',
        status: 'active' as const,
      },
      notifyOnPriceChange: false,
    }));
  }, [isAuthenticated, favoritesData, localFavorites]);

  // Get favorites count
  const count = isAuthenticated ? favoritesData?.total || 0 : localFavorites.length;

  // Check if vehicle is favorited
  const isFavorite = React.useCallback(
    (vehicleId: string): boolean => {
      if (isAuthenticated) {
        return favorites.some(f => f.vehicleId === vehicleId);
      }
      return localFavorites.includes(vehicleId);
    },
    [isAuthenticated, favorites, localFavorites]
  );

  // Add to favorites
  const addFavorite = React.useCallback(
    async (vehicleId: string, options?: Omit<AddFavoriteRequest, 'vehicleId'>) => {
      if (isAuthenticated) {
        await addMutation.mutateAsync({ vehicleId, ...options });
      } else {
        favoritesService.local.addFavorite(vehicleId);
        setLocalFavorites(prev => [...prev, vehicleId]);
      }
    },
    [isAuthenticated, addMutation]
  );

  // Remove from favorites
  const removeFavorite = React.useCallback(
    async (vehicleId: string) => {
      if (isAuthenticated) {
        await removeMutation.mutateAsync(vehicleId);
      } else {
        favoritesService.local.removeFavorite(vehicleId);
        setLocalFavorites(prev => prev.filter(id => id !== vehicleId));
      }
    },
    [isAuthenticated, removeMutation]
  );

  // Toggle favorite status
  const toggleFavorite = React.useCallback(
    async (vehicleId: string): Promise<boolean> => {
      const isFav = isFavorite(vehicleId);

      if (isFav) {
        await removeFavorite(vehicleId);
        return false;
      } else {
        await addFavorite(vehicleId);
        return true;
      }
    },
    [isFavorite, addFavorite, removeFavorite]
  );

  // Update favorite
  const updateFavorite = React.useCallback(
    async (vehicleId: string, updates: UpdateFavoriteRequest) => {
      if (isAuthenticated) {
        await updateMutation.mutateAsync({ vehicleId, updates });
      }
    },
    [isAuthenticated, updateMutation]
  );

  return {
    favorites,
    count,
    isLoading: isAuthenticated ? isLoading : false,
    error: error as Error | null,
    isFavorite,
    addFavorite,
    removeFavorite,
    toggleFavorite,
    updateFavorite,
    isAdding: addMutation.isPending,
    isRemoving: removeMutation.isPending,
  };
}

// =============================================================================
// SINGLE FAVORITE HOOK
// =============================================================================

/**
 * Hook to check if a single vehicle is favorited
 * Optimized for vehicle cards
 */
export function useFavoriteStatus(vehicleId: string) {
  const { isFavorite, toggleFavorite, isAdding, isRemoving } = useFavorites();

  const [isOptimisticFavorite, setIsOptimisticFavorite] = React.useState<boolean | null>(null);

  const isFavorited = isOptimisticFavorite ?? isFavorite(vehicleId);

  const toggle = React.useCallback(async () => {
    // Optimistic update
    setIsOptimisticFavorite(!isFavorited);

    try {
      await toggleFavorite(vehicleId);
    } catch {
      // Revert on error
      setIsOptimisticFavorite(null);
    } finally {
      // Clear optimistic state after mutation settles
      setTimeout(() => setIsOptimisticFavorite(null), 100);
    }
  }, [vehicleId, isFavorited, toggleFavorite]);

  return {
    isFavorite: isFavorited,
    toggle,
    isLoading: isAdding || isRemoving,
  };
}

export default useFavorites;
