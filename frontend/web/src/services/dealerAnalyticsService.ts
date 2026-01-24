import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { addRefreshTokenInterceptor } from './api';

// ============================================
// Sprint 12+ Dealer Analytics DTOs
// (Used by useDealerAnalytics hook + AdvancedDealerDashboard)
// ============================================

export type InsightPriority = 'High' | 'Medium' | 'Low';

// Internal type that matches the backend response for conversion funnel
interface ConversionFunnelApiResponse {
  id: string;
  dealerId: string;
  date: string;
  totalViews: number;
  totalContacts: number;
  testDriveRequests: number;
  actualSales: number;
  viewToContactRate: number;
  contactToTestDriveRate: number;
  testDriveToSaleRate: number;
  overallConversionRate: number;
  averageTimeToSale: number;
}

export interface MarketBenchmarkDto {
  id?: string;
  date?: string;
  vehicleCategory?: string;
  priceRange?: string;
  category: string;
  dealerValue: number;
  marketAverage: number;
  isAboveAverage: boolean;
  marketAveragePrice?: number;
  marketAverageDaysOnMarket?: number;
  marketAverageViews?: number;
  marketConversionRate?: number;
}

export interface DealerInsightDto {
  id: string;
  dealerId?: string;
  type: string;
  priority: InsightPriority;
  title: string;
  description?: string;
  actionRecommendation?: string;
  potentialImpact?: string | number;
  confidence?: number;
  isRead?: boolean;
  isActedUpon?: boolean;
  createdAt?: string;
  expiresAt?: string;
}

// Analytics object that comes nested in DashboardSummaryDto
export interface DealerAnalyticsData {
  id: string;
  dealerId: string;
  date: string;
  totalViews: number;
  uniqueViews: number;
  averageViewDuration: number;
  totalContacts: number;
  phoneCalls: number;
  whatsAppMessages: number;
  emailInquiries: number;
  testDriveRequests: number;
  actualSales: number;
  conversionRate: number;
  totalRevenue: number;
  averageVehiclePrice: number;
  revenuePerView: number;
  activeListings: number;
  averageDaysOnMarket: number;
  soldVehicles: number;
  createdAt: string;
  updatedAt: string;
}

export interface DashboardSummaryDto {
  dealerId: string;
  fromDate: string;
  toDate: string;
  analytics: DealerAnalyticsData;
  conversionFunnel?: ConversionFunnelApiResponse;
  benchmarks?: MarketBenchmarkDto[];
  insights?: DealerInsightDto[];
  viewsGrowth?: number;
  contactsGrowth?: number;
  salesGrowth?: number;
  revenueGrowth?: number;
}

export interface QuickStats {
  totalViews: number;
  viewsGrowth: number;
  totalContacts: number;
  contactsGrowth: number;
  actualSales: number;
  salesGrowth: number;
  totalRevenue: number;
  revenueGrowth: number;
}

// Frontend-friendly format for the component
export interface ConversionFunnelDto {
  views: number;
  inquiries: number;
  leads: number;
  closedSales: number;
  conversionRate: number;
  overallConversionRate: number;
}

export interface FunnelVisualization {
  steps: Array<{ label: string; value: number; rate?: number }>;
}

export interface TrendsDailyData {
  date: string;
  views: number;
  uniqueViews: number;
  contacts: number;
  sales: number;
  revenue: number;
}

export interface TrendsData {
  dealerId: string;
  periodDays: number;
  fromDate: string;
  toDate: string;
  dailyData: TrendsDailyData[];
}

export interface InsightsSummary {
  total: number;
  unread: number;
  highPriority: number;
}

export interface PerformanceComparison {
  periodDays: number;
  dealer: {
    conversionRate?: number;
    averageResponseTime?: number;
    customerSatisfactionScore?: number;
  };
  market: {
    conversionRate?: number;
    averageResponseTime?: number;
    customerSatisfactionScore?: number;
  };
}

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

export const ContactType = {
  Phone: 1,
  Email: 2,
  WhatsApp: 3,
  Website: 4,
  SocialMedia: 5,
} as const;

export type ContactType = (typeof ContactType)[keyof typeof ContactType];

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
        const token = localStorage.getItem('accessToken') || localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Add refresh token interceptor for automatic token refresh on 401
    addRefreshTokenInterceptor(this.api);
  }

  // ============================================
  // Sprint 12+ Advanced Dealer Analytics API
  // ============================================

  async getDashboardSummary(
    dealerId: string,
    fromDate?: Date,
    toDate?: Date
  ): Promise<DashboardSummaryDto> {
    const params: Record<string, string> = {};
    if (fromDate) params.fromDate = fromDate.toISOString();
    if (toDate) params.toDate = toDate.toISOString();
    const response = await this.api.get(`/api/dashboard/${dealerId}/summary`, {
      params,
    });
    return response.data;
  }

  async getQuickStats(dealerId: string): Promise<QuickStats> {
    const response = await this.api.get(`/api/dashboard/${dealerId}/quick-stats`);
    return response.data;
  }

  async getConversionFunnel(
    dealerId: string,
    fromDate?: Date,
    toDate?: Date
  ): Promise<ConversionFunnelDto> {
    const params: Record<string, string> = {};
    if (fromDate) params.fromDate = fromDate.toISOString();
    if (toDate) params.toDate = toDate.toISOString();
    const response = await this.api.get<ConversionFunnelApiResponse>(
      `/api/conversionfunnel/${dealerId}`,
      { params }
    );

    // Transform backend response to frontend-friendly format
    const data = response.data;
    return {
      views: data.totalViews ?? 0,
      inquiries: data.totalContacts ?? 0,
      leads: data.testDriveRequests ?? 0,
      closedSales: data.actualSales ?? 0,
      conversionRate: data.overallConversionRate ?? 0,
      overallConversionRate: data.overallConversionRate ?? 0,
    };
  }

  async getFunnelVisualization(
    dealerId: string,
    fromDate?: Date,
    toDate?: Date
  ): Promise<FunnelVisualization> {
    const params: Record<string, string> = {};
    if (fromDate) params.fromDate = fromDate.toISOString();
    if (toDate) params.toDate = toDate.toISOString();
    const response = await this.api.get(`/api/analytics/funnel/${dealerId}/visualization`, {
      params,
    });
    return response.data;
  }

  async getDealerInsights(dealerId: string): Promise<DealerInsightDto[]> {
    const response = await this.api.get(`/api/insights/${dealerId}`);
    return response.data;
  }

  async getInsightsSummary(dealerId: string): Promise<InsightsSummary> {
    const response = await this.api.get(`/api/analytics/insights/${dealerId}/summary`);
    return response.data;
  }

  async generateInsights(dealerId: string): Promise<void> {
    await this.api.post(`/api/insights/${dealerId}/generate`);
  }

  async markInsightsAsRead(dealerId: string, insightIds: string[]): Promise<void> {
    await this.api.put(`/api/insights/${dealerId}/mark-read`, { insightIds });
  }

  async markInsightAsActedUpon(insightId: string): Promise<void> {
    await this.api.post(`/api/analytics/insights/${insightId}/acted`);
  }

  // Convenience methods (used by older tests / UI buttons)
  async markInsightAsRead(insightId: string): Promise<boolean> {
    const response = await this.api.post(`/api/analytics/insights/${insightId}/read`);
    return response.data;
  }

  async dismissInsight(insightId: string): Promise<boolean> {
    const response = await this.api.delete(`/api/analytics/insights/${insightId}`);
    return response.data;
  }

  async getMarketBenchmarks(): Promise<MarketBenchmarkDto[]> {
    const response = await this.api.get('/api/benchmark');
    return response.data;
  }

  async getPerformanceComparison(
    dealerId: string,
    periodDays: number
  ): Promise<PerformanceComparison> {
    const response = await this.api.get(`/api/analytics/performance/${dealerId}`, {
      params: { periodDays },
    });
    return response.data;
  }

  async recalculateAnalytics(dealerId: string, fromDate: Date, toDate: Date): Promise<void> {
    await this.api.post(`/api/analytics/dashboard/${dealerId}/recalculate`, {
      fromDate: fromDate.toISOString(),
      toDate: toDate.toISOString(),
    });
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
   * Format currency (USD, no decimals)
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  /**
   * Map insight priority to Tailwind classes
   */
  getPriorityColor(priority: InsightPriority): string {
    switch (priority) {
      case 'High':
        return 'text-red-600 bg-red-50';
      case 'Medium':
        return 'text-yellow-600 bg-yellow-50';
      case 'Low':
        return 'text-green-600 bg-green-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  }

  /**
   * Simple icon mapping for priorities
   */
  getPriorityIcon(priority: InsightPriority): string {
    switch (priority) {
      case 'High':
        return '🚨';
      case 'Medium':
        return '⚠️';
      case 'Low':
        return '💡';
      default:
        return 'ℹ️';
    }
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
   * Get trends data for chart
   */
  async getTrends(dealerId: string, periodDays: number = 30): Promise<TrendsData> {
    const response = await this.api.get(`/api/dashboard/${dealerId}/trends`, {
      params: { periodDays },
    });
    return response.data;
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

  // ============================================
  // ADVANCED ANALYTICS - OVERVIEW API
  // ============================================

  async getAnalyticsOverview(dealerId: string): Promise<AnalyticsOverviewDto> {
    const response = await this.api.get(`/api/analytics/overview/${dealerId}`);
    return response.data;
  }

  async getKpis(dealerId: string): Promise<KpiSummaryDto> {
    const response = await this.api.get(`/api/analytics/overview/${dealerId}/kpis`);
    return response.data;
  }

  async getSnapshot(dealerId: string, date?: string): Promise<DealerSnapshotDto> {
    const params = date ? { date } : {};
    const response = await this.api.get(`/api/analytics/overview/${dealerId}/snapshot`, { params });
    return response.data;
  }

  async getSnapshotComparison(
    dealerId: string,
    currentDate: string,
    previousDate: string
  ): Promise<SnapshotComparisonDto> {
    const response = await this.api.get(`/api/analytics/overview/${dealerId}/comparison`, {
      params: { currentDate, previousDate },
    });
    return response.data;
  }

  async getMetricTrend(
    dealerId: string,
    metricType: string,
    days: number = 30
  ): Promise<TrendDataPointDto[]> {
    const response = await this.api.get(
      `/api/analytics/overview/${dealerId}/trends/${metricType}`,
      {
        params: { days },
      }
    );
    return response.data;
  }

  async getEngagementStats(dealerId: string): Promise<EngagementStatsDto> {
    const response = await this.api.get(`/api/analytics/overview/${dealerId}/engagement`);
    return response.data;
  }

  // ============================================
  // ADVANCED ANALYTICS - INVENTORY API
  // ============================================

  async getInventoryStats(dealerId: string): Promise<InventoryStatsDto> {
    const response = await this.api.get(`/api/analytics/inventory/${dealerId}/stats`);
    return response.data;
  }

  async getInventoryAging(dealerId: string): Promise<InventoryAgingDto> {
    const response = await this.api.get(`/api/analytics/inventory/${dealerId}/aging`);
    return response.data;
  }

  async getInventoryTurnover(dealerId: string, months: number = 6): Promise<InventoryTurnoverDto> {
    const response = await this.api.get(`/api/analytics/inventory/${dealerId}/turnover`, {
      params: { months },
    });
    return response.data;
  }

  async getVehiclePerformance(
    dealerId: string,
    orderBy: string = 'views',
    top: number = 10
  ): Promise<VehiclePerformanceDto[]> {
    const response = await this.api.get(`/api/analytics/inventory/${dealerId}/performance`, {
      params: { orderBy, top },
    });
    return response.data;
  }

  async getLowPerformers(
    dealerId: string,
    threshold: number = 50
  ): Promise<VehiclePerformanceDto[]> {
    const response = await this.api.get(`/api/analytics/inventory/${dealerId}/low-performers`, {
      params: { threshold },
    });
    return response.data;
  }

  // ============================================
  // ADVANCED ANALYTICS - ALERTS API
  // ============================================

  async getActiveAlerts(dealerId: string): Promise<DealerAlertDto[]> {
    const response = await this.api.get(`/api/analytics/alerts/${dealerId}`);
    return response.data;
  }

  async getUnreadAlertCount(dealerId: string): Promise<number> {
    const response = await this.api.get(`/api/analytics/alerts/${dealerId}/unread-count`);
    return response.data;
  }

  async getAlertsByType(dealerId: string, alertType: string): Promise<DealerAlertDto[]> {
    const response = await this.api.get(`/api/analytics/alerts/${dealerId}/by-type/${alertType}`);
    return response.data;
  }

  async getAlertsBySeverity(dealerId: string, severity: string): Promise<DealerAlertDto[]> {
    const response = await this.api.get(
      `/api/analytics/alerts/${dealerId}/by-severity/${severity}`
    );
    return response.data;
  }

  async markAlertAsRead(alertId: string): Promise<void> {
    await this.api.post(`/api/analytics/alerts/${alertId}/read`);
  }

  async markAllAlertsAsRead(dealerId: string): Promise<void> {
    await this.api.post(`/api/analytics/alerts/${dealerId}/read-all`);
  }

  async dismissAlert(alertId: string): Promise<void> {
    await this.api.delete(`/api/analytics/alerts/${alertId}`);
  }

  async dismissAlertsByType(dealerId: string, alertType: string): Promise<void> {
    await this.api.delete(`/api/analytics/alerts/${dealerId}/dismiss-by-type/${alertType}`);
  }

  async markAlertAsActedUpon(alertId: string): Promise<void> {
    await this.api.post(`/api/analytics/alerts/${alertId}/acted-upon`);
  }

  async createAlert(request: CreateAlertRequest): Promise<DealerAlertDto> {
    const response = await this.api.post('/api/analytics/alerts', request);
    return response.data;
  }

  // ============================================
  // ADVANCED ANALYTICS - REPORTS API
  // ============================================

  async getDailyReport(dealerId: string, date?: string): Promise<AnalyticsReportDto> {
    const params = date ? { date } : {};
    const response = await this.api.get(`/api/analytics/reports/${dealerId}/daily`, { params });
    return response.data;
  }

  async getWeeklyReport(dealerId: string, weekStart?: string): Promise<AnalyticsReportDto> {
    const params = weekStart ? { weekStart } : {};
    const response = await this.api.get(`/api/analytics/reports/${dealerId}/weekly`, { params });
    return response.data;
  }

  async getMonthlyReport(
    dealerId: string,
    year?: number,
    month?: number
  ): Promise<AnalyticsReportDto> {
    const params: Record<string, any> = {};
    if (year) params.year = year;
    if (month) params.month = month;
    const response = await this.api.get(`/api/analytics/reports/${dealerId}/monthly`, { params });
    return response.data;
  }

  async getCustomReport(
    dealerId: string,
    fromDate: string,
    toDate: string
  ): Promise<AnalyticsReportDto> {
    const response = await this.api.get(`/api/analytics/reports/${dealerId}/custom`, {
      params: { fromDate, toDate },
    });
    return response.data;
  }

  async exportReport(
    dealerId: string,
    format: 'pdf' | 'excel' | 'csv',
    fromDate: string,
    toDate: string
  ): Promise<Blob> {
    const response = await this.api.get(`/api/analytics/reports/${dealerId}/export/${format}`, {
      params: { fromDate, toDate },
      responseType: 'blob',
    });
    return response.data;
  }

  // ============================================
  // LEAD FUNNEL API
  // ============================================

  async getLeadFunnelMetrics(
    dealerId: string,
    fromDate?: string,
    toDate?: string
  ): Promise<LeadFunnelDto> {
    const params: Record<string, string> = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    const response = await this.api.get(`/api/analytics/funnel/${dealerId}`, { params });
    return response.data;
  }

  async getLeadFunnelComparison(
    dealerId: string,
    currentPeriod: { start: string; end: string },
    previousPeriod: { start: string; end: string }
  ): Promise<FunnelComparisonDto> {
    const response = await this.api.get(`/api/analytics/funnel/${dealerId}/comparison`, {
      params: {
        currentStart: currentPeriod.start,
        currentEnd: currentPeriod.end,
        previousStart: previousPeriod.start,
        previousEnd: previousPeriod.end,
      },
    });
    return response.data;
  }

  async getConversionRates(dealerId: string): Promise<ConversionRatesDto> {
    const response = await this.api.get(`/api/analytics/funnel/${dealerId}/conversion-rates`);
    return response.data;
  }

  async getLeadsBySource(dealerId: string): Promise<LeadSourceDto[]> {
    const response = await this.api.get(`/api/analytics/funnel/${dealerId}/by-source`);
    return response.data;
  }

  // ============================================
  // BENCHMARKS API
  // ============================================

  async getDealerBenchmark(dealerId: string): Promise<DealerBenchmarkDto> {
    const response = await this.api.get(`/api/analytics/benchmark/${dealerId}`);
    return response.data;
  }

  async getMarketComparison(dealerId: string): Promise<MarketComparisonDto> {
    const response = await this.api.get(`/api/analytics/benchmark/${dealerId}/market`);
    return response.data;
  }

  async getDealerRankings(dealerId: string): Promise<RankingsDto> {
    const response = await this.api.get(`/api/analytics/benchmark/${dealerId}/rankings`);
    return response.data;
  }

  async getDealerTier(dealerId: string): Promise<DealerTierDto> {
    const response = await this.api.get(`/api/analytics/benchmark/${dealerId}/tier`);
    return response.data;
  }
}

// ============================================
// ADVANCED ANALYTICS TYPES
// ============================================

export interface AnalyticsOverviewDto {
  dealerId: string;
  period: string;
  kpis: KpiSummaryDto;
  snapshot: DealerSnapshotDto;
  trends: TrendDataPointDto[];
  funnel: LeadFunnelDto;
  benchmark: DealerBenchmarkDto;
  activeAlerts: DealerAlertDto[];
}

export interface KpiSummaryDto {
  totalViews: number;
  viewsChange: number;
  totalContacts: number;
  contactsChange: number;
  conversionRate: number;
  conversionChange: number;
  avgDaysOnMarket: number;
  daysChange: number;
  revenue: number;
  revenueChange: number;
  activeListings: number;
  listingsChange: number;
}

export interface DealerSnapshotDto {
  id: string;
  dealerId: string;
  date: string;
  activeVehicles: number;
  totalViews: number;
  uniqueViews: number;
  totalContacts: number;
  newLeads: number;
  qualifiedLeads: number;
  conversions: number;
  revenue: number;
  avgDaysOnMarket: number;
  ctr: number;
  contactRate: number;
  turnoverRate: number;
  agingRate: number;
}

export interface SnapshotComparisonDto {
  current: DealerSnapshotDto;
  previous: DealerSnapshotDto;
  changes: Record<string, number>;
}

export interface TrendDataPointDto {
  date: string;
  value: number;
  label?: string;
}

export interface EngagementStatsDto {
  totalViews: number;
  uniqueViews: number;
  avgViewDuration: number;
  totalFavorites: number;
  totalShares: number;
  ctr: number;
  bounceRate: number;
}

export interface InventoryStatsDto {
  totalVehicles: number;
  activeVehicles: number;
  soldVehicles: number;
  pendingVehicles: number;
  totalValue: number;
  avgPrice: number;
  avgDaysOnMarket: number;
}

export interface InventoryAgingDto {
  dealerId: string;
  date: string;
  buckets: AgingBucketDto[];
  avgDaysOnMarket: number;
  agingRate: number;
  atRiskValue: number;
  atRiskCount: number;
}

export interface AgingBucketDto {
  minDays: number;
  maxDays: number;
  label: string;
  count: number;
  value: number;
  percentage: number;
}

export interface InventoryTurnoverDto {
  dealerId: string;
  period: string;
  turnoverRate: number;
  avgDaysToSell: number;
  soldCount: number;
  newListingsCount: number;
  monthlyData: TurnoverMonthDto[];
}

export interface TurnoverMonthDto {
  month: string;
  sold: number;
  listed: number;
  turnoverRate: number;
}

export interface VehiclePerformanceDto {
  vehicleId: string;
  title: string;
  make: string;
  model: string;
  year: number;
  price: number;
  daysOnMarket: number;
  views: number;
  contacts: number;
  favorites: number;
  engagementScore: number;
  performanceScore: number;
  status: string;
}

export interface DealerAlertDto {
  id: string;
  dealerId: string;
  type: string;
  severity: 'info' | 'warning' | 'critical';
  status: 'unread' | 'read' | 'acted' | 'dismissed';
  title: string;
  message: string;
  vehicleId?: string;
  vehicleTitle?: string;
  actionUrl?: string;
  actionLabel?: string;
  createdAt: string;
  readAt?: string;
  expiresAt?: string;
  metadata?: Record<string, any>;
}

export interface CreateAlertRequest {
  dealerId: string;
  type: string;
  severity: 'info' | 'warning' | 'critical';
  title: string;
  message: string;
  vehicleId?: string;
  actionUrl?: string;
  actionLabel?: string;
  expiresAt?: string;
  metadata?: Record<string, any>;
}

export interface AnalyticsReportDto {
  dealerId: string;
  period: string;
  dateRange: { start: string; end: string };
  generatedAt: string;
  metrics: ReportMetricsDto;
  charts: ReportChartsDto;
  highlights: string[];
  recommendations: string[];
}

export interface ReportMetricsDto {
  inventory: ReportMetricDto[];
  engagement: ReportMetricDto[];
  leads: ReportMetricDto[];
  revenue: ReportMetricDto[];
}

export interface ReportMetricDto {
  label: string;
  current: number;
  previous: number;
  change: number;
  changePercent: number;
  trend: 'up' | 'down' | 'stable';
  format: 'number' | 'currency' | 'percentage' | 'time';
}

export interface ReportChartsDto {
  viewsTrend: { labels: string[]; values: number[] };
  leadsTrend: { labels: string[]; values: number[] };
  conversionsTrend: { labels: string[]; values: number[] };
}

export interface LeadFunnelDto {
  dealerId: string;
  period: string;
  stages: FunnelStageDto[];
  totalImpressions: number;
  totalViews: number;
  totalContacts: number;
  qualifiedLeads: number;
  negotiations: number;
  conversions: number;
  overallConversionRate: number;
}

export interface FunnelStageDto {
  id: string;
  name: string;
  value: number;
  previousValue?: number;
  conversionRate: number;
  dropoffRate: number;
}

export interface FunnelComparisonDto {
  current: LeadFunnelDto;
  previous: LeadFunnelDto;
  changes: Record<string, number>;
}

export interface ConversionRatesDto {
  impressionToView: number;
  viewToContact: number;
  contactToQualified: number;
  qualifiedToNegotiation: number;
  negotiationToConversion: number;
  overallConversion: number;
}

export interface LeadSourceDto {
  source: string;
  count: number;
  percentage: number;
  conversionRate: number;
}

export interface DealerBenchmarkDto {
  dealerId: string;
  date: string;
  tier: 'bronze' | 'silver' | 'gold' | 'platinum' | 'diamond';
  overallScore: number;
  viewsPercentile: number;
  contactsPercentile: number;
  conversionPercentile: number;
  responseTimePercentile: number;
  rankings: RankingsDto;
}

export interface MarketComparisonDto {
  dealerId: string;
  categories: MarketCategoryDto[];
}

export interface MarketCategoryDto {
  category: string;
  dealerValue: number;
  marketAverage: number;
  percentile: number;
  isAboveAverage: boolean;
}

export interface RankingsDto {
  overallRank: number;
  totalDealers: number;
  viewsRank: number;
  contactsRank: number;
  conversionRank: number;
  revenueRank: number;
}

export interface DealerTierDto {
  dealerId: string;
  tier: 'bronze' | 'silver' | 'gold' | 'platinum' | 'diamond';
  score: number;
  nextTier?: string;
  pointsToNextTier?: number;
}

// Export singleton instance
export const dealerAnalyticsService = new DealerAnalyticsService();
