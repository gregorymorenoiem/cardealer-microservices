/**
 * useDealerOnboarding Hook
 *
 * React Query hooks for the Dealer Onboarding V2 flow.
 * Provides:
 * - Registration mutation
 * - Email verification mutation
 * - Documents upload mutation
 * - Azul configuration mutation
 * - Status queries
 * - Activation mutation
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import {
  dealerOnboardingApi,
  type RegisterDealerRequest,
  type VerifyEmailRequest,
  type UpdateDocumentsRequest,
  type UpdateAzulIdsRequest,
  type DealerOnboarding,
  type DealerOnboardingStatus,
} from '@/services/dealerOnboardingService';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const dealerOnboardingKeys = {
  all: ['dealer-onboarding'] as const,
  detail: (id: string) => ['dealer-onboarding', id] as const,
  status: (id: string) => ['dealer-onboarding', id, 'status'] as const,
  byEmail: (email: string) => ['dealer-onboarding', 'email', email] as const,
  pending: ['dealer-onboarding', 'pending'] as const,
};

// ============================================================================
// QUERIES
// ============================================================================

/**
 * Get dealer onboarding status by ID
 */
export const useDealerOnboardingStatus = (dealerId: string | undefined) => {
  return useQuery({
    queryKey: dealerOnboardingKeys.status(dealerId || ''),
    queryFn: () => dealerOnboardingApi.getStatus(dealerId!),
    enabled: !!dealerId,
    staleTime: 1 * 60 * 1000, // 1 minute
    retry: 2,
  });
};

/**
 * Get full dealer onboarding details by ID
 */
export const useDealerOnboardingDetails = (dealerId: string | undefined) => {
  return useQuery({
    queryKey: dealerOnboardingKeys.detail(dealerId || ''),
    queryFn: () => dealerOnboardingApi.getById(dealerId!),
    enabled: !!dealerId,
    staleTime: 1 * 60 * 1000,
    retry: 2,
  });
};

/**
 * Get dealer onboarding by email
 */
export const useDealerOnboardingByEmail = (email: string | undefined) => {
  return useQuery({
    queryKey: dealerOnboardingKeys.byEmail(email || ''),
    queryFn: () => dealerOnboardingApi.getByEmail(email!),
    enabled: !!email,
    staleTime: 1 * 60 * 1000,
    retry: 1,
  });
};

/**
 * Get pending applications (admin only)
 */
export const usePendingOnboardingApplications = () => {
  return useQuery({
    queryKey: dealerOnboardingKeys.pending,
    queryFn: () => dealerOnboardingApi.getPendingApplications(),
    staleTime: 30 * 1000, // 30 seconds
    retry: 2,
  });
};

// ============================================================================
// MUTATIONS
// ============================================================================

/**
 * Step 1: Register a new dealer
 */
export const useRegisterDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: RegisterDealerRequest) => dealerOnboardingApi.register(data),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.setQueryData(dealerOnboardingKeys.byEmail(data.email), data);

      // Store dealer ID for onboarding flow
      localStorage.setItem('onboardingDealerId', data.id);
      localStorage.setItem('onboardingEmail', data.email);

      toast.success('¡Registro exitoso! Te enviamos un código de verificación por email.');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al registrar el dealer');
    },
  });
};

/**
 * Step 2: Verify email
 */
export const useVerifyDealerEmail = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: VerifyEmailRequest) => dealerOnboardingApi.verifyEmail(data),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.status(data.id) });
      toast.success('¡Email verificado correctamente!');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Código de verificación inválido');
    },
  });
};

/**
 * Resend verification email
 */
export const useResendVerificationEmail = () => {
  return useMutation({
    mutationFn: (email: string) => dealerOnboardingApi.resendVerificationEmail(email),
    onSuccess: () => {
      toast.success('Código de verificación reenviado');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al reenviar código');
    },
  });
};

/**
 * Step 3: Upload documents
 */
export const useUpdateDealerDocuments = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      dealerId,
      documents,
    }: {
      dealerId: string;
      documents: UpdateDocumentsRequest;
    }) => dealerOnboardingApi.updateDocuments(dealerId, documents),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.status(data.id) });
      toast.success('Documentos subidos correctamente');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al subir documentos');
    },
  });
};

/**
 * Step 4: Update Azul subscription IDs after payment
 *
 * IMPORTANT: This is called after the dealer PAYS OKLA.
 * The dealer is a CUSTOMER (payer), not a merchant.
 */
export const useUpdateAzulIds = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dealerId, data }: { dealerId: string; data: UpdateAzulIdsRequest }) =>
      dealerOnboardingApi.updateAzulIds(dealerId, data),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.status(data.id) });
      toast.success('¡Pago procesado exitosamente! Tu cuenta está siendo activada.');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al procesar el pago');
    },
  });
};

/**
 * Step 5: Activate dealer (after admin approval)
 */
export const useActivateDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dealerId: string) => dealerOnboardingApi.activate(dealerId),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.status(data.id) });

      // Clear onboarding localStorage
      localStorage.removeItem('onboardingDealerId');
      localStorage.removeItem('onboardingEmail');

      toast.success('¡Felicidades! Tu cuenta de dealer está activa.');
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al activar la cuenta');
    },
  });
};

// ============================================================================
// ADMIN MUTATIONS
// ============================================================================

/**
 * Approve dealer application (admin only)
 */
export const useApproveDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dealerId: string) => dealerOnboardingApi.approve(dealerId),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.pending });
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.status(data.id) });
      toast.success(`Dealer ${data.businessName} aprobado`);
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al aprobar dealer');
    },
  });
};

/**
 * Reject dealer application (admin only)
 */
export const useRejectDealer = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ dealerId, reason }: { dealerId: string; reason: string }) =>
      dealerOnboardingApi.reject(dealerId, reason),
    onSuccess: (data) => {
      queryClient.setQueryData(dealerOnboardingKeys.detail(data.id), data);
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.pending });
      queryClient.invalidateQueries({ queryKey: dealerOnboardingKeys.status(data.id) });
      toast.success(`Dealer ${data.businessName} rechazado`);
    },
    onError: (error: Error) => {
      toast.error(error.message || 'Error al rechazar dealer');
    },
  });
};

// ============================================================================
// HELPER HOOKS
// ============================================================================

/**
 * Hook to get current onboarding progress from localStorage
 */
export const useOnboardingProgress = () => {
  const dealerId = localStorage.getItem('onboardingDealerId');
  const email = localStorage.getItem('onboardingEmail');

  const { data: status, isLoading, error } = useDealerOnboardingStatus(dealerId || undefined);

  return {
    dealerId,
    email,
    status,
    isLoading,
    error,
    hasOnboardingInProgress: !!dealerId,
    clearProgress: () => {
      localStorage.removeItem('onboardingDealerId');
      localStorage.removeItem('onboardingEmail');
    },
  };
};

/**
 * Hook to check if user has completed onboarding
 */
export const useIsOnboardingComplete = (dealerId: string | undefined) => {
  const { data: status, isLoading } = useDealerOnboardingStatus(dealerId);

  return {
    isComplete: status?.isActive ?? false,
    isLoading,
    status: status?.status,
  };
};
