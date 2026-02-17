/**
 * DealerAnalyticsService API Client
 *
 * Client for the DealerAnalyticsService backend microservice.
 * Provides sales analytics, inventory analytics, vehicle performance,
 * reports, and export functionality.
 */

import { apiClient } from '@/lib/api-client';

// =============================================================================
// TYPES — Overview & KPIs
// =============================================================================

export interface KpiSummary {
  totalViews: number;
  viewsChange: number;
  totalContacts: number;
  contactsChange: number;
  totalLeads: number;
  leadsChange: number;
  totalSales: number;
  salesChange: number;
  totalRevenue: number;
  revenueChange: number;
  conversionRate: number;
  conversionChange: number;
  avgResponseTime: number;
  responseTimeChange: number;
  activeListings: number;
  inventoryValue: number;
}

export interface TrendDataPoint {
  date: string;
  value: number;
  label: string;
}

export interface DealerSnapshot {
  id: string;
  dealerId: string;
  snapshotDate: string;
  totalVehicles: number;
  activeVehicles: number;
  soldVehicles: number;
  totalInventoryValue: number;
  avgVehiclePrice: number;
  avgDaysOnMarket: number;
  vehiclesOver60Days: number;
  totalViews: number;
  uniqueViews: number;
  totalContacts: number;
  totalFavorites: number;
  searchImpressions: number;
  newLeads: number;
  qualifiedLeads: number;
  convertedLeads: number;
  leadConversionRate: number;
  totalRevenue: number;
  avgTransactionValue: number;
  clickThroughRate: number;
  contactRate: number;
  favoriteRate: number;
  inventoryTurnoverRate: number;
  agingRate: number;
}

export interface SnapshotComparison {
  current: DealerSnapshot;
  previous: DealerSnapshot | null;
  viewsChange: number;
  contactsChange: number;
  leadsChange: number;
  salesChange: number;
  revenueChange: number;
  conversionRateChange: number;
  inventoryValueChange: number;
}

export interface AnalyticsOverview {
  dealerId: string;
  fromDate: string;
  toDate: string;
  kpis: KpiSummary;
  currentSnapshot: DealerSnapshot;
  comparison: SnapshotComparison;
  viewsTrend: TrendDataPoint[];
  contactsTrend: TrendDataPoint[];
  salesTrend: TrendDataPoint[];
  revenueTrend: TrendDataPoint[];
  topPerformers: VehiclePerformance[];
  lastUpdated: string;
}

// =============================================================================
// TYPES — Vehicle Performance
// =============================================================================

export interface VehiclePerformance {
  id: string;
  vehicleId: string;
  dealerId: string;
  vehicleTitle: string | null;
  vehicleMake: string | null;
  vehicleModel: string | null;
  vehicleYear: number | null;
  vehiclePrice: number | null;
  vehicleThumbnailUrl: string | null;
  views: number;
  uniqueViews: number;
  contacts: number;
  favorites: number;
  searchImpressions: number;
  searchClicks: number;
  clickThroughRate: number;
  contactRate: number;
  favoriteRate: number;
  engagementScore: number;
  performanceScore: number;
  daysOnMarket: number;
  isSold: boolean;
  rank: number | null;
  performanceLabel: string | null;
}

// =============================================================================
// TYPES — Inventory Analytics
// =============================================================================

export interface AgingBucket {
  label: string;
  count: number;
  value: number;
  color: string;
  percentage: number;
}

export interface InventoryAging {
  dealerId: string;
  date: string;
  buckets: AgingBucket[];
  totalVehicles: number;
  totalValue: number;
  averageDaysOnMarket: number;
  medianDaysOnMarket: number;
  percentFresh: number;
  percentAging: number;
  atRiskCount: number;
  atRiskValue: number;
}

// =============================================================================
// TYPES — Engagement
// =============================================================================

export interface EngagementMetrics {
  dealerId: string;
  totalViews: number;
  uniqueViews: number;
  averageViewDuration: number;
  totalContacts: number;
  phoneCallClicks: number;
  whatsAppClicks: number;
  emailClicks: number;
  totalFavorites: number;
  contactRate: number;
  favoriteRate: number;
  bounceRate: number;
  deviceBreakdown: { deviceType: string; count: number; percentage: number }[];
  topCities: { city: string; count: number; percentage: number }[];
  hourlyDistribution: { hour: number; views: number; contacts: number }[];
  topReferrers: { source: string; count: number; percentage: number }[];
}

// =============================================================================
// TYPES — Reports
// =============================================================================

export interface AnalyticsReport {
  dealerId: string;
  dealerName: string;
  reportType: string;
  fromDate: string;
  toDate: string;
  generatedAt: string;
  summary: KpiSummary;
  vehiclePerformance: VehiclePerformance[];
  keyInsights: string[];
  recommendations: string[];
}

export interface CustomReportRequest {
  fromDate: string;
  toDate: string;
  includeSections: string[];
}

// =============================================================================
// TYPES — Lead Funnel
// =============================================================================

export interface FunnelStage {
  name: string;
  value: number;
  conversionRate: number;
  dropOffRate: number;
  color: string;
  percentage: number;
}

export interface LeadFunnel {
  dealerId: string;
  periodStart: string;
  periodEnd: string;
  impressions: number;
  views: number;
  contacts: number;
  qualified: number;
  negotiation: number;
  converted: number;
  impressionsToViews: number;
  viewsToContacts: number;
  contactsToQualified: number;
  qualifiedToConverted: number;
  overallConversion: number;
  stages: FunnelStage[];
  attributedRevenue: number;
  avgDealValue: number;
  conversionChange: number | null;
}

// =============================================================================
// TYPES — Vehicle Import
// =============================================================================

export interface ImportResult {
  id: string;
  dealerId: string;
  filename: string;
  totalRecords: number;
  successful: number;
  failed: number;
  status: 'processing' | 'completed' | 'failed';
  errors: ImportError[];
  createdAt: string;
  completedAt: string | null;
}

export interface ImportError {
  row: number;
  field: string;
  message: string;
}

export interface ImportHistoryItem {
  id: string;
  filename: string;
  date: string;
  totalRecords: number;
  successful: number;
  failed: number;
  status: string;
}

// =============================================================================
// API CLIENT
// =============================================================================

export const dealerAnalyticsService = {
  // --- Overview ---
  getOverview: async (dealerId: string, params?: { fromDate?: string; toDate?: string }) => {
    const response = await apiClient.get<AnalyticsOverview>(
      `/api/dealer-analytics/${dealerId}/overview`,
      { params }
    );
    return response.data;
  },

  getKpis: async (dealerId: string, params?: { fromDate?: string; toDate?: string }) => {
    const response = await apiClient.get<KpiSummary>(`/api/dealer-analytics/${dealerId}/kpis`, {
      params,
    });
    return response.data;
  },

  getSnapshot: async (dealerId: string) => {
    const response = await apiClient.get<DealerSnapshot>(
      `/api/dealer-analytics/${dealerId}/snapshot`
    );
    return response.data;
  },

  getComparison: async (dealerId: string) => {
    const response = await apiClient.get<SnapshotComparison>(
      `/api/dealer-analytics/${dealerId}/comparison`
    );
    return response.data;
  },

  getTrends: async (
    dealerId: string,
    metricType: 'views' | 'contacts' | 'sales' | 'revenue' | 'conversion' | 'leads',
    params?: { fromDate?: string; toDate?: string }
  ) => {
    const response = await apiClient.get<TrendDataPoint[]>(
      `/api/dealer-analytics/${dealerId}/trends/${metricType}`,
      { params }
    );
    return response.data;
  },

  getEngagement: async (dealerId: string, params?: { fromDate?: string; toDate?: string }) => {
    const response = await apiClient.get<EngagementMetrics>(
      `/api/dealer-analytics/${dealerId}/engagement`,
      { params }
    );
    return response.data;
  },

  // --- Inventory Analytics ---
  getInventoryStats: async (dealerId: string) => {
    const response = await apiClient.get<DealerSnapshot>(
      `/api/dealer-analytics/inventory/${dealerId}/stats`
    );
    return response.data;
  },

  getInventoryAging: async (dealerId: string) => {
    const response = await apiClient.get<InventoryAging>(
      `/api/dealer-analytics/inventory/${dealerId}/aging`
    );
    return response.data;
  },

  getInventoryPerformance: async (
    dealerId: string,
    params?: { sortBy?: string; limit?: number; ascending?: boolean }
  ) => {
    const response = await apiClient.get<VehiclePerformance[]>(
      `/api/dealer-analytics/inventory/${dealerId}/performance`,
      { params }
    );
    return response.data;
  },

  getInventoryLowPerformers: async (dealerId: string) => {
    const response = await apiClient.get<VehiclePerformance[]>(
      `/api/dealer-analytics/inventory/${dealerId}/low-performers`
    );
    return response.data;
  },

  // --- Reports ---
  getDailyReport: async (dealerId: string) => {
    const response = await apiClient.get<AnalyticsReport>(
      `/api/dealer-analytics/reports/${dealerId}/daily`
    );
    return response.data;
  },

  getWeeklyReport: async (dealerId: string) => {
    const response = await apiClient.get<AnalyticsReport>(
      `/api/dealer-analytics/reports/${dealerId}/weekly`
    );
    return response.data;
  },

  getMonthlyReport: async (dealerId: string) => {
    const response = await apiClient.get<AnalyticsReport>(
      `/api/dealer-analytics/reports/${dealerId}/monthly`
    );
    return response.data;
  },

  generateCustomReport: async (dealerId: string, request: CustomReportRequest) => {
    const response = await apiClient.post<AnalyticsReport>(
      `/api/dealer-analytics/reports/${dealerId}/custom`,
      request
    );
    return response.data;
  },

  exportReport: async (dealerId: string, format: 'pdf' | 'excel' | 'csv') => {
    const response = await apiClient.get(
      `/api/dealer-analytics/reports/${dealerId}/export/${format}`,
      { responseType: 'blob' }
    );
    return response.data;
  },

  // --- Vehicle Import ---
  importVehicles: async (dealerId: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('dealerId', dealerId);
    const response = await apiClient.post<ImportResult>('/api/vehicles/import', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 120000,
    });
    return response.data;
  },

  getImportHistory: async (dealerId: string) => {
    const response = await apiClient.get<ImportHistoryItem[]>(
      `/api/vehicles/import/history/${dealerId}`
    );
    return response.data;
  },

  downloadImportTemplate: async (format: 'csv' | 'xlsx' = 'csv') => {
    const response = await apiClient.get(`/api/vehicles/import/template`, {
      params: { format },
      responseType: 'blob',
    });
    return response.data;
  },
};
