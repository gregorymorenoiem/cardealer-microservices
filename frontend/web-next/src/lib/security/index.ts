/**
 * Security Utilities - Barrel Export
 *
 * Centralized security utilities for the OKLA frontend.
 */

// Input Sanitization
export {
  escapeHtml,
  stripHtml,
  sanitizeUrl,
  sanitizeSearchQuery,
  sanitizeFilename,
  sanitizeNumber,
  sanitizePhone,
  sanitizeEmail,
  sanitizeRNC,
  sanitizePlate,
  sanitizeVIN,
  sanitizePrice,
  sanitizeYear,
  sanitizeMileage,
  sanitizeText,
  sanitizeObject,
} from './sanitize';

// Rate Limiting
export {
  useRateLimit,
  createDebouncedRateLimiter,
  createThrottledRateLimiter,
  rateLimitPresets,
} from './rate-limit';

// CSRF Protection
export {
  getCsrfToken,
  refreshCsrfToken,
  getCsrfHeaders,
  useCsrfToken,
  csrfFetch,
  CsrfInput,
  isValidTokenFormat,
  validateDoubleSubmit,
} from './csrf';

// Auth Security
export {
  validatePassword,
  validateEmail,
  generateSecureToken,
  hashString,
  isSessionExpiringSoon,
  parseJwtPayload,
  isJwtExpired,
  isValidRedirectUrl,
  getSafeRedirectUrl,
  maskSensitiveData,
  maskEmail,
  maskPhone,
} from './auth';

export type { PasswordStrength, PasswordValidation } from './auth';
