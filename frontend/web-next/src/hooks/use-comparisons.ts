/**
 * Comparison Hook - React hook for vehicle comparisons
 *
 * Provides:
 * - Local comparison state (for guests)
 * - API comparison management (for authenticated users)
 * - Comparison sharing
 */

'use client';

import * as React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  comparisonService,
  type VehicleComparison,
  type CreateComparisonRequest,
} from '@/services/comparisons';
import { useAuth } from './use-auth';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const comparisonKeys = {
  all: ['comparisons'] as const,
  lists: () => [...comparisonKeys.all, 'list'] as const,
  details: () => [...comparisonKeys.all, 'detail'] as const,
  detail: (id: string) => [...comparisonKeys.details(), id] as const,
  shared: (token: string) => [...comparisonKeys.all, 'shared', token] as const,
  specs: (vehicleIds: string[]) => [...comparisonKeys.all, 'specs', vehicleIds] as const,
  local: () => ['local-comparison'] as const,
};

// =============================================================================
// LOCAL COMPARISON STATE
// =============================================================================

interface LocalComparisonState {
  vehicleIds: string[];
  addVehicle: (vehicleId: string) => void;
  removeVehicle: (vehicleId: string) => void;
  clearAll: () => void;
  isInComparison: (vehicleId: string) => boolean;
  canAddMore: boolean;
}

/**
 * Hook for local comparison state (guest users or quick compare)
 */
export function useLocalComparison(): LocalComparisonState {
  const [vehicleIds, setVehicleIds] = React.useState<string[]>([]);

  // Load from localStorage on mount
  React.useEffect(() => {
    setVehicleIds(comparisonService.getLocalComparisonVehicles());
  }, []);

  const addVehicle = React.useCallback((vehicleId: string) => {
    const updated = comparisonService.addToLocalComparison(vehicleId);
    setVehicleIds(updated);
  }, []);

  const removeVehicle = React.useCallback((vehicleId: string) => {
    const updated = comparisonService.removeFromLocalComparison(vehicleId);
    setVehicleIds(updated);
  }, []);

  const clearAll = React.useCallback(() => {
    comparisonService.clearLocalComparison();
    setVehicleIds([]);
  }, []);

  const isInComparison = React.useCallback(
    (vehicleId: string) => vehicleIds.includes(vehicleId),
    [vehicleIds]
  );

  return {
    vehicleIds,
    addVehicle,
    removeVehicle,
    clearAll,
    isInComparison,
    canAddMore: vehicleIds.length < comparisonService.MAX_COMPARISON_VEHICLES,
  };
}

// =============================================================================
// API COMPARISON HOOKS
// =============================================================================

/**
 * Get user's comparisons
 */
export function useComparisons() {
  const { isAuthenticated } = useAuth();

  return useQuery({
    queryKey: comparisonKeys.lists(),
    queryFn: () => comparisonService.getComparisons(),
    enabled: isAuthenticated,
    staleTime: 2 * 60 * 1000,
  });
}

/**
 * Get comparison by ID
 */
export function useComparison(id: string | undefined) {
  return useQuery({
    queryKey: comparisonKeys.detail(id || ''),
    queryFn: () => comparisonService.getComparisonById(id!),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get comparison by share token (public)
 */
export function useSharedComparison(token: string | undefined) {
  return useQuery({
    queryKey: comparisonKeys.shared(token || ''),
    queryFn: () => comparisonService.getComparisonByShareToken(token!),
    enabled: !!token,
    staleTime: 10 * 60 * 1000,
  });
}

/**
 * Get comparison specs for vehicles
 */
export function useComparisonSpecs(vehicleIds: string[]) {
  return useQuery({
    queryKey: comparisonKeys.specs(vehicleIds),
    queryFn: () => comparisonService.getComparisonSpecs(vehicleIds),
    enabled: vehicleIds.length >= 2,
    staleTime: 10 * 60 * 1000,
  });
}

// =============================================================================
// MUTATIONS
// =============================================================================

/**
 * Create comparison mutation
 */
export function useCreateComparison() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateComparisonRequest) => comparisonService.createComparison(data),
    onSuccess: newComparison => {
      queryClient.invalidateQueries({ queryKey: comparisonKeys.lists() });
      queryClient.setQueryData(comparisonKeys.detail(newComparison.id), newComparison);
    },
  });
}

/**
 * Add vehicle to comparison mutation
 */
export function useAddVehicleToComparison() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ comparisonId, vehicleId }: { comparisonId: string; vehicleId: string }) =>
      comparisonService.addVehicleToComparison(comparisonId, vehicleId),
    onSuccess: updated => {
      queryClient.setQueryData(comparisonKeys.detail(updated.id), updated);
    },
  });
}

/**
 * Remove vehicle from comparison mutation
 */
export function useRemoveVehicleFromComparison() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ comparisonId, vehicleId }: { comparisonId: string; vehicleId: string }) =>
      comparisonService.removeVehicleFromComparison(comparisonId, vehicleId),
    onSuccess: updated => {
      queryClient.setQueryData(comparisonKeys.detail(updated.id), updated);
    },
  });
}

/**
 * Delete comparison mutation
 */
export function useDeleteComparison() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => comparisonService.deleteComparison(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: comparisonKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: comparisonKeys.lists() });
    },
  });
}

/**
 * Share comparison mutation
 */
export function useShareComparison() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (comparisonId: string) => comparisonService.shareComparison(comparisonId),
    onSuccess: (result, comparisonId) => {
      // Update the comparison with share token
      queryClient.setQueryData<VehicleComparison | undefined>(
        comparisonKeys.detail(comparisonId),
        old =>
          old
            ? {
                ...old,
                shareToken: result.shareToken,
                isPublic: true,
              }
            : old
      );
    },
  });
}

// =============================================================================
// COMBINED HOOK
// =============================================================================

/**
 * Combined comparison hook that handles both local and API comparisons
 */
export function useComparisonManager() {
  const { isAuthenticated } = useAuth();
  const localComparison = useLocalComparison();
  const { data: savedComparisons } = useComparisons();
  const createMutation = useCreateComparison();

  // Save current local comparison to API
  const saveComparison = React.useCallback(
    async (name?: string) => {
      if (!isAuthenticated || localComparison.vehicleIds.length === 0) return null;

      const result = await createMutation.mutateAsync({
        vehicleIds: localComparison.vehicleIds,
        name,
      });

      // Clear local storage after saving
      localComparison.clearAll();

      return result;
    },
    [isAuthenticated, localComparison, createMutation]
  );

  return {
    // Local comparison state
    ...localComparison,
    // API comparisons
    savedComparisons: savedComparisons || [],
    // Actions
    saveComparison,
    isSaving: createMutation.isPending,
    // Auth status
    isAuthenticated,
  };
}

// =============================================================================
// EXPORTS
// =============================================================================

export { comparisonService };
export type { VehicleComparison };
