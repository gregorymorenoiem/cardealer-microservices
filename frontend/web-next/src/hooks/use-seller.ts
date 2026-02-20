/**
 * React Query hooks for Seller operations
 * Provides conversion, profile management, and stats
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  convertToSeller,
  getSellerProfile,
  getSellerByUserId,
  createSellerProfile,
  updateSellerProfile,
  getSellerStats,
  type ConvertToSellerRequest,
  type SellerConversionResult,
  type SellerProfile,
  type CreateSellerProfileRequest,
  type UpdateSellerProfileRequest,
  type SellerStats,
} from '@/services/sellers';

// =============================================================================
// QUERY KEYS
// =============================================================================

export const sellerKeys = {
  all: ['sellers'] as const,
  profiles: () => [...sellerKeys.all, 'profile'] as const,
  profile: (id: string) => [...sellerKeys.profiles(), id] as const,
  byUser: (userId: string) => [...sellerKeys.all, 'by-user', userId] as const,
  stats: (sellerId: string) => [...sellerKeys.all, 'stats', sellerId] as const,
};

// =============================================================================
// QUERIES
// =============================================================================

/**
 * Get a seller profile by ID
 */
export function useSellerProfile(sellerId: string | undefined) {
  return useQuery({
    queryKey: sellerKeys.profile(sellerId ?? ''),
    queryFn: () => getSellerProfile(sellerId!),
    enabled: !!sellerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get a seller profile by user ID
 */
export function useSellerByUserId(userId: string | undefined) {
  return useQuery({
    queryKey: sellerKeys.byUser(userId ?? ''),
    queryFn: () => getSellerByUserId(userId!),
    enabled: !!userId,
    staleTime: 5 * 60 * 1000,
    retry: false, // Don't retry 404s — user may not be a seller
  });
}

/**
 * Get seller stats
 */
export function useSellerStats(sellerId: string | undefined) {
  return useQuery({
    queryKey: sellerKeys.stats(sellerId ?? ''),
    queryFn: () => getSellerStats(sellerId!),
    enabled: !!sellerId,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

// =============================================================================
// MUTATIONS
// =============================================================================

/**
 * Convert a buyer account to a seller account
 */
export function useConvertToSeller() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      data,
      idempotencyKey,
    }: {
      data: ConvertToSellerRequest;
      idempotencyKey?: string;
    }) => convertToSeller(data, idempotencyKey),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sellerKeys.all });
    },
  });
}

/**
 * Create a new seller profile
 */
export function useCreateSellerProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateSellerProfileRequest) => createSellerProfile(data),
    onSuccess: profile => {
      queryClient.setQueryData(sellerKeys.profile(profile.id), profile);
      queryClient.invalidateQueries({ queryKey: sellerKeys.all });
    },
  });
}

/**
 * Update an existing seller profile
 */
export function useUpdateSellerProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ sellerId, data }: { sellerId: string; data: UpdateSellerProfileRequest }) =>
      updateSellerProfile(sellerId, data),
    onSuccess: profile => {
      queryClient.setQueryData(sellerKeys.profile(profile.id), profile);
      queryClient.invalidateQueries({ queryKey: sellerKeys.all });
    },
  });
}

// =============================================================================
// RE-EXPORTS
// =============================================================================

export type {
  ConvertToSellerRequest,
  SellerConversionResult,
  SellerProfile,
  CreateSellerProfileRequest,
  UpdateSellerProfileRequest,
  SellerStats,
};
