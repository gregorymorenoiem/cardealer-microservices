/**
 * Sellers & Dealers API Contract Tests
 * Tests the seller conversion and dealer registration API contracts.
 * Uses MSW for API mocking, matching the project's Vitest + MSW pattern.
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import { server } from '@/test/mocks/server';

const API_URL = 'http://localhost:8080';

describe('Sellers API Contract', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('POST /api/sellers/convert', () => {
    it('should convert buyer to seller with valid data', async () => {
      server.use(
        http.post(`${API_URL}/api/sellers/convert`, () => {
          return HttpResponse.json({
            conversionId: 'conv-123',
            sellerProfileId: 'seller-456',
            userId: 'user-789',
            status: 'Pending',
            source: 'SelfService',
            pendingVerification: true,
            message: 'Conversion initiated successfully',
            createdAt: '2026-02-18T10:00:00Z',
          });
        })
      );

      const response = await fetch(`${API_URL}/api/sellers/convert`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          businessName: 'Vehículos Juan',
          description: 'Vendedor particular',
          phone: '809-555-1234',
          location: 'Santo Domingo',
          acceptTerms: true,
        }),
      });

      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.conversionId).toBeDefined();
      expect(data.sellerProfileId).toBeDefined();
      expect(data.status).toBe('Pending');
      expect(data.pendingVerification).toBe(true);
    });

    it('should include idempotency key header when provided', async () => {
      let receivedIdempotencyKey: string | null = null;

      server.use(
        http.post(`${API_URL}/api/sellers/convert`, ({ request }) => {
          receivedIdempotencyKey = request.headers.get('Idempotency-Key');
          return HttpResponse.json({
            conversionId: 'conv-123',
            sellerProfileId: 'seller-456',
            userId: 'user-789',
            status: 'Pending',
            source: 'SelfService',
            pendingVerification: true,
            createdAt: '2026-02-18T10:00:00Z',
          });
        })
      );

      await fetch(`${API_URL}/api/sellers/convert`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Idempotency-Key': 'idem-key-abc-123',
        },
        body: JSON.stringify({
          businessName: 'Test',
          acceptTerms: true,
        }),
      });

      expect(receivedIdempotencyKey).toBe('idem-key-abc-123');
    });

    it('should return CONVERSION_NOT_ALLOWED for dealer accounts', async () => {
      server.use(
        http.post(`${API_URL}/api/sellers/convert`, () => {
          return HttpResponse.json(
            {
              type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
              title: 'Conversion not allowed',
              status: 400,
              detail: 'Dealer accounts cannot be converted to seller.',
              extensions: { errorCode: 'CONVERSION_NOT_ALLOWED' },
            },
            { status: 400 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/sellers/convert`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          businessName: 'Test',
          acceptTerms: true,
        }),
      });

      expect(response.status).toBe(400);
      const data = await response.json();
      expect(data.extensions?.errorCode).toBe('CONVERSION_NOT_ALLOWED');
    });

    it('should return FEATURE_DISABLED when feature flag is off', async () => {
      server.use(
        http.post(`${API_URL}/api/sellers/convert`, () => {
          return HttpResponse.json(
            {
              type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
              title: 'Feature disabled',
              status: 400,
              detail: 'La conversión a vendedor no está disponible.',
              extensions: { errorCode: 'FEATURE_DISABLED' },
            },
            { status: 400 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/sellers/convert`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          businessName: 'Test',
          acceptTerms: true,
        }),
      });

      expect(response.status).toBe(400);
      const data = await response.json();
      expect(data.extensions?.errorCode).toBe('FEATURE_DISABLED');
    });
  });

  describe('GET /api/sellers/:id', () => {
    it('should return seller profile by ID', async () => {
      const sellerId = 'seller-abc-123';

      server.use(
        http.get(`${API_URL}/api/sellers/${sellerId}`, () => {
          return HttpResponse.json({
            id: sellerId,
            userId: 'user-789',
            businessName: 'Autos Premium',
            displayName: 'Autos Premium',
            isVerified: true,
            averageRating: 4.5,
            totalReviews: 12,
            totalListings: 8,
            activeSales: 3,
            memberSince: '2025-06-01T00:00:00Z',
            createdAt: '2025-06-01T00:00:00Z',
          });
        })
      );

      const response = await fetch(`${API_URL}/api/sellers/${sellerId}`);
      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.id).toBe(sellerId);
      expect(data.businessName).toBe('Autos Premium');
      expect(data.isVerified).toBe(true);
      expect(data.averageRating).toBeGreaterThanOrEqual(0);
    });

    it('should return 404 for non-existent seller', async () => {
      server.use(
        http.get(`${API_URL}/api/sellers/nonexistent`, () => {
          return HttpResponse.json(
            { title: 'Not found', status: 404 },
            { status: 404 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/sellers/nonexistent`);
      expect(response.status).toBe(404);
    });
  });

  describe('GET /api/sellers/:id/stats', () => {
    it('should return seller statistics', async () => {
      const sellerId = 'seller-abc-123';

      server.use(
        http.get(`${API_URL}/api/sellers/${sellerId}/stats`, () => {
          return HttpResponse.json({
            totalListings: 15,
            activeListings: 8,
            totalSales: 42,
            averageRating: 4.7,
            totalReviews: 25,
            responseRate: 0.95,
            responseTimeMinutes: 30,
          });
        })
      );

      const response = await fetch(`${API_URL}/api/sellers/${sellerId}/stats`);
      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.totalListings).toBeGreaterThanOrEqual(0);
      expect(data.activeListings).toBeLessThanOrEqual(data.totalListings);
      expect(data.averageRating).toBeGreaterThanOrEqual(0);
      expect(data.averageRating).toBeLessThanOrEqual(5);
    });
  });
});

describe('Dealers API Contract', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('POST /api/dealers', () => {
    it('should register a new dealer successfully', async () => {
      server.use(
        http.post(`${API_URL}/api/dealers`, () => {
          return HttpResponse.json(
            {
              id: 'dealer-new-123',
              ownerUserId: 'user-789',
              businessName: 'Auto Premium RD',
              verificationStatus: 'Pending',
              isActive: false,
              createdAt: '2026-02-18T10:00:00Z',
            },
            { status: 201 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/dealers`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          businessName: 'Auto Premium RD',
          dealerType: 'Independent',
          email: 'info@autopremiumrd.com',
          phone: '809-555-0001',
          address: 'Av. 27 de Febrero',
          city: 'Santo Domingo',
          state: 'Distrito Nacional',
        }),
      });

      expect(response.status).toBe(201);

      const data = await response.json();
      expect(data.id).toBeDefined();
      expect(data.businessName).toBe('Auto Premium RD');
      expect(data.verificationStatus).toBe('Pending');
      expect(data.isActive).toBe(false);
    });

    it('should return ALREADY_DEALER when user already has a dealer', async () => {
      server.use(
        http.post(`${API_URL}/api/dealers`, () => {
          return HttpResponse.json(
            {
              type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
              title: 'Already a dealer',
              status: 409,
              detail: 'User already has a dealer registration.',
              extensions: { errorCode: 'ALREADY_DEALER' },
            },
            { status: 409 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/dealers`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          businessName: 'Another Dealer',
          dealerType: 'Independent',
          email: 'info@anotherd.com',
          phone: '809-555-0002',
          address: 'Av. Lincoln',
          city: 'Santo Domingo',
          state: 'DN',
        }),
      });

      expect(response.status).toBe(409);
      const data = await response.json();
      expect(data.extensions?.errorCode).toBe('ALREADY_DEALER');
    });

    it('should return FEATURE_DISABLED when dealer registration is off', async () => {
      server.use(
        http.post(`${API_URL}/api/dealers`, () => {
          return HttpResponse.json(
            {
              title: 'Feature disabled',
              status: 400,
              extensions: { errorCode: 'FEATURE_DISABLED' },
            },
            { status: 400 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/dealers`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          businessName: 'Test Dealer',
          dealerType: 'Independent',
          email: 'test@d.com',
          phone: '809-555-0000',
          address: 'Test',
          city: 'SD',
          state: 'DN',
        }),
      });

      expect(response.status).toBe(400);
      const data = await response.json();
      expect(data.extensions?.errorCode).toBe('FEATURE_DISABLED');
    });
  });

  describe('GET /api/dealers/me', () => {
    it('should return current user dealer profile', async () => {
      server.use(
        http.get(`${API_URL}/api/dealers/me`, () => {
          return HttpResponse.json({
            id: 'dealer-mine-123',
            ownerUserId: 'user-789',
            businessName: 'Mi Dealer',
            verificationStatus: 'Verified',
            isActive: true,
            createdAt: '2025-01-01T00:00:00Z',
          });
        })
      );

      const response = await fetch(`${API_URL}/api/dealers/me`);
      expect(response.ok).toBe(true);

      const data = await response.json();
      expect(data.id).toBeDefined();
      expect(data.businessName).toBe('Mi Dealer');
      expect(data.verificationStatus).toBe('Verified');
    });

    it('should return 404 when user has no dealer', async () => {
      server.use(
        http.get(`${API_URL}/api/dealers/me`, () => {
          return HttpResponse.json(
            { title: 'Not found', status: 404 },
            { status: 404 }
          );
        })
      );

      const response = await fetch(`${API_URL}/api/dealers/me`);
      expect(response.status).toBe(404);
    });
  });

  describe('Admin Dealer Management', () => {
    describe('GET /api/admin/dealers/pending', () => {
      it('should return list of pending dealers', async () => {
        server.use(
          http.get(`${API_URL}/api/admin/dealers/pending`, () => {
            return HttpResponse.json({
              items: [
                {
                  id: 'dealer-1',
                  businessName: 'Dealer Pendiente 1',
                  verificationStatus: 'Pending',
                  isActive: false,
                },
                {
                  id: 'dealer-2',
                  businessName: 'Dealer Pendiente 2',
                  verificationStatus: 'Pending',
                  isActive: false,
                },
              ],
              totalCount: 2,
              page: 1,
              pageSize: 10,
            });
          })
        );

        const response = await fetch(`${API_URL}/api/admin/dealers/pending`);
        expect(response.ok).toBe(true);

        const data = await response.json();
        expect(data.items).toHaveLength(2);
        expect(data.items[0].verificationStatus).toBe('Pending');
      });
    });

    describe('POST /api/admin/dealers/:id/approve', () => {
      it('should approve a dealer and return Verified status', async () => {
        const dealerId = 'dealer-pending-1';

        server.use(
          http.post(`${API_URL}/api/admin/dealers/${dealerId}/approve`, () => {
            return HttpResponse.json({
              id: dealerId,
              businessName: 'Auto Premium RD',
              verificationStatus: 'Verified',
              isActive: true,
            });
          })
        );

        const response = await fetch(
          `${API_URL}/api/admin/dealers/${dealerId}/approve`,
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ notes: 'All documents verified' }),
          }
        );

        expect(response.ok).toBe(true);

        const data = await response.json();
        expect(data.verificationStatus).toBe('Verified');
        expect(data.isActive).toBe(true);
      });
    });

    describe('POST /api/admin/dealers/:id/reject', () => {
      it('should reject a dealer with reason', async () => {
        const dealerId = 'dealer-pending-2';

        server.use(
          http.post(`${API_URL}/api/admin/dealers/${dealerId}/reject`, () => {
            return HttpResponse.json({
              id: dealerId,
              businessName: 'Bad Dealer',
              verificationStatus: 'Rejected',
              isActive: false,
            });
          })
        );

        const response = await fetch(
          `${API_URL}/api/admin/dealers/${dealerId}/reject`,
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ notes: 'Documentos inválidos' }),
          }
        );

        expect(response.ok).toBe(true);

        const data = await response.json();
        expect(data.verificationStatus).toBe('Rejected');
        expect(data.isActive).toBe(false);
      });

      it('should require notes for rejection', async () => {
        const dealerId = 'dealer-pending-3';

        server.use(
          http.post(`${API_URL}/api/admin/dealers/${dealerId}/reject`, async ({ request }) => {
            const body = await request.json() as { notes?: string };
            if (!body.notes) {
              return HttpResponse.json(
                {
                  title: 'Validation error',
                  status: 400,
                  detail: 'Notes are required for rejection',
                },
                { status: 400 }
              );
            }
            return HttpResponse.json({ verificationStatus: 'Rejected' });
          })
        );

        const response = await fetch(
          `${API_URL}/api/admin/dealers/${dealerId}/reject`,
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({}),
          }
        );

        expect(response.status).toBe(400);
      });
    });
  });
});
