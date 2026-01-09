import axios, { AxiosInstance } from 'axios';

// ============================================
// INTERFACES - Analytics DTOs
// ============================================

export interface AnalyticsDashboard {
  summary: AnalyticsSummary;
  viewsTrend: TimeseriesDataPoint[];
  contactMethodBreakdown: ContactMethodStats[];
  deviceBreakdown: DeviceStats[];
  topReferrers: TopReferrer[];
  liveStats: LiveStats;
}

export interface AnalyticsSummary {
  totalViews: number;
  uniqueVisitors: number;
  averageViewDuration: number;
  totalContacts: number;
  contactConversionRate: number;
  inquiryConversionRate: number;
  bounceRate: number;
  engagementRate: number;
  comparedToLastPeriod?: PeriodComparison;
}

export interface TimeseriesDataPoint {
  date: string;
  views: number;
  contacts: number;
  uniqueVisitors: number;
}

export interface ContactMethodStats {
  type: ContactType;
  label: string;
  count: number;
  percentage: number;
  convertedCount: number;
  conversionRate: number;
}

export interface DeviceStats {
  deviceType: string;
  count: number;
  percentage: number;
}

export interface TopReferrer {
  source: string;
  views: number;
  percentage: number;
}

export interface LiveStats {
  currentViewers: number;
  mostRecentView?: MostRecentView;
  viewsToday: number;
  contactsToday: number;
}

export interface MostRecentView {
  viewedAt: string;
  deviceType: string;
  city?: string;
  country?: string;
}

export interface PeriodComparison {
  currentPeriod: number;
  previousPeriod: number;
  changePercentage: number;
  isIncrease: boolean;
}

export interface TrackProfileViewRequest {
  dealerId: string;
  viewerUserId?: string;
  viewerIpAddress?: string;
  viewerUserAgent?: string;
  referrerUrl?: string;
  viewedPage?: string;
  durationSeconds?: number;
}

export interface TrackContactEventRequest {
  dealerId: string;
  contactType: ContactType;
  viewerUserId?: string;
  viewerIpAddress?: string;
  contactValue?: string;
  vehicleId?: string;
  source?: string;
}

export enum ContactType {
  Phone = 1,
  Email = 2,
  WhatsApp = 3,
  Website = 4,
  SocialMedia = 5,
}

// ============================================
// SERVICE CLASS
// ============================================

export class DealerAnalyticsService {
  private api: AxiosInstance;

  constructor() {
    const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:18443';

    this.api = axios.create({
      baseURL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Interceptor para agregar JWT token automáticamente
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );
  }

  // ============================================
  // DASHBOARD ANALYTICS
  // ============================================

  /**
   * Get analytics dashboard for a dealer
   */
  async getDashboard(
    dealerId: string,
    startDate?: string,
    endDate?: string
  ): Promise<AnalyticsDashboard> {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await this.api.get(
      `/api/analytics/dashboard/${dealerId}?${params.toString()}`
    );
    return response.data;
  }

  /**
   * Track profile view (anonymous call)
   */
  async trackView(request: TrackProfileViewRequest): Promise<void> {
    // Get client info
    const enrichedRequest = {
      ...request,
      viewerUserAgent: navigator.userAgent,
      viewedPage: window.location.pathname,
    };

    try {
      await this.api.post('/api/analytics/track/view', enrichedRequest);
    } catch (error) {
      console.error('Error tracking profile view:', error);
      // Silent fail - don't disrupt user experience
    }
  }

  /**
   * Track contact button click (anonymous call)
   */
  async trackContact(request: TrackContactEventRequest): Promise<void> {
    try {
      await this.api.post('/api/analytics/track/contact', request);
    } catch (error) {
      console.error('Error tracking contact event:', error);
      // Silent fail
    }
  }

  // ============================================
  // HELPER METHODS
  // ============================================

  /**
   * Format duration in seconds to human-readable format
   */
  formatDuration(seconds: number): string {
    if (seconds < 60) return `${seconds}s`;
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return secs > 0 ? `${minutes}m ${secs}s` : `${minutes}m`;
  }

  /**
   * Format percentage with 1 decimal
   */
  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  /**
   * Get icon for device type
   */
  getDeviceIcon(deviceType: string): string {
    switch (deviceType.toLowerCase()) {
      case 'mobile':
        return '📱';
      case 'tablet':
        return '📱';
      case 'desktop':
        return '💻';
      default:
        return '🖥️';
    }
  }

  /**
   * Get icon for contact type
   */
  getContactTypeIcon(type: ContactType): string {
    switch (type) {
      case ContactType.Phone:
        return '📞';
      case ContactType.Email:
        return '✉️';
      case ContactType.WhatsApp:
        return '💬';
      case ContactType.Website:
        return '🌐';
      case ContactType.SocialMedia:
        return '📱';
      default:
        return '📧';
    }
  }

  /**
   * Get color for contact type (Tailwind classes)
   */
  getContactTypeColor(type: ContactType): string {
    switch (type) {
      case ContactType.Phone:
        return 'blue';
      case ContactType.Email:
        return 'purple';
      case ContactType.WhatsApp:
        return 'green';
      case ContactType.Website:
        return 'orange';
      case ContactType.SocialMedia:
        return 'pink';
      default:
        return 'gray';
    }
  }

  /**
   * Calculate date range for "Last X Days"
   */
  getDateRange(days: number): { startDate: string; endDate: string } {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - days);

    return {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
    };
  }

  /**
   * Format number with thousands separator
   */
  formatNumber(value: number): string {
    return new Intl.NumberFormat('es-DO').format(value);
  }

  /**
   * Get trend direction icon
   */
  getTrendIcon(changePercentage: number): string {
    if (changePercentage > 0) return '📈';
    if (changePercentage < 0) return '📉';
    return '➡️';
  }

  /**
   * Get trend color (Tailwind classes)
   */
  getTrendColor(changePercentage: number): string {
    if (changePercentage > 0) return 'text-green-600';
    if (changePercentage < 0) return 'text-red-600';
    return 'text-gray-600';
  }
}

// Export singleton instance
export const dealerAnalyticsService = new DealerAnalyticsService();
