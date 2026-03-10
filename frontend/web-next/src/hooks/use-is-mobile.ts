'use client';

import { useSyncExternalStore } from 'react';

/**
 * Detects whether the current device is mobile (phone/tablet).
 * Used to conditionally render camera-specific UI in the PWA.
 * Returns `false` during SSR to avoid hydration mismatches.
 */

function getIsMobileSnapshot(): boolean {
  if (typeof navigator === 'undefined') return false;
  const ua = navigator.userAgent;
  if (/iPhone|iPad|iPod|Android/i.test(ua)) return true;
  if ('ontouchstart' in window && window.innerWidth < 1024) return true;
  return false;
}

function getServerSnapshot(): boolean {
  return false;
}

function subscribe(callback: () => void): () => void {
  // Re-check on resize (e.g. tablet rotation)
  window.addEventListener('resize', callback);
  return () => window.removeEventListener('resize', callback);
}

export function useIsMobile(): boolean {
  return useSyncExternalStore(subscribe, getIsMobileSnapshot, getServerSnapshot);
}
