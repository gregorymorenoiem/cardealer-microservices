/**
 * useSeller Hook - React Query hooks for Seller profile management
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { sellerApi } from '@/services/dealerSellerService';
import type {
  CreateSellerRequest,
  UpdateSellerRequest,
  VerifySellerRequest,
} from '@/types/dealer';
import { toast } from 'react-hot-toast';

// Query keys
export const sellerKeys = {
  all: ['sellers'] as const,
  detail: (id: string) => ['sellers', id] as const,
  byUser: (userId: string) => ['sellers', 'user', userId] as const,
  stats: (id: string) => ['sellers', id, 'stats'] as const,
};

/**
 * Hook to get seller by ID
 */
export const useSeller = (sellerId: string | undefined) => {
  return useQuery({
    queryKey: sellerKeys.detail(sellerId || ''),
    queryFn: () => sellerApi.getById(sellerId!),
    enabled: !!sellerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
  });
};

/**
 * Hook to get seller by user ID
 */
export const useSellerByUser = (userId: string | undefined) => {
  return useQuery({
    queryKey: sellerKeys.byUser(userId || ''),
    queryFn: () => sellerApi.getByUser(userId!),
    enabled: !!userId,
    staleTime: 5 * 60 * 1000,
    retry: 1,
  });
};

/**
 * Hook to get seller stats
 */
export const useSellerStats = (sellerId: string | undefined) => {
  return useQuery({
    queryKey: sellerKeys.stats(sellerId || ''),
    queryFn: () => sellerApi.getStats(sellerId!),
    enabled: !!sellerId,
    staleTime: 2 * 60 * 1000,
  });
};

/**
 * Hook to create a new seller profile
 */
export const useCreateSeller = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateSellerRequest) => sellerApi.create(data),
    onSuccess: (seller) => {
      queryClient.invalidateQueries({ queryKey: sellerKeys.all });
      queryClient.setQueryData(sellerKeys.detail(seller.id), seller);
      queryClient.setQueryData(sellerKeys.byUser(seller.userId), seller);
      toast.success('Seller profile created successfully!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create seller profile');
    },
  });
};

/**
 * Hook to update seller profile
 */
export const useUpdateSeller = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ sellerId, data }: { sellerId: string; data: UpdateSellerRequest }) =>
      sellerApi.update(sellerId, data),
    onSuccess: (seller) => {
      queryClient.setQueryData(sellerKeys.detail(seller.id), seller);
      queryClient.setQueryData(sellerKeys.byUser(seller.userId), seller);
      toast.success('Profile updated successfully!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update profile');
    },
  });
};

/**
 * Hook to verify seller (Admin)
 */
export const useVerifySeller = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ sellerId, data }: { sellerId: string; data: VerifySellerRequest }) =>
      sellerApi.verify(sellerId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: sellerKeys.detail(variables.sellerId) });
      queryClient.invalidateQueries({ queryKey: sellerKeys.all });
      toast.success('Seller verification status updated!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to verify seller');
    },
  });
};

/**
 * Combined hook for seller profile management
 */
export const useSellerManagement = (sellerId?: string, userId?: string) => {
  const sellerQuery = useSeller(sellerId);
  const sellerByUserQuery = useSellerByUser(userId);
  
  // Use the seller ID from either query
  const effectiveSellerId = sellerId || sellerByUserQuery.data?.id;
  const statsQuery = useSellerStats(effectiveSellerId);
  
  const createMutation = useCreateSeller();
  const updateMutation = useUpdateSeller();
  const verifyMutation = useVerifySeller();

  // Determine which seller data to use
  const seller = sellerQuery.data || sellerByUserQuery.data;
  const isLoading = sellerQuery.isLoading || sellerByUserQuery.isLoading;
  const error = sellerQuery.error || sellerByUserQuery.error;

  return {
    // Data
    seller,
    stats: statsQuery.data,
    
    // States
    isLoading,
    error,
    hasProfile: !!seller,
    isStatsLoading: statsQuery.isLoading,
    
    // Mutations
    createSeller: createMutation.mutateAsync,
    updateSeller: updateMutation.mutateAsync,
    verifySeller: verifyMutation.mutateAsync,
    
    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isVerifying: verifyMutation.isPending,
  };
};

export default useSellerManagement;
