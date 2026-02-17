/**
 * MSW Handlers - API Mocking for Tests
 *
 * This file contains all the mock handlers for API endpoints
 * used in testing the CarDealer frontend application.
 */

import { http, HttpResponse, delay } from 'msw';

const API_BASE = 'http://localhost:15095';

// Sample data
const mockVehicles = [
  {
    id: '1',
    title: '2023 Toyota Camry LE',
    make: 'Toyota',
    model: 'Camry',
    year: 2023,
    price: 25000,
    mileage: 15000,
    fuelType: 'Gasoline',
    transmission: 'Automatic',
    images: ['/images/camry.jpg'],
    location: 'Santo Domingo',
    sellerId: 'seller-1',
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: '2',
    title: '2022 Honda Civic Sport',
    make: 'Honda',
    model: 'Civic',
    year: 2022,
    price: 22000,
    mileage: 20000,
    fuelType: 'Gasoline',
    transmission: 'Manual',
    images: ['/images/civic.jpg'],
    location: 'Santiago',
    sellerId: 'seller-2',
    createdAt: '2024-01-10T10:00:00Z',
  },
];

const mockUser = {
  id: 'user-1',
  email: 'test@example.com',
  name: 'Test User',
  role: 'user',
  dealerId: null,
  createdAt: '2024-01-01T10:00:00Z',
};

const mockDealer = {
  id: 'dealer-1',
  name: 'Premium Motors',
  email: 'contact@premiummotors.do',
  phone: '+1 809 555 1234',
  location: 'Santo Domingo',
  rating: 4.5,
  totalSales: 150,
};

/**
 * Authentication Handlers
 */
export const authHandlers = [
  // Login
  http.post(`${API_BASE}/api/auth/login`, async ({ request }) => {
    await delay(100);
    const body = (await request.json()) as { email: string; password: string };

    if (body.email === 'test@example.com' && body.password === 'password123') {
      return HttpResponse.json({
        success: true,
        data: {
          user: mockUser,
          accessToken: 'mock-access-token',
          refreshToken: 'mock-refresh-token',
        },
      });
    }

    return HttpResponse.json({ success: false, message: 'Invalid credentials' }, { status: 401 });
  }),

  // Register
  http.post(`${API_BASE}/api/auth/register`, async ({ request }) => {
    await delay(150);
    const body = (await request.json()) as { email: string; password: string; name: string };

    if (body.email === 'existing@example.com') {
      return HttpResponse.json(
        { success: false, message: 'Email already exists' },
        { status: 409 }
      );
    }

    return HttpResponse.json({
      success: true,
      data: {
        user: { ...mockUser, email: body.email, name: body.name },
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
      },
    });
  }),

  // Refresh token
  http.post(`${API_BASE}/api/auth/refresh-token`, async () => {
    await delay(50);
    return HttpResponse.json({
      success: true,
      data: {
        accessToken: 'new-mock-access-token',
        refreshToken: 'new-mock-refresh-token',
      },
    });
  }),

  // Get current user
  http.get(`${API_BASE}/api/auth/me`, async ({ request }) => {
    const authHeader = request.headers.get('Authorization');

    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json({ success: false, message: 'Unauthorized' }, { status: 401 });
    }

    return HttpResponse.json({
      success: true,
      data: mockUser,
    });
  }),

  // Logout
  http.post(`${API_BASE}/api/auth/logout`, async () => {
    return HttpResponse.json({ success: true });
  }),
];

/**
 * Vehicle Handlers
 */
export const vehicleHandlers = [
  // Get all vehicles
  http.get(`${API_BASE}/api/vehicles`, async ({ request }) => {
    await delay(100);
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const limit = parseInt(url.searchParams.get('limit') || '10');
    const search = url.searchParams.get('search') || '';

    let filteredVehicles = mockVehicles;

    if (search) {
      filteredVehicles = mockVehicles.filter(
        (v) =>
          v.title.toLowerCase().includes(search.toLowerCase()) ||
          v.make.toLowerCase().includes(search.toLowerCase())
      );
    }

    return HttpResponse.json({
      success: true,
      data: {
        items: filteredVehicles.slice((page - 1) * limit, page * limit),
        total: filteredVehicles.length,
        page,
        limit,
        totalPages: Math.ceil(filteredVehicles.length / limit),
      },
    });
  }),

  // Get vehicle by ID
  http.get(`${API_BASE}/api/vehicles/:id`, async ({ params }) => {
    await delay(80);
    const vehicle = mockVehicles.find((v) => v.id === params.id);

    if (!vehicle) {
      return HttpResponse.json({ success: false, message: 'Vehicle not found' }, { status: 404 });
    }

    return HttpResponse.json({
      success: true,
      data: vehicle,
    });
  }),

  // Create vehicle
  http.post(`${API_BASE}/api/vehicles`, async ({ request }) => {
    await delay(200);
    const body = (await request.json()) as Partial<(typeof mockVehicles)[0]>;

    const newVehicle = {
      id: `vehicle-${Date.now()}`,
      ...body,
      createdAt: new Date().toISOString(),
    };

    return HttpResponse.json(
      {
        success: true,
        data: newVehicle,
      },
      { status: 201 }
    );
  }),

  // Update vehicle
  http.put(`${API_BASE}/api/vehicles/:id`, async ({ params, request }) => {
    await delay(150);
    const body = (await request.json()) as Partial<(typeof mockVehicles)[0]>;
    const vehicle = mockVehicles.find((v) => v.id === params.id);

    if (!vehicle) {
      return HttpResponse.json({ success: false, message: 'Vehicle not found' }, { status: 404 });
    }

    return HttpResponse.json({
      success: true,
      data: { ...vehicle, ...body },
    });
  }),

  // Delete vehicle
  http.delete(`${API_BASE}/api/vehicles/:id`, async ({ params }) => {
    await delay(100);
    const vehicle = mockVehicles.find((v) => v.id === params.id);

    if (!vehicle) {
      return HttpResponse.json({ success: false, message: 'Vehicle not found' }, { status: 404 });
    }

    return HttpResponse.json({ success: true }, { status: 204 });
  }),
];

/**
 * Dealer Handlers
 */
export const dealerHandlers = [
  // Get dealer by ID
  http.get(`${API_BASE}/api/dealers/:id`, async ({ params }) => {
    await delay(80);

    if (params.id !== 'dealer-1') {
      return HttpResponse.json({ success: false, message: 'Dealer not found' }, { status: 404 });
    }

    return HttpResponse.json({
      success: true,
      data: mockDealer,
    });
  }),

  // Get dealer vehicles
  http.get(`${API_BASE}/api/dealers/:id/vehicles`, async () => {
    await delay(100);
    return HttpResponse.json({
      success: true,
      data: mockVehicles,
    });
  }),
];

/**
 * Test-environment handlers (wildcard localhost ports)
 * Prevents real network calls from components that use fetch/axios directly.
 */
export const testEnvironmentHandlers = [
  http.get('*/api/maintenance/status', async () => {
    return HttpResponse.json({
      isMaintenanceMode: false,
      maintenanceWindow: null,
    });
  }),

  http.options('*/api/vehicles', async () => {
    return HttpResponse.json({}, { status: 204 });
  }),

  http.post('*/api/vehicles', async ({ request }) => {
    await delay(120);
    const body = (await request.json()) as Record<string, unknown>;

    return HttpResponse.json(
      {
        id: `vehicle-test-${Date.now()}`,
        ...body,
        images: [],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
      { status: 201 }
    );
  }),
];

/**
 * Vehicle Intelligence Handlers
 */
export const vehicleIntelligenceHandlers = [
  http.options('*/api/vehicleintelligence/price-suggestion', async () => {
    return HttpResponse.json({}, { status: 204 });
  }),
  http.options('*/api/vehicleintelligence/demand/categories', async () => {
    return HttpResponse.json({}, { status: 204 });
  }),

  http.post('*/api/vehicleintelligence/price-suggestion', async ({ request }) => {
    await delay(120);
    const authHeader = request.headers.get('Authorization');
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json({ message: 'Unauthorized' }, { status: 401 });
    }

    // Keep the mock deterministic and simple for UI tests
    return HttpResponse.json({
      marketPrice: 24500,
      suggestedPrice: 23900,
      deltaPercent: 3,
      demandScore: 72,
      estimatedDaysToSell: 18,
      confidence: 0.74,
      modelVersion: 'baseline-v1',
      sellingTips: [
        'Agrega al menos 15 fotos claras',
        'Incluye mantenimiento reciente en la descripción',
        'Considera ajustar el precio si no recibes contactos en 7 días',
      ],
    });
  }),

  http.get('*/api/vehicleintelligence/demand/categories', async ({ request }) => {
    await delay(80);
    const authHeader = request.headers.get('Authorization');
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json({ message: 'Unauthorized' }, { status: 401 });
    }

    return HttpResponse.json([
      { category: 'SUV', demandScore: 86, trend: 'Up', updatedAt: '2026-01-09T10:00:00Z' },
      { category: 'Sedan', demandScore: 62, trend: 'Stable', updatedAt: '2026-01-09T10:00:00Z' },
      { category: 'Camioneta', demandScore: 78, trend: 'Up', updatedAt: '2026-01-09T10:00:00Z' },
      { category: 'Eléctrico', demandScore: 55, trend: 'Down', updatedAt: '2026-01-09T10:00:00Z' },
    ]);
  }),
];

/**
 * All handlers combined
 */
export const handlers = [
  ...authHandlers,
  ...vehicleHandlers,
  ...dealerHandlers,
  ...testEnvironmentHandlers,
  ...vehicleIntelligenceHandlers,
];

export default handlers;
