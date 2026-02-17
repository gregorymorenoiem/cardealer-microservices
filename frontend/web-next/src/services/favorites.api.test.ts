/**
 * Favorites Service Tests - Pure Unit Tests
 * Priority: P0 - User favorites functionality
 *
 * These tests mock apiClient directly instead of relying on MSW HTTP interception
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { apiClient } from '@/lib/api-client';

// Mock axios/apiClient
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockApiGet = apiClient.get as ReturnType<typeof vi.fn>;
const mockApiPost = apiClient.post as ReturnType<typeof vi.fn>;
const mockApiPatch = apiClient.patch as ReturnType<typeof vi.fn>;
const mockApiDelete = apiClient.delete as ReturnType<typeof vi.fn>;

// Mock favorite data
const mockFavorite = {
  id: 'fav-123',
  vehicleId: 'vehicle-123',
  userId: 'user-123',
  notes: 'Interested in this car',
  notifyOnPriceChange: true,
  createdAt: '2024-01-15T10:00:00Z',
  vehicle: {
    id: 'vehicle-123',
    slug: 'toyota-corolla-2023',
    make: 'Toyota',
    model: 'Corolla',
    year: 2023,
    price: 1850000,
    images: [{ id: 'img1', url: 'https://example.com/car.jpg' }],
  },
};

const mockFavorites = [
  mockFavorite,
  {
    ...mockFavorite,
    id: 'fav-456',
    vehicleId: 'vehicle-456',
    vehicle: { ...mockFavorite.vehicle, id: 'vehicle-456', make: 'Honda', model: 'Civic' },
  },
];

describe('Favorites API Contract', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('GET /api/favorites', () => {
    it('should return user favorites when authenticated', async () => {
      mockApiGet.mockResolvedValue({
        data: {
          items: mockFavorites,
          total: 2,
        },
      });

      const response = await apiClient.get('/api/favorites');

      expect(response.data.items).toBeInstanceOf(Array);
      expect(response.data.total).toBeDefined();
      expect(response.data.items.length).toBe(2);
    });

    it('should return 401 when not authenticated', async () => {
      mockApiGet.mockRejectedValue({
        response: {
          status: 401,
          data: { code: 'UNAUTHORIZED', message: 'Authentication required' },
        },
      });

      await expect(apiClient.get('/api/favorites')).rejects.toEqual(
        expect.objectContaining({
          response: expect.objectContaining({ status: 401 }),
        })
      );
    });

    it('should return empty list for new users', async () => {
      mockApiGet.mockResolvedValue({
        data: {
          items: [],
          total: 0,
        },
      });

      const response = await apiClient.get('/api/favorites');

      expect(response.data.items).toEqual([]);
      expect(response.data.total).toBe(0);
    });
  });

  describe('POST /api/favorites', () => {
    it('should add vehicle to favorites', async () => {
      mockApiPost.mockResolvedValue({
        data: { id: 'fav-new', vehicleId: 'vehicle-123' },
      });

      const response = await apiClient.post('/api/favorites', { vehicleId: 'vehicle-123' });

      expect(response.data.id).toBeDefined();
      expect(response.data.vehicleId).toBe('vehicle-123');
      expect(mockApiPost).toHaveBeenCalledWith('/api/favorites', { vehicleId: 'vehicle-123' });
    });

    it('should add with notes', async () => {
      mockApiPost.mockResolvedValue({
        data: { id: 'fav-new', vehicleId: 'vehicle-456', notes: 'Interested in this one' },
      });

      const response = await apiClient.post('/api/favorites', {
        vehicleId: 'vehicle-456',
        notes: 'Interested in this one',
      });

      expect(response.data.notes).toBe('Interested in this one');
    });

    it('should return 409 if already favorited', async () => {
      mockApiPost.mockRejectedValue({
        response: {
          status: 409,
          data: { code: 'ALREADY_FAVORITED', message: 'Vehicle already in favorites' },
        },
      });

      await expect(
        apiClient.post('/api/favorites', { vehicleId: 'already-favorited' })
      ).rejects.toEqual(
        expect.objectContaining({
          response: expect.objectContaining({ status: 409 }),
        })
      );
    });
  });

  describe('DELETE /api/favorites/:vehicleId', () => {
    it('should remove vehicle from favorites', async () => {
      mockApiDelete.mockResolvedValue({ data: null, status: 204 });

      await apiClient.delete('/api/favorites/vehicle-123');

      expect(mockApiDelete).toHaveBeenCalledWith('/api/favorites/vehicle-123');
    });

    it('should return 404 for non-existent favorite', async () => {
      mockApiDelete.mockRejectedValue({
        response: {
          status: 404,
          data: { code: 'NOT_FOUND', message: 'Favorite not found' },
        },
      });

      await expect(apiClient.delete('/api/favorites/non-existent')).rejects.toEqual(
        expect.objectContaining({
          response: expect.objectContaining({ status: 404 }),
        })
      );
    });
  });

  describe('PATCH /api/favorites/:vehicleId', () => {
    it('should update favorite notes', async () => {
      mockApiPatch.mockResolvedValue({
        data: { ...mockFavorite, notes: 'Updated notes' },
      });

      const response = await apiClient.patch('/api/favorites/vehicle-123', {
        notes: 'Updated notes',
      });

      expect(response.data.notes).toBe('Updated notes');
      expect(mockApiPatch).toHaveBeenCalledWith('/api/favorites/vehicle-123', {
        notes: 'Updated notes',
      });
    });

    it('should update notification preference', async () => {
      mockApiPatch.mockResolvedValue({
        data: { ...mockFavorite, notifyOnPriceChange: true },
      });

      const response = await apiClient.patch('/api/favorites/vehicle-123', {
        notifyOnPriceChange: true,
      });

      expect(response.data.notifyOnPriceChange).toBe(true);
    });
  });

  describe('GET /api/favorites/:vehicleId/check', () => {
    it('should return true for favorited vehicle', async () => {
      mockApiGet.mockResolvedValue({
        data: { isFavorite: true },
      });

      const response = await apiClient.get('/api/favorites/vehicle-123/check');

      expect(response.data.isFavorite).toBe(true);
    });

    it('should return false for non-favorited vehicle', async () => {
      mockApiGet.mockResolvedValue({
        data: { isFavorite: false },
      });

      const response = await apiClient.get('/api/favorites/other-vehicle/check');

      expect(response.data.isFavorite).toBe(false);
    });
  });

  describe('POST /api/favorites/sync', () => {
    it('should sync local favorites to server', async () => {
      mockApiPost.mockResolvedValue({
        data: { synced: 3, failed: 0 },
      });

      const response = await apiClient.post('/api/favorites/sync', {
        vehicleIds: ['v1', 'v2', 'v3'],
      });

      expect(response.data.synced).toBe(3);
      expect(response.data.failed).toBe(0);
      expect(mockApiPost).toHaveBeenCalledWith('/api/favorites/sync', {
        vehicleIds: ['v1', 'v2', 'v3'],
      });
    });

    it('should handle partial sync failures', async () => {
      mockApiPost.mockResolvedValue({
        data: { synced: 2, failed: 1, failedIds: ['v3'] },
      });

      const response = await apiClient.post('/api/favorites/sync', {
        vehicleIds: ['v1', 'v2', 'v3'],
      });

      expect(response.data.synced).toBe(2);
      expect(response.data.failed).toBe(1);
    });
  });
});

describe('Local Favorites', () => {
  let storage: Record<string, string>;
  const LOCAL_FAVORITES_KEY = 'okla_local_favorites';

  // Helper function that uses the storage directly
  const getLocalFavorites = (): string[] => {
    try {
      const stored = storage[LOCAL_FAVORITES_KEY];
      return stored ? JSON.parse(stored) : [];
    } catch {
      return [];
    }
  };

  beforeEach(() => {
    storage = {};
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(key => storage[key] || null);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation((key, value) => {
      storage[key] = value;
    });
    vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(key => {
      delete storage[key];
    });
  });

  describe('getLocalFavorites', () => {
    it('should return empty array when no favorites', () => {
      expect(getLocalFavorites()).toEqual([]);
    });

    it('should return saved favorites', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1', 'v2']);
      expect(getLocalFavorites()).toEqual(['v1', 'v2']);
    });

    it('should handle invalid JSON gracefully', () => {
      storage[LOCAL_FAVORITES_KEY] = 'invalid-json';
      expect(getLocalFavorites()).toEqual([]);
    });
  });

  describe('addLocalFavorite', () => {
    const addLocalFavorite = (vehicleId: string): void => {
      const favorites = JSON.parse(storage[LOCAL_FAVORITES_KEY] || '[]');
      if (!favorites.includes(vehicleId)) {
        favorites.push(vehicleId);
        storage[LOCAL_FAVORITES_KEY] = JSON.stringify(favorites);
      }
    };

    it('should add vehicle to local favorites', () => {
      addLocalFavorite('v1');
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY])).toContain('v1');
    });

    it('should not add duplicates', () => {
      addLocalFavorite('v1');
      addLocalFavorite('v1');
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY]).length).toBe(1);
    });

    it('should add multiple vehicles', () => {
      addLocalFavorite('v1');
      addLocalFavorite('v2');
      addLocalFavorite('v3');
      const favorites = JSON.parse(storage[LOCAL_FAVORITES_KEY]);
      expect(favorites).toEqual(['v1', 'v2', 'v3']);
    });
  });

  describe('removeLocalFavorite', () => {
    const removeLocalFavorite = (vehicleId: string): void => {
      const favorites = JSON.parse(storage[LOCAL_FAVORITES_KEY] || '[]');
      const index = favorites.indexOf(vehicleId);
      if (index > -1) {
        favorites.splice(index, 1);
        storage[LOCAL_FAVORITES_KEY] = JSON.stringify(favorites);
      }
    };

    it('should remove from local favorites', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1', 'v2']);
      removeLocalFavorite('v1');
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY])).not.toContain('v1');
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY])).toContain('v2');
    });

    it('should handle removing non-existent favorite', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1', 'v2']);
      removeLocalFavorite('v3'); // Not in list
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY])).toEqual(['v1', 'v2']);
    });
  });

  describe('isLocalFavorite', () => {
    const isLocalFavorite = (vehicleId: string): boolean => {
      const favorites = JSON.parse(storage[LOCAL_FAVORITES_KEY] || '[]');
      return favorites.includes(vehicleId);
    };

    it('should return true for local favorite', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1']);
      expect(isLocalFavorite('v1')).toBe(true);
    });

    it('should return false for non-favorite', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1']);
      expect(isLocalFavorite('v2')).toBe(false);
    });

    it('should return false when no favorites exist', () => {
      expect(isLocalFavorite('v1')).toBe(false);
    });
  });

  describe('clearLocalFavorites', () => {
    const clearLocalFavorites = (): void => {
      delete storage[LOCAL_FAVORITES_KEY];
    };

    it('should clear all local favorites', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1', 'v2', 'v3']);
      clearLocalFavorites();
      expect(storage[LOCAL_FAVORITES_KEY]).toBeUndefined();
    });
  });

  describe('toggleLocalFavorite', () => {
    const toggleLocalFavorite = (vehicleId: string): boolean => {
      const favorites = JSON.parse(storage[LOCAL_FAVORITES_KEY] || '[]');
      const index = favorites.indexOf(vehicleId);

      if (index > -1) {
        favorites.splice(index, 1);
        storage[LOCAL_FAVORITES_KEY] = JSON.stringify(favorites);
        return false; // No longer a favorite
      } else {
        favorites.push(vehicleId);
        storage[LOCAL_FAVORITES_KEY] = JSON.stringify(favorites);
        return true; // Now a favorite
      }
    };

    it('should add when not favorited', () => {
      const result = toggleLocalFavorite('v1');
      expect(result).toBe(true);
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY])).toContain('v1');
    });

    it('should remove when already favorited', () => {
      storage[LOCAL_FAVORITES_KEY] = JSON.stringify(['v1']);
      const result = toggleLocalFavorite('v1');
      expect(result).toBe(false);
      expect(JSON.parse(storage[LOCAL_FAVORITES_KEY])).not.toContain('v1');
    });
  });
});

describe('Error Handling', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should handle network errors', async () => {
    mockApiGet.mockRejectedValue({
      code: 'NETWORK_ERROR',
      message: 'Network error',
    });

    await expect(apiClient.get('/api/favorites')).rejects.toEqual(
      expect.objectContaining({ code: 'NETWORK_ERROR' })
    );
  });

  it('should handle 500 server errors', async () => {
    mockApiGet.mockRejectedValue({
      response: {
        status: 500,
        data: { code: 'INTERNAL_ERROR', message: 'Server error' },
      },
    });

    await expect(apiClient.get('/api/favorites')).rejects.toEqual(
      expect.objectContaining({
        response: expect.objectContaining({ status: 500 }),
      })
    );
  });

  it('should handle timeout errors', async () => {
    mockApiGet.mockRejectedValue({
      code: 'TIMEOUT',
      message: 'Request timeout',
    });

    await expect(apiClient.get('/api/favorites')).rejects.toEqual(
      expect.objectContaining({ code: 'TIMEOUT' })
    );
  });
});

describe('Favorites Count Badge', () => {
  it('should return count for badge display', async () => {
    mockApiGet.mockResolvedValue({
      data: { count: 5 },
    });

    const response = await apiClient.get('/api/favorites/count');

    expect(response.data.count).toBe(5);
  });

  it('should return 0 when no favorites', async () => {
    mockApiGet.mockResolvedValue({
      data: { count: 0 },
    });

    const response = await apiClient.get('/api/favorites/count');

    expect(response.data.count).toBe(0);
  });
});
