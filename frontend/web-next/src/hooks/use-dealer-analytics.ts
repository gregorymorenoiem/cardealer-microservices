/**
 * React Query hooks for DealerAnalyticsService
 *
 * Provides hooks for sales analytics, inventory analytics,
 * vehicle performance, reports, and imports.
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  dealerAnalyticsService,
  type AnalyticsOverview,
  type KpiSummary,
  type DealerSnapshot,
  type SnapshotComparison,
  type TrendDataPoint,
  type EngagementMetrics,
  type VehiclePerformance,
  type InventoryAging,
  type AnalyticsReport,
  type CustomReportRequest,
  type ImportResult,
  type ImportHistoryItem,
} from '@/services/dealer-analytics';

// =============================================================================
// QUERY KEYS
// =============================================================================

const analyticsKeys = {
  all: ['dealer-analytics'] as const,
  overview: (dealerId: string) => [...analyticsKeys.all, 'overview', dealerId] as const,
  kpis: (dealerId: string) => [...analyticsKeys.all, 'kpis', dealerId] as const,
  snapshot: (dealerId: string) => [...analyticsKeys.all, 'snapshot', dealerId] as const,
  comparison: (dealerId: string) => [...analyticsKeys.all, 'comparison', dealerId] as const,
  trends: (dealerId: string, metric: string) =>
    [...analyticsKeys.all, 'trends', dealerId, metric] as const,
  engagement: (dealerId: string) => [...analyticsKeys.all, 'engagement', dealerId] as const,
  inventoryStats: (dealerId: string) =>
    [...analyticsKeys.all, 'inventory-stats', dealerId] as const,
  inventoryAging: (dealerId: string) =>
    [...analyticsKeys.all, 'inventory-aging', dealerId] as const,
  inventoryPerformance: (dealerId: string) =>
    [...analyticsKeys.all, 'inventory-performance', dealerId] as const,
  inventoryLowPerformers: (dealerId: string) =>
    [...analyticsKeys.all, 'inventory-low-performers', dealerId] as const,
  reportDaily: (dealerId: string) => [...analyticsKeys.all, 'report-daily', dealerId] as const,
  reportWeekly: (dealerId: string) => [...analyticsKeys.all, 'report-weekly', dealerId] as const,
  reportMonthly: (dealerId: string) => [...analyticsKeys.all, 'report-monthly', dealerId] as const,
  importHistory: (dealerId: string) => [...analyticsKeys.all, 'import-history', dealerId] as const,
};

// =============================================================================
// OVERVIEW & KPI HOOKS
// =============================================================================

export function useAnalyticsOverview(
  dealerId: string,
  params?: { fromDate?: string; toDate?: string }
) {
  return useQuery<AnalyticsOverview>({
    queryKey: [...analyticsKeys.overview(dealerId), params],
    queryFn: () => dealerAnalyticsService.getOverview(dealerId, params),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useKpis(dealerId: string, params?: { fromDate?: string; toDate?: string }) {
  return useQuery<KpiSummary>({
    queryKey: [...analyticsKeys.kpis(dealerId), params],
    queryFn: () => dealerAnalyticsService.getKpis(dealerId, params),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useSnapshot(dealerId: string) {
  return useQuery<DealerSnapshot>({
    queryKey: analyticsKeys.snapshot(dealerId),
    queryFn: () => dealerAnalyticsService.getSnapshot(dealerId),
    enabled: !!dealerId,
    staleTime: 10 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useComparison(dealerId: string) {
  return useQuery<SnapshotComparison>({
    queryKey: analyticsKeys.comparison(dealerId),
    queryFn: () => dealerAnalyticsService.getComparison(dealerId),
    enabled: !!dealerId,
    staleTime: 10 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useTrends(
  dealerId: string,
  metricType: 'views' | 'contacts' | 'sales' | 'revenue' | 'conversion' | 'leads',
  params?: { fromDate?: string; toDate?: string }
) {
  return useQuery<TrendDataPoint[]>({
    queryKey: [...analyticsKeys.trends(dealerId, metricType), params],
    queryFn: () => dealerAnalyticsService.getTrends(dealerId, metricType, params),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useEngagement(dealerId: string, params?: { fromDate?: string; toDate?: string }) {
  return useQuery<EngagementMetrics>({
    queryKey: [...analyticsKeys.engagement(dealerId), params],
    queryFn: () => dealerAnalyticsService.getEngagement(dealerId, params),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

// =============================================================================
// INVENTORY ANALYTICS HOOKS
// =============================================================================

export function useInventoryStats(dealerId: string) {
  return useQuery<DealerSnapshot>({
    queryKey: analyticsKeys.inventoryStats(dealerId),
    queryFn: () => dealerAnalyticsService.getInventoryStats(dealerId),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useInventoryAging(dealerId: string) {
  return useQuery<InventoryAging>({
    queryKey: analyticsKeys.inventoryAging(dealerId),
    queryFn: () => dealerAnalyticsService.getInventoryAging(dealerId),
    enabled: !!dealerId,
    staleTime: 10 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useInventoryPerformance(
  dealerId: string,
  params?: { sortBy?: string; limit?: number; ascending?: boolean }
) {
  return useQuery<VehiclePerformance[]>({
    queryKey: [...analyticsKeys.inventoryPerformance(dealerId), params],
    queryFn: () => dealerAnalyticsService.getInventoryPerformance(dealerId, params),
    enabled: !!dealerId,
    staleTime: 5 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useInventoryLowPerformers(dealerId: string) {
  return useQuery<VehiclePerformance[]>({
    queryKey: analyticsKeys.inventoryLowPerformers(dealerId),
    queryFn: () => dealerAnalyticsService.getInventoryLowPerformers(dealerId),
    enabled: !!dealerId,
    staleTime: 10 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

// =============================================================================
// REPORTS HOOKS
// =============================================================================

export function useDailyReport(dealerId: string) {
  return useQuery<AnalyticsReport>({
    queryKey: analyticsKeys.reportDaily(dealerId),
    queryFn: () => dealerAnalyticsService.getDailyReport(dealerId),
    enabled: !!dealerId,
    staleTime: 30 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useWeeklyReport(dealerId: string) {
  return useQuery<AnalyticsReport>({
    queryKey: analyticsKeys.reportWeekly(dealerId),
    queryFn: () => dealerAnalyticsService.getWeeklyReport(dealerId),
    enabled: !!dealerId,
    staleTime: 30 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useMonthlyReport(dealerId: string) {
  return useQuery<AnalyticsReport>({
    queryKey: analyticsKeys.reportMonthly(dealerId),
    queryFn: () => dealerAnalyticsService.getMonthlyReport(dealerId),
    enabled: !!dealerId,
    staleTime: 30 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useCustomReport(dealerId: string) {
  return useMutation({
    mutationFn: (request: CustomReportRequest) =>
      dealerAnalyticsService.generateCustomReport(dealerId, request),
  });
}

export function useExportReport(dealerId: string) {
  return useMutation({
    mutationFn: (format: 'pdf' | 'excel' | 'csv') =>
      dealerAnalyticsService.exportReport(dealerId, format),
    onSuccess: (data, format) => {
      const blob = new Blob([data]);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `reporte-dealer.${format === 'excel' ? 'xlsx' : format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    },
  });
}

// =============================================================================
// IMPORT HOOKS
// =============================================================================

export function useImportVehicles(dealerId: string) {
  const queryClient = useQueryClient();
  return useMutation<ImportResult, Error, File>({
    mutationFn: (file: File) => dealerAnalyticsService.importVehicles(dealerId, file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: analyticsKeys.importHistory(dealerId) });
      queryClient.invalidateQueries({ queryKey: ['vehicles'] });
    },
  });
}

export function useImportHistory(dealerId: string) {
  return useQuery<ImportHistoryItem[]>({
    queryKey: analyticsKeys.importHistory(dealerId),
    queryFn: () => dealerAnalyticsService.getImportHistory(dealerId),
    enabled: !!dealerId,
    staleTime: 2 * 60 * 1000,
    retry: 2,
    refetchOnWindowFocus: false,
  });
}

export function useDownloadImportTemplate() {
  return useMutation({
    mutationFn: (format: 'csv' | 'xlsx') => dealerAnalyticsService.downloadImportTemplate(format),
    onSuccess: (data, format) => {
      const blob = new Blob([data]);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `plantilla-importacion.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    },
  });
}
