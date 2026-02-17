/**
 * MSW Server Setup
 * Used for Node.js environment (Vitest)
 */

import { setupServer } from 'msw/node';
import { handlers } from './handlers';

// Create MSW server with all handlers
export const server = setupServer(...handlers);
