/**
 * Input Sanitization Utilities
 *
 * Provides functions to sanitize user input and prevent XSS attacks.
 * These should be used on any user-provided content before rendering.
 */

// HTML entities map for escaping
const HTML_ENTITIES: Record<string, string> = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#x27;',
  '/': '&#x2F;',
  '`': '&#x60;',
  '=': '&#x3D;',
};

/**
 * Escape HTML special characters to prevent XSS
 */
export function escapeHtml(str: string): string {
  if (typeof str !== 'string') return '';
  return str.replace(/[&<>"'`=/]/g, char => HTML_ENTITIES[char] || char);
}

/**
 * Remove all HTML tags from a string
 */
export function stripHtml(str: string): string {
  if (typeof str !== 'string') return '';
  return str.replace(/<[^>]*>/g, '');
}

/**
 * Sanitize a string for use in URLs
 */
export function sanitizeUrl(url: string): string {
  if (typeof url !== 'string') return '';

  // Trim whitespace
  const trimmed = url.trim();

  // Block javascript: and data: URLs
  const lower = trimmed.toLowerCase();
  if (
    lower.startsWith('javascript:') ||
    lower.startsWith('data:') ||
    lower.startsWith('vbscript:')
  ) {
    return '';
  }

  // Only allow http, https, mailto, tel protocols
  if (trimmed.includes(':')) {
    const protocol = trimmed.split(':')[0].toLowerCase();
    if (!['http', 'https', 'mailto', 'tel'].includes(protocol)) {
      return '';
    }
  }

  return trimmed;
}

/**
 * Sanitize user input for search queries
 */
export function sanitizeSearchQuery(query: string): string {
  if (typeof query !== 'string') return '';

  return query
    .trim()
    .replace(/[<>'"]/g, '') // Remove potential XSS chars
    .replace(/\s+/g, ' ') // Normalize whitespace
    .slice(0, 200); // Limit length
}

/**
 * Sanitize a filename
 */
export function sanitizeFilename(filename: string): string {
  if (typeof filename !== 'string') return '';

  return filename
    .replace(/[^a-zA-Z0-9._-]/g, '_') // Replace invalid chars
    .replace(/\.{2,}/g, '.') // No double dots
    .replace(/^\./, '') // No leading dot
    .slice(0, 255); // Max filename length
}

/**
 * Sanitize numeric input
 */
export function sanitizeNumber(
  value: string | number,
  options: {
    min?: number;
    max?: number;
    allowFloat?: boolean;
    defaultValue?: number;
  } = {}
): number {
  const { min, max, allowFloat = false, defaultValue = 0 } = options;

  let num = typeof value === 'number' ? value : parseFloat(String(value));

  if (isNaN(num)) return defaultValue;

  if (!allowFloat) {
    num = Math.floor(num);
  }

  if (min !== undefined && num < min) return min;
  if (max !== undefined && num > max) return max;

  return num;
}

/**
 * Sanitize phone number (Dominican Republic format)
 */
export function sanitizePhone(phone: string): string {
  if (typeof phone !== 'string') return '';

  // Remove all non-digit characters
  const digits = phone.replace(/\D/g, '');

  // Handle Dominican Republic format
  if (digits.length === 10 && digits.startsWith('8')) {
    return digits;
  }

  // Handle with country code
  if (digits.length === 11 && digits.startsWith('1')) {
    return digits.slice(1);
  }

  if (digits.length === 12 && digits.startsWith('18')) {
    return digits.slice(2);
  }

  return digits.slice(0, 10);
}

/**
 * Sanitize email address
 */
export function sanitizeEmail(email: string): string {
  if (typeof email !== 'string') return '';

  const sanitized = email.trim().toLowerCase().slice(0, 254);

  // Basic email format validation
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(sanitized)) {
    return '';
  }

  return sanitized;
}

/**
 * Sanitize RNC (Dominican tax ID)
 */
export function sanitizeRNC(rnc: string): string {
  if (typeof rnc !== 'string') return '';

  // Remove all non-digit characters
  const digits = rnc.replace(/\D/g, '');

  // RNC should be 9 or 11 digits
  if (digits.length !== 9 && digits.length !== 11) {
    return '';
  }

  return digits;
}

/**
 * Sanitize license plate (Dominican format)
 */
export function sanitizePlate(plate: string): string {
  if (typeof plate !== 'string') return '';

  return plate
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '')
    .slice(0, 7);
}

/**
 * Sanitize VIN number
 */
export function sanitizeVIN(vin: string): string {
  if (typeof vin !== 'string') return '';

  const sanitized = vin
    .toUpperCase()
    .replace(/[^A-HJ-NPR-Z0-9]/g, '') // VIN excludes I, O, Q
    .slice(0, 17);

  // VIN must be exactly 17 characters
  if (sanitized.length !== 17) {
    return '';
  }

  return sanitized;
}

/**
 * Sanitize price input
 */
export function sanitizePrice(price: string | number): number {
  return sanitizeNumber(price, {
    min: 0,
    max: 100_000_000, // 100 million max
    allowFloat: false,
    defaultValue: 0,
  });
}

/**
 * Sanitize year input
 */
export function sanitizeYear(year: string | number): number {
  const currentYear = new Date().getFullYear();
  return sanitizeNumber(year, {
    min: 1900,
    max: currentYear + 2, // Allow 2 years in future for new models
    allowFloat: false,
    defaultValue: currentYear,
  });
}

/**
 * Sanitize mileage input
 */
export function sanitizeMileage(mileage: string | number): number {
  return sanitizeNumber(mileage, {
    min: 0,
    max: 2_000_000, // 2 million km max
    allowFloat: false,
    defaultValue: 0,
  });
}

/**
 * Sanitize text content (for descriptions, etc.)
 */
export function sanitizeText(
  text: string,
  options: {
    maxLength?: number;
    allowNewlines?: boolean;
  } = {}
): string {
  const { maxLength = 5000, allowNewlines = true } = options;

  if (typeof text !== 'string') return '';

  let sanitized = text
    .trim()
    .replace(/<[^>]*>/g, '') // Strip HTML tags
    .replace(/[<>]/g, ''); // Remove any remaining angle brackets

  if (!allowNewlines) {
    sanitized = sanitized.replace(/[\r\n]+/g, ' ');
  }

  // Normalize whitespace
  sanitized = sanitized.replace(/[ \t]+/g, ' ');

  return sanitized.slice(0, maxLength);
}

/**
 * Create a safe object from user input
 * Only keeps specified keys and sanitizes values
 */
export function sanitizeObject<T extends Record<string, unknown>>(
  input: unknown,
  schema: {
    [K in keyof T]: (value: unknown) => T[K];
  }
): T {
  if (!input || typeof input !== 'object') {
    return {} as T;
  }

  const result: Partial<T> = {};

  for (const key in schema) {
    const sanitizer = schema[key];
    const value = (input as Record<string, unknown>)[key];
    result[key] = sanitizer(value);
  }

  return result as T;
}

export default {
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
};
