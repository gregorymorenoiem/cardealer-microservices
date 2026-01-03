/**
 * Billing Service
 * 
 * Frontend service for billing and subscription operations.
 * Uses mock data when USE_MOCK_AUTH is true.
 */

import { api } from './api';
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
  BillingStats 
} from '@/types/billing';

const USE_MOCK_DATA = import.meta.env.VITE_USE_MOCK_AUTH !== 'false';

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
      await new Promise(resolve => setTimeout(resolve, 200));
      return plans;
    }
    const response = await api.get<PlanConfig[]>('/billing/plans');
    return response.data;
  },

  // Get a specific plan
  getById: async (planId: string): Promise<PlanConfig | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 100));
      return getPlanById(planId) || null;
    }
    const response = await api.get<PlanConfig>(`/billing/plans/${planId}`);
    return response.data;
  },

  // Compare plans (features matrix)
  compare: async (): Promise<PlanConfig[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return plans;
    }
    const response = await api.get<PlanConfig[]>('/billing/plans/compare');
    return response.data;
  },
};

// ============================================================================
// SUBSCRIPTION API
// ============================================================================

export const subscriptionApi = {
  // Get current subscription
  getCurrent: async (dealerId: string): Promise<Subscription | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return getSubscriptionByDealerId(dealerId) || null;
    }
    const response = await api.get<Subscription>('/billing/subscription');
    return response.data;
  },

  // Create a new subscription
  create: async (data: CreateSubscriptionRequest): Promise<Subscription> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 500));
      const plan = getPlanById(data.plan);
      if (!plan) throw new Error('Plan not found');
      
      const price = data.cycle === 'monthly' 
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
      await new Promise(resolve => setTimeout(resolve, 500));
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
      await new Promise(resolve => setTimeout(resolve, 400));
      console.log('Subscription cancelled:', reason);
      return;
    }
    await api.post('/billing/subscription/cancel', { reason });
  },

  // Reactivate subscription
  reactivate: async (): Promise<Subscription> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 400));
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

export const invoicesApi = {
  // Get all invoices
  getAll: async (): Promise<Invoice[]> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      return mockInvoices;
    }
    const response = await api.get<Invoice[]>('/billing/invoices');
    return response.data;
  },

  // Get invoice by ID
  getById: async (invoiceId: string): Promise<Invoice | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockInvoices.find(inv => inv.id === invoiceId) || null;
    }
    const response = await api.get<Invoice>(`/billing/invoices/${invoiceId}`);
    return response.data;
  },

  // Download invoice PDF
  downloadPdf: async (invoiceId: string): Promise<Blob> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 500));
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
      await new Promise(resolve => setTimeout(resolve, 600));
      const invoice = mockInvoices.find(inv => inv.id === invoiceId);
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
      await new Promise(resolve => setTimeout(resolve, 300));
      return mockPayments;
    }
    const response = await api.get<Payment[]>('/billing/payments');
    return response.data;
  },

  // Get payment by ID
  getById: async (paymentId: string): Promise<Payment | null> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockPayments.find(p => p.id === paymentId) || null;
    }
    const response = await api.get<Payment>(`/billing/payments/${paymentId}`);
    return response.data;
  },

  // Request refund
  refund: async (paymentId: string, amount?: number, reason?: string): Promise<Payment> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 500));
      const payment = mockPayments.find(p => p.id === paymentId);
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
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockPaymentMethods;
    }
    const response = await api.get<PaymentMethodInfo[]>('/billing/payment-methods');
    return response.data;
  },

  // Add a new payment method
  add: async (data: AddPaymentMethodRequest): Promise<PaymentMethodInfo> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 500));
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
      await new Promise(resolve => setTimeout(resolve, 300));
      mockPaymentMethods.forEach(pm => {
        pm.isDefault = pm.id === paymentMethodId;
      });
      return;
    }
    await api.post(`/billing/payment-methods/${paymentMethodId}/default`);
  },

  // Remove a payment method
  remove: async (paymentMethodId: string): Promise<void> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 300));
      const index = mockPaymentMethods.findIndex(pm => pm.id === paymentMethodId);
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
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockUsageMetrics;
    }
    const response = await api.get<UsageMetrics>('/billing/usage');
    return response.data;
  },

  // Get billing stats
  getStats: async (): Promise<BillingStats> => {
    if (USE_MOCK_DATA) {
      await new Promise(resolve => setTimeout(resolve, 200));
      return mockBillingStats;
    }
    const response = await api.get<BillingStats>('/billing/stats');
    return response.data;
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
