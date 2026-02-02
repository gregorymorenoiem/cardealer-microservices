/**
 * Vehicle Intelligence Service
 *
 * API client for VehicleIntelligenceService backend
 * AI-powered pricing recommendations and market analysis
 */

import { apiClient } from '@/lib/api-client';

// ============================================================================
// Types
// ============================================================================

export interface PriceRecommendation {
  id: string;
  type: string;
  reason: string;
  suggestedValue?: number;
  impactDescription: string;
  priority: number;
}

export interface MarketComparable {
  id: string;
  source: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  price: number;
  similarityScore: number;
  status: string;
  daysOnMarket?: number;
  externalUrl?: string;
}

export interface PriceAnalysis {
  id: string;
  vehicleId: string;
  currentPrice: number;
  suggestedPrice: number;
  suggestedPriceMin: number;
  suggestedPriceMax: number;
  marketAvgPrice: number;
  priceVsMarket: number;
  pricePosition: 'below' | 'at' | 'above' | string;
  predictedDaysToSaleAtCurrentPrice: number;
  predictedDaysToSaleAtSuggestedPrice: number;
  confidenceScore: number;
  analysisDate: string;
  recommendations: PriceRecommendation[];
  comparables: MarketComparable[];
}

export interface DemandPrediction {
  id: string;
  make: string;
  model: string;
  year: number;
  currentDemand: 'Low' | 'Medium' | 'High' | 'VeryHigh' | string;
  demandScore: number;
  trend: 'Decreasing' | 'Stable' | 'Increasing' | string;
  trendStrength: number;
  predictedDemand30Days: string;
  predictedDemand90Days: string;
  searchesPerDay: number;
  availableInventory: number;
  avgDaysToSale: number;
  buyRecommendation: 'Strong' | 'Moderate' | 'Weak' | 'Avoid' | string;
  buyRecommendationReason: string;
  insights: string[];
  predictionDate: string;
}

export interface CategoryDemand {
  category: string;
  demandScore: number;
  trend: string;
  avgDaysToSale: number;
  inventoryCount: number;
}

export interface MarketAnalysis {
  make: string;
  model: string;
  year: number;
  avgPrice: number;
  minPrice: number;
  maxPrice: number;
  activeListings: number;
  avgDaysOnMarket: number;
  demandScore: number;
  priceHistory: { date: string; avgPrice: number }[];
}

export interface PriceSuggestionRequest {
  make: string;
  model: string;
  year: number;
  mileage: number;
  condition?: string;
  fuelType?: string;
  transmission?: string;
  currentPrice?: number;
}

export interface PriceSuggestion {
  suggestedPrice: number;
  suggestedPriceMin: number;
  suggestedPriceMax: number;
  marketAvgPrice: number;
  demandLevel: string;
  estimatedDaysToSale: number;
  confidence: number;
  reasoning: string[];
}

export interface CreatePriceAnalysisRequest {
  vehicleId: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  condition?: string;
  fuelType?: string;
  transmission?: string;
  currentPrice: number;
  photoCount?: number;
  viewCount?: number;
  daysListed?: number;
}

// ============================================================================
// API Functions
// ============================================================================

/**
 * Get price suggestion for a vehicle (for publishing wizard)
 */
export async function getPriceSuggestion(data: PriceSuggestionRequest): Promise<PriceSuggestion> {
  const response = await apiClient.post<PriceSuggestion>(
    '/api/vehicleintelligence/price-suggestion',
    data
  );
  return response.data;
}

/**
 * Create a full price analysis for a vehicle
 */
export async function createPriceAnalysis(
  data: CreatePriceAnalysisRequest
): Promise<PriceAnalysis> {
  const response = await apiClient.post<PriceAnalysis>('/api/pricing/analyze', data);
  return response.data;
}

/**
 * Get latest price analysis for a vehicle
 */
export async function getLatestPriceAnalysis(vehicleId: string): Promise<PriceAnalysis | null> {
  try {
    const response = await apiClient.get<PriceAnalysis>(`/api/pricing/vehicle/${vehicleId}/latest`);
    return response.data;
  } catch {
    return null;
  }
}

/**
 * Get price analysis by ID
 */
export async function getPriceAnalysisById(id: string): Promise<PriceAnalysis> {
  const response = await apiClient.get<PriceAnalysis>(`/api/pricing/${id}`);
  return response.data;
}

/**
 * Get demand by category
 */
export async function getDemandByCategory(): Promise<CategoryDemand[]> {
  const response = await apiClient.get<CategoryDemand[]>(
    '/api/vehicleintelligence/demand/categories'
  );
  return response.data;
}

/**
 * Get market analysis for specific make/model/year
 */
export async function getMarketAnalysis(
  make: string,
  model: string,
  year: number
): Promise<MarketAnalysis | null> {
  try {
    const response = await apiClient.get<MarketAnalysis>(
      `/api/vehicleintelligence/market-analysis/${encodeURIComponent(make)}/${encodeURIComponent(model)}/${year}`
    );
    return response.data;
  } catch {
    return null;
  }
}

/**
 * Get market analysis dashboard data
 */
export async function getMarketAnalysisDashboard(params?: {
  make?: string;
  bodyType?: string;
  minYear?: number;
  maxYear?: number;
}): Promise<MarketAnalysis[]> {
  const searchParams = new URLSearchParams();
  if (params?.make) searchParams.append('make', params.make);
  if (params?.bodyType) searchParams.append('bodyType', params.bodyType);
  if (params?.minYear) searchParams.append('minYear', String(params.minYear));
  if (params?.maxYear) searchParams.append('maxYear', String(params.maxYear));

  const query = searchParams.toString();
  const response = await apiClient.get<MarketAnalysis[]>(
    `/api/vehicleintelligence/market-analysis/dashboard${query ? `?${query}` : ''}`
  );
  return response.data;
}

/**
 * Get demand prediction for specific vehicle type
 */
export async function getDemandPrediction(
  make: string,
  model: string,
  year: number
): Promise<DemandPrediction | null> {
  try {
    const response = await apiClient.get<DemandPrediction>(
      `/api/demand/${encodeURIComponent(make)}/${encodeURIComponent(model)}/${year}`
    );
    return response.data;
  } catch {
    return null;
  }
}

// ============================================================================
// Helper Functions
// ============================================================================

export function formatPrice(price: number): string {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(price);
}

export function getPricePositionLabel(position: string): string {
  const labels: Record<string, string> = {
    below: 'Por debajo del mercado',
    at: 'Precio de mercado',
    above: 'Por encima del mercado',
  };
  return labels[position] || position;
}

export function getPricePositionColor(position: string): string {
  const colors: Record<string, string> = {
    below: 'text-emerald-600',
    at: 'text-blue-600',
    above: 'text-amber-600',
  };
  return colors[position] || 'text-gray-600';
}

export function getDemandColor(demand: string): string {
  const colors: Record<string, string> = {
    Low: 'bg-red-100 text-red-700',
    Medium: 'bg-amber-100 text-amber-700',
    High: 'bg-emerald-100 text-emerald-700',
    VeryHigh: 'bg-blue-100 text-blue-700',
  };
  return colors[demand] || 'bg-gray-100 text-gray-700';
}

export function getDemandLabel(demand: string): string {
  const labels: Record<string, string> = {
    Low: 'Demanda Baja',
    Medium: 'Demanda Media',
    High: 'Demanda Alta',
    VeryHigh: 'Demanda Muy Alta',
  };
  return labels[demand] || demand;
}

export function getTrendIcon(trend: string): string {
  const icons: Record<string, string> = {
    Decreasing: '📉',
    Stable: '➡️',
    Increasing: '📈',
  };
  return icons[trend] || '➡️';
}

export function getRecommendationType(
  currentPrice: number,
  suggestedPrice: number
): 'reduce' | 'increase' | 'maintain' {
  const diff = suggestedPrice - currentPrice;
  const threshold = currentPrice * 0.02; // 2% threshold

  if (diff < -threshold) return 'reduce';
  if (diff > threshold) return 'increase';
  return 'maintain';
}

export function getRecommendationColor(type: 'reduce' | 'increase' | 'maintain'): string {
  const colors: Record<string, string> = {
    reduce: 'bg-red-100 text-red-700',
    increase: 'bg-emerald-100 text-emerald-700',
    maintain: 'bg-gray-100 text-gray-700',
  };
  return colors[type];
}

export function getRecommendationLabel(type: 'reduce' | 'increase' | 'maintain'): string {
  const labels: Record<string, string> = {
    reduce: 'Reducir Precio',
    increase: 'Aumentar Precio',
    maintain: 'Precio Óptimo',
  };
  return labels[type];
}
