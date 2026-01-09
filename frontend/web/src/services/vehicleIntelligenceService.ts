import api from './api';

export type DemandTrend = 'Up' | 'Down' | 'Stable' | string;

export interface PriceSuggestionRequest {
  make: string;
  model: string;
  year: number;
  mileage: number;
  bodyType?: string | null;
  location?: string | null;
  askingPrice: number;
}

export interface PriceSuggestion {
  marketPrice: number;
  suggestedPrice: number;
  deltaPercent: number;
  demandScore: number;
  estimatedDaysToSell: number;
  confidence: number;
  modelVersion: string;
  sellingTips: string[];
}

export interface CategoryDemandDto {
  category: string;
  demandScore: number;
  trend: DemandTrend;
  updatedAt: string;
}

export const vehicleIntelligenceService = {
  async getPriceSuggestion(request: PriceSuggestionRequest): Promise<PriceSuggestion> {
    const response = await api.post<PriceSuggestion>(
      '/api/vehicleintelligence/price-suggestion',
      request
    );

    return response.data;
  },

  async getCategoryDemand(): Promise<CategoryDemandDto[]> {
    const response = await api.get<CategoryDemandDto[]>(
      '/api/vehicleintelligence/demand/categories'
    );

    return response.data;
  },
};
