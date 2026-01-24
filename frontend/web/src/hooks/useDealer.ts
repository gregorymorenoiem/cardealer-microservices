/**
 * useDealer Hook - React Query hooks for Dealer management
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { dealerApi } from '@/services/dealerSellerService';
import type { CreateDealerRequest, UpdateDealerRequest, VerifyDealerRequest } from '@/types/dealer';
import { toast } from 'react-hot-toast';

// Query keys
export const dealerKeys = {
  all: ['dealers'] as const,
  detail: (id: string) => ['dealers', id] as const,
  byOwner: (ownerId: string) => ['dealers', 'owner', ownerId] as const,
  subscription: (id: string) => ['dealers', id, 'subscription'] as const,
  modules: (id: string) => ['dealers', id, 'modules'] as const,
};

/**
 * Hook to get dealer by ID
 */
export const useDealer = (dealerId: string | undefined) => {
  return useQuery({
    queryKey: dealerKeys.detail(dealerId || ''),
    queryFn: () => dealerApi.getById(dealerId!),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 2,
  });
};

/**
 * Hook to get dealer by owner user ID
 */
export const useDealerByOwner = (ownerUserId: string | undefined) => {
  return useQuery({
    queryKey: dealerKeys.byOwner(ownerUserId || ''),
    queryFn: () => dealerApi.getByOwner(ownerUserId!),
    enabled: !!ownerUserId,
    staleTime: 5 * 60 * 1000,
    retry: 1,
  });
};

/**
 * Hook to get dealer subscription
 */
export const useDealerSubscription = (dealerId: string | undefined) => {
  return useQuery({
    queryKey: dealerKeys.subscription(dealerId || ''),
    queryFn: () => dealerApi.getSubscription(dealerId!),
    enabled: !!dealerId,
    staleTime: 2 * 60 * 1000,
  });
};

/**
 * Hook to get dealer active modules
 */
export const useDealerModules = (dealerId: string | undefined) => {
  return useQuery({
    queryKey: dealerKeys.modules(dealerId || ''),
    queryFn: () => dealerApi.getActiveModules(dealerId!),
    enabled: !!dealerId,
    staleTime: 2 * 60 * 1000,
  });
};

/**
 * Hook to create a new dealer
 */
export const useCreateDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateDealerRequest) => dealerApi.create(data),
    onSuccess: (dealer) => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.all });
      queryClient.setQueryData(dealerKeys.detail(dealer.id), dealer);
      toast.success('Dealer created successfully!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to create dealer');
    },
  });
};

/**
 * Hook to update a dealer
 */
export const useUpdateDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dealerId, data }: { dealerId: string; data: UpdateDealerRequest }) =>
      dealerApi.update(dealerId, data),
    onSuccess: (dealer) => {
      queryClient.setQueryData(dealerKeys.detail(dealer.id), dealer);
      queryClient.invalidateQueries({ queryKey: dealerKeys.byOwner(dealer.ownerUserId) });
      toast.success('Dealer updated successfully!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to update dealer');
    },
  });
};

/**
 * Hook to verify a dealer (Admin)
 */
export const useVerifyDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dealerId, data }: { dealerId: string; data: VerifyDealerRequest }) =>
      dealerApi.verify(dealerId, data),
    onSuccess: (dealer) => {
      queryClient.setQueryData(dealerKeys.detail(dealer.id), dealer);
      queryClient.invalidateQueries({ queryKey: dealerKeys.all });
      toast.success(dealer.verificationStatus === 2 ? 'Dealer verified!' : 'Dealer rejected');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to verify dealer');
    },
  });
};

/**
 * Hook to subscribe to a module
 */
export const useSubscribeToModule = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dealerId, moduleCode }: { dealerId: string; moduleCode: string }) =>
      dealerApi.subscribeToModule(dealerId, moduleCode),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.modules(variables.dealerId) });
      toast.success('Module activated!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to subscribe to module');
    },
  });
};

/**
 * Hook to unsubscribe from a module
 */
export const useUnsubscribeFromModule = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dealerId, moduleCode }: { dealerId: string; moduleCode: string }) =>
      dealerApi.unsubscribeFromModule(dealerId, moduleCode),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.modules(variables.dealerId) });
      toast.success('Module deactivated');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to unsubscribe from module');
    },
  });
};

/**
 * Hook to register a new dealer (onboarding)
 */
export const useRegisterDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { email: string; businessName: string; phone: string }) =>
      dealerApi.register(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: dealerKeys.all });
      toast.success('Registration submitted! We will review your application.');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Failed to register');
    },
  });
};

/**
 * Combined hook for common dealer operations
 */
export const useDealerManagement = (dealerId?: string, ownerUserId?: string) => {
  const dealerQuery = useDealer(dealerId);
  const dealerByOwnerQuery = useDealerByOwner(ownerUserId);
  const subscriptionQuery = useDealerSubscription(dealerId);
  const modulesQuery = useDealerModules(dealerId);

  const createMutation = useCreateDealer();
  const updateMutation = useUpdateDealer();
  const verifyMutation = useVerifyDealer();

  // Determine which dealer data to use
  const dealer = dealerQuery.data || dealerByOwnerQuery.data;
  const isLoading = dealerQuery.isLoading || dealerByOwnerQuery.isLoading;
  const error = dealerQuery.error || dealerByOwnerQuery.error;

  return {
    // Data
    dealer,
    subscription: subscriptionQuery.data,
    modules: modulesQuery.data,

    // States
    isLoading,
    error,
    isSubscriptionLoading: subscriptionQuery.isLoading,
    isModulesLoading: modulesQuery.isLoading,

    // Mutations
    createDealer: createMutation.mutateAsync,
    updateDealer: updateMutation.mutateAsync,
    verifyDealer: verifyMutation.mutateAsync,

    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isVerifying: verifyMutation.isPending,
  };
};

export default useDealerManagement;
