import { useState, useEffect, useCallback } from 'react';
import {
  DashboardSummaryDto,
  ConversionFunnelDto,
  MarketBenchmarkDto,
  DealerInsightDto,
  QuickStats,
  PerformanceComparison,
  FunnelVisualization,
  InsightsSummary,
  dealerAnalyticsService,
} from '../services/dealerAnalyticsService';

export interface UseDealerAnalyticsProps {
  dealerId: string;
  fromDate?: Date;
  toDate?: Date;
  autoRefresh?: boolean;
  refreshInterval?: number; // in milliseconds
}

export interface UseDealerAnalyticsReturn {
  // Data
  dashboardSummary: DashboardSummaryDto | null;
  quickStats: QuickStats | null;
  conversionFunnel: ConversionFunnelDto | null;
  funnelVisualization: FunnelVisualization | null;
  benchmarks: MarketBenchmarkDto[];
  insights: DealerInsightDto[];
  insightsSummary: InsightsSummary | null;
  performanceComparison: PerformanceComparison | null;

  // Loading states
  isLoading: boolean;
  isLoadingDashboard: boolean;
  isLoadingFunnel: boolean;
  isLoadingInsights: boolean;
  isLoadingBenchmarks: boolean;

  // Error states
  error: string | null;

  // Actions
  refreshData: () => Promise<void>;
  refreshDashboard: () => Promise<void>;
  refreshFunnel: () => Promise<void>;
  refreshInsights: () => Promise<void>;
  refreshBenchmarks: () => Promise<void>;
  generateNewInsights: () => Promise<void>;
  markInsightsAsRead: (insightIds: string[]) => Promise<void>;
  markInsightAsActedUpon: (insightId: string) => Promise<void>;
  recalculateAnalytics: () => Promise<void>;
}

export const useDealerAnalytics = ({
  dealerId,
  fromDate,
  toDate,
  autoRefresh = false,
  refreshInterval = 60000, // 1 minuto por defecto
}: UseDealerAnalyticsProps): UseDealerAnalyticsReturn => {
  // State
  const [dashboardSummary, setDashboardSummary] = useState<DashboardSummaryDto | null>(null);
  const [quickStats, setQuickStats] = useState<QuickStats | null>(null);
  const [conversionFunnel, setConversionFunnel] = useState<ConversionFunnelDto | null>(null);
  const [funnelVisualization, setFunnelVisualization] = useState<FunnelVisualization | null>(null);
  const [benchmarks, setBenchmarks] = useState<MarketBenchmarkDto[]>([]);
  const [insights, setInsights] = useState<DealerInsightDto[]>([]);
  const [insightsSummary, setInsightsSummary] = useState<InsightsSummary | null>(null);
  const [performanceComparison, setPerformanceComparison] = useState<PerformanceComparison | null>(
    null
  );

  // Loading states
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingDashboard, setIsLoadingDashboard] = useState(false);
  const [isLoadingFunnel, setIsLoadingFunnel] = useState(false);
  const [isLoadingInsights, setIsLoadingInsights] = useState(false);
  const [isLoadingBenchmarks, setIsLoadingBenchmarks] = useState(false);

  // Error state
  const [error, setError] = useState<string | null>(null);

  // Fetch Dashboard Data
  const refreshDashboard = useCallback(async () => {
    if (!dealerId) return;

    setIsLoadingDashboard(true);
    setError(null);

    try {
      const [summary, stats] = await Promise.all([
        dealerAnalyticsService.getDashboardSummary(dealerId, fromDate, toDate),
        dealerAnalyticsService.getQuickStats(dealerId),
      ]);

      setDashboardSummary(summary);
      setQuickStats(stats);
    } catch (err) {
      console.error('Error fetching dashboard data:', err);
      setError('Error loading dashboard data');
    } finally {
      setIsLoadingDashboard(false);
    }
  }, [dealerId, fromDate, toDate]);

  // Fetch Conversion Funnel
  const refreshFunnel = useCallback(async () => {
    if (!dealerId) return;

    setIsLoadingFunnel(true);
    setError(null);

    try {
      const [funnel, visualization] = await Promise.all([
        dealerAnalyticsService.getConversionFunnel(dealerId, fromDate, toDate),
        dealerAnalyticsService.getFunnelVisualization(dealerId, fromDate, toDate),
      ]);

      setConversionFunnel(funnel);
      setFunnelVisualization(visualization);
    } catch (err) {
      console.error('Error fetching funnel data:', err);
      setError('Error loading conversion funnel');
    } finally {
      setIsLoadingFunnel(false);
    }
  }, [dealerId, fromDate, toDate]);

  // Fetch Insights
  const refreshInsights = useCallback(async () => {
    if (!dealerId) return;

    setIsLoadingInsights(true);
    setError(null);

    try {
      const [insightsList, summary] = await Promise.all([
        dealerAnalyticsService.getDealerInsights(dealerId),
        dealerAnalyticsService.getInsightsSummary(dealerId),
      ]);

      setInsights(insightsList);
      setInsightsSummary(summary);
    } catch (err) {
      console.error('Error fetching insights:', err);
      setError('Error loading insights');
    } finally {
      setIsLoadingInsights(false);
    }
  }, [dealerId]);

  // Fetch Benchmarks
  const refreshBenchmarks = useCallback(async () => {
    setIsLoadingBenchmarks(true);
    setError(null);

    try {
      const benchmarkData = await dealerAnalyticsService.getMarketBenchmarks();
      setBenchmarks(benchmarkData);
    } catch (err) {
      console.error('Error fetching benchmarks:', err);
      setError('Error loading market benchmarks');
    } finally {
      setIsLoadingBenchmarks(false);
    }
  }, []);

  // Fetch Performance Comparison
  const refreshPerformance = useCallback(async () => {
    if (!dealerId) return;

    try {
      const comparison = await dealerAnalyticsService.getPerformanceComparison(dealerId, 30);
      setPerformanceComparison(comparison);
    } catch (err) {
      console.error('Error fetching performance comparison:', err);
    }
  }, [dealerId]);

  // Refresh All Data
  const refreshData = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      await Promise.all([
        refreshDashboard(),
        refreshFunnel(),
        refreshInsights(),
        refreshBenchmarks(),
        refreshPerformance(),
      ]);
    } catch (err) {
      console.error('Error refreshing all data:', err);
      setError('Error refreshing data');
    } finally {
      setIsLoading(false);
    }
  }, [refreshDashboard, refreshFunnel, refreshInsights, refreshBenchmarks, refreshPerformance]);

  // Generate New Insights
  const generateNewInsights = useCallback(async () => {
    if (!dealerId) return;

    try {
      await dealerAnalyticsService.generateInsights(dealerId);
      await refreshInsights();
    } catch (err) {
      console.error('Error generating insights:', err);
      setError('Error generating insights');
    }
  }, [dealerId, refreshInsights]);

  // Mark Insights as Read
  const markInsightsAsRead = useCallback(
    async (insightIds: string[]) => {
      if (!dealerId || insightIds.length === 0) return;

      try {
        await dealerAnalyticsService.markInsightsAsRead(dealerId, insightIds);
        await refreshInsights();
      } catch (err) {
        console.error('Error marking insights as read:', err);
        setError('Error updating insights');
      }
    },
    [dealerId, refreshInsights]
  );

  // Mark Insight as Acted Upon
  const markInsightAsActedUpon = useCallback(
    async (insightId: string) => {
      try {
        await dealerAnalyticsService.markInsightAsActedUpon(insightId);
        await refreshInsights();
      } catch (err) {
        console.error('Error marking insight as acted upon:', err);
        setError('Error updating insight');
      }
    },
    [refreshInsights]
  );

  // Recalculate Analytics
  const recalculateAnalytics = useCallback(async () => {
    if (!dealerId || !fromDate || !toDate) return;

    try {
      await dealerAnalyticsService.recalculateAnalytics(dealerId, fromDate, toDate);
      await refreshData();
    } catch (err) {
      console.error('Error recalculating analytics:', err);
      setError('Error recalculating analytics');
    }
  }, [dealerId, fromDate, toDate, refreshData]);

  // Initial data load
  useEffect(() => {
    if (dealerId) {
      refreshData();
    }
  }, [dealerId, refreshData]);

  // Auto-refresh
  useEffect(() => {
    if (!autoRefresh || !dealerId) return;

    const interval = setInterval(() => {
      refreshData();
    }, refreshInterval);

    return () => clearInterval(interval);
  }, [autoRefresh, refreshInterval, dealerId, refreshData]);

  return {
    // Data
    dashboardSummary,
    quickStats,
    conversionFunnel,
    funnelVisualization,
    benchmarks,
    insights,
    insightsSummary,
    performanceComparison,

    // Loading states
    isLoading,
    isLoadingDashboard,
    isLoadingFunnel,
    isLoadingInsights,
    isLoadingBenchmarks,

    // Error
    error,

    // Actions
    refreshData,
    refreshDashboard,
    refreshFunnel,
    refreshInsights,
    refreshBenchmarks,
    generateNewInsights,
    markInsightsAsRead,
    markInsightAsActedUpon,
    recalculateAnalytics,
  };
};

export default useDealerAnalytics;
