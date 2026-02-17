/**
 * API URL Configuration for BFF (Backend for Frontend) Pattern
 *
 * In production, the Gateway is NOT exposed to the internet.
 * All API traffic flows: Browser → Next.js (okla.com.do) → Gateway (internal network)
 *
 * - Client-side (browser): Uses empty baseURL → relative paths → Next.js rewrites proxy to Gateway
 * - Server-side (SSR, API routes, middleware): Uses INTERNAL_API_URL (http://gateway:8080)
 *
 * Environment variables:
 * - NEXT_PUBLIC_API_URL: Build-time variable for client-side. Empty in prod, http://localhost:18443 in dev.
 * - INTERNAL_API_URL: Runtime variable for server-side only. http://gateway:8080 in prod.
 */

const isServer = typeof window === 'undefined';

/**
 * Get the API base URL for client-side code (browser).
 *
 * - Production: returns empty string → relative URLs → proxied by Next.js rewrites
 * - Development: returns http://localhost:18443 → direct to Gateway
 *
 * Uses `??` (nullish coalescing) instead of `||` so empty string is preserved.
 */
export function getClientApiUrl(): string {
  return process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:18443';
}

/**
 * Get the API base URL for server-side code (SSR, API routes, middleware).
 *
 * - Production: returns INTERNAL_API_URL (http://gateway:8080) → direct internal call
 * - Development: returns NEXT_PUBLIC_API_URL (http://localhost:18443) → dev Gateway
 *
 * ⚠️ ONLY use this in server-side code (typeof window === 'undefined')
 */
export function getInternalApiUrl(): string {
  return (
    process.env.INTERNAL_API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:18443'
  );
}

/**
 * Get the correct API base URL for the current execution context.
 *
 * Automatically detects server vs client:
 * - Server: Uses INTERNAL_API_URL for direct internal network calls
 * - Client: Uses NEXT_PUBLIC_API_URL (empty in prod = same-origin via rewrites)
 */
export function getApiBaseUrl(): string {
  if (isServer) {
    return getInternalApiUrl();
  }
  return getClientApiUrl();
}
