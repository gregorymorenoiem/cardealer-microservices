import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import DealerPortalLayout from '../layouts/DealerPortalLayout';
import { useAuth } from '../hooks/useAuth';
import useDealerAnalytics from '../hooks/useDealerAnalytics';
import {
  FiTrendingUp,
  FiEye,
  FiPhone,
  FiDollarSign,
  FiUsers,
  FiBarChart2,
  FiPieChart,
  FiTarget,
  FiRefreshCw,
  FiCalendar,
  FiFilter,
  FiDownload,
  FiAlertTriangle,
  FiCheckCircle,
  FiClock,
  FiTrendingDown,
} from 'react-icons/fi';
import { dealerAnalyticsService } from '../services/dealerAnalyticsService';

const AdvancedDealerDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  // Get dealerId from authenticated user
  const dealerId = user?.dealerId || user?.id;

  // Fix: Move navigate to useEffect to avoid setState during render
  useEffect(() => {
    if (!dealerId) {
      navigate('/dealer/dashboard');
    }
  }, [dealerId, navigate]);

  // Date range state
  const [dateRange, setDateRange] = useState({
    fromDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000), // 30 días atrás
    toDate: new Date(),
  });

  const [activeTab, setActiveTab] = useState<'overview' | 'funnel' | 'insights' | 'benchmark'>(
    'overview'
  );
  const [refreshing, setRefreshing] = useState(false);

  // Use custom hook only if dealerId exists
  const {
    dashboardSummary,
    quickStats,
    funnelVisualization,
    insights,
    insightsSummary,
    benchmarks,
    performanceComparison,
    isLoading,
    isLoadingDashboard,
    error,
    refreshData,
    generateNewInsights,
    markInsightsAsRead,
    markInsightAsActedUpon,
  } = useDealerAnalytics({
    dealerId: dealerId || '',
    fromDate: dateRange.fromDate,
    toDate: dateRange.toDate,
    autoRefresh: true,
    refreshInterval: 300000, // 5 minutos
  });

  const handleRefresh = async () => {
    setRefreshing(true);
    await refreshData();
    setRefreshing(false);
  };

  const handleDateRangeChange = (from: Date, to: Date) => {
    setDateRange({ fromDate: from, toDate: to });
  };

  const handleGenerateInsights = async () => {
    try {
      await generateNewInsights();
    } catch (error) {
      console.error('Error generating insights:', error);
    }
  };

  const handleMarkInsightRead = async (insightId: string) => {
    try {
      await markInsightsAsRead([insightId]);
    } catch (error) {
      console.error('Error marking insight as read:', error);
    }
  };

  const handleMarkInsightActed = async (insightId: string) => {
    try {
      await markInsightAsActedUpon(insightId);
    } catch (error) {
      console.error('Error marking insight as acted upon:', error);
    }
  };

  if (isLoading && !dashboardSummary) {
    return (
      <DealerPortalLayout>
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
        </div>
      </DealerPortalLayout>
    );
  }

  return (
    <DealerPortalLayout>
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between">
              <div>
                <h1 className="text-3xl font-bold text-gray-900">Dashboard Avanzado</h1>
                <p className="mt-1 text-lg text-gray-600">Analytics completos para tu negocio</p>
              </div>

              <div className="mt-4 lg:mt-0 flex flex-col sm:flex-row gap-3">
                {/* Date Range Picker */}
                <div className="flex items-center space-x-2">
                  <FiCalendar className="text-gray-400" />
                  <select
                    value="30"
                    onChange={(e) => {
                      const days = parseInt(e.target.value);
                      const to = new Date();
                      const from = new Date(Date.now() - days * 24 * 60 * 60 * 1000);
                      handleDateRangeChange(from, to);
                    }}
                    className="rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  >
                    <option value="7">Últimos 7 días</option>
                    <option value="30">Últimos 30 días</option>
                    <option value="90">Últimos 90 días</option>
                    <option value="365">Último año</option>
                  </select>
                </div>

                {/* Refresh Button */}
                <button
                  onClick={handleRefresh}
                  disabled={refreshing}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
                >
                  <FiRefreshCw className={`mr-2 h-4 w-4 ${refreshing ? 'animate-spin' : ''}`} />
                  {refreshing ? 'Actualizando...' : 'Actualizar'}
                </button>

                {/* Export Button */}
                <button className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
                  <FiDownload className="mr-2 h-4 w-4" />
                  Exportar
                </button>
              </div>
            </div>
          </div>

          {/* Error State */}
          {error && (
            <div className="mb-6 bg-red-50 border border-red-200 rounded-md p-4">
              <div className="flex">
                <FiAlertTriangle className="h-5 w-5 text-red-400" />
                <div className="ml-3">
                  <h3 className="text-sm font-medium text-red-800">Error cargando datos</h3>
                  <div className="mt-2 text-sm text-red-700">
                    <p>{error}</p>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Quick Stats Cards */}
          {quickStats && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
              <StatCard
                title="Vistas Totales"
                value={dealerAnalyticsService.formatNumber(quickStats.totalViews)}
                growth={quickStats.viewsGrowth}
                icon={<FiEye className="h-6 w-6" />}
                color="blue"
              />
              <StatCard
                title="Contactos"
                value={dealerAnalyticsService.formatNumber(quickStats.totalContacts)}
                growth={quickStats.contactsGrowth}
                icon={<FiPhone className="h-6 w-6" />}
                color="green"
              />
              <StatCard
                title="Ventas"
                value={dealerAnalyticsService.formatNumber(quickStats.actualSales)}
                growth={quickStats.salesGrowth}
                icon={<FiTarget className="h-6 w-6" />}
                color="purple"
              />
              <StatCard
                title="Ingresos"
                value={dealerAnalyticsService.formatCurrency(quickStats.totalRevenue)}
                growth={quickStats.revenueGrowth}
                icon={<FiDollarSign className="h-6 w-6" />}
                color="orange"
              />
            </div>
          )}

          {/* Tabs */}
          <div className="mb-8">
            <div className="border-b border-gray-200">
              <nav className="-mb-px flex space-x-8">
                <button
                  onClick={() => setActiveTab('overview')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'overview'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <FiBarChart2 className="inline-block mr-2 h-4 w-4" />
                  Visión General
                </button>
                <button
                  onClick={() => setActiveTab('funnel')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'funnel'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <FiPieChart className="inline-block mr-2 h-4 w-4" />
                  Funnel de Conversión
                </button>
                <button
                  onClick={() => setActiveTab('insights')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'insights'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <FiTrendingUp className="inline-block mr-2 h-4 w-4" />
                  Insights
                  {insightsSummary && insightsSummary.unread > 0 && (
                    <span className="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                      {insightsSummary.unread}
                    </span>
                  )}
                </button>
                <button
                  onClick={() => setActiveTab('benchmark')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'benchmark'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <FiUsers className="inline-block mr-2 h-4 w-4" />
                  Benchmark
                </button>
              </nav>
            </div>
          </div>

          {/* Tab Content */}
          <div className="mt-8">
            {activeTab === 'overview' && (
              <OverviewTab
                dashboardSummary={dashboardSummary}
                performanceComparison={performanceComparison}
                isLoading={isLoadingDashboard}
                dealerId={dealerId || ''}
              />
            )}

            {activeTab === 'funnel' && (
              <FunnelTab funnelVisualization={funnelVisualization} isLoading={isLoadingDashboard} />
            )}

            {activeTab === 'insights' && (
              <InsightsTab
                insights={insights}
                insightsSummary={insightsSummary}
                onGenerateInsights={handleGenerateInsights}
                onMarkAsRead={handleMarkInsightRead}
                onMarkAsActed={handleMarkInsightActed}
                isLoading={isLoadingDashboard}
              />
            )}

            {activeTab === 'benchmark' && (
              <BenchmarkTab
                benchmarks={benchmarks}
                dealerId={dealerId}
                isLoading={isLoadingDashboard}
              />
            )}
          </div>
        </div>
      </div>
    </DealerPortalLayout>
  );
};

// Stat Card Component
interface StatCardProps {
  title: string;
  value: string;
  growth: number;
  icon: React.ReactNode;
  color: 'blue' | 'green' | 'purple' | 'orange';
}

const StatCard: React.FC<StatCardProps> = ({ title, value, growth, icon, color }) => {
  const colorClasses = {
    blue: 'bg-blue-50 text-blue-700',
    green: 'bg-green-50 text-green-700',
    purple: 'bg-purple-50 text-purple-700',
    orange: 'bg-orange-50 text-orange-700',
  };

  return (
    <div className="bg-white overflow-hidden shadow rounded-lg">
      <div className="p-5">
        <div className="flex items-center">
          <div className={`flex-shrink-0 ${colorClasses[color]} p-3 rounded-md`}>{icon}</div>
          <div className="ml-5 w-0 flex-1">
            <dl>
              <dt className="text-sm font-medium text-gray-500 truncate">{title}</dt>
              <dd className="flex items-baseline">
                <div className="text-2xl font-semibold text-gray-900">{value}</div>
                {growth !== 0 && (
                  <div
                    className={`ml-2 flex items-baseline text-sm font-semibold ${
                      growth > 0 ? 'text-green-600' : 'text-red-600'
                    }`}
                  >
                    {growth > 0 ? (
                      <FiTrendingUp className="self-center flex-shrink-0 h-5 w-5" />
                    ) : (
                      <FiTrendingDown className="self-center flex-shrink-0 h-5 w-5" />
                    )}
                    <span className="sr-only">{growth > 0 ? 'Increased' : 'Decreased'} by</span>
                    {dealerAnalyticsService.formatPercentage(growth)}
                  </div>
                )}
              </dd>
            </dl>
          </div>
        </div>
      </div>
    </div>
  );
};

// Tab Components
const OverviewTab: React.FC<{
  dashboardSummary: any;
  performanceComparison: any;
  isLoading: boolean;
  dealerId: string;
}> = ({ dashboardSummary, performanceComparison, isLoading, dealerId }) => {
  const [trendsData, setTrendsData] = React.useState<any>(null);
  const [loadingTrends, setLoadingTrends] = React.useState(true);

  React.useEffect(() => {
    if (dealerId) {
      setLoadingTrends(true);
      dealerAnalyticsService
        .getTrends(dealerId, 30)
        .then((data) => {
          setTrendsData(data);
          setLoadingTrends(false);
        })
        .catch((err) => {
          console.error('Error loading trends:', err);
          setLoadingTrends(false);
        });
    }
  }, [dealerId]);

  if (isLoading) {
    return <div className="animate-pulse h-64 bg-gray-200 rounded"></div>;
  }

  // Calculate chart dimensions
  const chartWidth = 800;
  const chartHeight = 240;
  const paddingLeft = 60;
  const paddingRight = 20;
  const paddingTop = 20;
  const paddingBottom = 40;
  const graphWidth = chartWidth - paddingLeft - paddingRight;
  const graphHeight = chartHeight - paddingTop - paddingBottom;

  // Process trends data for the chart
  const processedData = trendsData?.dailyData || [];
  const maxViews = Math.max(...processedData.map((d: any) => d.views || 0), 1);
  const maxContacts = Math.max(...processedData.map((d: any) => d.contacts || 0), 1);

  // Generate SVG path for views
  const getViewsPath = () => {
    if (processedData.length === 0) return '';
    const points = processedData.map((d: any, i: number) => {
      const x = paddingLeft + (i / Math.max(processedData.length - 1, 1)) * graphWidth;
      const y = paddingTop + graphHeight - (d.views / maxViews) * graphHeight;
      return `${i === 0 ? 'M' : 'L'} ${x} ${y}`;
    });
    return points.join(' ');
  };

  // Generate SVG path for contacts
  const getContactsPath = () => {
    if (processedData.length === 0) return '';
    const points = processedData.map((d: any, i: number) => {
      const x = paddingLeft + (i / Math.max(processedData.length - 1, 1)) * graphWidth;
      const y = paddingTop + graphHeight - (d.contacts / maxContacts) * graphHeight;
      return `${i === 0 ? 'M' : 'L'} ${x} ${y}`;
    });
    return points.join(' ');
  };

  // Generate Y-axis labels for views
  const yAxisLabels = [0, Math.round(maxViews / 2), maxViews];

  // Get date labels (first, middle, last)
  const getDateLabels = () => {
    if (processedData.length === 0) return [];
    const first = processedData[0];
    const middle = processedData[Math.floor(processedData.length / 2)];
    const last = processedData[processedData.length - 1];
    return [
      {
        x: paddingLeft,
        label: new Date(first.date).toLocaleDateString('es-DO', { month: 'short', day: 'numeric' }),
      },
      {
        x: paddingLeft + graphWidth / 2,
        label: new Date(middle.date).toLocaleDateString('es-DO', {
          month: 'short',
          day: 'numeric',
        }),
      },
      {
        x: paddingLeft + graphWidth,
        label: new Date(last.date).toLocaleDateString('es-DO', { month: 'short', day: 'numeric' }),
      },
    ];
  };

  return (
    <div className="space-y-6">
      {/* Performance Chart */}
      <div className="bg-white p-6 rounded-lg shadow">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-semibold">Tendencia de Performance (Últimos 30 días)</h3>
          <div className="flex items-center gap-4 text-sm">
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-blue-500"></div>
              <span className="text-gray-600">Vistas</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-green-500"></div>
              <span className="text-gray-600">Contactos</span>
            </div>
          </div>
        </div>

        {loadingTrends ? (
          <div className="h-64 bg-gray-100 rounded flex items-center justify-center animate-pulse">
            <p className="text-gray-500">Cargando gráfico...</p>
          </div>
        ) : processedData.length === 0 ? (
          <div className="h-64 bg-gray-100 rounded flex items-center justify-center">
            <p className="text-gray-500">No hay datos de tendencias disponibles</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <svg width={chartWidth} height={chartHeight} className="min-w-full">
              {/* Background grid */}
              <g className="grid-lines">
                {yAxisLabels.map((val, i) => {
                  const y = paddingTop + graphHeight - (val / maxViews) * graphHeight;
                  return (
                    <g key={i}>
                      <line
                        x1={paddingLeft}
                        y1={y}
                        x2={paddingLeft + graphWidth}
                        y2={y}
                        stroke="#e5e7eb"
                        strokeDasharray="4,4"
                      />
                      <text
                        x={paddingLeft - 10}
                        y={y + 4}
                        textAnchor="end"
                        className="text-xs fill-gray-500"
                      >
                        {val}
                      </text>
                    </g>
                  );
                })}
              </g>

              {/* Views line */}
              <path
                d={getViewsPath()}
                fill="none"
                stroke="#3b82f6"
                strokeWidth={3}
                className="drop-shadow-sm"
              />

              {/* Contacts line */}
              <path
                d={getContactsPath()}
                fill="none"
                stroke="#22c55e"
                strokeWidth={3}
                className="drop-shadow-sm"
              />

              {/* Data points - Views */}
              {processedData.map((d: any, i: number) => {
                const x = paddingLeft + (i / Math.max(processedData.length - 1, 1)) * graphWidth;
                const y = paddingTop + graphHeight - (d.views / maxViews) * graphHeight;
                return (
                  <circle
                    key={`views-${i}`}
                    cx={x}
                    cy={y}
                    r={4}
                    fill="#3b82f6"
                    className="hover:r-6 cursor-pointer"
                  >
                    <title>{`${new Date(d.date).toLocaleDateString('es-DO')}: ${d.views} vistas`}</title>
                  </circle>
                );
              })}

              {/* Data points - Contacts */}
              {processedData.map((d: any, i: number) => {
                const x = paddingLeft + (i / Math.max(processedData.length - 1, 1)) * graphWidth;
                const y = paddingTop + graphHeight - (d.contacts / maxContacts) * graphHeight;
                return (
                  <circle
                    key={`contacts-${i}`}
                    cx={x}
                    cy={y}
                    r={4}
                    fill="#22c55e"
                    className="hover:r-6 cursor-pointer"
                  >
                    <title>{`${new Date(d.date).toLocaleDateString('es-DO')}: ${d.contacts} contactos`}</title>
                  </circle>
                );
              })}

              {/* X-axis labels */}
              {getDateLabels().map((label, i) => (
                <text
                  key={i}
                  x={label.x}
                  y={chartHeight - 10}
                  textAnchor="middle"
                  className="text-xs fill-gray-500"
                >
                  {label.label}
                </text>
              ))}

              {/* Axes */}
              <line
                x1={paddingLeft}
                y1={paddingTop}
                x2={paddingLeft}
                y2={paddingTop + graphHeight}
                stroke="#d1d5db"
                strokeWidth={1}
              />
              <line
                x1={paddingLeft}
                y1={paddingTop + graphHeight}
                x2={paddingLeft + graphWidth}
                y2={paddingTop + graphHeight}
                stroke="#d1d5db"
                strokeWidth={1}
              />
            </svg>
          </div>
        )}

        {/* Summary stats below chart */}
        {processedData.length > 0 && (
          <div className="mt-4 grid grid-cols-2 md:grid-cols-4 gap-4 pt-4 border-t">
            <div className="text-center">
              <p className="text-2xl font-bold text-blue-600">
                {processedData.reduce((sum: number, d: any) => sum + d.views, 0).toLocaleString()}
              </p>
              <p className="text-sm text-gray-500">Total Vistas</p>
            </div>
            <div className="text-center">
              <p className="text-2xl font-bold text-green-600">
                {processedData
                  .reduce((sum: number, d: any) => sum + d.contacts, 0)
                  .toLocaleString()}
              </p>
              <p className="text-sm text-gray-500">Total Contactos</p>
            </div>
            <div className="text-center">
              <p className="text-2xl font-bold text-purple-600">
                {processedData.reduce((sum: number, d: any) => sum + d.sales, 0)}
              </p>
              <p className="text-sm text-gray-500">Ventas</p>
            </div>
            <div className="text-center">
              <p className="text-2xl font-bold text-amber-600">
                {dealerAnalyticsService.formatCurrency(
                  processedData.reduce((sum: number, d: any) => sum + d.revenue, 0)
                )}
              </p>
              <p className="text-sm text-gray-500">Ingresos</p>
            </div>
          </div>
        )}
      </div>

      {/* Analytics Summary */}
      {dashboardSummary && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="bg-white p-6 rounded-lg shadow">
            <h3 className="text-lg font-semibold mb-4">Métricas de Tráfico</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Vistas Totales:</span>
                <span className="font-semibold">
                  {dealerAnalyticsService.formatNumber(dashboardSummary.analytics.totalViews)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Vistas Únicas:</span>
                <span className="font-semibold">
                  {dealerAnalyticsService.formatNumber(dashboardSummary.analytics.uniqueViews)}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Tiempo Promedio:</span>
                <span className="font-semibold">
                  {dashboardSummary.analytics.averageViewDuration.toFixed(1)}s
                </span>
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg shadow">
            <h3 className="text-lg font-semibold mb-4">Métricas de Ventas</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Conversión:</span>
                <span className="font-semibold">
                  {dashboardSummary.analytics.conversionRate.toFixed(2)}%
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Precio Promedio:</span>
                <span className="font-semibold">
                  {dealerAnalyticsService.formatCurrency(
                    dashboardSummary.analytics.averageVehiclePrice
                  )}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Días en Mercado:</span>
                <span className="font-semibold">
                  {dashboardSummary.analytics.averageDaysOnMarket.toFixed(0)} días
                </span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const FunnelTab: React.FC<{
  funnelVisualization: any;
  isLoading: boolean;
}> = ({ funnelVisualization, isLoading }) => {
  if (isLoading) {
    return <div className="animate-pulse h-64 bg-gray-200 rounded"></div>;
  }

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-6">Funnel de Conversión</h3>

        {funnelVisualization ? (
          <div className="space-y-4">
            {funnelVisualization.steps.map((step: any, index: number) => (
              <div key={step.name} className="flex items-center space-x-4">
                <div className="flex-shrink-0 w-24 text-right">
                  <span className="text-sm font-medium text-gray-600">{step.name}</span>
                </div>
                <div className="flex-1">
                  <div className="bg-gray-200 rounded-full h-6">
                    <div
                      className="h-6 rounded-full flex items-center px-3"
                      style={{
                        backgroundColor: step.color,
                        width: `${Math.max(step.percentage, 5)}%`,
                      }}
                    >
                      <span className="text-white text-sm font-medium">
                        {step.value.toLocaleString()}
                      </span>
                    </div>
                  </div>
                </div>
                <div className="flex-shrink-0 w-16 text-right">
                  <span className="text-sm font-medium text-gray-900">
                    {step.percentage.toFixed(1)}%
                  </span>
                </div>
              </div>
            ))}

            <div className="mt-6 pt-6 border-t border-gray-200">
              <div className="grid grid-cols-2 gap-4">
                <div className="text-center">
                  <div className="text-2xl font-bold text-blue-600">
                    {funnelVisualization.overall.conversionRate.toFixed(2)}%
                  </div>
                  <div className="text-sm text-gray-600">Conversión General</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold text-green-600">
                    {funnelVisualization.overall.averageTimeToSale.toFixed(0)}
                  </div>
                  <div className="text-sm text-gray-600">Días Promedio a Venta</div>
                </div>
              </div>
            </div>
          </div>
        ) : (
          <div className="text-center text-gray-500 py-8">No hay datos de funnel disponibles</div>
        )}
      </div>
    </div>
  );
};

const InsightsTab: React.FC<{
  insights: any[];
  insightsSummary: any;
  onGenerateInsights: () => void;
  onMarkAsRead: (id: string) => void;
  onMarkAsActed: (id: string) => void;
  isLoading: boolean;
}> = ({
  insights,
  insightsSummary,
  onGenerateInsights,
  onMarkAsRead,
  onMarkAsActed,
  isLoading,
}) => {
  if (isLoading) {
    return <div className="animate-pulse h-64 bg-gray-200 rounded"></div>;
  }

  return (
    <div className="space-y-6">
      {/* Generate Insights Button */}
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold">Insights y Recomendaciones</h3>
        <button
          onClick={onGenerateInsights}
          className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md shadow-sm hover:bg-blue-700"
        >
          <FiRefreshCw className="mr-2 h-4 w-4" />
          Generar Nuevos Insights
        </button>
      </div>

      {/* Insights Summary */}
      {insightsSummary && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-white p-4 rounded-lg shadow text-center">
            <div className="text-2xl font-bold text-blue-600">{insightsSummary.total}</div>
            <div className="text-sm text-gray-600">Total</div>
          </div>
          <div className="bg-white p-4 rounded-lg shadow text-center">
            <div className="text-2xl font-bold text-orange-600">{insightsSummary.unread}</div>
            <div className="text-sm text-gray-600">Sin Leer</div>
          </div>
          <div className="bg-white p-4 rounded-lg shadow text-center">
            <div className="text-2xl font-bold text-red-600">
              {insightsSummary.byPriority.critical}
            </div>
            <div className="text-sm text-gray-600">Críticos</div>
          </div>
          <div className="bg-white p-4 rounded-lg shadow text-center">
            <div className="text-2xl font-bold text-green-600">{insightsSummary.actedUpon}</div>
            <div className="text-sm text-gray-600">Implementados</div>
          </div>
        </div>
      )}

      {/* Insights List */}
      <div className="space-y-4">
        {insights && insights.length > 0 ? (
          insights.map((insight) => (
            <InsightCard
              key={insight.id}
              insight={insight}
              onMarkAsRead={onMarkAsRead}
              onMarkAsActed={onMarkAsActed}
            />
          ))
        ) : (
          <div className="bg-white p-6 rounded-lg shadow text-center">
            <p className="text-gray-500">No hay insights disponibles</p>
          </div>
        )}
      </div>
    </div>
  );
};

const BenchmarkTab: React.FC<{
  benchmarks: any[];
  dealerId: string;
  isLoading: boolean;
}> = ({ benchmarks, dealerId, isLoading }) => {
  if (isLoading) {
    return <div className="animate-pulse h-64 bg-gray-200 rounded"></div>;
  }

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4">Comparación con el Mercado</h3>

        {benchmarks && benchmarks.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Categoría
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Rango Precio
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Precio Promedio
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Días en Mercado
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Conversión
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {benchmarks.map((benchmark) => (
                  <tr key={benchmark.id}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {benchmark.vehicleCategory}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {benchmark.priceRange}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {dealerAnalyticsService.formatCurrency(benchmark.marketAveragePrice)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {benchmark.marketAverageDaysOnMarket.toFixed(0)} días
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {benchmark.marketConversionRate.toFixed(1)}%
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center text-gray-500 py-8">
            No hay datos de benchmark disponibles
          </div>
        )}
      </div>
    </div>
  );
};

// Insight Card Component
const InsightCard: React.FC<{
  insight: any;
  onMarkAsRead: (id: string) => void;
  onMarkAsActed: (id: string) => void;
}> = ({ insight, onMarkAsRead, onMarkAsActed }) => {
  const priorityColors = {
    Critical: 'border-red-500 bg-red-50',
    High: 'border-orange-500 bg-orange-50',
    Medium: 'border-yellow-500 bg-yellow-50',
    Low: 'border-green-500 bg-green-50',
  };

  return (
    <div
      className={`border-l-4 bg-white p-6 rounded-lg shadow ${priorityColors[insight.priority as keyof typeof priorityColors] || 'border-gray-500 bg-gray-50'}`}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center mb-2">
            <h4 className="text-lg font-semibold text-gray-900">{insight.title}</h4>
            <span
              className={`ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${dealerAnalyticsService.getPriorityColor(
                insight.priority
              )}`}
            >
              {insight.priority}
            </span>
            {!insight.isRead && (
              <span className="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                Nuevo
              </span>
            )}
          </div>

          <p className="text-gray-700 mb-3">{insight.description}</p>

          <div className="bg-blue-50 p-3 rounded-md mb-4">
            <p className="text-sm text-blue-800">
              <strong>Recomendación:</strong> {insight.actionRecommendation}
            </p>
          </div>

          <div className="flex items-center space-x-4 text-sm text-gray-600">
            <span>Impacto: {insight.potentialImpact.toFixed(0)}%</span>
            <span>Confianza: {insight.confidence.toFixed(0)}%</span>
            <span>Creado: {new Date(insight.createdAt).toLocaleDateString()}</span>
          </div>
        </div>

        <div className="flex flex-col space-y-2 ml-4">
          {!insight.isRead && (
            <button
              onClick={() => onMarkAsRead(insight.id)}
              className="inline-flex items-center px-3 py-1 text-xs font-medium text-blue-700 bg-blue-100 rounded-full hover:bg-blue-200"
            >
              <FiCheckCircle className="mr-1 h-3 w-3" />
              Marcar Leído
            </button>
          )}

          {!insight.isActedUpon && (
            <button
              onClick={() => onMarkAsActed(insight.id)}
              className="inline-flex items-center px-3 py-1 text-xs font-medium text-green-700 bg-green-100 rounded-full hover:bg-green-200"
            >
              <FiClock className="mr-1 h-3 w-3" />
              Implementado
            </button>
          )}

          {insight.isActedUpon && (
            <span className="inline-flex items-center px-3 py-1 text-xs font-medium text-green-700 bg-green-100 rounded-full">
              <FiCheckCircle className="mr-1 h-3 w-3" />
              Completado
            </span>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdvancedDealerDashboard;
