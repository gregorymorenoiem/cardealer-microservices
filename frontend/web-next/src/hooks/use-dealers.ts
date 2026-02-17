/**
 * Dealer Hook - React hook for dealer operations
 *
 * Provides:
 * - Current dealer state (for dealer users)
 * - Dealer CRUD operations
 * - Statistics and analytics
 */

'use client';

import * as React from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  dealerService,
  type DealerSearchParams,
  type CreateDealerRequest,
  type UpdateDealerRequest,
  type DealerLocationDto,
  type DealerDocumentDto,
  type UploadDocumentRequest,
  DOCUMENT_TYPE_LABELS,
  REQUIRED_DOCUMENT_TYPES,
  OPTIONAL_DOCUMENT_TYPES,
} from '@/services/dealers';
import type { Dealer } from '@/types';
import { useAuth } from './use-auth';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const dealerKeys = {
  all: ['dealers'] as const,
  lists: () => [...dealerKeys.all, 'list'] as const,
  list: (params: DealerSearchParams) => [...dealerKeys.lists(), params] as const,
  details: () => [...dealerKeys.all, 'detail'] as const,
  detail: (id: string) => [...dealerKeys.details(), id] as const,
  bySlug: (slug: string) => [...dealerKeys.all, 'slug', slug] as const,
  current: () => [...dealerKeys.all, 'current'] as const,
  stats: (id: string) => [...dealerKeys.all, 'stats', id] as const,
  locations: (id: string) => [...dealerKeys.all, 'locations', id] as const,
  documents: (id: string) => [...dealerKeys.all, 'documents', id] as const,
};

// =============================================================================
// HOOKS
// =============================================================================

/**
 * Get current dealer (for logged-in dealer users)
 */
export function useCurrentDealer() {
  const { user, isAuthenticated } = useAuth();

  const isDealerUser = user?.accountType === 'dealer';

  return useQuery({
    queryKey: dealerKeys.current(),
    queryFn: () => dealerService.getCurrentDealer(),
    enabled: isAuthenticated && isDealerUser,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get dealer by ID
 */
export function useDealer(id: string | undefined) {
  return useQuery({
    queryKey: dealerKeys.detail(id || ''),
    queryFn: () => dealerService.getDealerById(id!),
    enabled: !!id,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get dealer by slug
 */
export function useDealerBySlug(slug: string | undefined) {
  return useQuery({
    queryKey: dealerKeys.bySlug(slug || ''),
    queryFn: () => dealerService.getDealerBySlug(slug!),
    enabled: !!slug,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Get list of dealers
 */
export function useDealers(params: DealerSearchParams = {}) {
  return useQuery({
    queryKey: dealerKeys.list(params),
    queryFn: () => dealerService.getDealers(params),
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

/**
 * Get dealer statistics
 */
export function useDealerStats(dealerId: string | undefined) {
  return useQuery({
    queryKey: dealerKeys.stats(dealerId || ''),
    queryFn: () => dealerService.getDealerStats(dealerId!),
    enabled: !!dealerId,
    retry: 1,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get dealer locations
 */
export function useDealerLocations(dealerId: string | undefined) {
  return useQuery({
    queryKey: dealerKeys.locations(dealerId || ''),
    queryFn: () => dealerService.getDealerLocations(dealerId!),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
  });
}

// =============================================================================
// MUTATIONS
// =============================================================================

/**
 * Create dealer mutation
 */
export function useCreateDealer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateDealerRequest) => dealerService.createDealer(data),
    onSuccess: newDealer => {
      // Invalidate lists and set current dealer
      queryClient.invalidateQueries({ queryKey: dealerKeys.lists() });
      queryClient.setQueryData(dealerKeys.current(), newDealer);
      queryClient.setQueryData(dealerKeys.detail(newDealer.id), newDealer);
    },
  });
}

/**
 * Update dealer mutation
 */
export function useUpdateDealer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDealerRequest }) =>
      dealerService.updateDealer(id, data),
    onSuccess: updatedDealer => {
      // Update caches
      queryClient.setQueryData(dealerKeys.detail(updatedDealer.id), updatedDealer);
      queryClient.invalidateQueries({ queryKey: dealerKeys.current() });
      queryClient.invalidateQueries({ queryKey: dealerKeys.lists() });
    },
  });
}

/**
 * Add dealer location mutation
 */
export function useAddDealerLocation(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: Omit<DealerLocationDto, 'id' | 'dealerId'>) =>
      dealerService.addDealerLocation(dealerId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.locations(dealerId) });
    },
  });
}

/**
 * Update dealer location mutation
 */
export function useUpdateDealerLocation(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ locationId, data }: { locationId: string; data: Partial<DealerLocationDto> }) =>
      dealerService.updateDealerLocation(dealerId, locationId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.locations(dealerId) });
    },
  });
}

/**
 * Delete dealer location mutation
 */
export function useDeleteDealerLocation(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (locationId: string) => dealerService.deleteDealerLocation(dealerId, locationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.locations(dealerId) });
    },
  });
}

// =============================================================================
// DOCUMENT HOOKS
// =============================================================================

/**
 * Get dealer documents
 */
export function useDealerDocuments(dealerId: string | undefined) {
  return useQuery({
    queryKey: dealerKeys.documents(dealerId || ''),
    queryFn: () => dealerService.getDealerDocuments(dealerId!),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Upload dealer document mutation
 */
export function useUploadDealerDocument(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UploadDocumentRequest) =>
      dealerService.uploadDealerDocument(dealerId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.documents(dealerId) });
      queryClient.invalidateQueries({ queryKey: dealerKeys.current() });
    },
  });
}

/**
 * Delete dealer document mutation
 */
export function useDeleteDealerDocument(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (documentId: string) => dealerService.deleteDealerDocument(dealerId, documentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.documents(dealerId) });
      queryClient.invalidateQueries({ queryKey: dealerKeys.current() });
    },
  });
}

/**
 * Get document verification stats
 */
export function useDocumentVerificationStats(documents: DealerDocumentDto[] | undefined) {
  if (!documents) {
    return {
      verifiedCount: 0,
      totalRequired: REQUIRED_DOCUMENT_TYPES.length,
      pendingCount: 0,
      rejectedCount: 0,
      expiringCount: 0,
      isFullyVerified: false,
    };
  }

  const requiredDocs = documents.filter(d => REQUIRED_DOCUMENT_TYPES.includes(d.type));
  const verifiedCount = requiredDocs.filter(d => d.verificationStatus === 'Approved').length;
  const pendingCount = documents.filter(
    d => d.verificationStatus === 'Pending' || d.verificationStatus === 'UnderReview'
  ).length;
  const rejectedCount = documents.filter(d => d.verificationStatus === 'Rejected').length;
  const expiringCount = documents.filter(d => {
    if (!d.expiryDate) return false;
    const expiry = new Date(d.expiryDate);
    const thirtyDaysFromNow = new Date();
    thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
    return expiry <= thirtyDaysFromNow && expiry > new Date();
  }).length;

  return {
    verifiedCount,
    totalRequired: REQUIRED_DOCUMENT_TYPES.length,
    pendingCount,
    rejectedCount,
    expiringCount,
    isFullyVerified: verifiedCount === REQUIRED_DOCUMENT_TYPES.length,
  };
}

// =============================================================================
// UTILITY HOOKS
// =============================================================================

/**
 * Hook for early bird offer status
 */
export function useEarlyBird() {
  const [daysRemaining, setDaysRemaining] = React.useState(
    dealerService.getEarlyBirdDaysRemaining()
  );
  const isActive = dealerService.isEarlyBirdActive();

  React.useEffect(() => {
    if (!isActive) return;

    // Update days remaining every hour
    const interval = setInterval(
      () => {
        setDaysRemaining(dealerService.getEarlyBirdDaysRemaining());
      },
      60 * 60 * 1000
    );

    return () => clearInterval(interval);
  }, [isActive]);

  return {
    isActive,
    daysRemaining,
    getDiscountedPrice: (price: number) => dealerService.getEarlyBirdPrice(price),
  };
}

/**
 * Combined hook for dealer dashboard data
 */
export function useDealerDashboard() {
  const { data: dealer, isLoading: isDealerLoading } = useCurrentDealer();
  const { data: stats, isLoading: isStatsLoading } = useDealerStats(dealer?.id);
  const { data: locations, isLoading: isLocationsLoading } = useDealerLocations(dealer?.id);

  return {
    dealer,
    stats,
    locations,
    isLoading: isDealerLoading || isStatsLoading || isLocationsLoading,
    planInfo: dealer ? dealerService.getPlanInfo(dealer.plan) : undefined,
  };
}

// =============================================================================
// EXPORTS
// =============================================================================

export {
  dealerService,
  type Dealer,
  type DealerSearchParams,
  type CreateDealerRequest,
  type UpdateDealerRequest,
  type DealerDocumentDto,
  type UploadDocumentRequest,
  DOCUMENT_TYPE_LABELS,
  REQUIRED_DOCUMENT_TYPES,
  OPTIONAL_DOCUMENT_TYPES,
};
