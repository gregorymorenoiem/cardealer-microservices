import axios, { AxiosInstance } from 'axios';

// Types
export interface UserFeature {
  id: string;
  userId: string;
  featureName: string;
  featureValue: string;
  featureType: string;
  version: number;
  computedAt: string;
  expiresAt: string | null;
  source: string;
}

export interface VehicleFeature {
  id: string;
  vehicleId: string;
  featureName: string;
  featureValue: string;
  featureType: string;
  version: number;
  computedAt: string;
  expiresAt: string | null;
  source: string;
}

export interface FeatureDefinition {
  id: string;
  featureName: string;
  category: string;
  description: string;
  featureType: string;
  isActive: boolean;
  computationLogic: string;
  refreshIntervalHours: number;
  createdAt: string;
  updatedAt: string;
}

export interface UpsertUserFeatureRequest {
  userId: string;
  featureName: string;
  featureValue: string;
  featureType?: string;
  version?: number;
  expiresAt?: string;
}

export interface UpsertVehicleFeatureRequest {
  vehicleId: string;
  featureName: string;
  featureValue: string;
  featureType?: string;
  version?: number;
  expiresAt?: string;
}

class FeatureStoreService {
  private api: AxiosInstance;

  constructor() {
    const apiUrl = import.meta.env.VITE_API_URL || 'https://api.okla.com.do';
    
    this.api = axios.create({
      baseURL: `${apiUrl}/api/features`,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Add JWT token interceptor
    this.api.interceptors.request.use(config => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  // User Features
  async getUserFeatures(userId: string): Promise<UserFeature[]> {
    const response = await this.api.get<UserFeature[]>(`/users/${userId}`);
    return response.data;
  }

  async upsertUserFeature(request: UpsertUserFeatureRequest): Promise<UserFeature> {
    const response = await this.api.post<UserFeature>('/users', request);
    return response.data;
  }

  // Vehicle Features
  async getVehicleFeatures(vehicleId: string): Promise<VehicleFeature[]> {
    const response = await this.api.get<VehicleFeature[]>(`/vehicles/${vehicleId}`);
    return response.data;
  }

  async upsertVehicleFeature(request: UpsertVehicleFeatureRequest): Promise<VehicleFeature> {
    const response = await this.api.post<VehicleFeature>('/vehicles', request);
    return response.data;
  }

  // Feature Definitions
  async getFeatureDefinitions(category?: string): Promise<FeatureDefinition[]> {
    const response = await this.api.get<FeatureDefinition[]>('/definitions', {
      params: category ? { category } : {}
    });
    return response.data;
  }

  // Helper: Get feature type color
  getFeatureTypeColor(featureType: string): string {
    const colors: Record<string, string> = {
      'Numeric': 'blue',
      'Categorical': 'green',
      'Boolean': 'purple',
      'List': 'orange'
    };
    return colors[featureType] || 'gray';
  }

  // Helper: Parse feature value based on type
  parseFeatureValue(feature: UserFeature | VehicleFeature): any {
    switch (feature.featureType) {
      case 'Numeric':
        return parseFloat(feature.featureValue);
      case 'Boolean':
        return feature.featureValue === 'true';
      case 'List':
        try {
          return JSON.parse(feature.featureValue);
        } catch {
          return feature.featureValue.split(',');
        }
      default:
        return feature.featureValue;
    }
  }
}

export default new FeatureStoreService();
