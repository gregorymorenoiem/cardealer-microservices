/**
 * Billing Service
 *
 * Frontend service for billing and subscription operations.
 * Uses real backend data from DealerBillingController.
 */

import { api } from './api';
import { dealerBillingApi } from './dealerBillingService';
import {
  plans,
  subscriptionsByDealer,
  mockInvoices,
  mockPayments,
  mockPaymentMethods,
  mockUsageMetrics,
  mockBillingStats,
  getSubscriptionByDealerId,
  getPlanById,
} from '@/mocks/billingData';
import type { PlanConfig, SubscriptionPlan } from '@/types/billing';
import type {
  Subscription,
  Invoice,
  Payment,
  PaymentMethodInfo,
  UsageMetrics,
  BillingStats,
} from '@/types/billing';

// Set to false to use real backend data
const USE_MOCK_DATA = import.meta.env.VITE_USE_MOCK_AUTH === 'true';

// ============================================================================
// TYPES
// ============================================================================

export interface CreateSubscriptionRequest {
  plan: string;
  cycle: 'monthly' | 'quarterly' | 'yearly';
  paymentMethodId?: string;
}

export interface UpgradeSubscriptionRequest {
  plan: string;
  immediate?: boolean;
}

export interface AddPaymentMethodRequest {
  type: 'card';
  token: string;
  setAsDefault?: boolean;
}

// ============================================================================
// PLANS API
// ============================================================================

export const plansApi = {
  // Get all available plans
  getAll: async (): Promise<PlanConfig[]> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return plans;
    }
    try {
      return await dealerBillingApi.getPlans();
    } catch (error) {
      console.error('Error fetching plans:', error);
      return plans; // Fallback to mock
    }
  },

  // Get a specific plan
  getById: async (planId: string): Promise<PlanConfig | null> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 100));
      return getPlanById(planId) || null;
    }
    try {
      const allPlans = await dealerBillingApi.getPlans();
      return allPlans.find((p) => p.id === planId) || null;
    } catch (error) {
      console.error('Error fetching plan:', error);
      return getPlanById(planId) || null;
    }
  },

  // Compare plans (features matrix)
  compare: async (): Promise<PlanConfig[]> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return plans;
    }
    try {
      return await dealerBillingApi.getPlans();
    } catch (error) {
      console.error('Error fetching plans for compare:', error);
      return plans;
    }
  },
};

// ============================================================================
// SUBSCRIPTION API
// ============================================================================

export const subscriptionApi = {
  // Get current subscription
  getCurrent: async (dealerId: string): Promise<Subscription | null> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return getSubscriptionByDealerId(dealerId) || null;
    }
    try {
      return await dealerBillingApi.getSubscription(dealerId);
    } catch (error) {
      console.error('Error fetching subscription:', error);
      return getSubscriptionByDealerId(dealerId) || null;
    }
  },

  // Create a new subscription
  create: async (data: CreateSubscriptionRequest): Promise<Subscription> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 500));
      const plan = getPlanById(data.plan);
      if (!plan) throw new Error('Plan not found');

      const price =
        data.cycle === 'monthly'
          ? plan.prices.monthly
          : data.cycle === 'quarterly'
            ? plan.prices.quarterly
            : plan.prices.yearly;

      return {
        id: `sub_${Date.now()}`,
        dealerId: 'current_dealer',
        plan: data.plan as SubscriptionPlan,
        status: 'active',
        cycle: data.cycle,
        pricePerCycle: price,
        currency: 'USD',
        startDate: new Date().toISOString(),
        nextBillingDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
        maxUsers: typeof plan.features.users === 'number' ? plan.features.users : 999,
        maxVehicles: typeof plan.features.listings === 'number' ? plan.features.listings : 9999,
        features: plan.features,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
    }
    const response = await api.post<Subscription>('/billing/subscription', data);
    return response.data;
  },

  // Upgrade subscription
  upgrade: async (data: UpgradeSubscriptionRequest): Promise<Subscription> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 500));
      const plan = getPlanById(data.plan);
      if (!plan) throw new Error('Plan not found');

      return {
        ...subscriptionsByDealer['dealer-pro-001'],
        plan: data.plan as SubscriptionPlan,
        features: plan.features,
        updatedAt: new Date().toISOString(),
      };
    }
    const response = await api.post<Subscription>('/billing/subscription/upgrade', data);
    return response.data;
  },

  // Cancel subscription
  cancel: async (reason?: string): Promise<void> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 400));
      console.log('Subscription cancelled:', reason);
      return;
    }
    await api.post('/billing/subscription/cancel', { reason });
  },

  // Reactivate subscription
  reactivate: async (): Promise<Subscription> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 400));
      return {
        ...subscriptionsByDealer['dealer-pro-001'],
        status: 'active',
        updatedAt: new Date().toISOString(),
      };
    }
    const response = await api.post<Subscription>('/billing/subscription/reactivate');
    return response.data;
  },
};

// ============================================================================
// INVOICES API
// ============================================================================

// Get dealer ID from localStorage or context
const getDealerId = (): string => {
  try {
    // Try multiple storage keys for the token
    const token = localStorage.getItem('accessToken') || localStorage.getItem('okla_auth_token');
    if (token) {
      const decoded = JSON.parse(atob(token.split('.')[1]));
      return decoded.dealerId || decoded.dealer_id || decoded.sub || '';
    }
  } catch (e) {
    console.warn('Could not get dealer ID from token');
  }
  return '';
};

export const invoicesApi = {
  // Get all invoices
  getAll: async (): Promise<Invoice[]> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 300));
      return mockInvoices;
    }
    try {
      const dealerId = getDealerId();
      return await dealerBillingApi.getInvoices(dealerId);
    } catch (error) {
      console.error('Error fetching invoices:', error);
      return mockInvoices;
    }
  },

  // Get invoice by ID
  getById: async (invoiceId: string): Promise<Invoice | null> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return mockInvoices.find((inv) => inv.id === invoiceId) || null;
    }
    try {
      const dealerId = getDealerId();
      const invoices = await dealerBillingApi.getInvoices(dealerId);
      return invoices.find((inv) => inv.id === invoiceId) || null;
    } catch (error) {
      console.error('Error fetching invoice:', error);
      return mockInvoices.find((inv) => inv.id === invoiceId) || null;
    }
  },

  // Download invoice PDF
  downloadPdf: async (invoiceId: string): Promise<Blob> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 500));
      // Return a mock blob
      return new Blob(['Mock PDF content'], { type: 'application/pdf' });
    }
    const response = await api.get<Blob>(`/billing/invoices/${invoiceId}/pdf`, {
      responseType: 'blob',
    });
    return response.data;
  },

  // Pay an invoice
  pay: async (invoiceId: string, paymentMethodId?: string): Promise<Payment> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 600));
      const invoice = mockInvoices.find((inv) => inv.id === invoiceId);
      if (!invoice) throw new Error('Invoice not found');

      return {
        id: `pay_${Date.now()}`,
        dealerId: invoice.dealerId,
        subscriptionId: invoice.subscriptionId,
        invoiceId: invoiceId,
        amount: invoice.totalAmount,
        currency: invoice.currency,
        status: 'succeeded',
        method: 'credit_card',
        description: `Payment for ${invoice.invoiceNumber}`,
        receiptUrl: `/receipts/pay_${Date.now()}.pdf`,
        refundedAmount: 0,
        createdAt: new Date().toISOString(),
        processedAt: new Date().toISOString(),
        cardLast4: '4242',
        cardBrand: 'Visa',
      };
    }
    const response = await api.post<Payment>(`/billing/invoices/${invoiceId}/pay`, {
      paymentMethodId,
    });
    return response.data;
  },
};

// ============================================================================
// PAYMENTS API
// ============================================================================

export const paymentsApi = {
  // Get all payments
  getAll: async (): Promise<Payment[]> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 300));
      return mockPayments;
    }
    try {
      const dealerId = getDealerId();
      return await dealerBillingApi.getPayments(dealerId);
    } catch (error) {
      console.error('Error fetching payments:', error);
      return mockPayments;
    }
  },

  // Get payment by ID
  getById: async (paymentId: string): Promise<Payment | null> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return mockPayments.find((p) => p.id === paymentId) || null;
    }
    try {
      const dealerId = getDealerId();
      const payments = await dealerBillingApi.getPayments(dealerId);
      return payments.find((p) => p.id === paymentId) || null;
    } catch (error) {
      console.error('Error fetching payment:', error);
      return mockPayments.find((p) => p.id === paymentId) || null;
    }
  },

  // Request refund
  refund: async (paymentId: string, amount?: number, reason?: string): Promise<Payment> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 500));
      const payment = mockPayments.find((p) => p.id === paymentId);
      if (!payment) throw new Error('Payment not found');

      return {
        ...payment,
        status: 'refunded',
        refundedAmount: amount || payment.amount,
      };
    }
    const response = await api.post<Payment>(`/billing/payments/${paymentId}/refund`, {
      amount,
      reason,
    });
    return response.data;
  },
};

// ============================================================================
// PAYMENT METHODS API
// ============================================================================

export const paymentMethodsApi = {
  // Get all payment methods
  getAll: async (): Promise<PaymentMethodInfo[]> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return mockPaymentMethods;
    }
    try {
      const dealerId = getDealerId();
      return await dealerBillingApi.getPaymentMethods(dealerId);
    } catch (error) {
      console.error('Error fetching payment methods:', error);
      return mockPaymentMethods;
    }
  },

  // Add a new payment method
  add: async (data: AddPaymentMethodRequest): Promise<PaymentMethodInfo> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 500));
      const newMethod: PaymentMethodInfo = {
        id: `pm_${Date.now()}`,
        type: 'card',
        isDefault: data.setAsDefault || false,
        card: {
          brand: 'Visa',
          last4: '4242',
          expMonth: 12,
          expYear: 2028,
        },
        createdAt: new Date().toISOString(),
      };
      mockPaymentMethods.push(newMethod);
      return newMethod;
    }
    const response = await api.post<PaymentMethodInfo>('/billing/payment-methods', data);
    return response.data;
  },

  // Set as default
  setDefault: async (paymentMethodId: string): Promise<void> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 300));
      mockPaymentMethods.forEach((pm) => {
        pm.isDefault = pm.id === paymentMethodId;
      });
      return;
    }
    await api.post(`/billing/payment-methods/${paymentMethodId}/default`);
  },

  // Remove a payment method
  remove: async (paymentMethodId: string): Promise<void> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 300));
      const index = mockPaymentMethods.findIndex((pm) => pm.id === paymentMethodId);
      if (index !== -1) {
        mockPaymentMethods.splice(index, 1);
      }
      return;
    }
    await api.delete(`/billing/payment-methods/${paymentMethodId}`);
  },
};

// ============================================================================
// USAGE & STATS API
// ============================================================================

export const usageApi = {
  // Get current usage metrics
  getMetrics: async (): Promise<UsageMetrics> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return mockUsageMetrics;
    }
    try {
      const dealerId = getDealerId();
      return await dealerBillingApi.getUsage(dealerId);
    } catch (error) {
      console.error('Error fetching usage metrics:', error);
      return mockUsageMetrics;
    }
  },

  // Get billing stats
  getStats: async (): Promise<BillingStats> => {
    if (USE_MOCK_DATA) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return mockBillingStats;
    }
    try {
      const dealerId = getDealerId();
      return await dealerBillingApi.getStats(dealerId);
    } catch (error) {
      console.error('Error fetching billing stats:', error);
      return mockBillingStats;
    }
  },
};

// ============================================================================
// COMBINED BILLING SERVICE
// ============================================================================

export const billingService = {
  plans: plansApi,
  subscription: subscriptionApi,
  invoices: invoicesApi,
  payments: paymentsApi,
  paymentMethods: paymentMethodsApi,
  usage: usageApi,
};

export default billingService;
