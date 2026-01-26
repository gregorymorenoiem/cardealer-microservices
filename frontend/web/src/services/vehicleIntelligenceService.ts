import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

// ==========================================
// INTERFACES & TYPES
// ==========================================

export interface PriceAnalysisDto {
  id: string;
  vehicleId: string;
  currentPrice: number;
  suggestedPrice: number;
  suggestedPriceMin: number;
  suggestedPriceMax: number;
  marketAvgPrice: number;
  priceVsMarket: number;
  pricePosition: 'Above Market' | 'Below Market' | 'Fair';
  predictedDaysToSaleAtCurrentPrice: number;
  predictedDaysToSaleAtSuggestedPrice: number;
  confidenceScore: number;
  analysisDate: string;
  recommendations: PriceRecommendationDto[];
  comparables: MarketComparableDto[];
}

export interface PriceRecommendationDto {
  id: string;
  type: string;
  reason: string;
  suggestedValue?: number;
  impactDescription: string;
  priority: number;
}

export interface MarketComparableDto {
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

export interface DemandPredictionDto {
  id: string;
  make: string;
  model: string;
  year: number;
  currentDemand: string;
  demandScore: number;
  trend: string;
  trendStrength: number;
  predictedDemand30Days: string;
  predictedDemand90Days: string;
  searchesPerDay: number;
  availableInventory: number;
  avgDaysToSale: number;
  buyRecommendation: string;
  buyRecommendationReason: string;
  insights: string[];
  predictionDate: string;
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
  photoCount: number;
  viewCount: number;
  daysListed: number;
}

export interface CreateDemandPredictionRequest {
  make: string;
  model: string;
  year: number;
  fuelType?: string;
  transmission?: string;
}

// New DTOs for Market Analysis and ML Dashboard
export interface CategoryDemandDto {
  category: string;
  demandLevel: string;
  demandScore: number;
  avgDaysToSale: number;
  totalSearches: number;
  activeListings: number;
  updatedAt: string;
}

export interface MarketAnalysisDto {
  make: string;
  model: string;
  year: number;
  totalListings: number;
  avgPrice: number;
  minPrice: number;
  maxPrice: number;
  avgDaysToSale: number;
  medianDaysToSale: number;
  priceTrend: string;
  demandTrend: string;
  competitorCount: number;
  marketShare: number;
  recommendations: string[];
}

export interface MLStatisticsDto {
  totalInferences: number;
  successRate: number;
  errorsLast24h: number;
  lastUpdated: string;
}

export interface ModelPerformanceDto {
  modelName: string;
  accuracy: number;
  mae: number;
  rmse: number;
  lastTrained: string;
  nextTraining: string;
  status: 'healthy' | 'warning' | 'error';
}

export interface InferenceMetricsDto {
  totalInferences: number;
  successRate: number;
  avgLatencyMs: number;
  p95LatencyMs: number;
  p99LatencyMs: number;
  errorsLast24h: number;
  lastUpdated: string;
}

// ==========================================
// VEHICLE INTELLIGENCE SERVICE
// ==========================================

class VehicleIntelligenceService {
  private api: AxiosInstance;

  constructor() {
    const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

    this.api = axios.create({
      baseURL: `${baseURL}/api`,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add auth token interceptor
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('accessToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Add refresh token interceptor for automatic token refresh on 401
    addRefreshTokenInterceptor(this.api);
  }

  // ==========================================
  // PRICING ANALYSIS
  // ==========================================

  /**
   * Analiza el precio de un vehículo y genera recomendaciones
   */
  async analyzePricing(request: CreatePriceAnalysisRequest): Promise<PriceAnalysisDto> {
    const response = await this.api.post<PriceAnalysisDto>('/pricing/analyze', request);
    return response.data;
  }

  /**
   * Obtiene el último análisis de precio para un vehículo
   */
  async getLatestPriceAnalysis(vehicleId: string): Promise<PriceAnalysisDto | null> {
    try {
      const response = await this.api.get<PriceAnalysisDto>(`/pricing/vehicle/${vehicleId}/latest`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /**
   * Obtiene un análisis de precio específico por ID
   */
  async getPriceAnalysisById(id: string): Promise<PriceAnalysisDto | null> {
    try {
      const response = await this.api.get<PriceAnalysisDto>(`/pricing/${id}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  }

  // ==========================================
  // DEMAND PREDICTION
  // ==========================================

  /**
   * Predice la demanda para un vehículo específico
   */
  async predictDemand(request: CreateDemandPredictionRequest): Promise<DemandPredictionDto> {
    const response = await this.api.post<DemandPredictionDto>('/demand/predict', request);
    return response.data;
  }

  /**
   * Obtiene la última predicción de demanda
   */
  async getDemandPrediction(
    make: string,
    model: string,
    year: number
  ): Promise<DemandPredictionDto | null> {
    try {
      const response = await this.api.get<DemandPredictionDto>(`/demand/${make}/${model}/${year}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  }

  // ==========================================
  // MARKET ANALYSIS (VehicleIntelligenceService)
  // ==========================================

  /**
   * Obtiene demanda por categoría para dashboard de dealers
   */
  async getDemandByCategory(): Promise<CategoryDemandDto[]> {
    const response = await this.api.get<CategoryDemandDto[]>(
      '/vehicleintelligence/demand/categories'
    );
    return response.data;
  }

  /**
   * Obtiene análisis de mercado para make/model/year específico
   */
  async getMarketAnalysis(
    make: string,
    model: string,
    year: number
  ): Promise<MarketAnalysisDto | null> {
    try {
      const response = await this.api.get<MarketAnalysisDto>(
        `/vehicleintelligence/market-analysis/${make}/${model}/${year}`
      );
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  }

  /**
   * Obtiene dashboard de análisis de mercado con filtros opcionales
   */
  async getMarketAnalysisDashboard(filters?: {
    make?: string;
    model?: string;
    minYear?: number;
    maxYear?: number;
    fuelType?: string;
    bodyType?: string;
  }): Promise<MarketAnalysisDto[]> {
    const response = await this.api.get<MarketAnalysisDto[]>(
      '/vehicleintelligence/market-analysis/dashboard',
      { params: filters }
    );
    return response.data;
  }

  // ==========================================
  // ML METRICS (Admin Only)
  // ==========================================

  /**
   * Obtiene estadísticas del servicio ML (admin only)
   */
  async getMLStatistics(): Promise<MLStatisticsDto> {
    const response = await this.api.get<MLStatisticsDto>('/vehicleintelligence/ml/statistics');
    return response.data;
  }

  /**
   * Obtiene performance de modelos ML (admin only)
   */
  async getModelPerformance(): Promise<ModelPerformanceDto[]> {
    const response = await this.api.get<ModelPerformanceDto[]>(
      '/vehicleintelligence/ml/performance'
    );
    return response.data;
  }

  /**
   * Obtiene métricas de inferencia (admin only)
   */
  async getInferenceMetrics(): Promise<InferenceMetricsDto> {
    const response = await this.api.get<InferenceMetricsDto>('/vehicleintelligence/ml/metrics');
    return response.data;
  }

  // ==========================================
  // HELPERS & UTILITIES
  // ==========================================

  /**
   * Formatea precio en formato RD$
   */
  formatPrice(price: number): string {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(price);
  }

  /**
   * Calcula el ahorro potencial
   */
  calculateSavings(currentPrice: number, suggestedPrice: number): number {
    return currentPrice - suggestedPrice;
  }

  /**
   * Calcula el porcentaje de diferencia
   */
  calculatePercentageDiff(currentPrice: number, suggestedPrice: number): number {
    if (suggestedPrice === 0) return 0;
    return ((currentPrice - suggestedPrice) / suggestedPrice) * 100;
  }

  /**
   * Obtiene color para price position
   */
  getPricePositionColor(position: string): string {
    switch (position) {
      case 'Above Market':
        return 'text-red-600 bg-red-50';
      case 'Below Market':
        return 'text-green-600 bg-green-50';
      case 'Fair':
        return 'text-blue-600 bg-blue-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  }

  /**
   * Obtiene color para demand level
   */
  getDemandLevelColor(level: string): string {
    switch (level) {
      case 'VeryHigh':
        return 'text-green-700 bg-green-100';
      case 'High':
        return 'text-green-600 bg-green-50';
      case 'Medium':
        return 'text-yellow-600 bg-yellow-50';
      case 'Low':
        return 'text-orange-600 bg-orange-50';
      case 'VeryLow':
        return 'text-red-600 bg-red-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  }

  /**
   * Obtiene badge para buy recommendation
   */
  getBuyRecommendationBadge(recommendation: string): {
    text: string;
    color: string;
    icon: string;
  } {
    switch (recommendation) {
      case 'StrongBuy':
        return {
          text: 'Compra Fuerte',
          color: 'bg-green-600 text-white',
          icon: '🔥',
        };
      case 'Buy':
        return {
          text: 'Compra',
          color: 'bg-green-500 text-white',
          icon: '✅',
        };
      case 'Hold':
        return {
          text: 'Esperar',
          color: 'bg-yellow-500 text-white',
          icon: '⏸️',
        };
      case 'Avoid':
        return {
          text: 'Evitar',
          color: 'bg-red-500 text-white',
          icon: '❌',
        };
      default:
        return {
          text: 'Sin recomendación',
          color: 'bg-gray-500 text-white',
          icon: '❓',
        };
    }
  }

  /**
   * Obtiene texto amigable para demand level
   */
  getDemandLevelText(level: string): string {
    switch (level) {
      case 'VeryHigh':
        return 'Muy Alta (< 15 días)';
      case 'High':
        return 'Alta (15-30 días)';
      case 'Medium':
        return 'Media (30-60 días)';
      case 'Low':
        return 'Baja (60-90 días)';
      case 'VeryLow':
        return 'Muy Baja (> 90 días)';
      default:
        return level;
    }
  }

  /**
   * Obtiene icono de tendencia
   */
  getTrendIcon(trend: string): string {
    switch (trend) {
      case 'Rising':
        return '📈';
      case 'Falling':
        return '📉';
      case 'Stable':
        return '➡️';
      default:
        return '❓';
    }
  }

  /**
   * Obtiene texto de tendencia
   */
  getTrendText(trend: string): string {
    switch (trend) {
      case 'Rising':
        return 'Al alza';
      case 'Falling':
        return 'A la baja';
      case 'Stable':
        return 'Estable';
      default:
        return trend;
    }
  }

  /**
   * Genera descripción del análisis de precio
   */
  generatePriceAnalysisSummary(analysis: PriceAnalysisDto): string {
    const diff = this.calculatePercentageDiff(analysis.currentPrice, analysis.suggestedPrice);
    const diffAbs = Math.abs(diff);

    if (analysis.pricePosition === 'Above Market') {
      return `Tu precio está ${diffAbs.toFixed(1)}% arriba del mercado. Se vendería en ${analysis.predictedDaysToSaleAtCurrentPrice} días. Si reduces a ${this.formatPrice(analysis.suggestedPrice)}, podría venderse en ${analysis.predictedDaysToSaleAtSuggestedPrice} días.`;
    } else if (analysis.pricePosition === 'Below Market') {
      return `Tu precio está ${diffAbs.toFixed(1)}% abajo del mercado. Podrías aumentarlo a ${this.formatPrice(analysis.suggestedPrice)} sin afectar las ventas.`;
    } else {
      return `Tu precio está dentro del rango de mercado. Tiempo estimado de venta: ${analysis.predictedDaysToSaleAtCurrentPrice} días.`;
    }
  }

  /**
   * Genera resumen de demanda
   */
  generateDemandSummary(prediction: DemandPredictionDto): string {
    const demandText = this.getDemandLevelText(prediction.currentDemand);
    const trendText = this.getTrendText(prediction.trend);

    return `Demanda actual: ${demandText}. Tendencia: ${trendText}. Se vende en ${prediction.avgDaysToSale} días promedio.`;
  }
}

// Export singleton instance
const vehicleIntelligenceService = new VehicleIntelligenceService();
export default vehicleIntelligenceService;
