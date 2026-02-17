import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  plansApi,
  subscriptionApi,
  invoicesApi,
  paymentsApi,
  paymentMethodsApi,
  usageApi,
  type CreateSubscriptionRequest,
  type UpgradeSubscriptionRequest,
  type AddPaymentMethodRequest,
} from '@/services/billingService';
import type { 
  PlanConfig, 
  Subscription, 
  Invoice, 
  Payment, 
  PaymentMethodInfo,
  UsageMetrics,
  BillingStats,
} from '@/types/billing';

// ============================================================================
// Query Keys
// ============================================================================

export const billingKeys = {
  all: ['billing'] as const,
  plans: () => [...billingKeys.all, 'plans'] as const,
  plan: (id: string) => [...billingKeys.plans(), id] as const,
  subscription: () => [...billingKeys.all, 'subscription'] as const,
  subscriptionByDealer: (dealerId: string) => [...billingKeys.subscription(), dealerId] as const,
  invoices: () => [...billingKeys.all, 'invoices'] as const,
  invoice: (id: string) => [...billingKeys.invoices(), id] as const,
  payments: () => [...billingKeys.all, 'payments'] as const,
  payment: (id: string) => [...billingKeys.payments(), id] as const,
  paymentMethods: () => [...billingKeys.all, 'paymentMethods'] as const,
  usage: () => [...billingKeys.all, 'usage'] as const,
  stats: () => [...billingKeys.all, 'stats'] as const,
};

// ============================================================================
// Plans Hooks
// ============================================================================

/**
 * Fetch all available subscription plans
 */
export function usePlans() {
  return useQuery<PlanConfig[], Error>({
    queryKey: billingKeys.plans(),
    queryFn: plansApi.getAll,
    staleTime: 1000 * 60 * 60, // 1 hour - plans don't change often
  });
}

/**
 * Fetch a specific plan by ID
 */
export function usePlan(planId: string) {
  return useQuery<PlanConfig | null, Error>({
    queryKey: billingKeys.plan(planId),
    queryFn: () => plansApi.getById(planId),
    enabled: !!planId,
  });
}

/**
 * Compare plans - same as usePlans but for comparison view
 */
export function useComparePlans() {
  return useQuery<PlanConfig[], Error>({
    queryKey: [...billingKeys.plans(), 'compare'],
    queryFn: plansApi.compare,
    staleTime: 1000 * 60 * 60,
  });
}

// ============================================================================
// Subscription Hooks
// ============================================================================

/**
 * Fetch current subscription for a dealer
 */
export function useSubscription(dealerId: string) {
  return useQuery<Subscription | null, Error>({
    queryKey: billingKeys.subscriptionByDealer(dealerId),
    queryFn: () => subscriptionApi.getCurrent(dealerId),
    enabled: !!dealerId,
  });
}

/**
 * Create a new subscription
 */
export function useCreateSubscription(options?: {
  onSuccess?: (data: Subscription) => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, CreateSubscriptionRequest>({
    mutationFn: subscriptionApi.create,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.subscription() });
      queryClient.invalidateQueries({ queryKey: billingKeys.usage() });
      options?.onSuccess?.(data);
    },
    onError: options?.onError,
  });
}

/**
 * Upgrade subscription to a new plan
 */
export function useUpgradeSubscription(options?: {
  onSuccess?: (data: Subscription) => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, UpgradeSubscriptionRequest>({
    mutationFn: subscriptionApi.upgrade,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.subscription() });
      queryClient.invalidateQueries({ queryKey: billingKeys.usage() });
      options?.onSuccess?.(data);
    },
    onError: options?.onError,
  });
}

/**
 * Cancel subscription
 */
export function useCancelSubscription(options?: {
  onSuccess?: () => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string | undefined>({
    mutationFn: subscriptionApi.cancel,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: billingKeys.subscription() });
      options?.onSuccess?.();
    },
    onError: options?.onError,
  });
}

/**
 * Reactivate cancelled subscription
 */
export function useReactivateSubscription(options?: {
  onSuccess?: (data: Subscription) => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<Subscription, Error, void>({
    mutationFn: subscriptionApi.reactivate,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.subscription() });
      options?.onSuccess?.(data);
    },
    onError: options?.onError,
  });
}

// ============================================================================
// Invoice Hooks
// ============================================================================

/**
 * Fetch all invoices
 */
export function useInvoices() {
  return useQuery<Invoice[], Error>({
    queryKey: billingKeys.invoices(),
    queryFn: invoicesApi.getAll,
  });
}

/**
 * Fetch a specific invoice
 */
export function useInvoice(invoiceId: string) {
  return useQuery<Invoice | null, Error>({
    queryKey: billingKeys.invoice(invoiceId),
    queryFn: () => invoicesApi.getById(invoiceId),
    enabled: !!invoiceId,
  });
}

/**
 * Download invoice PDF
 */
export function useDownloadInvoice() {
  return useMutation<Blob, Error, string>({
    mutationFn: invoicesApi.downloadPdf,
    onSuccess: (blob, invoiceId) => {
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `invoice-${invoiceId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    },
  });
}

/**
 * Pay an invoice
 */
export function usePayInvoice(options?: {
  onSuccess?: (data: Payment) => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<Payment, Error, { invoiceId: string; paymentMethodId?: string }>({
    mutationFn: ({ invoiceId, paymentMethodId }) => invoicesApi.pay(invoiceId, paymentMethodId),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.invoices() });
      queryClient.invalidateQueries({ queryKey: billingKeys.payments() });
      queryClient.invalidateQueries({ queryKey: billingKeys.subscription() });
      options?.onSuccess?.(data);
    },
    onError: options?.onError,
  });
}

// ============================================================================
// Payment Hooks
// ============================================================================

/**
 * Fetch all payments
 */
export function usePayments() {
  return useQuery<Payment[], Error>({
    queryKey: billingKeys.payments(),
    queryFn: paymentsApi.getAll,
  });
}

/**
 * Fetch a specific payment
 */
export function usePayment(paymentId: string) {
  return useQuery<Payment | null, Error>({
    queryKey: billingKeys.payment(paymentId),
    queryFn: () => paymentsApi.getById(paymentId),
    enabled: !!paymentId,
  });
}

/**
 * Request refund for a payment
 */
export function useRefundPayment(options?: {
  onSuccess?: (data: Payment) => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<Payment, Error, { paymentId: string; amount?: number; reason?: string }>({
    mutationFn: ({ paymentId, amount, reason }) => paymentsApi.refund(paymentId, amount, reason),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.payments() });
      options?.onSuccess?.(data);
    },
    onError: options?.onError,
  });
}

// ============================================================================
// Payment Methods Hooks
// ============================================================================

/**
 * Fetch all payment methods
 */
export function usePaymentMethods() {
  return useQuery<PaymentMethodInfo[], Error>({
    queryKey: billingKeys.paymentMethods(),
    queryFn: paymentMethodsApi.getAll,
  });
}

/**
 * Add a new payment method
 */
export function useAddPaymentMethod(options?: {
  onSuccess?: (data: PaymentMethodInfo) => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<PaymentMethodInfo, Error, AddPaymentMethodRequest>({
    mutationFn: paymentMethodsApi.add,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: billingKeys.paymentMethods() });
      options?.onSuccess?.(data);
    },
    onError: options?.onError,
  });
}

/**
 * Set payment method as default
 */
export function useSetDefaultPaymentMethod(options?: {
  onSuccess?: () => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: paymentMethodsApi.setDefault,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: billingKeys.paymentMethods() });
      options?.onSuccess?.();
    },
    onError: options?.onError,
  });
}

/**
 * Remove a payment method
 */
export function useRemovePaymentMethod(options?: {
  onSuccess?: () => void;
  onError?: (error: Error) => void;
}) {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: paymentMethodsApi.remove,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: billingKeys.paymentMethods() });
      options?.onSuccess?.();
    },
    onError: options?.onError,
  });
}

// ============================================================================
// Usage & Stats Hooks
// ============================================================================

/**
 * Fetch current usage metrics
 */
export function useUsageMetrics() {
  return useQuery<UsageMetrics, Error>({
    queryKey: billingKeys.usage(),
    queryFn: usageApi.getMetrics,
    refetchInterval: 1000 * 60 * 5, // Refetch every 5 minutes
  });
}

/**
 * Fetch billing stats
 */
export function useBillingStats() {
  return useQuery<BillingStats, Error>({
    queryKey: billingKeys.stats(),
    queryFn: usageApi.getStats,
  });
}

// ============================================================================
// Composite Hook - All Billing Data
// ============================================================================

/**
 * All-in-one hook for billing dashboard
 */
export function useBillingDashboard(dealerId: string) {
  const subscription = useSubscription(dealerId);
  const invoices = useInvoices();
  const payments = usePayments();
  const paymentMethods = usePaymentMethods();
  const usage = useUsageMetrics();
  const stats = useBillingStats();

  return {
    subscription: subscription.data,
    invoices: invoices.data || [],
    payments: payments.data || [],
    paymentMethods: paymentMethods.data || [],
    usage: usage.data,
    stats: stats.data,
    isLoading:
      subscription.isLoading ||
      invoices.isLoading ||
      payments.isLoading ||
      paymentMethods.isLoading ||
      usage.isLoading ||
      stats.isLoading,
    error:
      subscription.error ||
      invoices.error ||
      payments.error ||
      paymentMethods.error ||
      usage.error ||
      stats.error,
  };
}

// ============================================================================
// Default Export
// ============================================================================

export const useBilling = {
  // Plans
  usePlans,
  usePlan,
  useComparePlans,
  // Subscription
  useSubscription,
  useCreateSubscription,
  useUpgradeSubscription,
  useCancelSubscription,
  useReactivateSubscription,
  // Invoices
  useInvoices,
  useInvoice,
  useDownloadInvoice,
  usePayInvoice,
  // Payments
  usePayments,
  usePayment,
  useRefundPayment,
  // Payment Methods
  usePaymentMethods,
  useAddPaymentMethod,
  useSetDefaultPaymentMethod,
  useRemovePaymentMethod,
  // Usage & Stats
  useUsageMetrics,
  useBillingStats,
  // Dashboard
  useBillingDashboard,
  // Keys
  keys: billingKeys,
};

export default useBilling;
