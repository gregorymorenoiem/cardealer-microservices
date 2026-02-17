/**
 * Azul Payment Service
 *
 * Frontend client for AzulPaymentService backend
 * Handles payments, subscriptions, and webhooks for Azul (Banco Popular)
 *
 * Gateway Routes:
 * - /api/azul-payment/payments/charge     → Process one-time charge
 * - /api/azul-payment/payments/{id}       → Get transaction details
 * - /api/azul-payment/payments/refund     → Process refund
 * - /api/azul-payment/subscriptions       → Create recurring subscription
 * - /api/azul-payment/subscriptions/{id}  → Get/cancel subscription
 * - /api/azul-payment/webhooks/event      → Webhook receiver
 */

import axios, { type AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

// ============================================================================
// TYPES
// ============================================================================

export enum TransactionStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Approved = 'Approved',
  Declined = 'Declined',
  Error = 'Error',
  Refunded = 'Refunded',
  PartiallyRefunded = 'PartiallyRefunded',
  Cancelled = 'Cancelled',
}

export enum SubscriptionFrequency {
  Daily = 'Daily',
  Weekly = 'Weekly',
  Biweekly = 'Biweekly',
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  Yearly = 'Yearly',
}

export enum PaymentMethod {
  CreditCard = 'CreditCard',
  DebitCard = 'DebitCard',
  CardToken = 'CardToken',
}

export enum SubscriptionStatus {
  Active = 'Active',
  Paused = 'Paused',
  Cancelled = 'Cancelled',
  Expired = 'Expired',
  PastDue = 'PastDue',
}

// ----------------------------------------------------------------------------
// Request DTOs
// ----------------------------------------------------------------------------

export interface ChargeRequest {
  amount: number;
  currency?: string; // Default: DOP
  description: string;
  orderId: string;
  customerId: string;
  customerEmail: string;
  customerPhone?: string;
  paymentMethod?: PaymentMethod;
  cardNumber?: string;
  cardExpiryMonth?: string;
  cardExpiryYear?: string;
  cardCVV?: string;
  cardholderName?: string;
  cardToken?: string; // If using tokenized card
  metadata?: Record<string, string>;
}

export interface RefundRequest {
  transactionId: string;
  amount?: number; // Optional for partial refund
  reason: string;
}

export interface SubscriptionRequest {
  userId: string;
  amount: number;
  currency?: string; // Default: DOP
  frequency?: SubscriptionFrequency; // Default: Monthly
  startDate?: string;
  endDate?: string;
  cardToken?: string;
  cardNumber?: string;
  cardExpiryMonth?: string;
  cardExpiryYear?: string;
  cardCVV?: string;
  cardholderName?: string;
  customerEmail?: string;
  customerPhone?: string;
  planName?: string;
  invoiceReference?: string;
  paymentMethod?: PaymentMethod;
}

// ----------------------------------------------------------------------------
// Response DTOs
// ----------------------------------------------------------------------------

export interface ChargeResponse {
  transactionId: string;
  azulTransactionId: string;
  status: TransactionStatus;
  amount: number;
  currency: string;
  approvalCode?: string;
  errorMessage?: string;
  cardLastFour?: string;
  cardBrand?: string;
  processedAt: string;
}

export interface SubscriptionResponse {
  subscriptionId: string;
  azulSubscriptionId?: string;
  status: SubscriptionStatus;
  amount: number;
  currency: string;
  frequency: SubscriptionFrequency;
  nextChargeDate: string;
  startDate: string;
  endDate?: string;
  cardLastFour?: string;
  cardBrand?: string;
  planName?: string;
  createdAt: string;
}

export interface TransactionDetails {
  id: string;
  azulTransactionId: string;
  status: TransactionStatus;
  amount: number;
  currency: string;
  description: string;
  orderId: string;
  customerId: string;
  customerEmail: string;
  approvalCode?: string;
  errorMessage?: string;
  cardLastFour?: string;
  cardBrand?: string;
  refundedAmount?: number;
  metadata?: Record<string, string>;
  createdAt: string;
  processedAt?: string;
}

export interface AzulHealthStatus {
  status: string;
  service: string;
}

// ============================================================================
// API CLIENT
// ============================================================================

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 60000, // 60s for payment operations
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Add refresh token interceptor
addRefreshTokenInterceptor(apiClient);

// ============================================================================
// AZUL PAYMENT API
// ============================================================================

export const azulPaymentApi = {
  // --------------------------------------------------------------------------
  // PAYMENTS
  // --------------------------------------------------------------------------

  /**
   * Process a one-time charge
   */
  charge: async (data: ChargeRequest): Promise<ChargeResponse> => {
    const response = await apiClient.post<ChargeResponse>(
      '/api/azul-payment/payments/charge',
      data
    );
    return response.data;
  },

  /**
   * Get transaction details by ID
   */
  getTransaction: async (transactionId: string): Promise<TransactionDetails> => {
    const response = await apiClient.get<TransactionDetails>(
      `/api/azul-payment/payments/${transactionId}`
    );
    return response.data;
  },

  /**
   * Process a refund (full or partial)
   */
  refund: async (data: RefundRequest): Promise<ChargeResponse> => {
    const response = await apiClient.post<ChargeResponse>(
      '/api/azul-payment/payments/refund',
      data
    );
    return response.data;
  },

  // --------------------------------------------------------------------------
  // SUBSCRIPTIONS
  // --------------------------------------------------------------------------

  /**
   * Create a recurring subscription
   * Used for dealer monthly plans
   */
  createSubscription: async (data: SubscriptionRequest): Promise<SubscriptionResponse> => {
    const response = await apiClient.post<SubscriptionResponse>(
      '/api/azul-payment/subscriptions',
      data
    );
    return response.data;
  },

  /**
   * Get subscription details
   */
  getSubscription: async (subscriptionId: string): Promise<SubscriptionResponse> => {
    const response = await apiClient.get<SubscriptionResponse>(
      `/api/azul-payment/subscriptions/${subscriptionId}`
    );
    return response.data;
  },

  /**
   * Cancel a subscription
   */
  cancelSubscription: async (subscriptionId: string, reason?: string): Promise<void> => {
    await apiClient.delete(`/api/azul-payment/subscriptions/${subscriptionId}`, {
      params: { reason },
    });
  },

  // --------------------------------------------------------------------------
  // HEALTH
  // --------------------------------------------------------------------------

  /**
   * Check if Azul Payment Service is healthy
   */
  healthCheck: async (): Promise<AzulHealthStatus> => {
    const response = await apiClient.get<AzulHealthStatus>('/api/azul-payment/health');
    return response.data;
  },
};

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Get display label for transaction status
 */
export const getTransactionStatusLabel = (status: TransactionStatus): string => {
  const labels: Record<TransactionStatus, string> = {
    [TransactionStatus.Pending]: 'Pendiente',
    [TransactionStatus.Processing]: 'Procesando',
    [TransactionStatus.Approved]: 'Aprobada',
    [TransactionStatus.Declined]: 'Rechazada',
    [TransactionStatus.Error]: 'Error',
    [TransactionStatus.Refunded]: 'Reembolsada',
    [TransactionStatus.PartiallyRefunded]: 'Reembolso Parcial',
    [TransactionStatus.Cancelled]: 'Cancelada',
  };
  return labels[status] || status;
};

/**
 * Get color for transaction status badge
 */
export const getTransactionStatusColor = (
  status: TransactionStatus
): 'green' | 'red' | 'yellow' | 'gray' | 'blue' => {
  switch (status) {
    case TransactionStatus.Approved:
      return 'green';
    case TransactionStatus.Declined:
    case TransactionStatus.Error:
    case TransactionStatus.Cancelled:
      return 'red';
    case TransactionStatus.Pending:
    case TransactionStatus.Processing:
      return 'yellow';
    case TransactionStatus.Refunded:
    case TransactionStatus.PartiallyRefunded:
      return 'blue';
    default:
      return 'gray';
  }
};

/**
 * Get display label for subscription frequency
 */
export const getFrequencyLabel = (frequency: SubscriptionFrequency): string => {
  const labels: Record<SubscriptionFrequency, string> = {
    [SubscriptionFrequency.Daily]: 'Diario',
    [SubscriptionFrequency.Weekly]: 'Semanal',
    [SubscriptionFrequency.Biweekly]: 'Quincenal',
    [SubscriptionFrequency.Monthly]: 'Mensual',
    [SubscriptionFrequency.Quarterly]: 'Trimestral',
    [SubscriptionFrequency.Yearly]: 'Anual',
  };
  return labels[frequency] || frequency;
};

/**
 * Get subscription status label
 */
export const getSubscriptionStatusLabel = (status: SubscriptionStatus): string => {
  const labels: Record<SubscriptionStatus, string> = {
    [SubscriptionStatus.Active]: 'Activa',
    [SubscriptionStatus.Paused]: 'Pausada',
    [SubscriptionStatus.Cancelled]: 'Cancelada',
    [SubscriptionStatus.Expired]: 'Expirada',
    [SubscriptionStatus.PastDue]: 'Vencida',
  };
  return labels[status] || status;
};

/**
 * Format amount for display (DOP currency)
 */
export const formatDOPAmount = (amount: number): string => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    minimumFractionDigits: 2,
  }).format(amount);
};

/**
 * Validate credit card number using Luhn algorithm
 */
export const validateCardNumber = (cardNumber: string): boolean => {
  const digits = cardNumber.replace(/\D/g, '');
  if (digits.length < 13 || digits.length > 19) return false;

  let sum = 0;
  let isEven = false;

  for (let i = digits.length - 1; i >= 0; i--) {
    let digit = parseInt(digits[i], 10);

    if (isEven) {
      digit *= 2;
      if (digit > 9) digit -= 9;
    }

    sum += digit;
    isEven = !isEven;
  }

  return sum % 10 === 0;
};

/**
 * Get card brand from card number
 */
export const getCardBrand = (cardNumber: string): string => {
  const digits = cardNumber.replace(/\D/g, '');

  if (/^4/.test(digits)) return 'Visa';
  if (/^5[1-5]/.test(digits)) return 'MasterCard';
  if (/^3[47]/.test(digits)) return 'American Express';
  if (/^6(?:011|5)/.test(digits)) return 'Discover';

  return 'Desconocida';
};

/**
 * Mask card number for display
 */
export const maskCardNumber = (cardNumber: string): string => {
  const digits = cardNumber.replace(/\D/g, '');
  const lastFour = digits.slice(-4);
  return `**** **** **** ${lastFour}`;
};

// ============================================================================
// DEALER PLAN PRICING
// ============================================================================

export interface DealerPlan {
  id: string;
  name: string;
  monthlyPrice: number;
  yearlyPrice: number;
  features: string[];
  maxListings: number;
  highlighted?: boolean;
}

export const DEALER_PLANS: DealerPlan[] = [
  {
    id: 'starter',
    name: 'Starter',
    monthlyPrice: 2900, // RD$2,900/mes
    yearlyPrice: 29000, // RD$29,000/año (2 meses gratis)
    maxListings: 10,
    features: [
      'Hasta 10 vehículos activos',
      'Perfil de dealer básico',
      'Estadísticas básicas',
      'Soporte por email',
    ],
  },
  {
    id: 'professional',
    name: 'Professional',
    monthlyPrice: 5900, // RD$5,900/mes
    yearlyPrice: 59000, // RD$59,000/año (2 meses gratis)
    maxListings: 50,
    highlighted: true,
    features: [
      'Hasta 50 vehículos activos',
      'Perfil de dealer verificado ✓',
      'Estadísticas avanzadas',
      'Importación CSV/Excel',
      'Soporte prioritario',
      'Badge "Verificado"',
    ],
  },
  {
    id: 'enterprise',
    name: 'Enterprise',
    monthlyPrice: 14900, // RD$14,900/mes
    yearlyPrice: 149000, // RD$149,000/año (2 meses gratis)
    maxListings: -1, // Unlimited
    features: [
      'Vehículos ilimitados',
      'Múltiples sucursales',
      'API de integración',
      'Analytics premium',
      'Gerente de cuenta dedicado',
      'Personalización de marca',
      'Leads prioritarios',
    ],
  },
];

/**
 * Get plan by ID
 */
export const getPlanById = (planId: string): DealerPlan | undefined => {
  return DEALER_PLANS.find((p) => p.id === planId);
};

/**
 * Calculate Early Bird discount (20% off first 3 months)
 */
export const calculateEarlyBirdPrice = (plan: DealerPlan): number => {
  return Math.round(plan.monthlyPrice * 0.8);
};

export default azulPaymentApi;
