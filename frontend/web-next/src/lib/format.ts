/**
 * Shared formatting utilities
 *
 * Canonical location for formatPrice and other display helpers.
 * Import from here instead of defining local copies.
 */

/**
 * Format a number as a currency string (default: DOP).
 *
 * @example
 * formatPrice(1500000)           // "RD$1,500,000"
 * formatPrice(25000, 'USD')      // "US$25,000"
 */
export function formatPrice(price: number, currency: string = 'DOP'): string {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(price);
}

/**
 * Format a price change amount with sign.
 */
export function formatPriceChange(change: number, currency: 'DOP' | 'USD' = 'DOP'): string {
  const prefix = change > 0 ? '+' : '';
  return `${prefix}${formatPrice(change, currency)}`;
}
