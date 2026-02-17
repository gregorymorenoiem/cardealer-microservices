/**
 * Auth Security Utilities
 *
 * Security-focused utilities for authentication and session management.
 */

import { sanitizeEmail } from './sanitize';

/**
 * Password strength levels
 */
export type PasswordStrength = 'weak' | 'fair' | 'good' | 'strong';

/**
 * Password validation result
 */
export interface PasswordValidation {
  isValid: boolean;
  strength: PasswordStrength;
  score: number; // 0-100
  suggestions: string[];
  checks: {
    minLength: boolean;
    hasUppercase: boolean;
    hasLowercase: boolean;
    hasNumber: boolean;
    hasSpecial: boolean;
    noCommonPatterns: boolean;
    noSequential: boolean;
  };
}

// Common weak passwords to check against
const COMMON_PASSWORDS = new Set([
  'password',
  '123456',
  '12345678',
  'qwerty',
  'abc123',
  'password1',
  'password123',
  'admin',
  'letmein',
  'welcome',
  'monkey',
  'dragon',
  'master',
  'login',
  '1234567890',
]);

// Sequential patterns to avoid
const SEQUENTIAL_PATTERNS = ['abcdefgh', '12345678', 'qwertyui', 'asdfghjk', '!@#$%^&*'];

/**
 * Validate password strength
 */
export function validatePassword(password: string): PasswordValidation {
  const checks = {
    minLength: password.length >= 8,
    hasUppercase: /[A-Z]/.test(password),
    hasLowercase: /[a-z]/.test(password),
    hasNumber: /[0-9]/.test(password),
    hasSpecial: /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(password),
    noCommonPatterns: !COMMON_PASSWORDS.has(password.toLowerCase()),
    noSequential: !hasSequentialChars(password),
  };

  const suggestions: string[] = [];

  if (!checks.minLength) {
    suggestions.push('Usa al menos 8 caracteres');
  }
  if (!checks.hasUppercase) {
    suggestions.push('Incluye al menos una letra mayúscula');
  }
  if (!checks.hasLowercase) {
    suggestions.push('Incluye al menos una letra minúscula');
  }
  if (!checks.hasNumber) {
    suggestions.push('Incluye al menos un número');
  }
  if (!checks.hasSpecial) {
    suggestions.push('Incluye al menos un carácter especial (!@#$%...)');
  }
  if (!checks.noCommonPatterns) {
    suggestions.push('Evita contraseñas comunes');
  }
  if (!checks.noSequential) {
    suggestions.push('Evita secuencias como "123456" o "abcdef"');
  }

  // Calculate score
  let score = 0;
  if (checks.minLength) score += 20;
  if (checks.hasUppercase) score += 15;
  if (checks.hasLowercase) score += 15;
  if (checks.hasNumber) score += 15;
  if (checks.hasSpecial) score += 15;
  if (checks.noCommonPatterns) score += 10;
  if (checks.noSequential) score += 10;

  // Bonus for length
  if (password.length >= 12) score += 10;
  if (password.length >= 16) score += 10;

  // Cap at 100
  score = Math.min(100, score);

  // Determine strength
  let strength: PasswordStrength;
  if (score < 40) strength = 'weak';
  else if (score < 60) strength = 'fair';
  else if (score < 80) strength = 'good';
  else strength = 'strong';

  const isValid =
    checks.minLength &&
    checks.hasUppercase &&
    checks.hasLowercase &&
    checks.hasNumber &&
    checks.noCommonPatterns;

  return {
    isValid,
    strength,
    score,
    suggestions,
    checks,
  };
}

/**
 * Check for sequential characters
 */
function hasSequentialChars(str: string): boolean {
  const lower = str.toLowerCase();

  for (const pattern of SEQUENTIAL_PATTERNS) {
    for (let i = 0; i <= pattern.length - 4; i++) {
      const seq = pattern.slice(i, i + 4);
      if (lower.includes(seq)) return true;
      // Also check reverse
      if (lower.includes(seq.split('').reverse().join(''))) return true;
    }
  }

  // Check for repeated characters (e.g., "aaaa")
  if (/(.)\1{3,}/.test(str)) return true;

  return false;
}

/**
 * Validate email format
 */
export function validateEmail(email: string): {
  isValid: boolean;
  sanitized: string;
  error?: string;
} {
  const sanitized = sanitizeEmail(email);

  if (!sanitized) {
    return {
      isValid: false,
      sanitized: '',
      error: 'Formato de email inválido',
    };
  }

  // Additional checks
  if (sanitized.length > 254) {
    return {
      isValid: false,
      sanitized,
      error: 'Email demasiado largo',
    };
  }

  // Check for common typos in domains
  const domain = sanitized.split('@')[1];
  const commonTypos: Record<string, string> = {
    'gmial.com': 'gmail.com',
    'gmal.com': 'gmail.com',
    'gmail.co': 'gmail.com',
    'hotmal.com': 'hotmail.com',
    'hotmai.com': 'hotmail.com',
    'yaho.com': 'yahoo.com',
    'yahooo.com': 'yahoo.com',
  };

  if (commonTypos[domain]) {
    return {
      isValid: true,
      sanitized,
      error: `¿Quisiste decir ${sanitized.replace(domain, commonTypos[domain])}?`,
    };
  }

  return {
    isValid: true,
    sanitized,
  };
}

/**
 * Generate a secure random string (for tokens, etc.)
 */
export function generateSecureToken(length: number = 32): string {
  if (typeof window !== 'undefined' && window.crypto) {
    const array = new Uint8Array(length);
    window.crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }
  // Fallback
  return Array.from({ length }, () => Math.random().toString(36).charAt(2)).join('');
}

/**
 * Hash a string using SHA-256 (for client-side hashing before sending)
 */
export async function hashString(str: string): Promise<string> {
  if (typeof window === 'undefined' || !window.crypto?.subtle) {
    throw new Error('Crypto API not available');
  }

  const encoder = new TextEncoder();
  const data = encoder.encode(str);
  const hashBuffer = await window.crypto.subtle.digest('SHA-256', data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  return hashArray.map(byte => byte.toString(16).padStart(2, '0')).join('');
}

/**
 * Check if session is about to expire
 */
export function isSessionExpiringSoon(
  expiresAt: Date | string | number,
  thresholdMinutes: number = 5
): boolean {
  const expiry = new Date(expiresAt).getTime();
  const threshold = thresholdMinutes * 60 * 1000;
  return Date.now() > expiry - threshold;
}

/**
 * Parse JWT token (without verification - for client-side info extraction only)
 * WARNING: Never trust data from this for security decisions
 */
export function parseJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    if (!base64Url) return null;

    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );

    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

/**
 * Check if JWT token is expired
 */
export function isJwtExpired(token: string): boolean {
  const payload = parseJwtPayload(token);
  if (!payload || typeof payload.exp !== 'number') return true;
  return Date.now() >= payload.exp * 1000;
}

/**
 * Safe redirect URL validation
 * Prevents open redirect vulnerabilities
 */
export function isValidRedirectUrl(url: string, allowedHosts: string[] = []): boolean {
  if (!url || typeof url !== 'string') return false;

  try {
    // Relative URLs are safe
    if (url.startsWith('/') && !url.startsWith('//')) {
      return true;
    }

    const parsed = new URL(url, window.location.origin);

    // Must be same origin or in allowed hosts
    if (parsed.origin === window.location.origin) {
      return true;
    }

    if (allowedHosts.includes(parsed.hostname)) {
      return true;
    }

    return false;
  } catch {
    return false;
  }
}

/**
 * Get safe redirect URL (returns fallback if invalid)
 */
export function getSafeRedirectUrl(
  url: string | null | undefined,
  fallback: string = '/',
  allowedHosts: string[] = []
): string {
  if (!url) return fallback;
  return isValidRedirectUrl(url, allowedHosts) ? url : fallback;
}

/**
 * Mask sensitive data (for logging/display)
 */
export function maskSensitiveData(
  data: string,
  options: {
    showFirst?: number;
    showLast?: number;
    maskChar?: string;
  } = {}
): string {
  const { showFirst = 2, showLast = 2, maskChar = '*' } = options;

  if (!data || data.length <= showFirst + showLast) {
    return maskChar.repeat(data?.length || 4);
  }

  const first = data.slice(0, showFirst);
  const last = data.slice(-showLast);
  const middle = maskChar.repeat(Math.min(data.length - showFirst - showLast, 8));

  return first + middle + last;
}

/**
 * Mask email for display
 */
export function maskEmail(email: string): string {
  const [local, domain] = email.split('@');
  if (!domain) return maskSensitiveData(email);

  return maskSensitiveData(local, { showFirst: 2, showLast: 0 }) + '@' + domain;
}

/**
 * Mask phone number for display
 */
export function maskPhone(phone: string): string {
  const digits = phone.replace(/\D/g, '');
  return maskSensitiveData(digits, { showFirst: 0, showLast: 4 });
}

export default {
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
};
