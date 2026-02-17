/**
 * MSW Server - Test Environment Setup
 * 
 * This file sets up MSW for Node.js environment (Vitest).
 * Used for mocking API calls during testing.
 */

import { setupServer } from 'msw/node';
import { handlers } from './handlers';

/**
 * MSW server instance for testing
 */
export const server = setupServer(...handlers);

/**
 * Helper to reset handlers to defaults
 */
export function resetHandlers(): void {
  server.resetHandlers();
}

/**
 * Helper to add custom handlers for specific tests
 */
export function useHandlers(...customHandlers: Parameters<typeof server.use>): void {
  server.use(...customHandlers);
}

export default server;
