/**
 * MSW Browser - Development Mock API
 * 
 * This file sets up MSW for browser environment.
 * Used for mocking API calls during development.
 */

import { setupWorker } from 'msw/browser';
import { handlers } from './handlers';

/**
 * MSW worker instance for browser
 */
export const worker = setupWorker(...handlers);

/**
 * Start the mock service worker
 * Call this in main.tsx to enable API mocking in development
 */
export async function startMockWorker(): Promise<void> {
  if (import.meta.env.DEV && import.meta.env.VITE_ENABLE_MSW === 'true') {
    await worker.start({
      onUnhandledRequest: 'bypass',
      quiet: false,
    });
    console.log('[MSW] Mock Service Worker started');
  }
}

export default worker;
