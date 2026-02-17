'use client';

import { useState, useCallback, useRef, useEffect } from 'react';

/**
 * Rate Limiting Utilities
 *
 * Client-side rate limiting to prevent abuse and provide better UX.
 * Note: Always implement server-side rate limiting as the primary defense.
 */

interface RateLimitConfig {
  /** Maximum number of requests allowed */
  maxRequests: number;
  /** Time window in milliseconds */
  windowMs: number;
  /** Cooldown period after limit reached (ms) */
  cooldownMs?: number;
}

interface RateLimitState {
  /** Whether rate limit has been exceeded */
  isLimited: boolean;
  /** Number of remaining requests */
  remaining: number;
  /** Time until rate limit resets (ms) */
  resetIn: number;
  /** Check if action is allowed and decrement counter */
  checkLimit: () => boolean;
  /** Reset the rate limiter */
  reset: () => void;
}

/**
 * Simple in-memory rate limiter for client-side use
 */
class RateLimiter {
  private tokens: number;
  private lastRefill: number;
  private readonly maxTokens: number;
  private readonly refillRate: number; // tokens per ms

  constructor(maxRequests: number, windowMs: number) {
    this.maxTokens = maxRequests;
    this.tokens = maxRequests;
    this.lastRefill = Date.now();
    this.refillRate = maxRequests / windowMs;
  }

  private refill(): void {
    const now = Date.now();
    const elapsed = now - this.lastRefill;
    const newTokens = elapsed * this.refillRate;
    this.tokens = Math.min(this.maxTokens, this.tokens + newTokens);
    this.lastRefill = now;
  }

  tryConsume(): boolean {
    this.refill();
    if (this.tokens >= 1) {
      this.tokens -= 1;
      return true;
    }
    return false;
  }

  getRemaining(): number {
    this.refill();
    return Math.floor(this.tokens);
  }

  getResetTime(): number {
    if (this.tokens >= this.maxTokens) return 0;
    const tokensNeeded = this.maxTokens - this.tokens;
    return Math.ceil(tokensNeeded / this.refillRate);
  }

  reset(): void {
    this.tokens = this.maxTokens;
    this.lastRefill = Date.now();
  }
}

/**
 * React hook for rate limiting actions
 *
 * @example
 * const { isLimited, remaining, checkLimit } = useRateLimit({
 *   maxRequests: 5,
 *   windowMs: 60000, // 5 requests per minute
 * });
 *
 * const handleSubmit = () => {
 *   if (!checkLimit()) {
 *     toast.error('Too many requests. Please wait.');
 *     return;
 *   }
 *   // Proceed with action
 * };
 */
export function useRateLimit(config: RateLimitConfig): RateLimitState {
  const { maxRequests, windowMs, cooldownMs = 0 } = config;

  const limiterRef = useRef<RateLimiter | null>(null);
  const [isLimited, setIsLimited] = useState(false);
  const [remaining, setRemaining] = useState(maxRequests);
  const [resetIn, setResetIn] = useState(0);
  const cooldownRef = useRef<NodeJS.Timeout | null>(null);

  // Initialize limiter
  useEffect(() => {
    limiterRef.current = new RateLimiter(maxRequests, windowMs);
    return () => {
      if (cooldownRef.current) {
        clearTimeout(cooldownRef.current);
      }
    };
  }, [maxRequests, windowMs]);

  // Update remaining count periodically
  useEffect(() => {
    const interval = setInterval(() => {
      if (limiterRef.current) {
        const newRemaining = limiterRef.current.getRemaining();
        setRemaining(newRemaining);
        setResetIn(limiterRef.current.getResetTime());

        if (newRemaining > 0 && isLimited && !cooldownRef.current) {
          setIsLimited(false);
        }
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [isLimited]);

  const checkLimit = useCallback((): boolean => {
    if (!limiterRef.current) return true;

    const allowed = limiterRef.current.tryConsume();
    const newRemaining = limiterRef.current.getRemaining();

    setRemaining(newRemaining);
    setResetIn(limiterRef.current.getResetTime());

    if (!allowed) {
      setIsLimited(true);

      // Apply cooldown if configured
      if (cooldownMs > 0 && !cooldownRef.current) {
        cooldownRef.current = setTimeout(() => {
          cooldownRef.current = null;
          setIsLimited(false);
        }, cooldownMs);
      }
    }

    return allowed;
  }, [cooldownMs]);

  const reset = useCallback(() => {
    if (limiterRef.current) {
      limiterRef.current.reset();
      setIsLimited(false);
      setRemaining(maxRequests);
      setResetIn(0);
    }
    if (cooldownRef.current) {
      clearTimeout(cooldownRef.current);
      cooldownRef.current = null;
    }
  }, [maxRequests]);

  return {
    isLimited,
    remaining,
    resetIn,
    checkLimit,
    reset,
  };
}

/**
 * Debounce function with rate limiting
 */
export function createDebouncedRateLimiter<T extends (...args: unknown[]) => unknown>(
  fn: T,
  debounceMs: number,
  config: RateLimitConfig
): (...args: Parameters<T>) => void {
  const limiter = new RateLimiter(config.maxRequests, config.windowMs);
  let timeoutId: NodeJS.Timeout | null = null;

  return (...args: Parameters<T>) => {
    if (timeoutId) {
      clearTimeout(timeoutId);
    }

    timeoutId = setTimeout(() => {
      if (limiter.tryConsume()) {
        fn(...args);
      }
      timeoutId = null;
    }, debounceMs);
  };
}

/**
 * Throttle function with rate limiting
 */
export function createThrottledRateLimiter<T extends (...args: unknown[]) => unknown>(
  fn: T,
  throttleMs: number,
  config: RateLimitConfig
): (...args: Parameters<T>) => void {
  const limiter = new RateLimiter(config.maxRequests, config.windowMs);
  let lastCall = 0;

  return (...args: Parameters<T>) => {
    const now = Date.now();

    if (now - lastCall >= throttleMs && limiter.tryConsume()) {
      lastCall = now;
      fn(...args);
    }
  };
}

/**
 * Pre-configured rate limiters for common use cases
 */
export const rateLimitPresets = {
  /** For form submissions - 3 per minute */
  formSubmit: { maxRequests: 3, windowMs: 60_000, cooldownMs: 10_000 },

  /** For search queries - 10 per minute */
  search: { maxRequests: 10, windowMs: 60_000 },

  /** For contact/messaging - 5 per 5 minutes */
  contact: { maxRequests: 5, windowMs: 300_000, cooldownMs: 30_000 },

  /** For auth attempts - 5 per 15 minutes */
  auth: { maxRequests: 5, windowMs: 900_000, cooldownMs: 60_000 },

  /** For favorites/likes - 30 per minute */
  favorites: { maxRequests: 30, windowMs: 60_000 },

  /** For page views tracking - 60 per minute */
  pageView: { maxRequests: 60, windowMs: 60_000 },

  /** For file uploads - 5 per 10 minutes */
  upload: { maxRequests: 5, windowMs: 600_000, cooldownMs: 30_000 },
} as const;

export default {
  useRateLimit,
  createDebouncedRateLimiter,
  createThrottledRateLimiter,
  rateLimitPresets,
};
