/**
 * useLocale Hook
 * 
 * Provides easy access to current locale and formatting utilities
 */

import { useTranslation } from 'react-i18next';
import { useMemo, useCallback } from 'react';
import {
  formatLocalizedNumber,
  formatLocalizedCurrency,
  formatLocalizedDate,
  formatRelativeTime,
  formatCompactNumber,
  formatMileage,
  formatArea,
} from './utils';

export const useLocale = () => {
  const { i18n } = useTranslation();
  const locale = i18n.language;

  // Memoized formatters that use current locale
  const formatters = useMemo(() => ({
    number: (value: number, options?: Intl.NumberFormatOptions) => 
      formatLocalizedNumber(value, locale, options),
    
    currency: (value: number, currency?: string) => 
      formatLocalizedCurrency(value, locale, currency),
    
    date: (date: Date | string, options?: Intl.DateTimeFormatOptions) => 
      formatLocalizedDate(date, locale, options),
    
    relativeTime: (date: Date | string) => 
      formatRelativeTime(date, locale),
    
    compact: (value: number) => 
      formatCompactNumber(value, locale),
    
    mileage: (value: number, unit?: 'km' | 'mi') => 
      formatMileage(value, locale, unit),
    
    area: (value: number, unit?: 'm2' | 'ft2') => 
      formatArea(value, locale, unit),
  }), [locale]);

  // Change language function
  const changeLanguage = useCallback((lang: string) => {
    i18n.changeLanguage(lang);
  }, [i18n]);

  return {
    locale,
    isSpanish: locale === 'es',
    isEnglish: locale === 'en',
    changeLanguage,
    ...formatters,
  };
};

export default useLocale;
