/**
 * Application-wide constants for the OKLA platform.
 */

/**
 * Default DOP/USD exchange rate used for OKLA Score price analysis.
 *
 * TODO: In production, this should be fetched from the BCRD API or
 * an exchange-rate service (e.g., ExchangeRate-API) and cached with
 * a short TTL. This fallback is only used when no live rate is available.
 */
export const DOP_USD_EXCHANGE_RATE = 58.5;
