/**
 * AZUL Payment Gateway Service
 *
 * Service for interacting with AZUL Payment Page (Banco Popular RD)
 * Handles payment initiation and transaction verification
 */

import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

export interface AzulInitiatePaymentRequest {
  amount: number;
  itbis: number;
  orderNumber: string;
  description?: string;
}

export interface AzulInitiatePaymentResponse {
  paymentPageUrl: string;
  formFields: {
    MerchantId: string;
    MerchantName: string;
    MerchantType: string;
    CurrencyCode: string;
    OrderNumber: string;
    Amount: string;
    ITBIS: string;
    ApprovedUrl: string;
    DeclinedUrl: string;
    CancelUrl: string;
    UseCustomField1: string;
    UseCustomField2: string;
    AuthHash: string;
  };
}

export interface AzulCallbackData {
  OrderNumber: string;
  Amount: string;
  AuthorizationCode?: string;
  DateTime: string;
  ResponseCode: string;
  ResponseMessage: string;
  IsoCode?: string;
  AcquirerRefData?: string;
  RRN?: string;
  AuthHash: string;
}

export interface AzulTransaction {
  id: string;
  orderNumber: string;
  merchantId: string;
  amount: number;
  itbis: number;
  status: 'Approved' | 'Declined' | 'Cancelled' | 'Pending' | 'Error';
  responseCode: string;
  responseMessage: string;
  authorizationCode?: string;
  isoCode?: string;
  acquirerRefData?: string;
  rrn?: string;
  transactionDateTime: string;
  createdAt: string;
  updatedAt: string;
}

class AzulService {
  /**
   * Initiate AZUL payment
   * Returns Payment Page URL and form fields for redirect
   */
  async initiatePayment(request: AzulInitiatePaymentRequest): Promise<AzulInitiatePaymentResponse> {
    try {
      const response = await axios.post<AzulInitiatePaymentResponse>(
        `${API_BASE_URL}/api/payment/azul/initiate`,
        request,
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      return response.data;
    } catch (error) {
      console.error('Error initiating AZUL payment:', error);

      if (axios.isAxiosError(error)) {
        const message = error.response?.data?.message || error.message;
        throw new Error(`AZUL Payment Error: ${message}`);
      }

      throw new Error('Failed to initiate AZUL payment. Please try again.');
    }
  }

  /**
   * Get transaction by order number
   */
  async getTransaction(orderNumber: string): Promise<AzulTransaction> {
    try {
      const response = await axios.get<AzulTransaction>(
        `${API_BASE_URL}/api/payment/azul/transaction/${orderNumber}`
      );

      return response.data;
    } catch (error) {
      console.error('Error fetching AZUL transaction:', error);
      throw new Error('Failed to fetch transaction details');
    }
  }

  /**
   * Calculate ITBIS (18% tax in Dominican Republic)
   */
  calculateITBIS(amount: number): number {
    return Math.round(amount * 0.18 * 100) / 100;
  }

  /**
   * Format amount for display (DOP currency)
   */
  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
    }).format(amount);
  }

  /**
   * Submit AZUL payment form programmatically
   * Creates a hidden form and submits it to AZUL Payment Page
   */
  submitAzulForm(data: AzulInitiatePaymentResponse): void {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = data.paymentPageUrl;

    // Add all form fields
    Object.entries(data.formFields).forEach(([key, value]) => {
      const input = document.createElement('input');
      input.type = 'hidden';
      input.name = key;
      input.value = value;
      form.appendChild(input);
    });

    // Add form to body and submit
    document.body.appendChild(form);
    form.submit();
  }
}

export const azulService = new AzulService();
