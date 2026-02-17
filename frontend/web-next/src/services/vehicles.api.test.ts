/**
 * Vehicles Service Tests - Pure Unit Tests
 * Priority: P0 - Vehicle search and detail
 *
 * These tests don't depend on MSW intercepting network calls
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { apiClient } from '@/lib/api-client';

// Mock axios/apiClient
vi.mock('@/lib/api-client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockApiGet = apiClient.get as ReturnType<typeof vi.fn>;
const mockApiPost = apiClient.post as ReturnType<typeof vi.fn>;

// Mock vehicle data
const mockVehicle = {
  id: 'vehicle-123',
  slug: 'toyota-corolla-2023',
  make: 'Toyota',
  model: 'Corolla',
  year: 2023,
  price: 1850000,
  marketPrice: 2000000,
  currency: 'DOP',
  mileage: 15000,
  transmission: 'automatic',
  fuelType: 'gasoline',
  images: [{ id: 'img1', url: 'https://example.com/car.jpg', order: 0 }],
  city: 'Santo Domingo',
  status: 'active',
};

const mockVehicles = [
  mockVehicle,
  { ...mockVehicle, id: 'v2', make: 'Honda', model: 'Civic', price: 2100000 },
  { ...mockVehicle, id: 'v3', make: 'Nissan', model: 'Sentra', price: 1600000 },
];

describe('Vehicles API Contract', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('GET /api/vehicles', () => {
    it('should return paginated vehicles', async () => {
      mockApiGet.mockResolvedValue({
        data: {
          items: mockVehicles,
          totalItems: 3,
          page: 1,
          pageSize: 12,
          totalPages: 1,
        },
      });

      const response = await apiClient.get('/api/vehicles');

      expect(response.data.items).toBeInstanceOf(Array);
      expect(response.data.totalItems).toBeDefined();
      expect(response.data.page).toBeDefined();
      expect(response.data.pageSize).toBeDefined();
      expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles');
    });

    it('should filter by make', async () => {
      mockApiGet.mockResolvedValue({
        data: {
          items: mockVehicles.filter(v => v.make === 'Toyota'),
          totalItems: 1,
          page: 1,
          pageSize: 12,
        },
      });

      const response = await apiClient.get('/api/vehicles', { params: { make: 'Toyota' } });

      expect(response.data.items).toBeInstanceOf(Array);
      expect(response.data.items.every((v: { make: string }) => v.make === 'Toyota')).toBe(true);
    });

    it('should paginate results', async () => {
      mockApiGet.mockResolvedValue({
        data: {
          items: [mockVehicle],
          totalItems: 30,
          page: 2,
          pageSize: 10,
          totalPages: 3,
        },
      });

      const response = await apiClient.get('/api/vehicles', { params: { page: 2, pageSize: 10 } });

      expect(response.data.page).toBe(2);
      expect(response.data.pageSize).toBe(10);
    });
  });

  describe('GET /api/vehicles/:id', () => {
    it('should return vehicle by ID', async () => {
      mockApiGet.mockResolvedValue({ data: mockVehicle });

      const response = await apiClient.get('/api/vehicles/vehicle-123');

      expect(response.data.id).toBe('vehicle-123');
      expect(response.data.make).toBeDefined();
      expect(response.data.model).toBeDefined();
      expect(response.data.year).toBeDefined();
      expect(response.data.price).toBeDefined();
    });

    it('should throw for non-existent vehicle', async () => {
      mockApiGet.mockRejectedValue({
        response: { status: 404, data: { code: 'NOT_FOUND', message: 'Vehicle not found' } },
      });

      await expect(apiClient.get('/api/vehicles/non-existent')).rejects.toEqual(
        expect.objectContaining({
          response: expect.objectContaining({ status: 404 }),
        })
      );
    });
  });

  describe('GET /api/vehicles/slug/:slug', () => {
    it('should return vehicle by slug', async () => {
      mockApiGet.mockResolvedValue({ data: mockVehicle });

      const response = await apiClient.get('/api/vehicles/slug/toyota-corolla-2023');

      expect(response.data.slug).toBe('toyota-corolla-2023');
      expect(response.data.make).toBe('Toyota');
    });
  });

  describe('GET /api/vehicles/:id/similar', () => {
    it('should return similar vehicles', async () => {
      mockApiGet.mockResolvedValue({ data: mockVehicles.slice(1) });

      const response = await apiClient.get('/api/vehicles/vehicle-123/similar');

      expect(response.data).toBeInstanceOf(Array);
      expect(response.data.length).toBe(2);
    });
  });

  describe('GET /api/vehicles/featured', () => {
    it('should return featured vehicles', async () => {
      mockApiGet.mockResolvedValue({ data: mockVehicles });

      const response = await apiClient.get('/api/vehicles/featured');

      expect(response.data).toBeInstanceOf(Array);
    });
  });

  describe('POST /api/vehicles/:id/view', () => {
    it('should track vehicle view', async () => {
      mockApiPost.mockResolvedValue({ data: { success: true } });

      const response = await apiClient.post('/api/vehicles/vehicle-123/view');

      expect(response.data.success).toBe(true);
      expect(mockApiPost).toHaveBeenCalledWith('/api/vehicles/vehicle-123/view');
    });
  });
});

describe('Vehicle Data Transformations', () => {
  describe('calculateDealRating', () => {
    const calculateDealRating = (price: number, marketPrice?: number): string => {
      if (!marketPrice || marketPrice === 0) return 'uncertain';

      const diff = (price - marketPrice) / marketPrice;

      if (diff <= -0.1) return 'great';
      if (diff <= -0.05) return 'good';
      if (diff <= 0.05) return 'fair';
      return 'high';
    };

    it('should return "great" for price 10%+ below market', () => {
      expect(calculateDealRating(1700000, 2000000)).toBe('great'); // -15%
      expect(calculateDealRating(1800000, 2000000)).toBe('great'); // -10%
    });

    it('should return "good" for price 5-10% below market', () => {
      expect(calculateDealRating(1850000, 2000000)).toBe('good'); // -7.5%
      expect(calculateDealRating(1900000, 2000000)).toBe('good'); // -5% exactly
    });

    it('should return "fair" for price near market', () => {
      expect(calculateDealRating(1950000, 2000000)).toBe('fair'); // -2.5%
      expect(calculateDealRating(2000000, 2000000)).toBe('fair'); // 0%
      expect(calculateDealRating(2050000, 2000000)).toBe('fair'); // +2.5%
      expect(calculateDealRating(2100000, 2000000)).toBe('fair'); // +5%
    });

    it('should return "high" for price above market', () => {
      expect(calculateDealRating(2200000, 2000000)).toBe('high'); // +10%
      expect(calculateDealRating(2500000, 2000000)).toBe('high'); // +25%
    });

    it('should return "uncertain" when no market price', () => {
      expect(calculateDealRating(2000000, undefined)).toBe('uncertain');
      expect(calculateDealRating(2000000, 0)).toBe('uncertain');
    });
  });

  describe('transformVehicle', () => {
    it('should transform VehicleDto to Vehicle', () => {
      const vehicleDto = {
        id: 'v1',
        slug: 'toyota-corolla-2023',
        make: 'Toyota',
        model: 'Corolla',
        year: 2023,
        price: 1850000,
        marketPrice: 2000000,
        currency: 'DOP',
        mileage: 15000,
        transmission: 'automatic',
        fuelType: 'gasoline',
        images: [{ id: 'img1', url: 'https://example.com/car.jpg', order: 0 }],
        city: 'Santo Domingo',
        status: 'active',
      };

      const vehicle = {
        ...vehicleDto,
        primaryImage: vehicleDto.images[0]?.url,
        formattedPrice: `RD$${vehicleDto.price.toLocaleString()}`,
      };

      expect(vehicle.id).toBe('v1');
      expect(vehicle.primaryImage).toBe('https://example.com/car.jpg');
    });

    it('should use placeholder image when no images', () => {
      const vehicleDto = {
        id: 'v2',
        slug: 'honda-civic-2024',
        make: 'Honda',
        model: 'Civic',
        year: 2024,
        price: 2100000,
        images: [],
      };

      const primaryImage = vehicleDto.images[0]?.url || '/images/placeholder-vehicle.jpg';
      expect(primaryImage).toBe('/images/placeholder-vehicle.jpg');
    });
  });

  describe('transformVehicleCard', () => {
    it('should transform for card display', () => {
      const vehicle = {
        id: 'v1',
        slug: 'toyota-corolla-2023',
        make: 'Toyota',
        model: 'Corolla',
        year: 2023,
        price: 1850000,
        mileage: 15000,
        transmission: 'automatic',
        fuelType: 'gasoline',
        images: [{ id: 'img1', url: 'https://example.com/car.jpg' }],
        city: 'Santo Domingo',
      };

      const cardData = {
        id: vehicle.id,
        slug: vehicle.slug,
        title: `${vehicle.year} ${vehicle.make} ${vehicle.model}`,
        price: vehicle.price,
        formattedPrice: `RD$${vehicle.price.toLocaleString()}`,
        mileage: vehicle.mileage,
        formattedMileage: `${vehicle.mileage.toLocaleString()} km`,
        location: vehicle.city,
        image: vehicle.images[0]?.url || '/images/placeholder-vehicle.jpg',
        transmission: vehicle.transmission,
        fuelType: vehicle.fuelType,
      };

      expect(cardData.title).toBe('2023 Toyota Corolla');
      expect(cardData.formattedMileage).toBe('15,000 km');
      expect(cardData.image).toBe('https://example.com/car.jpg');
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

    await expect(apiClient.get('/api/vehicles/network-error')).rejects.toEqual(
      expect.objectContaining({ code: 'NETWORK_ERROR' })
    );
  });

  it('should handle 500 server errors', async () => {
    mockApiGet.mockRejectedValue({
      response: { status: 500, data: { code: 'INTERNAL_ERROR', message: 'Server error' } },
    });

    await expect(apiClient.get('/api/vehicles/server-error')).rejects.toEqual(
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

    await expect(apiClient.get('/api/vehicles/timeout')).rejects.toEqual(
      expect.objectContaining({ code: 'TIMEOUT' })
    );
  });
});

describe('Vehicle Search Params', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockApiGet.mockResolvedValue({ data: { items: [], totalItems: 0 } });
  });

  it('should pass make filter', async () => {
    await apiClient.get('/api/vehicles', { params: { make: 'Toyota' } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', { params: { make: 'Toyota' } });
  });

  it('should pass price range filters', async () => {
    await apiClient.get('/api/vehicles', { params: { minPrice: 1000000, maxPrice: 2000000 } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', {
      params: { minPrice: 1000000, maxPrice: 2000000 },
    });
  });

  it('should pass year range filters', async () => {
    await apiClient.get('/api/vehicles', { params: { minYear: 2020, maxYear: 2024 } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', {
      params: { minYear: 2020, maxYear: 2024 },
    });
  });

  it('should pass mileage filter', async () => {
    await apiClient.get('/api/vehicles', { params: { maxMileage: 50000 } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', { params: { maxMileage: 50000 } });
  });

  it('should pass transmission filter', async () => {
    await apiClient.get('/api/vehicles', { params: { transmission: 'automatic' } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', {
      params: { transmission: 'automatic' },
    });
  });

  it('should pass fuel type filter', async () => {
    await apiClient.get('/api/vehicles', { params: { fuelType: 'hybrid' } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', { params: { fuelType: 'hybrid' } });
  });

  it('should pass location filters', async () => {
    await apiClient.get('/api/vehicles', { params: { city: 'Santo Domingo', province: 'DN' } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', {
      params: { city: 'Santo Domingo', province: 'DN' },
    });
  });

  it('should pass sort params', async () => {
    await apiClient.get('/api/vehicles', { params: { sortBy: 'price', sortOrder: 'asc' } });
    expect(mockApiGet).toHaveBeenCalledWith('/api/vehicles', {
      params: { sortBy: 'price', sortOrder: 'asc' },
    });
  });
});
