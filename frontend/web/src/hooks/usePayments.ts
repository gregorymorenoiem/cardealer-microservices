/**
 * Payment Hooks - React Query
 *
 * Hooks para pagos, suscripciones y métodos de pago
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import paymentService, {
  PaymentMethod,
  ChargeRequest,
  ChargeResponse,
  Transaction,
  Subscription,
  SubscriptionPlanInfo,
  CreateSubscriptionRequest,
  PaymentStatus,
  RefundRequest,
  SubscriptionPlan,
  PaymentGateway,
} from '@/services/paymentService';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const paymentKeys = {
  all: ['payments'] as const,
  methods: () => [...paymentKeys.all, 'methods'] as const,
  transactions: (filters?: { page?: number; status?: PaymentStatus }) =>
    [...paymentKeys.all, 'transactions', filters] as const,
  transaction: (id: string) => [...paymentKeys.all, 'transaction', id] as const,
  subscription: () => [...paymentKeys.all, 'subscription'] as const,
  subscriptionPlans: () => [...paymentKeys.all, 'plans'] as const,
  refundsPending: () => [...paymentKeys.all, 'refunds', 'pending'] as const,
};

// ============================================================================
// PAYMENT METHODS
// ============================================================================

/**
 * Get saved payment methods
 */
export function usePaymentMethods() {
  return useQuery<PaymentMethod[], Error>({
    queryKey: paymentKeys.methods(),
    queryFn: paymentService.getPaymentMethods,
  });
}

/**
 * Add new payment method
 */
export function useAddPaymentMethod() {
  const queryClient = useQueryClient();

  return useMutation<
    PaymentMethod,
    Error,
    {
      gateway: PaymentGateway;
      cardNumber: string;
      cardExpiry: string;
      cardCvv: string;
      cardholderName: string;
      setAsDefault?: boolean;
    }
  >({
    mutationFn: (data) =>
      paymentService.addPaymentMethod(
        data.gateway,
        data.cardNumber,
        data.cardExpiry,
        data.cardCvv,
        data.cardholderName,
        data.setAsDefault
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.methods() });
    },
  });
}

/**
 * Delete payment method
 */
export function useDeletePaymentMethod() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: paymentService.deletePaymentMethod,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.methods() });
    },
  });
}

/**
 * Set default payment method
 */
export function useSetDefaultPaymentMethod() {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: paymentService.setDefaultPaymentMethod,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.methods() });
    },
  });
}

// ============================================================================
// CHARGES & TRANSACTIONS
// ============================================================================

/**
 * Process a charge
 */
export function useCharge() {
  const queryClient = useQueryClient();

  return useMutation<ChargeResponse, Error, ChargeRequest>({
    mutationFn: paymentService.charge,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.transactions() });
    },
  });
}

/**
 * Get transactions list
 */
export function useTransactions(page = 1, pageSize = 20, status?: PaymentStatus) {
  return useQuery<{ data: Transaction[]; total: number }, Error>({
    queryKey: paymentKeys.transactions({ page, status }),
    queryFn: () => paymentService.getTransactions(page, pageSize, status),
  });
}

/**
 * Get single transaction
 */
export function useTransaction(id: string) {
  return useQuery<Transaction, Error>({
    queryKey: paymentKeys.transaction(id),
    queryFn: () => paymentService.getTransaction(id),
    enabled: !!id,
  });
}

// ============================================================================
// REFUNDS
// ============================================================================

/**
 * Get pending refunds (Admin)
 */
export function usePendingRefunds() {
  return useQuery<Transaction[], Error>({
    queryKey: paymentKeys.refundsPending(),
    queryFn: paymentService.getPendingRefunds,
  });
}

/**
 * Request a refund
 */
export function useRequestRefund() {
  const queryClient = useQueryClient();

  return useMutation<Transaction, Error, RefundRequest>({
    mutationFn: paymentService.requestRefund,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.transactions() });
      queryClient.invalidateQueries({ queryKey: paymentKeys.refundsPending() });
    },
  });
}

/**
 * Approve a refund (Admin)
 */
export function useApproveRefund() {
  const queryClient = useQueryClient();

  return useMutation<Transaction, Error, { transactionId: string; adminNote?: string }>({
    mutationFn: ({ transactionId, adminNote }) =>
      paymentService.approveRefund(transactionId, adminNote),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.refundsPending() });
      queryClient.invalidateQueries({ queryKey: paymentKeys.transactions() });
    },
  });
}

/**
 * Reject a refund (Admin)
 */
export function useRejectRefund() {
  const queryClient = useQueryClient();

  return useMutation<Transaction, Error, { transactionId: string; reason: string }>({
    mutationFn: ({ transactionId, reason }) => paymentService.rejectRefund(transactionId, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.refundsPending() });
    },
  });
}

// ============================================================================
// SUBSCRIPTIONS
// ============================================================================

/**
 * Get subscription plans
 */
export function useSubscriptionPlans() {
  return useQuery<SubscriptionPlanInfo[], Error>({
    queryKey: paymentKeys.subscriptionPlans(),
    queryFn: paymentService.getSubscriptionPlans,
    staleTime: 1000 * 60 * 60, // 1 hour
    initialData: paymentService.getSubscriptionPlanDetails,
  });
}

/**
 * Get current subscription
 */
export function useCurrentSubscription() {
  return useQuery<Subscription | null, Error>({
    queryKey: paymentKeys.subscription(),
    queryFn: paymentService.getCurrentSubscription,
  });
}

/**
 * Create subscription
 */
export function useCreateSubscription() {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, CreateSubscriptionRequest>({
    mutationFn: paymentService.createSubscription,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.subscription() });
      queryClient.invalidateQueries({ queryKey: paymentKeys.methods() });
    },
  });
}

/**
 * Cancel subscription
 */
export function useCancelSubscription() {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, { reason: string; cancelImmediately?: boolean }>({
    mutationFn: ({ reason, cancelImmediately }) =>
      paymentService.cancelSubscription(reason, cancelImmediately),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.subscription() });
    },
  });
}

/**
 * Pause subscription
 */
export function usePauseSubscription() {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, number>({
    mutationFn: paymentService.pauseSubscription,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.subscription() });
    },
  });
}

/**
 * Resume subscription
 */
export function useResumeSubscription() {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, void>({
    mutationFn: paymentService.resumeSubscription,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.subscription() });
    },
  });
}

/**
 * Change subscription plan
 */
export function useChangePlan() {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, SubscriptionPlan>({
    mutationFn: paymentService.changePlan,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.subscription() });
    },
  });
}

// ============================================================================
// HELPERS
// ============================================================================

/**
 * Get Early Bird info
 */
export function useEarlyBirdInfo() {
  const isActive = paymentService.isEarlyBirdActive();
  const daysRemaining = paymentService.getEarlyBirdDaysRemaining();
  const benefits = paymentService.getEarlyBirdBenefits();

  return {
    isActive,
    daysRemaining,
    benefits,
    calculatePrice: paymentService.calculateEarlyBirdPrice,
  };
}

/**
 * Get listing price info
 */
export function useListingPrice() {
  return paymentService.calculateListingTotal();
}

/**
 * Get gateway info
 */
export function useGatewayInfo(gateway: PaymentGateway) {
  return paymentService.getGatewayInfo(gateway);
}

/**
 * Select optimal gateway based on context
 */
export function useSelectGateway(context: {
  userCountry: string;
  isSubscription: boolean;
  merchantVolume?: number;
}) {
  return paymentService.selectOptimalGateway(context);
}
