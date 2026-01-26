/**
 * Privacy Service - Gestión de Derechos ARCO (Ley 172-13)
 *
 * Endpoints para:
 * - Acceso: Ver datos personales
 * - Rectificación: Corregir datos (manejado por userProfileService)
 * - Cancelación: Eliminar cuenta
 * - Oposición: Gestionar preferencias de comunicación
 * - Portabilidad: Exportar datos
 *
 * @module services/privacyService
 * @version 1.0.0
 * @since Enero 26, 2026
 */

import api from './api';

// ============================================================================
// Types & Interfaces
// ============================================================================

export type ExportFormat = 'Json' | 'Csv' | 'Pdf';

export type DeletionReason =
  | 'PrivacyConcerns'
  | 'NoLongerNeeded'
  | 'FoundAlternative'
  | 'BadExperience'
  | 'TooManyEmails'
  | 'Other';

export type PrivacyRequestStatus =
  | 'Pending'
  | 'Processing'
  | 'Completed'
  | 'Cancelled'
  | 'Rejected'
  | 'Expired';

// Data Summary Types
export interface ProfileSummary {
  fullName: string;
  email: string;
  phone?: string;
  city?: string;
  province?: string;
  accountType: string;
  memberSince: string;
  emailVerified: boolean;
}

export interface ActivitySummary {
  totalSearches: number;
  totalVehicleViews: number;
  totalFavorites: number;
  totalAlerts: number;
  totalMessages: number;
  lastActivity?: string;
}

export interface TransactionsSummary {
  totalPayments: number;
  totalSpent: number;
  activeSubscription?: string;
  totalInvoices: number;
}

export interface PrivacySettingsSummary {
  marketingOptIn: boolean;
  analyticsOptIn: boolean;
  thirdPartyOptIn: boolean;
  lastUpdated: string;
}

export interface UserDataSummary {
  profile: ProfileSummary;
  activity: ActivitySummary;
  transactions: TransactionsSummary;
  privacy: PrivacySettingsSummary;
  generatedAt: string;
}

// Export Types
export interface RequestDataExportDto {
  format: ExportFormat;
  includeProfile?: boolean;
  includeActivity?: boolean;
  includeMessages?: boolean;
  includeFavorites?: boolean;
  includeTransactions?: boolean;
}

export interface DataExportRequestResponse {
  requestId: string;
  status: string;
  message: string;
  estimatedCompletionTime?: string;
}

export interface DataExportStatus {
  requestId: string;
  status: string;
  requestedAt: string;
  readyAt?: string;
  expiresAt?: string;
  downloadToken?: string;
  fileSize?: string;
  format: string;
}

// Account Deletion Types
export interface RequestAccountDeletionDto {
  reason: DeletionReason;
  otherReason?: string;
  feedback?: string;
}

export interface ConfirmAccountDeletionDto {
  confirmationCode: string;
  password: string;
}

export interface AccountDeletionResponse {
  requestId: string;
  status: string;
  message: string;
  gracePeriodEndsAt: string;
  confirmationEmailSentTo: string;
}

export interface AccountDeletionStatus {
  requestId: string;
  status: string;
  requestedAt: string;
  gracePeriodEndsAt: string;
  canCancel: boolean;
  daysRemaining: number;
  reason?: string;
}

// Communication Preferences Types
export interface EmailPreferences {
  activityNotifications: boolean;
  listingUpdates: boolean;
  newsletter: boolean;
  promotions: boolean;
  priceAlerts: boolean;
}

export interface SmsPreferences {
  verificationCodes: boolean; // Read-only, always true
  priceAlerts: boolean;
  promotions: boolean;
}

export interface PushPreferences {
  newMessages: boolean;
  priceChanges: boolean;
  recommendations: boolean;
}

export interface PrivacyPreferences {
  allowProfiling: boolean;
  allowThirdPartySharing: boolean;
  allowAnalytics: boolean;
  allowRetargeting: boolean;
}

export interface CommunicationPreferences {
  email: EmailPreferences;
  sms: SmsPreferences;
  push: PushPreferences;
  privacy: PrivacyPreferences;
  lastUpdated: string;
}

export interface UpdateCommunicationPreferencesDto {
  // Email
  emailActivityNotifications?: boolean;
  emailListingUpdates?: boolean;
  emailNewsletter?: boolean;
  emailPromotions?: boolean;
  emailPriceAlerts?: boolean;
  // SMS
  smsPriceAlerts?: boolean;
  smsPromotions?: boolean;
  // Push
  pushNewMessages?: boolean;
  pushPriceChanges?: boolean;
  pushRecommendations?: boolean;
  // Privacy
  allowProfiling?: boolean;
  allowThirdPartySharing?: boolean;
  allowAnalytics?: boolean;
  allowRetargeting?: boolean;
}

// Request History Types
export interface PrivacyRequestHistory {
  id: string;
  type: string;
  status: PrivacyRequestStatus;
  description: string;
  createdAt: string;
  completedAt?: string;
}

export interface PrivacyRequestsList {
  requests: PrivacyRequestHistory[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ARCO Rights Info Types
export interface ARCORight {
  name: string;
  description: string;
  endpoint: string;
}

export interface ARCORightsInfo {
  law: string;
  rights: ARCORight[];
  contact: {
    email: string;
    phone: string;
    address: string;
  };
  responseTimes: {
    access: string;
    rectification: string;
    cancellation: string;
    opposition: string;
    portability: string;
  };
}

// ============================================================================
// API Functions
// ============================================================================

const BASE_URL = '/api/privacy';

/**
 * Servicio de Privacidad ARCO
 */
export const privacyService = {
  // -------------------------------------------------------------------------
  // Derecho de Acceso
  // -------------------------------------------------------------------------

  /**
   * Obtener resumen de datos del usuario
   */
  async getMyDataSummary(): Promise<UserDataSummary> {
    const response = await api.get<UserDataSummary>(`${BASE_URL}/my-data`);
    return response.data;
  },

  /**
   * Obtener todos los datos del usuario (exportación completa en pantalla)
   */
  async getMyFullData(): Promise<Record<string, unknown>> {
    const response = await api.get(`${BASE_URL}/my-data/full`);
    return response.data;
  },

  // -------------------------------------------------------------------------
  // Derecho de Portabilidad
  // -------------------------------------------------------------------------

  /**
   * Solicitar exportación de datos
   */
  async requestDataExport(options: RequestDataExportDto): Promise<DataExportRequestResponse> {
    const response = await api.post<DataExportRequestResponse>(
      `${BASE_URL}/export/request`,
      options
    );
    return response.data;
  },

  /**
   * Obtener estado de exportación
   */
  async getExportStatus(): Promise<DataExportStatus | null> {
    try {
      const response = await api.get<DataExportStatus>(`${BASE_URL}/export/status`);
      return response.data;
    } catch {
      return null;
    }
  },

  /**
   * Obtener URL de descarga de exportación
   */
  getExportDownloadUrl(token: string): string {
    return `${BASE_URL}/export/download/${token}`;
  },

  /**
   * Descargar archivo de exportación
   */
  async downloadExport(token: string): Promise<Blob> {
    const response = await api.get(`${BASE_URL}/export/download/${token}`, {
      responseType: 'blob',
    });
    return response.data;
  },

  // -------------------------------------------------------------------------
  // Derecho de Cancelación
  // -------------------------------------------------------------------------

  /**
   * Solicitar eliminación de cuenta
   */
  async requestAccountDeletion(
    request: RequestAccountDeletionDto
  ): Promise<AccountDeletionResponse> {
    const response = await api.post<AccountDeletionResponse>(
      `${BASE_URL}/delete-account/request`,
      request
    );
    return response.data;
  },

  /**
   * Confirmar eliminación de cuenta
   */
  async confirmAccountDeletion(request: ConfirmAccountDeletionDto): Promise<{ message: string }> {
    const response = await api.post<{ message: string }>(
      `${BASE_URL}/delete-account/confirm`,
      request
    );
    return response.data;
  },

  /**
   * Cancelar solicitud de eliminación
   */
  async cancelAccountDeletion(): Promise<{ message: string }> {
    const response = await api.post<{ message: string }>(`${BASE_URL}/delete-account/cancel`);
    return response.data;
  },

  /**
   * Obtener estado de solicitud de eliminación
   */
  async getAccountDeletionStatus(): Promise<AccountDeletionStatus | null> {
    try {
      const response = await api.get<AccountDeletionStatus>(`${BASE_URL}/delete-account/status`);
      return response.data;
    } catch {
      return null;
    }
  },

  // -------------------------------------------------------------------------
  // Derecho de Oposición
  // -------------------------------------------------------------------------

  /**
   * Obtener preferencias de comunicación
   */
  async getCommunicationPreferences(): Promise<CommunicationPreferences> {
    const response = await api.get<CommunicationPreferences>(`${BASE_URL}/preferences`);
    return response.data;
  },

  /**
   * Actualizar preferencias de comunicación
   */
  async updateCommunicationPreferences(
    preferences: UpdateCommunicationPreferencesDto
  ): Promise<CommunicationPreferences> {
    const response = await api.put<CommunicationPreferences>(
      `${BASE_URL}/preferences`,
      preferences
    );
    return response.data;
  },

  /**
   * Darse de baja de todo el marketing
   */
  async unsubscribeFromAllMarketing(): Promise<{ message: string }> {
    const response = await api.post<{ message: string }>(`${BASE_URL}/preferences/unsubscribe-all`);
    return response.data;
  },

  // -------------------------------------------------------------------------
  // Historial de Solicitudes
  // -------------------------------------------------------------------------

  /**
   * Obtener historial de solicitudes ARCO
   */
  async getPrivacyRequestHistory(page = 1, pageSize = 10): Promise<PrivacyRequestsList> {
    const response = await api.get<PrivacyRequestsList>(`${BASE_URL}/requests`, {
      params: { page, pageSize },
    });
    return response.data;
  },

  // -------------------------------------------------------------------------
  // Información Legal
  // -------------------------------------------------------------------------

  /**
   * Obtener información sobre derechos ARCO (público)
   */
  async getARCORightsInfo(): Promise<ARCORightsInfo> {
    const response = await api.get<ARCORightsInfo>(`${BASE_URL}/rights-info`);
    return response.data;
  },
};

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Mapeo de razones de eliminación para UI
 */
export const deletionReasonLabels: Record<DeletionReason, string> = {
  PrivacyConcerns: 'Preocupaciones de privacidad',
  NoLongerNeeded: 'Ya no necesito el servicio',
  FoundAlternative: 'Encontré una alternativa',
  BadExperience: 'Tuve una mala experiencia',
  TooManyEmails: 'Recibo demasiados emails',
  Other: 'Otra razón',
};

/**
 * Mapeo de estados de solicitud para UI
 */
export const requestStatusLabels: Record<PrivacyRequestStatus, string> = {
  Pending: 'Pendiente',
  Processing: 'Procesando',
  Completed: 'Completado',
  Cancelled: 'Cancelado',
  Rejected: 'Rechazado',
  Expired: 'Expirado',
};

/**
 * Colores para badges de estado
 */
export const requestStatusColors: Record<PrivacyRequestStatus, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Processing: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Cancelled: 'bg-gray-100 text-gray-800',
  Rejected: 'bg-red-100 text-red-800',
  Expired: 'bg-gray-100 text-gray-500',
};

/**
 * Formatear bytes a tamaño legible
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * Calcular días restantes hasta una fecha
 */
export function daysUntil(dateString: string): number {
  const target = new Date(dateString);
  const now = new Date();
  const diff = target.getTime() - now.getTime();
  return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
}

export default privacyService;
