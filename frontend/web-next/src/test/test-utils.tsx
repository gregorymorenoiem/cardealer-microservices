/**
 * Test Utilities
 * Custom render function with providers and helper utilities
 */

import * as React from 'react';
import { render, RenderOptions, RenderResult } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from '@/hooks/use-auth';
import type { User } from '@/types';

// =============================================================================
// MOCK USER DATA
// =============================================================================

export const mockAuthenticatedUser: User = {
  id: 'user-123',
  email: 'test@example.com',
  firstName: 'Juan',
  lastName: 'Pérez',
  fullName: 'Juan Pérez',
  avatarUrl: 'https://example.com/avatar.jpg',
  phone: '+1809555001',
  accountType: 'buyer',
  isVerified: true,
  isEmailVerified: true,
  isPhoneVerified: false,
  preferredLocale: 'es-DO',
  preferredCurrency: 'DOP',
  createdAt: '2024-01-15T10:00:00Z',
  lastLoginAt: '2026-02-01T08:30:00Z',
};

export const mockDealerUser: User = {
  ...mockAuthenticatedUser,
  id: 'dealer-user-123',
  accountType: 'dealer',
};

export const mockAdminUser: User = {
  ...mockAuthenticatedUser,
  id: 'admin-user-123',
  accountType: 'admin',
};

// =============================================================================
// QUERY CLIENT FOR TESTS
// =============================================================================

export function createTestQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
        staleTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

// =============================================================================
// ALL PROVIDERS WRAPPER
// =============================================================================

interface AllProvidersProps {
  children: React.ReactNode;
  queryClient?: QueryClient;
  initialUser?: User | null;
}

export function AllProviders({
  children,
  queryClient,
  initialUser = null,
}: AllProvidersProps): React.JSX.Element {
  const client = queryClient || createTestQueryClient();

  return (
    <QueryClientProvider client={client}>
      <AuthProvider initialUser={initialUser}>{children}</AuthProvider>
    </QueryClientProvider>
  );
}

// =============================================================================
// CUSTOM RENDER FUNCTION
// =============================================================================

interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  queryClient?: QueryClient;
  initialUser?: User | null;
}

interface CustomRenderResult extends RenderResult {
  user: ReturnType<typeof userEvent.setup>;
  queryClient: QueryClient;
}

/**
 * Custom render function that wraps component with all providers
 */
export function customRender(
  ui: React.ReactElement,
  options: CustomRenderOptions = {}
): CustomRenderResult {
  const { queryClient = createTestQueryClient(), initialUser = null, ...renderOptions } = options;

  const user = userEvent.setup();

  const Wrapper = ({ children }: { children: React.ReactNode }) => (
    <AllProviders queryClient={queryClient} initialUser={initialUser}>
      {children}
    </AllProviders>
  );

  const result = render(ui, { wrapper: Wrapper, ...renderOptions });

  return {
    ...result,
    user,
    queryClient,
  };
}

// =============================================================================
// RENDER WITH AUTH
// =============================================================================

/**
 * Render with authenticated user
 */
export function renderWithAuth(
  ui: React.ReactElement,
  options: Omit<CustomRenderOptions, 'initialUser'> & { user?: User } = {}
): CustomRenderResult {
  const { user = mockAuthenticatedUser, ...rest } = options;
  return customRender(ui, { ...rest, initialUser: user });
}

/**
 * Render with dealer user
 */
export function renderWithDealer(
  ui: React.ReactElement,
  options: Omit<CustomRenderOptions, 'initialUser'> = {}
): CustomRenderResult {
  return customRender(ui, { ...options, initialUser: mockDealerUser });
}

/**
 * Render with admin user
 */
export function renderWithAdmin(
  ui: React.ReactElement,
  options: Omit<CustomRenderOptions, 'initialUser'> = {}
): CustomRenderResult {
  return customRender(ui, { ...options, initialUser: mockAdminUser });
}

// =============================================================================
// HELPER FUNCTIONS
// =============================================================================

/**
 * Wait for loading state to finish
 */
export async function waitForLoadingToFinish(): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, 0));
}

/**
 * Create mock function with typed return value
 */
export function createMockFn<T extends (...args: Parameters<T>) => ReturnType<T>>() {
  return vi.fn() as unknown as T;
}

/**
 * Mock localStorage
 */
export function mockLocalStorage(): {
  getItem: ReturnType<typeof vi.fn>;
  setItem: ReturnType<typeof vi.fn>;
  removeItem: ReturnType<typeof vi.fn>;
  clear: ReturnType<typeof vi.fn>;
} {
  const store: Record<string, string> = {};

  const getItem = vi.fn((key: string) => store[key] || null);
  const setItem = vi.fn((key: string, value: string) => {
    store[key] = value;
  });
  const removeItem = vi.fn((key: string) => {
    delete store[key];
  });
  const clear = vi.fn(() => {
    Object.keys(store).forEach(key => delete store[key]);
  });

  Object.defineProperty(window, 'localStorage', {
    value: { getItem, setItem, removeItem, clear },
    writable: true,
  });

  return { getItem, setItem, removeItem, clear };
}

/**
 * Set auth tokens in localStorage
 */
export function setAuthTokens(
  accessToken = 'test-access-token',
  refreshToken = 'test-refresh-token'
): void {
  if (typeof window !== 'undefined') {
    localStorage.setItem('okla_access_token', accessToken);
    localStorage.setItem('okla_refresh_token', refreshToken);
  }
}

/**
 * Clear auth tokens from localStorage
 */
export function clearAuthTokens(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('okla_access_token');
    localStorage.removeItem('okla_refresh_token');
  }
}

// =============================================================================
// RE-EXPORT EVERYTHING FROM RTL
// =============================================================================

export * from '@testing-library/react';
export { userEvent };

// Default export is custom render
export { customRender as render };
