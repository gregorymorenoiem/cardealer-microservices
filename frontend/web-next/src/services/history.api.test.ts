/**
 * Viewing History Service Tests
 *
 * Tests for tracking and managing vehicle viewing history
 * @see src/services/history.ts
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

// Mock the api-client module
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
  },
}));

import { apiClient } from '@/lib/api-client';
import {
  getHistory,
  recordView,
  removeFromHistory,
  clearHistory,
  type ViewingHistoryResponse,
} from './history';

// =============================================================================
// MOCK DATA
// =============================================================================

const mockHistoryResponse: ViewingHistoryResponse = {
  items: [
    {
      id: 'view-1',
      vehicleId: 'vehicle-1',
      viewedAt: '2024-01-15T10:00:00Z',
      vehicle: {
        id: 'vehicle-1',
        slug: 'toyota-corolla-2022',
        title: 'Toyota Corolla 2022',
        make: 'Toyota',
        model: 'Corolla',
        year: 2022,
        price: 1200000,
        mileage: 25000,
        location: 'Santo Domingo',
        imageUrl: 'https://example.com/car.jpg',
        dealerName: 'Auto Dealer RD',
        status: 'active',
      },
      isFavorite: true,
    },
    {
      id: 'view-2',
      vehicleId: 'vehicle-2',
      viewedAt: '2024-01-14T15:30:00Z',
      vehicle: {
        id: 'vehicle-2',
        slug: 'honda-civic-2023',
        title: 'Honda Civic 2023',
        make: 'Honda',
        model: 'Civic',
        year: 2023,
        price: 1500000,
        mileage: 10000,
        location: 'Santiago',
        imageUrl: 'https://example.com/car2.jpg',
        dealerName: 'Honda Santiago',
        status: 'active',
      },
      isFavorite: false,
    },
  ],
  total: 2,
  totalFavorites: 1,
  oldestDate: '2024-01-14T15:30:00Z',
};

// =============================================================================
// TEST SETUP
// =============================================================================

describe('History Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  // ===========================================================================
  // GET HISTORY
  // ===========================================================================

  describe('getHistory', () => {
    it('should fetch viewing history', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockHistoryResponse });

      const result = await getHistory();

      expect(apiClient.get).toHaveBeenCalledWith('/api/history/views', { params: undefined });
      expect(result.items).toHaveLength(2);
      expect(result.total).toBe(2);
    });

    it('should fetch history with pagination params', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockHistoryResponse });

      await getHistory({ page: 1, pageSize: 10 });

      expect(apiClient.get).toHaveBeenCalledWith('/api/history/views', {
        params: { page: 1, pageSize: 10 },
      });
    });

    it('should fetch history with days filter', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockHistoryResponse });

      await getHistory({ days: 7 });

      expect(apiClient.get).toHaveBeenCalledWith('/api/history/views', {
        params: { days: 7 },
      });
    });

    it('should include vehicle details in response', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({ data: mockHistoryResponse });

      const result = await getHistory();

      expect(result.items[0].vehicle.title).toBe('Toyota Corolla 2022');
      expect(result.items[0].vehicle.price).toBe(1200000);
      expect(result.items[0].isFavorite).toBe(true);
    });

    it('should handle empty history', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({
        data: {
          items: [],
          total: 0,
          totalFavorites: 0,
          oldestDate: null,
        },
      });

      const result = await getHistory();

      expect(result.items).toHaveLength(0);
      expect(result.total).toBe(0);
    });
  });

  // ===========================================================================
  // RECORD VIEW
  // ===========================================================================

  describe('recordView', () => {
    it('should record a vehicle view for authenticated user', async () => {
      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: {} });

      await recordView('vehicle-123');

      expect(apiClient.post).toHaveBeenCalledWith('/api/history/views/vehicle-123');
    });
  });

  // ===========================================================================
  // REMOVE FROM HISTORY
  // ===========================================================================

  describe('removeFromHistory', () => {
    it('should remove vehicle from history', async () => {
      vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: {} });

      await removeFromHistory('vehicle-123');

      expect(apiClient.delete).toHaveBeenCalledWith('/api/history/views/vehicle-123');
    });
  });

  // ===========================================================================
  // CLEAR HISTORY
  // ===========================================================================

  describe('clearHistory', () => {
    it('should clear all history', async () => {
      vi.mocked(apiClient.delete).mockResolvedValueOnce({ data: {} });

      await clearHistory();

      expect(apiClient.delete).toHaveBeenCalledWith('/api/history/views');
    });
  });
});
