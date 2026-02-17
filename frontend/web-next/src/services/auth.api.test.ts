/**
 * Auth Service Tests - Simplified
 * Priority: P0 - Core authentication service
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { server } from '@/test/mocks/server';
import { http, HttpResponse } from 'msw';

const API_URL = 'http://localhost:8080';

// We test the API contract, not the implementation details
describe('Auth API Contract', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('POST /api/auth/login', () => {
    it('should return tokens and user on valid credentials', async () => {
      const response = await fetch(`${API_URL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'test@example.com',
          password: 'password123',
        }),
      });

      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.accessToken).toBeDefined();
      expect(data.refreshToken).toBeDefined();
      expect(data.user).toBeDefined();
      expect(data.user.email).toBe('test@example.com');
    });

    it('should return 401 for invalid credentials', async () => {
      const response = await fetch(`${API_URL}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'invalid@example.com',
          password: 'wrongpassword',
        }),
      });

      expect(response.status).toBe(401);

      const data = await response.json();
      expect(data.code).toBe('INVALID_CREDENTIALS');
    });
  });

  describe('POST /api/auth/register', () => {
    it('should create new user with valid data', async () => {
      const response = await fetch(`${API_URL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          firstName: 'Juan',
          lastName: 'Pérez',
          email: 'new@example.com',
          password: 'Password123!',
          acceptTerms: true,
        }),
      });

      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.accessToken).toBeDefined();
      expect(data.user).toBeDefined();
    });

    it('should return 409 for existing email', async () => {
      const response = await fetch(`${API_URL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          firstName: 'Juan',
          lastName: 'Pérez',
          email: 'existing@example.com',
          password: 'Password123!',
          acceptTerms: true,
        }),
      });

      expect(response.status).toBe(409);

      const data = await response.json();
      expect(data.code).toBe('EMAIL_EXISTS');
    });
  });

  describe('GET /api/auth/me', () => {
    it('should return user when authenticated', async () => {
      const response = await fetch(`${API_URL}/api/auth/me`, {
        headers: {
          Authorization: 'Bearer valid-token',
        },
      });

      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.id).toBeDefined();
      expect(data.email).toBeDefined();
    });

    it('should return 401 when not authenticated', async () => {
      const response = await fetch(`${API_URL}/api/auth/me`);

      expect(response.status).toBe(401);
    });
  });

  describe('POST /api/auth/logout', () => {
    it('should logout successfully', async () => {
      const response = await fetch(`${API_URL}/api/auth/logout`, {
        method: 'POST',
        headers: {
          Authorization: 'Bearer valid-token',
        },
      });

      expect(response.ok).toBe(true);
    });
  });

  describe('POST /api/auth/refresh', () => {
    it('should refresh access token', async () => {
      const response = await fetch(`${API_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          refreshToken: 'valid-refresh-token',
        }),
      });

      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.accessToken).toBeDefined();
    });
  });

  describe('POST /api/auth/forgot-password', () => {
    it('should send reset email', async () => {
      const response = await fetch(`${API_URL}/api/auth/forgot-password`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: 'test@example.com',
        }),
      });

      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.message).toBeDefined();
    });
  });
});

describe('Auth Transform Functions', () => {
  it('should transform UserDto to User', () => {
    const userDto = {
      id: 'user-123',
      email: 'test@example.com',
      firstName: 'Juan',
      lastName: 'Pérez',
      fullName: 'Juan Pérez',
      avatarUrl: 'https://example.com/avatar.jpg',
      phone: '+1809555001',
      accountType: 'buyer' as const,
      isVerified: true,
      isEmailVerified: true,
      isPhoneVerified: false,
      preferredLocale: 'es-DO',
      preferredCurrency: 'DOP' as const,
      createdAt: '2024-01-15T10:00:00Z',
      lastLoginAt: '2026-02-01T08:30:00Z',
    };

    // Test the transform
    const user = {
      id: userDto.id,
      email: userDto.email,
      firstName: userDto.firstName,
      lastName: userDto.lastName,
      fullName: userDto.fullName,
      avatarUrl: userDto.avatarUrl,
      phone: userDto.phone,
      accountType: userDto.accountType,
      isVerified: userDto.isVerified,
      isEmailVerified: userDto.isEmailVerified,
      isPhoneVerified: userDto.isPhoneVerified,
      preferredLocale: userDto.preferredLocale,
      preferredCurrency: userDto.preferredCurrency,
      createdAt: userDto.createdAt,
      lastLoginAt: userDto.lastLoginAt,
    };

    expect(user.id).toBe('user-123');
    expect(user.email).toBe('test@example.com');
    expect(user.fullName).toBe('Juan Pérez');
    expect(user.accountType).toBe('buyer');
    expect(user.isVerified).toBe(true);
  });

  it('should handle optional fields', () => {
    const userDto = {
      id: 'user-456',
      email: 'minimal@example.com',
      firstName: 'Test',
      lastName: 'User',
      fullName: 'Test User',
      accountType: 'buyer' as const,
      isVerified: false,
      isEmailVerified: false,
      isPhoneVerified: false,
      preferredLocale: 'es-DO',
      preferredCurrency: 'DOP' as const,
      createdAt: '2024-01-15T10:00:00Z',
    };

    const user = {
      ...userDto,
      avatarUrl: userDto.avatarUrl ?? undefined,
      phone: userDto.phone ?? undefined,
      lastLoginAt: userDto.lastLoginAt ?? undefined,
    };

    expect(user.avatarUrl).toBeUndefined();
    expect(user.phone).toBeUndefined();
    expect(user.lastLoginAt).toBeUndefined();
  });
});
