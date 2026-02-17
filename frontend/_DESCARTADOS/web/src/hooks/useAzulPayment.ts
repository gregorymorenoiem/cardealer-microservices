/**
 * Azul Payment Hooks
 *
 * React Query hooks for AzulPaymentService operations
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  azulPaymentApi,
  type ChargeRequest,
  type ChargeResponse,
  type RefundRequest,
  type SubscriptionRequest,
  type SubscriptionResponse,
  type TransactionDetails,
} from '@/services/azulPaymentService';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const azulPaymentKeys = {
  all: ['azul-payment'] as const,
  transactions: () => [...azulPaymentKeys.all, 'transactions'] as const,
  transaction: (id: string) => [...azulPaymentKeys.transactions(), id] as const,
  subscriptions: () => [...azulPaymentKeys.all, 'subscriptions'] as const,
  subscription: (id: string) => [...azulPaymentKeys.subscriptions(), id] as const,
  health: () => [...azulPaymentKeys.all, 'health'] as const,
};

// ============================================================================
// PAYMENT MUTATIONS
// ============================================================================

/**
 * Process a one-time charge
 */
export const useChargePayment = () => {
  const queryClient = useQueryClient();

  return useMutation<ChargeResponse, Error, ChargeRequest>({
    mutationFn: (data) => azulPaymentApi.charge(data),
    onSuccess: (data) => {
      // Invalidate transactions cache
      queryClient.invalidateQueries({ queryKey: azulPaymentKeys.transactions() });
      console.log('Payment successful:', data.transactionId);
    },
    onError: (error) => {
      console.error('Payment failed:', error.message);
    },
  });
};

/**
 * Process a refund
 */
export const useRefundPayment = () => {
  const queryClient = useQueryClient();

  return useMutation<ChargeResponse, Error, RefundRequest>({
    mutationFn: (data) => azulPaymentApi.refund(data),
    onSuccess: (data, variables) => {
      // Invalidate the specific transaction
      queryClient.invalidateQueries({
        queryKey: azulPaymentKeys.transaction(variables.transactionId),
      });
      queryClient.invalidateQueries({ queryKey: azulPaymentKeys.transactions() });
    },
  });
};

// ============================================================================
// SUBSCRIPTION MUTATIONS
// ============================================================================

/**
 * Create a subscription (for dealer monthly plans)
 */
export const useCreateSubscription = () => {
  const queryClient = useQueryClient();

  return useMutation<SubscriptionResponse, Error, SubscriptionRequest>({
    mutationFn: (data) => azulPaymentApi.createSubscription(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: azulPaymentKeys.subscriptions() });
      console.log('Subscription created:', data.subscriptionId);
    },
    onError: (error) => {
      console.error('Subscription creation failed:', error.message);
    },
  });
};

/**
 * Cancel a subscription
 */
export const useCancelSubscription = () => {
  const queryClient = useQueryClient();

  return useMutation<void, Error, { subscriptionId: string; reason?: string }>({
    mutationFn: ({ subscriptionId, reason }) =>
      azulPaymentApi.cancelSubscription(subscriptionId, reason),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: azulPaymentKeys.subscription(variables.subscriptionId),
      });
      queryClient.invalidateQueries({ queryKey: azulPaymentKeys.subscriptions() });
    },
  });
};

// ============================================================================
// QUERIES
// ============================================================================

/**
 * Get transaction details
 */
export const useTransaction = (transactionId: string | undefined) => {
  return useQuery<TransactionDetails, Error>({
    queryKey: azulPaymentKeys.transaction(transactionId || ''),
    queryFn: () => azulPaymentApi.getTransaction(transactionId!),
    enabled: !!transactionId,
    staleTime: 30 * 1000, // 30 seconds
  });
};

/**
 * Get subscription details
 */
export const useSubscription = (subscriptionId: string | undefined) => {
  return useQuery<SubscriptionResponse, Error>({
    queryKey: azulPaymentKeys.subscription(subscriptionId || ''),
    queryFn: () => azulPaymentApi.getSubscription(subscriptionId!),
    enabled: !!subscriptionId,
    staleTime: 60 * 1000, // 1 minute
  });
};

/**
 * Check Azul Payment Service health
 */
export const useAzulHealthCheck = () => {
  return useQuery({
    queryKey: azulPaymentKeys.health(),
    queryFn: () => azulPaymentApi.healthCheck(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 1,
  });
};

// ============================================================================
// COMBINED HOOKS
// ============================================================================

/**
 * Parameters for creating a dealer subscription
 *
 * IMPORTANT: Dealers PAY OKLA for advertising services.
 * This creates a subscription where the dealer is the CUSTOMER (payer).
 */
interface DealerSubscriptionParams {
  dealerId: string;
  planId: string;
  planName: string;
  amount: number;
  isEarlyBird: boolean;
  // Card details (dealer pays OKLA)
  cardNumber: string;
  cardExpiryMonth: string;
  cardExpiryYear: string;
  cardCVV: string;
  cardholderName: string;
  customerEmail?: string;
  customerPhone?: string;
}

/**
 * Combined hook for dealer subscription flow
 *
 * Creates a subscription in AzulPaymentService where:
 * - OKLA is the MERCHANT (receives payment)
 * - Dealer is the CUSTOMER (makes payment)
 *
 * Flow:
 * 1. Collects card details from dealer
 * 2. Creates subscription via AzulPaymentService
 * 3. Returns subscription ID for updating dealer onboarding
 */
export const useDealerSubscription = () => {
  const createSubscription = useCreateSubscription();

  return useMutation<SubscriptionResponse, Error, DealerSubscriptionParams>({
    mutationFn: async (params) => {
      const subscriptionRequest: SubscriptionRequest = {
        userId: params.dealerId,
        amount: params.amount, // Already calculated with Early Bird if applicable
        currency: 'DOP',
        frequency: 'Monthly' as any,
        startDate: params.isEarlyBird
          ? new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString() // 90 days trial
          : new Date().toISOString(),
        planName: params.planName,
        customerEmail: params.customerEmail,
        customerPhone: params.customerPhone,
        invoiceReference: `DEALER-${params.dealerId}-${Date.now()}`,
        paymentMethod: 'CreditCard' as any,
        // Card details
        cardNumber: params.cardNumber,
        cardExpiryMonth: params.cardExpiryMonth,
        cardExpiryYear: params.cardExpiryYear,
        cardCVV: params.cardCVV,
        cardholderName: params.cardholderName,
      };

      return azulPaymentApi.createSubscription(subscriptionRequest);
    },
    onSuccess: (data) => {
      console.log('Dealer subscription created:', data.subscriptionId);
    },
    onError: (error) => {
      console.error('Dealer subscription failed:', error.message);
    },
  });
};
