import axios, { AxiosInstance } from 'axios';

// Types
export interface UserBehaviorProfile {
  id: string;
  userId: string;
  userSegment: string;
  engagementScore: number;
  purchaseIntentScore: number;
  preferredMakes: string[];
  preferredModels: string[];
  preferredYears: number[];
  preferredPriceMin: number | null;
  preferredPriceMax: number | null;
  preferredBodyTypes: string[];
  preferredFuelTypes: string[];
  preferredTransmissions: string[];
  totalSearches: number;
  totalVehicleViews: number;
  totalContactRequests: number;
  totalFavorites: number;
  totalComparisons: number;
  totalSessions: number;
  totalTimeSpent: string; // TimeSpan
  recentSearchQueries: string[];
  recentVehicleViews: string[];
  lastActivityAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface UserAction {
  id: string;
  userId: string;
  actionType: string;
  actionDetails: string;
  relatedVehicleId: string | null;
  searchQuery: string | null;
  timestamp: string;
  sessionId: string;
  deviceType: string;
}

export interface UserBehaviorSummary {
  totalUsers: number;
  activeUsers7Days: number;
  activeUsers30Days: number;
  segmentDistribution: Record<string, number>;
  topMakes: Record<string, number>;
  topModels: Record<string, number>;
  averagePriceMin: number;
  averagePriceMax: number;
}

export interface RecordActionRequest {
  userId: string;
  actionType: string;
  actionDetails: string;
  relatedVehicleId?: string;
  searchQuery?: string;
  sessionId?: string;
  deviceType?: string;
}

class UserBehaviorService {
  private api: AxiosInstance;

  constructor() {
    const apiUrl = import.meta.env.VITE_API_URL || 'https://api.okla.com.do';
    
    this.api = axios.create({
      baseURL: `${apiUrl}/api/userbehavior`,
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

  async getUserProfile(userId: string): Promise<UserBehaviorProfile> {
    const response = await this.api.get<UserBehaviorProfile>(`/${userId}`);
    return response.data;
  }

  async getUserActions(userId: string, limit: number = 50): Promise<UserAction[]> {
    const response = await this.api.get<UserAction[]>(`/${userId}/actions`, {
      params: { limit }
    });
    return response.data;
  }

  async recordAction(request: RecordActionRequest): Promise<UserAction> {
    const response = await this.api.post<UserAction>('/actions', request);
    return response.data;
  }

  async getSummary(): Promise<UserBehaviorSummary> {
    const response = await this.api.get<UserBehaviorSummary>('/summary');
    return response.data;
  }

  // Helper: Get segment label in Spanish
  getSegmentLabel(segment: string): string {
    const labels: Record<string, string> = {
      'SeriousBuyer': 'Comprador Serio',
      'Researcher': 'Investigador',
      'Browser': 'Explorador',
      'TireKicker': 'Curioso',
      'Casual': 'Casual',
      'Unknown': 'Desconocido'
    };
    return labels[segment] || segment;
  }

  // Helper: Get segment color
  getSegmentColor(segment: string): string {
    const colors: Record<string, string> = {
      'SeriousBuyer': 'green',
      'Researcher': 'blue',
      'Browser': 'yellow',
      'TireKicker': 'gray',
      'Casual': 'purple',
      'Unknown': 'slate'
    };
    return colors[segment] || 'gray';
  }
}

export default new UserBehaviorService();
