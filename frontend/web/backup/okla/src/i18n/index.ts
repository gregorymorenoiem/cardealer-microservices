/**
 * i18n Configuration
 * 
 * Internationalization setup with react-i18next
 * - Default language: Spanish (es)
 * - Secondary language: English (en)
 * - Auto-detection from browser
 * - Persistence in localStorage
 */

import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

// Import translations
import esCommon from './locales/es/common.json';
import esVehicles from './locales/es/vehicles.json';
import esProperties from './locales/es/properties.json';
import esAuth from './locales/es/auth.json';
import esDealer from './locales/es/dealer.json';
import esAdmin from './locales/es/admin.json';
import esBilling from './locales/es/billing.json';
import esErrors from './locales/es/errors.json';
import esUser from './locales/es/user.json';

import enCommon from './locales/en/common.json';
import enVehicles from './locales/en/vehicles.json';
import enProperties from './locales/en/properties.json';
import enAuth from './locales/en/auth.json';
import enDealer from './locales/en/dealer.json';
import enAdmin from './locales/en/admin.json';
import enBilling from './locales/en/billing.json';
import enErrors from './locales/en/errors.json';
import enUser from './locales/en/user.json';

// Supported languages
export const supportedLanguages = ['es', 'en'] as const;
export type SupportedLanguage = typeof supportedLanguages[number];

// Language labels for UI
export const languageLabels: Record<SupportedLanguage, string> = {
  es: 'Español',
  en: 'English',
};

// Language flags (emoji or can be replaced with icon components)
export const languageFlags: Record<SupportedLanguage, string> = {
  es: '🇪🇸',
  en: '🇺🇸',
};

// Resources configuration
const resources = {
  es: {
    common: esCommon,
    vehicles: esVehicles,
    properties: esProperties,
    auth: esAuth,
    dealer: esDealer,
    admin: esAdmin,
    billing: esBilling,
    errors: esErrors,
    user: esUser,
  },
  en: {
    common: enCommon,
    vehicles: enVehicles,
    properties: enProperties,
    auth: enAuth,
    dealer: enDealer,
    admin: enAdmin,
    billing: enBilling,
    errors: enErrors,
    user: enUser,
  },
};

// Namespaces
export const defaultNS = 'common';
export const namespaces = [
  'common',
  'vehicles',
  'properties',
  'auth',
  'dealer',
  'admin',
  'billing',
  'errors',
  'user',
] as const;

export type Namespace = typeof namespaces[number];

// Initialize i18n
i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    defaultNS,
    fallbackLng: 'es',
    supportedLngs: supportedLanguages,
    
    // Language detection options
    detection: {
      order: ['localStorage', 'navigator', 'htmlTag'],
      lookupLocalStorage: 'i18nextLng',
      caches: ['localStorage'],
    },

    interpolation: {
      escapeValue: false, // React already escapes
    },

    // React specific options
    react: {
      useSuspense: true,
    },

    // Debug in development
    debug: import.meta.env.DEV,
  });

// Helper to change language
export const changeLanguage = async (lng: SupportedLanguage) => {
  await i18n.changeLanguage(lng);
  document.documentElement.lang = lng;
  localStorage.setItem('i18nextLng', lng);
};

// Get current language
export const getCurrentLanguage = (): SupportedLanguage => {
  const lang = i18n.language?.split('-')[0] as SupportedLanguage;
  return supportedLanguages.includes(lang) ? lang : 'es';
};

// Re-export utilities and hooks
export * from './utils';
export { useLocale } from './useLocale';

export default i18n;
