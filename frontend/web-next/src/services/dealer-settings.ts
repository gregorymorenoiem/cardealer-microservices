/**
 * Dealer Settings Service
 *
 * Frontend service for dealer settings operations
 */

import { apiClient } from '@/lib/api-client';

// ============================================================================
// Types
// ============================================================================

export interface NotificationSettings {
  // Email notifications
  emailNewLead: boolean;
  emailMessages: boolean;
  emailAppointments: boolean;
  emailWeeklyReport: boolean;

  // SMS notifications
  smsNewLead: boolean;
  smsAppointments: boolean;

  // Push notifications
  pushMessages: boolean;
  pushLeads: boolean;
}

export interface SecuritySettings {
  twoFactorEnabled: boolean;
  sessionTimeoutMinutes: number;
  lastPasswordChange?: string;
}

export interface DealerSettings {
  dealerId: string;
  notifications: NotificationSettings;
  security: SecuritySettings;
  updatedAt: string;
}

export const defaultNotificationSettings: NotificationSettings = {
  emailNewLead: true,
  emailMessages: true,
  emailAppointments: true,
  emailWeeklyReport: true,
  smsNewLead: false,
  smsAppointments: true,
  pushMessages: true,
  pushLeads: true,
};

export const defaultSecuritySettings: SecuritySettings = {
  twoFactorEnabled: false,
  sessionTimeoutMinutes: 30,
};

// ============================================================================
// API Functions
// ============================================================================

/**
 * Get dealer settings
 */
export async function getDealerSettings(dealerId: string): Promise<DealerSettings> {
  try {
    const response = await apiClient.get<DealerSettings>(`/api/dealers/${dealerId}/settings`);
    return response.data;
  } catch {
    // Return defaults if settings don't exist
    return {
      dealerId,
      notifications: defaultNotificationSettings,
      security: defaultSecuritySettings,
      updatedAt: new Date().toISOString(),
    };
  }
}

/**
 * Update notification settings
 */
export async function updateNotificationSettings(
  dealerId: string,
  settings: NotificationSettings
): Promise<NotificationSettings> {
  const response = await apiClient.put<NotificationSettings>(
    `/api/dealers/${dealerId}/settings/notifications`,
    settings
  );
  return response.data;
}

/**
 * Update security settings
 */
export async function updateSecuritySettings(
  dealerId: string,
  settings: Partial<SecuritySettings>
): Promise<SecuritySettings> {
  const response = await apiClient.put<SecuritySettings>(
    `/api/dealers/${dealerId}/settings/security`,
    settings
  );
  return response.data;
}

/**
 * Enable two-factor authentication
 */
export async function enableTwoFactor(
  dealerId: string
): Promise<{ qrCodeUrl: string; secret: string }> {
  const response = await apiClient.post<{ qrCodeUrl: string; secret: string }>(
    `/api/dealers/${dealerId}/settings/2fa/enable`
  );
  return response.data;
}

/**
 * Disable two-factor authentication
 */
export async function disableTwoFactor(dealerId: string, code: string): Promise<void> {
  await apiClient.post(`/api/dealers/${dealerId}/settings/2fa/disable`, { code });
}

/**
 * Verify two-factor code
 */
export async function verifyTwoFactor(dealerId: string, code: string): Promise<boolean> {
  const response = await apiClient.post<{ valid: boolean }>(
    `/api/dealers/${dealerId}/settings/2fa/verify`,
    { code }
  );
  return response.data.valid;
}

/**
 * Change password
 */
export async function changePassword(
  dealerId: string,
  currentPassword: string,
  newPassword: string
): Promise<void> {
  await apiClient.post(`/api/dealers/${dealerId}/settings/change-password`, {
    currentPassword,
    newPassword,
  });
}

// ============================================================================
// Helpers
// ============================================================================

export function getSessionTimeoutLabel(minutes: number): string {
  if (minutes < 60) return `${minutes} minutos`;
  if (minutes === 60) return '1 hora';
  return `${minutes / 60} horas`;
}

export const sessionTimeoutOptions = [
  { value: 15, label: '15 minutos' },
  { value: 30, label: '30 minutos' },
  { value: 60, label: '1 hora' },
  { value: 120, label: '2 horas' },
];

// ==============================================================================
// Payment Gateway Settings
// ==============================================================================

export interface PaymentGatewayStatus {
  id: string;
  name: string;
  description: string;
  recommended: boolean;
  isLocal: boolean;
  isEnabled: boolean;
}

export interface GatewaySettingsResponse {
  gateways: PaymentGatewayStatus[];
}

/**
 * Get the authenticated dealer's payment gateway preferences.
 * Falls back to all-enabled defaults if no preferences saved.
 */
export async function getPaymentGatewaySettings(): Promise<PaymentGatewayStatus[]> {
  try {
    const response = await apiClient.get<GatewaySettingsResponse>(
      '/api/billing/dealers/me/payment-gateways'
    );
    return response.data.gateways;
  } catch {
    // Return defaults (all enabled) if endpoint not reachable
    return defaultPaymentGateways;
  }
}

/**
 * Update which payment gateways the dealer has enabled.
 */
export async function updatePaymentGatewaySettings(enabledIds: string[]): Promise<void> {
  await apiClient.put('/api/billing/dealers/me/payment-gateways', {
    enabledGatewayIds: enabledIds,
  });
}

/** Default gateway list — all enabled — used as fallback */
export const defaultPaymentGateways: PaymentGatewayStatus[] = [
  {
    id: 'Azul',
    name: 'Azul (Banco Popular)',
    description: 'Tarjetas dominicanas — Visa, Mastercard, Discover',
    recommended: true,
    isLocal: true,
    isEnabled: true,
  },
  {
    id: 'CardNET',
    name: 'CardNET',
    description: 'Red de pagos Visa/MasterCard local',
    recommended: false,
    isLocal: true,
    isEnabled: true,
  },
  {
    id: 'PixelPay',
    name: 'PixelPay',
    description: 'Fintech regional — Centroamérica y Caribe',
    recommended: false,
    isLocal: true,
    isEnabled: true,
  },
  {
    id: 'Fygaro',
    name: 'Fygaro',
    description: 'Crédito y débito local dominicano',
    recommended: false,
    isLocal: true,
    isEnabled: true,
  },
  {
    id: 'PayPal',
    name: 'PayPal',
    description: 'Tarjetas internacionales y cuenta PayPal',
    recommended: true,
    isLocal: false,
    isEnabled: true,
  },
  {
    id: 'Stripe',
    name: 'Stripe',
    description: 'Apple Pay, Google Pay y tarjetas internacionales',
    recommended: false,
    isLocal: false,
    isEnabled: true,
  },
];
