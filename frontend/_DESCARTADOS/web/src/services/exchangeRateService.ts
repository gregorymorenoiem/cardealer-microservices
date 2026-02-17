/**
 * Exchange Rate Service - API Client
 *
 * Servicio de tasas de cambio del Banco Central RD
 * Cumplimiento DGII para pagos en USD/EUR
 */

import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'https://api.okla.com.do';

// ============================================================================
// TYPES & ENUMS
// ============================================================================

export enum ExchangeRateSource {
  BancoCentralApi = 'BancoCentralApi',
  BancoCentralWeb = 'BancoCentralWeb',
  ExternalProvider = 'ExternalProvider',
  Manual = 'Manual',
}

export enum Currency {
  DOP = 'DOP',
  USD = 'USD',
  EUR = 'EUR',
}

export interface ExchangeRate {
  id: string;
  rateDate: string;
  sourceCurrency: Currency;
  targetCurrency: Currency;
  buyRate: number;
  sellRate: number;
  source: ExchangeRateSource;
  bcrdReferenceId?: string;
  fetchedAt: string;
  isActive: boolean;
}

export interface ConversionResult {
  originalAmount: number;
  originalCurrency: Currency;
  convertedAmountDop: number;
  appliedRate: number;
  rateDate: string;
  rateSource: ExchangeRateSource;
  itbisDop: number;
  itbisRate: number;
  totalWithItbisDop: number;
  conversionRecordId?: string;
}

export interface ConversionQuote {
  originalAmount: number;
  originalCurrency: Currency;
  convertedAmountDop: number;
  appliedRate: number;
  rateDate: string;
  validUntil: string;
  itbisDop: number;
  totalWithItbisDop: number;
}

export interface CurrencyConversion {
  id: string;
  paymentTransactionId: string;
  exchangeRateId: string;
  originalCurrency: Currency;
  originalAmount: number;
  convertedAmountDop: number;
  appliedRate: number;
  rateDate: string;
  rateSource: ExchangeRateSource;
  itbisDop: number;
  totalWithItbisDop: number;
  ncf?: string;
  ncfIssuedAt?: string;
  createdAt: string;
}

// ============================================================================
// API CLIENT
// ============================================================================

const apiClient = axios.create({
  baseURL: `${API_URL}/api/exchangerates`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token interceptor
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ============================================================================
// EXCHANGE RATES
// ============================================================================

/**
 * Get current exchange rate for a currency
 */
export async function getCurrentRate(currency: Currency): Promise<ExchangeRate> {
  const response = await apiClient.get<ExchangeRate>(`/current/${currency}`);
  return response.data;
}

/**
 * Get all current exchange rates (USD and EUR)
 */
export async function getAllCurrentRates(): Promise<ExchangeRate[]> {
  const response = await apiClient.get<ExchangeRate[]>('/current');
  return response.data;
}

/**
 * Get exchange rate history for a currency
 */
export async function getRateHistory(
  currency: Currency,
  startDate?: string,
  endDate?: string
): Promise<ExchangeRate[]> {
  const response = await apiClient.get<ExchangeRate[]>(`/history/${currency}`, {
    params: { startDate, endDate },
  });
  return response.data;
}

// ============================================================================
// CONVERSIONS
// ============================================================================

/**
 * Get a conversion quote (without recording transaction)
 */
export async function getConversionQuote(
  amount: number,
  currency: Currency
): Promise<ConversionQuote> {
  const response = await apiClient.get<ConversionQuote>('/quote', {
    params: { amount, currency },
  });
  return response.data;
}

/**
 * Convert amount to DOP (with transaction recording)
 */
export async function convertToDop(
  amount: number,
  currency: Currency,
  transactionId?: string
): Promise<ConversionResult> {
  const response = await apiClient.post<ConversionResult>('/convert', {
    amount,
    currency,
    transactionId,
  });
  return response.data;
}

/**
 * Get conversion record by transaction ID
 */
export async function getConversionByTransaction(
  transactionId: string
): Promise<CurrencyConversion> {
  const response = await apiClient.get<CurrencyConversion>(`/conversions/${transactionId}`);
  return response.data;
}

// ============================================================================
// ADMIN
// ============================================================================

/**
 * Force refresh exchange rates from BCRD (Admin only)
 */
export async function refreshRates(): Promise<ExchangeRate[]> {
  const response = await apiClient.post<ExchangeRate[]>('/refresh');
  return response.data;
}

// ============================================================================
// HELPERS
// ============================================================================

export function getCurrencySymbol(currency: Currency): string {
  const symbols: Record<Currency, string> = {
    [Currency.DOP]: 'RD$',
    [Currency.USD]: 'US$',
    [Currency.EUR]: '€',
  };
  return symbols[currency] || currency;
}

export function getCurrencyName(currency: Currency): string {
  const names: Record<Currency, string> = {
    [Currency.DOP]: 'Peso Dominicano',
    [Currency.USD]: 'Dólar Estadounidense',
    [Currency.EUR]: 'Euro',
  };
  return names[currency] || currency;
}

export function getCurrencyFlag(currency: Currency): string {
  const flags: Record<Currency, string> = {
    [Currency.DOP]: '🇩🇴',
    [Currency.USD]: '🇺🇸',
    [Currency.EUR]: '🇪🇺',
  };
  return flags[currency] || '🏳️';
}

export function formatAmount(amount: number, currency: Currency): string {
  const locales: Record<Currency, string> = {
    [Currency.DOP]: 'es-DO',
    [Currency.USD]: 'en-US',
    [Currency.EUR]: 'de-DE',
  };

  return new Intl.NumberFormat(locales[currency] || 'es-DO', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatRate(rate: number): string {
  return rate.toFixed(4);
}

export function getRateSourceLabel(source: ExchangeRateSource): string {
  const labels: Record<ExchangeRateSource, string> = {
    [ExchangeRateSource.BancoCentralApi]: 'API Banco Central',
    [ExchangeRateSource.BancoCentralWeb]: 'Web Banco Central',
    [ExchangeRateSource.ExternalProvider]: 'Proveedor Externo',
    [ExchangeRateSource.Manual]: 'Entrada Manual',
  };
  return labels[source] || source;
}

/**
 * Calculate ITBIS (18%) on an amount
 */
export function calculateItbis(amount: number): number {
  const ITBIS_RATE = 0.18;
  return Math.round(amount * ITBIS_RATE * 100) / 100;
}

/**
 * Calculate total with ITBIS
 */
export function calculateTotalWithItbis(amount: number): number {
  return amount + calculateItbis(amount);
}

/**
 * Convert USD to DOP (client-side calculation for display only)
 * For actual transactions, always use server-side conversion
 */
export function estimateConversion(
  amount: number,
  currency: Currency,
  rate: number
): { amountDop: number; itbis: number; total: number } {
  if (currency === Currency.DOP) {
    return {
      amountDop: amount,
      itbis: calculateItbis(amount),
      total: calculateTotalWithItbis(amount),
    };
  }

  const amountDop = Math.round(amount * rate * 100) / 100;
  const itbis = calculateItbis(amountDop);
  const total = amountDop + itbis;

  return { amountDop, itbis, total };
}

// ============================================================================
// CACHE
// ============================================================================

const RATE_CACHE_KEY = 'okla_exchange_rates';
const RATE_CACHE_TTL = 60 * 60 * 1000; // 1 hour

interface CachedRates {
  rates: ExchangeRate[];
  fetchedAt: number;
}

export function getCachedRates(): ExchangeRate[] | null {
  try {
    const cached = localStorage.getItem(RATE_CACHE_KEY);
    if (!cached) return null;

    const { rates, fetchedAt }: CachedRates = JSON.parse(cached);
    const isExpired = Date.now() - fetchedAt > RATE_CACHE_TTL;

    if (isExpired) {
      localStorage.removeItem(RATE_CACHE_KEY);
      return null;
    }

    return rates;
  } catch {
    return null;
  }
}

export function setCachedRates(rates: ExchangeRate[]): void {
  const cache: CachedRates = {
    rates,
    fetchedAt: Date.now(),
  };
  localStorage.setItem(RATE_CACHE_KEY, JSON.stringify(cache));
}

/**
 * Get rates with caching
 */
export async function getRatesWithCache(): Promise<ExchangeRate[]> {
  const cached = getCachedRates();
  if (cached) return cached;

  const rates = await getAllCurrentRates();
  setCachedRates(rates);
  return rates;
}

export default {
  // Rates
  getCurrentRate,
  getAllCurrentRates,
  getRateHistory,
  // Conversions
  getConversionQuote,
  convertToDop,
  getConversionByTransaction,
  // Admin
  refreshRates,
  // Helpers
  getCurrencySymbol,
  getCurrencyName,
  getCurrencyFlag,
  formatAmount,
  formatRate,
  getRateSourceLabel,
  calculateItbis,
  calculateTotalWithItbis,
  estimateConversion,
  // Cache
  getCachedRates,
  setCachedRates,
  getRatesWithCache,
};
