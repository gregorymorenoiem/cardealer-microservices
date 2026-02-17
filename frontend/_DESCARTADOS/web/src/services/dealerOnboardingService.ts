/**
 * Dealer Onboarding Service
 *
 * Connects to DealerOnboardingV2Controller endpoints for the new dealer registration flow.
 * Uses Azul (Banco Popular) for payments in Dominican Republic.
 *
 * Flow:
 * 1. Register → POST /api/dealer-onboarding/register
 * 2. Verify Email → POST /api/dealer-onboarding/verify-email
 * 3. Upload Documents → PUT /api/dealer-onboarding/{id}/documents
 * 4. Setup Azul IDs → PUT /api/dealer-onboarding/{id}/azul-ids
 * 5. Admin Approval → PUT /api/dealer-onboarding/{id}/approve (admin only)
 * 6. Activate → PUT /api/dealer-onboarding/{id}/activate
 */

import axios, { type AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

// ============================================================================
// TYPES
// ============================================================================

export enum OnboardingStatus {
  Registered = 'Registered',
  EmailVerified = 'EmailVerified',
  DocumentsSubmitted = 'DocumentsSubmitted',
  PendingApproval = 'PendingApproval',
  Approved = 'Approved',
  Rejected = 'Rejected',
  PaymentSetup = 'PaymentSetup',
  Active = 'Active',
}

export enum DealerType {
  INDIVIDUAL = 'Individual',
  MULTI_STORE = 'MultiStore',
  FRANCHISE = 'Franchise',
}

export interface RegisterDealerRequest {
  businessName: string;
  email: string;
  phone: string;
  rnc?: string; // Dominican Republic Tax ID
  dealerType: DealerType;
  address: string;
  city: string;
  province: string;
  country?: string;
  website?: string;
  selectedPlan: string;
}

export interface VerifyEmailRequest {
  email: string;
  verificationCode: string;
}

export interface UpdateDocumentsRequest {
  rncDocument?: File | string; // URL or File
  businessLicense?: File | string;
  identificationCard?: File | string;
  proofOfAddress?: File | string;
  bankCertificate?: File | string;
}

/**
 * Request to update Azul subscription IDs after successful payment
 *
 * IMPORTANT: This is for OKLA to receive payments FROM dealers.
 * Dealers PAY OKLA for advertising services.
 * The dealer is a CUSTOMER, not a merchant.
 */
export interface UpdateAzulIdsRequest {
  azulCustomerId: string; // ID del dealer como CLIENTE en Azul
  azulSubscriptionId?: string; // ID de la suscripción recurrente
  azulCardToken?: string; // Token de tarjeta para pagos recurrentes
  enrollEarlyBird?: boolean; // ¿Inscribir en Early Bird?
}

export interface DealerOnboardingStatus {
  id: string;
  businessName: string;
  email: string;
  status: OnboardingStatus;
  isEmailVerified: boolean;
  documentsSubmitted: boolean;
  azulConfigured: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  currentStep: number; // 1-5 based on status
  nextStepRequired: string; // What to do next
  rejectionReason?: string;
  approvedAt?: string;
}

export interface DealerOnboarding {
  id: string;
  businessName: string;
  email: string;
  phone: string;
  rnc?: string;
  dealerType: DealerType;
  address: string;
  city: string;
  province: string;
  country: string;
  website?: string;
  selectedPlan: string;
  status: OnboardingStatus;
  isEmailVerified: boolean;
  emailVerifiedAt?: string;
  documentsSubmitted: boolean;
  documentsSubmittedAt?: string;
  rncDocument?: string;
  businessLicense?: string;
  identificationCard?: string;
  proofOfAddress?: string;
  bankCertificate?: string;
  azulMerchantId?: string;
  azulTerminalId?: string;
  azulMerchantName?: string;
  isActive: boolean;
  approvedAt?: string;
  approvedBy?: string;
  rejectionReason?: string;
  createdAt: string;
  updatedAt: string;
}

// ============================================================================
// API CLIENT
// ============================================================================

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
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
// DEALER ONBOARDING API
// ============================================================================

export const dealerOnboardingApi = {
  /**
   * Step 1: Register a new dealer
   * Sends verification email automatically
   */
  register: async (data: RegisterDealerRequest): Promise<DealerOnboarding> => {
    const response = await apiClient.post<DealerOnboarding>(
      '/api/dealer-onboarding/register',
      data
    );
    return response.data;
  },

  /**
   * Step 2: Verify email with code
   */
  verifyEmail: async (data: VerifyEmailRequest): Promise<DealerOnboarding> => {
    const response = await apiClient.post<DealerOnboarding>(
      '/api/dealer-onboarding/verify-email',
      data
    );
    return response.data;
  },

  /**
   * Resend verification email
   */
  resendVerificationEmail: async (email: string): Promise<void> => {
    await apiClient.post('/api/dealer-onboarding/resend-verification', { email });
  },

  /**
   * Step 3: Upload documents
   * Supports both File objects and URLs
   */
  updateDocuments: async (
    dealerId: string,
    documents: UpdateDocumentsRequest
  ): Promise<DealerOnboarding> => {
    const formData = new FormData();

    // Add files or URLs
    Object.entries(documents).forEach(([key, value]) => {
      if (value) {
        if (value instanceof File) {
          formData.append(key, value);
        } else {
          formData.append(key, value as string);
        }
      }
    });

    const response = await apiClient.put<DealerOnboarding>(
      `/api/dealer-onboarding/${dealerId}/documents`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  /**
   * Step 4: Configure Azul payment credentials
   */
  updateAzulIds: async (
    dealerId: string,
    data: UpdateAzulIdsRequest
  ): Promise<DealerOnboarding> => {
    const response = await apiClient.put<DealerOnboarding>(
      `/api/dealer-onboarding/${dealerId}/azul-ids`,
      data
    );
    return response.data;
  },

  /**
   * Get current onboarding status
   */
  getStatus: async (dealerId: string): Promise<DealerOnboardingStatus> => {
    const response = await apiClient.get<DealerOnboarding>(
      `/api/dealer-onboarding/${dealerId}/status`
    );

    const data = response.data;

    // Calculate current step and next action
    let currentStep = 1;
    let nextStepRequired = 'Verificar email';

    if (data.isEmailVerified) {
      currentStep = 2;
      nextStepRequired = 'Subir documentos';
    }
    if (data.documentsSubmitted) {
      currentStep = 3;
      nextStepRequired = 'Configurar Azul';
    }
    if (data.azulMerchantId && data.azulTerminalId) {
      currentStep = 4;
      nextStepRequired = 'Pendiente aprobación';
    }
    if (data.status === OnboardingStatus.Approved) {
      currentStep = 5;
      nextStepRequired = 'Activar cuenta';
    }
    if (data.isActive) {
      currentStep = 5;
      nextStepRequired = '¡Completado!';
    }

    return {
      id: data.id,
      businessName: data.businessName,
      email: data.email,
      status: data.status,
      isEmailVerified: data.isEmailVerified,
      documentsSubmitted: data.documentsSubmitted,
      azulConfigured: !!(data.azulMerchantId && data.azulTerminalId),
      isActive: data.isActive,
      createdAt: data.createdAt,
      updatedAt: data.updatedAt,
      currentStep,
      nextStepRequired,
      rejectionReason: data.rejectionReason,
      approvedAt: data.approvedAt,
    };
  },

  /**
   * Get full onboarding details
   */
  getById: async (dealerId: string): Promise<DealerOnboarding> => {
    const response = await apiClient.get<DealerOnboarding>(`/api/dealer-onboarding/${dealerId}`);
    return response.data;
  },

  /**
   * Get onboarding by email
   */
  getByEmail: async (email: string): Promise<DealerOnboarding | null> => {
    try {
      const response = await apiClient.get<DealerOnboarding>(
        `/api/dealer-onboarding/email/${encodeURIComponent(email)}`
      );
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  /**
   * Step 5: Activate dealer (after admin approval)
   */
  activate: async (dealerId: string): Promise<DealerOnboarding> => {
    const response = await apiClient.put<DealerOnboarding>(
      `/api/dealer-onboarding/${dealerId}/activate`
    );
    return response.data;
  },

  // =========================================================================
  // ADMIN ENDPOINTS
  // =========================================================================

  /**
   * Get all pending onboarding applications (admin only)
   */
  getPendingApplications: async (): Promise<DealerOnboarding[]> => {
    const response = await apiClient.get<DealerOnboarding[]>('/api/dealer-onboarding/pending');
    return response.data;
  },

  /**
   * Approve dealer application (admin only)
   */
  approve: async (dealerId: string): Promise<DealerOnboarding> => {
    const response = await apiClient.put<DealerOnboarding>(
      `/api/dealer-onboarding/${dealerId}/approve`
    );
    return response.data;
  },

  /**
   * Reject dealer application (admin only)
   */
  reject: async (dealerId: string, reason: string): Promise<DealerOnboarding> => {
    const response = await apiClient.put<DealerOnboarding>(
      `/api/dealer-onboarding/${dealerId}/reject`,
      { reason }
    );
    return response.data;
  },
};

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Get step label in Spanish
 */
export const getStepLabel = (step: number): string => {
  const labels: Record<number, string> = {
    1: 'Registro',
    2: 'Verificación de Email',
    3: 'Documentos',
    4: 'Configuración de Pagos',
    5: 'Activación',
  };
  return labels[step] || 'Desconocido';
};

/**
 * Get status label in Spanish
 */
export const getStatusLabel = (status: OnboardingStatus): string => {
  const labels: Record<OnboardingStatus, string> = {
    [OnboardingStatus.Registered]: 'Registrado',
    [OnboardingStatus.EmailVerified]: 'Email Verificado',
    [OnboardingStatus.DocumentsSubmitted]: 'Documentos Enviados',
    [OnboardingStatus.PendingApproval]: 'Pendiente de Aprobación',
    [OnboardingStatus.Approved]: 'Aprobado',
    [OnboardingStatus.Rejected]: 'Rechazado',
    [OnboardingStatus.PaymentSetup]: 'Pagos Configurados',
    [OnboardingStatus.Active]: 'Activo',
  };
  return labels[status] || status;
};

/**
 * Get status color for badges
 */
export const getStatusColor = (
  status: OnboardingStatus
): 'green' | 'yellow' | 'red' | 'blue' | 'gray' => {
  const colors: Record<OnboardingStatus, 'green' | 'yellow' | 'red' | 'blue' | 'gray'> = {
    [OnboardingStatus.Registered]: 'blue',
    [OnboardingStatus.EmailVerified]: 'blue',
    [OnboardingStatus.DocumentsSubmitted]: 'yellow',
    [OnboardingStatus.PendingApproval]: 'yellow',
    [OnboardingStatus.Approved]: 'green',
    [OnboardingStatus.Rejected]: 'red',
    [OnboardingStatus.PaymentSetup]: 'green',
    [OnboardingStatus.Active]: 'green',
  };
  return colors[status] || 'gray';
};

/**
 * Check if user can proceed to next step
 */
export const canProceedToNextStep = (status: DealerOnboardingStatus): boolean => {
  if (status.status === OnboardingStatus.Rejected) return false;
  if (status.isActive) return false; // Already complete
  return true;
};

/**
 * Get required documents list for Dominican Republic
 */
export const getRequiredDocuments = (): {
  key: keyof UpdateDocumentsRequest;
  label: string;
  required: boolean;
}[] => {
  return [
    { key: 'rncDocument', label: 'Certificación del RNC (DGII)', required: true },
    { key: 'businessLicense', label: 'Registro Mercantil', required: true },
    { key: 'identificationCard', label: 'Cédula del Representante Legal', required: true },
    { key: 'proofOfAddress', label: 'Comprobante de Dirección', required: false },
    { key: 'bankCertificate', label: 'Certificación Bancaria', required: false },
  ];
};

export default dealerOnboardingApi;
