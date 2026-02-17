/**
 * Dealer Billing Hooks
 *
 * React Query hooks for dealer billing operations
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  getBillingDashboard,
  getSubscription,
  getInvoices,
  getInvoiceById,
  downloadInvoicePdf,
  getPaymentMethods,
  addPaymentMethod,
  removePaymentMethod,
  setDefaultPaymentMethod,
  getUsageMetrics,
  getBillingStats,
  getPlans,
  changePlan,
  cancelSubscription,
  reactivateSubscription,
} from '@/services/dealer-billing';

// ============================================================================
// Query Keys
// ============================================================================

export const dealerBillingKeys = {
  all: ['dealer-billing'] as const,
  dashboard: (dealerId: string) => [...dealerBillingKeys.all, 'dashboard', dealerId] as const,
  subscription: (dealerId: string) => [...dealerBillingKeys.all, 'subscription', dealerId] as const,
  invoices: () => [...dealerBillingKeys.all, 'invoices'] as const,
  invoiceList: (dealerId: string) => [...dealerBillingKeys.invoices(), dealerId] as const,
  invoice: (invoiceId: string) => [...dealerBillingKeys.invoices(), 'detail', invoiceId] as const,
  paymentMethods: (dealerId: string) =>
    [...dealerBillingKeys.all, 'payment-methods', dealerId] as const,
  usage: (dealerId: string) => [...dealerBillingKeys.all, 'usage', dealerId] as const,
  stats: (dealerId: string) => [...dealerBillingKeys.all, 'stats', dealerId] as const,
  plans: () => [...dealerBillingKeys.all, 'plans'] as const,
};

// ============================================================================
// Query Hooks
// ============================================================================

/**
 * Get complete billing dashboard
 */
export function useBillingDashboard(dealerId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.dashboard(dealerId),
    queryFn: () => getBillingDashboard(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get current subscription
 */
export function useSubscription(dealerId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.subscription(dealerId),
    queryFn: () => getSubscription(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get all invoices
 */
export function useInvoices(dealerId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.invoiceList(dealerId),
    queryFn: () => getInvoices(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get invoice by ID
 */
export function useInvoice(invoiceId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.invoice(invoiceId),
    queryFn: () => getInvoiceById(invoiceId),
    enabled: !!invoiceId,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
}

/**
 * Get payment methods
 */
export function usePaymentMethods(dealerId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.paymentMethods(dealerId),
    queryFn: () => getPaymentMethods(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get usage metrics
 */
export function useUsageMetrics(dealerId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.usage(dealerId),
    queryFn: () => getUsageMetrics(dealerId),
    enabled: !!dealerId,
    staleTime: 2 * 60 * 1000, // 2 minutes (usage changes more frequently)
  });
}

/**
 * Get billing stats
 */
export function useBillingStats(dealerId: string) {
  return useQuery({
    queryKey: dealerBillingKeys.stats(dealerId),
    queryFn: () => getBillingStats(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Get available plans
 */
export function usePlans() {
  return useQuery({
    queryKey: dealerBillingKeys.plans(),
    queryFn: getPlans,
    staleTime: 30 * 60 * 1000, // 30 minutes (plans rarely change)
  });
}

// ============================================================================
// Mutation Hooks
// ============================================================================

/**
 * Download invoice PDF
 */
export function useDownloadInvoice() {
  return useMutation({
    mutationFn: async (invoiceId: string) => {
      const blob = await downloadInvoicePdf(invoiceId);
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `factura-${invoiceId}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    },
  });
}

/**
 * Add payment method
 */
export function useAddPaymentMethod(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (token: string) => addPaymentMethod(dealerId, token),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.paymentMethods(dealerId),
      });
    },
  });
}

/**
 * Remove payment method
 */
export function useRemovePaymentMethod(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (paymentMethodId: string) => removePaymentMethod(dealerId, paymentMethodId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.paymentMethods(dealerId),
      });
    },
  });
}

/**
 * Set default payment method
 */
export function useSetDefaultPaymentMethod(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (paymentMethodId: string) => setDefaultPaymentMethod(dealerId, paymentMethodId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.paymentMethods(dealerId),
      });
    },
  });
}

/**
 * Change subscription plan
 */
export function useChangePlan(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (planId: string) => changePlan(dealerId, planId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.subscription(dealerId),
      });
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.dashboard(dealerId),
      });
    },
  });
}

/**
 * Cancel subscription
 */
export function useCancelSubscription(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (immediately: boolean = false) => cancelSubscription(dealerId, immediately),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.subscription(dealerId),
      });
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.dashboard(dealerId),
      });
    },
  });
}

/**
 * Reactivate subscription
 */
export function useReactivateSubscription(dealerId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => reactivateSubscription(dealerId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.subscription(dealerId),
      });
      queryClient.invalidateQueries({
        queryKey: dealerBillingKeys.dashboard(dealerId),
      });
    },
  });
}

// ============================================================================
// Derived Hooks
// ============================================================================

/**
 * Get pending invoices
 */
export function usePendingInvoices(dealerId: string) {
  const { data: invoices, ...rest } = useInvoices(dealerId);

  const pendingInvoices = invoices?.filter(
    inv => inv.status === 'Pending' || inv.status === 'Overdue'
  );

  return {
    data: pendingInvoices,
    ...rest,
  };
}

/**
 * Get paid invoices
 */
export function usePaidInvoices(dealerId: string) {
  const { data: invoices, ...rest } = useInvoices(dealerId);

  const paidInvoices = invoices?.filter(inv => inv.status === 'Paid');

  return {
    data: paidInvoices,
    ...rest,
  };
}

/**
 * Get default payment method
 */
export function useDefaultPaymentMethod(dealerId: string) {
  const { data: methods, ...rest } = usePaymentMethods(dealerId);

  const defaultMethod = methods?.find(m => m.isDefault) || methods?.[0];

  return {
    data: defaultMethod,
    ...rest,
  };
}
