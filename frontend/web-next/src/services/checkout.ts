/**
 * Checkout Service
 *
 * Handles checkout and payment processing
 */

import { apiClient } from '@/lib/api-client';

// =============================================================================
// TYPES
// =============================================================================

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  originalPrice?: number;
  currency: 'DOP' | 'USD';
  type: 'boost' | 'subscription' | 'listing';
  features: string[];
  duration?: number; // days for boost, null for subscription
}

export interface CheckoutSession {
  sessionId: string;
  productId: string;
  product: Product;
  subtotal: number;
  tax: number;
  total: number;
  currency: 'DOP' | 'USD';
  status: 'pending' | 'processing' | 'completed' | 'failed' | 'cancelled';
  paymentUrl?: string; // For redirect to payment gateway
}

export interface CreateCheckoutRequest {
  productId: string;
  vehicleId?: string; // For boost products
  dealerId?: string; // For dealer subscriptions
  promoCode?: string;
  paymentMethod: 'card' | 'azul' | 'stripe';
  returnUrl?: string;
  cancelUrl?: string;
}

export interface PaymentIntent {
  id: string;
  clientSecret: string;
  amount: number;
  currency: 'DOP' | 'USD';
  status:
    | 'requires_payment_method'
    | 'requires_confirmation'
    | 'requires_action'
    | 'processing'
    | 'succeeded'
    | 'canceled';
}

export interface ProcessPaymentRequest {
  sessionId: string;
  paymentMethodId?: string;
  cardToken?: string;
}

export interface ProcessPaymentResponse {
  success: boolean;
  orderId?: string;
  transactionId?: string;
  receiptUrl?: string;
  error?: string;
}

export interface PromoCodeValidation {
  valid: boolean;
  discountType: 'percentage' | 'fixed';
  discountValue: number;
  discountAmount: number;
  newTotal: number;
  errorMessage?: string;
}

// =============================================================================
// STATIC PRODUCTS (fallback)
// =============================================================================

const staticProducts: Record<string, Product> = {
  'boost-basic': {
    id: 'boost-basic',
    name: 'Boost Básico',
    description: 'Destaca tu vehículo por 7 días',
    price: 499,
    currency: 'DOP',
    type: 'boost',
    duration: 7,
    features: ['Publicación destacada 7 días', 'Badge "Destacado"', '+50% más vistas'],
  },
  'boost-premium': {
    id: 'boost-premium',
    name: 'Boost Premium',
    description: 'Destaca tu vehículo por 30 días',
    price: 1499,
    originalPrice: 1996,
    currency: 'DOP',
    type: 'boost',
    duration: 30,
    features: [
      'Publicación destacada 30 días',
      'Badge "Destacado"',
      'Aparece en homepage',
      '+150% más vistas',
      'Estadísticas avanzadas',
    ],
  },
  'dealer-starter': {
    id: 'dealer-starter',
    name: 'Plan Dealer Starter',
    description: 'Plan mensual para dealers pequeños',
    price: 2999,
    currency: 'DOP',
    type: 'subscription',
    features: [
      'Hasta 20 vehículos',
      'Dashboard básico',
      'Soporte por email',
      'Badge de verificación',
    ],
  },
  'dealer-pro': {
    id: 'dealer-pro',
    name: 'Plan Dealer Pro',
    description: 'Plan mensual para dealers medianos',
    price: 5999,
    originalPrice: 7499,
    currency: 'DOP',
    type: 'subscription',
    features: [
      'Hasta 50 vehículos',
      'Dashboard completo',
      'CRM integrado',
      'Analytics avanzados',
      'Soporte prioritario',
      'Múltiples usuarios',
    ],
  },
  'listing-single': {
    id: 'listing-single',
    name: 'Publicación Individual',
    description: 'Publica un vehículo',
    price: 29,
    currency: 'USD',
    type: 'listing',
    duration: 30,
    features: [
      'Publicación por 30 días',
      'Hasta 20 fotos',
      'Contacto directo',
      'Estadísticas básicas',
    ],
  },
};

// =============================================================================
// API FUNCTIONS
// =============================================================================

/**
 * Get product by ID
 */
export async function getProduct(productId: string): Promise<Product> {
  try {
    const response = await apiClient.get<Product>(`/api/checkout/products/${productId}`);
    return response.data;
  } catch {
    // Fallback to static products
    const product = staticProducts[productId];
    if (product) return product;
    throw new Error('Producto no encontrado');
  }
}

/**
 * Get all available products
 */
export async function getProducts(): Promise<Product[]> {
  try {
    const response = await apiClient.get<Product[]>('/api/checkout/products');
    return response.data;
  } catch {
    return Object.values(staticProducts);
  }
}

/**
 * Create checkout session
 */
export async function createCheckoutSession(
  request: CreateCheckoutRequest
): Promise<CheckoutSession> {
  const response = await apiClient.post<CheckoutSession>('/api/checkout/sessions', request);
  return response.data;
}

/**
 * Get checkout session
 */
export async function getCheckoutSession(sessionId: string): Promise<CheckoutSession> {
  const response = await apiClient.get<CheckoutSession>(`/api/checkout/sessions/${sessionId}`);
  return response.data;
}

/**
 * Create payment intent (for Stripe)
 */
export async function createPaymentIntent(sessionId: string): Promise<PaymentIntent> {
  const response = await apiClient.post<PaymentIntent>(
    `/api/checkout/sessions/${sessionId}/payment-intent`
  );
  return response.data;
}

/**
 * Process payment
 */
export async function processPayment(
  request: ProcessPaymentRequest
): Promise<ProcessPaymentResponse> {
  const response = await apiClient.post<ProcessPaymentResponse>(
    '/api/checkout/process-payment',
    request
  );
  return response.data;
}

/**
 * Validate promo code
 */
export async function validatePromoCode(
  code: string,
  productId: string
): Promise<PromoCodeValidation> {
  try {
    const response = await apiClient.post<PromoCodeValidation>('/api/checkout/validate-promo', {
      code,
      productId,
    });
    return response.data;
  } catch {
    return {
      valid: false,
      discountType: 'percentage',
      discountValue: 0,
      discountAmount: 0,
      newTotal: 0,
      errorMessage: 'Código promocional no válido',
    };
  }
}

/**
 * Get checkout history
 */
export async function getCheckoutHistory(): Promise<CheckoutSession[]> {
  const response = await apiClient.get<CheckoutSession[]>('/api/checkout/history');
  return response.data;
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

/**
 * Calculate ITBIS (18% tax in DR)
 */
export function calculateTax(subtotal: number): number {
  return Math.round(subtotal * 0.18);
}

/**
 * Calculate total with tax
 */
export function calculateTotal(subtotal: number): number {
  return subtotal + calculateTax(subtotal);
}

/**
 * Format currency
 */
export function formatCheckoutCurrency(amount: number, currency: 'DOP' | 'USD' = 'DOP'): string {
  if (currency === 'USD') {
    return `$${amount.toLocaleString()}`;
  }
  return `RD$${amount.toLocaleString()}`;
}

/**
 * Get static product (for fallback)
 */
export function getStaticProduct(productId: string): Product | null {
  return staticProducts[productId] || null;
}

// =============================================================================
// EXPORT SERVICE
// =============================================================================

export const checkoutService = {
  getProduct,
  getProducts,
  createCheckoutSession,
  getCheckoutSession,
  createPaymentIntent,
  processPayment,
  validatePromoCode,
  getCheckoutHistory,
  calculateTax,
  calculateTotal,
  formatCurrency: formatCheckoutCurrency,
  getStaticProduct,
};

export default checkoutService;
