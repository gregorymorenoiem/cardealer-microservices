/**
 * i18n Utilities
 * 
 * Helper functions for localized formatting
 */

import { useTranslation as useI18nTranslation } from 'react-i18next';
import type { Namespace } from '@/i18n';

/**
 * Enhanced useTranslation hook with namespace support
 */
export const useTranslation = (ns?: Namespace | Namespace[]) => {
  return useI18nTranslation(ns);
};

/**
 * Format a number according to the current locale
 */
export const formatLocalizedNumber = (
  value: number,
  locale: string = 'es',
  options?: Intl.NumberFormatOptions
): string => {
  const localeMap: Record<string, string> = {
    es: 'es-DO', // Dominican Republic Spanish
    en: 'en-US',
  };
  
  return new Intl.NumberFormat(localeMap[locale] || locale, options).format(value);
};

/**
 * Format currency according to the current locale
 */
export const formatLocalizedCurrency = (
  value: number,
  locale: string = 'es',
  currency: string = 'DOP'
): string => {
  const localeMap: Record<string, string> = {
    es: 'es-DO',
    en: 'en-US',
  };
  
  // Use USD for English locale by default
  const currencyToUse = locale === 'en' ? 'USD' : currency;
  
  return new Intl.NumberFormat(localeMap[locale] || locale, {
    style: 'currency',
    currency: currencyToUse,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

/**
 * Format a date according to the current locale
 */
export const formatLocalizedDate = (
  date: Date | string,
  locale: string = 'es',
  options?: Intl.DateTimeFormatOptions
): string => {
  const localeMap: Record<string, string> = {
    es: 'es-DO',
    en: 'en-US',
  };
  
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  
  const defaultOptions: Intl.DateTimeFormatOptions = {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  };
  
  return new Intl.DateTimeFormat(
    localeMap[locale] || locale, 
    options || defaultOptions
  ).format(dateObj);
};

/**
 * Format relative time (e.g., "2 hours ago", "in 3 days")
 */
export const formatRelativeTime = (
  date: Date | string,
  locale: string = 'es'
): string => {
  const localeMap: Record<string, string> = {
    es: 'es-DO',
    en: 'en-US',
  };
  
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffMs = dateObj.getTime() - now.getTime();
  const diffSeconds = Math.round(diffMs / 1000);
  const diffMinutes = Math.round(diffSeconds / 60);
  const diffHours = Math.round(diffMinutes / 60);
  const diffDays = Math.round(diffHours / 24);
  
  const rtf = new Intl.RelativeTimeFormat(localeMap[locale] || locale, {
    numeric: 'auto',
  });
  
  if (Math.abs(diffSeconds) < 60) {
    return rtf.format(diffSeconds, 'second');
  } else if (Math.abs(diffMinutes) < 60) {
    return rtf.format(diffMinutes, 'minute');
  } else if (Math.abs(diffHours) < 24) {
    return rtf.format(diffHours, 'hour');
  } else if (Math.abs(diffDays) < 30) {
    return rtf.format(diffDays, 'day');
  } else {
    return formatLocalizedDate(dateObj, locale);
  }
};

/**
 * Format a compact number (e.g., 1.5K, 2.3M)
 */
export const formatCompactNumber = (
  value: number,
  locale: string = 'es'
): string => {
  const localeMap: Record<string, string> = {
    es: 'es-DO',
    en: 'en-US',
  };
  
  return new Intl.NumberFormat(localeMap[locale] || locale, {
    notation: 'compact',
    compactDisplay: 'short',
  }).format(value);
};

/**
 * Get ordinal suffix for a number (1st, 2nd, 3rd, etc.)
 */
export const getOrdinal = (n: number, locale: string = 'es'): string => {
  if (locale === 'es') {
    return `${n}°`;
  }
  
  // English ordinals
  const s = ['th', 'st', 'nd', 'rd'];
  const v = n % 100;
  return n + (s[(v - 20) % 10] || s[v] || s[0]);
};

/**
 * Format distance/mileage
 */
export const formatMileage = (
  value: number,
  locale: string = 'es',
  unit: 'km' | 'mi' = 'km'
): string => {
  const formattedNumber = formatLocalizedNumber(value, locale);
  const unitLabel = unit === 'km' ? 'km' : 'mi';
  return `${formattedNumber} ${unitLabel}`;
};

/**
 * Format area (square meters or square feet)
 */
export const formatArea = (
  value: number,
  locale: string = 'es',
  unit: 'm2' | 'ft2' = 'm2'
): string => {
  const formattedNumber = formatLocalizedNumber(value, locale);
  const unitLabel = unit === 'm2' ? 'm²' : 'ft²';
  return `${formattedNumber} ${unitLabel}`;
};
