import { useEffect, useState } from 'react';
import { Line, Pie, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler,
} from 'chart.js';
import {
  dealerAnalyticsService,
  AnalyticsDashboard,
  ContactType,
} from '../services/dealerAnalyticsService';
import { FiTrendingUp, FiEye, FiPhone, FiActivity, FiClock } from 'react-icons/fi';
import MainLayout from '../layouts/MainLayout';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler
);

interface DealerAnalyticsDashboardProps {
  dealerId: string;
}

export const DealerAnalyticsDashboard = ({ dealerId }: DealerAnalyticsDashboardProps) => {
  const [analytics, setAnalytics] = useState<AnalyticsDashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [dateRange, setDateRange] = useState('30'); // 7, 30, 90 days
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadAnalytics();
  }, [dealerId, dateRange]);

  const loadAnalytics = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const { startDate, endDate } = dealerAnalyticsService.getDateRange(parseInt(dateRange));
      const data = await dealerAnalyticsService.getDashboard(dealerId, startDate, endDate);
      setAnalytics(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al cargar analytics');
      console.error('Error loading analytics:', err);
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  if (error) {
    return (
      <MainLayout>
        <div className="p-8 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-red-600 font-medium">{error}</p>
          <button
            onClick={loadAnalytics}
            className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            Reintentar
          </button>
        </div>
      </MainLayout>
    );
  }

  if (!analytics)
    return (
      <MainLayout>
        <div className="p-8">No hay datos disponibles</div>
      </MainLayout>
    );

  const { summary, viewsTrend, contactMethodBreakdown, deviceBreakdown, topReferrers, liveStats } =
    analytics;

  // ============================================
  // CHART DATA CONFIGURATION
  // ============================================

  const viewsTrendData = {
    labels: viewsTrend.map((point) =>
      new Date(point.date).toLocaleDateString('es-DO', { month: 'short', day: 'numeric' })
    ),
    datasets: [
      {
        label: 'Vistas',
        data: viewsTrend.map((point) => point.views),
        borderColor: 'rgb(59, 130, 246)',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        fill: true,
        tension: 0.4,
      },
      {
        label: 'Contactos',
        data: viewsTrend.map((point) => point.contacts),
        borderColor: 'rgb(16, 185, 129)',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: true,
        tension: 0.4,
      },
    ],
  };

  const contactMethodData = {
    labels: contactMethodBreakdown.map((method) => method.label),
    datasets: [
      {
        data: contactMethodBreakdown.map((method) => method.count),
        backgroundColor: [
          'rgba(59, 130, 246, 0.8)', // Phone - blue
          'rgba(147, 51, 234, 0.8)', // Email - purple
          'rgba(16, 185, 129, 0.8)', // WhatsApp - green
          'rgba(249, 115, 22, 0.8)', // Website - orange
          'rgba(236, 72, 153, 0.8)', // Social - pink
        ],
        borderColor: [
          'rgb(59, 130, 246)',
          'rgb(147, 51, 234)',
          'rgb(16, 185, 129)',
          'rgb(249, 115, 22)',
          'rgb(236, 72, 153)',
        ],
        borderWidth: 2,
      },
    ],
  };

  const deviceBreakdownData = {
    labels: deviceBreakdown.map((device) => device.deviceType),
    datasets: [
      {
        data: deviceBreakdown.map((device) => device.count),
        backgroundColor: [
          'rgba(59, 130, 246, 0.8)', // Mobile
          'rgba(99, 102, 241, 0.8)', // Desktop
          'rgba(139, 92, 246, 0.8)', // Tablet
        ],
        borderColor: ['rgb(59, 130, 246)', 'rgb(99, 102, 241)', 'rgb(139, 92, 246)'],
        borderWidth: 2,
      },
    ],
  };

  return (
    <MainLayout>
      <div className="space-y-6">
        {/* Header con Date Range Picker */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">📊 Analytics & Métricas</h1>
            <p className="text-gray-600 mt-1">Panel completo de análisis de rendimiento</p>
          </div>
          <div className="flex items-center space-x-2">
            <label className="text-sm font-medium text-gray-700">Período:</label>
            <select
              value={dateRange}
              onChange={(e) => setDateRange(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="7">Últimos 7 días</option>
              <option value="30">Últimos 30 días</option>
              <option value="90">Últimos 90 días</option>
            </select>
          </div>
        </div>

        {/* Live Stats Bar */}
        {liveStats && (
          <div className="bg-gradient-to-r from-blue-500 to-purple-600 rounded-lg p-6 text-white">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              <div>
                <p className="text-blue-100 text-sm font-medium">Visitantes Activos</p>
                <div className="flex items-center space-x-2 mt-1">
                  <span className="text-3xl font-bold">{liveStats.currentViewers}</span>
                  <span className="flex items-center space-x-1 animate-pulse">
                    <span className="w-2 h-2 bg-green-400 rounded-full"></span>
                    <span className="text-sm">En vivo</span>
                  </span>
                </div>
              </div>
              <div>
                <p className="text-blue-100 text-sm font-medium">Vistas Hoy</p>
                <p className="text-3xl font-bold mt-1">
                  {dealerAnalyticsService.formatNumber(liveStats.viewsToday)}
                </p>
              </div>
              <div>
                <p className="text-blue-100 text-sm font-medium">Contactos Hoy</p>
                <p className="text-3xl font-bold mt-1">
                  {dealerAnalyticsService.formatNumber(liveStats.contactsToday)}
                </p>
              </div>
              {liveStats.mostRecentView && (
                <div>
                  <p className="text-blue-100 text-sm font-medium">Última Visita</p>
                  <p className="text-lg font-semibold mt-1">
                    {dealerAnalyticsService.getDeviceIcon(liveStats.mostRecentView.deviceType)}{' '}
                    {liveStats.mostRecentView.city || 'Desconocido'}
                  </p>
                  <p className="text-sm text-blue-100">
                    {new Date(liveStats.mostRecentView.viewedAt).toLocaleTimeString('es-DO', {
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </p>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Summary Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <StatCard
            icon={<FiEye className="w-6 h-6 text-blue-600" />}
            title="Total Vistas"
            value={dealerAnalyticsService.formatNumber(summary.totalViews)}
            subtitle={`${dealerAnalyticsService.formatNumber(summary.uniqueVisitors)} únicos`}
            trend={summary.comparedToLastPeriod}
            color="blue"
          />
          <StatCard
            icon={<FiPhone className="w-6 h-6 text-green-600" />}
            title="Contactos"
            value={dealerAnalyticsService.formatNumber(summary.totalContacts)}
            subtitle={`${dealerAnalyticsService.formatPercentage(summary.contactConversionRate)} conversión`}
            trend={summary.comparedToLastPeriod}
            color="green"
          />
          <StatCard
            icon={<FiActivity className="w-6 h-6 text-purple-600" />}
            title="Engagement"
            value={dealerAnalyticsService.formatPercentage(summary.engagementRate)}
            subtitle={`${dealerAnalyticsService.formatPercentage(summary.bounceRate)} bounce rate`}
            color="purple"
          />
          <StatCard
            icon={<FiClock className="w-6 h-6 text-orange-600" />}
            title="Tiempo Promedio"
            value={dealerAnalyticsService.formatDuration(summary.averageViewDuration)}
            subtitle="Por visita"
            color="orange"
          />
        </div>

        {/* Charts Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Views Trend Chart */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Tendencia de Vistas y Contactos
            </h3>
            <Line
              data={viewsTrendData}
              options={{
                responsive: true,
                maintainAspectRatio: true,
                aspectRatio: 2,
                plugins: {
                  legend: {
                    position: 'bottom',
                  },
                },
                scales: {
                  y: {
                    beginAtZero: true,
                  },
                },
              }}
            />
          </div>

          {/* Contact Method Breakdown */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Métodos de Contacto</h3>
            <div className="h-64">
              <Pie
                data={contactMethodData}
                options={{
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                    legend: {
                      position: 'right',
                    },
                  },
                }}
              />
            </div>
          </div>

          {/* Device Breakdown */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Dispositivos</h3>
            <div className="h-64">
              <Doughnut
                data={deviceBreakdownData}
                options={{
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                    legend: {
                      position: 'bottom',
                    },
                  },
                }}
              />
            </div>
          </div>

          {/* Top Referrers */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Fuentes de Tráfico</h3>
            <div className="space-y-3">
              {topReferrers.map((referrer, index) => (
                <div key={index} className="flex items-center justify-between">
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">{referrer.source}</p>
                    <div className="w-full bg-gray-200 rounded-full h-2 mt-1">
                      <div
                        className="bg-blue-600 h-2 rounded-full"
                        style={{ width: `${referrer.percentage}%` }}
                      ></div>
                    </div>
                  </div>
                  <p className="ml-4 text-sm font-semibold text-gray-900 w-16 text-right">
                    {dealerAnalyticsService.formatPercentage(referrer.percentage)}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Contact Methods Detailed Table */}
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <div className="p-6 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-900">Detalle de Métodos de Contacto</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Método
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Clicks
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    % Total
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Convertidos
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Tasa Conversión
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {contactMethodBreakdown.map((method, index) => (
                  <tr key={index}>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <span className="text-2xl mr-2">
                          {dealerAnalyticsService.getContactTypeIcon(method.type)}
                        </span>
                        <span className="text-sm font-medium text-gray-900">{method.label}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {dealerAnalyticsService.formatNumber(method.count)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {dealerAnalyticsService.formatPercentage(method.percentage)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {dealerAnalyticsService.formatNumber(method.convertedCount)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          method.conversionRate >= 50
                            ? 'bg-green-100 text-green-800'
                            : method.conversionRate >= 25
                              ? 'bg-yellow-100 text-yellow-800'
                              : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {dealerAnalyticsService.formatPercentage(method.conversionRate)}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>{' '}
      {/* Close space-y-6 div */}
    </MainLayout>
  );
};

// ============================================
// STAT CARD COMPONENT
// ============================================

interface StatCardProps {
  icon: React.ReactNode;
  title: string;
  value: string | number;
  subtitle?: string;
  trend?: {
    currentPeriod: number;
    previousPeriod: number;
    changePercentage: number;
    isIncrease: boolean;
  };
  color: 'blue' | 'green' | 'purple' | 'orange';
}

const StatCard = ({ icon, title, value, subtitle, trend, color }: StatCardProps) => {
  const colorClasses = {
    blue: 'bg-blue-50 border-blue-100',
    green: 'bg-green-50 border-green-100',
    purple: 'bg-purple-50 border-purple-100',
    orange: 'bg-orange-50 border-orange-100',
  };

  return (
    <div className={`${colorClasses[color]} border rounded-lg p-6`}>
      <div className="flex items-center justify-between">
        <div className="flex-1">
          <div className="flex items-center space-x-2">
            {icon}
            <p className="text-sm font-medium text-gray-600">{title}</p>
          </div>
          <p className="text-2xl font-bold text-gray-900 mt-2">{value}</p>
          {subtitle && <p className="text-sm text-gray-500 mt-1">{subtitle}</p>}
          {trend && (
            <div
              className={`flex items-center space-x-1 mt-2 text-sm ${
                trend.isIncrease ? 'text-green-600' : 'text-red-600'
              }`}
            >
              <span>{dealerAnalyticsService.getTrendIcon(trend.changePercentage)}</span>
              <span className="font-medium">{Math.abs(trend.changePercentage).toFixed(1)}%</span>
              <span className="text-gray-500">vs período anterior</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default DealerAnalyticsDashboard;
