/**
 * Dealer Settings Service
 *
 * Manages dealer settings, team members, notifications, security, and billing
 */

import { api } from './api';

// ============================================================================
// TYPES
// ============================================================================

// Team Members
export interface DealerEmployee {
  id: string;
  userId: string;
  dealerId: string;
  name: string;
  email: string;
  role: DealerRole;
  status: EmployeeStatus;
  invitationDate: string;
  activationDate?: string;
  avatarUrl?: string;
}

export interface DealerEmployeeInvitation {
  id: string;
  email: string;
  role: string;
  status: string;
  invitationDate: string;
  expirationDate: string;
}

export type DealerRole =
  | 'Owner'
  | 'Admin'
  | 'SalesManager'
  | 'Salesperson'
  | 'InventoryManager'
  | 'Viewer';
export type EmployeeStatus = 'Pending' | 'Active' | 'Suspended';

export interface RoleDefinition {
  id: string;
  name: string;
  description: string;
  permissions: string[];
}

export interface InviteEmployeeRequest {
  email: string;
  role: DealerRole;
  permissions?: string;
  invitedBy: string;
}

export interface UpdateRoleRequest {
  role: DealerRole;
  permissions?: string[];
}

// Profile
export interface DealerProfile {
  id: string;
  businessName: string;
  rnc: string;
  legalName?: string;
  tradeName?: string;
  email: string;
  phone: string;
  mobilePhone?: string;
  website?: string;
  address: string;
  city: string;
  province: string;
  zipCode?: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  workingHours?: string;
}

export interface UpdateProfileRequest {
  businessName?: string;
  legalName?: string;
  tradeName?: string;
  email?: string;
  phone?: string;
  mobilePhone?: string;
  website?: string;
  address?: string;
  city?: string;
  province?: string;
  zipCode?: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  workingHours?: string;
}

// Notifications
export interface NotificationPreference {
  id: string;
  type: NotificationType;
  title: string;
  description: string;
  enabled: boolean;
  channels: NotificationChannel[];
}

export type NotificationType =
  | 'new_leads'
  | 'vehicle_inquiries'
  | 'daily_summary'
  | 'inventory_alerts'
  | 'system_updates'
  | 'payment_reminders'
  | 'marketing';

export type NotificationChannel = 'email' | 'push' | 'sms' | 'whatsapp';

export interface UpdateNotificationPreferenceRequest {
  type: NotificationType;
  enabled: boolean;
  channels?: NotificationChannel[];
}

// Security
export interface SecuritySettings {
  twoFactorEnabled: boolean;
  lastPasswordChange?: string;
  activeSessions: ActiveSession[];
}

export interface ActiveSession {
  id: string;
  device: string;
  browser: string;
  location: string;
  lastActive: string;
  isCurrent: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// Billing / Payment Methods
export interface PaymentMethod {
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

export interface AddPaymentMethodRequest {
  type: 'card' | 'bank_account';
  // For card
  cardNumber?: string;
  expMonth?: number;
  expYear?: number;
  cvv?: string;
  // For bank
  accountNumber?: string;
  routingNumber?: string;
  accountType?: string;
}

export interface BillingInfo {
  currentPlan: string;
  planDisplayName: string;
  monthlyPrice: number;
  nextBillingDate?: string;
  paymentMethods: PaymentMethod[];
  billingAddress?: {
    line1: string;
    line2?: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
  };
}

// ============================================================================
// API SERVICE
// ============================================================================

export const dealerSettingsApi = {
  // ========================================
  // PROFILE
  // ========================================

  /**
   * Get dealer profile
   */
  getProfile: async (dealerId: string): Promise<DealerProfile> => {
    const response = await api.get(`/api/dealers/${dealerId}`);
    return mapDealerProfile(response.data);
  },

  /**
   * Update dealer profile
   */
  updateProfile: async (dealerId: string, data: UpdateProfileRequest): Promise<DealerProfile> => {
    const response = await api.put(`/api/dealers/${dealerId}`, data);
    return mapDealerProfile(response.data);
  },

  /**
   * Upload dealer logo
   */
  uploadLogo: async (dealerId: string, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('folder', `dealers/${dealerId}`);
    formData.append('purpose', 'logo');

    const response = await api.post('/api/media/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });

    return response.data.url;
  },

  // ========================================
  // TEAM MEMBERS (EMPLOYEES)
  // ========================================

  /**
   * Get all team members
   */
  getTeamMembers: async (dealerId: string): Promise<DealerEmployee[]> => {
    const response = await api.get(`/api/dealers/${dealerId}/employees`);
    return (response.data || []).map(mapEmployee);
  },

  /**
   * Get single team member
   */
  getTeamMember: async (dealerId: string, employeeId: string): Promise<DealerEmployee> => {
    const response = await api.get(`/api/dealers/${dealerId}/employees/${employeeId}`);
    return mapEmployee(response.data);
  },

  /**
   * Invite a new team member
   */
  inviteTeamMember: async (
    dealerId: string,
    request: InviteEmployeeRequest
  ): Promise<DealerEmployeeInvitation> => {
    const response = await api.post(`/api/dealers/${dealerId}/employees/invite`, request);
    return response.data;
  },

  /**
   * Update team member role
   */
  updateTeamMemberRole: async (
    dealerId: string,
    employeeId: string,
    request: UpdateRoleRequest
  ): Promise<DealerEmployee> => {
    const response = await api.put(
      `/api/dealers/${dealerId}/employees/${employeeId}/role`,
      request
    );
    return mapEmployee(response.data);
  },

  /**
   * Remove team member
   */
  removeTeamMember: async (dealerId: string, employeeId: string): Promise<void> => {
    await api.delete(`/api/dealers/${dealerId}/employees/${employeeId}`);
  },

  /**
   * Get pending invitations
   */
  getInvitations: async (dealerId: string): Promise<DealerEmployeeInvitation[]> => {
    const response = await api.get(`/api/dealers/${dealerId}/employees/invitations`);
    return response.data || [];
  },

  /**
   * Cancel invitation
   */
  cancelInvitation: async (dealerId: string, invitationId: string): Promise<void> => {
    await api.delete(`/api/dealers/${dealerId}/employees/invitations/${invitationId}`);
  },

  /**
   * Get available roles
   */
  getAvailableRoles: async (): Promise<RoleDefinition[]> => {
    const response = await api.get('/api/dealer-roles');
    return response.data || [];
  },

  // ========================================
  // NOTIFICATIONS
  // ========================================

  /**
   * Get notification preferences
   */
  getNotificationPreferences: async (dealerId: string): Promise<NotificationPreference[]> => {
    try {
      const response = await api.get(`/api/notifications/preferences`, {
        headers: { 'X-Dealer-Id': dealerId },
      });
      return response.data || getDefaultNotificationPreferences();
    } catch {
      return getDefaultNotificationPreferences();
    }
  },

  /**
   * Update notification preference
   */
  updateNotificationPreference: async (
    dealerId: string,
    request: UpdateNotificationPreferenceRequest
  ): Promise<NotificationPreference> => {
    const response = await api.put(`/api/notifications/preferences/${request.type}`, request, {
      headers: { 'X-Dealer-Id': dealerId },
    });
    return response.data;
  },

  // ========================================
  // SECURITY
  // ========================================

  /**
   * Get security settings
   */
  getSecuritySettings: async (userId: string): Promise<SecuritySettings> => {
    try {
      const response = await api.get(`/api/auth/security`, {
        headers: { 'X-User-Id': userId },
      });
      return response.data;
    } catch {
      return {
        twoFactorEnabled: false,
        activeSessions: [],
      };
    }
  },

  /**
   * Change password
   */
  changePassword: async (
    request: ChangePasswordRequest
  ): Promise<{ success: boolean; message: string }> => {
    const response = await api.post('/api/auth/change-password', request);
    return response.data;
  },

  /**
   * Enable two-factor authentication
   */
  enableTwoFactor: async (): Promise<{ qrCode: string; secret: string }> => {
    const response = await api.post('/api/auth/2fa/enable');
    return response.data;
  },

  /**
   * Disable two-factor authentication
   */
  disableTwoFactor: async (code: string): Promise<void> => {
    await api.post('/api/auth/2fa/disable', { code });
  },

  /**
   * Revoke session
   */
  revokeSession: async (sessionId: string): Promise<void> => {
    await api.delete(`/api/auth/sessions/${sessionId}`);
  },

  /**
   * Revoke all sessions
   */
  revokeAllSessions: async (): Promise<void> => {
    await api.post('/api/auth/sessions/revoke-all');
  },

  // ========================================
  // BILLING / PAYMENT METHODS
  // ========================================

  /**
   * Get billing info
   */
  getBillingInfo: async (dealerId: string): Promise<BillingInfo> => {
    const response = await api.get(`/api/dealer-billing/dashboard/${dealerId}`);
    const data = response.data;

    return {
      currentPlan: data.summary?.currentSubscription?.plan || 'free',
      planDisplayName: getPlanDisplayName(data.summary?.currentSubscription?.plan || 'free'),
      monthlyPrice: data.summary?.currentSubscription?.pricePerCycle || 0,
      nextBillingDate: data.summary?.currentSubscription?.nextBillingDate,
      paymentMethods: (data.paymentMethods || []).map(mapPaymentMethod),
      billingAddress: data.billingAddress,
    };
  },

  /**
   * Get payment methods
   */
  getPaymentMethods: async (dealerId: string): Promise<PaymentMethod[]> => {
    const response = await api.get('/api/dealer-billing/payment-methods', {
      headers: { 'X-Dealer-Id': dealerId },
    });
    return (response.data || []).map(mapPaymentMethod);
  },

  /**
   * Add payment method (for Azul/card)
   */
  addPaymentMethod: async (
    dealerId: string,
    _request: AddPaymentMethodRequest
  ): Promise<PaymentMethod> => {
    // Note: For Azul integration, this would redirect to Azul's payment page
    // For now, we'll call the billing service
    const response = await api.post('/api/dealer-billing/payment-methods', {
      dealerId,
      // Azul will handle the actual card tokenization
    });
    return mapPaymentMethod(response.data);
  },

  /**
   * Set default payment method
   */
  setDefaultPaymentMethod: async (dealerId: string, paymentMethodId: string): Promise<void> => {
    await api.put(
      `/api/dealer-billing/payment-methods/${paymentMethodId}/default`,
      {},
      {
        headers: { 'X-Dealer-Id': dealerId },
      }
    );
  },

  /**
   * Remove payment method
   */
  removePaymentMethod: async (dealerId: string, paymentMethodId: string): Promise<void> => {
    await api.delete(`/api/dealer-billing/payment-methods/${paymentMethodId}`, {
      headers: { 'X-Dealer-Id': dealerId },
    });
  },

  /**
   * Initialize Azul payment page (for adding new card)
   */
  initAzulPaymentPage: async (
    dealerId: string
  ): Promise<{ redirectUrl: string; sessionId: string }> => {
    const response = await api.post('/api/azul/payment-page/init', {
      dealerId,
      purpose: 'add_payment_method',
      returnUrl: `${window.location.origin}/dealer/settings?tab=billing&azul=success`,
      cancelUrl: `${window.location.origin}/dealer/settings?tab=billing&azul=cancel`,
    });
    return response.data;
  },
};

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

function mapDealerProfile(data: any): DealerProfile {
  return {
    id: data.id || '',
    businessName: data.businessName || '',
    rnc: data.rnc || '',
    legalName: data.legalName,
    tradeName: data.tradeName,
    email: data.email || '',
    phone: data.phone || '',
    mobilePhone: data.mobilePhone,
    website: data.website,
    address: data.address || '',
    city: data.city || '',
    province: data.province || '',
    zipCode: data.zipCode,
    description: data.description,
    logoUrl: data.logoUrl,
    bannerUrl: data.bannerUrl,
    workingHours: data.workingHours,
  };
}

function mapEmployee(data: any): DealerEmployee {
  return {
    id: data.id || '',
    userId: data.userId || '',
    dealerId: data.dealerId || '',
    name: data.name || 'Sin nombre',
    email: data.email || '',
    role: data.role || 'Viewer',
    status: data.status || 'Pending',
    invitationDate: data.invitationDate || new Date().toISOString(),
    activationDate: data.activationDate,
    avatarUrl: data.avatarUrl,
  };
}

function mapPaymentMethod(data: any): PaymentMethod {
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

function getPlanDisplayName(plan: string | number): string {
  // Handle numeric enum values from backend
  const numericNames: Record<number, string> = {
    0: 'Gratis',
    1: 'Básico',
    2: 'Profesional',
    3: 'Enterprise',
  };

  // Handle string values
  const stringNames: Record<string, string> = {
    free: 'Gratis',
    basic: 'Básico',
    starter: 'Starter',
    pro: 'Pro',
    professional: 'Profesional',
    enterprise: 'Enterprise',
  };

  if (typeof plan === 'number') {
    return numericNames[plan] || `Plan ${plan}`;
  }

  if (typeof plan === 'string') {
    return stringNames[plan.toLowerCase()] || plan;
  }

  return 'Gratis';
}

function getDefaultNotificationPreferences(): NotificationPreference[] {
  return [
    {
      id: '1',
      type: 'new_leads',
      title: 'Nuevos leads',
      description: 'Recibe notificaciones cuando llegue un nuevo lead',
      enabled: true,
      channels: ['email', 'push'],
    },
    {
      id: '2',
      type: 'vehicle_inquiries',
      title: 'Consultas de vehículos',
      description: 'Notificaciones de consultas sobre tus vehículos',
      enabled: true,
      channels: ['email'],
    },
    {
      id: '3',
      type: 'daily_summary',
      title: 'Resumen diario',
      description: 'Recibe un resumen de actividad cada día',
      enabled: false,
      channels: ['email'],
    },
    {
      id: '4',
      type: 'inventory_alerts',
      title: 'Alertas de inventario',
      description: 'Cuando un vehículo lleve mucho tiempo sin vistas',
      enabled: true,
      channels: ['email'],
    },
    {
      id: '5',
      type: 'system_updates',
      title: 'Actualizaciones del sistema',
      description: 'Novedades y mejoras de la plataforma',
      enabled: false,
      channels: ['email'],
    },
  ];
}

export default dealerSettingsApi;
