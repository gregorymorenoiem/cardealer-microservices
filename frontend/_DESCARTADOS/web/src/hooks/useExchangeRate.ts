/**
 * Exchange Rate Hooks - React Query
 *
 * Hooks para tasas de cambio y conversión de monedas
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import exchangeRateService, {
  ExchangeRate,
  Currency,
  ConversionQuote,
  ConversionResult,
  CurrencyConversion,
} from '@/services/exchangeRateService';

// ============================================================================
// QUERY KEYS
// ============================================================================

export const exchangeRateKeys = {
  all: ['exchange-rates'] as const,
  current: (currency?: Currency) => [...exchangeRateKeys.all, 'current', currency] as const,
  currentAll: () => [...exchangeRateKeys.all, 'current-all'] as const,
  history: (currency: Currency, startDate?: string, endDate?: string) =>
    [...exchangeRateKeys.all, 'history', currency, startDate, endDate] as const,
  quote: (amount: number, currency: Currency) =>
    [...exchangeRateKeys.all, 'quote', amount, currency] as const,
  conversion: (transactionId: string) =>
    [...exchangeRateKeys.all, 'conversion', transactionId] as const,
};

// ============================================================================
// EXCHANGE RATES
// ============================================================================

/**
 * Get current exchange rate for a currency
 */
export function useCurrentRate(currency: Currency) {
  return useQuery<ExchangeRate, Error>({
    queryKey: exchangeRateKeys.current(currency),
    queryFn: () => exchangeRateService.getCurrentRate(currency),
    staleTime: 1000 * 60 * 60, // 1 hour
  });
}

/**
 * Get all current exchange rates (USD and EUR)
 */
export function useAllCurrentRates() {
  return useQuery<ExchangeRate[], Error>({
    queryKey: exchangeRateKeys.currentAll(),
    queryFn: exchangeRateService.getRatesWithCache,
    staleTime: 1000 * 60 * 60, // 1 hour
  });
}

/**
 * Get exchange rate history
 */
export function useRateHistory(currency: Currency, startDate?: string, endDate?: string) {
  return useQuery<ExchangeRate[], Error>({
    queryKey: exchangeRateKeys.history(currency, startDate, endDate),
    queryFn: () => exchangeRateService.getRateHistory(currency, startDate, endDate),
  });
}

// ============================================================================
// CONVERSIONS
// ============================================================================

/**
 * Get a conversion quote (without recording transaction)
 */
export function useConversionQuote(amount: number, currency: Currency) {
  return useQuery<ConversionQuote, Error>({
    queryKey: exchangeRateKeys.quote(amount, currency),
    queryFn: () => exchangeRateService.getConversionQuote(amount, currency),
    enabled: amount > 0 && currency !== Currency.DOP,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Convert amount to DOP (with transaction recording)
 */
export function useConvertToDop() {
  return useMutation<
    ConversionResult,
    Error,
    { amount: number; currency: Currency; transactionId?: string }
  >({
    mutationFn: ({ amount, currency, transactionId }) =>
      exchangeRateService.convertToDop(amount, currency, transactionId),
  });
}

/**
 * Get conversion record by transaction ID
 */
export function useConversionByTransaction(transactionId: string) {
  return useQuery<CurrencyConversion, Error>({
    queryKey: exchangeRateKeys.conversion(transactionId),
    queryFn: () => exchangeRateService.getConversionByTransaction(transactionId),
    enabled: !!transactionId,
  });
}

// ============================================================================
// ADMIN
// ============================================================================

/**
 * Force refresh exchange rates from BCRD (Admin only)
 */
export function useRefreshRates() {
  const queryClient = useQueryClient();

  return useMutation<ExchangeRate[], Error>({
    mutationFn: exchangeRateService.refreshRates,
    onSuccess: (rates) => {
      // Update cache
      exchangeRateService.setCachedRates(rates);
      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: exchangeRateKeys.all });
    },
  });
}

// ============================================================================
// HELPERS
// ============================================================================

/**
 * Get currency symbol
 */
export function useCurrencySymbol(currency: Currency) {
  return exchangeRateService.getCurrencySymbol(currency);
}

/**
 * Get currency name
 */
export function useCurrencyName(currency: Currency) {
  return exchangeRateService.getCurrencyName(currency);
}

/**
 * Get currency flag
 */
export function useCurrencyFlag(currency: Currency) {
  return exchangeRateService.getCurrencyFlag(currency);
}

/**
 * Format amount with currency
 */
export function useFormatAmount(amount: number, currency: Currency) {
  return exchangeRateService.formatAmount(amount, currency);
}

/**
 * Format exchange rate
 */
export function useFormatRate(rate: number) {
  return exchangeRateService.formatRate(rate);
}

/**
 * Calculate ITBIS (18%)
 */
export function useCalculateItbis(amount: number) {
  return exchangeRateService.calculateItbis(amount);
}

/**
 * Calculate total with ITBIS
 */
export function useCalculateTotalWithItbis(amount: number) {
  return exchangeRateService.calculateTotalWithItbis(amount);
}

/**
 * Estimate conversion (client-side for display only)
 */
export function useEstimateConversion(amount: number, currency: Currency, rate: number) {
  return exchangeRateService.estimateConversion(amount, currency, rate);
}

/**
 * Combined hook for currency selection in checkout
 */
export function useCurrencySelection() {
  const { data: rates, isLoading, error } = useAllCurrentRates();

  const currencies = [
    {
      currency: Currency.DOP,
      symbol: exchangeRateService.getCurrencySymbol(Currency.DOP),
      name: exchangeRateService.getCurrencyName(Currency.DOP),
      flag: exchangeRateService.getCurrencyFlag(Currency.DOP),
      rate: 1,
    },
    {
      currency: Currency.USD,
      symbol: exchangeRateService.getCurrencySymbol(Currency.USD),
      name: exchangeRateService.getCurrencyName(Currency.USD),
      flag: exchangeRateService.getCurrencyFlag(Currency.USD),
      rate: rates?.find((r) => r.sourceCurrency === Currency.USD)?.buyRate || 0,
    },
    {
      currency: Currency.EUR,
      symbol: exchangeRateService.getCurrencySymbol(Currency.EUR),
      name: exchangeRateService.getCurrencyName(Currency.EUR),
      flag: exchangeRateService.getCurrencyFlag(Currency.EUR),
      rate: rates?.find((r) => r.sourceCurrency === Currency.EUR)?.buyRate || 0,
    },
  ];

  return {
    currencies,
    isLoading,
    error,
    rateDate: rates?.[0]?.rateDate,
    formatAmount: exchangeRateService.formatAmount,
    estimateConversion: exchangeRateService.estimateConversion,
  };
}
