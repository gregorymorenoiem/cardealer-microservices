/**
 * Dealer Billing Service
 *
 * Frontend service for dealer billing operations.
 * Connects to the DealerBillingController endpoints.
 */

import { api } from './api';
import type {
  PlanConfig,
  Subscription,
  Invoice,
  Payment,
  PaymentMethodInfo,
  UsageMetrics,
  BillingStats,
} from '@/types/billing';

const USE_MOCK_DATA = import.meta.env.VITE_USE_MOCK_AUTH === 'true';

// ============================================================================
// TYPES
// ============================================================================

export interface DealerBillingDashboard {
  subscription: Subscription | null;
  invoices: Invoice[];
  payments: Payment[];
  usage: UsageMetrics;
  stats: BillingStats;
  plans: PlanConfig[];
}

// ============================================================================
// DEALER BILLING API
// ============================================================================

export const dealerBillingApi = {
  /**
   * Get complete billing dashboard data for a dealer
   */
  getDashboard: async (dealerId: string): Promise<DealerBillingDashboard> => {
    const response = await api.get(`/api/dealer-billing/dashboard/${dealerId}`);
    const data = response.data;

    return {
      subscription: data.summary?.currentSubscription
        ? mapSubscription(data.summary.currentSubscription)
        : null,
      invoices: (data.summary?.recentInvoices || []).map(mapInvoice),
      payments: (data.summary?.recentPayments || []).map(mapPayment),
      usage: mapUsage(data.usage),
      stats: mapStats(data.stats),
      plans: (data.plans || []).map(mapPlan),
    };
  },

  /**
   * Get current subscription
   */
  getSubscription: async (dealerId: string): Promise<Subscription | null> => {
    try {
      const response = await api.get(`/api/dealer-billing/subscription`, {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return mapSubscriptionFromDto(response.data);
    } catch (error) {
      console.error('Error fetching subscription:', error);
      return null;
    }
  },

  /**
   * Get all invoices for a dealer
   */
  getInvoices: async (dealerId: string): Promise<Invoice[]> => {
    try {
      const response = await api.get('/api/dealer-billing/invoices', {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return (response.data || []).map(mapInvoiceFromDto);
    } catch (error) {
      console.error('Error fetching invoices:', error);
      return [];
    }
  },

  /**
   * Get all payments for a dealer
   */
  getPayments: async (dealerId: string): Promise<Payment[]> => {
    try {
      const response = await api.get('/api/dealer-billing/payments', {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return (response.data || []).map(mapPaymentFromDto);
    } catch (error) {
      console.error('Error fetching payments:', error);
      return [];
    }
  },

  /**
   * Get usage metrics for a dealer
   */
  getUsage: async (dealerId: string): Promise<UsageMetrics> => {
    try {
      const response = await api.get('/api/dealer-billing/usage', {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return mapUsageFromDto(response.data);
    } catch (error) {
      console.error('Error fetching usage:', error);
      return getDefaultUsage();
    }
  },

  /**
   * Get billing stats for a dealer
   */
  getStats: async (dealerId: string): Promise<BillingStats> => {
    try {
      const response = await api.get('/api/dealer-billing/stats', {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return mapStatsFromDto(response.data);
    } catch (error) {
      console.error('Error fetching stats:', error);
      return getDefaultStats();
    }
  },

  /**
   * Get all available plans
   */
  getPlans: async (dealerId?: string): Promise<PlanConfig[]> => {
    try {
      const response = await api.get('/api/dealer-billing/plans', {
        headers: dealerId ? { 'X-Dealer-Id': dealerId } : {},
      });
      return (response.data || []).map(mapPlanFromDto);
    } catch (error) {
      console.error('Error fetching plans:', error);
      return getDefaultPlans();
    }
  },

  /**
   * Get payment methods for a dealer
   */
  getPaymentMethods: async (dealerId: string): Promise<PaymentMethodInfo[]> => {
    try {
      const response = await api.get('/api/dealer-billing/payment-methods', {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return (response.data || []).map(mapPaymentMethodFromDto);
    } catch (error) {
      console.error('Error fetching payment methods:', error);
      return [];
    }
  },
};

// ============================================================================
// MAPPING FUNCTIONS
// ============================================================================

function mapSubscription(data: any): Subscription {
  return {
    id: data.id?.toString() || '',
    dealerId: data.dealerId?.toString() || '',
    plan: (data.plan || 'free').toLowerCase(),
    status: (data.status || 'active').toLowerCase().replace(/_/g, '-'),
    cycle: (data.cycle || 'monthly').toLowerCase(),
    pricePerCycle: data.pricePerCycle || 0,
    currency: data.currency || 'USD',
    startDate: data.startDate || new Date().toISOString(),
    endDate: data.endDate,
    trialEndDate: data.trialEndDate,
    nextBillingDate: data.nextBillingDate,
    maxUsers: data.maxUsers || 1,
    maxVehicles: data.maxVehicles || 3,
    features: mapFeatures(data.features || {}),
    stripeCustomerId: data.stripeCustomerId,
    stripeSubscriptionId: data.stripeSubscriptionId,
    createdAt: data.createdAt || new Date().toISOString(),
    updatedAt: data.updatedAt,
  };
}

function mapSubscriptionFromDto(data: any): Subscription {
  return {
    id: data.id || '',
    dealerId: data.dealerId || '',
    plan: (data.plan || 'free').toLowerCase(),
    status: (data.status || 'active').toLowerCase().replace(/_/g, '-'),
    cycle: (data.cycle || 'monthly').toLowerCase(),
    pricePerCycle: data.pricePerCycle || 0,
    currency: data.currency || 'USD',
    startDate: data.startDate || new Date().toISOString(),
    nextBillingDate: data.nextBillingDate,
    maxUsers: data.maxUsers || 1,
    maxVehicles: data.maxVehicles || 3,
    features: mapFeaturesFromDto(data.features || {}),
    createdAt: data.startDate || new Date().toISOString(),
  };
}

function mapInvoice(data: any): Invoice {
  return {
    id: data.id?.toString() || '',
    dealerId: data.dealerId?.toString() || '',
    invoiceNumber: data.invoiceNumber || '',
    subscriptionId: data.subscriptionId?.toString(),
    status: (data.status || 'draft').toLowerCase().replace(/_/g, '-'),
    subtotal: data.subtotal || 0,
    taxAmount: data.taxAmount || 0,
    totalAmount: data.totalAmount || 0,
    paidAmount: data.paidAmount || 0,
    currency: data.currency || 'USD',
    issueDate: data.issueDate || new Date().toISOString(),
    dueDate: data.dueDate || new Date().toISOString(),
    paidDate: data.paidDate,
    pdfUrl: data.pdfUrl,
    lineItems: [],
  };
}

function mapInvoiceFromDto(data: any): Invoice {
  return {
    id: data.id || '',
    dealerId: data.dealerId || '',
    invoiceNumber: data.invoiceNumber || '',
    subscriptionId: data.subscriptionId,
    status: (data.status || 'draft').toLowerCase().replace(/_/g, '-'),
    subtotal: data.subtotal || 0,
    taxAmount: data.taxAmount || 0,
    totalAmount: data.totalAmount || 0,
    paidAmount: data.paidAmount || 0,
    currency: data.currency || 'USD',
    issueDate: data.issueDate || new Date().toISOString(),
    dueDate: data.dueDate || new Date().toISOString(),
    paidDate: data.paidDate,
    pdfUrl: data.pdfUrl,
    lineItems: [],
  };
}

function mapPayment(data: any): Payment {
  return {
    id: data.id?.toString() || '',
    dealerId: data.dealerId?.toString() || '',
    subscriptionId: data.subscriptionId?.toString(),
    invoiceId: data.invoiceId?.toString(),
    amount: data.amount || 0,
    currency: data.currency || 'USD',
    status: (data.status || 'pending').toLowerCase(),
    method: (data.paymentMethod || data.method || 'credit_card').toLowerCase().replace(/-/g, '_'),
    description: data.description,
    receiptUrl: data.receiptUrl,
    refundedAmount: data.refundedAmount || 0,
    createdAt: data.createdAt || new Date().toISOString(),
    processedAt: data.processedAt,
    cardLast4: data.cardLast4 || '4242',
    cardBrand: data.cardBrand || 'Visa',
  };
}

function mapPaymentFromDto(data: any): Payment {
  return {
    id: data.id || '',
    dealerId: data.dealerId || '',
    subscriptionId: data.subscriptionId,
    invoiceId: data.invoiceId,
    amount: data.amount || 0,
    currency: data.currency || 'USD',
    status: (data.status || 'pending').toLowerCase(),
    method: (data.method || 'credit_card').toLowerCase().replace(/-/g, '_'),
    description: data.description,
    receiptUrl: data.receiptUrl,
    refundedAmount: data.refundedAmount || 0,
    createdAt: data.createdAt || new Date().toISOString(),
    processedAt: data.processedAt,
    cardLast4: data.cardLast4 || '4242',
    cardBrand: data.cardBrand || 'Visa',
  };
}

function mapUsage(data: any): UsageMetrics {
  return {
    currentListings: data.currentListings || 0,
    maxListings: data.maxListings === 'unlimited' ? 'unlimited' : parseInt(data.maxListings) || 3,
    currentUsers: data.currentUsers || 1,
    maxUsers: data.maxUsers === 'unlimited' ? 'unlimited' : parseInt(data.maxUsers) || 1,
    storageUsed: data.storageUsed || '0 GB',
    storageLimit: data.storageLimit || '1 GB',
    apiCalls: data.apiCalls || 0,
    apiLimit: data.apiLimit === 'unlimited' ? 'unlimited' : parseInt(data.apiLimit) || 1000,
  };
}

function mapUsageFromDto(data: any): UsageMetrics {
  return mapUsage(data);
}

function mapStats(data: any): BillingStats {
  return {
    currentPlan: (data.currentPlan || 'free').toLowerCase(),
    monthlySpend: data.monthlySpend || 0,
    yearlySpend: data.yearlySpend || 0,
    outstandingBalance: data.outstandingBalance || 0,
    nextBillingAmount: data.nextBillingAmount || 0,
    nextBillingDate: data.nextBillingDate || new Date().toISOString(),
    totalPaid: data.totalPaid || 0,
    invoiceCount: data.invoiceCount || 0,
  };
}

function mapStatsFromDto(data: any): BillingStats {
  return mapStats(data);
}

function mapFeatures(data: any): any {
  return {
    listings: data.listings || data.maxVehicles || 3,
    users: data.users || data.maxUsers || 1,
    storage: data.storage || '1 GB',
    analytics: data.analytics ?? false,
    api: data.api ?? false,
    customBranding: data.customBranding ?? false,
    prioritySupport: data.prioritySupport ?? false,
    dedicatedManager: data.dedicatedManager ?? false,
    bulkUpload: data.bulkUpload ?? false,
    marketplace: data.marketplace ?? false,
    realEstate: data.realEstate ?? false,
    crm: data.crm ?? false,
    reporting: data.reporting ?? false,
  };
}

function mapFeaturesFromDto(data: any): any {
  return {
    listings: data.listings === -1 ? 'unlimited' : data.listings || 3,
    users: data.users === -1 ? 'unlimited' : data.users || 1,
    storage: data.storage || '1 GB',
    analytics: data.analytics ?? false,
    api: data.api ?? false,
    customBranding: data.customBranding ?? false,
    prioritySupport: data.prioritySupport ?? false,
    dedicatedManager: data.dedicatedManager ?? false,
    bulkUpload: data.bulkUpload ?? false,
    marketplace: data.marketplace ?? false,
    realEstate: data.realEstate ?? false,
    crm: data.crm ?? false,
    reporting: data.reporting ?? false,
  };
}

function mapPlan(data: any): PlanConfig {
  return {
    id: (data.plan || data.id || 'free').toString().toLowerCase(),
    name: data.planName || data.name || 'Free',
    description: data.description || '',
    prices: {
      monthly: data.monthlyPrice || data.prices?.monthly || 0,
      quarterly: data.prices?.quarterly || (data.monthlyPrice || 0) * 3 * 0.9,
      yearly: data.yearlyPrice || data.prices?.yearly || 0,
    },
    features: mapFeatures({
      listings: data.maxVehicles,
      users: data.maxUsers,
      ...data.features,
    }),
    popular: data.plan === 'Professional' || data.popular,
    enterprise: data.plan === 'Enterprise' || data.enterprise,
  };
}

function mapPlanFromDto(data: any): PlanConfig {
  return {
    id: data.id || 'free',
    name: data.name || 'Free',
    description: data.description || '',
    prices: {
      monthly: data.prices?.monthly || 0,
      quarterly: data.prices?.quarterly || 0,
      yearly: data.prices?.yearly || 0,
    },
    features: mapFeaturesFromDto(data.features || {}),
    popular: data.popular || false,
    enterprise: data.enterprise || false,
  };
}

function mapPaymentMethodFromDto(data: any): PaymentMethodInfo {
  return {
    id: data.id || '',
    type: data.type || 'card',
    isDefault: data.isDefault || false,
    card: data.card
      ? {
          brand: data.card.brand || 'Unknown',
          last4: data.card.last4 || '****',
          expMonth: data.card.expMonth || 12,
          expYear: data.card.expYear || 2025,
        }
      : undefined,
    bankAccount: data.bankAccount,
    createdAt: data.createdAt || new Date().toISOString(),
  };
}

// ============================================================================
// DEFAULT VALUES
// ============================================================================

function getDefaultUsage(): UsageMetrics {
  return {
    currentListings: 0,
    maxListings: 3,
    currentUsers: 1,
    maxUsers: 1,
    storageUsed: '0 GB',
    storageLimit: '1 GB',
    apiCalls: 0,
    apiLimit: 1000,
  };
}

function getDefaultStats(): BillingStats {
  return {
    currentPlan: 'free',
    monthlySpend: 0,
    yearlySpend: 0,
    outstandingBalance: 0,
    nextBillingAmount: 0,
    nextBillingDate: new Date().toISOString(),
    totalPaid: 0,
    invoiceCount: 0,
  };
}

function getDefaultPlans(): PlanConfig[] {
  return [
    {
      id: 'free',
      name: 'Free',
      description: 'Para empezar tu negocio',
      prices: { monthly: 0, quarterly: 0, yearly: 0 },
      features: {
        listings: 3,
        users: 1,
        storage: '1 GB',
        analytics: false,
        api: false,
        customBranding: false,
        prioritySupport: false,
        dedicatedManager: false,
        bulkUpload: false,
        marketplace: false,
        realEstate: false,
        crm: false,
        reporting: false,
      },
    },
    {
      id: 'basic',
      name: 'Basic',
      description: 'Para dealers en crecimiento',
      prices: { monthly: 29, quarterly: 78, yearly: 290 },
      features: {
        listings: 50,
        users: 5,
        storage: '5 GB',
        analytics: true,
        api: false,
        customBranding: false,
        prioritySupport: true,
        dedicatedManager: false,
        bulkUpload: true,
        marketplace: true,
        realEstate: false,
        crm: true,
        reporting: true,
      },
    },
    {
      id: 'professional',
      name: 'Professional',
      description: 'Para dealers establecidos',
      prices: { monthly: 79, quarterly: 213, yearly: 790 },
      features: {
        listings: 500,
        users: 20,
        storage: '20 GB',
        analytics: true,
        api: true,
        customBranding: true,
        prioritySupport: true,
        dedicatedManager: true,
        bulkUpload: true,
        marketplace: true,
        realEstate: true,
        crm: true,
        reporting: true,
      },
      popular: true,
    },
    {
      id: 'enterprise',
      name: 'Enterprise',
      description: 'Para grandes operaciones',
      prices: { monthly: 199, quarterly: 537, yearly: 1990 },
      features: {
        listings: 'unlimited',
        users: 'unlimited',
        storage: 'unlimited',
        analytics: true,
        api: true,
        customBranding: true,
        prioritySupport: true,
        dedicatedManager: true,
        bulkUpload: true,
        marketplace: true,
        realEstate: true,
        crm: true,
        reporting: true,
      },
      enterprise: true,
    },
  ];
}

export default dealerBillingApi;
