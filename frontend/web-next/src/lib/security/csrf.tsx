'use client';

import React, { useCallback, useEffect, useState } from 'react';

/**
 * CSRF Protection Utilities
 *
 * Provides client-side CSRF token management for forms and API requests.
 * Works in conjunction with server-side CSRF validation.
 */

const CSRF_COOKIE_NAME = 'csrf_token';
const CSRF_HEADER_NAME = 'X-CSRF-Token';
const CSRF_TOKEN_LENGTH = 32;

/**
 * Generate a cryptographically secure random token
 */
function generateToken(length: number = CSRF_TOKEN_LENGTH): string {
  if (typeof window !== 'undefined' && window.crypto) {
    const array = new Uint8Array(length);
    window.crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }
  // Fallback for SSR or environments without crypto
  return Math.random().toString(36).slice(2) + Date.now().toString(36);
}

/**
 * Get CSRF token from cookie
 */
function getTokenFromCookie(): string | null {
  if (typeof document === 'undefined') return null;

  const match = document.cookie.match(new RegExp(`(^| )${CSRF_COOKIE_NAME}=([^;]+)`));
  return match ? decodeURIComponent(match[2]) : null;
}

/**
 * Set CSRF token in cookie
 */
function setTokenInCookie(token: string): void {
  if (typeof document === 'undefined') return;

  const secure = window.location.protocol === 'https:' ? '; Secure' : '';
  document.cookie = `${CSRF_COOKIE_NAME}=${encodeURIComponent(token)}; Path=/; SameSite=Strict${secure}`;
}

/**
 * Get or create CSRF token
 */
export function getCsrfToken(): string {
  let token = getTokenFromCookie();

  if (!token) {
    token = generateToken();
    setTokenInCookie(token);
  }

  return token;
}

/**
 * Refresh CSRF token
 */
export function refreshCsrfToken(): string {
  const token = generateToken();
  setTokenInCookie(token);
  return token;
}

/**
 * Get headers with CSRF token
 */
export function getCsrfHeaders(): Record<string, string> {
  return {
    [CSRF_HEADER_NAME]: getCsrfToken(),
  };
}

/**
 * React hook for CSRF token management
 *
 * @example
 * const { token, headers, refresh } = useCsrfToken();
 *
 * // Use in form
 * <input type="hidden" name="csrf" value={token} />
 *
 * // Use in fetch
 * fetch('/api/action', { headers })
 */
export function useCsrfToken() {
  const [token, setToken] = useState<string>('');

  useEffect(() => {
    setToken(getCsrfToken());
  }, []);

  const refresh = useCallback(() => {
    const newToken = refreshCsrfToken();
    setToken(newToken);
    return newToken;
  }, []);

  const headers = {
    [CSRF_HEADER_NAME]: token,
  };

  return {
    token,
    headers,
    refresh,
    headerName: CSRF_HEADER_NAME,
  };
}

/**
 * CSRF-protected fetch wrapper
 */
export async function csrfFetch(url: string, options: RequestInit = {}): Promise<Response> {
  const headers = new Headers(options.headers);
  headers.set(CSRF_HEADER_NAME, getCsrfToken());

  return fetch(url, {
    ...options,
    headers,
    credentials: 'same-origin', // Include cookies
  });
}

/**
 * Hidden CSRF input component for forms
 */
export function CsrfInput() {
  const { token } = useCsrfToken();
  return <input type="hidden" name="csrf" value={token} />;
}

/**
 * Validate that a token matches the expected format
 */
export function isValidTokenFormat(token: string): boolean {
  if (!token || typeof token !== 'string') return false;
  // Token should be hex string of expected length
  return /^[a-f0-9]+$/i.test(token) && token.length >= CSRF_TOKEN_LENGTH;
}

/**
 * Double-submit cookie pattern helper
 * Compares token from header/body with token from cookie
 */
export function validateDoubleSubmit(
  headerOrBodyToken: string | null,
  cookieToken: string | null
): boolean {
  if (!headerOrBodyToken || !cookieToken) return false;
  if (!isValidTokenFormat(headerOrBodyToken) || !isValidTokenFormat(cookieToken)) return false;

  // Timing-safe comparison
  if (headerOrBodyToken.length !== cookieToken.length) return false;

  let result = 0;
  for (let i = 0; i < headerOrBodyToken.length; i++) {
    result |= headerOrBodyToken.charCodeAt(i) ^ cookieToken.charCodeAt(i);
  }
  return result === 0;
}

export default {
  getCsrfToken,
  refreshCsrfToken,
  getCsrfHeaders,
  useCsrfToken,
  csrfFetch,
  CsrfInput,
  isValidTokenFormat,
  validateDoubleSubmit,
  CSRF_HEADER_NAME,
};
