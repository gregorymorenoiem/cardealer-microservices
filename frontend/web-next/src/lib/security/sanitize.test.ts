import { describe, it, expect } from 'vitest';
import {
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
} from './sanitize';

describe('Security - Sanitization', () => {
  describe('escapeHtml', () => {
    it('should escape HTML special characters', () => {
      expect(escapeHtml('<script>alert("xss")</script>')).toBe(
        '&lt;script&gt;alert(&quot;xss&quot;)&lt;&#x2F;script&gt;'
      );
    });

    it('should escape ampersands', () => {
      expect(escapeHtml('Tom & Jerry')).toBe('Tom &amp; Jerry');
    });

    it('should handle empty string', () => {
      expect(escapeHtml('')).toBe('');
    });

    it('should handle non-string input', () => {
      expect(escapeHtml(null as unknown as string)).toBe('');
      expect(escapeHtml(undefined as unknown as string)).toBe('');
    });
  });

  describe('stripHtml', () => {
    it('should remove all HTML tags', () => {
      expect(stripHtml('<p>Hello <strong>World</strong></p>')).toBe('Hello World');
    });

    it('should handle self-closing tags', () => {
      expect(stripHtml('Hello<br/>World')).toBe('HelloWorld');
    });

    it('should handle script tags with content', () => {
      expect(stripHtml('<script>alert("xss")</script>Safe')).toBe('alert("xss")Safe');
    });
  });

  describe('sanitizeUrl', () => {
    it('should allow valid http URLs', () => {
      expect(sanitizeUrl('https://example.com')).toBe('https://example.com');
      expect(sanitizeUrl('http://example.com/path')).toBe('http://example.com/path');
    });

    it('should allow mailto and tel protocols', () => {
      expect(sanitizeUrl('mailto:test@example.com')).toBe('mailto:test@example.com');
      expect(sanitizeUrl('tel:+18091234567')).toBe('tel:+18091234567');
    });

    it('should block javascript: URLs', () => {
      expect(sanitizeUrl('javascript:alert(1)')).toBe('');
      expect(sanitizeUrl('JAVASCRIPT:alert(1)')).toBe('');
    });

    it('should block data: URLs', () => {
      expect(sanitizeUrl('data:text/html,<script>alert(1)</script>')).toBe('');
    });

    it('should allow relative URLs', () => {
      expect(sanitizeUrl('/path/to/page')).toBe('/path/to/page');
    });
  });

  describe('sanitizeSearchQuery', () => {
    it('should trim and normalize whitespace', () => {
      expect(sanitizeSearchQuery('  hello   world  ')).toBe('hello world');
    });

    it('should remove HTML-like characters', () => {
      expect(sanitizeSearchQuery('<script>test</script>')).toBe('scripttest/script');
    });

    it('should limit length to 200 characters', () => {
      const longQuery = 'a'.repeat(300);
      expect(sanitizeSearchQuery(longQuery).length).toBe(200);
    });
  });

  describe('sanitizeFilename', () => {
    it('should replace invalid characters', () => {
      expect(sanitizeFilename('my file<>:.pdf')).toBe('my_file___.pdf');
    });

    it('should remove leading dots', () => {
      expect(sanitizeFilename('.htaccess')).toBe('htaccess');
    });

    it('should prevent double dots', () => {
      expect(sanitizeFilename('file..txt')).toBe('file.txt');
    });
  });

  describe('sanitizeNumber', () => {
    it('should parse valid numbers', () => {
      expect(sanitizeNumber('42')).toBe(42);
      expect(sanitizeNumber(42)).toBe(42);
    });

    it('should respect min/max bounds', () => {
      expect(sanitizeNumber(100, { min: 0, max: 50 })).toBe(50);
      expect(sanitizeNumber(-10, { min: 0 })).toBe(0);
    });

    it('should return default for invalid input', () => {
      expect(sanitizeNumber('invalid', { defaultValue: 0 })).toBe(0);
      expect(sanitizeNumber(NaN, { defaultValue: 10 })).toBe(10);
    });

    it('should handle float/integer option', () => {
      expect(sanitizeNumber(3.7, { allowFloat: false })).toBe(3);
      expect(sanitizeNumber(3.7, { allowFloat: true })).toBe(3.7);
    });
  });

  describe('sanitizePhone', () => {
    it('should extract digits from phone number', () => {
      expect(sanitizePhone('(809) 123-4567')).toBe('8091234567');
    });

    it('should handle country code', () => {
      expect(sanitizePhone('+1 809 123 4567')).toBe('8091234567');
      expect(sanitizePhone('1-809-123-4567')).toBe('8091234567');
    });

    it('should limit to 10 digits', () => {
      expect(sanitizePhone('123456789012345')).toBe('1234567890');
    });
  });

  describe('sanitizeEmail', () => {
    it('should lowercase and trim email', () => {
      expect(sanitizeEmail('  TEST@EXAMPLE.COM  ')).toBe('test@example.com');
    });

    it('should return empty for invalid emails', () => {
      expect(sanitizeEmail('invalid')).toBe('');
      expect(sanitizeEmail('no@domain')).toBe('');
    });

    it('should validate basic email format', () => {
      expect(sanitizeEmail('user@example.com')).toBe('user@example.com');
    });
  });

  describe('sanitizeRNC', () => {
    it('should extract 9-digit RNC', () => {
      expect(sanitizeRNC('101-12345-6')).toBe('101123456');
    });

    it('should extract 11-digit RNC (cédula)', () => {
      expect(sanitizeRNC('001-1234567-8')).toBe('00112345678');
    });

    it('should return empty for invalid RNC', () => {
      expect(sanitizeRNC('12345')).toBe('');
      expect(sanitizeRNC('1234567890123')).toBe('');
    });
  });

  describe('sanitizePlate', () => {
    it('should uppercase and remove invalid chars', () => {
      expect(sanitizePlate('a-123-bc')).toBe('A123BC');
    });

    it('should limit to 7 characters', () => {
      expect(sanitizePlate('ABCDEFGHIJ')).toBe('ABCDEFG');
    });
  });

  describe('sanitizeVIN', () => {
    it('should uppercase valid VIN', () => {
      const vin = '1hgbh41jxmn109186';
      expect(sanitizeVIN(vin)).toBe('1HGBH41JXMN109186');
    });

    it('should remove invalid VIN characters (I, O, Q)', () => {
      expect(sanitizeVIN('1HGIOQJXMN109186')).toBe('');
    });

    it('should return empty for wrong length', () => {
      expect(sanitizeVIN('TOOSHORT')).toBe('');
      expect(sanitizeVIN('WAYTOOLONGFORTHISTOWORK')).toBe('');
    });
  });

  describe('sanitizePrice', () => {
    it('should sanitize valid prices', () => {
      expect(sanitizePrice(1500000)).toBe(1500000);
      expect(sanitizePrice('2000000')).toBe(2000000);
    });

    it('should enforce price limits', () => {
      expect(sanitizePrice(-100)).toBe(0);
      expect(sanitizePrice(500000000)).toBe(100000000);
    });
  });

  describe('sanitizeYear', () => {
    it('should sanitize valid years', () => {
      expect(sanitizeYear(2024)).toBe(2024);
      expect(sanitizeYear('2020')).toBe(2020);
    });

    it('should enforce year limits', () => {
      expect(sanitizeYear(1800)).toBe(1900);
      const currentYear = new Date().getFullYear();
      expect(sanitizeYear(currentYear + 10)).toBe(currentYear + 2);
    });
  });

  describe('sanitizeMileage', () => {
    it('should sanitize valid mileage', () => {
      expect(sanitizeMileage(50000)).toBe(50000);
      expect(sanitizeMileage('100000')).toBe(100000);
    });

    it('should enforce mileage limits', () => {
      expect(sanitizeMileage(-500)).toBe(0);
      expect(sanitizeMileage(5000000)).toBe(2000000);
    });
  });

  describe('sanitizeText', () => {
    it('should strip HTML tags', () => {
      expect(sanitizeText('<p>Hello</p>')).toBe('Hello');
    });

    it('should respect maxLength', () => {
      const longText = 'a'.repeat(100);
      expect(sanitizeText(longText, { maxLength: 50 }).length).toBe(50);
    });

    it('should optionally remove newlines', () => {
      expect(sanitizeText('Hello\nWorld', { allowNewlines: false })).toBe('Hello World');
      expect(sanitizeText('Hello\nWorld', { allowNewlines: true })).toBe('Hello\nWorld');
    });
  });
});
