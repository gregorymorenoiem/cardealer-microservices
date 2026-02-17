import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import type { VehicleFormData } from '@/pages/vehicles/SellYourCarPage';

vi.mock('@/services/vehicleIntelligenceService', () => ({
  default: {
    analyzePricing: vi.fn(),
    predictDemand: vi.fn(),
  },
}));

import vehicleIntelligenceService from '@/services/vehicleIntelligenceService';
import PricingStep from './PricingStep';

describe('PricingStep (Sprint 18)', () => {
  it('renders intelligent pricing suggestion when enough data is provided', async () => {
    localStorage.setItem('accessToken', 'mock-access-token');
    localStorage.setItem('refreshToken', 'mock-refresh-token');

    vi.mocked(vehicleIntelligenceService.analyzePricing).mockResolvedValueOnce({
      id: 'test-id',
      vehicleId: 'test-vehicle-id',
      currentPrice: 25000,
      suggestedPrice: 29500,
      suggestedPriceMin: 28000,
      suggestedPriceMax: 31000,
      marketAvgPrice: 28000,
      priceVsMarket: 0.054,
      pricePosition: 'Fair',
      predictedDaysToSaleAtCurrentPrice: 23,
      predictedDaysToSaleAtSuggestedPrice: 18,
      confidenceScore: 84,
      analysisDate: new Date().toISOString(),
      recommendations: [
        {
          id: 'rec1',
          type: 'Fotos',
          reason: 'Publica fotos claras',
          impactDescription: 'Aumenta visibilidad',
          priority: 1,
        },
      ],
      comparables: [],
    });

    const baseData: Partial<VehicleFormData> = {
      make: 'Toyota',
      model: 'Camry',
      year: 2022,
      mileage: 25000,
      bodyType: 'Sedan',
      sellerType: 'private',
    };

    render(<PricingStep data={baseData} onNext={vi.fn()} onBack={vi.fn()} />);

    fireEvent.change(screen.getByPlaceholderText('50,000'), { target: { value: '25000' } });
    fireEvent.change(screen.getByPlaceholderText('e.g., Los Angeles, CA'), {
      target: { value: 'Santo Domingo' },
    });

    // Debounce is 500ms in the component
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 600));
    });

    await waitFor(() => {
      expect(screen.getByText('Precio sugerido')).toBeInTheDocument();
    });

    expect(screen.getByText(/Rango:/)).toBeInTheDocument();
    expect(screen.getByText(/Tiempo estimado de venta/)).toBeInTheDocument();
  });
});
