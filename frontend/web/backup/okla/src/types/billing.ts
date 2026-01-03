// Billing Domain Types

export type SubscriptionPlan = 'free' | 'basic' | 'professional' | 'enterprise' | 'custom';
export type SubscriptionStatus = 'trial' | 'active' | 'past_due' | 'cancelled' | 'suspended' | 'expired';
export type BillingCycle = 'monthly' | 'quarterly' | 'yearly';
export type InvoiceStatus = 'draft' | 'issued' | 'sent' | 'paid' | 'partially_paid' | 'overdue' | 'cancelled' | 'voided';
export type PaymentStatus = 'pending' | 'processing' | 'succeeded' | 'failed' | 'refunded' | 'partially_refunded' | 'disputed';
export type PaymentMethod = 'credit_card' | 'debit_card' | 'bank_transfer' | 'cash' | 'check' | 'other';

export interface Subscription {
  id: string;
  dealerId: string;
  plan: SubscriptionPlan;
  status: SubscriptionStatus;
  cycle: BillingCycle;
  pricePerCycle: number;
  currency: string;
  startDate: string;
  endDate?: string;
  trialEndDate?: string;
  nextBillingDate?: string;
  maxUsers: number;
  maxVehicles: number;
  features: PlanFeatures;
  stripeCustomerId?: string;
  stripeSubscriptionId?: string;
  createdAt: string;
  updatedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
}

export interface PlanFeatures {
  listings: number | 'unlimited';
  users: number | 'unlimited';
  storage: string;
  analytics: boolean;
  api: boolean;
  customBranding: boolean;
  prioritySupport: boolean;
  dedicatedManager: boolean;
  bulkUpload: boolean;
  marketplace: boolean;
  realEstate: boolean;
  crm: boolean;
  reporting: boolean;
}

export interface Invoice {
  id: string;
  dealerId: string;
  invoiceNumber: string;
  subscriptionId?: string;
  status: InvoiceStatus;
  subtotal: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  currency: string;
  issueDate: string;
  dueDate: string;
  paidDate?: string;
  pdfUrl?: string;
  notes?: string;
  lineItems: InvoiceLineItem[];
}

export interface InvoiceLineItem {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
}

export interface Payment {
  id: string;
  dealerId: string;
  subscriptionId?: string;
  invoiceId?: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  method: PaymentMethod;
  description?: string;
  receiptUrl?: string;
  failureReason?: string;
  refundReason?: string;
  refundedAmount: number;
  createdAt: string;
  processedAt?: string;
  refundedAt?: string;
  cardLast4?: string;
  cardBrand?: string;
}

export interface PaymentMethodInfo {
  id: string;
  type: 'card' | 'bank_account';
  isDefault: boolean;
  card?: {
    brand: string;
    last4: string;
    expMonth: number;
    expYear: number;
  };
  bankAccount?: {
    bankName: string;
    last4: string;
    accountType: string;
  };
  createdAt: string;
}

export interface UsageMetrics {
  currentListings: number;
  maxListings: number | 'unlimited';
  currentUsers: number;
  maxUsers: number | 'unlimited';
  storageUsed: string;
  storageLimit: string;
  apiCalls: number;
  apiLimit: number | 'unlimited';
}

export interface BillingStats {
  currentPlan: SubscriptionPlan;
  monthlySpend: number;
  yearlySpend: number;
  outstandingBalance: number;
  nextBillingAmount: number;
  nextBillingDate: string;
  totalPaid: number;
  invoiceCount: number;
}

// Plan configuration
export interface PlanConfig {
  id: SubscriptionPlan;
  name: string;
  description: string;
  prices: {
    monthly: number;
    quarterly: number;
    yearly: number;
  };
  features: PlanFeatures;
  popular?: boolean;
  enterprise?: boolean;
}

// API DTOs
export interface CreateSubscriptionRequest {
  plan: SubscriptionPlan;
  cycle: BillingCycle;
  paymentMethodId?: string;
  trialDays?: number;
}

export interface UpdateSubscriptionRequest {
  plan?: SubscriptionPlan;
  cycle?: BillingCycle;
}

export interface CreatePaymentRequest {
  amount: number;
  method: PaymentMethod;
  invoiceId?: string;
  description?: string;
}

export interface RefundRequest {
  amount: number;
  reason: string;
}
