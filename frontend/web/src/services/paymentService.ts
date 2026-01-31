/**
 * Payment Service - API Client
 *
 * Servicio de pagos multi-proveedor para OKLA
 * Soporta: AZUL, CardNET, PixelPay, Fygaro, PayPal
 */

import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'https://api.okla.com.do';

// ============================================================================
// TYPES & ENUMS
// ============================================================================

export enum PaymentGateway {
  Azul = 'Azul',
  CardNET = 'CardNET',
  PixelPay = 'PixelPay',
  Fygaro = 'Fygaro',
  PayPal = 'PayPal',
}

export enum PaymentStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Approved = 'Approved',
  Declined = 'Declined',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded',
  PartialRefund = 'PartialRefund',
  Chargeback = 'Chargeback',
  Expired = 'Expired',
}

export enum Currency {
  DOP = 'DOP',
  USD = 'USD',
  EUR = 'EUR',
}

export enum ProductType {
  Listing = 'Listing',
  Subscription = 'Subscription',
  Promotion = 'Promotion',
  Featured = 'Featured',
}

export enum SubscriptionPlan {
  None = 'None',
  Starter = 'Starter',
  Pro = 'Pro',
  Enterprise = 'Enterprise',
}

export interface PaymentMethod {
  id: string;
  gateway: PaymentGateway;
  type: 'card' | 'paypal';
  last4?: string;
  brand?: string; // Visa, Mastercard, etc
  expiryMonth?: number;
  expiryYear?: number;
  isDefault: boolean;
  createdAt: string;
}

export interface ChargeRequest {
  amount: number;
  currency: Currency;
  gateway: PaymentGateway;
  productType: ProductType;
  productId?: string;
  paymentMethodId?: string; // Use saved card
  saveCard?: boolean;
  cardNumber?: string;
  cardExpiry?: string;
  cardCvv?: string;
  cardholderName?: string;
  discountCode?: string;
  idempotencyKey?: string;
}

export interface ChargeResponse {
  transactionId: string;
  status: PaymentStatus;
  gateway: PaymentGateway;
  amount: number;
  currency: Currency;
  amountDop?: number; // Converted amount if foreign currency
  exchangeRate?: number;
  ncf?: string; // NCF number
  invoiceId?: string;
  redirectUrl?: string; // For PayPal
  message?: string;
  error?: string;
}

export interface SubscriptionPlanInfo {
  plan: SubscriptionPlan;
  name: string;
  price: number;
  earlyBirdPrice?: number;
  maxListings: number;
  features: string[];
  recommended?: boolean;
}

export interface CreateSubscriptionRequest {
  plan: SubscriptionPlan;
  gateway: PaymentGateway;
  paymentMethodId?: string;
  cardNumber?: string;
  cardExpiry?: string;
  cardCvv?: string;
  cardholderName?: string;
  discountCode?: string;
}

export interface Subscription {
  id: string;
  plan: SubscriptionPlan;
  status: 'Active' | 'PastDue' | 'Cancelled' | 'Paused';
  currentPeriodStart: string;
  currentPeriodEnd: string;
  cancelAtPeriodEnd: boolean;
  amount: number;
  currency: Currency;
  gateway: PaymentGateway;
  paymentMethodId: string;
  createdAt: string;
}

export interface Transaction {
  id: string;
  type: 'Charge' | 'Refund' | 'Subscription';
  status: PaymentStatus;
  amount: number;
  currency: Currency;
  gateway: PaymentGateway;
  productType: ProductType;
  productId?: string;
  ncf?: string;
  invoiceId?: string;
  refundedAmount?: number;
  createdAt: string;
  updatedAt: string;
}

export interface RefundRequest {
  transactionId: string;
  amount?: number; // Partial refund
  reason: string;
}

// ============================================================================
// API CLIENT
// ============================================================================

const apiClient = axios.create({
  baseURL: `${API_URL}/api/payments`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token interceptor
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ============================================================================
// PAYMENT METHODS
// ============================================================================

export async function getPaymentMethods(): Promise<PaymentMethod[]> {
  const response = await apiClient.get<PaymentMethod[]>('/methods');
  return response.data;
}

export async function addPaymentMethod(
  gateway: PaymentGateway,
  cardNumber: string,
  cardExpiry: string,
  cardCvv: string,
  cardholderName: string,
  setAsDefault?: boolean
): Promise<PaymentMethod> {
  const response = await apiClient.post<PaymentMethod>('/methods', {
    gateway,
    cardNumber,
    cardExpiry,
    cardCvv,
    cardholderName,
    setAsDefault,
  });
  return response.data;
}

export async function deletePaymentMethod(id: string): Promise<void> {
  await apiClient.delete(`/methods/${id}`);
}

export async function setDefaultPaymentMethod(id: string): Promise<void> {
  await apiClient.put(`/methods/${id}/default`);
}

// ============================================================================
// CHARGES
// ============================================================================

export async function charge(request: ChargeRequest): Promise<ChargeResponse> {
  const response = await apiClient.post<ChargeResponse>('/charge', request);
  return response.data;
}

export async function getTransaction(id: string): Promise<Transaction> {
  const response = await apiClient.get<Transaction>(`/transactions/${id}`);
  return response.data;
}

export async function getTransactions(
  page = 1,
  pageSize = 20,
  status?: PaymentStatus
): Promise<{ data: Transaction[]; total: number }> {
  const response = await apiClient.get('/transactions', {
    params: { page, pageSize, status },
  });
  return response.data;
}

// ============================================================================
// REFUNDS
// ============================================================================

export async function requestRefund(request: RefundRequest): Promise<Transaction> {
  const response = await apiClient.post<Transaction>('/refunds', request);
  return response.data;
}

export async function getPendingRefunds(): Promise<Transaction[]> {
  const response = await apiClient.get<Transaction[]>('/refunds/pending');
  return response.data;
}

export async function approveRefund(
  transactionId: string,
  adminNote?: string
): Promise<Transaction> {
  const response = await apiClient.post<Transaction>(`/refunds/${transactionId}/approve`, {
    adminNote,
  });
  return response.data;
}

export async function rejectRefund(transactionId: string, reason: string): Promise<Transaction> {
  const response = await apiClient.post<Transaction>(`/refunds/${transactionId}/reject`, {
    reason,
  });
  return response.data;
}

// ============================================================================
// SUBSCRIPTIONS
// ============================================================================

export async function getSubscriptionPlans(): Promise<SubscriptionPlanInfo[]> {
  const response = await apiClient.get<SubscriptionPlanInfo[]>('/subscriptions/plans');
  return response.data;
}

export async function getCurrentSubscription(): Promise<Subscription | null> {
  try {
    const response = await apiClient.get<Subscription>('/subscriptions/current');
    return response.data;
  } catch (error: unknown) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
}

export async function createSubscription(
  request: CreateSubscriptionRequest
): Promise<Subscription> {
  const response = await apiClient.post<Subscription>('/subscriptions', request);
  return response.data;
}

export async function cancelSubscription(
  reason: string,
  cancelImmediately = false
): Promise<Subscription> {
  const response = await apiClient.post<Subscription>('/subscriptions/cancel', {
    reason,
    cancelImmediately,
  });
  return response.data;
}

export async function pauseSubscription(months = 1): Promise<Subscription> {
  const response = await apiClient.post<Subscription>('/subscriptions/pause', { months });
  return response.data;
}

export async function resumeSubscription(): Promise<Subscription> {
  const response = await apiClient.post<Subscription>('/subscriptions/resume');
  return response.data;
}

export async function changePlan(newPlan: SubscriptionPlan): Promise<Subscription> {
  const response = await apiClient.post<Subscription>('/subscriptions/change-plan', {
    newPlan,
  });
  return response.data;
}

// ============================================================================
// DISCOUNT CODES
// ============================================================================

export interface DiscountCode {
  code: string;
  type: 'percentage' | 'fixed';
  value: number;
  validFor: ProductType[];
  expiresAt?: string;
}

export async function validateDiscountCode(
  code: string,
  productType: ProductType
): Promise<DiscountCode | null> {
  try {
    const response = await apiClient.get<DiscountCode>(`/discounts/validate`, {
      params: { code, productType },
    });
    return response.data;
  } catch (error: unknown) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
}

// ============================================================================
// GATEWAY SELECTION
// ============================================================================

export function selectOptimalGateway(context: {
  userCountry: string;
  isSubscription: boolean;
  merchantVolume?: number;
}): PaymentGateway {
  // International → PayPal
  if (context.userCountry !== 'DO') {
    return PaymentGateway.PayPal;
  }

  // Subscriptions → Fygaro (optimized for recurring)
  if (context.isSubscription) {
    return PaymentGateway.Fygaro;
  }

  // High volume → PixelPay (lower fees)
  if (context.merchantVolume && context.merchantVolume > 50000) {
    return PaymentGateway.PixelPay;
  }

  // Default → AZUL (banking, reliable)
  return PaymentGateway.Azul;
}

export function getGatewayInfo(gateway: PaymentGateway): {
  name: string;
  logo: string;
  color: string;
  description: string;
} {
  const info: Record<PaymentGateway, ReturnType<typeof getGatewayInfo>> = {
    [PaymentGateway.Azul]: {
      name: 'AZUL',
      logo: '/logos/azul.svg',
      color: '#0066B3',
      description: 'Banco Popular Dominicano',
    },
    [PaymentGateway.CardNET]: {
      name: 'CardNET',
      logo: '/logos/cardnet.svg',
      color: '#E31837',
      description: 'Red de tarjetas dominicana',
    },
    [PaymentGateway.PixelPay]: {
      name: 'PixelPay',
      logo: '/logos/pixelpay.svg',
      color: '#6366F1',
      description: 'Pagos modernos y seguros',
    },
    [PaymentGateway.Fygaro]: {
      name: 'Fygaro',
      logo: '/logos/fygaro.svg',
      color: '#10B981',
      description: 'Suscripciones automatizadas',
    },
    [PaymentGateway.PayPal]: {
      name: 'PayPal',
      logo: '/logos/paypal.svg',
      color: '#003087',
      description: 'Pagos internacionales',
    },
  };
  return info[gateway];
}

// ============================================================================
// EARLY BIRD HELPERS
// ============================================================================

const EARLY_BIRD_DEADLINE = new Date('2026-01-31T23:59:59');
const EARLY_BIRD_DISCOUNT = 0.2; // 20%
const EARLY_BIRD_FREE_MONTHS = 3;

export function isEarlyBirdActive(): boolean {
  return new Date() < EARLY_BIRD_DEADLINE;
}

export function getEarlyBirdDaysRemaining(): number {
  const now = new Date();
  const diff = EARLY_BIRD_DEADLINE.getTime() - now.getTime();
  return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
}

export function calculateEarlyBirdPrice(regularPrice: number): number {
  return Math.round(regularPrice * (1 - EARLY_BIRD_DISCOUNT));
}

export function getEarlyBirdBenefits(): string[] {
  return [
    `${EARLY_BIRD_FREE_MONTHS} meses gratis`,
    `${EARLY_BIRD_DISCOUNT * 100}% descuento de por vida`,
    'Badge de Miembro Fundador 🏆',
  ];
}

// ============================================================================
// SUBSCRIPTION PLAN HELPERS
// ============================================================================

export function getSubscriptionPlanDetails(): SubscriptionPlanInfo[] {
  const isEarlyBird = isEarlyBirdActive();

  return [
    {
      plan: SubscriptionPlan.Starter,
      name: 'Starter',
      price: 49,
      earlyBirdPrice: isEarlyBird ? calculateEarlyBirdPrice(49) : undefined,
      maxListings: 15,
      features: [
        '15 vehículos activos',
        'Panel de control básico',
        'Soporte por email',
        'Estadísticas básicas',
        'Perfil verificado',
        'Sin anuncios',
      ],
    },
    {
      plan: SubscriptionPlan.Pro,
      name: 'Pro',
      price: 129,
      earlyBirdPrice: isEarlyBird ? calculateEarlyBirdPrice(129) : undefined,
      maxListings: 50,
      features: [
        '50 vehículos activos',
        'Panel de control avanzado',
        'Soporte prioritario',
        'Estadísticas completas',
        'Importación CSV/Excel',
        'API de integración',
        'Múltiples sucursales',
        'CRM básico',
      ],
      recommended: true,
    },
    {
      plan: SubscriptionPlan.Enterprise,
      name: 'Enterprise',
      price: 299,
      earlyBirdPrice: isEarlyBird ? calculateEarlyBirdPrice(299) : undefined,
      maxListings: Infinity,
      features: [
        'Vehículos ILIMITADOS',
        'Panel enterprise',
        'Soporte 24/7 dedicado',
        'Analytics avanzados + IA',
        'Import/Export masivo',
        'API completa + Webhooks',
        'Sucursales ilimitadas',
        'CRM completo',
        'Account Manager dedicado',
      ],
    },
  ];
}

// ============================================================================
// LISTING PRICE
// ============================================================================

export const LISTING_PRICE = 29; // USD
export const ITBIS_RATE = 0.18; // 18%

export function calculateListingTotal(): {
  subtotal: number;
  itbis: number;
  total: number;
} {
  const subtotal = LISTING_PRICE;
  const itbis = subtotal * ITBIS_RATE;
  const total = subtotal + itbis;
  return {
    subtotal,
    itbis: Math.round(itbis * 100) / 100,
    total: Math.round(total * 100) / 100,
  };
}

export default {
  // Payment Methods
  getPaymentMethods,
  addPaymentMethod,
  deletePaymentMethod,
  setDefaultPaymentMethod,
  // Charges
  charge,
  getTransaction,
  getTransactions,
  // Refunds
  requestRefund,
  getPendingRefunds,
  approveRefund,
  rejectRefund,
  // Subscriptions
  getSubscriptionPlans,
  getCurrentSubscription,
  createSubscription,
  cancelSubscription,
  pauseSubscription,
  resumeSubscription,
  changePlan,
  // Discounts
  validateDiscountCode,
  // Helpers
  selectOptimalGateway,
  getGatewayInfo,
  isEarlyBirdActive,
  getEarlyBirdDaysRemaining,
  calculateEarlyBirdPrice,
  getEarlyBirdBenefits,
  getSubscriptionPlanDetails,
  calculateListingTotal,
};
