/**
 * Settings Service
 *
 * Manages user preferences and settings:
 * - App settings (theme, language, currency) via UserService + localStorage
 * - Notification preferences via PrivacyController
 */

import { apiClient } from '@/lib/api-client';

// ============================================================
// TYPES
// ============================================================

export type Theme = 'light' | 'dark' | 'system';
export type Language = 'es' | 'en';
export type Currency = 'DOP' | 'USD';

export interface AppSettings {
  theme: Theme;
  language: Language;
  currency: Currency;
}

export interface EmailNotificationSettings {
  messages: boolean;
  inquiries: boolean;
  priceAlerts: boolean;
  newListings: boolean;
  marketing: boolean;
}

export interface PushNotificationSettings {
  messages: boolean;
  inquiries: boolean;
  priceAlerts: boolean;
}

export interface NotificationSettings {
  email: EmailNotificationSettings;
  push: PushNotificationSettings;
}

// Backend DTOs from PrivacyController
interface BackendCommunicationPreferences {
  email: {
    activityNotifications: boolean;
    listingUpdates: boolean;
    newsletter: boolean;
    promotions: boolean;
    priceAlerts: boolean;
  };
  sms: {
    verificationCodes: boolean;
    priceAlerts: boolean;
    promotions: boolean;
  };
  push: {
    newMessages: boolean;
    priceChanges: boolean;
    recommendations: boolean;
  };
  privacy: {
    allowProfiling: boolean;
    allowThirdPartySharing: boolean;
    allowAnalytics: boolean;
    allowRetargeting: boolean;
  };
  lastUpdated: string;
}

interface BackendUpdatePreferencesRequest {
  emailActivityNotifications?: boolean;
  emailListingUpdates?: boolean;
  emailNewsletter?: boolean;
  emailPromotions?: boolean;
  emailPriceAlerts?: boolean;
  smsPriceAlerts?: boolean;
  smsPromotions?: boolean;
  pushNewMessages?: boolean;
  pushPriceChanges?: boolean;
  pushRecommendations?: boolean;
  allowProfiling?: boolean;
  allowThirdPartySharing?: boolean;
  allowAnalytics?: boolean;
  allowRetargeting?: boolean;
}

// ============================================================
// CONSTANTS
// ============================================================

const THEME_STORAGE_KEY = 'okla-theme';
const DEFAULT_APP_SETTINGS: AppSettings = {
  theme: 'system',
  language: 'es',
  currency: 'DOP',
};

const DEFAULT_NOTIFICATIONS: NotificationSettings = {
  email: {
    messages: true,
    inquiries: true,
    priceAlerts: true,
    newListings: false,
    marketing: false,
  },
  push: {
    messages: true,
    inquiries: true,
    priceAlerts: false,
  },
};

// ============================================================
// THEME HELPERS (localStorage only - not in backend)
// ============================================================

/**
 * Get saved theme from localStorage
 */
export function getSavedTheme(): Theme {
  if (typeof window === 'undefined') return 'system';
  const saved = localStorage.getItem(THEME_STORAGE_KEY);
  if (saved === 'light' || saved === 'dark' || saved === 'system') {
    return saved;
  }
  return 'system';
}

/**
 * Save theme to localStorage and apply to document
 */
export function saveTheme(theme: Theme): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(THEME_STORAGE_KEY, theme);
  applyTheme(theme);
}

/**
 * Apply theme to document (adds/removes 'dark' class)
 */
export function applyTheme(theme: Theme): void {
  if (typeof window === 'undefined') return;

  console.log('=== [applyTheme] START ===');
  console.log('[applyTheme] Requested theme:', theme);

  const root = document.documentElement;
  const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  const isDark = theme === 'dark' || (theme === 'system' && prefersDark);

  console.log('[applyTheme] System prefers dark:', prefersDark);
  console.log('[applyTheme] Will apply dark mode:', isDark);
  console.log('[applyTheme] BEFORE - <html> classList:', root.className);
  console.log(
    '[applyTheme] BEFORE - --background CSS var:',
    getComputedStyle(root).getPropertyValue('--background')
  );

  if (isDark) {
    root.classList.add('dark');
  } else {
    root.classList.remove('dark');
  }

  // Force reflow to ensure CSS variables are recalculated
  void root.offsetHeight;

  console.log('[applyTheme] AFTER - <html> classList:', root.className);
  console.log(
    '[applyTheme] AFTER - --background CSS var:',
    getComputedStyle(root).getPropertyValue('--background')
  );
  console.log(
    '[applyTheme] AFTER - body backgroundColor:',
    getComputedStyle(document.body).backgroundColor
  );
  console.log('=== [applyTheme] END ===');
}

// ============================================================
// APP SETTINGS (UserService + localStorage)
// ============================================================

/**
 * Get app settings from backend (language, currency) + localStorage (theme)
 */
export async function getAppSettings(): Promise<AppSettings> {
  const theme = getSavedTheme();

  try {
    const response = await apiClient.get<{
      preferredLocale?: string;
      preferredCurrency?: string;
    }>('/api/users/me');

    const data = response.data;

    return {
      theme,
      language: (data.preferredLocale?.startsWith('en') ? 'en' : 'es') as Language,
      currency: (data.preferredCurrency === 'USD' ? 'USD' : 'DOP') as Currency,
    };
  } catch {
    // If not authenticated or error, return defaults with saved theme
    return { ...DEFAULT_APP_SETTINGS, theme };
  }
}

/**
 * Update app settings
 * - Theme: localStorage only
 * - Language & Currency: backend via PUT /api/users/me
 */
export async function updateAppSettings(settings: Partial<AppSettings>): Promise<AppSettings> {
  // Save theme to localStorage
  if (settings.theme) {
    saveTheme(settings.theme);
  }

  // Update backend for language and currency
  if (settings.language !== undefined || settings.currency !== undefined) {
    try {
      await apiClient.put('/api/users/me', {
        preferredLocale: settings.language === 'en' ? 'en-US' : 'es-DO',
        preferredCurrency: settings.currency,
      });
    } catch (error) {
      console.error('Error updating app settings:', error);
      // Don't throw - theme was saved, just log error
    }
  }

  return getAppSettings();
}

// ============================================================
// NOTIFICATION SETTINGS (PrivacyController)
// ============================================================

/**
 * Transform backend communication preferences to frontend format
 */
function transformBackendToFrontend(
  backend: BackendCommunicationPreferences
): NotificationSettings {
  return {
    email: {
      messages: backend.email.activityNotifications,
      inquiries: backend.email.listingUpdates,
      priceAlerts: backend.email.priceAlerts,
      newListings: backend.email.newsletter,
      marketing: backend.email.promotions,
    },
    push: {
      messages: backend.push.newMessages,
      inquiries: backend.push.priceChanges,
      priceAlerts: backend.push.recommendations,
    },
  };
}

/**
 * Transform frontend notification settings to backend format
 */
function transformFrontendToBackend(
  frontend: NotificationSettings
): BackendUpdatePreferencesRequest {
  return {
    emailActivityNotifications: frontend.email.messages,
    emailListingUpdates: frontend.email.inquiries,
    emailPriceAlerts: frontend.email.priceAlerts,
    emailNewsletter: frontend.email.newListings,
    emailPromotions: frontend.email.marketing,
    pushNewMessages: frontend.push.messages,
    pushPriceChanges: frontend.push.inquiries,
    pushRecommendations: frontend.push.priceAlerts,
  };
}

/**
 * Get notification preferences from backend
 */
export async function getNotificationSettings(): Promise<NotificationSettings> {
  try {
    const response = await apiClient.get<BackendCommunicationPreferences>(
      '/api/privacy/preferences'
    );
    return transformBackendToFrontend(response.data);
  } catch {
    return DEFAULT_NOTIFICATIONS;
  }
}

/**
 * Update notification preferences in backend
 */
export async function updateNotificationSettings(
  settings: NotificationSettings
): Promise<NotificationSettings> {
  try {
    const backendRequest = transformFrontendToBackend(settings);
    const response = await apiClient.put<BackendCommunicationPreferences>(
      '/api/privacy/preferences',
      backendRequest
    );
    return transformBackendToFrontend(response.data);
  } catch (error) {
    console.error('Error updating notification settings:', error);
    throw error;
  }
}

// ============================================================
// COMBINED SETTINGS API
// ============================================================

export interface AllSettings {
  app: AppSettings;
  notifications: NotificationSettings;
}

/**
 * Load all settings (app + notifications)
 */
export async function getAllSettings(): Promise<AllSettings> {
  const [app, notifications] = await Promise.all([getAppSettings(), getNotificationSettings()]);
  return { app, notifications };
}

/**
 * Save all settings
 */
export async function saveAllSettings(
  app: AppSettings,
  notifications: NotificationSettings
): Promise<AllSettings> {
  const [updatedApp, updatedNotifications] = await Promise.all([
    updateAppSettings(app),
    updateNotificationSettings(notifications),
  ]);
  return { app: updatedApp, notifications: updatedNotifications };
}

// ============================================================
// SERVICE EXPORT
// ============================================================

export const settingsService = {
  // Theme (localStorage)
  getSavedTheme,
  saveTheme,
  applyTheme,

  // App settings
  getAppSettings,
  updateAppSettings,

  // Notifications
  getNotificationSettings,
  updateNotificationSettings,

  // Combined
  getAllSettings,
  saveAllSettings,

  // Defaults
  DEFAULT_APP_SETTINGS,
  DEFAULT_NOTIFICATIONS,
};

export default settingsService;
