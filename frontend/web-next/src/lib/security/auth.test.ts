import { describe, it, expect } from 'vitest';
import {
  validatePassword,
  validateEmail,
  isValidRedirectUrl,
  getSafeRedirectUrl,
  maskSensitiveData,
  maskEmail,
  maskPhone,
  isJwtExpired,
  parseJwtPayload,
} from './auth';

describe('Security - Auth Utilities', () => {
  describe('validatePassword', () => {
    it('should reject weak passwords', () => {
      const result = validatePassword('123456');
      expect(result.isValid).toBe(false);
      expect(result.strength).toBe('weak');
    });

    it('should accept strong passwords', () => {
      const result = validatePassword('MySecure@Pass123!');
      expect(result.isValid).toBe(true);
      expect(result.strength).toBe('strong');
    });

    it('should detect common passwords', () => {
      const result = validatePassword('password123');
      expect(result.checks.noCommonPatterns).toBe(false);
    });

    it('should detect sequential characters', () => {
      const result = validatePassword('Abcd1234!');
      expect(result.checks.noSequential).toBe(false);
    });

    it('should provide helpful suggestions', () => {
      const result = validatePassword('weak');
      expect(result.suggestions.length).toBeGreaterThan(0);
      expect(result.suggestions).toContain('Usa al menos 8 caracteres');
    });

    it('should check for uppercase letters', () => {
      const result = validatePassword('lowercase123!');
      expect(result.checks.hasUppercase).toBe(false);
    });

    it('should check for special characters', () => {
      const result = validatePassword('NoSpecial123');
      expect(result.checks.hasSpecial).toBe(false);
    });
  });

  describe('validateEmail', () => {
    it('should accept valid emails', () => {
      const result = validateEmail('user@example.com');
      expect(result.isValid).toBe(true);
      expect(result.sanitized).toBe('user@example.com');
    });

    it('should reject invalid emails', () => {
      const result = validateEmail('not-an-email');
      expect(result.isValid).toBe(false);
    });

    it('should detect common typos', () => {
      const result = validateEmail('user@gmial.com');
      expect(result.isValid).toBe(true);
      expect(result.error).toContain('gmail.com');
    });

    it('should sanitize email', () => {
      const result = validateEmail('  USER@EXAMPLE.COM  ');
      expect(result.sanitized).toBe('user@example.com');
    });
  });

  describe('isValidRedirectUrl', () => {
    it('should allow relative URLs', () => {
      expect(isValidRedirectUrl('/dashboard')).toBe(true);
      expect(isValidRedirectUrl('/path/to/page?query=1')).toBe(true);
    });

    it('should block protocol-relative URLs', () => {
      expect(isValidRedirectUrl('//evil.com')).toBe(false);
    });

    it('should block external URLs', () => {
      expect(isValidRedirectUrl('https://evil.com')).toBe(false);
    });

    it('should allow whitelisted hosts', () => {
      expect(isValidRedirectUrl('https://okla.com.do', ['okla.com.do'])).toBe(true);
    });

    it('should handle invalid URLs', () => {
      expect(isValidRedirectUrl('')).toBe(false);
      expect(isValidRedirectUrl(null as unknown as string)).toBe(false);
    });
  });

  describe('getSafeRedirectUrl', () => {
    it('should return valid URL', () => {
      expect(getSafeRedirectUrl('/dashboard')).toBe('/dashboard');
    });

    it('should return fallback for invalid URL', () => {
      expect(getSafeRedirectUrl('https://evil.com', '/home')).toBe('/home');
    });

    it('should return fallback for null/undefined', () => {
      expect(getSafeRedirectUrl(null, '/home')).toBe('/home');
      expect(getSafeRedirectUrl(undefined, '/home')).toBe('/home');
    });
  });

  describe('maskSensitiveData', () => {
    it('should mask middle of string', () => {
      expect(maskSensitiveData('1234567890')).toBe('12******90');
    });

    it('should handle custom mask options', () => {
      expect(maskSensitiveData('1234567890', { showFirst: 3, showLast: 3 })).toBe('123****890');
    });

    it('should handle short strings', () => {
      expect(maskSensitiveData('abc')).toBe('***');
    });

    it('should use custom mask character', () => {
      expect(maskSensitiveData('1234567890', { maskChar: 'X' })).toBe('12XXXXXX90');
    });
  });

  describe('maskEmail', () => {
    it('should mask email local part', () => {
      expect(maskEmail('user@example.com')).toBe('us**user@example.com');
    });

    it('should handle short local parts', () => {
      expect(maskEmail('ab@example.com')).toBe('**@example.com');
    });
  });

  describe('maskPhone', () => {
    it('should show only last 4 digits', () => {
      expect(maskPhone('8091234567')).toBe('******4567');
    });

    it('should handle formatted phone numbers', () => {
      expect(maskPhone('(809) 123-4567')).toBe('******4567');
    });
  });

  describe('parseJwtPayload', () => {
    const validToken =
      'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxNzE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c';

    it('should parse valid JWT payload', () => {
      const payload = parseJwtPayload(validToken);
      expect(payload).not.toBeNull();
      expect(payload?.sub).toBe('1234567890');
      expect(payload?.name).toBe('John Doe');
    });

    it('should return null for invalid token', () => {
      expect(parseJwtPayload('invalid')).toBeNull();
      expect(parseJwtPayload('')).toBeNull();
    });
  });

  describe('isJwtExpired', () => {
    it('should return true for expired token', () => {
      // Token with exp in the past
      const expiredToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE2MDAwMDAwMDB9.invalid';
      expect(isJwtExpired(expiredToken)).toBe(true);
    });

    it('should return true for invalid token', () => {
      expect(isJwtExpired('invalid')).toBe(true);
    });
  });
});
