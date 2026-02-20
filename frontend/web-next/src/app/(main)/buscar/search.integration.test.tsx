/**
 * Search Integration Tests
 * Priority: P0 - Vehicle search functionality
 *
 * Tests mock the service layer directly for reliable testing
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as React from 'react';

// Mock the vehicles service
vi.mock('@/services/vehicles', () => ({
  vehicleService: {
    search: vi.fn(),
    getById: vi.fn(),
    getBySlug: vi.fn(),
    getSimilar: vi.fn(),
    getFeatured: vi.fn(),
    trackView: vi.fn(),
  },
  getVehiclesByDealer: vi.fn(),
  getVehiclesByIds: vi.fn(),
  createVehicle: vi.fn(),
  updateVehicle: vi.fn(),
  deleteVehicle: vi.fn(),
  getMakes: vi.fn(),
  getModelsByMake: vi.fn(),
  getBodyTypes: vi.fn(),
  getFuelTypes: vi.fn(),
  getTransmissions: vi.fn(),
  getColors: vi.fn(),
  getProvinces: vi.fn(),
  getFeatures: vi.fn(),
}));

// Import after mocking
import { useVehicleList } from '@/hooks/use-vehicles';
import { vehicleService } from '@/services/vehicles';

const mockSearch = vehicleService.search as ReturnType<typeof vi.fn>;

// Mock vehicle data
const mockVehicles = [
  {
    id: 'v1',
    slug: 'toyota-corolla-2023',
    make: 'Toyota',
    model: 'Corolla',
    year: 2023,
    price: 1850000,
    mileage: 15000,
    transmission: 'automatic',
    fuelType: 'gasoline',
    city: 'Santo Domingo',
    images: [{ url: 'https://example.com/car1.jpg' }],
  },
  {
    id: 'v2',
    slug: 'honda-civic-2024',
    make: 'Honda',
    model: 'Civic',
    year: 2024,
    price: 2100000,
    mileage: 5000,
    transmission: 'automatic',
    fuelType: 'gasoline',
    city: 'Santiago',
    images: [{ url: 'https://example.com/car2.jpg' }],
  },
  {
    id: 'v3',
    slug: 'nissan-sentra-2023',
    make: 'Nissan',
    model: 'Sentra',
    year: 2023,
    price: 1600000,
    mileage: 20000,
    transmission: 'manual',
    fuelType: 'gasoline',
    city: 'Santo Domingo',
    images: [],
  },
];

const mockSearchResponse = {
  items: mockVehicles,
  page: 1,
  pageSize: 12,
  totalItems: 3,
  totalPages: 1,
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0, staleTime: 0 },
    },
  });

  return function Wrapper({ children }: { children: React.ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}

describe('Vehicle Search Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockSearch.mockResolvedValue(mockSearchResponse);
  });

  describe('useVehicleList', () => {
    it('should search vehicles with default params', async () => {
      const { result } = renderHook(() => useVehicleList({}), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.data).toBeDefined();
      expect(result.current.data?.items).toBeInstanceOf(Array);
      expect(result.current.data?.items).toHaveLength(3);
      expect(mockSearch).toHaveBeenCalledWith({});
    });

    it('should search with make filter', async () => {
      mockSearch.mockResolvedValue({
        items: mockVehicles.filter(v => v.make === 'Toyota'),
        page: 1,
        pageSize: 12,
        totalItems: 1,
        totalPages: 1,
      });

      const { result } = renderHook(() => useVehicleList({ make: 'Toyota' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.data?.items).toHaveLength(1);
      expect(result.current.data?.items[0].make).toBe('Toyota');
      expect(mockSearch).toHaveBeenCalledWith({ make: 'Toyota' });
    });

    it('should search with multiple filters', async () => {
      const filters = { make: 'Toyota', minYear: 2022, maxPrice: 2000000 };

      mockSearch.mockResolvedValue({
        items: [mockVehicles[0]],
        page: 1,
        pageSize: 12,
        totalItems: 1,
        totalPages: 1,
      });

      const { result } = renderHook(() => useVehicleList(filters), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.data?.items).toHaveLength(1);
      expect(mockSearch).toHaveBeenCalledWith(filters);
    });

    it('should handle empty search results', async () => {
      mockSearch.mockResolvedValue({
        items: [],
        page: 1,
        pageSize: 12,
        totalItems: 0,
        totalPages: 0,
      });

      const { result } = renderHook(() => useVehicleList({ make: 'NonExistent' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.data?.items).toHaveLength(0);
      expect(result.current.data?.totalItems).toBe(0);
    });

    it('should handle search error', async () => {
      mockSearch.mockRejectedValue(new Error('Search failed'));

      const { result } = renderHook(() => useVehicleList({}), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.error).toBeTruthy();
    });
  });
});

describe('Search Pagination', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should paginate search results', async () => {
    mockSearch.mockResolvedValue({
      items: [mockVehicles[0]],
      page: 2,
      pageSize: 1,
      totalItems: 3,
      totalPages: 3,
    });

    const { result } = renderHook(() => useVehicleList({ page: 2, pageSize: 1 }), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data?.page).toBe(2);
    expect(result.current.data?.totalPages).toBe(3);
    expect(mockSearch).toHaveBeenCalledWith({ page: 2, pageSize: 1 });
  });

  it('should change page size', async () => {
    mockSearch.mockResolvedValue({
      items: mockVehicles.slice(0, 2),
      page: 1,
      pageSize: 2,
      totalItems: 3,
      totalPages: 2,
    });

    const { result } = renderHook(() => useVehicleList({ pageSize: 2 }), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.data?.pageSize).toBe(2);
    expect(result.current.data?.items).toHaveLength(2);
  });
});

describe('Search Sorting', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should sort by price ascending', async () => {
    const sortedVehicles = [...mockVehicles].sort((a, b) => a.price - b.price);
    mockSearch.mockResolvedValue({
      items: sortedVehicles,
      page: 1,
      pageSize: 12,
      totalItems: 3,
      totalPages: 1,
    });

    const { result } = renderHook(() => useVehicleList({ sortBy: 'price', sortOrder: 'asc' }), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    const items = result.current.data?.items;
    expect(items?.[0].price).toBe(1600000);
    expect(items?.[2].price).toBe(2100000);
  });

  it('should sort by year descending', async () => {
    const sortedVehicles = [...mockVehicles].sort((a, b) => b.year - a.year);
    mockSearch.mockResolvedValue({
      items: sortedVehicles,
      page: 1,
      pageSize: 12,
      totalItems: 3,
      totalPages: 1,
    });

    const { result } = renderHook(() => useVehicleList({ sortBy: 'year', sortOrder: 'desc' }), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    const items = result.current.data?.items;
    expect(items?.[0].year).toBe(2024);
  });
});

describe('Search Filters', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockSearch.mockResolvedValue(mockSearchResponse);
  });

  describe('Price Range Filter', () => {
    it('should filter by min price', async () => {
      const { result } = renderHook(() => useVehicleList({ minPrice: 1000000 }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ minPrice: 1000000 });
    });

    it('should filter by max price', async () => {
      const { result } = renderHook(() => useVehicleList({ maxPrice: 2000000 }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ maxPrice: 2000000 });
    });

    it('should filter by price range', async () => {
      const { result } = renderHook(
        () => useVehicleList({ minPrice: 1500000, maxPrice: 2000000 }),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ minPrice: 1500000, maxPrice: 2000000 });
    });
  });

  describe('Year Range Filter', () => {
    it('should filter by year range', async () => {
      const { result } = renderHook(() => useVehicleList({ minYear: 2022, maxYear: 2024 }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ minYear: 2022, maxYear: 2024 });
    });
  });

  describe('Mileage Filter', () => {
    it('should filter by max mileage', async () => {
      const { result } = renderHook(() => useVehicleList({ maxMileage: 50000 }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ maxMileage: 50000 });
    });
  });

  describe('Transmission Filter', () => {
    it('should filter by transmission type', async () => {
      const { result } = renderHook(() => useVehicleList({ transmission: 'automatic' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ transmission: 'automatic' });
    });
  });

  describe('Fuel Type Filter', () => {
    it('should filter by fuel type', async () => {
      const { result } = renderHook(() => useVehicleList({ fuelType: 'hybrid' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ fuelType: 'hybrid' });
    });
  });

  describe('Location Filter', () => {
    it('should filter by city', async () => {
      const { result } = renderHook(() => useVehicleList({ city: 'Santo Domingo' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ city: 'Santo Domingo' });
    });

    it('should filter by province', async () => {
      const { result } = renderHook(() => useVehicleList({ province: 'Santiago' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ province: 'Santiago' });
    });
  });

  describe('Condition Filter', () => {
    it('should filter by new condition', async () => {
      const { result } = renderHook(() => useVehicleList({ condition: 'new' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ condition: 'new' });
    });

    it('should filter by used condition', async () => {
      const { result } = renderHook(() => useVehicleList({ condition: 'used' }), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(mockSearch).toHaveBeenCalledWith({ condition: 'used' });
    });
  });
});

describe('Search Query Caching', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockSearch.mockResolvedValue(mockSearchResponse);
  });

  it('should cache search results by query key', async () => {
    const wrapper = createWrapper();

    // First search
    const { result: result1 } = renderHook(() => useVehicleList({ make: 'Toyota' }), { wrapper });

    await waitFor(() => {
      expect(result1.current.isLoading).toBe(false);
    });

    expect(mockSearch).toHaveBeenCalledTimes(1);

    // Same search should use cache
    const { result: result2 } = renderHook(() => useVehicleList({ make: 'Toyota' }), { wrapper });

    expect(result2.current.data?.items).toHaveLength(3);
    expect(mockSearch).toHaveBeenCalledTimes(1); // No additional call
  });

  it('should make new call for different query', async () => {
    const wrapper = createWrapper();

    // First search
    const { result: result1 } = renderHook(() => useVehicleList({ make: 'Toyota' }), { wrapper });

    await waitFor(() => {
      expect(result1.current.isLoading).toBe(false);
    });

    // Different search should make new call
    const { result: result2 } = renderHook(() => useVehicleList({ make: 'Honda' }), { wrapper });

    await waitFor(() => {
      expect(result2.current.isLoading).toBe(false);
    });

    expect(mockSearch).toHaveBeenCalledTimes(2);
  });
});

describe('Text Search', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should search by keyword', async () => {
    mockSearch.mockResolvedValue({
      items: mockVehicles.filter(v => v.make === 'Toyota'),
      page: 1,
      pageSize: 12,
      totalItems: 1,
      totalPages: 1,
    });

    const { result } = renderHook(() => useVehicleList({ q: 'Toyota' }), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(mockSearch).toHaveBeenCalledWith({ q: 'Toyota' });
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });
});
