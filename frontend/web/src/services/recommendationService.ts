import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

// ==================== INTERFACES ====================

export interface Recommendation {
  id: string;
  userId: string;
  vehicleId: string;
  type: RecommendationType;
  score: number;
  reason: string;
  metadata: Record<string, any>;
  createdAt: string;
  viewedAt?: string;
  clickedAt?: string;
  isRelevant: boolean;
}

export interface UserPreference {
  id: string;
  userId: string;
  preferredMakes: string[];
  preferredModels: string[];
  preferredBodyTypes: string[];
  preferredFuelTypes: string[];
  preferredTransmissions: string[];
  minYear?: number;
  maxYear?: number;
  minPrice?: number;
  maxPrice?: number;
  minMileage?: number;
  maxMileage?: number;
  preferredColors: string[];
  preferredFeatures: string[];
  confidence: number;
  totalVehiclesViewed: number;
  totalSearches: number;
  totalFavorites: number;
  totalContacts: number;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleInteraction {
  id: string;
  userId: string;
  vehicleId: string;
  type: InteractionType;
  createdAt: string;
  durationSeconds: number;
  source?: string;
}

export const RecommendationType = {
  ForYou: 'ForYou',
  Similar: 'Similar',
  AlsoViewed: 'AlsoViewed',
  Popular: 'Popular',
  Trending: 'Trending',
  RecentlyViewed: 'RecentlyViewed',
} as const;

export type RecommendationType = (typeof RecommendationType)[keyof typeof RecommendationType];

export const InteractionType = {
  View: 'View',
  Favorite: 'Favorite',
  Contact: 'Contact',
  Share: 'Share',
  Compare: 'Compare',
} as const;

export type InteractionType = (typeof InteractionType)[keyof typeof InteractionType];

export interface TrackInteractionRequest {
  vehicleId: string;
  type: string;
  durationSeconds?: number;
  source?: string;
}

// ==================== SERVICE ====================

class RecommendationServiceClass {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: `${API_URL}/api`,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Interceptor para agregar token JWT
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Add refresh token interceptor for automatic token renewal
    addRefreshTokenInterceptor(this.api);
  }

  // ==================== RECOMENDACIONES ====================

  /**
   * Obtener recomendaciones "Para ti"
   */
  async getForYouRecommendations(limit: number = 10): Promise<Recommendation[]> {
    const response = await this.api.get<Recommendation[]>('/recommendations/for-you', {
      params: { limit },
    });
    return response.data;
  }

  /**
   * Obtener vehículos similares
   */
  async getSimilarVehicles(vehicleId: string, limit: number = 10): Promise<Recommendation[]> {
    const response = await this.api.get<Recommendation[]>(`/recommendations/similar/${vehicleId}`, {
      params: { limit },
    });
    return response.data;
  }

  /**
   * Generar nuevas recomendaciones
   */
  async generateRecommendations(limit: number = 10): Promise<Recommendation[]> {
    const response = await this.api.post<Recommendation[]>('/recommendations/generate', {
      limit,
    });
    return response.data;
  }

  /**
   * Marcar recomendación como vista
   */
  async markRecommendationViewed(recommendationId: string): Promise<void> {
    await this.api.post(`/recommendations/${recommendationId}/viewed`);
  }

  /**
   * Marcar recomendación como clickeada
   */
  async markRecommendationClicked(recommendationId: string): Promise<void> {
    await this.api.post(`/recommendations/${recommendationId}/clicked`);
  }

  /**
   * Obtener preferencias del usuario
   */
  async getUserPreferences(): Promise<UserPreference> {
    const response = await this.api.get<UserPreference>('/recommendations/preferences');
    return response.data;
  }

  // ==================== INTERACCIONES ====================

  /**
   * Registrar interacción con vehículo
   */
  async trackInteraction(request: TrackInteractionRequest): Promise<VehicleInteraction> {
    const response = await this.api.post<VehicleInteraction>('/interactions', request);
    return response.data;
  }

  /**
   * Registrar interacción anónima (sin auth)
   */
  async trackAnonymousInteraction(request: TrackInteractionRequest): Promise<void> {
    await this.api.post('/interactions/anonymous', request);
  }

  // ==================== HELPERS ====================

  /**
   * Track view de vehículo (helper)
   */
  async trackVehicleView(
    vehicleId: string,
    durationSeconds: number = 0,
    source: string = 'direct'
  ): Promise<void> {
    try {
      await this.trackInteraction({
        vehicleId,
        type: InteractionType.View,
        durationSeconds,
        source,
      });
    } catch (error) {
      // Si falla (ej: no autenticado), intentar anonymous tracking
      try {
        await this.trackAnonymousInteraction({
          vehicleId,
          type: InteractionType.View,
          durationSeconds,
          source,
        });
      } catch {
        // Silently fail - no bloquear la experiencia del usuario
        console.warn('Failed to track vehicle view');
      }
    }
  }

  /**
   * Track favorite de vehículo
   */
  async trackVehicleFavorite(vehicleId: string, source: string = 'detail'): Promise<void> {
    await this.trackInteraction({
      vehicleId,
      type: InteractionType.Favorite,
      source,
    });
  }

  /**
   * Track contact de vendedor
   */
  async trackVehicleContact(vehicleId: string, source: string = 'detail'): Promise<void> {
    await this.trackInteraction({
      vehicleId,
      type: InteractionType.Contact,
      source,
    });
  }

  /**
   * Track share de vehículo
   */
  async trackVehicleShare(vehicleId: string, source: string = 'detail'): Promise<void> {
    await this.trackInteraction({
      vehicleId,
      type: InteractionType.Share,
      source,
    });
  }

  /**
   * Track compare de vehículo
   */
  async trackVehicleCompare(vehicleId: string, source: string = 'detail'): Promise<void> {
    await this.trackInteraction({
      vehicleId,
      type: InteractionType.Compare,
      source,
    });
  }

  // ==================== FORMATTERS ====================

  /**
   * Formatear tipo de recomendación para UI
   */
  formatRecommendationType(type: RecommendationType): string {
    const types: Record<RecommendationType, string> = {
      [RecommendationType.ForYou]: 'Para ti',
      [RecommendationType.Similar]: 'Similar',
      [RecommendationType.AlsoViewed]: 'También vieron',
      [RecommendationType.Popular]: 'Popular',
      [RecommendationType.Trending]: 'Tendencia',
      [RecommendationType.RecentlyViewed]: 'Vistos recientemente',
    };
    return types[type] || type;
  }

  /**
   * Formatear score (0-1) a porcentaje
   */
  formatScore(score: number): string {
    return `${Math.round(score * 100)}%`;
  }

  /**
   * Obtener color badge por confidence
   */
  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.7) return 'green';
    if (confidence >= 0.4) return 'yellow';
    return 'gray';
  }

  /**
   * Formatear confidence para UI
   */
  formatConfidence(confidence: number): string {
    if (confidence >= 0.7) return 'Alta';
    if (confidence >= 0.4) return 'Media';
    return 'Baja';
  }
}

export const recommendationService = new RecommendationServiceClass();
