import { afterEach, afterAll, beforeAll, vi } from 'vitest';
import { cleanup } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';
import { server } from '../mocks/server';

// ============================
// Mock localStorage
// ============================
const localStorageMock = {
  store: {} as Record<string, string>,
  getItem: vi.fn((key: string) => localStorageMock.store[key] || null),
  setItem: vi.fn((key: string, value: string) => {
    localStorageMock.store[key] = value;
  }),
  removeItem: vi.fn((key: string) => {
    delete localStorageMock.store[key];
  }),
  clear: vi.fn(() => {
    localStorageMock.store = {};
  }),
};
Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

// ============================
// Mock i18next
// ============================
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: {
      language: 'en',
      changeLanguage: vi.fn(),
    },
  }),
  Trans: ({ children }: { children: React.ReactNode }) => children,
  initReactI18next: {
    type: '3rdParty',
    init: vi.fn(),
  },
}));

// ============================
// Mock useCompare hook
// ============================
vi.mock('@/hooks/useCompare', () => ({
  useCompare: () => ({
    compareItems: [],
    count: 0,
    addToCompare: vi.fn(),
    removeFromCompare: vi.fn(),
    clearCompare: vi.fn(),
    isInCompare: vi.fn().mockReturnValue(false),
  }),
}));

// ============================
// Mock useFavorites hook
// ============================
vi.mock('@/hooks/useFavorites', () => ({
  useFavorites: () => ({
    favorites: [],
    addFavorite: vi.fn(),
    removeFavorite: vi.fn(),
    toggleFavorite: vi.fn(),
    isFavorite: vi.fn().mockReturnValue(false),
    count: 0,
  }),
}));

// ============================
// Mock useSearch hooks
// ============================
vi.mock('@/hooks/useSearch', () => ({
  useSearchPage: () => ({
    vehicles: [],
    total: 0,
    totalPages: 1,
    isLoading: false,
    isError: false,
    savedSearches: [],
    recentSearches: [],
  }),
  useAddRecentSearch: () => ({
    mutate: vi.fn(),
  }),
  useCreateSavedSearch: () => ({
    mutate: vi.fn(),
    isPending: false,
  }),
  useSavedSearches: () => ({
    data: [],
    isLoading: false,
  }),
  useRecentSearches: () => ({
    data: [],
    isLoading: false,
  }),
}));

// ============================
// Mock window.matchMedia
// ============================
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// ============================
// Mock IntersectionObserver
// ============================
class MockIntersectionObserver {
  observe = vi.fn();
  disconnect = vi.fn();
  unobserve = vi.fn();
}
Object.defineProperty(window, 'IntersectionObserver', {
  writable: true,
  value: MockIntersectionObserver,
});

// ============================
// Mock window methods
// ============================
window.scrollTo = vi.fn();
window.alert = vi.fn();

// ============================
// MSW Server Setup
// ============================
beforeAll(() => {
  server.listen({ onUnhandledRequest: 'warn' });
});

afterEach(() => {
  cleanup();
  server.resetHandlers();
  localStorageMock.clear();
});

afterAll(() => {
  server.close();
});
